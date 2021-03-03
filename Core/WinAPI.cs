using System;
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
        #endregion

        #region macros
        /// <summary>Macro that maps a scan code to a virtual key by <see cref="KeyMapFlags.ScanCode_to_VirtualKeyEx"/></summary>
        public static VKey MapVirtualKey(ScanCode sc) => (VKey) MapVirtualKey((uint) sc, KeyMapFlags.ScanCode_to_VirtualKeyEx);
        /// <summary>Macro that maps a virtual key to a scan code by <see cref="KeyMapFlags.VirtualKey_to_ScanCode"/></summary>
        public static ScanCode MapVirtualKey(VKey vk) => (ScanCode) MapVirtualKey((uint) vk, KeyMapFlags.VirtualKey_to_ScanCode);
        /// <summary>Macro that maps a scan code to a char by <see cref="KeyMapFlags.ScanCode_to_Char"/></summary>
        public static char MapVirtualKeyChar(ScanCode sc) => (char) MapVirtualKey((uint) sc, KeyMapFlags.ScanCode_to_Char);

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

        public static IntPtr SendMessage<T>(IntPtr handle, WM message, IntPtr wParam, ref T lParam) => SendMessage(handle, (uint) message, wParam, ref lParam);
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
        #endregion

        #region enums
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
        #endregion

        #region structs
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
                    return Equals((RECT) obj);
                else if (obj is System.Drawing.Rectangle)
                    return Equals(new RECT((System.Drawing.Rectangle) obj));
                return false;
            }

            public override int GetHashCode() {
                return ((System.Drawing.Rectangle) this).GetHashCode();
            }

            public override string ToString() {
                return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
            }
        }

        public struct POINT {
            public int X;
            public int Y;

            public POINT(int x, int y) {
                this.X = x;
                this.Y = y;
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
                ColorDWORD = (uint) color.R + (((uint) color.G) << 8) + (((uint) color.B) << 16);
            }

            public System.Drawing.Color GetColor() {
                return System.Drawing.Color.FromArgb((int) (0x000000FFU & ColorDWORD),
               (int) (0x0000FF00U & ColorDWORD) >> 8, (int) (0x00FF0000U & ColorDWORD) >> 16);
            }

            public void SetColor(System.Drawing.Color color) {
                ColorDWORD = (uint) color.R + (((uint) color.G) << 8) + (((uint) color.B) << 16);
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
        #endregion
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
