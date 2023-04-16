using System.ComponentModel;
using JetBrains.Annotations;
using JXS.Async;
using JXS.Audio.Internals;
using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;

namespace JXS.Audio;

/// <summary>
///     Represents a single-buffered sound stream. This is usually only appropriate for small sound bytes (i.e. highly
///     compressed or only few seconds long).
/// </summary>
public class Sound
{
	private const int MIN_VOLUME = 0;
	private const int MAX_VOLUME = 1;

	private readonly Queue<AudioSource> availableAudioSources = new();

	private readonly AudioBuffer buffer;

	public Sound(ReadOnlySpan<short> data, SoundChannel channel, int sampleRate, float duration)
	{
		Channel = channel;
		Duration = duration;

		if (sampleRate <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(sampleRate), sampleRate, $"{nameof(sampleRate)} must be > 0");
		}

		var format = channel switch
		{
			SoundChannel.Mono => ALFormat.Mono16,
			SoundChannel.Stereo => ALFormat.Stereo16,
			_ => throw new InvalidEnumArgumentException(nameof(channel), (int)channel, typeof(SoundChannel))
		};
		buffer = AudioBuffer.Create(format, data, sampleRate);
	}

	public SoundChannel Channel { get; }
	public float Duration { get; }

	private AudioSource ObtainSource()
	{
		if (!availableAudioSources.TryDequeue(out var src))
		{
			src = new AudioSource(buffer);
		}

		return src;
	}

	private void FreeSource(AudioSource source)
	{
		availableAudioSources.Enqueue(source);
	}

	/// <summary>
	///     Plays this sound and returns the playing instance.
	/// </summary>
	/// <remarks>The <paramref name="freeOnStop" /> parameter <b>requires</b> <see cref="Coroutines"/> to be available and processed.</remarks>
	/// <returns>the new playing instance</returns>
	public SoundInstance Play([ValueRange(MIN_VOLUME, MAX_VOLUME)] float volume = 1f, bool looping = false,
		bool freeOnStop = false)
	{
		var source = ObtainSource();
		source.Volume = MathHelper.Clamp(volume, MIN_VOLUME, MAX_VOLUME);
		source.Looping = looping;
		source.Play();

		var soundInstance = new SoundInstance(this, source);
		if (freeOnStop)
		{
			Coroutines.Start(FreeOnStop());
		}

		IEnumerator<Wait?> FreeOnStop()
		{
			while (soundInstance.State != ALSourceState.Stopped)
			{
				yield return null;
			}

			soundInstance.Free();
		}

		return soundInstance;
	}

	internal void FreeInstance(SoundInstance instance)
	{
		FreeSource(instance.Source);
	}
}

public enum SoundChannel
{
	Mono,
	Stereo
}