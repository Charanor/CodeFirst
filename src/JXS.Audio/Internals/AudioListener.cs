using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;

namespace JXS.Audio.Internals;

internal class AudioListener
{
	public float MasterVolume
	{
		get
		{
			AL.GetListener(ALListenerf.Gain, out var gain);
			return gain;
		}

		set => AL.Listener(ALListenerf.Gain, value);
	}

	public Vector3 Position
	{
		get
		{
			AL.GetListener(ALListener3f.Position, out var position);
			return position;
		}

		set => AL.Listener(ALListener3f.Position, value.X, value.Y, value.Z);
	}

	public Vector3 Velocity
	{
		get
		{
			AL.GetListener(ALListener3f.Velocity, out var position);
			return position;
		}

		set => AL.Listener(ALListener3f.Velocity, value.X, value.Y, value.Z);
	}

	public AudioListenerOrientation Orientation
	{
		get
		{
			AL.GetListener(ALListenerfv.Orientation, out var at, out var up);
			return new AudioListenerOrientation(at, up);
		}

		set
		{
			var at = value.At;
			var up = value.Up;
			AL.Listener(ALListenerfv.Orientation, ref at, ref up);
		}
	}
}