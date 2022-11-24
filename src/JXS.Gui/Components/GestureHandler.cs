using JXS.Utils.Events;

namespace JXS.Gui.Components;

public class GestureHandlerEnterEventArgs : EventArgs
{
    public GestureHandlerEnterEventArgs(Component component)
    {
        Component = component;
    }

    /// <summary>
    ///     The actual component. Might be the GestureHandler itself or a child.
    /// </summary>
    public Component Component { get; }
}

public class GestureHandlerExitEventArgs : EventArgs
{
    public GestureHandlerExitEventArgs(Component? exitToComponent)
    {
        ExitToComponent = exitToComponent;
    }

    /// <summary>
    ///     The component that is now being hovered over, or null if no component.
    /// </summary>
    public Component? ExitToComponent { get; }
}

public class GestureHandler : View
{
    private bool cursorInside;

    public GestureHandler(Style? style, string? id, IInputProvider inputProvider) : base(style, id, inputProvider)
    {
    }

    /// <summary>
    ///     Invoked when this gesture handler is "entered" by a user. E.g. by them hovering their cursor or
    ///     tab-selecting this component.
    /// </summary>
    public event EventHandler<GestureHandler, GestureHandlerEnterEventArgs>? OnEnter;

    /// <summary>
    ///     Invoked when this gesture handler is "exited" by a user. E.g. by them moving their cursor out or
    ///     tab-selecting away from this component.
    /// </summary>
    public event EventHandler<GestureHandler, GestureHandlerExitEventArgs>? OnExit;

    public override void Update(float delta)
    {
        base.Update(delta);
        if (!Visible) return;
        var mousePos = InputProvider.MousePosition;
        var component = Scene!.Hit(mousePos);
        var hitsThisOrChild = component is not null && (component == this || HasChild(component));

        switch (hitsThisOrChild)
        {
            case true when !cursorInside:
                cursorInside = true;
                OnEnter?.Invoke(this, new GestureHandlerEnterEventArgs(component!));
                break;
            case false when cursorInside:
                cursorInside = false;
                OnExit?.Invoke(this, new GestureHandlerExitEventArgs(component));
                break;
        }
    }
}