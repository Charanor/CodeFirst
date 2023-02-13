using JXS.Utils;
using OpenTK.Audio.OpenAL;

namespace JXS.Audio.Internals;

internal class AudioBuffer : NativeResource
{
	private readonly int bufferId;

	private AudioBuffer()
	{
		AL.GetError(); // Clear error
		bufferId = AL.GenBuffer();
		ALError.ThrowIfError("Could not instantiate audio buffer");
	}

	protected override void DisposeNativeResources()
	{
		AL.DeleteBuffer(bufferId);
	}

	public static implicit operator int(AudioBuffer buffer) => buffer.bufferId;

	public static AudioBuffer Create(ALFormat format, ReadOnlySpan<short> data, int frequency)
	{
		var buffer = new AudioBuffer();

		AL.BufferData(buffer, format, data, frequency);
		ALError.ThrowIfError("Could not upload data to audio buffer");

		return buffer;
	}

	public static AudioBuffer Create(ALFormat format, ReadOnlySpan<byte> data, int frequency)
	{
		var buffer = new AudioBuffer();

		AL.BufferData(buffer, format, data, frequency);
		ALError.ThrowIfError("Could not upload data to audio buffer");

		return buffer;
	}
}