# IdleShutdown
Shuts down your computer after a period of inactivity only during inactive hours.
## Configuration
* StartHour - The starting hour of your inactive hours (0-23).
* EndHour - The ending hour of your inactive hours (0-23).
* IdleTime - The time threshold for determining inactivity (in seconds).
* IdleCheck - The interval at which to check for inactivity (in seconds).
* KeepAliveProcesses - A list of processes (by name, not case-sensitive) that prevents the shutdown if running.