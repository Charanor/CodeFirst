using System.Runtime.InteropServices;
using Facebook.Yoga;
using JXS.Graphics.Core;
using JXS.Gui.Components;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace JXS.Gui.Demo;

public class Demo : GameWindow
{
	private const int PADDING = 20; // px

	private readonly Camera camera;
	private readonly Scene scene;

	private Texture2D? texture;

	public Demo(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(
		gameWindowSettings, nativeWindowSettings)
	{
		var windowSize = nativeWindowSettings.Size;
		camera = new OrthographicCamera(windowSize.X, windowSize.Y)
		{
			Position = new Vector3((windowSize.X - PADDING) / 2f, (windowSize.Y - PADDING) / 2f, z: 1)
		};

		scene = new Scene(new DemoGraphicsProvider(camera), new DemoInputProvider())
		{
			Size = windowSize
		};

		GL.Enable(EnableCap.DebugOutput);
		GL.Enable(EnableCap.DebugOutputSynchronous);
		GL.DebugMessageCallback(
			callback: (source, type, id, severity, length, message, _) =>
			{
				Console.WriteLine($"{source}, {type}, {id}, {severity}, {Marshal.PtrToStringAnsi(message, length)}");
			}, IntPtr.Zero);
	}

	protected override void OnLoad()
	{
		base.OnLoad();

		Color4<Rgba>[] colors =
		{
			Color4.Red,
			Color4.Green,
			Color4.Green,
			Color4.Red
		};
		var bytes = colors.SelectMany(color =>
		{
			var (r, g, b, a) = color;
			return BitConverter.GetBytes(r)
				.Concat(BitConverter.GetBytes(g))
				.Concat(BitConverter.GetBytes(b))
				.Concat(BitConverter.GetBytes(a));
		}).ToArray();
		texture = new Texture2D(bytes, width: 2, height: 2);
		var root = new View(id: "root", new Style
		{
			Width = YogaValue.Percent(100),
			Height = YogaValue.Percent(100),
			JustifyContent = YogaJustify.Center,
			AlignItems = YogaAlign.Center,
			BackgroundColor = Color4.Orange
		});
		var center = new Image(id: "center", texture, new Style
		{
			Width = 100,
			Height = 100,
			BackgroundColor = Color4.Blue
		});
		root.AddChild(center);
		scene.AddComponent(root);
		scene.__DELETE__ME_CalculateLayout();
	}

	protected override void OnUnload()
	{
		base.OnUnload();
		texture?.Dispose();
	}

	protected override void OnUpdateFrame(FrameEventArgs args)
	{
		base.OnUpdateFrame(args);
		scene.Update((float)args.Time);
	}

	protected override void OnRenderFrame(FrameEventArgs args)
	{
		base.OnRenderFrame(args);
		GL.Disable(EnableCap.DepthTest);
		GL.ClearColor(red: 0, green: 0, blue: 0, alpha: 1);
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

		scene.Draw();

		SwapBuffers();
	}

	protected override void OnResize(ResizeEventArgs e)
	{
		base.OnResize(e);
		scene.Size = e.Size - new Vector2i(PADDING, PADDING);
		camera.Update(e.Width, e.Height);
	}
}