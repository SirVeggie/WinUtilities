using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace WinUtilities {
    /// <summary>Class for subscribing to mouse and keyboard events</summary>
    public static class DeviceHook {

        /// <summary>Receiving keyboard events</summary>
        public static bool KeyboardHookRunning { get; private set; }
        /// <summary>Receiving mouse events</summary>
        public static bool MouseHookRunning { get; private set; }

        private static WinAPI.MessageProc keyboardProc = KeyboardCallback;
        private static WinAPI.MessageProc mouseProc = MouseCallback;

        private static IntPtr KeyboardHookID = IntPtr.Zero;
        private static IntPtr MouseHookID = IntPtr.Zero;

        private static IntPtr BlockCode => new IntPtr(-1);

        /// <summary>Specify if hooking keyboard is allowed</summary>
        public static bool KeyboardHookEnabled { get; set; } = true;
        /// <summary>Specify if hooking mouse is allowed</summary>
        public static bool MouseHookEnabled { get; set; } = true;
        /// <summary>Subscribe to all hook events</summary>
        public static event Func<IDeviceInput, bool> InputEvent;

        /// <summary>Start hooking device events</summary>
        public static void StartHooks() {
            StartKeyboardHook();
            StartMouseHook();
        }

        /// <summary>Stop hooking device events</summary>
        public static void StopHooks() {
            StopKeyboardHook();
            StopMouseHook();
        }

        /// <summary>Restart device hooks</summary>
        public static async Task RestartHooks() {
            StopHooks();
            await Task.Delay(1000);
            StartHooks();
        }

        /// <summary>Start hooking keyboard</summary>
        public static void StartKeyboardHook() {
            if (KeyboardHookEnabled && !KeyboardHookRunning) {
                if (InputEvent == null)
                    throw new Exception($"Trying to start keyboard hook while {nameof(InputEvent)} isn't set");
                KeyboardHookID = SetKeyboardHook(keyboardProc);
                KeyboardHookRunning = true;
            }
        }

        /// <summary>Stop hooking keyboard</summary>
        public static void StopKeyboardHook() {
            if (KeyboardHookRunning) {
                WinAPI.UnhookWindowsHookEx(KeyboardHookID);
                KeyboardHookRunning = false;
            }
        }

        /// <summary>Start hooking mouse</summary>
        public static void StartMouseHook() {
            if (MouseHookEnabled && !MouseHookRunning) {
                if (InputEvent == null)
                    throw new Exception($"Trying to start mouse hook while {nameof(InputEvent)} isn't set");
                MouseHookID = SetMouseHook(mouseProc);
                MouseHookRunning = true;
            }
        }

        /// <summary>Stop hooking mouse</summary>
        public static void StopMouseHook() {
            if (MouseHookRunning) {
                WinAPI.UnhookWindowsHookEx(MouseHookID);
                MouseHookRunning = false;
            }
        }

        #region private
        private static IntPtr KeyboardCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0) {
                var input = new KeyboardInput(wParam, lParam);

                if (InputEvent(input)) {
                    return BlockCode;
                }
            }

            return WinAPI.CallNextHookEx(KeyboardHookID, nCode, wParam, lParam);
        }

        private static IntPtr MouseCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0) {
                var input = new MouseInput(wParam, lParam);

                if (InputEvent(input)) {
                    return BlockCode;
                }
            }

            return WinAPI.CallNextHookEx(MouseHookID, nCode, wParam, lParam);
        }

        private static IntPtr SetKeyboardHook(WinAPI.MessageProc proc) {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule) {
                return WinAPI.SetWindowsHookEx(WinAPI.WH.Keyboard_LL, proc, curModule.BaseAddress, 0);
            }
        }

        private static IntPtr SetMouseHook(WinAPI.MessageProc proc) {
            using (Process process = Process.GetCurrentProcess())
            using (ProcessModule module = process.MainModule) {
                return WinAPI.SetWindowsHookEx(WinAPI.WH.Mouse_LL, proc, module.BaseAddress, 0);
            }
        }
        #endregion
    }

    #region input objects
    /// <summary>Interface that represents keyboard or mouse input</summary>
    public interface IDeviceInput {
        /// <summary>The inputted key</summary>
        Key Key { get; }
        /// <summary>The inputted key's state</summary>
        bool State { get; }
        /// <summary>Specifies if the key was emitted by a process</summary>
        bool Injected { get; }
        /// <summary>Specifies if the key was emitted by a lower integrity level process</summary>
        bool InjectedLower { get; }
        /// <summary>Extra information given by the event source</summary>
        UIntPtr ExtraInfo { get; }
        /// <summary>Specifies if the event is a mouse key</summary>
        bool IsMouse { get; }
    }

    /// <summary>Represents a keyboard input event</summary>
    public class KeyboardInput : IDeviceInput {

        /// <summary>The inputted key</summary>
        public Key Key { get; }
        /// <summary>The inputted key's scan code</summary>
        public ScanCode SC { get; }
        /// <summary>The inputted key's state</summary>
        public bool State { get; }

        /// <summary>Documentation <a href="https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-syskeydown">here</a></summary>
        public bool SysKey { get; }
        /// <summary>Specifies if the key has the extended property</summary>
        public bool Extended { get; }
        /// <summary>Specifies if the key was emitted by a process</summary>
        public bool Injected { get; }
        /// <summary>Specifies if the key was emitted by a lower integrity level process</summary>
        public bool InjectedLower { get; }
        /// <summary>Specifies if an alt key was down while emitting event</summary>
        public bool AltDown { get; }
        /// <summary>Specifies the key was released</summary>
        public bool Release { get; }
        /// <summary>Extra information given by the event source</summary>
        public UIntPtr ExtraInfo { get; }
        /// <summary>Specifies if the event was a mouse key. Always false.</summary>
        public bool IsMouse { get; } = false;

        /// <summary>Parse a new keyboard event from a hooked windows message</summary>
        public KeyboardInput(IntPtr wParam, IntPtr lParam) {
            var raw = (WinAPI.KBDLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof(WinAPI.KBDLLHOOKSTRUCT));

            SC = (ScanCode) raw.scanCode;

            WM state = (WM) wParam;
            State = state == WM.KEYDOWN || state == WM.SYSKEYDOWN;
            SysKey = state == WM.SYSKEYDOWN || state == WM.SYSKEYUP;

            Extended = raw.flags.HasFlag(WinAPI.KbdllFlags.Extended);
            Injected = raw.flags.HasFlag(WinAPI.KbdllFlags.Injected);
            InjectedLower = raw.flags.HasFlag(WinAPI.KbdllFlags.InjectedLower);
            AltDown = raw.flags.HasFlag(WinAPI.KbdllFlags.AltDown);
            Release = raw.flags.HasFlag(WinAPI.KbdllFlags.Release);

            ExtraInfo = raw.dwExtraInfo;

            Key = ((VKey) raw.vkCode).AsCustom(Extended);
        }

        /// <summary>Return the object as a string that shows the main information</summary>
        public override string ToString() {
            return "{Key: " + Key + " SC: " + SC + " State: " + State + " Injected: " + Injected + " Extended: "
                    + Extended + " Alt: " + AltDown + " Syskey: " + SysKey + " Extra: " + ExtraInfo + "}";
        }
    }

    /// <summary>Represents a mouse input event</summary>
    public class MouseInput : IDeviceInput {
        /// <summary>The inputted key</summary>
        public Key Key { get; }
        /// <summary>The inputted key's state</summary>
        public bool State { get; }

        /// <summary>The screen location of where the mouse event occurred</summary>
        public Coord Pos { get; }
        /// <summary>Specifies if the key was emitted by a process</summary>
        public bool Injected { get; }
        /// <summary>Specifies if the key was emitted by a lower integrity level process</summary>
        public bool InjectedLower { get; }
        /// <summary>Extra information given by the event source</summary>
        public UIntPtr ExtraInfo { get; }
        /// <summary>Specifies if the event was a mouse key. Always true.</summary>
        public bool IsMouse { get; } = true;

        /// <summary>Parse a new mouse event from a hooked windows message</summary>
        public MouseInput(IntPtr wParam, IntPtr lParam) {
            var raw = (WinAPI.MSLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof(WinAPI.MSLLHOOKSTRUCT));

            var res = Input.FromMouseEvent((WM) wParam, raw.mouseData);

            Key = res.Key;
            State = res.State;
            Pos = raw.pt;
            Injected = raw.flags.HasFlag(WinAPI.MsllFlags.Injected);
            InjectedLower = raw.flags.HasFlag(WinAPI.MsllFlags.InjectedLower);
            ExtraInfo = raw.dwExtraInfo;
        }

        /// <summary>Return the object as a string that shows the main information</summary>
        public override string ToString() {
            string action = Key == Key.MouseMove ? "Event: MouseMove" : "VK: " + Key;
            return "{" + action + " State: " + State + " Point: " + Pos.X + ", " + Pos.Y
                + " Scroll: " + Key.IsScroll() + " Injected: " + Injected + " Extra: " + ExtraInfo + "}";
        }
    }
    #endregion
}
