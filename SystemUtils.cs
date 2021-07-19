using System;
using System.Diagnostics;

namespace WinUtilities {
    /// <summary>Some utilities used to perform system actions</summary>
    public static class SystemUtils {

        /// <summary>Go to windows lock screen</summary>
        public static bool Lock() => WinAPI.LockWorkStation();

        /// <summary>Enable/disable current internet connection</summary>
        public static void Internet(bool state) {
            StartHidden(@"C:\Windows\System32\ipconfig.exe", state ? "/renew" : "/release");
        }

        /// <summary>Shutdown the computer</summary>
        /// <param name="delaySeconds">Delay until activation in seconds</param>
        public static void Shutdown(int delaySeconds = 1) {
            string arguments = "/s" + (delaySeconds > 0 ? $" /t {delaySeconds}" : "");
            StartHidden(@"shutdown.exe", arguments);
        }

        /// <summary>Restart the computer</summary>
        /// <param name="delaySeconds">Delay until activation in seconds</param>
        public static void Restart(int delaySeconds = 1) {
            string arguments = "/r" + (delaySeconds > 0 ? $" /t {delaySeconds}" : "");
            StartHidden(@"shutdown.exe", arguments);
        }

        /// <summary>Put the computer to sleep</summary>
        /// <param name="delaySeconds">Delay until activation in seconds</param>
        public static void Sleep(int delaySeconds = 1) {
            string arguments = "/h" + (delaySeconds > 0 ? $" /t {delaySeconds}" : "");
            StartHidden(@"shutdown.exe", arguments);
        }

        /// <summary>Stops Shutdown, Restart or Sleep started from this process</summary>
        public static void StopShutdown() {
            StartHidden(@"shutdown.exe", "/a");
        }

        private static void StartHidden(string filename, string arguments = null, string directory = null) {
            Process p = new Process();
            p.StartInfo.FileName = filename;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            if (!string.IsNullOrEmpty(arguments))
                p.StartInfo.Arguments = arguments;
            if (!string.IsNullOrEmpty(directory))
                p.StartInfo.WorkingDirectory = directory;
            p.Start();
        }
    }
}
