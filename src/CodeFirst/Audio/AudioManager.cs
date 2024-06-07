using System.ComponentModel.DataAnnotations;
using CodeFirst.Audio.Internals;
using JetBrains.Annotations;
using OpenTK.Mathematics;

namespace CodeFirst.Audio;

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

	[PublicAPI]
	public Vector3 ListenerPosition
	{
		get => listener.Position;
		set => listener.Position = value;
	}

	[PublicAPI]
	public AudioListenerOrientation Orientation
	{
		get => listener.Orientation;
		set => listener.Orientation = value;
	}

	[PublicAPI]
	[Range(minimum: 0, maximum: 1)]
	public float MasterVolume
	{
		get => listener.MasterVolume;
		set => listener.MasterVolume = value;
	}

	[PublicAPI]
	public void Dispose()
	{
		device.Dispose();
		context.Dispose();
	}
}