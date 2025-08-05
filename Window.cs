using System.Diagnostics;
using Newtonsoft.Json;

namespace IdleShutdown;

public partial class Window : Form
{
    private readonly System.Timers.Timer _mainTimer = new();
    private readonly System.Timers.Timer _shutdownTimer = new();
    private string _settingsFilePath;
    private static SettingsFile _settingsFile;
    private NotifyIcon _trayIcon;
    private string _logFilePath;
    
    public Window()
    {
        InitializeComponent();
        InitLogFile();
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
            Log("Opening settings window.");
            var settingsWindow = new SettingsWindow();
            settingsWindow.Setup(_settingsFile);
            settingsWindow.ShowDialog();
            Log("Settings window closed.");
        };
        _trayIcon.ContextMenuStrip.Items.Add(settingsMenuItem);
    }
    
    #endregion
    
    
    #region Logging

    private void InitLogFile()
    {
        _logFilePath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath) ?? string.Empty, "IdleShutdown.log");
        if (File.Exists(_logFilePath))
        {
            File.Delete(_logFilePath);
        }
        File.Create(_logFilePath).Close();
    }
    
    private void Log(string message)
    {
        if (string.IsNullOrEmpty(_logFilePath))
        {
            InitLogFile();
        }

        if (_logFilePath == null) return;
        using var writer = new StreamWriter(_logFilePath, true);
        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
        writer.Flush();
        writer.Close();
    }
    
    #endregion
    
    #region Main Timer
    
    private void StartMainTimer()
    {
        Log("Starting main timer.");
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
                Log("Starting shutdown timer due to inactive hours.");
                StartShutdownTimer();
            }
        }
        else
        {
            if (_shutdownTimer.Enabled)
            {
                Log("Stopping shutdown timer due to active hours.");
                StopShutdownTimer();
            }
        }
        _mainTimer.Elapsed -= MainTimerElapsed;
        StartMainTimer();
    }
    
    private void StopMainTimer()
    {
        Log("Stopping main timer.");
        _mainTimer.Stop();
        _mainTimer.Elapsed -= MainTimerElapsed;
    }
    
    #endregion
    
    #region Shutdown Timer
    
    private void StartShutdownTimer()
    {
        Log("Starting shutdown timer.");
        _shutdownTimer.Interval = _settingsFile.IdleCheckInterval * 1000;
        _shutdownTimer.Elapsed += ShutdownTimerElapsed;
        _shutdownTimer.Start();
    }
    
    private void ShutdownTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (IsIdle() && !KeepAliveProcessesRunning())
        {
            Log("System is idle and no keep-alive processes are running. Initiating shutdown.");
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
        Log("Stopping shutdown timer.");
        _shutdownTimer.Stop();
        _shutdownTimer.Elapsed -= ShutdownTimerElapsed;
    }
    
    #endregion
    
    #region Checks

    private bool IsIdle()
    {
        var idleTime = IdleDetector.GetIdleTime();
        Log($"Idle time detected: {idleTime.TotalSeconds} seconds.");
        return idleTime.TotalSeconds >= _settingsFile.IdleTimeThreshold;
    }
    
    private bool KeepAliveProcessesRunning()
    {
        var processes = Process.GetProcesses();
        foreach (var process in processes)
        {
            if (!_settingsFile.KeepAliveProcesses.Contains(process.ProcessName, StringComparer.OrdinalIgnoreCase))
                continue;
            Log($"Keep-alive process detected: {process.ProcessName} (PID: {process.Id}).");
            return true;
        }
        Log("No keep-alive processes detected.");
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
        Log($"Settings loaded: Threshold: {_settingsFile.IdleTimeThreshold} seconds, Check Interval: {_settingsFile.IdleCheckInterval} seconds, Start Hour: {_settingsFile.StartHour}, End Hour: {_settingsFile.EndHour}, Keep Alive Processes: {string.Join(", ", _settingsFile.KeepAliveProcesses)}");
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
        Log("Creating default settings file.");
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