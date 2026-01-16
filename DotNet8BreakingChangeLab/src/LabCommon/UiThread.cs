using System;
using System.Windows.Threading;

namespace LabCommon;

public static class UiThread
{
    /// <summary>
    /// UI スレッドに戻して action を実行する。
    /// </summary>
    public static void InvokeIfRequired(Dispatcher dispatcher, Action action)
    {
        if (dispatcher.CheckAccess()) action();
        else dispatcher.Invoke(action);
    }
}
