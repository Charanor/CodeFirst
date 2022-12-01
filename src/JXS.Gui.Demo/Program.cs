using JXS.Gui.Demo;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

var gameWindowSettings = GameWindowSettings.Default;
var nativeWindowSettings = new NativeWindowSettings
{
	Title = "Gui Demo",
	NumberOfSamples = 16,
	Size = new Vector2i(x: 1280, y: 720)
};

using var game = new Demo(gameWindowSettings, nativeWindowSettings);
game.Run();