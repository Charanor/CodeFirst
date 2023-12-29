using System.Collections;

namespace CodeFirst.Gui;

public partial class Frame : IEnumerable<Frame>
{	
	public  void AddChild(Frame frame)
	{
		Node.AddChild(frame.Node);
		frame.Parent = this;
		frame.Scene = Scene;
		children.Add(frame);
	}

	public  bool RemoveChild(Frame frame)
	{
		var removed = children.Remove(frame);
		if (removed)
		{
			Node.RemoveChild(frame.Node);
			frame.Parent = null;
			frame.Scene = null;
		}

		return removed;
	}

	public void RemoveAllChildren()
	{
		foreach (var child in children)
		{
			Node.RemoveChild(child.Node);
			child.Parent = null;
			child.Scene = null;
		}

		children.Clear();
	}

	public T? GetChild<T>(string id) where T : Frame
	{
		foreach (var child in GetChildren())
		{
			if (child.Id == id)
			{
				if (child is not T validChild)
				{
					throw new InvalidOperationException(
						$"A child of {Id ?? "<no id>"} with id {id} exists, but is of type {child.GetType().Name}, expected {typeof(T).Name}.");
				}

				return validChild;
			}

			var frame = child.GetChild<T>(id);
			if (frame != null)
			{
				return frame;
			}
		}

		return null;
	}

	/// <summary>
	///		Checks if this frame, or any child of this frame (recursive) contains the given frame as a child.
	/// </summary>
	/// <param name="frame"></param>
	/// <returns></returns>
	/// <seealso cref="HasDirectChild"/>
	public bool HasChild(Frame frame)
	{
		foreach (var child in GetChildren())
		{
			if (child == frame)
			{
				return true;
			}

			if (child.HasChild(frame))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	///		Checks if this frame has the given frame as a direct child.
	/// </summary>
	/// <param name="frame"></param>
	/// <returns></returns>
	/// <seealso cref="HasChild"/>
	public bool HasDirectChild(Frame frame) => GetChildren().Contains(frame);

	public IEnumerator<Frame> GetEnumerator() => GetChildren().GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public IEnumerable<Frame> GetChildren() => children;
	public IEnumerable<T> GetChildren<T>() => GetChildren().OfType<T>();
}