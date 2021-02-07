using System;
using System.Diagnostics;

namespace WinUtilities {
    /// <summary>Some utilities used to perform system actions</summary>
    public static class SystemUtils {

        /// <summary>Go to windows lock screen</summary>
        public static bool Lock() => WinAPI.LockWorkStation();

        /// <summary>Enable/disable current internet connection</summary>
        public static void Internet(bool state) {
            Process p = new Process();
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.FileName = @"C:\Windows\System32\ipconfig.exe";
            p.StartInfo.Arguments = state ? "/renew" : "/release";
            p.Start();
        }

        /// <summary>Shutdown the computer</summary>
        /// <param name="delay">Delay until activation in seconds</param>
        public static void Shutdown(int delay = 0) {
            throw new NotImplementedException();
        }

        /// <summary>Restart the computer</summary>
        /// <param name="delay">Delay until activation in seconds</param>
        public static void Restart(int delay = 0) {
            throw new NotImplementedException();
        }

        /// <summary>Put the computer to sleep</summary>
        /// <param name="delay">Delay until activation in seconds</param>
        public static void Sleep(int delay = 0) {
            throw new NotImplementedException();
        }

        /// <summary>Stops Shutdown, Restart or Sleep started from this process</summary>
        public static void StopShutdown() {
            throw new NotImplementedException();
        }
    }
}
