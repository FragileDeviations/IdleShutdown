using System.Runtime.InteropServices;

namespace IdleShutdown;

[StructLayout(LayoutKind.Sequential)]
struct LASTINPUTINFO
{
    public uint cbSize;
    public uint dwTime;
}

internal static class IdleDetector
{
    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    public static TimeSpan GetIdleTime()
    {
        var info = new LASTINPUTINFO();
        info.cbSize = (uint)Marshal.SizeOf(info);

        if (!GetLastInputInfo(ref info))
            throw new Exception("GetLastInputInfo failed.");

        var idleTime = (uint)Environment.TickCount - info.dwTime;
        return TimeSpan.FromMilliseconds(idleTime);
    }
}