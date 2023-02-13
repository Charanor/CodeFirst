using JXS.Utils;
using OpenTK.Audio.OpenAL;

namespace JXS.Audio.Internals;

internal class AudioDevice : NativeResource
{
	private readonly ALDevice device;

	public AudioDevice(string? deviceName = null)
	{
		device = ALC.OpenDevice(deviceName);
		if (device == ALDevice.Null)
		{
			// Something went wrong
			var error = ALC.GetError(device);
			var nullNote = deviceName == null ? " (note: \"null\" is a valid name)" : "";
			throw new NullReferenceException(
				$"Could not create OpenAL device with name \"{deviceName}\"{nullNote}. Reason: {error}");
		}
	}

	protected override void DisposeNativeResources()
	{
		ALC.CloseDevice(this);
	}

	public static implicit operator ALDevice(AudioDevice device) => device.device;
}