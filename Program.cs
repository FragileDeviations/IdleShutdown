using IdleShutdown.Windows;

namespace IdleShutdown;

internal static class Program
{
    public const string Version = "1.0.1";
    public const string Changelog = "1.0.1 - Fixed glaringly obvious memory leak.\n" +
        "1.0.0 - Initial release.";
    
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new Window());
    }
}