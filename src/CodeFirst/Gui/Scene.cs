using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Facebook.Yoga;
using OpenTK.Mathematics;

namespace CodeFirst.Gui;

public class Scene : IEnumerable<Frame>
{
	private readonly List<Frame> frames;
	private Vector2 mousePosition;

	private Frame? previousHoverFrame;

	public Scene(IGraphicsProvider graphicsProvider)
	{
		GraphicsProvider = graphicsProvider;
		frames = new List<Frame>();
	}

	public IGraphicsProvider GraphicsProvider { get; init; }

	public Vector2 Size { get; set; } = new(float.NaN, float.NaN);

	public Vector2 MousePosition
	{
		get => mousePosition;
		set
		{
			mousePosition = value;
			var hit = Hit(value);
			if (hit == previousHoverFrame)
			{
				return;
			}
			
			previousHoverFrame?.CursorExit();
			previousHoverFrame = hit;
			hit?.CursorEnter();
		}
	}

	public IEnumerator<Frame> GetEnumerator() => frames.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public IReadOnlyList<Frame> GetFrames() => frames;

	public void Update(float delta)
	{
		foreach (var frame in frames.Where(c => c.Visible))
		{
			frame.Update(delta);
		}
	}

	public void Draw()
	{
		GraphicsProvider.Begin();
		foreach (var frame in frames.Where(c => c.Visible))
		{
			frame.ApplyStyle();
			frame.CalculateLayout(Size.X, Size.Y);
			frame.Draw(GraphicsProvider);
		}

		GraphicsProvider.End();
	}

	public void AddFrame<TFrame>(TFrame frame) where TFrame : Frame
	{
		frame.Scene = this;
		frames.Add(frame);
	}

	public bool RemoveFrame<TFrame>(TFrame frame) where TFrame : Frame
	{
		var removed = frames.Remove(frame);
		if (removed)
		{
			frame.Scene = null;
		}

		return removed;
	}

	/// <summary>
	///     Gets a component in this scene with the given "id".
	/// </summary>
	/// <param name="id">the id of the component</param>
	/// <typeparam name="TFrame">the type of the component</typeparam>
	/// <returns>the found component with given id of type T</returns>
	/// <exception cref="NullReferenceException">if no component with given id exists</exception>
	/// <exception cref="InvalidOperationException">if a component with given id exists, but is not of type T</exception>
	public TFrame GetFrame<TFrame>(string id) where TFrame : Frame
	{
		foreach (var frame in frames)
		{
			if (frame.Id == id)
			{
				if (frame is TFrame cmp)
				{
					return cmp;
				}

				throw new InvalidOperationException(
					$"A component with id {id} exists, but is of type {frame.GetType().Name}, expected {typeof(TFrame).Name}.");
			}

			var child = frame.GetChild<TFrame>(id);
			if (child != null)
			{
				return child;
			}
		}

		throw new NullReferenceException($"No component with id {id} exists.");
	}

	public bool TryGetFrame<TFrame>(string id, [NotNullWhen(true)] out TFrame? outFrame) where TFrame : Frame
	{
		foreach (var frame in frames)
		{
			if (frame.Id == id)
			{
				if (frame is TFrame cmp)
				{
					outFrame = cmp;
					return true;
				}

				outFrame = null;
				return false;
			}

			var child = frame.GetChild<TFrame>(id);
			if (child == null)
			{
				continue;
			}

			outFrame = child;
			return true;
		}

		outFrame = null;
		return false;
	}

	public Frame? Hit(Vector2 position)
	{
		// Reverse iteration
		for (var i = frames.Count - 1; i >= 0; i--)
		{
			var component = frames[i].Hit(position);
			if (component != null)
			{
				return component;
			}
		}

		return null;
	}

	public bool PressIn(UiAction action)
	{
		var hit = Hit(MousePosition);
		if (hit == null)
		{
			return false;
		}

		return hit.PressIn(action);
	}

	public bool PressOut(UiAction action)
	{
		var hit = Hit(MousePosition);
		if (hit == null)
		{
			return false;
		}

		return hit.PressOut(action);
	}
}