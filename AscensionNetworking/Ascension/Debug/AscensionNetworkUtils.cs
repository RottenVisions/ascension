using System;

public static class AscensionNetworkUtils
{
    public static Action Combine(this Action self, Action action)
    {
        return (Action)Delegate.Combine(self, action);
    }

}
