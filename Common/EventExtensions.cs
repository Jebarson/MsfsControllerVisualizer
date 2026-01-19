namespace Msfs.ControllerVisualizer.Common;

public static class EventExtensions
{
    public static void RaiseEvent(this EventHandler? handler, object sender)
    {
        handler?.Invoke(sender, EventArgs.Empty);
    }

    public static void RaiseEvent<T>(this EventHandler<T>? handler, object sender, T args) where T : EventArgs
    {
        handler?.Invoke(sender, args);
    }
}
