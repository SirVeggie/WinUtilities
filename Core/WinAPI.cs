using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace WinUtilities {
    /// <summary>Access to a variety of native Windows API calls, structures, enums and some custom macros</summary>
    public class WinAPI {
        #region delegates
        public delegate bool EnumWindowsDelegate(IntPtr hwnd, int lParam);
        public delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);
        public delegate IntPtr MessageProc(int nCode, IntPtr wParam, IntPtr lParam);
        #endregion

        #region imports
        /// <summary>Flashes the specified window one time. It does not change the active state of the window. To flash the window a specified number of times, use the <see cref="FlashWindowEx(FLASHWINFO)"/> function.</summary>
        /// <param name="hwnd">Window handle of the target window</param>
        /// <param name="bInvert">If this parameter is TRUE, the window is flashed from one state to the other. If it is FALSE, the window is returned to its original state (either active or inactive).</param>
        [DllImport("user32.dll")]
        public static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

        [DllImport("user32.dll")]
        public static extern bool FlashWindowEx(FLASHWINFO data);

        [DllImport("user32.dll")]
        public static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll")]
        public static extern bool DestroyCursor(IntPtr hCursor);

        [DllImport("user32.dll")]
        public static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO piconinfo);

        [DllImport("user32.dll")]
        public static extern IntPtr CreateIconIndirect([In] ref ICONINFO piconinfo);

        /// <summary><a href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-systemparametersinfoa"></a></summary>
        [DllImport("user32.dll")]
        public static extern bool SystemParametersInfo(SPI action, uint uiParam, string pvParam, SPIF fWinIni);

        /// <summary><a href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-systemparametersinfoa"></a></summary>
        [DllImport("user32.dll")]
        public static extern bool SystemParametersInfo(SPI action, uint uiParam, uint pvParam, SPIF fWinIni);

        /// <summary>Changes the parent window of the specified child window</summary>
        /// <param name="hwndChild">A handle to the child window</param>
        /// <param name="hwndParent">A handle to the new parent window. If this parameter is NULL, the desktop window becomes the new parent window. If this parameter is HWND_MESSAGE, the child window becomes a message-only window.</param>
        /// <remarks><a href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setparent"></a></remarks>
        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hwndChild, IntPtr hwndParent);

        [DllImport("user32.dll")]
        public static extern bool IsHungAppWindow(IntPtr hwnd);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        public static extern bool BringWindowToTop(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        public static extern int RegisterWindowMessage(string messageName);

        [DllImport("user32.dll")]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint nCode, KeyMapFlags flag);

        [DllImport("dwmapi.dll")]
        public static extern int DwmIsCompositionEnabled(out bool enabled);

        [DllImport("dwmapi.dll", PreserveSig = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

        [DllImport("user32.dll")]
        public static extern uint GetClassLong(IntPtr hwnd, ClassLongFlags flags);

        [DllImport("user32.dll")]
        public static extern uint SetClassLong(IntPtr hwnd, ClassLongFlags flags, uint newValue);

        [DllImport("user32.dll")]
        public static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, bool clientOnly);

        [DllImport("gdi32.dll")]
        public static extern int BitBlt(IntPtr hDestDC, int x, int y, int width, int height, IntPtr hSrcDC, int xSrc, int ySrc, TernaryRasterOperations op);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int width, int height);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern int DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] bool nativePath, [Out] StringBuilder lpExeName, ref int lpdwSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, uint processId);

        [DllImport("kernel32.dll")]
        public static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32.dll")]
        public static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateToolhelp32Snapshot(SnapshotFlags dwFlags, uint th32ProcessID);

        [DllImport("user32.dll")]
        public static extern int GetScrollInfo(IntPtr hwnd, ScrollInfoType bar, ref SCROLLINFO info);

        [DllImport("user32.dll")]
        public static extern int SetScrollInfo(IntPtr hwnd, ScrollInfoType bar, [In] ref SCROLLINFO info, bool redraw);

        [DllImport("user32.dll")]
        public static extern bool HideCaret(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern RegionType GetWindowRgn(IntPtr hwnd, IntPtr hRgn);

        [DllImport("user32.dll")]
        public static extern int SetWindowRgn(IntPtr hwnd, IntPtr hRgn, bool bRedraw);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreatePolygonRgn(POINT[] lppt, int cPoints, FillRgnFlags fnPolyFillMode);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateRoundRectRgn(int left, int top, int right, int bottom, int roundWidth, int roundHeight);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateEllipticRgn(int left, int top, int right, int bottom);

        [DllImport("gdi32.dll")]
        public static extern RegionType CombineRgn(IntPtr hrgnDest, IntPtr hrgnSrc1, IntPtr hrgnSrc2, CombineRgnFlags fnCombineMode);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        [DllImport("gdi32.dll")]
        public static extern int GetRegionData(IntPtr hRgn, int dwCount, IntPtr lpRgnData);

        [DllImport("user32.dll")]
        public static extern RegionType GetWindowRgnBox(IntPtr hwnd, out RECT area);

        [DllImport("user32.dll")]
        public static extern void keybd_event(VKey vk, ScanCode sc, KEYEVENTF flags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern void mouse_event(MOUSEEVENTF dwFlags, int dx, int dy, int dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterShellHookWindow(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(WH idHook, MessageProc lpfn, IntPtr hMod, int dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern short GetKeyState(VKey code);

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(VKey code);

        [DllImport("user32.dll", SetLastError = false)]
        public static extern IntPtr GetMessageExtraInfo();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool LockWorkStation();

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(SM smIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hwnd, StringBuilder title, int size);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowTextLength(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetClassName(IntPtr hwnd, StringBuilder className, int size);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hwnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hwnd, ref RECT rect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetClientRect(IntPtr hwnd, ref RECT rect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ClientToScreen(IntPtr hwnd, ref POINT point);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hwnd, SW cmd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hwnd, WM msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostMessage(IntPtr hwnd, WM msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        public static extern IntPtr GetWindowLongPtr(IntPtr hwnd, WindowLongFlags flag);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        public static extern IntPtr SetWindowLongPtr(IntPtr hwnd, WindowLongFlags flag, IntPtr newValue);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hwnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsZoomed(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsterAfter, int x, int y, int w, int h, WindowPosFlags flags);

        [DllImport("user32.dll")]
        public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, LayeredWindowFlags flags);

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(POINT p);

        [DllImport("user32.dll")]
        public static extern IntPtr GetAncestor(IntPtr hwnd, AncestorFlags flags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnableWindow(IntPtr hwnd, bool state);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowEnabled(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT point);

        [DllImport("user32.dll")]
        public static extern bool GetCursorInfo(ref CURSORINFO info);

        [DllImport("user32.dll")]
        public static extern bool ClipCursor(ref RECT area);

        /// <summary>Only pass IntPtr.Zero as an argument.</summary>
        [DllImport("user32.dll")]
        public static extern bool ClipCursor(IntPtr zero);

        [DllImport("user32.dll")]
        public static extern int ShowCursor(bool state);

        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursor(IntPtr hInstance, CursorType type);

        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursorFromFile(string file);

        [DllImport("user32.dll")]
        public static extern IntPtr CopyIcon(IntPtr hIcon);

        [DllImport("user32.dll")]
        public static extern bool GetClipCursor(out RECT area);

        [DllImport("user32.dll")]
        public static extern bool SetSystemCursor(IntPtr hReplacement, CursorType target);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumMonitorsDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumWindows(EnumWindowsDelegate enumCallback, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumWindowsDelegate enumCallback, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool EnumThreadWindows(int threadID, EnumWindowsDelegate enumCallback, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr MonitorFromPoint(POINT pt, MonitorDefault dwFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromRect([In] ref RECT lprc, MonitorDefault dwFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, MonitorDefault dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern DisplayReturn ChangeDisplaySettingsEx(string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, DisplayFlags dwflags, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        public static extern IntPtr GetOpenClipboardWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr CloseClipboard();

        [DllImport("user32.dll")]
        public static extern int SystemParametersInfo(uint action, uint uParam, string vParam, uint winIni);
        #endregion

        #region macros
        /// <summary>Macro that maps a scan code to a virtual key by <see cref="KeyMapFlags.ScanCode_to_VirtualKeyEx"/></summary>
        public static VKey MapVirtualKey(ScanCode sc) => (VKey)MapVirtualKey((uint)sc, KeyMapFlags.ScanCode_to_VirtualKeyEx);
        /// <summary>Macro that maps a virtual key to a scan code by <see cref="KeyMapFlags.VirtualKey_to_ScanCode"/></summary>
        public static ScanCode MapVirtualKey(VKey vk) => (ScanCode)MapVirtualKey((uint)vk, KeyMapFlags.VirtualKey_to_ScanCode);
        /// <summary>Macro that maps a scan code to a char by <see cref="KeyMapFlags.ScanCode_to_Char"/></summary>
        public static char MapVirtualKeyChar(ScanCode sc) => (char)MapVirtualKey((uint)sc, KeyMapFlags.ScanCode_to_Char);

        /// <summary>Tries to retrieve Region data. Getting all rects doesn't work.</summary>
        public static RGNDATA? GetRegionDataManaged(IntPtr hRgn) {
            int datasize = GetRegionData(hRgn, 0, IntPtr.Zero);

            if (datasize == 0) {
                return null;
            }

            IntPtr pointer = Marshal.AllocCoTaskMem(datasize);
            int success = GetRegionData(hRgn, datasize, pointer);

            if (success == 0) {
                return null;
            }

            return Marshal.PtrToStructure<RGNDATA>(pointer);
        }

        public static IntPtr SendMessage<T>(IntPtr handle, WM message, IntPtr wParam, ref T lParam) => SendMessage(handle, (uint)message, wParam, ref lParam);
        public static IntPtr SendMessage<T>(IntPtr handle, uint message, IntPtr wParam, ref T lParam) {
            IntPtr res;
            var size = Marshal.SizeOf(lParam);
            IntPtr pointer = Marshal.AllocCoTaskMem(size);

            try {
                Marshal.StructureToPtr(lParam, pointer, false);
                res = SendMessage(handle, message, wParam, pointer);
                lParam = Marshal.PtrToStructure<T>(pointer);
            } finally {
                Marshal.FreeCoTaskMem(pointer);
            }

            return res;
        }

        /// <summary>Get process id from a window handle</summary>
        public static uint GetPidFromHwnd(IntPtr hwnd) {
            GetWindowThreadProcessId(hwnd, out uint pid);
            return pid;
        }

        /// <summary>Get class name from a window handle</summary>
        public static string GetClassFromHwnd(IntPtr hwnd) {
            StringBuilder className = new StringBuilder(256);
            GetClassName(hwnd, className, 256);
            return className.ToString();
        }

        /// <summary>Get exe path from a window handle</summary>
        public static string GetPathFromPid(uint pid) {
            var handle = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, pid);
            string res = null;

            if (handle != IntPtr.Zero) {
                try {
                    int size = 1024;
                    StringBuilder b = new StringBuilder(size);

                    if (QueryFullProcessImageName(handle, false, b, ref size)) {
                        res = b.ToString();
                    }
                } finally {
                    CloseHandle(handle);
                }
            }

            return res;
        }

        /// <summary>Get the name of the exe file from it's full path</summary>
        public static string GetExeNameFromPath(string path) {
            var s = path?.Split('\\').Last().Split('.');
            if (s == null) return "";
            return string.Join(".", s.Take(s.Length - 1));
        }

        /// <summary>Get the name of the exe file from a process id</summary>
        public static string GetExeNameFromPid(uint pid) => GetExeNameFromPath(GetPathFromPid(pid));


        /// <summary><a href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-systemparametersinfoa"></a></summary>
        [DllImport("user32.dll")]
        public static extern bool SystemParametersInfo(SPI action, uint uiParam, RECT pvParam, SPIF fWinIni);
        /// <summary><a href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-systemparametersinfoa"></a></summary>
        [DllImport("user32.dll")]
        public static extern bool SystemParametersInfo(SPI action, uint uiParam, out uint pvParam, SPIF fWinIni);
        /// <summary><a href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-systemparametersinfoa"></a></summary>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SystemParametersInfo(SPI action, int uiParam, ref NONCLIENTMETRICS pvParam, SPIF fWinIni);

        public static bool SetWorkArea(RECT rect) => SystemParametersInfo(SPI.SPI_SETWORKAREA, 0, rect, SPIF.None);
        public static bool ResetSystemCursors() => SystemParametersInfo(SPI.SPI_SETCURSORS, 0, 0, SPIF.None);

        public static bool SetForegroundLockTimeout(int timeout) {
            throw new NotImplementedException();
            return SystemParametersInfo(SPI.SPI_SETFOREGROUNDLOCKTIMEOUT, 0, (uint)timeout, SPIF.None);
        }

        public static int GetForegroundLockTimeout() {
            throw new NotImplementedException();
            if (SystemParametersInfo(SPI.SPI_SETFOREGROUNDLOCKTIMEOUT, 0, out uint time, SPIF.None))
                return (int)time;
            return -1;
        }

        /// <summary>Value 0 causes flashing to last forever. Value 7 (default) is the maximum. Requires Windows reboot to take effect.</summary>
        public static bool SetFlashCount(int count) {
            return SystemParametersInfo(SPI.SPI_SETFOREGROUNDFLASHCOUNT, 0, (uint)count, SPIF.SPIF_SENDCHANGE | SPIF.SPIF_UPDATEINIFILE);
        }

        public static int GetFlashCount() {
            if (SystemParametersInfo(SPI.SPI_GETFOREGROUNDFLASHCOUNT, 0, out uint count, SPIF.None))
                return (int)count;
            return -1;
        }

        public static int GetBorderPadding() {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop\WindowMetrics", true)) {
                var value = int.Parse((string)key.GetValue("PaddedBorderWidth"));
                return value / -15;
            }
        }

        public static int GetBorder() {
            throw new NotImplementedException();
            var metrics = new NONCLIENTMETRICS();
            var size = Marshal.SizeOf(metrics);
            metrics.cbSize = size;
            if (!SystemParametersInfo(SPI.SPI_GETNONCLIENTMETRICS, size, ref metrics, SPIF.None))
                Console.WriteLine(Marshal.GetLastWin32Error());
            return metrics.iBorderWidth;
        }

        public static bool SetWallpaper(Image image, WallpaperStyle style) {
            bool success = false;
            try {
                string temp = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");
                image.Save(temp, ImageFormat.Bmp);
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
                int tile = style == WallpaperStyle.Tile ? 1 : 0;
                int wstyle = 0;

                switch (style) {
                    case WallpaperStyle.Stretch:
                        wstyle = 2;
                        break;
                    case WallpaperStyle.Fill:
                        wstyle = 10;
                        break;
                    case WallpaperStyle.Fit:
                        wstyle = 6;
                        break;
                    default:
                        wstyle = 0;
                        break;
                }

                SystemParametersInfo(SPI.SPI_SETDESKWALLPAPER, 0, temp, SPIF.SPIF_UPDATEINIFILE | SPIF.SPIF_SENDCHANGE);
                success = true;
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
            return success;
        }
        #endregion

        #region enums
        /// <summary>Window flashing flags</summary>
        [Flags]
        public enum FlashWF : uint {
            /// <summary>Stop flashing. The system restores the window to its original state</summary>    
            STOP = 0,
            /// <summary>Flash the window caption</summary>
            CAPTION = 1,
            /// <summary>Flash the taskbar button</summary>
            TRAY = 2,
            /// <summary>Flash both the window caption and taskbar button. This is equivalent to setting the CAPTION | TRAY flags.</summary>
            ALL = 3,
            /// <summary>Flash continuously, until the STOP flag is set</summary>
            TIMER = 4,
            /// <summary>Flash continuously until the window comes to the foreground</summary>
            TIMERNOFG = 12
        }

        public enum ShellHook {
            Activate = 4,
            Create = 1,
            Destroy = 2,
            Flash = 32774,
            RudeActivate = 32772,
            Redraw = 6,
            MonitorChanged = 16
        }

        public enum KeyMapFlags : uint {
            /// <summary>The uCode parameter is a virtual-key code and is translated into a scan code. If it is a virtual-key code that does not distinguish between left- and right-hand keys, the left-hand scan code is returned. If there is no translation, the function returns 0.</summary>
            VirtualKey_to_ScanCode = 0,
            /// <summary>The uCode parameter is a scan code and is translated into a virtual-key code that does not distinguish between left- and right-hand keys. If there is no translation, the function returns 0.</summary>
            ScanCode_to_VirtualKey = 1,
            /// <summary>The uCode parameter is a virtual-key code and is translated into an unshifted character value in the low order word of the return value. Dead keys (diacritics) are indicated by setting the top bit of the return value. If there is no translation, the function returns 0.</summary>
            ScanCode_to_Char = 2,
            /// <summary>The uCode parameter is a scan code and is translated into a virtual-key code that distinguishes between left- and right-hand keys. If there is no translation, the function returns 0.</summary>
            ScanCode_to_VirtualKeyEx = 3
        }

        [Flags]
        public enum ClassStyles : uint {
            /// <summary>Aligns the window's client area on a byte boundary (in the x direction). This style affects the width of the window and its horizontal placement on the display.</summary>
            ByteAlignClient = 0x1000,

            /// <summary>Aligns the window on a byte boundary (in the x direction). This style affects the width of the window and its horizontal placement on the display.</summary>
            ByteAlignWindow = 0x2000,

            /// <summary>
            /// Allocates one device context to be shared by all windows in the class.
            /// Because window classes are process specific, it is possible for multiple threads of an application to create a window of the same class.
            /// It is also possible for the threads to attempt to use the device context simultaneously. When this happens, the system allows only one thread to successfully finish its drawing operation.
            /// </summary>
            ClassDC = 0x40,

            /// <summary>Sends a double-click message to the window procedure when the user double-clicks the mouse while the cursor is within a window belonging to the class.</summary>
            DoubleClicks = 0x8,

            /// <summary>
            /// Enables the drop shadow effect on a window. The effect is turned on and off through SPI_SETDROPSHADOW.
            /// Typically, this is enabled for small, short-lived windows such as menus to emphasize their Z order relationship to other windows.
            /// </summary>
            DropShadow = 0x20000,

            /// <summary>Indicates that the window class is an application global class. For more information, see the "Application Global Classes" section of About Window Classes.</summary>
            GlobalClass = 0x4000,

            /// <summary>Redraws the entire window if a movement or size adjustment changes the width of the client area.</summary>
            HorizontalRedraw = 0x2,

            /// <summary>Disables Close on the window menu.</summary>
            NoClose = 0x200,

            /// <summary>Allocates a unique device context for each window in the class.</summary>
            OwnDC = 0x20,

            /// <summary>
            /// Sets the clipping rectangle of the child window to that of the parent window so that the child can draw on the parent.
            /// A window with the CS_PARENTDC style bit receives a regular device context from the system's cache of device contexts.
            /// It does not give the child the parent's device context or device context settings. Specifying CS_PARENTDC enhances an application's performance.
            /// </summary>
            ParentDC = 0x80,

            /// <summary>
            /// Saves, as a bitmap, the portion of the screen image obscured by a window of this class.
            /// When the window is removed, the system uses the saved bitmap to restore the screen image, including other windows that were obscured.
            /// Therefore, the system does not send WM_PAINT messages to windows that were obscured if the memory used by the bitmap has not been discarded and if other screen actions have not invalidated the stored image.
            /// This style is useful for small windows (for example, menus or dialog boxes) that are displayed briefly and then removed before other screen activity takes place.
            /// This style increases the time required to display the window, because the system must first allocate memory to store the bitmap.
            /// </summary>
            SaveBits = 0x800,

            /// <summary>Redraws the entire window if a movement or size adjustment changes the height of the client area.</summary>
            VerticalRedraw = 0x1
        }

        /// <summary>Enum of Windows Hooks</summary>
        public enum WH : int {
            CallWndProc = 4,
            CallWndProcRet = 12,
            Cbt = 5,
            Debug = 9,
            ForegroundIdle = 11,
            GetMessage = 3,
            JournalPlayback = 1,
            JournalRecord = 0,
            Keyboard = 2,
            Keyboard_LL = 13,
            Mouse = 7,
            Mouse_LL = 14,
            MsgFilter = -1,
            Shell = 10,
            SysMsgFilter = 6
        }

        /// <summary>Enum of System Metrics</summary>
        public enum SM : int {
            /// <summary>
            /// The flags that specify how the system arranged minimized windows. For more information, see the Remarks section in this topic.
            /// </summary>
            ARRANGE = 56,

            /// <summary>
            /// The value that specifies how the system is started:
            /// 0 Normal boot
            /// 1 Fail-safe boot
            /// 2 Fail-safe with network boot
            /// A fail-safe boot (also called SafeBoot, Safe Mode, or Clean Boot) bypasses the user startup files.
            /// </summary>
            CLEANBOOT = 67,

            /// <summary>
            /// The number of display monitors on a desktop. For more information, see the Remarks section in this topic.
            /// </summary>
            CMONITORS = 80,

            /// <summary>
            /// The number of buttons on a mouse, or zero if no mouse is installed.
            /// </summary>
            CMOUSEBUTTONS = 43,

            /// <summary>
            /// The width of a window border, in pixels. This is equivalent to the CXEDGE value for windows with the 3-D look.
            /// </summary>
            CXBORDER = 5,

            /// <summary>
            /// The width of a cursor, in pixels. The system cannot create cursors of other sizes.
            /// </summary>
            CXCURSOR = 13,

            /// <summary>
            /// This value is the same as CXFIXEDFRAME.
            /// </summary>
            CXDLGFRAME = 7,

            /// <summary>
            /// The width of the rectangle around the location of a first click in a double-click sequence, in pixels. ,
            /// The second click must occur within the rectangle that is defined by CXDOUBLECLK and CYDOUBLECLK for the system
            /// to consider the two clicks a double-click. The two clicks must also occur within a specified time.
            /// To set the width of the double-click rectangle, call SystemParametersInfo with SPI_SETDOUBLECLKWIDTH.
            /// </summary>
            CXDOUBLECLK = 36,

            /// <summary>
            /// The number of pixels on either side of a mouse-down point that the mouse pointer can move before a drag operation begins.
            /// This allows the user to click and release the mouse button easily without unintentionally starting a drag operation.
            /// If this value is negative, it is subtracted from the left of the mouse-down point and added to the right of it.
            /// </summary>
            CXDRAG = 68,

            /// <summary>
            /// The width of a 3-D border, in pixels. This metric is the 3-D counterpart of CXBORDER.
            /// </summary>
            CXEDGE = 45,

            /// <summary>
            /// The thickness of the frame around the perimeter of a window that has a caption but is not sizable, in pixels.
            /// CXFIXEDFRAME is the height of the horizontal border, and CYFIXEDFRAME is the width of the vertical border.
            /// This value is the same as CXDLGFRAME.
            /// </summary>
            CXFIXEDFRAME = 7,

            /// <summary>
            /// The width of the left and right edges of the focus rectangle that the DrawFocusRectdraws.
            /// This value is in pixels.
            /// Windows 2000:  This value is not supported.
            /// </summary>
            CXFOCUSBORDER = 83,

            /// <summary>
            /// This value is the same as CXSIZEFRAME.
            /// </summary>
            CXFRAME = 32,

            /// <summary>
            /// The width of the client area for a full-screen window on the primary display monitor, in pixels.
            /// To get the coordinates of the portion of the screen that is not obscured by the system taskbar or by application desktop toolbars,
            /// call the SystemParametersInfofunction with the SPI_GETWORKAREA value.
            /// </summary>
            CXFULLSCREEN = 16,

            /// <summary>
            /// The width of the arrow bitmap on a horizontal scroll bar, in pixels.
            /// </summary>
            CXHSCROLL = 21,

            /// <summary>
            /// The width of the thumb box in a horizontal scroll bar, in pixels.
            /// </summary>
            CXHTHUMB = 10,

            /// <summary>
            /// The default width of an icon, in pixels. The LoadIcon function can load only icons with the dimensions
            /// that CXICON and CYICON specifies.
            /// </summary>
            CXICON = 11,

            /// <summary>
            /// The width of a grid cell for items in large icon view, in pixels. Each item fits into a rectangle of size
            /// CXICONSPACING by CYICONSPACING when arranged. This value is always greater than or equal to CXICON.
            /// </summary>
            CXICONSPACING = 38,

            /// <summary>
            /// The default width, in pixels, of a maximized top-level window on the primary display monitor.
            /// </summary>
            CXMAXIMIZED = 61,

            /// <summary>
            /// The default maximum width of a window that has a caption and sizing borders, in pixels.
            /// This metric refers to the entire desktop. The user cannot drag the window frame to a size larger than these dimensions.
            /// A window can override this value by processing the WM_GETMINMAXINFO message.
            /// </summary>
            CXMAXTRACK = 59,

            /// <summary>
            /// The width of the default menu check-mark bitmap, in pixels.
            /// </summary>
            CXMENUCHECK = 71,

            /// <summary>
            /// The width of menu bar buttons, such as the child window close button that is used in the multiple document interface, in pixels.
            /// </summary>
            CXMENUSIZE = 54,

            /// <summary>
            /// The minimum width of a window, in pixels.
            /// </summary>
            CXMIN = 28,

            /// <summary>
            /// The width of a minimized window, in pixels.
            /// </summary>
            CXMINIMIZED = 57,

            /// <summary>
            /// The width of a grid cell for a minimized window, in pixels. Each minimized window fits into a rectangle this size when arranged.
            /// This value is always greater than or equal to CXMINIMIZED.
            /// </summary>
            CXMINSPACING = 47,

            /// <summary>
            /// The minimum tracking width of a window, in pixels. The user cannot drag the window frame to a size smaller than these dimensions.
            /// A window can override this value by processing the WM_GETMINMAXINFO message.
            /// </summary>
            CXMINTRACK = 34,

            /// <summary>
            /// The amount of border padding for captioned windows, in pixels. Windows XP/2000:  This value is not supported.
            /// </summary>
            CXPADDEDBORDER = 92,

            /// <summary>
            /// The width of the screen of the primary display monitor, in pixels. This is the same value obtained by calling 
            /// GetDeviceCaps as follows: GetDeviceCaps( hdcPrimaryMonitor, HORZRES).
            /// </summary>
            CXSCREEN = 0,

            /// <summary>
            /// The width of a button in a window caption or title bar, in pixels.
            /// </summary>
            CXSIZE = 30,

            /// <summary>
            /// The thickness of the sizing border around the perimeter of a window that can be resized, in pixels.
            /// CXSIZEFRAME is the width of the horizontal border, and CYSIZEFRAME is the height of the vertical border.
            /// This value is the same as CXFRAME.
            /// </summary>
            CXSIZEFRAME = 32,

            /// <summary>
            /// The recommended width of a small icon, in pixels. Small icons typically appear in window captions and in small icon view.
            /// </summary>
            CXSMICON = 49,

            /// <summary>
            /// The width of small caption buttons, in pixels.
            /// </summary>
            CXSMSIZE = 52,

            /// <summary>
            /// The width of the virtual screen, in pixels. The virtual screen is the bounding rectangle of all display monitors.
            /// The XVIRTUALSCREEN metric is the coordinates for the left side of the virtual screen.
            /// </summary>
            CXVIRTUALSCREEN = 78,

            /// <summary>
            /// The width of a vertical scroll bar, in pixels.
            /// </summary>
            CXVSCROLL = 2,

            /// <summary>
            /// The height of a window border, in pixels. This is equivalent to the CYEDGE value for windows with the 3-D look.
            /// </summary>
            CYBORDER = 6,

            /// <summary>
            /// The height of a caption area, in pixels.
            /// </summary>
            CYCAPTION = 4,

            /// <summary>
            /// The height of a cursor, in pixels. The system cannot create cursors of other sizes.
            /// </summary>
            CYCURSOR = 14,

            /// <summary>
            /// This value is the same as CYFIXEDFRAME.
            /// </summary>
            CYDLGFRAME = 8,

            /// <summary>
            /// The height of the rectangle around the location of a first click in a double-click sequence, in pixels.
            /// The second click must occur within the rectangle defined by CXDOUBLECLK and CYDOUBLECLK for the system to consider
            /// the two clicks a double-click. The two clicks must also occur within a specified time. To set the height of the double-click
            /// rectangle, call SystemParametersInfo with SPI_SETDOUBLECLKHEIGHT.
            /// </summary>
            CYDOUBLECLK = 37,

            /// <summary>
            /// The number of pixels above and below a mouse-down point that the mouse pointer can move before a drag operation begins.
            /// This allows the user to click and release the mouse button easily without unintentionally starting a drag operation.
            /// If this value is negative, it is subtracted from above the mouse-down point and added below it.
            /// </summary>
            CYDRAG = 69,

            /// <summary>
            /// The height of a 3-D border, in pixels. This is the 3-D counterpart of CYBORDER.
            /// </summary>
            CYEDGE = 46,

            /// <summary>
            /// The thickness of the frame around the perimeter of a window that has a caption but is not sizable, in pixels.
            /// CXFIXEDFRAME is the height of the horizontal border, and CYFIXEDFRAME is the width of the vertical border.
            /// This value is the same as CYDLGFRAME.
            /// </summary>
            CYFIXEDFRAME = 8,

            /// <summary>
            /// The height of the top and bottom edges of the focus rectangle drawn byDrawFocusRect.
            /// This value is in pixels.
            /// Windows 2000:  This value is not supported.
            /// </summary>
            CYFOCUSBORDER = 84,

            /// <summary>
            /// This value is the same as CYSIZEFRAME.
            /// </summary>
            CYFRAME = 33,

            /// <summary>
            /// The height of the client area for a full-screen window on the primary display monitor, in pixels.
            /// To get the coordinates of the portion of the screen not obscured by the system taskbar or by application desktop toolbars,
            /// call the SystemParametersInfo function with the SPI_GETWORKAREA value.
            /// </summary>
            CYFULLSCREEN = 17,

            /// <summary>
            /// The height of a horizontal scroll bar, in pixels.
            /// </summary>
            CYHSCROLL = 3,

            /// <summary>
            /// The default height of an icon, in pixels. The LoadIcon function can load only icons with the dimensions CXICON and CYICON.
            /// </summary>
            CYICON = 12,

            /// <summary>
            /// The height of a grid cell for items in large icon view, in pixels. Each item fits into a rectangle of size
            /// CXICONSPACING by CYICONSPACING when arranged. This value is always greater than or equal to CYICON.
            /// </summary>
            CYICONSPACING = 39,

            /// <summary>
            /// For double byte character set versions of the system, this is the height of the Kanji window at the bottom of the screen, in pixels.
            /// </summary>
            CYKANJIWINDOW = 18,

            /// <summary>
            /// The default height, in pixels, of a maximized top-level window on the primary display monitor.
            /// </summary>
            CYMAXIMIZED = 62,

            /// <summary>
            /// The default maximum height of a window that has a caption and sizing borders, in pixels. This metric refers to the entire desktop.
            /// The user cannot drag the window frame to a size larger than these dimensions. A window can override this value by processing
            /// the WM_GETMINMAXINFO message.
            /// </summary>
            CYMAXTRACK = 60,

            /// <summary>
            /// The height of a single-line menu bar, in pixels.
            /// </summary>
            CYMENU = 15,

            /// <summary>
            /// The height of the default menu check-mark bitmap, in pixels.
            /// </summary>
            CYMENUCHECK = 72,

            /// <summary>
            /// The height of menu bar buttons, such as the child window close button that is used in the multiple document interface, in pixels.
            /// </summary>
            CYMENUSIZE = 55,

            /// <summary>
            /// The minimum height of a window, in pixels.
            /// </summary>
            CYMIN = 29,

            /// <summary>
            /// The height of a minimized window, in pixels.
            /// </summary>
            CYMINIMIZED = 58,

            /// <summary>
            /// The height of a grid cell for a minimized window, in pixels. Each minimized window fits into a rectangle this size when arranged.
            /// This value is always greater than or equal to CYMINIMIZED.
            /// </summary>
            CYMINSPACING = 48,

            /// <summary>
            /// The minimum tracking height of a window, in pixels. The user cannot drag the window frame to a size smaller than these dimensions.
            /// A window can override this value by processing the WM_GETMINMAXINFO message.
            /// </summary>
            CYMINTRACK = 35,

            /// <summary>
            /// The height of the screen of the primary display monitor, in pixels. This is the same value obtained by calling 
            /// GetDeviceCaps as follows: GetDeviceCaps( hdcPrimaryMonitor, VERTRES).
            /// </summary>
            CYSCREEN = 1,

            /// <summary>
            /// The height of a button in a window caption or title bar, in pixels.
            /// </summary>
            CYSIZE = 31,

            /// <summary>
            /// The thickness of the sizing border around the perimeter of a window that can be resized, in pixels.
            /// CXSIZEFRAME is the width of the horizontal border, and CYSIZEFRAME is the height of the vertical border.
            /// This value is the same as CYFRAME.
            /// </summary>
            CYSIZEFRAME = 33,

            /// <summary>
            /// The height of a small caption, in pixels.
            /// </summary>
            CYSMCAPTION = 51,

            /// <summary>
            /// The recommended height of a small icon, in pixels. Small icons typically appear in window captions and in small icon view.
            /// </summary>
            CYSMICON = 50,

            /// <summary>
            /// The height of small caption buttons, in pixels.
            /// </summary>
            CYSMSIZE = 53,

            /// <summary>
            /// The height of the virtual screen, in pixels. The virtual screen is the bounding rectangle of all display monitors.
            /// The YVIRTUALSCREEN metric is the coordinates for the top of the virtual screen.
            /// </summary>
            CYVIRTUALSCREEN = 79,

            /// <summary>
            /// The height of the arrow bitmap on a vertical scroll bar, in pixels.
            /// </summary>
            CYVSCROLL = 20,

            /// <summary>
            /// The height of the thumb box in a vertical scroll bar, in pixels.
            /// </summary>
            CYVTHUMB = 9,

            /// <summary>
            /// Nonzero if User32.dll supports DBCS; otherwise, 0.
            /// </summary>
            DBCSENABLED = 42,

            /// <summary>
            /// Nonzero if the debug version of User.exe is installed; otherwise, 0.
            /// </summary>
            DEBUG = 22,

            /// <summary>
            /// Nonzero if the current operating system is Windows 7 or Windows Server 2008 R2 and the Tablet PC Input
            /// service is started; otherwise, 0. The return value is a bitmask that specifies the type of digitizer input supported by the device.
            /// For more information, see Remarks.
            /// Windows Server 2008, Windows Vista, and Windows XP/2000:  This value is not supported.
            /// </summary>
            DIGITIZER = 94,

            /// <summary>
            /// Nonzero if Input Method Manager/Input Method Editor features are enabled; otherwise, 0.
            /// IMMENABLED indicates whether the system is ready to use a Unicode-based IME on a Unicode application.
            /// To ensure that a language-dependent IME works, check DBCSENABLED and the system ANSI code page.
            /// Otherwise the ANSI-to-Unicode conversion may not be performed correctly, or some components like fonts
            /// or registry settings may not be present.
            /// </summary>
            IMMENABLED = 82,

            /// <summary>
            /// Nonzero if there are digitizers in the system; otherwise, 0. MAXIMUMTOUCHES returns the aggregate maximum of the
            /// maximum number of contacts supported by every digitizer in the system. If the system has only single-touch digitizers,
            /// the return value is 1. If the system has multi-touch digitizers, the return value is the number of simultaneous contacts
            /// the hardware can provide. Windows Server 2008, Windows Vista, and Windows XP/2000:  This value is not supported.
            /// </summary>
            MAXIMUMTOUCHES = 95,

            /// <summary>
            /// Nonzero if the current operating system is the Windows XP, Media Center Edition, 0 if not.
            /// </summary>
            MEDIACENTER = 87,

            /// <summary>
            /// Nonzero if drop-down menus are right-aligned with the corresponding menu-bar item; 0 if the menus are left-aligned.
            /// </summary>
            MENUDROPALIGNMENT = 40,

            /// <summary>
            /// Nonzero if the system is enabled for Hebrew and Arabic languages, 0 if not.
            /// </summary>
            MIDEASTENABLED = 74,

            /// <summary>
            /// Nonzero if a mouse is installed; otherwise, 0. This value is rarely zero, because of support for virtual mice and because
            /// some systems detect the presence of the port instead of the presence of a mouse.
            /// </summary>
            MOUSEPRESENT = 19,

            /// <summary>
            /// Nonzero if a mouse with a horizontal scroll wheel is installed; otherwise 0.
            /// </summary>
            MOUSEHORIZONTALWHEELPRESENT = 91,

            /// <summary>
            /// Nonzero if a mouse with a vertical scroll wheel is installed; otherwise 0.
            /// </summary>
            MOUSEWHEELPRESENT = 75,

            /// <summary>
            /// The least significant bit is set if a network is present; otherwise, it is cleared. The other bits are reserved for future use.
            /// </summary>
            NETWORK = 63,

            /// <summary>
            /// Nonzero if the Microsoft Windows for Pen computing extensions are installed; zero otherwise.
            /// </summary>
            PENWINDOWS = 41,

            /// <summary>
            /// This system metric is used in a Terminal Services environment to determine if the current Terminal Server session is
            /// being remotely controlled. Its value is nonzero if the current session is remotely controlled; otherwise, 0.
            /// You can use terminal services management tools such as Terminal Services Manager (tsadmin.msc) and shadow.exe to
            /// control a remote session. When a session is being remotely controlled, another user can view the contents of that session
            /// and potentially interact with it.
            /// </summary>
            REMOTECONTROL = 0x2001,

            /// <summary>
            /// This system metric is used in a Terminal Services environment. If the calling process is associated with a Terminal Services
            /// client session, the return value is nonzero. If the calling process is associated with the Terminal Services console session,
            /// the return value is 0.
            /// Windows Server 2003 and Windows XP:  The console session is not necessarily the physical console.
            /// For more information, seeWTSGetActiveConsoleSessionId.
            /// </summary>
            REMOTESESSION = 0x1000,

            /// <summary>
            /// Nonzero if all the display monitors have the same color format, otherwise, 0. Two displays can have the same bit depth,
            /// but different color formats. For example, the red, green, and blue pixels can be encoded with different numbers of bits,
            /// or those bits can be located in different places in a pixel color value.
            /// </summary>
            SAMEDISPLAYFORMAT = 81,

            /// <summary>
            /// This system metric should be ignored; it always returns 0.
            /// </summary>
            SECURE = 44,

            /// <summary>
            /// The build number if the system is Windows Server 2003 R2; otherwise, 0.
            /// </summary>
            SERVERR2 = 89,

            /// <summary>
            /// Nonzero if the user requires an application to present information visually in situations where it would otherwise present
            /// the information only in audible form; otherwise, 0.
            /// </summary>
            SHOWSOUNDS = 70,

            /// <summary>
            /// Nonzero if the current session is shutting down; otherwise, 0. Windows 2000:  This value is not supported.
            /// </summary>
            SHUTTINGDOWN = 0x2000,

            /// <summary>
            /// Nonzero if the computer has a low-end (slow) processor; otherwise, 0.
            /// </summary>
            SLOWMACHINE = 73,

            /// <summary>
            /// Nonzero if the current operating system is Windows 7 Starter Edition, Windows Vista Starter, or Windows XP Starter Edition; otherwise, 0.
            /// </summary>
            STARTER = 88,

            /// <summary>
            /// Nonzero if the meanings of the left and right mouse buttons are swapped; otherwise, 0.
            /// </summary>
            SWAPBUTTON = 23,

            /// <summary>
            /// Nonzero if the current operating system is the Windows XP Tablet PC edition or if the current operating system is Windows Vista
            /// or Windows 7 and the Tablet PC Input service is started; otherwise, 0. The DIGITIZER setting indicates the type of digitizer
            /// input supported by a device running Windows 7 or Windows Server 2008 R2. For more information, see Remarks.
            /// </summary>
            TABLETPC = 86,

            /// <summary>
            /// The coordinates for the left side of the virtual screen. The virtual screen is the bounding rectangle of all display monitors.
            /// The CXVIRTUALSCREEN metric is the width of the virtual screen.
            /// </summary>
            XVIRTUALSCREEN = 76,

            /// <summary>
            /// The coordinates for the top of the virtual screen. The virtual screen is the bounding rectangle of all display monitors.
            /// The CYVIRTUALSCREEN metric is the height of the virtual screen.
            /// </summary>
            YVIRTUALSCREEN = 77,
        }

        public enum SW {
            HIDE,
            SHOWNORMAL,
            SHOWMINIMIZED,
            MAXIMIZE,
            SHOWNOACTICATE,
            SHOW,
            MINIMIZE,
            SHOWMINNOACTIVATE,
            SHOWNA,
            RESTORE,
            SHOWDEFAULT,
            FORCEMINIMIZE
        }

        public enum RegionType : int {
            /// <summary>Region does not exist or an error occurred.</summary>
            Error = 0,
            /// <summary>Region is empty.</summary>
            Null = 1,
            /// <summary>Region consists of one rectangle.</summary>
            Simple = 2,
            /// <summary>Region is a complex shape.</summary>
            Complex = 3
        }

        public enum CombineRgnFlags : int {
            And = 1,
            Or = 2,
            Xor = 3,
            Diff = 4,
            Copy = 5,
            Min = And,
            Max = Copy
        }

        /// <summary>Documentation <a href="https://docs.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-setpolyfillmode">here</a></summary>
        public enum FillRgnFlags : int {
            Alternate = 1,
            Winding = 2
        }

        public enum WindowLongFlags : int {
            GWL_EXSTYLE = -20,
            GWLP_HINSTANCE = -6,
            GWLP_HWNDPARENT = -8,
            GWL_ID = -12,
            GWL_STYLE = -16,
            GWL_USERDATA = -21,
            GWL_WNDPROC = -4,

            DWLP_USER = 0x8,
            DWLP_MSGRESULT = 0x0,
            DWLP_DLGPROC = 0x4
        }

        public enum WindowPosFlags : uint {
            /// <summary>If the calling thread and the thread that owns the window are attached to different input queues,
            /// the system posts the request to the thread that owns the window. This prevents the calling thread from
            /// blocking its execution while other threads process the request.</summary>
            /// <remarks>SWP_ASYNCWINDOWPOS</remarks>
            AsyncWindowPos = 0x4000,
            /// <summary>Prevents generation of the WM_SYNCPAINT message.</summary>
            /// <remarks>SWP_DEFERERASE</remarks>
            DeferErase = 0x2000,
            /// <summary>Draws a frame (defined in the window's class description) around the window.</summary>
            /// <remarks>SWP_DRAWFRAME</remarks>
            DrawFrame = 0x0020,
            /// <summary>Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to
            /// the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE
            /// is sent only when the window's size is being changed.</summary>
            /// <remarks>SWP_FRAMECHANGED</remarks>
            FrameChanged = 0x0020,
            /// <summary>Hides the window.</summary>
            /// <remarks>SWP_HIDEWINDOW</remarks>
            HideWindow = 0x0080,
            /// <summary>Does not activate the window. If this flag is not set, the window is activated and moved to the
            /// top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter
            /// parameter).</summary>
            /// <remarks>SWP_NOACTIVATE</remarks>
            NoActivate = 0x0010,
            /// <summary>Discards the entire contents of the client area. If this flag is not specified, the valid
            /// contents of the client area are saved and copied back into the client area after the window is sized or
            /// repositioned.</summary>
            /// <remarks>SWP_NOCOPYBITS</remarks>
            NoCopyBits = 0x0100,
            /// <summary>Retains the current position (ignores X and Y parameters).</summary>
            /// <remarks>SWP_NOMOVE</remarks>
            NoMove = 0x0002,
            /// <summary>Does not change the owner window's position in the Z order.</summary>
            /// <remarks>SWP_NOOWNERZORDER</remarks>
            NoOwnerZOrder = 0x0200,
            /// <summary>Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to
            /// the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent
            /// window uncovered as a result of the window being moved. When this flag is set, the application must
            /// explicitly invalidate or redraw any parts of the window and parent window that need redrawing.</summary>
            /// <remarks>SWP_NOREDRAW</remarks>
            NoRedraw = 0x0008,
            /// <summary>Same as the SWP_NOOWNERZORDER flag.</summary>
            /// <remarks>SWP_NOREPOSITION</remarks>
            NoReposition = 0x0200,
            /// <summary>Prevents the window from receiving the WM_WINDOWPOSCHANGING message.</summary>
            /// <remarks>SWP_NOSENDCHANGING</remarks>
            NoSendChangingEvent = 0x0400,
            /// <summary>Retains the current size (ignores the cx and cy parameters).</summary>
            /// <remarks>SWP_NOSIZE</remarks>
            NoSize = 0x0001,
            /// <summary>Retains the current Z order (ignores the hWndInsertAfter parameter).</summary>
            /// <remarks>SWP_NOZORDER</remarks>
            NoZOrder = 0x0004,
            /// <summary>Displays the window.</summary>
            /// <remarks>SWP_SHOWWINDOW</remarks>
            ShowWindow = 0x0040,
        }

        public enum HWND_Z : int {
            TOP = 0,
            BOTTOM = 1,
            TOPMOST = -1,
            NOTOPMOST = -2
        }

        public enum LayeredWindowFlags : uint {
            LWA_COLORKEY = 0x1,
            LWA_ALPHA = 0x2
        }

        public enum AncestorFlags {
            /// <summary>
            /// Retrieves the parent window. This does not include the owner, as it does with the GetParent function.
            /// </summary>
            GetParent = 1,
            /// <summary>
            /// Retrieves the root window by walking the chain of parent windows.
            /// </summary>
            GetRoot = 2,
            /// <summary>
            /// Retrieves the owned root window by walking the chain of parent and owner windows returned by GetParent.
            /// </summary>
            GetRootOwner = 3
        }

        [Flags]
        public enum DisplayFlags : uint {
            CDS_NONE = 0,
            CDS_UPDATEREGISTRY = 0x00000001,
            CDS_TEST = 0x00000002,
            CDS_FULLSCREEN = 0x00000004,
            CDS_GLOBAL = 0x00000008,
            CDS_SET_PRIMARY = 0x00000010,
            CDS_VIDEOPARAMETERS = 0x00000020,
            CDS_ENABLE_UNSAFE_MODES = 0x00000100,
            CDS_DISABLE_UNSAFE_MODES = 0x00000200,
            CDS_RESET = 0x40000000,
            CDS_RESET_EX = 0x20000000,
            CDS_NORESET = 0x10000000
        }

        public enum DisplayReturn : int {
            Successful = 0,
            Restart = 1,
            Failed = -1,
            BadMode = -2,
            NotUpdated = -3,
            BadFlags = -4,
            BadParam = -5,
            BadDualView = -6
        }

        public enum InputType : uint {
            Mouse = 0,
            Keyboard = 1,
            Hardware = 2
        }

        [Flags]
        public enum MOUSEEVENTF : uint {
            ABSOLUTE = 0x8000,
            HWHEEL = 0x01000,
            MOVE = 0x0001,
            MOVE_NOCOALESCE = 0x2000,
            LEFTDOWN = 0x0002,
            LEFTUP = 0x0004,
            RIGHTDOWN = 0x0008,
            RIGHTUP = 0x0010,
            MIDDLEDOWN = 0x0020,
            MIDDLEUP = 0x0040,
            VIRTUALDESK = 0x4000,
            WHEEL = 0x0800,
            XDOWN = 0x0080,
            XUP = 0x0100
        }

        [Flags]
        public enum KEYEVENTF : uint {
            EXTENDEDKEY = 0x0001,
            KEYUP = 0x0002,
            SCANCODE = 0x0008,
            UNICODE = 0x0004
        }

        [Flags]
        public enum KbdllFlags : uint {
            Extended = 0x01,
            Injected = 0x10,
            InjectedLower = 0x02,
            AltDown = 0x20,
            Release = 0x80,
        }

        [Flags]
        public enum MsllFlags : uint {
            None,
            Injected,
            InjectedLower
        }

        public enum CursorState {
            Hidden,
            Showing,
            Suppressed
        }

        public enum ScrollInfoType {
            Horizontal = 0,
            Vertical = 1,
            Control = 2,
            Both = 3
        }

        public enum ScrollInfoMask : uint {
            Range = 1,
            Page = 2,
            Pos = 4,
            DisableNoScroll = 8,
            TrackPos = 16,
            All = Range | Page | Pos | TrackPos
        }

        [Flags]
        public enum SnapshotFlags : uint {
            HeapList = 0x00000001,
            Process = 0x00000002,
            Thread = 0x00000004,
            Module = 0x00000008,
            Module32 = 0x00000010,
            All = (HeapList | Process | Thread | Module),
            Inherit = 0x80000000,
            NoHeaps = 0x40000000

        }

        [Flags]
        public enum ProcessAccessFlags : uint {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        /// <summary>
        ///     Specifies a raster-operation code. These codes define how the color data for the
        ///     source rectangle is to be combined with the color data for the destination
        ///     rectangle to achieve the final color.
        /// </summary>
        public enum TernaryRasterOperations : uint {
            /// <summary>dest = source</summary>
            SRCCOPY = 0x00CC0020,
            /// <summary>dest = source OR dest</summary>
            SRCPAINT = 0x00EE0086,
            /// <summary>dest = source AND dest</summary>
            SRCAND = 0x008800C6,
            /// <summary>dest = source XOR dest</summary>
            SRCINVERT = 0x00660046,
            /// <summary>dest = source AND (NOT dest)</summary>
            SRCERASE = 0x00440328,
            /// <summary>dest = (NOT source)</summary>
            NOTSRCCOPY = 0x00330008,
            /// <summary>dest = (NOT src) AND (NOT dest)</summary>
            NOTSRCERASE = 0x001100A6,
            /// <summary>dest = (source AND pattern)</summary>
            MERGECOPY = 0x00C000CA,
            /// <summary>dest = (NOT source) OR dest</summary>
            MERGEPAINT = 0x00BB0226,
            /// <summary>dest = pattern</summary>
            PATCOPY = 0x00F00021,
            /// <summary>dest = DPSnoo</summary>
            PATPAINT = 0x00FB0A09,
            /// <summary>dest = pattern XOR dest</summary>
            PATINVERT = 0x005A0049,
            /// <summary>dest = (NOT dest)</summary>
            DSTINVERT = 0x00550009,
            /// <summary>dest = BLACK</summary>
            BLACKNESS = 0x00000042,
            /// <summary>dest = WHITE</summary>
            WHITENESS = 0x00FF0062,
            /// <summary>
            /// Capture window as seen on screen.  This includes layered windows
            /// such as WPF windows with AllowsTransparency="true"
            /// </summary>
            CAPTUREBLT = 0x40000000
        }

        public enum ClassLongFlags : int {
            GCLP_MENUNAME = -8,
            GCLP_HBRBACKGROUND = -10,
            GCLP_HCURSOR = -12,
            GCLP_HICON = -14,
            GCLP_HMODULE = -16,
            GCL_CBWNDEXTRA = -18,
            GCL_CBCLSEXTRA = -20,
            GCLP_WNDPROC = -24,
            GCL_STYLE = -26,
            GCLP_HICONSM = -34,
            GCW_ATOM = -32
        }

        public enum DWMWINDOWATTRIBUTE : uint {
            NCRenderingEnabled = 1,
            NCRenderingPolicy = 2,
            TransitionsForceDisabled = 3,
            AllowNCPaint = 4,
            CaptionButtonBounds = 5,
            NonClientRtlLayout = 6,
            ForceIconicRepresentation = 7,
            Flip3DPolicy = 8,
            ExtendedFrameBounds = 9,
            HasIconicBitmap = 10,
            DisallowPeek = 11,
            ExcludedFromPeek = 12,
            Cloak = 13,
            Cloaked = 14,
            FreezeRepresentation = 15
        }

        [Flags]
        public enum SPIF {
            None = 0x00,
            /// <summary>Writes the new system-wide parameter setting to the user profile.</summary>
            SPIF_UPDATEINIFILE = 0x01,
            /// <summary>Broadcasts the WM_SETTINGCHANGE message after updating the user profile.</summary>
            SPIF_SENDCHANGE = 0x02,
            /// <summary>Same as SPIF_SENDCHANGE.</summary>
            SPIF_SENDWININICHANGE = 0x02
        }

        /// <summary>SPI_ System-wide parameter - Used in SystemParametersInfo function</summary>
        public enum SPI : uint {
            /// <summary>
            /// Determines whether the warning beeper is on.
            /// The pvParam parameter must point to a BOOL variable that receives TRUE if the beeper is on, or FALSE if it is off.
            /// </summary>
            SPI_GETBEEP = 0x0001,

            /// <summary>
            /// Turns the warning beeper on or off. The uiParam parameter specifies TRUE for on, or FALSE for off.
            /// </summary>
            SPI_SETBEEP = 0x0002,

            /// <summary>
            /// Retrieves the two mouse threshold values and the mouse speed.
            /// </summary>
            SPI_GETMOUSE = 0x0003,

            /// <summary>
            /// Sets the two mouse threshold values and the mouse speed.
            /// </summary>
            SPI_SETMOUSE = 0x0004,

            /// <summary>
            /// Retrieves the border multiplier factor that determines the width of a window's sizing border.
            /// The pvParam parameter must point to an integer variable that receives this value.
            /// </summary>
            SPI_GETBORDER = 0x0005,

            /// <summary>
            /// Sets the border multiplier factor that determines the width of a window's sizing border.
            /// The uiParam parameter specifies the new value.
            /// </summary>
            SPI_SETBORDER = 0x0006,

            /// <summary>
            /// Retrieves the keyboard repeat-speed setting, which is a value in the range from 0 (approximately 2.5 repetitions per second)
            /// through 31 (approximately 30 repetitions per second). The actual repeat rates are hardware-dependent and may vary from
            /// a linear scale by as much as 20%. The pvParam parameter must point to a DWORD variable that receives the setting
            /// </summary>
            SPI_GETKEYBOARDSPEED = 0x000A,

            /// <summary>
            /// Sets the keyboard repeat-speed setting. The uiParam parameter must specify a value in the range from 0
            /// (approximately 2.5 repetitions per second) through 31 (approximately 30 repetitions per second).
            /// The actual repeat rates are hardware-dependent and may vary from a linear scale by as much as 20%.
            /// If uiParam is greater than 31, the parameter is set to 31.
            /// </summary>
            SPI_SETKEYBOARDSPEED = 0x000B,

            /// <summary>
            /// Not implemented.
            /// </summary>
            SPI_LANGDRIVER = 0x000C,

            /// <summary>
            /// Sets or retrieves the width, in pixels, of an icon cell. The system uses this rectangle to arrange icons in large icon view.
            /// To set this value, set uiParam to the new value and set pvParam to null. You cannot set this value to less than SM_CXICON.
            /// To retrieve this value, pvParam must point to an integer that receives the current value.
            /// </summary>
            SPI_ICONHORIZONTALSPACING = 0x000D,

            /// <summary>
            /// Retrieves the screen saver time-out value, in seconds. The pvParam parameter must point to an integer variable that receives the value.
            /// </summary>
            SPI_GETSCREENSAVETIMEOUT = 0x000E,

            /// <summary>
            /// Sets the screen saver time-out value to the value of the uiParam parameter. This value is the amount of time, in seconds,
            /// that the system must be idle before the screen saver activates.
            /// </summary>
            SPI_SETSCREENSAVETIMEOUT = 0x000F,

            /// <summary>
            /// Determines whether screen saving is enabled. The pvParam parameter must point to a bool variable that receives TRUE
            /// if screen saving is enabled, or FALSE otherwise.
            /// </summary>
            SPI_GETSCREENSAVEACTIVE = 0x0010,

            /// <summary>
            /// Sets the state of the screen saver. The uiParam parameter specifies TRUE to activate screen saving, or FALSE to deactivate it.
            /// </summary>
            SPI_SETSCREENSAVEACTIVE = 0x0011,

            /// <summary>
            /// Retrieves the current granularity value of the desktop sizing grid. The pvParam parameter must point to an integer variable
            /// that receives the granularity.
            /// </summary>
            SPI_GETGRIDGRANULARITY = 0x0012,

            /// <summary>
            /// Sets the granularity of the desktop sizing grid to the value of the uiParam parameter.
            /// </summary>
            SPI_SETGRIDGRANULARITY = 0x0013,

            /// <summary>
            /// Sets the desktop wallpaper. The value of the pvParam parameter determines the new wallpaper. To specify a wallpaper bitmap,
            /// set pvParam to point to a null-terminated string containing the name of a bitmap file. Setting pvParam to "" removes the wallpaper.
            /// Setting pvParam to SETWALLPAPER_DEFAULT or null reverts to the default wallpaper.
            /// </summary>
            SPI_SETDESKWALLPAPER = 0x0014,

            /// <summary>
            /// Sets the current desktop pattern by causing Windows to read the Pattern= setting from the WIN.INI file.
            /// </summary>
            SPI_SETDESKPATTERN = 0x0015,

            /// <summary>
            /// Retrieves the keyboard repeat-delay setting, which is a value in the range from 0 (approximately 250 ms delay) through 3
            /// (approximately 1 second delay). The actual delay associated with each value may vary depending on the hardware. The pvParam parameter must point to an integer variable that receives the setting.
            /// </summary>
            SPI_GETKEYBOARDDELAY = 0x0016,

            /// <summary>
            /// Sets the keyboard repeat-delay setting. The uiParam parameter must specify 0, 1, 2, or 3, where zero sets the shortest delay
            /// (approximately 250 ms) and 3 sets the longest delay (approximately 1 second). The actual delay associated with each value may
            /// vary depending on the hardware.
            /// </summary>
            SPI_SETKEYBOARDDELAY = 0x0017,

            /// <summary>
            /// Sets or retrieves the height, in pixels, of an icon cell.
            /// To set this value, set uiParam to the new value and set pvParam to null. You cannot set this value to less than SM_CYICON.
            /// To retrieve this value, pvParam must point to an integer that receives the current value.
            /// </summary>
            SPI_ICONVERTICALSPACING = 0x0018,

            /// <summary>
            /// Determines whether icon-title wrapping is enabled. The pvParam parameter must point to a bool variable that receives TRUE
            /// if enabled, or FALSE otherwise.
            /// </summary>
            SPI_GETICONTITLEWRAP = 0x0019,

            /// <summary>
            /// Turns icon-title wrapping on or off. The uiParam parameter specifies TRUE for on, or FALSE for off.
            /// </summary>
            SPI_SETICONTITLEWRAP = 0x001A,

            /// <summary>
            /// Determines whether pop-up menus are left-aligned or right-aligned, relative to the corresponding menu-bar item.
            /// The pvParam parameter must point to a bool variable that receives TRUE if left-aligned, or FALSE otherwise.
            /// </summary>
            SPI_GETMENUDROPALIGNMENT = 0x001B,

            /// <summary>
            /// Sets the alignment value of pop-up menus. The uiParam parameter specifies TRUE for right alignment, or FALSE for left alignment.
            /// </summary>
            SPI_SETMENUDROPALIGNMENT = 0x001C,

            /// <summary>
            /// Sets the width of the double-click rectangle to the value of the uiParam parameter.
            /// The double-click rectangle is the rectangle within which the second click of a double-click must fall for it to be registered
            /// as a double-click.
            /// To retrieve the width of the double-click rectangle, call GetSystemMetrics with the SM_CXDOUBLECLK flag.
            /// </summary>
            SPI_SETDOUBLECLKWIDTH = 0x001D,

            /// <summary>
            /// Sets the height of the double-click rectangle to the value of the uiParam parameter.
            /// The double-click rectangle is the rectangle within which the second click of a double-click must fall for it to be registered
            /// as a double-click.
            /// To retrieve the height of the double-click rectangle, call GetSystemMetrics with the SM_CYDOUBLECLK flag.
            /// </summary>
            SPI_SETDOUBLECLKHEIGHT = 0x001E,

            /// <summary>
            /// Retrieves the logical font information for the current icon-title font. The uiParam parameter specifies the size of a LOGFONT structure,
            /// and the pvParam parameter must point to the LOGFONT structure to fill in.
            /// </summary>
            SPI_GETICONTITLELOGFONT = 0x001F,

            /// <summary>
            /// Sets the double-click time for the mouse to the value of the uiParam parameter. The double-click time is the maximum number
            /// of milliseconds that can occur between the first and second clicks of a double-click. You can also call the SetDoubleClickTime
            /// function to set the double-click time. To get the current double-click time, call the GetDoubleClickTime function.
            /// </summary>
            SPI_SETDOUBLECLICKTIME = 0x0020,

            /// <summary>
            /// Swaps or restores the meaning of the left and right mouse buttons. The uiParam parameter specifies TRUE to swap the meanings
            /// of the buttons, or FALSE to restore their original meanings.
            /// </summary>
            SPI_SETMOUSEBUTTONSWAP = 0x0021,

            /// <summary>
            /// Sets the font that is used for icon titles. The uiParam parameter specifies the size of a LOGFONT structure,
            /// and the pvParam parameter must point to a LOGFONT structure.
            /// </summary>
            SPI_SETICONTITLELOGFONT = 0x0022,

            /// <summary>
            /// This flag is obsolete. Previous versions of the system use this flag to determine whether ALT+TAB fast task switching is enabled.
            /// For Windows 95, Windows 98, and Windows NT version 4.0 and later, fast task switching is always enabled.
            /// </summary>
            SPI_GETFASTTASKSWITCH = 0x0023,

            /// <summary>
            /// This flag is obsolete. Previous versions of the system use this flag to enable or disable ALT+TAB fast task switching.
            /// For Windows 95, Windows 98, and Windows NT version 4.0 and later, fast task switching is always enabled.
            /// </summary>
            SPI_SETFASTTASKSWITCH = 0x0024,

            //#if(WINVER >= 0x0400)
            /// <summary>
            /// Sets dragging of full windows either on or off. The uiParam parameter specifies TRUE for on, or FALSE for off.
            /// Windows 95:  This flag is supported only if Windows Plus! is installed. See SPI_GETWINDOWSEXTENSION.
            /// </summary>
            SPI_SETDRAGFULLWINDOWS = 0x0025,

            /// <summary>
            /// Determines whether dragging of full windows is enabled. The pvParam parameter must point to a BOOL variable that receives TRUE
            /// if enabled, or FALSE otherwise.
            /// Windows 95:  This flag is supported only if Windows Plus! is installed. See SPI_GETWINDOWSEXTENSION.
            /// </summary>
            SPI_GETDRAGFULLWINDOWS = 0x0026,

            /// <summary>
            /// Retrieves the metrics associated with the nonclient area of nonminimized windows. The pvParam parameter must point
            /// to a NONCLIENTMETRICS structure that receives the information. Set the cbSize member of this structure and the uiParam parameter
            /// to sizeof(NONCLIENTMETRICS).
            /// </summary>
            SPI_GETNONCLIENTMETRICS = 0x0029,

            /// <summary>
            /// Sets the metrics associated with the nonclient area of nonminimized windows. The pvParam parameter must point
            /// to a NONCLIENTMETRICS structure that contains the new parameters. Set the cbSize member of this structure
            /// and the uiParam parameter to sizeof(NONCLIENTMETRICS). Also, the lfHeight member of the LOGFONT structure must be a negative value.
            /// </summary>
            SPI_SETNONCLIENTMETRICS = 0x002A,

            /// <summary>
            /// Retrieves the metrics associated with minimized windows. The pvParam parameter must point to a MINIMIZEDMETRICS structure
            /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(MINIMIZEDMETRICS).
            /// </summary>
            SPI_GETMINIMIZEDMETRICS = 0x002B,

            /// <summary>
            /// Sets the metrics associated with minimized windows. The pvParam parameter must point to a MINIMIZEDMETRICS structure
            /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(MINIMIZEDMETRICS).
            /// </summary>
            SPI_SETMINIMIZEDMETRICS = 0x002C,

            /// <summary>
            /// Retrieves the metrics associated with icons. The pvParam parameter must point to an ICONMETRICS structure that receives
            /// the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(ICONMETRICS).
            /// </summary>
            SPI_GETICONMETRICS = 0x002D,

            /// <summary>
            /// Sets the metrics associated with icons. The pvParam parameter must point to an ICONMETRICS structure that contains
            /// the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(ICONMETRICS).
            /// </summary>
            SPI_SETICONMETRICS = 0x002E,

            /// <summary>
            /// Sets the size of the work area. The work area is the portion of the screen not obscured by the system taskbar
            /// or by application desktop toolbars. The pvParam parameter is a pointer to a RECT structure that specifies the new work area rectangle,
            /// expressed in virtual screen coordinates. In a system with multiple display monitors, the function sets the work area
            /// of the monitor that contains the specified rectangle.
            /// </summary>
            SPI_SETWORKAREA = 0x002F,

            /// <summary>
            /// Retrieves the size of the work area on the primary display monitor. The work area is the portion of the screen not obscured
            /// by the system taskbar or by application desktop toolbars. The pvParam parameter must point to a RECT structure that receives
            /// the coordinates of the work area, expressed in virtual screen coordinates.
            /// To get the work area of a monitor other than the primary display monitor, call the GetMonitorInfo function.
            /// </summary>
            SPI_GETWORKAREA = 0x0030,

            /// <summary>
            /// Windows Me/98/95:  Pen windows is being loaded or unloaded. The uiParam parameter is TRUE when loading and FALSE
            /// when unloading pen windows. The pvParam parameter is null.
            /// </summary>
            SPI_SETPENWINDOWS = 0x0031,

            /// <summary>
            /// Retrieves information about the HighContrast accessibility feature. The pvParam parameter must point to a HIGHCONTRAST structure
            /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(HIGHCONTRAST).
            /// For a general discussion, see remarks.
            /// Windows NT:  This value is not supported.
            /// </summary>
            /// <remarks>
            /// There is a difference between the High Contrast color scheme and the High Contrast Mode. The High Contrast color scheme changes
            /// the system colors to colors that have obvious contrast; you switch to this color scheme by using the Display Options in the control panel.
            /// The High Contrast Mode, which uses SPI_GETHIGHCONTRAST and SPI_SETHIGHCONTRAST, advises applications to modify their appearance
            /// for visually-impaired users. It involves such things as audible warning to users and customized color scheme
            /// (using the Accessibility Options in the control panel). For more information, see HIGHCONTRAST on MSDN.
            /// For more information on general accessibility features, see Accessibility on MSDN.
            /// </remarks>
            SPI_GETHIGHCONTRAST = 0x0042,

            /// <summary>
            /// Sets the parameters of the HighContrast accessibility feature. The pvParam parameter must point to a HIGHCONTRAST structure
            /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(HIGHCONTRAST).
            /// Windows NT:  This value is not supported.
            /// </summary>
            SPI_SETHIGHCONTRAST = 0x0043,

            /// <summary>
            /// Determines whether the user relies on the keyboard instead of the mouse, and wants applications to display keyboard interfaces
            /// that would otherwise be hidden. The pvParam parameter must point to a BOOL variable that receives TRUE
            /// if the user relies on the keyboard; or FALSE otherwise.
            /// Windows NT:  This value is not supported.
            /// </summary>
            SPI_GETKEYBOARDPREF = 0x0044,

            /// <summary>
            /// Sets the keyboard preference. The uiParam parameter specifies TRUE if the user relies on the keyboard instead of the mouse,
            /// and wants applications to display keyboard interfaces that would otherwise be hidden; uiParam is FALSE otherwise.
            /// Windows NT:  This value is not supported.
            /// </summary>
            SPI_SETKEYBOARDPREF = 0x0045,

            /// <summary>
            /// Determines whether a screen reviewer utility is running. A screen reviewer utility directs textual information to an output device,
            /// such as a speech synthesizer or Braille display. When this flag is set, an application should provide textual information
            /// in situations where it would otherwise present the information graphically.
            /// The pvParam parameter is a pointer to a BOOL variable that receives TRUE if a screen reviewer utility is running, or FALSE otherwise.
            /// Windows NT:  This value is not supported.
            /// </summary>
            SPI_GETSCREENREADER = 0x0046,

            /// <summary>
            /// Determines whether a screen review utility is running. The uiParam parameter specifies TRUE for on, or FALSE for off.
            /// Windows NT:  This value is not supported.
            /// </summary>
            SPI_SETSCREENREADER = 0x0047,

            /// <summary>
            /// Retrieves the animation effects associated with user actions. The pvParam parameter must point to an ANIMATIONINFO structure
            /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(ANIMATIONINFO).
            /// </summary>
            SPI_GETANIMATION = 0x0048,

            /// <summary>
            /// Sets the animation effects associated with user actions. The pvParam parameter must point to an ANIMATIONINFO structure
            /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(ANIMATIONINFO).
            /// </summary>
            SPI_SETANIMATION = 0x0049,

            /// <summary>
            /// Determines whether the font smoothing feature is enabled. This feature uses font antialiasing to make font curves appear smoother
            /// by painting pixels at different gray levels.
            /// The pvParam parameter must point to a BOOL variable that receives TRUE if the feature is enabled, or FALSE if it is not.
            /// Windows 95:  This flag is supported only if Windows Plus! is installed. See SPI_GETWINDOWSEXTENSION.
            /// </summary>
            SPI_GETFONTSMOOTHING = 0x004A,

            /// <summary>
            /// Enables or disables the font smoothing feature, which uses font antialiasing to make font curves appear smoother
            /// by painting pixels at different gray levels.
            /// To enable the feature, set the uiParam parameter to TRUE. To disable the feature, set uiParam to FALSE.
            /// Windows 95:  This flag is supported only if Windows Plus! is installed. See SPI_GETWINDOWSEXTENSION.
            /// </summary>
            SPI_SETFONTSMOOTHING = 0x004B,

            /// <summary>
            /// Sets the width, in pixels, of the rectangle used to detect the start of a drag operation. Set uiParam to the new value.
            /// To retrieve the drag width, call GetSystemMetrics with the SM_CXDRAG flag.
            /// </summary>
            SPI_SETDRAGWIDTH = 0x004C,

            /// <summary>
            /// Sets the height, in pixels, of the rectangle used to detect the start of a drag operation. Set uiParam to the new value.
            /// To retrieve the drag height, call GetSystemMetrics with the SM_CYDRAG flag.
            /// </summary>
            SPI_SETDRAGHEIGHT = 0x004D,

            /// <summary>
            /// Used internally; applications should not use this value.
            /// </summary>
            SPI_SETHANDHELD = 0x004E,

            /// <summary>
            /// Retrieves the time-out value for the low-power phase of screen saving. The pvParam parameter must point to an integer variable
            /// that receives the value. This flag is supported for 32-bit applications only.
            /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
            /// Windows 95:  This flag is supported for 16-bit applications only.
            /// </summary>
            SPI_GETLOWPOWERTIMEOUT = 0x004F,

            /// <summary>
            /// Retrieves the time-out value for the power-off phase of screen saving. The pvParam parameter must point to an integer variable
            /// that receives the value. This flag is supported for 32-bit applications only.
            /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
            /// Windows 95:  This flag is supported for 16-bit applications only.
            /// </summary>
            SPI_GETPOWEROFFTIMEOUT = 0x0050,

            /// <summary>
            /// Sets the time-out value, in seconds, for the low-power phase of screen saving. The uiParam parameter specifies the new value.
            /// The pvParam parameter must be null. This flag is supported for 32-bit applications only.
            /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
            /// Windows 95:  This flag is supported for 16-bit applications only.
            /// </summary>
            SPI_SETLOWPOWERTIMEOUT = 0x0051,

            /// <summary>
            /// Sets the time-out value, in seconds, for the power-off phase of screen saving. The uiParam parameter specifies the new value.
            /// The pvParam parameter must be null. This flag is supported for 32-bit applications only.
            /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
            /// Windows 95:  This flag is supported for 16-bit applications only.
            /// </summary>
            SPI_SETPOWEROFFTIMEOUT = 0x0052,

            /// <summary>
            /// Determines whether the low-power phase of screen saving is enabled. The pvParam parameter must point to a BOOL variable
            /// that receives TRUE if enabled, or FALSE if disabled. This flag is supported for 32-bit applications only.
            /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
            /// Windows 95:  This flag is supported for 16-bit applications only.
            /// </summary>
            SPI_GETLOWPOWERACTIVE = 0x0053,

            /// <summary>
            /// Determines whether the power-off phase of screen saving is enabled. The pvParam parameter must point to a BOOL variable
            /// that receives TRUE if enabled, or FALSE if disabled. This flag is supported for 32-bit applications only.
            /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
            /// Windows 95:  This flag is supported for 16-bit applications only.
            /// </summary>
            SPI_GETPOWEROFFACTIVE = 0x0054,

            /// <summary>
            /// Activates or deactivates the low-power phase of screen saving. Set uiParam to 1 to activate, or zero to deactivate.
            /// The pvParam parameter must be null. This flag is supported for 32-bit applications only.
            /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
            /// Windows 95:  This flag is supported for 16-bit applications only.
            /// </summary>
            SPI_SETLOWPOWERACTIVE = 0x0055,

            /// <summary>
            /// Activates or deactivates the power-off phase of screen saving. Set uiParam to 1 to activate, or zero to deactivate.
            /// The pvParam parameter must be null. This flag is supported for 32-bit applications only.
            /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
            /// Windows 95:  This flag is supported for 16-bit applications only.
            /// </summary>
            SPI_SETPOWEROFFACTIVE = 0x0056,

            /// <summary>
            /// Reloads the system cursors. Set the uiParam parameter to zero and the pvParam parameter to null.
            /// </summary>
            SPI_SETCURSORS = 0x0057,

            /// <summary>
            /// Reloads the system icons. Set the uiParam parameter to zero and the pvParam parameter to null.
            /// </summary>
            SPI_SETICONS = 0x0058,

            /// <summary>
            /// Retrieves the input locale identifier for the system default input language. The pvParam parameter must point
            /// to an HKL variable that receives this value. For more information, see Languages, Locales, and Keyboard Layouts on MSDN.
            /// </summary>
            SPI_GETDEFAULTINPUTLANG = 0x0059,

            /// <summary>
            /// Sets the default input language for the system shell and applications. The specified language must be displayable
            /// using the current system character set. The pvParam parameter must point to an HKL variable that contains
            /// the input locale identifier for the default language. For more information, see Languages, Locales, and Keyboard Layouts on MSDN.
            /// </summary>
            SPI_SETDEFAULTINPUTLANG = 0x005A,

            /// <summary>
            /// Sets the hot key set for switching between input languages. The uiParam and pvParam parameters are not used.
            /// The value sets the shortcut keys in the keyboard property sheets by reading the registry again. The registry must be set before this flag is used. the path in the registry is \HKEY_CURRENT_USER\keyboard layout\toggle. Valid values are "1" = ALT+SHIFT, "2" = CTRL+SHIFT, and "3" = none.
            /// </summary>
            SPI_SETLANGTOGGLE = 0x005B,

            /// <summary>
            /// Windows 95:  Determines whether the Windows extension, Windows Plus!, is installed. Set the uiParam parameter to 1.
            /// The pvParam parameter is not used. The function returns TRUE if the extension is installed, or FALSE if it is not.
            /// </summary>
            SPI_GETWINDOWSEXTENSION = 0x005C,

            /// <summary>
            /// Enables or disables the Mouse Trails feature, which improves the visibility of mouse cursor movements by briefly showing
            /// a trail of cursors and quickly erasing them.
            /// To disable the feature, set the uiParam parameter to zero or 1. To enable the feature, set uiParam to a value greater than 1
            /// to indicate the number of cursors drawn in the trail.
            /// Windows 2000/NT:  This value is not supported.
            /// </summary>
            SPI_SETMOUSETRAILS = 0x005D,

            /// <summary>
            /// Determines whether the Mouse Trails feature is enabled. This feature improves the visibility of mouse cursor movements
            /// by briefly showing a trail of cursors and quickly erasing them.
            /// The pvParam parameter must point to an integer variable that receives a value. If the value is zero or 1, the feature is disabled.
            /// If the value is greater than 1, the feature is enabled and the value indicates the number of cursors drawn in the trail.
            /// The uiParam parameter is not used.
            /// Windows 2000/NT:  This value is not supported.
            /// </summary>
            SPI_GETMOUSETRAILS = 0x005E,

            /// <summary>
            /// Windows Me/98:  Used internally; applications should not use this flag.
            /// </summary>
            SPI_SETSCREENSAVERRUNNING = 0x0061,

            /// <summary>
            /// Same as SPI_SETSCREENSAVERRUNNING.
            /// </summary>
            SPI_SCREENSAVERRUNNING = SPI_SETSCREENSAVERRUNNING,
            //#endif /* WINVER >= 0x0400 */

            /// <summary>
            /// Retrieves information about the FilterKeys accessibility feature. The pvParam parameter must point to a FILTERKEYS structure
            /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(FILTERKEYS).
            /// </summary>
            SPI_GETFILTERKEYS = 0x0032,

            /// <summary>
            /// Sets the parameters of the FilterKeys accessibility feature. The pvParam parameter must point to a FILTERKEYS structure
            /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(FILTERKEYS).
            /// </summary>
            SPI_SETFILTERKEYS = 0x0033,

            /// <summary>
            /// Retrieves information about the ToggleKeys accessibility feature. The pvParam parameter must point to a TOGGLEKEYS structure
            /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(TOGGLEKEYS).
            /// </summary>
            SPI_GETTOGGLEKEYS = 0x0034,

            /// <summary>
            /// Sets the parameters of the ToggleKeys accessibility feature. The pvParam parameter must point to a TOGGLEKEYS structure
            /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(TOGGLEKEYS).
            /// </summary>
            SPI_SETTOGGLEKEYS = 0x0035,

            /// <summary>
            /// Retrieves information about the MouseKeys accessibility feature. The pvParam parameter must point to a MOUSEKEYS structure
            /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(MOUSEKEYS).
            /// </summary>
            SPI_GETMOUSEKEYS = 0x0036,

            /// <summary>
            /// Sets the parameters of the MouseKeys accessibility feature. The pvParam parameter must point to a MOUSEKEYS structure
            /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(MOUSEKEYS).
            /// </summary>
            SPI_SETMOUSEKEYS = 0x0037,

            /// <summary>
            /// Determines whether the Show Sounds accessibility flag is on or off. If it is on, the user requires an application
            /// to present information visually in situations where it would otherwise present the information only in audible form.
            /// The pvParam parameter must point to a BOOL variable that receives TRUE if the feature is on, or FALSE if it is off.
            /// Using this value is equivalent to calling GetSystemMetrics (SM_SHOWSOUNDS). That is the recommended call.
            /// </summary>
            SPI_GETSHOWSOUNDS = 0x0038,

            /// <summary>
            /// Sets the parameters of the SoundSentry accessibility feature. The pvParam parameter must point to a SOUNDSENTRY structure
            /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(SOUNDSENTRY).
            /// </summary>
            SPI_SETSHOWSOUNDS = 0x0039,

            /// <summary>
            /// Retrieves information about the StickyKeys accessibility feature. The pvParam parameter must point to a STICKYKEYS structure
            /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(STICKYKEYS).
            /// </summary>
            SPI_GETSTICKYKEYS = 0x003A,

            /// <summary>
            /// Sets the parameters of the StickyKeys accessibility feature. The pvParam parameter must point to a STICKYKEYS structure
            /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(STICKYKEYS).
            /// </summary>
            SPI_SETSTICKYKEYS = 0x003B,

            /// <summary>
            /// Retrieves information about the time-out period associated with the accessibility features. The pvParam parameter must point
            /// to an ACCESSTIMEOUT structure that receives the information. Set the cbSize member of this structure and the uiParam parameter
            /// to sizeof(ACCESSTIMEOUT).
            /// </summary>
            SPI_GETACCESSTIMEOUT = 0x003C,

            /// <summary>
            /// Sets the time-out period associated with the accessibility features. The pvParam parameter must point to an ACCESSTIMEOUT
            /// structure that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(ACCESSTIMEOUT).
            /// </summary>
            SPI_SETACCESSTIMEOUT = 0x003D,

            //#if(WINVER >= 0x0400)
            /// <summary>
            /// Windows Me/98/95:  Retrieves information about the SerialKeys accessibility feature. The pvParam parameter must point
            /// to a SERIALKEYS structure that receives the information. Set the cbSize member of this structure and the uiParam parameter
            /// to sizeof(SERIALKEYS).
            /// Windows Server 2003, Windows XP/2000/NT:  Not supported. The user controls this feature through the control panel.
            /// </summary>
            SPI_GETSERIALKEYS = 0x003E,

            /// <summary>
            /// Windows Me/98/95:  Sets the parameters of the SerialKeys accessibility feature. The pvParam parameter must point
            /// to a SERIALKEYS structure that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter
            /// to sizeof(SERIALKEYS).
            /// Windows Server 2003, Windows XP/2000/NT:  Not supported. The user controls this feature through the control panel.
            /// </summary>
            SPI_SETSERIALKEYS = 0x003F,
            //#endif /* WINVER >= 0x0400 */

            /// <summary>
            /// Retrieves information about the SoundSentry accessibility feature. The pvParam parameter must point to a SOUNDSENTRY structure
            /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(SOUNDSENTRY).
            /// </summary>
            SPI_GETSOUNDSENTRY = 0x0040,

            /// <summary>
            /// Sets the parameters of the SoundSentry accessibility feature. The pvParam parameter must point to a SOUNDSENTRY structure
            /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(SOUNDSENTRY).
            /// </summary>
            SPI_SETSOUNDSENTRY = 0x0041,

            //#if(_WIN32_WINNT >= 0x0400)
            /// <summary>
            /// Determines whether the snap-to-default-button feature is enabled. If enabled, the mouse cursor automatically moves
            /// to the default button, such as OK or Apply, of a dialog box. The pvParam parameter must point to a BOOL variable
            /// that receives TRUE if the feature is on, or FALSE if it is off.
            /// Windows 95:  Not supported.
            /// </summary>
            SPI_GETSNAPTODEFBUTTON = 0x005F,

            /// <summary>
            /// Enables or disables the snap-to-default-button feature. If enabled, the mouse cursor automatically moves to the default button,
            /// such as OK or Apply, of a dialog box. Set the uiParam parameter to TRUE to enable the feature, or FALSE to disable it.
            /// Applications should use the ShowWindow function when displaying a dialog box so the dialog manager can position the mouse cursor.
            /// Windows 95:  Not supported.
            /// </summary>
            SPI_SETSNAPTODEFBUTTON = 0x0060,
            //#endif /* _WIN32_WINNT >= 0x0400 */

            //#if (_WIN32_WINNT >= 0x0400) || (_WIN32_WINDOWS > 0x0400)
            /// <summary>
            /// Retrieves the width, in pixels, of the rectangle within which the mouse pointer has to stay for TrackMouseEvent
            /// to generate a WM_MOUSEHOVER message. The pvParam parameter must point to a UINT variable that receives the width.
            /// Windows 95:  Not supported.
            /// </summary>
            SPI_GETMOUSEHOVERWIDTH = 0x0062,

            /// <summary>
            /// Retrieves the width, in pixels, of the rectangle within which the mouse pointer has to stay for TrackMouseEvent
            /// to generate a WM_MOUSEHOVER message. The pvParam parameter must point to a UINT variable that receives the width.
            /// Windows 95:  Not supported.
            /// </summary>
            SPI_SETMOUSEHOVERWIDTH = 0x0063,

            /// <summary>
            /// Retrieves the height, in pixels, of the rectangle within which the mouse pointer has to stay for TrackMouseEvent
            /// to generate a WM_MOUSEHOVER message. The pvParam parameter must point to a UINT variable that receives the height.
            /// Windows 95:  Not supported.
            /// </summary>
            SPI_GETMOUSEHOVERHEIGHT = 0x0064,

            /// <summary>
            /// Sets the height, in pixels, of the rectangle within which the mouse pointer has to stay for TrackMouseEvent
            /// to generate a WM_MOUSEHOVER message. Set the uiParam parameter to the new height.
            /// Windows 95:  Not supported.
            /// </summary>
            SPI_SETMOUSEHOVERHEIGHT = 0x0065,

            /// <summary>
            /// Retrieves the time, in milliseconds, that the mouse pointer has to stay in the hover rectangle for TrackMouseEvent
            /// to generate a WM_MOUSEHOVER message. The pvParam parameter must point to a UINT variable that receives the time.
            /// Windows 95:  Not supported.
            /// </summary>
            SPI_GETMOUSEHOVERTIME = 0x0066,

            /// <summary>
            /// Sets the time, in milliseconds, that the mouse pointer has to stay in the hover rectangle for TrackMouseEvent
            /// to generate a WM_MOUSEHOVER message. This is used only if you pass HOVER_DEFAULT in the dwHoverTime parameter in the call to TrackMouseEvent. Set the uiParam parameter to the new time.
            /// Windows 95:  Not supported.
            /// </summary>
            SPI_SETMOUSEHOVERTIME = 0x0067,

            /// <summary>
            /// Retrieves the number of lines to scroll when the mouse wheel is rotated. The pvParam parameter must point
            /// to a UINT variable that receives the number of lines. The default value is 3.
            /// Windows 95:  Not supported.
            /// </summary>
            SPI_GETWHEELSCROLLLINES = 0x0068,

            /// <summary>
            /// Sets the number of lines to scroll when the mouse wheel is rotated. The number of lines is set from the uiParam parameter.
            /// The number of lines is the suggested number of lines to scroll when the mouse wheel is rolled without using modifier keys.
            /// If the number is 0, then no scrolling should occur. If the number of lines to scroll is greater than the number of lines viewable,
            /// and in particular if it is WHEEL_PAGESCROLL (#defined as UINT_MAX), the scroll operation should be interpreted
            /// as clicking once in the page down or page up regions of the scroll bar.
            /// Windows 95:  Not supported.
            /// </summary>
            SPI_SETWHEELSCROLLLINES = 0x0069,

            /// <summary>
            /// Retrieves the time, in milliseconds, that the system waits before displaying a shortcut menu when the mouse cursor is
            /// over a submenu item. The pvParam parameter must point to a DWORD variable that receives the time of the delay.
            /// Windows 95:  Not supported.
            /// </summary>
            SPI_GETMENUSHOWDELAY = 0x006A,

            /// <summary>
            /// Sets uiParam to the time, in milliseconds, that the system waits before displaying a shortcut menu when the mouse cursor is
            /// over a submenu item.
            /// Windows 95:  Not supported.
            /// </summary>
            SPI_SETMENUSHOWDELAY = 0x006B,

            /// <summary>
            /// Determines whether the IME status window is visible (on a per-user basis). The pvParam parameter must point to a BOOL variable
            /// that receives TRUE if the status window is visible, or FALSE if it is not.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETSHOWIMEUI = 0x006E,

            /// <summary>
            /// Sets whether the IME status window is visible or not on a per-user basis. The uiParam parameter specifies TRUE for on or FALSE for off.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_SETSHOWIMEUI = 0x006F,
            //#endif

            //#if(WINVER >= 0x0500)
            /// <summary>
            /// Retrieves the current mouse speed. The mouse speed determines how far the pointer will move based on the distance the mouse moves.
            /// The pvParam parameter must point to an integer that receives a value which ranges between 1 (slowest) and 20 (fastest).
            /// A value of 10 is the default. The value can be set by an end user using the mouse control panel application or
            /// by an application using SPI_SETMOUSESPEED.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETMOUSESPEED = 0x0070,

            /// <summary>
            /// Sets the current mouse speed. The pvParam parameter is an integer between 1 (slowest) and 20 (fastest). A value of 10 is the default.
            /// This value is typically set using the mouse control panel application.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_SETMOUSESPEED = 0x0071,

            /// <summary>
            /// Determines whether a screen saver is currently running on the window station of the calling process.
            /// The pvParam parameter must point to a BOOL variable that receives TRUE if a screen saver is currently running, or FALSE otherwise.
            /// Note that only the interactive window station, "WinSta0", can have a screen saver running.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETSCREENSAVERRUNNING = 0x0072,

            /// <summary>
            /// Retrieves the full path of the bitmap file for the desktop wallpaper. The pvParam parameter must point to a buffer
            /// that receives a null-terminated path string. Set the uiParam parameter to the size, in characters, of the pvParam buffer. The returned string will not exceed MAX_PATH characters. If there is no desktop wallpaper, the returned string is empty.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETDESKWALLPAPER = 0x0073,
            //#endif /* WINVER >= 0x0500 */

            //#if(WINVER >= 0x0500)
            /// <summary>
            /// Determines whether active window tracking (activating the window the mouse is on) is on or off. The pvParam parameter must point
            /// to a BOOL variable that receives TRUE for on, or FALSE for off.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETACTIVEWINDOWTRACKING = 0x1000,

            /// <summary>
            /// Sets active window tracking (activating the window the mouse is on) either on or off. Set pvParam to TRUE for on or FALSE for off.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_SETACTIVEWINDOWTRACKING = 0x1001,

            /// <summary>
            /// Determines whether the menu animation feature is enabled. This master switch must be on to enable menu animation effects.
            /// The pvParam parameter must point to a BOOL variable that receives TRUE if animation is enabled and FALSE if it is disabled.
            /// If animation is enabled, SPI_GETMENUFADE indicates whether menus use fade or slide animation.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETMENUANIMATION = 0x1002,

            /// <summary>
            /// Enables or disables menu animation. This master switch must be on for any menu animation to occur.
            /// The pvParam parameter is a BOOL variable; set pvParam to TRUE to enable animation and FALSE to disable animation.
            /// If animation is enabled, SPI_GETMENUFADE indicates whether menus use fade or slide animation.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_SETMENUANIMATION = 0x1003,

            /// <summary>
            /// Determines whether the slide-open effect for combo boxes is enabled. The pvParam parameter must point to a BOOL variable
            /// that receives TRUE for enabled, or FALSE for disabled.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETCOMBOBOXANIMATION = 0x1004,

            /// <summary>
            /// Enables or disables the slide-open effect for combo boxes. Set the pvParam parameter to TRUE to enable the gradient effect,
            /// or FALSE to disable it.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_SETCOMBOBOXANIMATION = 0x1005,

            /// <summary>
            /// Determines whether the smooth-scrolling effect for list boxes is enabled. The pvParam parameter must point to a BOOL variable
            /// that receives TRUE for enabled, or FALSE for disabled.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETLISTBOXSMOOTHSCROLLING = 0x1006,

            /// <summary>
            /// Enables or disables the smooth-scrolling effect for list boxes. Set the pvParam parameter to TRUE to enable the smooth-scrolling effect,
            /// or FALSE to disable it.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_SETLISTBOXSMOOTHSCROLLING = 0x1007,

            /// <summary>
            /// Determines whether the gradient effect for window title bars is enabled. The pvParam parameter must point to a BOOL variable
            /// that receives TRUE for enabled, or FALSE for disabled. For more information about the gradient effect, see the GetSysColor function.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETGRADIENTCAPTIONS = 0x1008,

            /// <summary>
            /// Enables or disables the gradient effect for window title bars. Set the pvParam parameter to TRUE to enable it, or FALSE to disable it.
            /// The gradient effect is possible only if the system has a color depth of more than 256 colors. For more information about
            /// the gradient effect, see the GetSysColor function.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_SETGRADIENTCAPTIONS = 0x1009,

            /// <summary>
            /// Determines whether menu access keys are always underlined. The pvParam parameter must point to a BOOL variable that receives TRUE
            /// if menu access keys are always underlined, and FALSE if they are underlined only when the menu is activated by the keyboard.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETKEYBOARDCUES = 0x100A,

            /// <summary>
            /// Sets the underlining of menu access key letters. The pvParam parameter is a BOOL variable. Set pvParam to TRUE to always underline menu
            /// access keys, or FALSE to underline menu access keys only when the menu is activated from the keyboard.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_SETKEYBOARDCUES = 0x100B,

            /// <summary>
            /// Same as SPI_GETKEYBOARDCUES.
            /// </summary>
            SPI_GETMENUUNDERLINES = SPI_GETKEYBOARDCUES,

            /// <summary>
            /// Same as SPI_SETKEYBOARDCUES.
            /// </summary>
            SPI_SETMENUUNDERLINES = SPI_SETKEYBOARDCUES,

            /// <summary>
            /// Determines whether windows activated through active window tracking will be brought to the top. The pvParam parameter must point
            /// to a BOOL variable that receives TRUE for on, or FALSE for off.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETACTIVEWNDTRKZORDER = 0x100C,

            /// <summary>
            /// Determines whether or not windows activated through active window tracking should be brought to the top. Set pvParam to TRUE
            /// for on or FALSE for off.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_SETACTIVEWNDTRKZORDER = 0x100D,

            /// <summary>
            /// Determines whether hot tracking of user-interface elements, such as menu names on menu bars, is enabled. The pvParam parameter
            /// must point to a BOOL variable that receives TRUE for enabled, or FALSE for disabled.
            /// Hot tracking means that when the cursor moves over an item, it is highlighted but not selected. You can query this value to decide
            /// whether to use hot tracking in the user interface of your application.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETHOTTRACKING = 0x100E,

            /// <summary>
            /// Enables or disables hot tracking of user-interface elements such as menu names on menu bars. Set the pvParam parameter to TRUE
            /// to enable it, or FALSE to disable it.
            /// Hot-tracking means that when the cursor moves over an item, it is highlighted but not selected.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_SETHOTTRACKING = 0x100F,

            /// <summary>
            /// Determines whether menu fade animation is enabled. The pvParam parameter must point to a BOOL variable that receives TRUE
            /// when fade animation is enabled and FALSE when it is disabled. If fade animation is disabled, menus use slide animation.
            /// This flag is ignored unless menu animation is enabled, which you can do using the SPI_SETMENUANIMATION flag.
            /// For more information, see AnimateWindow.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETMENUFADE = 0x1012,

            /// <summary>
            /// Enables or disables menu fade animation. Set pvParam to TRUE to enable the menu fade effect or FALSE to disable it.
            /// If fade animation is disabled, menus use slide animation. he The menu fade effect is possible only if the system
            /// has a color depth of more than 256 colors. This flag is ignored unless SPI_MENUANIMATION is also set. For more information,
            /// see AnimateWindow.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_SETMENUFADE = 0x1013,

            /// <summary>
            /// Determines whether the selection fade effect is enabled. The pvParam parameter must point to a BOOL variable that receives TRUE
            /// if enabled or FALSE if disabled.
            /// The selection fade effect causes the menu item selected by the user to remain on the screen briefly while fading out
            /// after the menu is dismissed.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETSELECTIONFADE = 0x1014,

            /// <summary>
            /// Set pvParam to TRUE to enable the selection fade effect or FALSE to disable it.
            /// The selection fade effect causes the menu item selected by the user to remain on the screen briefly while fading out
            /// after the menu is dismissed. The selection fade effect is possible only if the system has a color depth of more than 256 colors.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_SETSELECTIONFADE = 0x1015,

            /// <summary>
            /// Determines whether ToolTip animation is enabled. The pvParam parameter must point to a BOOL variable that receives TRUE
            /// if enabled or FALSE if disabled. If ToolTip animation is enabled, SPI_GETTOOLTIPFADE indicates whether ToolTips use fade or slide animation.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETTOOLTIPANIMATION = 0x1016,

            /// <summary>
            /// Set pvParam to TRUE to enable ToolTip animation or FALSE to disable it. If enabled, you can use SPI_SETTOOLTIPFADE
            /// to specify fade or slide animation.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_SETTOOLTIPANIMATION = 0x1017,

            /// <summary>
            /// If SPI_SETTOOLTIPANIMATION is enabled, SPI_GETTOOLTIPFADE indicates whether ToolTip animation uses a fade effect or a slide effect.
            ///  The pvParam parameter must point to a BOOL variable that receives TRUE for fade animation or FALSE for slide animation.
            ///  For more information on slide and fade effects, see AnimateWindow.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETTOOLTIPFADE = 0x1018,

            /// <summary>
            /// If the SPI_SETTOOLTIPANIMATION flag is enabled, use SPI_SETTOOLTIPFADE to indicate whether ToolTip animation uses a fade effect
            /// or a slide effect. Set pvParam to TRUE for fade animation or FALSE for slide animation. The tooltip fade effect is possible only
            /// if the system has a color depth of more than 256 colors. For more information on the slide and fade effects,
            /// see the AnimateWindow function.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_SETTOOLTIPFADE = 0x1019,

            /// <summary>
            /// Determines whether the cursor has a shadow around it. The pvParam parameter must point to a BOOL variable that receives TRUE
            /// if the shadow is enabled, FALSE if it is disabled. This effect appears only if the system has a color depth of more than 256 colors.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETCURSORSHADOW = 0x101A,

            /// <summary>
            /// Enables or disables a shadow around the cursor. The pvParam parameter is a BOOL variable. Set pvParam to TRUE to enable the shadow
            /// or FALSE to disable the shadow. This effect appears only if the system has a color depth of more than 256 colors.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_SETCURSORSHADOW = 0x101B,

            //#if(_WIN32_WINNT >= 0x0501)
            /// <summary>
            /// Retrieves the state of the Mouse Sonar feature. The pvParam parameter must point to a BOOL variable that receives TRUE
            /// if enabled or FALSE otherwise. For more information, see About Mouse Input on MSDN.
            /// Windows 2000/NT, Windows 98/95:  This value is not supported.
            /// </summary>
            SPI_GETMOUSESONAR = 0x101C,

            /// <summary>
            /// Turns the Sonar accessibility feature on or off. This feature briefly shows several concentric circles around the mouse pointer
            /// when the user presses and releases the CTRL key. The pvParam parameter specifies TRUE for on and FALSE for off. The default is off.
            /// For more information, see About Mouse Input.
            /// Windows 2000/NT, Windows 98/95:  This value is not supported.
            /// </summary>
            SPI_SETMOUSESONAR = 0x101D,

            /// <summary>
            /// Retrieves the state of the Mouse ClickLock feature. The pvParam parameter must point to a BOOL variable that receives TRUE
            /// if enabled, or FALSE otherwise. For more information, see About Mouse Input.
            /// Windows 2000/NT, Windows 98/95:  This value is not supported.
            /// </summary>
            SPI_GETMOUSECLICKLOCK = 0x101E,

            /// <summary>
            /// Turns the Mouse ClickLock accessibility feature on or off. This feature temporarily locks down the primary mouse button
            /// when that button is clicked and held down for the time specified by SPI_SETMOUSECLICKLOCKTIME. The uiParam parameter specifies
            /// TRUE for on,
            /// or FALSE for off. The default is off. For more information, see Remarks and About Mouse Input on MSDN.
            /// Windows 2000/NT, Windows 98/95:  This value is not supported.
            /// </summary>
            SPI_SETMOUSECLICKLOCK = 0x101F,

            /// <summary>
            /// Retrieves the state of the Mouse Vanish feature. The pvParam parameter must point to a BOOL variable that receives TRUE
            /// if enabled or FALSE otherwise. For more information, see About Mouse Input on MSDN.
            /// Windows 2000/NT, Windows 98/95:  This value is not supported.
            /// </summary>
            SPI_GETMOUSEVANISH = 0x1020,

            /// <summary>
            /// Turns the Vanish feature on or off. This feature hides the mouse pointer when the user types; the pointer reappears
            /// when the user moves the mouse. The pvParam parameter specifies TRUE for on and FALSE for off. The default is off.
            /// For more information, see About Mouse Input on MSDN.
            /// Windows 2000/NT, Windows 98/95:  This value is not supported.
            /// </summary>
            SPI_SETMOUSEVANISH = 0x1021,

            /// <summary>
            /// Determines whether native User menus have flat menu appearance. The pvParam parameter must point to a BOOL variable
            /// that returns TRUE if the flat menu appearance is set, or FALSE otherwise.
            /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETFLATMENU = 0x1022,

            /// <summary>
            /// Enables or disables flat menu appearance for native User menus. Set pvParam to TRUE to enable flat menu appearance
            /// or FALSE to disable it.
            /// When enabled, the menu bar uses COLOR_MENUBAR for the menubar background, COLOR_MENU for the menu-popup background, COLOR_MENUHILIGHT
            /// for the fill of the current menu selection, and COLOR_HILIGHT for the outline of the current menu selection.
            /// If disabled, menus are drawn using the same metrics and colors as in Windows 2000 and earlier.
            /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_SETFLATMENU = 0x1023,

            /// <summary>
            /// Determines whether the drop shadow effect is enabled. The pvParam parameter must point to a BOOL variable that returns TRUE
            /// if enabled or FALSE if disabled.
            /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETDROPSHADOW = 0x1024,

            /// <summary>
            /// Enables or disables the drop shadow effect. Set pvParam to TRUE to enable the drop shadow effect or FALSE to disable it.
            /// You must also have CS_DROPSHADOW in the window class style.
            /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_SETDROPSHADOW = 0x1025,

            /// <summary>
            /// Retrieves a BOOL indicating whether an application can reset the screensaver's timer by calling the SendInput function
            /// to simulate keyboard or mouse input. The pvParam parameter must point to a BOOL variable that receives TRUE
            /// if the simulated input will be blocked, or FALSE otherwise.
            /// </summary>
            SPI_GETBLOCKSENDINPUTRESETS = 0x1026,

            /// <summary>
            /// Determines whether an application can reset the screensaver's timer by calling the SendInput function to simulate keyboard
            /// or mouse input. The uiParam parameter specifies TRUE if the screensaver will not be deactivated by simulated input,
            /// or FALSE if the screensaver will be deactivated by simulated input.
            /// </summary>
            SPI_SETBLOCKSENDINPUTRESETS = 0x1027,
            //#endif /* _WIN32_WINNT >= 0x0501 */

            /// <summary>
            /// Determines whether UI effects are enabled or disabled. The pvParam parameter must point to a BOOL variable that receives TRUE
            /// if all UI effects are enabled, or FALSE if they are disabled.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETUIEFFECTS = 0x103E,

            /// <summary>
            /// Enables or disables UI effects. Set the pvParam parameter to TRUE to enable all UI effects or FALSE to disable all UI effects.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_SETUIEFFECTS = 0x103F,

            /// <summary>
            /// Retrieves the amount of time following user input, in milliseconds, during which the system will not allow applications
            /// to force themselves into the foreground. The pvParam parameter must point to a DWORD variable that receives the time.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETFOREGROUNDLOCKTIMEOUT = 0x2000,

            /// <summary>
            /// Sets the amount of time following user input, in milliseconds, during which the system does not allow applications
            /// to force themselves into the foreground. Set pvParam to the new timeout value.
            /// The calling thread must be able to change the foreground window, otherwise the call fails.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_SETFOREGROUNDLOCKTIMEOUT = 0x2001,

            /// <summary>
            /// Retrieves the active window tracking delay, in milliseconds. The pvParam parameter must point to a DWORD variable
            /// that receives the time.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETACTIVEWNDTRKTIMEOUT = 0x2002,

            /// <summary>
            /// Sets the active window tracking delay. Set pvParam to the number of milliseconds to delay before activating the window
            /// under the mouse pointer.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_SETACTIVEWNDTRKTIMEOUT = 0x2003,

            /// <summary>
            /// Retrieves the number of times SetForegroundWindow will flash the taskbar button when rejecting a foreground switch request.
            /// The pvParam parameter must point to a DWORD variable that receives the value.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETFOREGROUNDFLASHCOUNT = 0x2004,

            /// <summary>
            /// Sets the number of times SetForegroundWindow will flash the taskbar button when rejecting a foreground switch request.
            /// Set pvParam to the number of times to flash.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_SETFOREGROUNDFLASHCOUNT = 0x2005,

            /// <summary>
            /// Retrieves the caret width in edit controls, in pixels. The pvParam parameter must point to a DWORD that receives this value.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETCARETWIDTH = 0x2006,

            /// <summary>
            /// Sets the caret width in edit controls. Set pvParam to the desired width, in pixels. The default and minimum value is 1.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_SETCARETWIDTH = 0x2007,

            //#if(_WIN32_WINNT >= 0x0501)
            /// <summary>
            /// Retrieves the time delay before the primary mouse button is locked. The pvParam parameter must point to DWORD that receives
            /// the time delay. This is only enabled if SPI_SETMOUSECLICKLOCK is set to TRUE. For more information, see About Mouse Input on MSDN.
            /// Windows 2000/NT, Windows 98/95:  This value is not supported.
            /// </summary>
            SPI_GETMOUSECLICKLOCKTIME = 0x2008,

            /// <summary>
            /// Turns the Mouse ClickLock accessibility feature on or off. This feature temporarily locks down the primary mouse button
            /// when that button is clicked and held down for the time specified by SPI_SETMOUSECLICKLOCKTIME. The uiParam parameter
            /// specifies TRUE for on, or FALSE for off. The default is off. For more information, see Remarks and About Mouse Input on MSDN.
            /// Windows 2000/NT, Windows 98/95:  This value is not supported.
            /// </summary>
            SPI_SETMOUSECLICKLOCKTIME = 0x2009,

            /// <summary>
            /// Retrieves the type of font smoothing. The pvParam parameter must point to a UINT that receives the information.
            /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETFONTSMOOTHINGTYPE = 0x200A,

            /// <summary>
            /// Sets the font smoothing type. The pvParam parameter points to a UINT that contains either FE_FONTSMOOTHINGSTANDARD,
            /// if standard anti-aliasing is used, or FE_FONTSMOOTHINGCLEARTYPE, if ClearType is used. The default is FE_FONTSMOOTHINGSTANDARD.
            /// When using this option, the fWinIni parameter must be set to SPIF_SENDWININICHANGE | SPIF_UPDATEINIFILE; otherwise,
            /// SystemParametersInfo fails.
            /// </summary>
            SPI_SETFONTSMOOTHINGTYPE = 0x200B,

            /// <summary>
            /// Retrieves a contrast value that is used in ClearType™ smoothing. The pvParam parameter must point to a UINT
            /// that receives the information.
            /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETFONTSMOOTHINGCONTRAST = 0x200C,

            /// <summary>
            /// Sets the contrast value used in ClearType smoothing. The pvParam parameter points to a UINT that holds the contrast value.
            /// Valid contrast values are from 1000 to 2200. The default value is 1400.
            /// When using this option, the fWinIni parameter must be set to SPIF_SENDWININICHANGE | SPIF_UPDATEINIFILE; otherwise,
            /// SystemParametersInfo fails.
            /// SPI_SETFONTSMOOTHINGTYPE must also be set to FE_FONTSMOOTHINGCLEARTYPE.
            /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_SETFONTSMOOTHINGCONTRAST = 0x200D,

            /// <summary>
            /// Retrieves the width, in pixels, of the left and right edges of the focus rectangle drawn with DrawFocusRect.
            /// The pvParam parameter must point to a UINT.
            /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETFOCUSBORDERWIDTH = 0x200E,

            /// <summary>
            /// Sets the height of the left and right edges of the focus rectangle drawn with DrawFocusRect to the value of the pvParam parameter.
            /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_SETFOCUSBORDERWIDTH = 0x200F,

            /// <summary>
            /// Retrieves the height, in pixels, of the top and bottom edges of the focus rectangle drawn with DrawFocusRect.
            /// The pvParam parameter must point to a UINT.
            /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETFOCUSBORDERHEIGHT = 0x2010,

            /// <summary>
            /// Sets the height of the top and bottom edges of the focus rectangle drawn with DrawFocusRect to the value of the pvParam parameter.
            /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_SETFOCUSBORDERHEIGHT = 0x2011,

            /// <summary>
            /// Not implemented.
            /// </summary>
            SPI_GETFONTSMOOTHINGORIENTATION = 0x2012,

            /// <summary>
            /// Not implemented.
            /// </summary>
            SPI_SETFONTSMOOTHINGORIENTATION = 0x2013,
        }

        public enum WallpaperStyle {
            Center,
            Tile,
            Stretch,
            Fit,
            Fill,
        }
        #endregion

        #region structs
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct LOGFONT {
            public int lfHeight;
            public int lfWidth;
            public int lfEscapement;
            public int lfOrientation;
            public int lfWeight;
            public byte lfItalic;
            public byte lfUnderline;
            public byte lfStrikeOut;
            public byte lfCharSet;
            public byte lfOutPrecision;
            public byte lfClipPrecision;
            public byte lfQuality;
            public byte lfPitchAndFamily;

            /// <summary>
            /// <see cref="UnmanagedType.ByValTStr"/> means that the string
            /// should be marshalled as an array of TCHAR embedded in the
            /// structure.  This implies that the font names can be no larger
            /// than 32 including the terminating '\0'.
            /// That works out to 31 characters.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string lfFaceName;

            // to shut it up about the warnings
            public LOGFONT(string lfFaceName) {
                this.lfFaceName = lfFaceName;
                lfHeight = lfWidth = lfEscapement = lfOrientation = lfWeight = 0;
                lfItalic = lfUnderline = lfStrikeOut = lfCharSet = lfOutPrecision
                = lfClipPrecision = lfQuality = lfPitchAndFamily = 0;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NONCLIENTMETRICS {
            public int cbSize;
            public int iBorderWidth;
            public int iScrollWidth;
            public int iScrollHeight;
            public int iCaptionWidth;
            public int iCaptionHeight;
            /// <summary>
            /// Since <see cref="LOGFONT"/> is a struct instead of a class,
            /// we don't have to do any special marshalling here.  Much
            /// simpler this way.
            /// </summary>
            public LOGFONT lfCaptionFont;
            public int iSMCaptionWidth;
            public int iSMCaptionHeight;
            public LOGFONT lfSMCaptionFont;
            public int iMenuWidth;
            public int iMenuHeight;
            public LOGFONT lfMenuFont;
            public LOGFONT lfStatusFont;
            public LOGFONT lfMessageFont;
            //public int iPaddedBorderWidth;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public FlashWF dwFlags;
            public UInt32 uCount;
            public UInt32 dwTimeout;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;

            public MARGINS(int left, int top, int right, int bottom) {
                leftWidth = left;
                topHeight = top;
                rightWidth = right;
                bottomHeight = bottom;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESSENTRY32 {
            public uint dwSize;
            public uint cntUsage;
            public uint th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public uint th32ModuleID;
            public uint cntThreads;
            public uint th32ParentProcessID;
            public int pcPriClassBase;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szExeFile;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct SCROLLINFO {
            /// <summary>Size of the structure in bytes</summary>
            public int cbSize;
            /// <summary>Mask of info to retrieve</summary>
            public ScrollInfoMask fMask;
            /// <summary>Minimum scrolling position</summary>
            public int min;
            /// <summary>Maximum scrolling position</summary>
            public int max;
            /// <summary>Page size in device units</summary>
            public int nPage;
            /// <summary>Position of the scrollbox, doesn't change while dragging</summary>
            public int nPos;
            /// <summary>Live position of the scrollbox while dragging</summary>
            public int nTrackPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RGNDATA {
            public RGNDATAHEADER header;
            public RECT[] rects; // doesn't work for GetRegionDataManaged()
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RGNDATAHEADER {
            public uint dwSize;
            public uint iType;
            public uint nCount;
            public uint nRgnSize;
            public RECT rcBound;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CWPSTRUCT {
            public IntPtr lparam;
            public IntPtr wparam;
            public WM message;
            public IntPtr hwnd;

            public override string ToString() {
                return $"{{message: {message}, wParam: {wparam}, lParam: {lparam}, hwnd: {hwnd}}}";
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int Left, Top, Right, Bottom;

            public RECT(int left, int top, int right, int bottom) {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public RECT(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

            public int X {
                get { return Left; }
                set { Right -= (Left - value); Left = value; }
            }

            public int Y {
                get { return Top; }
                set { Bottom -= (Top - value); Top = value; }
            }

            public int Height {
                get { return Bottom - Top; }
                set { Bottom = value + Top; }
            }

            public int Width {
                get { return Right - Left; }
                set { Right = value + Left; }
            }

            public System.Drawing.Point Location {
                get { return new System.Drawing.Point(Left, Top); }
                set { X = value.X; Y = value.Y; }
            }

            public System.Drawing.Size Size {
                get { return new System.Drawing.Size(Width, Height); }
                set { Width = value.Width; Height = value.Height; }
            }

            public static implicit operator System.Drawing.Rectangle(RECT r) {
                return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
            }

            public static implicit operator RECT(System.Drawing.Rectangle r) {
                return new RECT(r);
            }

            public static bool operator ==(RECT r1, RECT r2) {
                return r1.Equals(r2);
            }

            public static bool operator !=(RECT r1, RECT r2) {
                return !r1.Equals(r2);
            }

            public bool Equals(RECT r) {
                return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
            }

            public override bool Equals(object obj) {
                if (obj is RECT)
                    return Equals((RECT)obj);
                else if (obj is System.Drawing.Rectangle)
                    return Equals(new RECT((System.Drawing.Rectangle)obj));
                return false;
            }

            public override int GetHashCode() {
                return ((System.Drawing.Rectangle)this).GetHashCode();
            }

            public override string ToString() {
                return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
            }
        }

        public struct POINT {
            public int X;
            public int Y;

            public POINT(int x, int y) {
                X = x;
                Y = y;
            }

            public static implicit operator System.Drawing.Point(POINT p) {
                return new System.Drawing.Point(p.X, p.Y);
            }

            public static implicit operator POINT(System.Drawing.Point p) {
                return new POINT(p.X, p.Y);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KBDLLHOOKSTRUCT {
            public uint vkCode;
            public uint scanCode;
            public KbdllFlags flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT {
            public POINT pt;
            public int mouseData;
            public MsllFlags flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct COLORREF {
            public uint ColorDWORD;

            public COLORREF(System.Drawing.Color color) {
                ColorDWORD = (uint)color.R + (((uint)color.G) << 8) + (((uint)color.B) << 16);
            }

            public System.Drawing.Color GetColor() {
                return System.Drawing.Color.FromArgb((int)(0x000000FFU & ColorDWORD),
               (int)(0x0000FF00U & ColorDWORD) >> 8, (int)(0x00FF0000U & ColorDWORD) >> 16);
            }

            public void SetColor(System.Drawing.Color color) {
                ColorDWORD = (uint)color.R + (((uint)color.G) << 8) + (((uint)color.B) << 16);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        /// <summary>
        /// The MONITORINFOEX structure contains information about a display monitor.
        /// The GetMonitorInfo function stores information into a MONITORINFOEX structure or a MONITORINFO structure.
        /// The MONITORINFOEX structure is a superset of the MONITORINFO structure. The MONITORINFOEX structure adds a string member to contain a name
        /// for the display monitor.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MONITORINFOEX {
            /// <summary>
            /// The size, in bytes, of the structure. Set this member to sizeof(MONITORINFOEX) (72) before calling the GetMonitorInfo function.
            /// Doing so lets the function determine the type of structure you are passing to it.
            /// </summary>
            public int Size;

            /// <summary>
            /// A RECT structure that specifies the display monitor rectangle, expressed in virtual-screen coordinates.
            /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
            /// </summary>
            public RECT Monitor;

            /// <summary>
            /// A RECT structure that specifies the work area rectangle of the display monitor that can be used by applications,
            /// expressed in virtual-screen coordinates. Windows uses this rectangle to maximize an application on the monitor.
            /// The rest of the area in rcMonitor contains system windows such as the task bar and side bars.
            /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
            /// </summary>
            public RECT WorkArea;

            /// <summary>
            /// The attributes of the display monitor.
            ///
            /// This member can be the following value:
            ///   1 : MONITORINFOF_PRIMARY
            /// </summary>
            public uint Flags;

            /// <summary>
            /// A string that specifies the device name of the monitor being used. Most applications have no use for a display monitor name,
            /// and so can save some bytes by using a MONITORINFO structure.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;

            public void Init() {
                Size = 40 + 2 * 32;
                DeviceName = string.Empty;
            }
        }

        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
        public struct DEVMODE {
            public const int CCHDEVICENAME = 32;
            public const int CCHFORMNAME = 32;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            [FieldOffset(0)]
            public string dmDeviceName;
            [FieldOffset(32)]
            public Int16 dmSpecVersion;
            [FieldOffset(34)]
            public Int16 dmDriverVersion;
            [FieldOffset(36)]
            public Int16 dmSize;
            [FieldOffset(38)]
            public Int16 dmDriverExtra;
            [FieldOffset(40)]
            public DisplayFlags dmFields;

            [FieldOffset(44)]
            Int16 dmOrientation;
            [FieldOffset(46)]
            Int16 dmPaperSize;
            [FieldOffset(48)]
            Int16 dmPaperLength;
            [FieldOffset(50)]
            Int16 dmPaperWidth;
            [FieldOffset(52)]
            Int16 dmScale;
            [FieldOffset(54)]
            Int16 dmCopies;
            [FieldOffset(56)]
            Int16 dmDefaultSource;
            [FieldOffset(58)]
            Int16 dmPrintQuality;

            [FieldOffset(44)]
            public POINTL dmPosition;
            [FieldOffset(52)]
            public Int32 dmDisplayOrientation;
            [FieldOffset(56)]
            public Int32 dmDisplayFixedOutput;

            [FieldOffset(60)]
            public short dmColor; // See note below!
            [FieldOffset(62)]
            public short dmDuplex; // See note below!
            [FieldOffset(64)]
            public short dmYResolution;
            [FieldOffset(66)]
            public short dmTTOption;
            [FieldOffset(68)]
            public short dmCollate; // See note below!
            [FieldOffset(70)]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            public string dmFormName;
            [FieldOffset(102)]
            public Int16 dmLogPixels;
            [FieldOffset(104)]
            public Int32 dmBitsPerPel;
            [FieldOffset(108)]
            public Int32 dmPelsWidth;
            [FieldOffset(112)]
            public Int32 dmPelsHeight;
            [FieldOffset(116)]
            public Int32 dmDisplayFlags;
            [FieldOffset(116)]
            public Int32 dmNup;
            [FieldOffset(120)]
            public Int32 dmDisplayFrequency;
        }

        public struct POINTL {
            public Int32 x;
            public Int32 y;

            public POINTL(Int32 x, Int32 y) {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT {
            public InputType type;
            public InputUnion union;
            public static int Size { get => Marshal.SizeOf(typeof(INPUT)); }
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion {
            [FieldOffset(0)]
            public MOUSEINPUT mouse;
            [FieldOffset(0)]
            public KEYBDINPUT keyboard;
            [FieldOffset(0)]
            public HARDWAREINPUT hardware;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT {
            public int dx;
            public int dy;
            public int mouseData;
            public MOUSEEVENTF flags;
            public uint time;
            public UIntPtr extraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT {
            public VKey vk;
            public ScanCode sc;
            public KEYEVENTF flags;
            public int time;
            public UIntPtr extraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT {
            public int msg;
            public short lparam;
            public short hparam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CURSORINFO {
            public Int32 cbSize;        // Specifies the size, in bytes, of the structure.
                                        // The caller must set this to Marshal.SizeOf(typeof(CURSORINFO)).
            public CursorState state;
            public IntPtr hCursor;          // Handle to the cursor.
            public POINT ptScreenPos;       // A POINT structure that receives the screen coordinates of the cursor.

            public static CURSORINFO Initialized() {
                var info = new CURSORINFO();
                info.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
                return info;
            }
        }

        public struct ICONINFO {
            /// <summary>
            /// Specifies whether this structure defines an icon or a cursor.
            /// A value of TRUE specifies an icon; FALSE specifies a cursor
            /// </summary>
            public bool fIcon;
            /// <summary>
            /// The x-coordinate of a cursor's hot spot
            /// </summary>
            public Int32 xHotspot;
            /// <summary>
            /// The y-coordinate of a cursor's hot spot
            /// </summary>
            public Int32 yHotspot;
            /// <summary>
            /// The icon bitmask bitmap
            /// </summary>
            public IntPtr hbmMask;
            /// <summary>
            /// A handle to the icon color bitmap.
            /// </summary>
            public IntPtr hbmColor;
        }
        #endregion
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
