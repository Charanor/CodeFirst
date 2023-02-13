﻿using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using JXS.AssetManagement.AssetResolvers;
using JXS.FileSystem;
using JXS.Utils;
using JXS.Utils.Logging;

namespace JXS.AssetManagement;

public static class Assets
{
	private const string GENERATED_ASSET_SCHEME = "generated";
	private const string DEFAULT_ASSET_SCHEME = "file";

	private static readonly ILogger Logger = LoggingManager.Get(nameof(Assets));

	private static readonly IDictionary<string, PathResolver> PathResolvers = new Dictionary<string, PathResolver>();

	private static readonly IList<IAssetResolver> AssetResolvers = new List<IAssetResolver>
	{
		new TextAssetResolver()
	};

	private static readonly ConcurrentDictionary<string, Task<object>> AssetTasks = new();

	/// <summary>
	///     Asynchronously loads the given asset to memory on a different thread and saves it to the cache for future use.
	/// </summary>
	/// <param name="asset"></param>
	/// <param name="assetType">
	///     The type of the asset. If left blank, or <c>null</c>, the type will be guessed by the asset
	///     loaders.
	/// </param>
	/// <returns></returns>
	public static async Task<bool> Precache([UriString] string asset, Type? assetType = null)
	{
		if (asset is not { Length: > 0 })
		{
			// This asset was probably due to the asset string not being initialized. This is logically an error, 
			// but due to how some libraries that use this asset module work (e.g. the ECS library), it is undesirable
			// to throw an error. So in this case we just warn the user that a default asset was supplied and return false.
			Logger.Debug($"{nameof(Precache)}({asset}): Default asset URI supplied.");
			return false;
		}

		// ReSharper disable once InvertIf
		if (!IsValidAssetUri(asset, out string? host, out var path))
		{
			Logger.Warn($"{nameof(Precache)}({asset}): Invalid asset uri; aborting preload.");
			return false;
		}

		if (host == default)
		{
			Logger.Warn($"{nameof(Precache)}({asset}): Asset is a generated asset; aborting preload.");
			return false;
		}

		var assetTask = AssetTasks.GetOrAdd(asset,
			valueFactory: key => Task.Run(() => LoadAsset(key, host, path, assetType)));
		await assetTask;
		return assetTask.IsCompletedSuccessfully;
	}

	public static Task<bool> Precache<TAssetType>([UriString] string asset) =>
		Precache(asset, typeof(TAssetType));

	private static object LoadAsset<TAssetType>([UriString] string asset, string host, string path) =>
		LoadAsset(asset, host, path, typeof(TAssetType));

	private static object LoadAsset([UriString] string asset, string host, string path, Type? assetType)
	{
		var pathResolver = PathResolvers[host];
		var fileHandle = pathResolver(path);

		if (!fileHandle.Exists)
		{
			throw new FileNotFoundException($"Asset {asset} does not exist", fileHandle.FilePath);
		}

		if (fileHandle.Type != FileType.File)
		{
			throw new FileNotFoundException($"Asset {asset} points to a directory; not a file!", fileHandle.FilePath);
		}

		object? result = null;
		foreach (var assetResolver in AssetResolvers)
		{
			if (assetType != null && !assetResolver.CanLoadAssetOfType(assetType))
			{
				continue;
			}

			if (assetResolver.TryLoadAsset(fileHandle, out result))
			{
				break;
			}
		}

		return result ?? throw new UnresolvedAssetException(asset);
	}

	/// <summary>
	///     Attempts to unload the given asset. If the asset does not exist or is invalid this does nothing.
	/// </summary>
	/// <param name="asset"></param>
	public static void Unload([UriString] string asset)
	{
		if (!AssetTasks.TryRemove(asset, out var assetTask))
		{
			return;
		}

		if (!assetTask.IsCompletedSuccessfully)
		{
			// If we don't run it as a task we will synchronously wait for the asset to load before disposing.
			// Not good to lock the current thread.
			Task.Run(DisposeTaskAsset);
		}
		else
		{
			DisposeTaskAsset();
		}

		void DisposeTaskAsset()
		{
			if (assetTask.Result is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
	}

	/// <summary>
	///     Checks the current state of the given asset.
	/// </summary>
	/// <param name="asset">the asset to check</param>
	/// <returns>the state of the asset</returns>
	public static AssetState GetAssetState([UriString] string asset)
	{
		if (!IsValidAssetUri(asset))
		{
			return AssetState.Invalid;
		}

		if (!AssetTasks.TryGetValue(asset, out var assetTask))
		{
			return AssetState.Unloaded;
		}

		return assetTask.IsCompletedSuccessfully ? AssetState.Loaded : AssetState.Loading;
	}

	/// <summary>
	///     Get an asset. Will get from cache if the asset is cached, otherwise will load it from the file system.
	/// </summary>
	/// <typeparam name="TAsset">the asset to load</typeparam>
	/// <returns>The asset</returns>
	/// <exception cref="UnloadedAssetException">If this asset could not be loaded</exception>
	/// <exception cref="UriFormatException">If the given asset path is an invalid uri (see <see cref="IsValidAssetUri" />)</exception>
	/// <exception cref="WrongAssetTypeException">If the given asset is the wrong type</exception>
	[return: System.Diagnostics.CodeAnalysis.NotNull]
	public static TAsset Get<TAsset>([UriString] string asset)
	{
		if (!IsValidAssetUri(asset))
		{
			throw new UriFormatException($"Invalid URI {asset}");
		}

		if (!AssetTasks.TryGetValue(asset, out var assetTask))
		{
			var precacheTask = Precache(asset);
			precacheTask.WaitWhileHandlingMainThreadTasks();
			if (!precacheTask.Result)
			{
				throw new UnloadedAssetException(asset);
			}

			assetTask = AssetTasks[asset];
		}

		if (assetTask.IsFaulted)
		{
			Logger.Warn($"{nameof(Get)}({asset}): Asset failed to load.");
			throw new UnloadedAssetException(asset);
		}

		while (!assetTask.IsCompleted)
		{
			assetTask.WaitWhileHandlingMainThreadTasks();
		}

		if (assetTask.Result is not TAsset typedAsset)
		{
			// ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
			throw new WrongAssetTypeException(asset, typeof(TAsset), assetTask.Result?.GetType());
		}

		return typedAsset;
	}

	/// <summary>
	///     Tries to get the asset
	/// </summary>
	/// <param name="asset"></param>
	/// <param name="result"></param>
	/// <typeparam name="TAsset"></typeparam>
	/// <returns></returns>
	public static bool TryGetAsset<TAsset>([UriString] string asset, [NotNullWhen(true)] out TAsset? result)
	{
		var state = GetAssetState(asset);
		if (state != AssetState.Loaded)
		{
			result = default;
			return false;
		}

		try
		{
			result = Get<TAsset>(asset);
			return true;
		}
		// We don't want to catch any other exceptions, since they should be "impossible".
		catch (WrongAssetTypeException)
		{
			result = default;
			return false;
		}
	}

	/// <summary>
	///     Checks if the given asset string is a correctly formatted asset string.
	/// </summary>
	/// <remarks>
	///     Please note that this does <b>not</b> check if the asset is loaded, or if the asset could be loaded, or
	///     anything of the sort. It <b>only</b> checks if the string is in a valid format and that there is a loader
	///     that is prepared to handle this asset type (but once again; does not guarantee that the load will succeed).
	/// </remarks>
	/// <param name="asset">the asset string</param>
	/// <returns><c>true</c> if <paramref name="asset" /> is a valid asset string, <c>false</c> otherwise</returns>
	public static bool IsValidAssetUri([UriString] string asset) =>
		IsValidAssetUri(asset, out ReadOnlySpan<char> _, out _);

	private static bool IsValidAssetUri([UriString] string asset, out string? host,
		[NotNullWhen(true)] out string? path)
	{
		if (!IsValidAssetUri(asset, out ReadOnlySpan<char> hostSpan, out var pathSpan))
		{
			host = default;
			path = default;
			return false;
		}

		host = hostSpan == default ? default : hostSpan.ToString();
		path = pathSpan.ToString();
		return true;
	}

	private static bool IsValidAssetUri([UriString] string asset, out ReadOnlySpan<char> host,
		out ReadOnlySpan<char> path)
	{
		if (!TryParseUri(asset, out host, out path))
		{
			Logger.Debug($"{nameof(IsValidAssetUri)}({asset}): Invalid uri format");
			return false;
		}

		// ReSharper disable once InvertIf
		if (host != default && !PathResolvers.ContainsKey(host.ToString()))
		{
			Logger.Debug(
				$"{nameof(IsValidAssetUri)}({asset}): Asset has unknown root path \"{host}\"; expected one of [{string.Join(separator: ", ", PathResolvers.Keys)}].");
			return false;
		}

		return true;
	}

	/// <summary>
	///     Generates a temporary asset handle for the given object. This allows assets generated via code to be accessed
	///     by the <see cref="Get{TAsset}" /> methods and similar. Note that this asset is treated <b>exactly</b> like
	///     any other asset, meaning it can be unloaded as well.
	/// </summary>
	/// <remarks>
	///     The asset does not have to be loaded via any given asset loader. In fact, any object can be passed to this
	///     function (even nonsensical ones like <see cref="int" />...). However if there are no assets loaders that can load
	///     assets of that type that means that the asset can not be reloaded if it goes out of scope! Edge case but might be
	///     important to note.
	/// </remarks>
	/// <param name="assetObject"></param>
	/// <typeparam name="TAsset"></typeparam>
	/// <returns>an <b>opaque</b> asset handle. It is not guaranteed to be of any particular structure, so do not rely on it.</returns>
	public static string GenerateAssetHandle<TAsset>(TAsset assetObject)
	{
		if (assetObject == null)
		{
			DevTools.ThrowStatic(new ArgumentNullException(nameof(assetObject)));
			return string.Empty;
		}

		var anyResolversCanLoadAsset =
			AssetResolvers.Any(assetResolver => assetResolver.CanLoadAssetOfType(typeof(TAsset)));

		if (!anyResolversCanLoadAsset)
		{
			// Not necessarily an error but might be good to warn about this
			Logger.Warn(
				$"Generated asset for asset of type {typeof(TAsset)}, however there are no asset resolvers that can load assets of that type. This is not guaranteed to work, since the asset can not be reloaded if it would be taken out of scope.");
		}

		var uuid = $"{GENERATED_ASSET_SCHEME}:{Path.GetRandomFileName()}";
		var task = Task.FromResult((object)assetObject);
		while (!AssetTasks.TryAdd(uuid, task))
		{
			uuid = $"{GENERATED_ASSET_SCHEME}:{Path.GetRandomFileName()}";
		}

		return uuid;
	}

	/// <summary>
	///     Checks if the given asset handle points to a generated asset generated via
	///     <see cref="GenerateAssetHandle{TAsset}" />. This method will also return <c>false</c> when the asset handle is
	///     invalid, or does not exist, or is not loaded, etc.
	/// </summary>
	/// <param name="asset"></param>
	/// <returns></returns>
	public static bool IsGeneratedAsset([UriString] string asset) =>
		TryParseUri(asset, out var host, out _) && host == null;

	public static bool TryResolveAsset([UriString] string asset, [NotNullWhen(true)] out FileHandle? fileHandle)
	{
		if (!IsValidAssetUri(asset, out string? host, out var path) || host == default)
		{
			fileHandle = default;
			return false;
		}

		var pathResolver = PathResolvers[host];
		fileHandle = pathResolver(path);
		return true;
	}

	public static void AddAssetResolver(IAssetResolver assetResolver)
	{
		if (AssetResolvers.Any(resolver => resolver.GetType() == assetResolver.GetType()))
		{
			return;
		}

		AssetResolvers.Add(assetResolver);
	}

	public static void AddSchemeResolver(string scheme, PathResolver pathResolver)
	{
		PathResolvers.Add(scheme, pathResolver);
	}

	public static void UnloadAll()
	{
		foreach (var asset in AssetTasks.Keys)
		{
			Unload(asset);
		}
	}

	/// <summary>
	///     Same as <see cref="UnloadAll" />, but named Dispose to match dispose pattern naming.
	/// </summary>
	public static void Dispose()
	{
		UnloadAll();
	}

	public static bool TryParseUri([UriString] string uri, out ReadOnlySpan<char> host, out ReadOnlySpan<char> path)
	{
		const string filePrefix = $"{DEFAULT_ASSET_SCHEME}://";
		const string generatedPrefix = $"{GENERATED_ASSET_SCHEME}:";

		var span = uri.AsSpan();
		if (uri.StartsWith(filePrefix))
		{
			var hostStartIdx = filePrefix.Length;
			var hostEndIdx = hostStartIdx;
			for (; hostEndIdx < uri.Length; hostEndIdx++)
			{
				if (uri[hostEndIdx] == '/')
				{
					break;
				}
			}

			if (hostEndIdx <= hostStartIdx)
			{
				host = default;
				path = default;
				return false;
			}

			var pathLength = uri.Length - hostEndIdx;
			if (pathLength <= 1) // 1, since we skip the first slash
			{
				host = default;
				path = default;
				return false;
			}

			host = span[hostStartIdx..hostEndIdx];
			path = span[(hostEndIdx + 1)..];
			return true;
		}

		host = default;
		if (uri.StartsWith(generatedPrefix))
		{
			path = span[generatedPrefix.Length..];
			return true;
		}

		path = default;
		return false;
	}
}

public enum AssetState
{
	/// <summary>
	///     This is an invalid asset
	/// </summary>
	Invalid,

	/// <summary>
	///     This asset has not yet been loaded
	/// </summary>
	Unloaded,

	/// <summary>
	///     This asset is currently loading
	/// </summary>
	Loading,

	/// <summary>
	///     This asset is loaded and ready to be used
	/// </summary>
	Loaded
}