using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Automation;
using LabCommon;

namespace UiAutomationTreeProbe;

public partial class MainWindow : Window
{
    private readonly LogBuffer _log = new();

    public MainWindow()
    {
        InitializeComponent();

        Grid.ItemsSource = new[]
        {
            new Row("Alpha", "1"),
            new Row("Beta", "2"),
            new Row("Gamma", "3"),
        };

        _log.Changed += () =>
        {
            Dispatcher.Invoke(() =>
            {
                LogTextBox.Text = _log.Snapshot();
                LogTextBox.ScrollToEnd();
            });
        };

        Loaded += (_, _) =>
        {
            _log.WriteLine($"FrameworkDescription: {RuntimeInformation.FrameworkDescription}");
            _log.WriteLine("Tip: Compare this project between net6.0-windows and net8.0-windows by changing TargetFramework in the csproj.");
        };
    }

    private void Dump_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var handle = new WindowInteropHelper(this).Handle;
            if (handle == IntPtr.Zero)
            {
                _log.WriteLine("Window handle is zero. Try again after the window is fully shown.");
                return;
            }

            var root = AutomationElement.FromHandle(handle);
            if (root == null)
            {
                _log.WriteLine("AutomationElement.FromHandle returned null.");
                return;
            }

            _log.WriteLine("=== UIA tree dump begin ===");
            var maxDepth = (LimitDepth.IsChecked == true) ? 6 : 64;
            var onlyInteresting = OnlyInteresting.IsChecked == true;
            DumpElement(root, 0, maxDepth, onlyInteresting);
            _log.WriteLine("=== UIA tree dump end ===");
        }
        catch (Exception ex)
        {
            _log.WriteLine($"Dump error: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private void DumpElement(AutomationElement el, int depth, int maxDepth, bool onlyInteresting)
    {
        if (depth > maxDepth)
            return;

        var indent = new string(' ', depth * 2);

        string name = Safe(() => el.Current.Name) ?? "";
        string aid = Safe(() => el.Current.AutomationId) ?? "";
        string className = Safe(() => el.Current.ClassName) ?? "";
        string controlType = Safe(() => el.Current.ControlType?.ProgrammaticName) ?? "";
        bool? isOffscreenNullable = Safe(() => el.Current.IsOffscreen);
        bool isOffscreen = isOffscreenNullable.HasValue ? isOffscreenNullable.Value : false;

        if (!onlyInteresting || IsInteresting(el))
        {
            _log.WriteLine($"{indent}- {controlType} name=\"{name}\" aid=\"{aid}\" class=\"{className}\" offscreen={isOffscreen}");
        }

        if (depth == maxDepth)
            return;

        AutomationElementCollection children;
        try
        {
            children = el.FindAll(TreeScope.Children, System.Windows.Automation.Condition.TrueCondition);
        }
        catch
        {
            return;
        }

        foreach (AutomationElement child in children)
        {
            DumpElement(child, depth + 1, maxDepth, onlyInteresting);
        }
    }

    private static T? Safe<T>(Func<T> get)
    {
        try { return get(); }
        catch { return default; }
    }

    private static bool IsInteresting(AutomationElement el)
    {
        // Keep noise down: show elements that likely matter for UI tests
        try
        {
            var ct = el.Current.ControlType;
            if (ct == ControlType.Window
                || ct == ControlType.Button
                || ct == ControlType.Edit
                || ct == ControlType.Document
                || ct == ControlType.List
                || ct == ControlType.ListItem
                || ct == ControlType.ComboBox
                || ct == ControlType.DataGrid
                || ct == ControlType.DataItem
                || ct == ControlType.Menu
                || ct == ControlType.MenuItem
                || ct == ControlType.Tab
                || ct == ControlType.TabItem)
                return true;

            if (!string.IsNullOrEmpty(el.Current.AutomationId))
                return true;

            if (!string.IsNullOrEmpty(el.Current.Name))
                return true;
        }
        catch
        {
        }

        return false;
    }

    private void Clear_Click(object sender, RoutedEventArgs e) => _log.Clear();

    private sealed record Row(string Name, string Value);
}
