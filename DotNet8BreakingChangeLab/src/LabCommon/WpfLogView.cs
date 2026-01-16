#if WINDOWS
using System;
using System.Windows;
using System.Windows.Controls;

namespace LabCommon;

/// <summary>
/// Minimal log viewer for WPF demos.
/// </summary>
public sealed class WpfLogView
{
    private readonly TextBox _box;
    private readonly LogBuffer _log;

    public WpfLogView(TextBox box, LogBuffer log)
    {
        _box = box;
        _log = log;
        _box.IsReadOnly = true;
        _box.TextWrapping = TextWrapping.Wrap;
        _box.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

        // Changedイベントを利用し、ログ追加時にAppendを呼び出す
        _log.Changed += () =>
        {
            var snapshot = _log.Snapshot(1);
            if (!string.IsNullOrEmpty(snapshot))
            {
                Append(snapshot);
            }
        };
    }

    private void Append(string line)
    {
        // Ensure UI thread
        if (!_box.Dispatcher.CheckAccess())
        {
            _box.Dispatcher.Invoke(() => Append(line));
            return;
        }

        _box.AppendText(line + Environment.NewLine);
        _box.ScrollToEnd();
    }
}
#endif
