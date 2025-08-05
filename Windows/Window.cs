using System.Diagnostics;
using Newtonsoft.Json;

namespace IdleShutdown.Windows;

public partial class Window : Form
{
    private readonly System.Timers.Timer _mainTimer = new();
    private readonly System.Timers.Timer _shutdownTimer = new();
    private string _settingsFilePath;
    private static SettingsFile _settingsFile;
    private NotifyIcon _trayIcon;
    
    public Window()
    {
        InitializeComponent();
        LoadSettings();
        InitializeTrayIcon();
        StartMainTimer();
    }
    
    #region TrayIcon
    
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        ShowInTaskbar = false;
        WindowState = FormWindowState.Minimized;
        Visible = false;
        Hide();
    }
    
    private void InitializeTrayIcon()
    {
        _trayIcon = new NotifyIcon
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
            Visible = true,
            Text = "Idle Shutdown",
            ContextMenuStrip = new ContextMenuStrip()
        };
        
        InitializeSettingsMenu();
        _trayIcon.ContextMenuStrip.Items.Add("Exit", null, (_, __) => Application.Exit());
    }

    private void InitializeSettingsMenu()
    {
        var settingsMenuItem = new ToolStripMenuItem("Settings");
        settingsMenuItem.Click += (sender, args) =>
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.Setup(_settingsFile);
            settingsWindow.ShowDialog();
        };
        _trayIcon.ContextMenuStrip.Items.Add(settingsMenuItem);
    }
    
    #endregion
    
    
    #region Main Timer
    
    private void StartMainTimer()
    {
        _mainTimer.Interval = 60000;
        _mainTimer.Elapsed += MainTimerElapsed;
        _mainTimer.Start();
    }
    
    private void MainTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        var currentTime = DateTime.Now;
        if (currentTime.Hour >= _settingsFile.StartHour && currentTime.Hour < _settingsFile.EndHour)
        {
            if (!_shutdownTimer.Enabled)
            {
                StartShutdownTimer();
            }
        }
        else
        {
            if (_shutdownTimer.Enabled)
            {
                StopShutdownTimer();
            }
        }
        _mainTimer.Elapsed -= MainTimerElapsed;
        StartMainTimer();
    }
    
    private void StopMainTimer()
    {
        _mainTimer.Stop();
        _mainTimer.Elapsed -= MainTimerElapsed;
    }
    
    #endregion
    
    #region Shutdown Timer
    
    private void StartShutdownTimer()
    {
        _shutdownTimer.Interval = _settingsFile.IdleCheckInterval * 1000;
        _shutdownTimer.Elapsed += ShutdownTimerElapsed;
        _shutdownTimer.Start();
    }
    
    private void ShutdownTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (IsIdle() && !KeepAliveProcessesRunning())
        {
            // shutdown with 5 minute warning
            Process.Start(new ProcessStartInfo
            {
                FileName = "shutdown",
                Arguments = "/s /t 300 /c \"System will shut down in 5 minutes due to inactivity.\"",
                CreateNoWindow = true,
                UseShellExecute = false
            });
            return;
        }
        StartShutdownTimer();
    }
    
    private void StopShutdownTimer()
    {
        _shutdownTimer.Stop();
        _shutdownTimer.Elapsed -= ShutdownTimerElapsed;
    }
    
    #endregion
    
    #region Checks

    private bool IsIdle()
    {
        var idleTime = IdleDetector.GetIdleTime();
        return idleTime.TotalSeconds >= _settingsFile.IdleTimeThreshold;
    }
    
    private bool KeepAliveProcessesRunning()
    {
        var processes = Process.GetProcesses();
        foreach (var process in processes)
        {
            if (!_settingsFile.KeepAliveProcesses.Contains(process.ProcessName, StringComparer.OrdinalIgnoreCase))
                continue;
            return true;
        }
        return false;
    }
    
    #endregion
    
    #region Settings
    
    private void LoadSettings()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var basePath = Path.Combine(appDataPath, "Mabel Amber");
        var applicationPath = Path.Combine(basePath, "IdleShutdown");
        _settingsFilePath = Path.Combine(applicationPath, "settings.json");
        if (!File.Exists(_settingsFilePath))
        {
            CreateSettingsFile();
        }
        var json = File.ReadAllText(_settingsFilePath);
        _settingsFile = JsonConvert.DeserializeObject<SettingsFile>(json);
    }

    public static void SaveSettings()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var basePath = Path.Combine(path, "Mabel Amber");
        var applicationPath = Path.Combine(basePath, "IdleShutdown");
        var settingsFilePath = Path.Combine(applicationPath, "settings.json");
        var json = JsonConvert.SerializeObject(_settingsFile, Formatting.Indented);
        File.WriteAllText(settingsFilePath, json);
    }
    
    private void CreateSettingsFile()
    {
        _settingsFile = new SettingsFile();
        var json = JsonConvert.SerializeObject(_settingsFile, Formatting.Indented);
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var basePath = Path.Combine(appDataPath, "Mabel Amber");
        var applicationPath = Path.Combine(basePath, "IdleShutdown");
        Directory.CreateDirectory(basePath);
        Directory.CreateDirectory(applicationPath);
        File.WriteAllText(_settingsFilePath, json);
    }
    
    #endregion
}