using System.Diagnostics.CodeAnalysis;
using CodeFirst.Utils;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using StbImageSharp;

namespace CodeFirst.Graphics.Core.Assets;

public static class TextureAssetUtils
{
	private static readonly IDictionary<string, TextureMetadata> MetadataCache =
		new Dictionary<string, TextureMetadata>();

	public static bool TryGetMetadata([UriString] string textureAsset,
		[NotNullWhen(true)] out TextureMetadata? metadata, FetchMode fetchMode = FetchMode.CacheThenSource)
	{
		switch (fetchMode)
		{
			case FetchMode.CacheThenSource:
				if (MetadataCache.TryGetValue(textureAsset, out metadata))
				{
					return true;
				}

				if (!TryReadMetadataFromFile(textureAsset, out metadata))
				{
					return false;
				}

				CacheMetadata(textureAsset, metadata);
				return true;
			case FetchMode.SourceThenCache:
				// ReSharper disable once InvertIf
				if (TryReadMetadataFromFile(textureAsset, out metadata))
				{
					CacheMetadata(textureAsset, metadata);
					return true;
				}

				return MetadataCache.TryGetValue(textureAsset, out metadata);
			case FetchMode.CacheOnly:
				return MetadataCache.TryGetValue(textureAsset, out metadata);
			case FetchMode.SourceOnly:
				if (!TryReadMetadataFromFile(textureAsset, out metadata))
				{
					return false;
				}

				CacheMetadata(textureAsset, metadata);
				return true;
			case FetchMode.NoCache:
				return TryReadMetadataFromFile(textureAsset, out metadata);
			default:
				throw new ArgumentOutOfRangeException(nameof(fetchMode), fetchMode, message: null);
		}
	}

	private static bool TryReadMetadataFromFile([UriString] string textureAsset,
		[NotNullWhen(true)] out TextureMetadata? metadata)
	{
		if (!AssetManagement.Assets.TryResolveAsset(textureAsset, out var fileHandle))
		{
			metadata = default;
			return false;
		}

		using var stream = fileHandle.Read();
		var fileInfo = ImageInfo.FromStream(stream);
		if (!fileInfo.HasValue)
		{
			metadata = null;
			return false;
		}

		var info = fileInfo.Value;
		metadata = new TextureMetadata(new Vector2i(info.Width, info.Height));
		return true;
	}

	private static void CacheMetadata([UriString] string textureAsset, TextureMetadata metadata)
	{
		if (MetadataCache.ContainsKey(textureAsset))
		{
			MetadataCache[textureAsset] = metadata;
		}
		else
		{
			MetadataCache.Add(textureAsset, metadata);
		}
	}

	public record TextureMetadata(Vector2i Size);
}