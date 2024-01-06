using System.ComponentModel.DataAnnotations;
using CodeFirst.Graphics.Core.Utils;

namespace CodeFirst.Audio;

public class AudioChannel
{
	private float volume = 1;

	[Range(0, 1)]
	public float Volume
	{
		get => volume;
		set => volume = value.Clamp(0, 1);
	}

	public SoundInstance? PlaySound(Sound sound, bool looping = false, bool freeOnStop = false)
	{
		return sound.Play(volume, looping, freeOnStop);
	}
}