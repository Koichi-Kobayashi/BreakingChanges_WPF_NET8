using System.Collections.Concurrent;
using System.Text;

namespace LabCommon;

/// <summary>
/// Thread-safe log buffer.
/// </summary>
public sealed class LogBuffer
{
    private readonly ConcurrentQueue<string> _lines = new();

    public event Action? Changed;

    public void WriteLine(string message)
    {
        var line = $"{DateTime.Now:HH:mm:ss.fff} {message}";
        _lines.Enqueue(line);
        Changed?.Invoke();
    }

    public string Snapshot(int maxLines = 2000)
    {
        var sb = new StringBuilder();
        var items = _lines.ToArray();

        var start = Math.Max(0, items.Length - maxLines);
        for (int i = start; i < items.Length; i++)
        {
            sb.AppendLine(items[i]);
        }

        return sb.ToString();
    }

    public void Clear()
    {
        while (_lines.TryDequeue(out _)) { }
        Changed?.Invoke();
    }
}
