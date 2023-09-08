using OpenTK.Audio.OpenAL;

namespace CodeFirst.Audio.Internals;

public static class ALError
{
	public static void ThrowIfError(string message)
	{
		var error = AL.GetError();
		if (error != OpenTK.Audio.OpenAL.ALError.NoError)
		{
			throw new NullReferenceException($"{message}. Reason: {error}-{AL.GetErrorString(error)}");
		}
	}
}