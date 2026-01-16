using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using LabCommon;

namespace WindowsFormsHostProbe;

public partial class MainWindow : Window
{
    private readonly LogBuffer _log = new();
    private readonly System.Windows.Forms.Panel _panel = new();
    private readonly System.Windows.Forms.Button _wfButton = new();
    private bool _toggle;

    public MainWindow()
    {
        InitializeComponent();

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
            SetupWinForms();
        };
    }

    private void SetupWinForms()
    {
        _panel.Dock = System.Windows.Forms.DockStyle.Fill;
        _panel.BackColor = Color.White;

        _wfButton.Text = "WinForms Button";
        _wfButton.AutoSize = true;
        _wfButton.Location = new System.Drawing.Point(10, 10);
        _wfButton.Click += (_, _) => _log.WriteLine("WinForms button clicked");

        _panel.Controls.Add(_wfButton);
        Host.Child = _panel;

        _log.WriteLine("WindowsFormsHost initialized.");
    }

    private void ToggleColor_Click(object sender, RoutedEventArgs e)
    {
        _toggle = !_toggle;
        _panel.BackColor = _toggle ? Color.FromArgb(30, 144, 255) : Color.White;
        _wfButton.ForeColor = _toggle ? Color.White : Color.Black;
        _log.WriteLine($"Toggled BackColor: {_panel.BackColor}");
    }

    private void Clear_Click(object sender, RoutedEventArgs e) => _log.Clear();
}
