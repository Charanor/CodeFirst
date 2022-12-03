using System.Runtime.InteropServices;
using Facebook.Yoga;
using JXS.Assets.Core;
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

	private static readonly TextureAssetDefinition ImmortalFontAtlas = new("Fonts/IMMORTAL.mtsdf.png");
	private static readonly FontAssetDefinition ImmortalFont = new(Path: "Fonts/IMMORTAL.mtsdf.json", ImmortalFontAtlas);

	private readonly Camera camera;
	private readonly Scene scene;
	private readonly AssetManager assetManager;

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

		assetManager = new AssetManager();
		assetManager.AddAssetLoader(new TextureAssetLoader());
		assetManager.AddAssetLoader(new FontAssetLoader(assetManager));

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

		if (!assetManager.TryLoadAsset(ImmortalFont, out var font))
		{
			throw new NullReferenceException();
		}

		var bytes = new[]
		{
			Color4.Red,
			Color4.Green,
			Color4.Green,
			Color4.Red
		}.ToByteArray();

		texture = new Texture2D(bytes, width: 2, height: 2)
		{
			MinFilter = TextureMinFilter.Nearest,
			MagFilter = TextureMagFilter.Nearest
		};

		var text = new __NEW_Text(font, value: "abcd efgh ijkl mnop qrst uvw xyz ABCD EFGH IJKL MNOP QRST UVW XYZ")
		{
			Style = new TextStyle
			{
				Width = YogaValue.Percent(100),
				Height = YogaValue.Percent(100),
				FontSize = 100,
				BackgroundColor = Color4.White,
				FontColor = Color4.Red,
			}
		};
		scene.AddComponent(text);
	}

	protected override void OnUnload()
	{
		base.OnUnload();
		texture?.Dispose();
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