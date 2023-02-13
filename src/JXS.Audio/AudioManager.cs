using JXS.Audio.Internals;
using OpenTK.Mathematics;

namespace JXS.Audio;

public sealed class AudioManager : IDisposable
{
	private readonly AudioDevice device;
	private readonly AudioContext context;
	private readonly AudioListener listener;

	public AudioManager()
	{
		device = new AudioDevice();
		context = new AudioContext(device);
		context.MakeCurrent();
		listener = context.Listener;
	}

	public Vector3 ListenerPosition
	{
		get => listener.Position;
		set => listener.Position = value;
	}

	public AudioListenerOrientation Orientation
	{
		get => listener.Orientation;
		set => listener.Orientation = value;
	}

	public float MasterVolume
	{
		get => listener.MasterVolume;
		set => listener.MasterVolume = value;
	}

	public void Dispose()
	{
		device.Dispose();
		context.Dispose();
	}
}