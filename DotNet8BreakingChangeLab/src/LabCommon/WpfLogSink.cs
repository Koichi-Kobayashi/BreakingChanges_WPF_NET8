using System.Windows.Controls;
using System.Windows.Threading;

namespace LabCommon;

/// <summary>
/// LogBuffer を WPF TextBox に流す簡易シンク。
/// </summary>
public sealed class WpfLogSink : IDisposable
{
    private readonly LogBuffer _log;
    private readonly TextBox _textBox;
    private readonly DispatcherTimer _timer;

    public WpfLogSink(LogBuffer log, TextBox textBox, TimeSpan? interval = null)
    {
        _log = log;
        _textBox = textBox;

        _timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = interval ?? TimeSpan.FromMilliseconds(100)
        };
        _timer.Tick += (_, __) => Flush();
        _timer.Start();
    }

    public void Flush()
    {
        var snapshot = _log.Snapshot();
        if (string.IsNullOrEmpty(snapshot)) return;

        _textBox.AppendText(snapshot + Environment.NewLine);
        _textBox.ScrollToEnd();
        _log.Clear();
    }

    public void Dispose()
    {
        _timer.Stop();
    }
}
