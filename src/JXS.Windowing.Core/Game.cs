using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace JXS.Windowing.Core;

public abstract class Game : GameWindow
{
	private bool running;
	private Vector2i currentWindowSize;

	private Screen? screen;
	private bool hasShownScreen;

	protected Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(
		gameWindowSettings, nativeWindowSettings)
	{
	}

	public Screen? Screen
	{
		get => screen;
		set
		{
			if (value != screen)
			{
				screen?.Hide();
				screen = value;
				hasShownScreen = false;
			}
			ShowScreenIfRunning();
		}
	}

	protected abstract void LoadResources();
	protected abstract void UnloadResources();

	protected override void OnUpdateFrame(FrameEventArgs args)
	{
		base.OnUpdateFrame(args);
		screen?.Update((float)args.Time);
	}

	protected override void OnRenderFrame(FrameEventArgs args)
	{
		base.OnRenderFrame(args);
		screen?.Draw((float)args.Time);
	}

	protected override void OnLoad()
	{
		base.OnLoad();
		running = true;
		LoadResources();
		ShowScreenIfRunning();
	}

	protected override void OnUnload()
	{
		base.OnUnload();
		UnloadResources();

		screen?.Hide();
		screen?.Dispose();
		// Note: Do not do use the property "Screen", use the internal "screen" so we don't call the logic of the setter
		screen = null;
	}

	protected override void OnResize(ResizeEventArgs e)
	{
		base.OnResize(e);
		currentWindowSize = e.Size;
		screen?.Resized(e.Width, e.Height);
	}

	private void ShowScreenIfRunning()
	{
		if (screen == null || !running || hasShownScreen)
		{
			return;
		}

		hasShownScreen = true;

		screen.Game = this;
		screen.Show();
		if (currentWindowSize.EuclideanLength != 0)
		{
			screen.Resized(currentWindowSize.X, currentWindowSize.Y);
		}
	}
}