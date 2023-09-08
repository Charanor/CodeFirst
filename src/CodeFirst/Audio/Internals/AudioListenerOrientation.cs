using OpenTK.Mathematics;

namespace CodeFirst.Audio.Internals;

public readonly record struct AudioListenerOrientation(Vector3 At, Vector3 Up);