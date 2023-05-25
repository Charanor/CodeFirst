using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Facebook.Yoga;
using JXS.Assets.Core;
using JXS.Assets.Core.Loaders;
using JXS.Graphics.Core;
using JXS.Graphics.Core.Assets;
using JXS.Graphics.Text.Assets;
using JXS.Graphics.Utils;
using JXS.Gui.Components;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace JXS.Gui.Demo;

public class Demo : GameWindow
{
	private const int PADDING = 50; // px

	private static readonly TextAssetDefinition TextExampleGuiLayout = new("Layouts/TextExample.xml");

	private readonly Camera camera;
	private readonly Scene scene;
	private readonly AssetManager assetManager;

	public Demo(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(
		gameWindowSettings, nativeWindowSettings)
	{
		var windowSize = nativeWindowSettings.Size;
		camera = new OrthographicCamera(windowSize.X, windowSize.Y)
		{
			Position = new Vector3((windowSize.X - PADDING) / 2f, (windowSize.Y - PADDING) / 2f, z: 1)
		};

		assetManager = new AssetManager();
		assetManager.AddAssetLoader(new TextAssetLoader());
		assetManager.AddAssetLoader(new TextureAssetLoader());
		assetManager.AddAssetLoader(new FontAssetLoader(assetManager));

		var demoGraphicsProvider = new GLGraphicsProvider(camera);
		var demoGuiInputProvider = new DemoGuiInputProvider();

		Debug.Assert(assetManager.TryLoadAsset(TextExampleGuiLayout, out var textAsset));
		var uiLoader = new UILoader(demoGraphicsProvider, demoGuiInputProvider, assetManager);
		scene = uiLoader.LoadSceneFromXml(XDocument.Parse(textAsset.Text));

		GL.Enable(EnableCap.DebugOutput);
		GL.Enable(EnableCap.DebugOutputSynchronous);
		GL.DebugMessageCallback(
			(source, type, id, severity, length, message, _) =>
			{
				Console.WriteLine($"{source}, {type}, {id}, {severity}, {Marshal.PtrToStringAnsi(message, length)}");
			}, IntPtr.Zero);

		TextInput += e =>
		{
			demoGuiInputProvider.TextInput(e.AsString);
			Console.WriteLine($"code: {e.Unicode}, str: {e.AsString}");
		};
		KeyDown += e =>
		{
			Console.WriteLine(
				$"k: {e.Key}, alt: {e.Alt}, ctrl: {e.Control}, cmd: {e.Command}, shift: {e.Shift}, modifiers: {e.Modifiers}, repeat: {e.IsRepeat}, scanCode: {e.ScanCode}");
		};
	}

	protected override void OnUnload()
	{
		base.OnUnload();
		assetManager.Dispose();
	}

	protected override void OnUpdateFrame(FrameEventArgs args)
	{
		base.OnUpdateFrame(args);
		scene.Update((float)args.Time);
	}

	protected override void OnRenderFrame(FrameEventArgs args)
	{
		base.OnRenderFrame(args);
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