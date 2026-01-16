using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LabCommon;

namespace DnDDataObjectFormatProbe;

public partial class MainWindow : Window
{
    private readonly LogBuffer _log = new();

    public MainWindow()
    {
        InitializeComponent();

        _log.Changed += () =>
        {
            Dispatcher.Invoke(() =>
            {
                LogTextBox.Text = _log.Snapshot();
                if (AutoScroll.IsChecked == true)
                    LogTextBox.ScrollToEnd();
            });
        };

        Loaded += (_, _) =>
        {
            _log.WriteLine($"FrameworkDescription: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
            _log.WriteLine("Tip: In .NET 7+, some text drag-and-drop DataObject formats changed (Text/UnicodeText vs StringFormat). If you have code that expects a specific format, this project helps you see what you actually get at runtime.");
        };

        // Start drag from source
        SourceTextBox.PreviewMouseMove += Source_PreviewMouseMove;
    }

    private void Source_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
            return;

        if (sender is not TextBox tb)
            return;

        var text = tb.SelectedText;
        if (string.IsNullOrEmpty(text))
            text = tb.Text;

        var data = new DataObject();
        data.SetData(DataFormats.UnicodeText, text);
        data.SetData(DataFormats.Text, text);
        data.SetData(DataFormats.StringFormat, text);

        _log.WriteLine("--- DoDragDrop start ---");
        _log.WriteLine("We explicitly set: UnicodeText, Text, StringFormat");
        DumpFormats(data, "DoDragDrop payload formats");

        DragDrop.DoDragDrop(tb, data, DragDropEffects.Copy);
        _log.WriteLine("--- DoDragDrop end ---");
    }

    private void Target_PreviewDragEnter(object sender, DragEventArgs e)
    {
        _log.WriteLine("[DragEnter]");
        DumpFormats(e.Data, "Formats");
    }

    private void Target_PreviewDragOver(object sender, DragEventArgs e)
    {
        // Keep copy
        e.Effects = DragDropEffects.Copy;
        e.Handled = true;
    }

    private void Target_PreviewDrop(object sender, DragEventArgs e)
    {
        _log.WriteLine("[Drop]");
        DumpFormats(e.Data, "Formats");

        var text =
            (e.Data.GetData(DataFormats.UnicodeText) as string)
            ?? (e.Data.GetData(DataFormats.Text) as string)
            ?? (e.Data.GetData(DataFormats.StringFormat) as string)
            ?? (e.Data.GetData(typeof(string)) as string)
            ?? string.Empty;

        TargetTextBox.Text = text;
    }

    private void DumpFormats(IDataObject data, string caption)
    {
        try
        {
            var formats = data.GetFormats();
            _log.WriteLine($"{caption}: ({formats.Length})");
            foreach (var f in formats.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                bool present;
                try { present = data.GetDataPresent(f); }
                catch { present = false; }

                _log.WriteLine($"  - {f} (present={present})");
            }
        }
        catch (Exception ex)
        {
            _log.WriteLine($"DumpFormats error: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private void Clear_Click(object sender, RoutedEventArgs e) => _log.Clear();
}
