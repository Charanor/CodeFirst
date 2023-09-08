using CodeFirst.Audio.Internals;
using CodeFirst.Utils;
using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;

namespace CodeFirst.Audio;

public class SoundInstance
{
	private readonly object lockObject = new();

	private readonly Sound sound;
	private readonly float startVolume;

	private bool isFreed;

	internal SoundInstance(Sound sound, AudioSource source)
	{
		this.sound = sound;
		Source = source;
		startVolume = source.Volume;
		isFreed = false;
	}

	internal AudioSource Source { get; }

	public SoundChannel Channel => sound.Channel;
	public float TotalDuration => sound.Duration;

	public ALSourceState State => Source.State;

	public Vector3 Position
	{
		get => Source.Position;
		set => Source.Position = value;
	}

	public float ReferenceDistance
	{
		get => Source.ReferenceDistance;
		set => Source.ReferenceDistance = value;
	}

	public float Volume
	{
		get => Source.Volume;
		set => Source.Volume = value * startVolume;
	}

	public void Play()
	{
		lock (lockObject)
		{
			if (isFreed)
			{
				DevTools.Throw<SoundInstance>(
					new InvalidOperationException($"Attempted to call {nameof(Play)} on a freed sound instance."));
				return;
			}

			Source.Play();
		}
	}

	public void Pause()
	{
		lock (lockObject)
		{
			if (isFreed)
			{
				DevTools.Throw<SoundInstance>(
					new InvalidOperationException($"Attempted to call {nameof(Pause)} on a freed sound instance."));
				return;
			}

			Source.Pause();
		}
	}

	public void Stop()
	{
		lock (lockObject)
		{
			if (isFreed)
			{
				DevTools.Throw<SoundInstance>(
					new InvalidOperationException($"Attempted to call {nameof(Stop)} on a freed sound instance."));
				return;
			}

			Source.Stop();
		}
	}

	public void Free()
	{
		lock (lockObject)
		{
			if (isFreed)
			{
				return;
			}

			isFreed = true;
			sound.FreeInstance(this);
		}
	}
}