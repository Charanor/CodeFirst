using System.Collections;
using JXS.Gui.Components;
using OpenTK.Mathematics;

namespace JXS.Gui;

public class Scene : IEnumerable<Component>
{
	private readonly List<Component> components;

	public Scene(IGraphicsProvider graphicsProvider, IInputProvider inputProvider)
	{
		GraphicsProvider = graphicsProvider;
		InputProvider = inputProvider;
		components = new List<Component>();
	}

	public IInputProvider InputProvider { get; init; }
	public IGraphicsProvider GraphicsProvider { get; init; }

	public Vector2 Size { get; set; } = new(float.NaN, float.NaN);

	public IEnumerator<Component> GetEnumerator() => components.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public IReadOnlyList<Component> GetComponents() => components;

	public void Update(float delta)
	{
		if (InputProvider.JustPressed(InputAction.Primary))
		{
			var mousePos = InputProvider.MousePosition;
			var component = Hit(mousePos);
			if (component is null)
			{
				InputProvider.KeyboardFocus = null;
			}
		}

		foreach (var component in components.Where(c => c.Visible))
		{
			component.Update(delta);
		}
	}

	public void Draw()
	{
		GraphicsProvider.Begin();
		foreach (var component in components.Where(c => c.Visible))
		{
			component.ApplyStyle();
			component.CalculateLayout(Size.X, Size.Y);
			component.Draw(GraphicsProvider);
		}

		GraphicsProvider.End();
	}

	public void AddComponent<TComponent>(TComponent component) where TComponent : Component
	{
		component.Scene = this;
		components.Add(component);
	}

	public bool RemoveComponent<TComponent>(TComponent component) where TComponent : Component
	{
		var removed = components.Remove(component);
		if (removed)
		{
			component.Scene = null;
		}

		return removed;
	}

	/// <summary>
	///     Gets a component in this scene with the given "id".
	/// </summary>
	/// <param name="id">the id of the component</param>
	/// <typeparam name="TComponent">the type of the component</typeparam>
	/// <returns>the found component with given id of type T</returns>
	/// <exception cref="NullReferenceException">if no component with given id exists</exception>
	/// <exception cref="InvalidOperationException">if a component with given id exists, but is not of type T</exception>
	public TComponent GetComponent<TComponent>(string id) where TComponent : Component
	{
		foreach (var component in components)
		{
			if (component.Id == id)
			{
				if (component is TComponent cmp)
				{
					return cmp;
				}

				throw new InvalidOperationException(
					$"A component with id {id} exists, but is of type {component.GetType().Name}, expected {typeof(TComponent).Name}.");
			}

			if (component is not View view)
			{
				continue;
			}

			var child = view.GetChild<TComponent>(id);
			if (child != null)
			{
				return child;
			}
		}

		throw new NullReferenceException($"No component with id {id} exists.");
	}

	public bool TryGetComponent<TComponent>(string id, out TComponent? outComponent) where TComponent : Component
	{
		foreach (var component in components)
		{
			if (component.Id == id)
			{
				if (component is TComponent cmp)
				{
					outComponent = cmp;
					return true;
				}

				outComponent = null;
				return false;
			}

			if (component is not View view)
			{
				continue;
			}

			var child = view.GetChild<TComponent>(id);
			if (child == null)
			{
				continue;
			}

			outComponent = child;
			return true;
		}

		outComponent = null;
		return false;
	}

	public Component? Hit(Vector2 position)
	{
		// Reverse iteration
		for (var i = components.Count - 1; i >= 0; i--)
		{
			var component = components[i].Hit(position);
			if (component != null)
			{
				return component;
			}
		}

		return null;
	}
}