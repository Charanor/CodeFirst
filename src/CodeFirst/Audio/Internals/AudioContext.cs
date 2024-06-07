using CodeFirst.Utils;
using OpenTK.Audio.OpenAL;

namespace CodeFirst.Audio.Internals;

internal class AudioContext : NativeResource
{
	private readonly ALContext context;

	public AudioContext(AudioDevice device)
	{
		context = ALC.CreateContext(device, new ALContextAttributes());
		if (context == ALContext.Null)
		{
			// Something went wrong
			var error = ALC.GetError(device);
			// throw new NullReferenceException($"Could not create OpenAL context. Reason: {error}");
		}
	}

	public AudioListener Listener { get; } = new();

	public bool MakeCurrent() => ALC.MakeContextCurrent(this);

	protected override void DisposeNativeResources()
	{
		ALC.DestroyContext(this);
	}

	public static implicit operator ALContext(AudioContext context) => context.context;
}