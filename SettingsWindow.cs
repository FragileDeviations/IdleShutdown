namespace IdleShutdown;

public partial class SettingsWindow : Form
{
    private SettingsFile _settingsFile;

    public SettingsWindow()
    {
        InitializeComponent();
        
        var startHourButton = new Button
        {
            Text = "Start Hour",
            Location = new Point(10, 10)
        };
        startHourButton.Click += (sender, args) =>
        {
            var input = Microsoft.VisualBasic.Interaction.InputBox("Enter Start Hour (0-23):", "Start Hour",
                _settingsFile.StartHour.ToString());
            if (int.TryParse(input, out int startHour) && startHour >= 0 && startHour <= 23)
            {
                _settingsFile.StartHour = startHour;
                MessageBox.Show($"Start Hour set to {startHour}");
                Window.SaveSettings();
            }
            else
            {
                MessageBox.Show("Invalid input. Please enter a number between 0 and 23.");
            }
        };
        var endHourButton = new Button
        {
            Text = "End Hour",
            Location = new Point(10, 50)
        };
        endHourButton.Click += (sender, args) =>
        {
            var input = Microsoft.VisualBasic.Interaction.InputBox("Enter End Hour (0-23):", "End Hour",
                _settingsFile.EndHour.ToString());
            if (int.TryParse(input, out int endHour) && endHour >= 0 && endHour <= 23)
            {
                _settingsFile.EndHour = endHour;
                MessageBox.Show($"End Hour set to {endHour}");
                Window.SaveSettings();
            }
            else
            {
                MessageBox.Show("Invalid input. Please enter a number between 0 and 23.");
            }
        };
        var idleCheckIntervalButton = new Button
        {
            Text = "Idle Check Interval (seconds)",
            Location = new Point(10, 90)
        };
        idleCheckIntervalButton.Click += (sender, args) =>
        {
            var input = Microsoft.VisualBasic.Interaction.InputBox("Enter Idle Check Interval (seconds):",
                "Idle Check Interval", _settingsFile.IdleCheckInterval.ToString());
            if (int.TryParse(input, out int interval) && interval > 0)
            {
                _settingsFile.IdleCheckInterval = interval;
                MessageBox.Show($"Idle Check Interval set to {interval} seconds");
                Window.SaveSettings();
            }
        };
        var idleTimeThresholdButton = new Button
        {
            Text = "Idle Time Threshold (seconds)",
            Location = new Point(10, 130)
        };
        idleTimeThresholdButton.Click += (sender, args) =>
        {
            var input = Microsoft.VisualBasic.Interaction.InputBox("Enter Idle Time Threshold (seconds):",
                "Idle Time Threshold", _settingsFile.IdleTimeThreshold.ToString());
            if (int.TryParse(input, out int threshold) && threshold > 0)
            {
                _settingsFile.IdleTimeThreshold = threshold;
                MessageBox.Show($"Idle Time Threshold set to {threshold} seconds");
                Window.SaveSettings();
            }
        };
        var keepAliveProcessesButton = new Button
        {
            Text = "Manage Keep Alive Processes",
            Location = new Point(10, 170)
        };
        keepAliveProcessesButton.Click += (sender, args) =>
        {
            var input = Microsoft.VisualBasic.Interaction.InputBox("Enter Keep Alive Processes (comma separated):",
                "Keep Alive Processes", string.Join(", ", _settingsFile.KeepAliveProcesses));
            var processes = input.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim()).ToList();
            _settingsFile.KeepAliveProcesses = processes;
            MessageBox.Show($"Keep Alive Processes set to: {string.Join(", ", processes)}");
            Window.SaveSettings();
        };
        
        Controls.Add(startHourButton);
        Controls.Add(endHourButton);
        Controls.Add(idleCheckIntervalButton);
        Controls.Add(idleTimeThresholdButton);
        Controls.Add(keepAliveProcessesButton);
        Text = "Idle Shutdown Settings";
        Size = new Size(300, 300);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowIcon = false;
        ShowInTaskbar = false;
        ControlBox = true;
    }

    public void Setup(SettingsFile settingsFile)
    {
        _settingsFile = settingsFile;
    }
}