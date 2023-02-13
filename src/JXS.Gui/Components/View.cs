using System.Collections;
using Facebook.Yoga;
using OpenTK.Mathematics;

namespace JXS.Gui.Components;

public class View : Component<Style>, IEnumerable<Component>
{
	private readonly List<Component> children;

	public View()
	{
		children = new List<Component>();
	}

	public IEnumerator<Component> GetEnumerator() => children.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public override void Update(float delta)
	{
		base.Update(delta);
		foreach (var child in children.Where(c => c.Visible))
		{
			child.Update(delta);
		}
	}

	public override void Draw(IGraphicsProvider graphicsProvider)
	{
		base.Draw(graphicsProvider);

		if (Style.Overflow == YogaOverflow.Hidden)
		{
			graphicsProvider.BeginOverflow();
			{
				DrawChildren();
			}
			graphicsProvider.EndOverflow();
		}
		else
		{
			DrawChildren();
		}


		void DrawChildren()
		{
			foreach (var child in children.Where(c => c.Visible))
			{
				child.Draw(graphicsProvider);
			}
		}
	}

	public virtual void AddChild(Component component)
	{
		Node.AddChild(component.Node);
		component.Parent = this;
		component.Scene = Scene;
		children.Add(component);
	}

	public virtual bool RemoveChild(Component component)
	{
		var removed = children.Remove(component);
		if (removed)
		{
			Node.RemoveChild(component.Node);
			component.Parent = null;
			component.Scene = null;
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

	public override void ApplyStyle()
	{
		base.ApplyStyle();
		foreach (var child in children)
		{
			child.ApplyStyle();
		}
	}

	public T? GetChild<T>(string id) where T : Component
	{
		foreach (var child in children)
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

			if (child is not View view)
			{
				continue;
			}

			var cmp = view.GetChild<T>(id);
			if (cmp != null)
			{
				return cmp;
			}
		}

		return null;
	}

	public bool HasChild(Component component)
	{
		if (component is null)
		{
			throw new ArgumentNullException(nameof(component));
		}

		foreach (var child in children)
		{
			if (child == component)
			{
				return true;
			}

			if (child is View view && view.HasChild(component))
			{
				return true;
			}
		}

		return false;
	}

	public bool HasDirectChild(Component component)
	{
		if (component is null)
		{
			throw new ArgumentNullException(nameof(component));
		}

		return children.Any(child => child == component);
	}

	public IEnumerable<Component> GetChildren() => children;
	public IEnumerable<T> GetChildren<T>() => children.Where(c => c is T).Cast<T>();

	public override Component? Hit(Vector2 position)
	{
		if (!Visible)
		{
			return null;
		}

		var component = base.Hit(position);
		if (component is null)
		{
			return null;
		}

		// If we are hit, check if it also hit a child.
		// If no child was hit, return this.
		for (var i = children.Count - 1; i >= 0; i--)
		{
			var child = children[i];
			var childHit = child.Hit(position);
			if (childHit != null)
			{
				return childHit;
			}
		}

		return component;
	}
}