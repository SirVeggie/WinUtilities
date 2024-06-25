using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinUtilities {
    /// <summary>Some utilities used to perform system actions</summary>
    public static class SystemUtils {

        [DllImport("Powrprof.dll")]
        private static extern uint SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);

        /// <summary>Go to windows lock screen</summary>
        public static bool Lock() => WinAPI.LockWorkStation();

        /// <summary>Enable/disable current internet connection</summary>
        public static void Internet(bool state) {
            StartHidden(@"C:\Windows\System32\ipconfig.exe", state ? "/renew" : "/release");
        }

        /// <summary>Shutdown the computer</summary>
        /// <param name="delaySeconds">Delay until activation in seconds</param>
        public static void Shutdown(int delaySeconds = 0) {
            string arguments = $"-s -t {delaySeconds} -f";
            StartHidden(@"shutdown.exe", arguments);
        }

        /// <summary>Restart the computer</summary>
        /// <param name="delaySeconds">Delay until activation in seconds</param>
        public static void Restart(int delaySeconds = 1) {
            string arguments = $"-r -t {delaySeconds} -f";
            StartHidden(@"shutdown.exe", arguments);
        }

        /// <summary>Put the computer to sleep</summary>
        public static void Sleep() {
            SetSuspendState(false, true, false);
        }
        
        /// <summary>Restart the computer and boot straight to BIOS</summary>
        /// <param name="delaySeconds">Delay until activation in seconds</param>
        public static void RestartToBios(int delaySeconds = 1) {
            string arguments = $"-fw -r -t {delaySeconds}";
            StartHidden(@"shutdown.exe", arguments);
        }

        /// <summary>Stops Shutdown, Restart or Sleep started from this process</summary>
        public static void StopShutdown() {
            StartHidden(@"shutdown.exe", "-a");
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

        /// <summary>Get the window that is currently using the clipboard</summary>
        public static Window GetClipboardOwner() => new Window(WinAPI.GetOpenClipboardWindow());

        /// <summary>Close the clipboard</summary>
        public static void CloseClipboard() => WinAPI.CloseClipboard();
    }
}
