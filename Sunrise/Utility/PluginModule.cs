namespace Sunrise.Utility;

public abstract class PluginModule
{
    protected virtual List<PluginModule> SubModules { get; } = [];

    public bool IsEnabled { get; private set; }

    public void Enable()
    {
        if (IsEnabled)
            return;

        OnEnabled();
        IsEnabled = true;

        Handlers.Server.WaitingForPlayers += OnReset;

        foreach (PluginModule module in SubModules)
            module.Enable();
    }

    public void Disable()
    {
        if (!IsEnabled)
            return;

        OnDisabled();
        IsEnabled = false;

        Handlers.Server.WaitingForPlayers -= OnReset;

        foreach (PluginModule module in SubModules)
            module.Disable();
    }

    protected virtual void OnEnabled() { }
    protected virtual void OnDisabled() { }
    protected virtual void OnReset() { }
}