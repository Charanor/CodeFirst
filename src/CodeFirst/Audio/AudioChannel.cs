using System.ComponentModel.DataAnnotations;
using CodeFirst.Graphics.Core.Utils;

namespace CodeFirst.Audio;

public class AudioChannel
{
	private float volume = 1;

	[Range(minimum: 0, maximum: 1)]
	public float Volume
	{
		get => volume;
		set => volume = value.Clamp(min: 0, max: 1);
	}

	public SoundInstance? PlaySound(Sound sound, bool looping = false, bool freeOnStop = false) =>
		sound.Play(volume, looping, freeOnStop);
}