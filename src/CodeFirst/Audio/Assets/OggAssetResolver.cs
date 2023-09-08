using System.Diagnostics.CodeAnalysis;
using CodeFirst.AssetManagement;
using CodeFirst.FileSystem;
using CodeFirst.Utils;
using CodeFirst.Utils.Logging;
using NVorbis;
using OpenTK.Mathematics;

namespace CodeFirst.Audio.Assets;

public class OggAssetResolver : IAssetResolver
{
	private const int MEGABYTES = 1_000_000;
	private const int MAX_FILE_SIZE = 10 * MEGABYTES;
	private static readonly ILogger Logger = LoggingManager.Get<OggAssetResolver>();

	public bool CanLoadAssetOfType(Type type) => type == typeof(Sound);

	public bool TryLoadAsset(FileHandle fileHandle, [NotNullWhen(true)] out object? asset)
	{
		if (!fileHandle.HasExtension(".ogg"))
		{
			asset = default;
			return false;
		}

		try
		{
			using var stream = fileHandle.Read();
			using var reader = new VorbisReader(stream, closeOnDispose: false);

			var totalSize = reader.TotalSamples * sizeof(float);
			if (totalSize > MAX_FILE_SIZE)
			{
				Logger.Warn(
					$"Tried to load very large sound file {fileHandle}. Maximum file size is {MAX_FILE_SIZE} bytes, but got {totalSize}.");
				asset = default;
				return false;
			}

			var samplesToRead = (int)reader.TotalSamples * reader.Channels;
			var samples = new float[samplesToRead];
			var readSampleCount = reader.ReadSamples(samples, offset: 0, samplesToRead);
			var data = ConvertSamplesToShort(samples, readSampleCount);

			var soundChannel = reader.Channels == 1 ? SoundChannel.Mono : SoundChannel.Stereo;
			var sampleRate = reader.SampleRate;
			var duration = (float)reader.TotalTime.TotalSeconds;
			asset = MainThread.Post(() => new Sound(data, soundChannel, sampleRate, duration)).Result;
			return true;
		}
		catch (Exception e)
		{
			Logger.Error($"Failed to load asset: {e}");
			asset = default;
			return false;
		}
	}

	private static short[] ConvertSamplesToShort(ReadOnlySpan<float> samples, int length)
	{
		if (length > samples.Length)
		{
			throw new ArgumentOutOfRangeException(nameof(length), length,
				$"{nameof(length)} ({length}) is greater than samples length ({samples.Length})");
		}

		var result = new short[length];
		for (var i = 0; i < length; i++)
		{
			result[i] = (short)MathHelper.Clamp((int)(samples[i] * short.MaxValue), short.MinValue, short.MaxValue);
		}

		return result;
	}
}