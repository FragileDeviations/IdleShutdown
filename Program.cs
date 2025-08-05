using IdleShutdown.Windows;

namespace IdleShutdown;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new Window());
    }
}