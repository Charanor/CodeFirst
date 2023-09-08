using CodeFirst.Utils;
using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;

namespace CodeFirst.Audio.Internals;

internal class AudioSource : NativeResource
{
	private readonly int sourceId;

	private AudioSource()
	{
		sourceId = AL.GenSource();
		ALError.ThrowIfError("Could not instantiate audio source");
	}

	public AudioSource(AudioBuffer buffer) : this()
	{
		AL.BindBufferToSource(this, buffer);
		ALError.ThrowIfError("Could not bind buffer to audio source");
	}

	public AudioSource(IEnumerable<AudioBuffer> buffers) : this()
	{
		AL.SourceQueueBuffers(this, buffers.Select(buff => (int)buff).ToArray());
		ALError.ThrowIfError("Could not stream buffers to audio source");
	}

	public ALSourceState State => AL.GetSourceState(this);

	public Vector3 Position
	{
		get
		{
			AL.GetSource(this, ALSource3f.Position, out var position);
			return position;
		}

		set => AL.Source(this, ALSource3f.Position, value.X, value.Y, value.Z);
	}

	public bool Looping
	{
		get
		{
			AL.GetSource(this, ALSourceb.Looping, out var looping);
			return looping;
		}

		set => AL.Source(this, ALSourceb.Looping, value);
	}

	public float Volume
	{
		get
		{
			AL.GetSource(this, ALSourcef.Gain, out var volume);
			return volume;
		}

		set => AL.Source(this, ALSourcef.Gain, value);
	}

	public float ReferenceDistance
	{
		get
		{
			AL.GetSource(this, ALSourcef.ReferenceDistance, out var distance);
			return distance;
		}

		set => AL.Source(this, ALSourcef.ReferenceDistance, value);
	}

	public void Play()
	{
		AL.SourcePlay(this);
		ALError.ThrowIfError("Could not play audio source");
	}

	public void Pause()
	{
		AL.SourcePause(this);
		ALError.ThrowIfError("Could not pause audio source");
	}

	public void Stop()
	{
		AL.SourceStop(this);
		ALError.ThrowIfError("Could not stop audio source");
	}

	protected override void DisposeNativeResources()
	{
		AL.DeleteSource(sourceId);
	}

	public static implicit operator int(AudioSource source) => source.sourceId;
}