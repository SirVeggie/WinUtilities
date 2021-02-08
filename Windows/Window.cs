using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WinUtilities {

    /// <summary>An object that specifies additional borderless settings for all matching windows</summary>
    public struct BorderlessInfo {
        /// <summary>Specifies which windows are affected by this setting</summary>
        public IMatchObject match;
        /// <summary></summary>
        public Area offset;

        /// <summary>An object that specifies additional borderless settings for all matching windows</summary>
        /// <param name="match">Specifies which windows are affected by this setting</param>
        /// <param name="offset">Amount cropped inwards from each edge of the window. Width and height here mean the amount cropped from right and bottom.</param>
        public BorderlessInfo(IMatchObject match, Area offset) {
            this.match = match;
            this.offset = offset;
        }

        /// <summary>An object that specifies additional borderless settings for all matching windows</summary>
        /// <param name="match">Specifies which windows are affected by this setting</param>
        /// <param name="left">Amount cropped inwards from the left edge</param>
        /// <param name="top">Amount cropped inwards from the top edge</param>
        /// <param name="right">Amount cropped inwards from the right edge</param>
        /// <param name="bottom">Amount cropped inwards from the bottom edge</param>
        public BorderlessInfo(IMatchObject match, int left, int top, int right, int bottom) {
            this.match = match;
            offset = new Area(left, top, right, bottom);
        }
    }

    /// <summary>A wrapper object for a windows window</summary>
    [DataContract]
    public class Window {

        /// <summary>The handle of the window.</summary>
        [DataMember]
        public WinHandle Hwnd { get; set; }
        private Process process;
        private string exepath;
        private string @class;
        private int threadID;
        [DataMember]
        private string exe;
        private uint pid;

        private static int borderWidth = WinAPI.GetSystemMetrics(WinAPI.SM.CXSIZEFRAME);
        private static int borderVisibleWidth = WinAPI.GetSystemMetrics(WinAPI.SM.CXBORDER);

        #region properties

        #region basic
        /// <summary>The title of the window.</summary>
        public string Title {
            get {
                int length = WinAPI.GetWindowTextLength(Hwnd.Raw);
                StringBuilder title = new StringBuilder(length);
                WinAPI.GetWindowText(Hwnd.Raw, title, length + 1);
                return title.ToString();
            }
        }

        /// <summary>The class of the window.</summary>
        public string Class {
            get {
                while (@class == null)
                    @class = WinAPI.GetClassFromHwnd(Hwnd.Raw);
                return @class;
            }
        }

        /// <summary>The name of this window's <see cref="System.Diagnostics.Process"/>' .exe file. The .exe part is excluded.</summary>
        public string Exe {
            get {
                if (exe == null) {
                    var s = ExePath.Split('\\').Last().Split('.');
                    exe = string.Join(".", s.Take(s.Length - 1));
                }

                return exe;
            }
        }

        /// <summary>The path of this window's exe file.</summary>
        public string ExePath {
            get {
                while (exepath == null)
                    exepath = WinAPI.GetPathFromPid(PID);
                return exepath;
            }
        }

        /// <summary>The process handle of this window's <see cref="System.Diagnostics.Process"/>.</summary>
        public uint PID {
            get {
                while (pid == 0)
                    pid = WinAPI.GetPidFromHwnd(Hwnd.Raw);
                return pid;
            }
        }

        /// <summary>The <see cref="System.Diagnostics.Process"/> this window belongs to.</summary>
        public Process Process {
            get {
                if (process == null)
                    process = Process.GetProcessById((int) PID);
                return process;
            }
        }

        /// <summary>The ID of the system thread that spawned this window.</summary>
        public int ThreadID {
            get {
                if (threadID == 0)
                    threadID = (int) WinAPI.GetWindowThreadProcessId(Hwnd.Raw, out _);
                return threadID;
            }
        }
        #endregion

        #region state
        /// <summary>Check if the window is not hidden.</summary>
        public bool IsVisible => WinAPI.IsWindowVisible(Hwnd.Raw);
        /// <summary>Check if the window is interactable.</summary>
        public bool IsEnabled => WinAPI.IsWindowEnabled(Hwnd.Raw);
        /// <summary>Check if the window is the foreground window.</summary>
        public bool IsActive => HwndActive(Hwnd);
        /// <summary>Check if a window with this handle still exists.</summary>
        public bool Exists => HwndExists(Hwnd);
        /// <summary>Check if this is a top level window.</summary>
        public bool IsAlwaysOnTop => HasExStyle(WS_EX.TOPMOST);
        /// <summary>Check if clicks go through the window.</summary>
        public bool IsClickThrough => HasExStyle(WS_EX.TRANSPARENT | WS_EX.LAYERED);
        /// <summary>Check if this is a child window of some other window.</summary>
        public bool IsChild => HasStyle(WS.CHILD);
        /// <summary>Check if the window is maximized.</summary>
        public bool IsMaximized => WinAPI.IsZoomed(Hwnd.Raw);
        /// <summary>Check if the window is minimized.</summary>
        public bool IsMinimized => WinAPI.IsIconic(Hwnd.Raw);
        /// <summary>Check if the window is fullscreen.</summary>
        public bool IsFullscreen => !HasStyle(WS.CAPTION) && !HasStyle(WS.BORDER) && Area == Monitor.Area;
        /// <summary>Check if the window is set to borderless mode.</summary>
        public bool IsBorderless => HasRegion;
        /// <summary>Check if the window is a proper foreground window.</summary>
        public bool IsApp {
            get {
                var ex = ExStyle;

                if (!HasStyle(WS.VISIBLE)) {
                    return false;
                } else if (ex.HasFlag(WS_EX.APPWINDOW)) {
                    return true;
                } else if (this != Owner) {
                    return false;
                } else if (ex.HasFlag(WS_EX.TOOLWINDOW)) {
                    return false;
                } else if (ex.HasFlag(WS_EX.NOREDIRECTIONBITMAP)) {
                    return false;
                } else {
                    return true;
                }
            }
        }

        /// <summary>Full combination of the associated Window Styles.</summary>
        public WS Style => (WS) (long) WinAPI.GetWindowLongPtr(Hwnd.Raw, WinAPI.WindowLongFlags.GWL_STYLE);
        /// <summary>Full combination of the associated Window Ex Styles.</summary>
        public WS_EX ExStyle => (WS_EX) (long) WinAPI.GetWindowLongPtr(Hwnd.Raw, WinAPI.WindowLongFlags.GWL_EXSTYLE);
        /// <summary>The percentage of how see-through the window is.</summary>
        public double Opacity {
            get => throw new NotImplementedException();
            set => SetOpacity(value);
        }
        /// <summary>The color of the window that is rendered as fully transparent.</summary>
        public Color Transcolor {
            get => throw new NotImplementedException();
            set => SetTranscolor(value);
        }
        /// <summary>Check if a window has a region.</summary>
        public bool HasRegion => WinAPI.GetWindowRgnBox(Hwnd.Raw, out _) != WinAPI.RegionType.Error;
        /// <summary>Check the type of the region.</summary>
        public WinAPI.RegionType RegionType => WinAPI.GetWindowRgnBox(Hwnd.Raw, out _);
        /// <summary>Get the bounding area of the current region. Relative to raw window coordinates.</summary>
        public Area RegionBounds {
            get {
                if (WinAPI.GetWindowRgnBox(Hwnd.Raw, out WinAPI.RECT rect) != WinAPI.RegionType.Error)
                    return rect;
                return Area.NaN;
            }
        }
        #endregion

        #region positions
        /// <summary>Attempt at reusing area information because getting them is somewhat costly.</summary>
        private Area CalculateRealArea(Area? raw = null, Area? client = null, Area? region = null) {
            if (IsMaximized) {
                return Monitor.Area;
            }

            if (IsBorderless) {
                return CalculateRegionArea(raw, region);
            }

            Area realRaw = raw == null ? RawArea : (Area) raw;
            Area realClient = client == null ? ClientArea : (Area) client;

            if (realRaw.Point == realClient.Point) {
                return realClient;
            }

            var fix = borderWidth - borderVisibleWidth;
            return new Area(realRaw.X + fix, realRaw.Y, realRaw.W - fix * 2, realRaw.H - fix);
        }

        private Area CalculateRegionArea(Area? raw = null, Area? region = null) {
            Area realRaw = raw == null ? RawArea : (Area) raw;
            Area realRegion = region == null ? RegionBounds : (Area) region;
            return realRegion.AddPoint(realRaw);
        }

        /// <summary>A corrected version of the window's area.</summary>
        public Area Area {
            get {
                return CalculateRealArea();
            }
            set {
                Area raw = RawArea;
                Area client = ClientArea;
                Area? region = IsBorderless ? (Area?) RegionBounds : null;
                Area real = CalculateRealArea(raw, client, region);

                OffsetMove(value, real, raw);

                if (IsBorderless) {
                    SetRegion(((Area) region).AddSize(value.FillNaN(real) - real));
                }
            }
        }

        /// <summary>The area of the window as given by the OS.</summary>
        public Area RawArea {
            get {
                WinAPI.RECT rect = new WinAPI.RECT();
                WinAPI.GetWindowRect(Hwnd.Raw, ref rect);
                return new Area(rect.Left, rect.Top, rect.Width, rect.Height);
            }
            set {
                Area target = value.IsValid ? value : value.FillNaN(RawArea);
                WinAPI.SetWindowPos(Hwnd.Raw, IntPtr.Zero, target.IntX, target.IntY, target.IntW, target.IntH, WinAPI.WindowPosFlags.NoZOrder | WinAPI.WindowPosFlags.NoActivate);
            }
        }

        /// <summary>The client area of the window. Excludes the caption and the borders.</summary>
        public Area ClientArea {
            get {
                WinAPI.POINT point = new WinAPI.POINT();
                WinAPI.RECT rect = new WinAPI.RECT();

                WinAPI.ClientToScreen(Hwnd.Raw, ref point);
                WinAPI.GetClientRect(Hwnd.Raw, ref rect);

                return new Area(point.X, point.Y, rect.Width, rect.Height);
            }
            set => OffsetMove(value, ClientArea);
        }

        /// <summary>The visible area of the window when in borderless mode</summary>
        public Area BorderlessArea {
            get {
                if (!IsBorderless)
                    return CalculateBorderlessArea(ClientArea);
                return CalculateRegionArea(RawArea);
            }
            set {
                if (!IsBorderless)
                    OffsetMove(value, BorderlessArea);
                Area = value;
            }
        }
        #endregion

        /// <summary>The object points to nothing</summary>
        public bool IsNone => Hwnd.IsZero;
        /// <summary>The object points to a real window</summary>
        public bool IsValid => Hwnd.IsValid;

        /// <summary>A list of borderless settings that direct window behaviour when setting to borderless mode</summary>
        public static List<BorderlessInfo> BorderlessSettings { get; set; } = new List<BorderlessInfo>();

        /// <summary>A window object that doesn't point to any window</summary>
        public static Window None => new Window(IntPtr.Zero);
        /// <summary>Retrieves the active window.</summary>
        public static Window Active => new Window(GetActiveWindow());
        /// <summary>Retrieves the first window under the mouse.</summary>
        public static Window FromMouse => FromPoint(Mouse.Position);
        /// <summary>Retrieves the current process's windows.</summary>
        public static List<Window> This => GetWindows(new WinMatch(pid: (uint) Process.GetCurrentProcess().Id), true);

        /// <summary>The parent of the window.</summary>
        public Window Parent => new Window(WinAPI.GetWindowLongPtr(Hwnd.Raw, WinAPI.WindowLongFlags.GWLP_HWNDPARENT));
        /// <summary>The topmost window in the window's parent chain.</summary>
        public Window Ancestor => new Window(WinAPI.GetAncestor(Hwnd.Raw, WinAPI.AncestorFlags.GetRoot));
        /// <summary>The topmost window in the window's parent chain on a deeper level than <see cref="Ancestor"/>.</summary>
        public Window Owner => new Window(WinAPI.GetAncestor(Hwnd.Raw, WinAPI.AncestorFlags.GetRootOwner));
        /// <summary>Retrieves all window of the same process.</summary>
        public List<Window> Siblings => GetWindows(new WinMatch(pid: PID), true);

        /// <summary>Handle of the <see cref="WinUtilities.Monitor"/> the window is on.</summary>
        public Monitor Monitor => Monitor.FromWindow(this);
        #endregion

        #region constructors
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        private Window() { }
        public Window(WinHandle hwnd) => Hwnd = hwnd;
        public Window(IntPtr hwnd) => Hwnd = new WinHandle(hwnd);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #endregion

        #region instance

        #region queries
        /// <summary>Check if the window matches with the given description.</summary>
        public bool Match(IMatchObject match) => match?.Match(this) ?? false;
        /// <summary>Check if the <paramref name="point"/> is inside the window.</summary>
        public bool ContainsPoint(Coord point) => ContainsPoint(point.IntX, point.IntY);
        /// <summary>Check if the point is inside the window.</summary>
        public bool ContainsPoint(int x, int y) => Area.Round().Contains(x, y);
        /// <summary>Check if the <see cref="Mouse"/> is inside the window.</summary>
        public bool ContainsMouse() => ContainsPoint(Mouse.Position);
        /// <summary>Check what part of the window the given point is a part of.</summary>
        public HT GetSection(int x, int y) => GetSection(new Coord(x, y));
        /// <summary>Check what part of the window the given <paramref name="point"/> is a part of.</summary>
        public HT GetSection(Coord point) => (HT) SendMessage(WM.NCHITTEST, 0, point.AsValue);
        /// <summary>Check if the given point is part of the titlebar.</summary>
        public bool IsTitlebar(int x, int y) => IsTitlebar(new Coord(x, y));
        /// <summary>Check if the given <paramref name="point"/> is part of the titlebar.</summary>
        public bool IsTitlebar(Coord point) {
            var res = (int) GetSection(point);
            var set = new HashSet<int> { 2, 3, 8, 9, 12, 20, 21 };
            return set.Contains(res);
        }
        /// <summary>Check for individual Window Styles.</summary>
        public bool HasStyle(WS style) => (Style & style) == style;
        /// <summary>Check for individual Window Ex Styles.</summary>
        public bool HasExStyle(WS_EX style) => (ExStyle & style) == style;
        #endregion

        #region basic actions
        /// <summary>Set the window as the foreground window.</summary>
        public Window Activate() {
            WinAPI.SetForegroundWindow(Hwnd.Raw);
            return this;
        }
        /// <summary>Enable/disable the window. Disabled windows cannot be interacted with.</summary>
        public Window Enable(bool state) {
            WinAPI.EnableWindow(Hwnd.Raw, state);
            return this;
        }
        /// <summary>Kill the process associated with the window.</summary>
        public Window Kill() {
            Process.Kill();
            return this;
        }

        /// <summary>Send a request to close to the window. Returns true on success.</summary>
        public bool Close() => Ancestor.PostMessage(WM.CLOSE, 0, 0);

        /// <summary>Minimize the window.</summary>
        public Window Minimize() {
            WinAPI.ShowWindow(Hwnd.Raw, WinAPI.SW.FORCEMINIMIZE);
            return this;
        }
        /// <summary>Maximize the window.</summary>
        public Window Maximize() {
            WinAPI.ShowWindow(Hwnd.Raw, WinAPI.SW.MAXIMIZE);
            return this;
        }
        /// <summary>Restore the window from a minimized or a maximized state to normal.</summary>
        public Window Restore() {
            WinAPI.ShowWindow(Hwnd.Raw, WinAPI.SW.RESTORE);
            return this;
        }
        /// <summary>Set window visibility. False hides the window from the user completely. It's more complex than simple transparency.</summary>
        public Window SetVisible(bool state) {
            WinAPI.ShowWindow(Hwnd.Raw, state ? WinAPI.SW.SHOWNA : WinAPI.SW.HIDE);
            return this;
        }
        /// <summary>Normally hidden windows often have weird alternate behaviour. This version is less prone to that while not 'truly' hiding a window.</summary>
        public Window SetVisibleSoft(bool state) {
            SetClickThrough(!state);
            return SetOpacity(state ? 100 : 0);
        }

        /// <summary>Post a message to the window's message pump. Returns true on success.</summary>
        public bool PostMessage(WM msg, int wParam, int lParam) => WinAPI.PostMessage(Hwnd.Raw, (uint) msg, (IntPtr) wParam, (IntPtr) lParam);
        /// <summary>Send a message the window's message pump. Waits for a reply from the window.</summary>
        public IntPtr SendMessage(WM msg, int wParam, int lParam) => WinAPI.SendMessage(Hwnd.Raw, (uint) msg, (IntPtr) wParam, (IntPtr) lParam);
        /// <summary>Set individual Window Styles on and off.</summary>
        public Window SetStyle(WS style, bool state) {
            WS newStyle = state ? Style | style : Style & ~style;
            WinAPI.SetWindowLongPtr(Hwnd.Raw, WinAPI.WindowLongFlags.GWL_STYLE, (IntPtr) newStyle);
            WinAPI.SetWindowPos(Hwnd.Raw, IntPtr.Zero, 0, 0, 0, 0, WinAPI.WindowPosFlags.NoActivate | WinAPI.WindowPosFlags.NoMove | WinAPI.WindowPosFlags.NoSize | WinAPI.WindowPosFlags.NoZOrder | WinAPI.WindowPosFlags.FrameChanged);
            return this;
        }
        /// <summary>Set individual Window Ex Styles on and off.</summary>
        public Window SetExStyle(WS_EX style, bool state) {
            WS_EX newStyle = state ? ExStyle | style : ExStyle & ~style;
            WinAPI.SetWindowLongPtr(Hwnd.Raw, WinAPI.WindowLongFlags.GWL_EXSTYLE, (IntPtr) newStyle);
            WinAPI.SetWindowPos(Hwnd.Raw, IntPtr.Zero, 0, 0, 0, 0, WinAPI.WindowPosFlags.NoActivate | WinAPI.WindowPosFlags.NoMove | WinAPI.WindowPosFlags.NoSize | WinAPI.WindowPosFlags.NoZOrder | WinAPI.WindowPosFlags.FrameChanged);
            return this;
        }

        /// <summary>Brings the window to the top of visibility.</summary>
        public Window MoveTop() {
            WinAPI.SetWindowPos(Hwnd.Raw, (IntPtr) WinAPI.HWND_Z.TOP, 0, 0, 0, 0, WinAPI.WindowPosFlags.NoMove | WinAPI.WindowPosFlags.NoSize | WinAPI.WindowPosFlags.NoActivate);
            return this;
        }
        /// <summary>Drop the window to the bottom of visibility.</summary>
        public Window MoveBottom() {
            WinAPI.SetWindowPos(Hwnd.Raw, (IntPtr) WinAPI.HWND_Z.BOTTOM, 0, 0, 0, 0, WinAPI.WindowPosFlags.NoMove | WinAPI.WindowPosFlags.NoSize | WinAPI.WindowPosFlags.NoActivate);
            return this;
        }
        /// <summary>Move this window under the specified window in visibility.</summary>
        public Window MoveUnder(Window win) {
            WinAPI.SetWindowPos(Hwnd.Raw, win.Hwnd.Raw, 0, 0, 0, 0, WinAPI.WindowPosFlags.NoMove | WinAPI.WindowPosFlags.NoSize | WinAPI.WindowPosFlags.NoActivate);
            return this;
        }

        /// <summary>Make a window always stay visible.</summary>
        public Window SetAlwaysOnTop(bool state) {
            IntPtr msg = state ? (IntPtr) WinAPI.HWND_Z.TOPMOST : (IntPtr) WinAPI.HWND_Z.NOTOPMOST;
            WinAPI.SetWindowPos(Hwnd.Raw, msg, 0, 0, 0, 0, WinAPI.WindowPosFlags.NoMove | WinAPI.WindowPosFlags.NoSize | WinAPI.WindowPosFlags.NoActivate);
            return this;
        }

        /// <summary>Make clicks phase through the window to the windows below.</summary>
        public Window SetClickThrough(bool state) {
            if (state) {
                return SetExStyle(WS_EX.TRANSPARENT | WS_EX.LAYERED, true);
            } else {
                return SetExStyle(WS_EX.TRANSPARENT, false);
            }
        }

        /// <summary>Set the degree of see-through of the window in percentages.</summary>
        public Window SetOpacity(double percentage) {
            if (!HasExStyle(WS_EX.LAYERED)) {
                SetExStyle(WS_EX.LAYERED, true);
            }

            percentage = Math.Min(Math.Max(percentage, 0), 100);
            int alpha = (int) Math.Round(percentage * 255 / 100);

            WinAPI.SetLayeredWindowAttributes(Hwnd.Raw, 0, (byte) alpha, WinAPI.LayeredWindowFlags.LWA_ALPHA);
            return this;
        }

        /// <summary>Set the color of the window that is rendered as fully transparent.</summary>
        public Window SetTranscolor(Color color) {
            if (!HasExStyle(WS_EX.LAYERED)) {
                SetExStyle(WS_EX.LAYERED, true);
            }

            var c = new WinAPI.COLORREF(color);

            WinAPI.SetLayeredWindowAttributes(Hwnd.Raw, c.ColorDWORD, 0, WinAPI.LayeredWindowFlags.LWA_COLORKEY);
            return this;
        }

        /// <summary>Fully disable transparency. Might improve performance after window transparency has been tweaked.</summary>
        public Window SetTransOff() => SetExStyle(WS_EX.LAYERED | WS_EX.TRANSPARENT, false);

        /// <summary>Disable the resizing limits of the window.</summary>
        public Window UnlockSize() {
            throw new NotImplementedException();
        }
        #endregion

        #region moving
        /// <summary>Move the window to the new coordinates.</summary>
        /// <param name="x">Left edge of the window. Null to not change.</param>
        /// <param name="y">Top edge of the window. Null to not change.</param>
        /// <param name="w">Width of the window. Null to not change.</param>
        /// <param name="h">Height of the window. Null to not change.</param>
        /// <param name="type">Set what the coordinates are relative to.</param>
        public Window Move(int? x = null, int? y = null, int? w = null, int? h = null, CoordType type = CoordType.Normal) {
            return Move(new Area(x ?? double.NaN, y ?? double.NaN, w ?? double.NaN, h ?? double.NaN), type);
        }

        /// <summary>Move the window to the new coordinates.</summary>
        /// <param name="point">Location of the window. Null to not move the window.</param>
        /// <param name="size">Size of the window. Null to not resize the window.</param>
        /// <param name="type">Set what the coordinates are relative to.</param>
        public Window Move(Coord? point = null, Coord? size = null, CoordType type = CoordType.Normal) {
            point = point ?? Coord.NaN;
            size = size ?? Coord.NaN;
            return Move(new Area(point, size), type);
        }

        /// <summary>Move the window to the new coordinates.</summary>
        /// <param name="pos">The target area of the window.</param>
        /// <param name="type">Set what the coordinates are relative to.</param>
        public Window Move(Area pos, CoordType type = CoordType.Normal) {
            if (type == CoordType.Normal) {
                Area = pos;
            } else if (type == CoordType.Client) {
                ClientArea = pos;
            } else {
                RawArea = pos;
            }

            return this;
        }

        /// <summary>Move a window by using an offset.</summary>
        public Window OffsetMove(Area pos, Area offset) => OffsetMove(pos, offset, RawArea);
        private Window OffsetMove(Area pos, Area offset, Area raw) {
            pos = pos.IsValid ? pos : pos.FillNaN(offset);
            pos += raw - offset;
            WinAPI.SetWindowPos(Hwnd.Raw, IntPtr.Zero, pos.IntX, pos.IntY, pos.IntW, pos.IntH, WinAPI.WindowPosFlags.NoZOrder | WinAPI.WindowPosFlags.NoActivate);
            return this;
        }

        /// <summary>Center the window to the specified monitor.</summary>
        /// <param name="ignoreTaskbar">Set to true to ignore the space taken by the taskbar when calculating centering.</param>
        public Window Center(bool ignoreTaskbar = false) => Center(null, null, ignoreTaskbar);
        /// <summary>Center the window to the specified monitor.</summary>
        /// <param name="size">Set the target size of the window before centering.</param>
        /// <param name="ignoreTaskbar">Set to true to ignore the space taken by the taskbar when calculating centering.</param>
        public Window Center(Coord size, bool ignoreTaskbar = false) => Center(null, size, ignoreTaskbar);
        /// <summary>Center the window to the specified monitor.</summary>
        /// <param name="monitor">Index of the target monitor. Null targets current monitor. Zero based indexing.</param>
        /// <param name="size">Set the target size of the window before centering.</param>
        /// <param name="ignoreTaskbar">Set to true to ignore the space taken by the taskbar when calculating centering.</param>
        public Window Center(int? monitor, Coord? size = null, bool ignoreTaskbar = false) {
            Monitor mon = monitor == null ? Monitor : Monitor.FromIndex((int) monitor);

            var newSize = size == null ? Area.Size : (Coord) size;
            var area = ignoreTaskbar ? mon.Area : mon.WorkArea;

            return Move(new Area(area.Center - newSize / 2, newSize));
        }
        #endregion

        #region regions
        /// <summary>Set only a specified area of a window visible.</summary>
        /// <param name="region">Relative to raw window coordinates.</param>
        public Window SetRegion(Area region) {
            var r = WinAPI.CreateRectRgn((int) region.Left, (int) region.Top, (int) region.Right, (int) region.Bottom);
            WinAPI.SetWindowRgn(Hwnd.Raw, r, true);
            WinAPI.DeleteObject(r);
            return this;
        }

        /// <summary>Set only a specified area of a window visible. Has rounded corners.</summary>
        /// <param name="region">Relative to raw window coordinates.</param>
        /// <param name="horizontalRounding">Amount of horizontal rounding</param>
        /// <param name="verticalRounding">Amount of vertical rounding</param>
        public Window SetRoundedRegion(Area region, int horizontalRounding, int verticalRounding) {
            var r = WinAPI.CreateRoundRectRgn((int) region.Left, (int) region.Top, (int) region.Right, (int) region.Bottom, horizontalRounding, verticalRounding);
            WinAPI.SetWindowRgn(Hwnd.Raw, r, true);
            WinAPI.DeleteObject(r);
            return this;
        }

        /// <summary>Set only a specified area of a window visible. Has an elliptic shape.</summary>
        /// <param name="region">Relative to raw window coordinates.</param>
        public Window SetEllipticRegion(Area region) {
            var r = WinAPI.CreateEllipticRgn((int) region.Left, (int) region.Top, (int) region.Right, (int) region.Bottom);
            WinAPI.SetWindowRgn(Hwnd.Raw, r, true);
            WinAPI.DeleteObject(r);
            return this;
        }

        /// <summary>Set only a specified area of a window visible. Create a region with multiple areas.</summary>
        /// <param name="regions">Relative to raw window coordinates.</param>
        public Window SetComplexRegion(params Area[] regions) {
            if (regions.Length == 0) {
                throw new ArgumentException("Must have at least 2 rectangles specified");
            } else if (regions.Length == 1) {
                return SetRegion(regions[0]);
            }

            IntPtr full = IntPtr.Zero;

            IntPtr[] r = new IntPtr[regions.Length];
            for (int i = 0; i < regions.Length; i++) {
                r[i] = WinAPI.CreateRectRgn((int) regions[i].Left, (int) regions[i].Top, (int) regions[i].Right, (int) regions[i].Bottom);
            }

            foreach (var item in r) {
                WinAPI.CombineRgn(full, full, item, WinAPI.CombineRgnFlags.Or);
            }

            WinAPI.SetWindowRgn(Hwnd.Raw, full, true);

            foreach (var item in r) {
                WinAPI.DeleteObject(item);
            }

            WinAPI.DeleteObject(full);
            return this;
        }

        /// <summary>Set only a specified area of a window visible. Create a polygon shape with multiple points.</summary>
        /// <param name="points">Relative to raw window coordinates.</param>
        public Window SetComplexRegion(params Coord[] points) => SetComplexRegion(WinAPI.FillRgnFlags.Alternate, points);

        /// <summary>Set only a specified area of a window visible. Create a polygon shape with multiple points.</summary>
        /// <param name="fillType">Set the fill logic of when lines intersect</param>
        /// <param name="points">Relative to raw window coordinates</param>
        public Window SetComplexRegion(WinAPI.FillRgnFlags fillType, params Coord[] points) {
            if (points.Length < 3) {
                throw new ArgumentException("Must have at least 3 points to make a polygon shape");
            }

            var region = WinAPI.CreatePolygonRgn(points.Cast<WinAPI.POINT>().ToArray(), points.Length, fillType);

            WinAPI.SetWindowRgn(Hwnd.Raw, region, true);
            WinAPI.DeleteObject(region);
            return this;
        }

        /// <summary>Remove the window's region to display the full window.</summary>
        public Window RemoveRegion() {
            WinAPI.SetWindowRgn(Hwnd.Raw, IntPtr.Zero, true);
            return this;
        }
        #endregion

        #region borderless
        /// <summary>Set the window to borderless mode.</summary>
        public Window SetBorderless(bool state) {
            var style = WS.BORDER | WS.SIZEFRAME | WS.DLGFRAME;

            if (state) {
                var pos = Area;
                SetStyle(style, false);
                PostMessage(WM.SIZE, 0, 0);
                SetRegion(CalculateBorderlessRegion(RawArea, ClientArea));
                Area = pos;
            } else {
                var pos = Area;
                SetStyle(style, true);
                PostMessage(WM.SIZE, 0, 0);
                RemoveRegion();
                Area = pos;
            }

            return this;
        }

        /// <summary>Return region.</summary>
        private Area CalculateBorderlessRegion(Area raw, Area client) {
            return CalculateBorderlessArea(client).AddPoint(-raw);
        }

        /// <summary>Return screen coordinates of the visible area when borderless.</summary>
        private Area CalculateBorderlessArea(Area client) {
            foreach (var info in BorderlessSettings.ToArray()) {
                if (info.match.Match(this)) {
                    client.TopLeftR += info.offset.Point;
                    client.Size -= info.offset.Size;
                }
            }

            return client;
        }
        #endregion

        #region images
        /// <summary>Get an image of the window using BitBlt</summary>
        /// <param name="clientOnly">Capture only the client area</param>
        public Image GetImage(bool clientOnly = false) => GetImage(Area.NaN, clientOnly);
        /// <summary>Get a cropped image of the window using BitBlt</summary>
        /// <param name="subArea">Set the capture sub area relative to the full capture area</param>
        /// <param name="clientOnly">Capture only the client area</param>
        public Image GetImage(Area subArea, bool clientOnly = false) {
            Area area = Area;
            Area client = ClientArea;
            Area capture;

            if (subArea.IsValid) {
                Area raw = RawArea;
                Area fullArea = clientOnly ? client : area;
                if (!fullArea.Contains(subArea.AddPoint(fullArea.Point)))
                    throw new ArgumentException("Given sub area does not fit within the capture area");
                capture = subArea.AddPoint(fullArea.Point - raw.Point);
            } else {
                capture = area.SetPoint(new Coord(area.X == client.X ? 0 : borderWidth - borderVisibleWidth, 0));
            }

            // get te hDC of the target window
            IntPtr hdcSrc = WinAPI.GetWindowDC(Hwnd.Raw);
            // create a device context we can copy to
            IntPtr hdcDest = WinAPI.CreateCompatibleDC(hdcSrc);
            // create a bitmap we can copy it to,
            // using GetDeviceCaps to get the width/height
            IntPtr hBitmap = WinAPI.CreateCompatibleBitmap(hdcSrc, capture.IntW, capture.IntH);
            // select the bitmap object
            WinAPI.SelectObject(hdcDest, hBitmap);
            // bitblt over
            WinAPI.BitBlt(hdcDest, 0, 0, capture.IntW, capture.IntH, hdcSrc, capture.IntX, capture.IntY, WinAPI.TernaryRasterOperations.SRCCOPY);
            // clean up
            WinAPI.DeleteDC(hdcDest);
            WinAPI.ReleaseDC(Hwnd.Raw, hdcSrc);
            // get a .NET image object for it
            Image img = Image.FromHbitmap(hBitmap);
            // free up the Bitmap object
            WinAPI.DeleteObject(hBitmap);

            return img;
        }

        /// <summary>Get an image of the window using WindowPrint API. Capable of imaging off screen windows.</summary>
        public Image GetImagePrint(bool clientOnly = false) {
            Area area = clientOnly ? ClientArea : Area;
            Image img = new Bitmap(area.IntW, area.IntH);
            Graphics g = Graphics.FromImage(img);
            IntPtr dc = g.GetHdc();
            WinAPI.PrintWindow(Hwnd.Raw, dc, clientOnly);
            g.ReleaseHdc();
            g.Dispose();
            return img;
        }

        /// <summary>Get an image of the window based on what's visible on the desktop currently</summary>
        /// <param name="clientOnly">Capture only the client area</param>
        public Image GetImageDesktop(bool clientOnly = false) => GetImageDesktop(Area.NaN, clientOnly);
        /// <summary>Get a cropped image of the window based on what's visible on the desktop currently</summary>
        /// <param name="subArea">Set the capture sub area relative to the full capture area</param>
        /// <param name="clientOnly">Capture only the client area</param>
        public Image GetImageDesktop(Area subArea, bool clientOnly = false) {
            var area = clientOnly ? ClientArea : Area;

            if (subArea.IsValid) {
                subArea.Point += area.Point;
                if (!area.Contains(subArea))
                    throw new ArgumentException("Given sub area does not fit within the capture area");
                area = subArea;
            }

            Image img = new Bitmap(area.IntW, area.IntH);
            Graphics g = Graphics.FromImage(img);
            g.CopyFromScreen(area, Point.Empty, area);
            g.Dispose();
            return img;
        }
        #endregion

        #region control send
        /// <summary>Send text to the window using <see cref="Input.SendControl(Window, string)"/></summary>
        public Window Send(string text) {
            Input.SendControl(this, text);
            return this;
        }

        /// <summary>Send raw text to the window using <see cref="Input.SendControlRaw(Window, string)"/></summary>
        public Window SendRaw(string text) {
            Input.SendControlRaw(this, text);
            return this;
        }

        /// <summary>Send key events to the window using <see cref="Input.SendControl(Window, Key[])"/></summary>
        public Window Send(params Key[] keys) {
            Input.SendControl(this, keys);
            return this;
        }

        /// <summary>Send key down events to the window using <see cref="Input.SendControlDown(Window, Key[])"/></summary>
        public Window SendDown(params Key[] keys) {
            Input.SendControlDown(this, keys);
            return this;
        }

        /// <summary>Send key up events to the window using <see cref="Input.SendControlUp(Window, Key[])"/></summary>
        public Window SendUp(params Key[] keys) {
            Input.SendControlUp(this, keys);
            return this;
        }


        /// <summary>Emulates a click at the specified position.</summary>
        public Window Click(Coord pos, Key key = Key.LButton, CoordRelation rel = CoordRelation.ActiveWindow) {
            WM down;
            WM up;

            if (key == Key.LButton) {
                down = WM.LBUTTONDOWN;
                up = WM.LBUTTONUP;
            } else if (key == Key.RButton) {
                down = WM.RBUTTONDOWN;
                up = WM.RBUTTONUP;
            } else if (key == Key.MButton) {
                down = WM.MBUTTONDOWN;
                up = WM.MBUTTONUP;
            } else {
                throw new ArgumentException("Invalid key");
            }

            if (rel == CoordRelation.Screen) {
                pos = RawArea.Relative(pos);
            } else if (rel == CoordRelation.Mouse) {
                pos = RawArea.Point + Mouse.Position + pos;
            }

            PostMessage(down, 0, pos.AsValue);
            PostMessage(up, 0, pos.AsValue);
            return this;
        }

        /// <summary>Emulates a right click at the specified position.</summary>
        public Window RightClick(Coord pos, CoordRelation rel = CoordRelation.ActiveWindow) => Click(pos, Key.RButton, rel);

        /// <summary>Emulates a middle click at the specified position.</summary>
        public Window MiddleClick(Coord pos, CoordRelation rel = CoordRelation.ActiveWindow) => Click(pos, Key.MButton, rel);

        /// <summary>Emulates a click at the specified position. Tries to prevent window activation.</summary>
        public async Task ClickNA(Coord pos, Key key = Key.LButton, CoordRelation rel = CoordRelation.ActiveWindow) {
            var na = !HasExStyle(WS_EX.NOACTIVATE);
            if (na) SetExStyle(WS_EX.NOACTIVATE, true);
            await Task.Delay(1);
            Click(pos, key, rel);
            await Task.Delay(1);
            if (na) SetExStyle(WS_EX.NOACTIVATE, false);
        }

        /// <summary>Emulates a click at the specified position.</summary>
        public Window DoubleClick(Coord pos, Key key = Key.LButton, CoordRelation rel = CoordRelation.ActiveWindow) {
            WM msg;

            if (key == Key.LButton) {
                msg = WM.LBUTTONDBLCLK;
            } else if (key == Key.RButton) {
                msg = WM.RBUTTONDBLCLK;
            } else if (key == Key.MButton) {
                msg = WM.MBUTTONDBLCLK;
            } else {
                throw new ArgumentException("Invalid key");
            }

            if (rel == CoordRelation.Screen) {
                pos = RawArea.Relative(pos);
            } else if (rel == CoordRelation.Mouse) {
                pos = RawArea.Point + Mouse.Position + pos;
            }

            PostMessage(msg, 0, pos.AsValue);
            return this;
        }

        /// <summary>Emulates a click at the specified position. Tries to prevent window activation.</summary>
        public async Task DoubleClickNA(Coord pos, Key key = Key.LButton, CoordRelation rel = CoordRelation.ActiveWindow) {
            var na = !HasExStyle(WS_EX.NOACTIVATE);
            if (na) SetExStyle(WS_EX.NOACTIVATE, true);
            await Task.Delay(1);
            DoubleClick(pos, key, rel);
            await Task.Delay(1);
            if (na) SetExStyle(WS_EX.NOACTIVATE, false);
        }

        /// <summary>Emulates a click at the specified position.</summary>
        public Window ClickDown(Coord pos, Key key = Key.LButton, CoordRelation rel = CoordRelation.ActiveWindow) {
            WM msg;

            if (key == Key.LButton) {
                msg = WM.LBUTTONDOWN;
            } else if (key == Key.RButton) {
                msg = WM.RBUTTONDOWN;
            } else if (key == Key.MButton) {
                msg = WM.MBUTTONDOWN;
            } else {
                throw new ArgumentException("Invalid key");
            }

            if (rel == CoordRelation.Screen) {
                pos = RawArea.Relative(pos);
            } else if (rel == CoordRelation.Mouse) {
                pos = RawArea.Point + Mouse.Position + pos;
            }

            PostMessage(msg, 0, pos.AsValue);
            return this;
        }

        /// <summary>Emulates a click at the specified position. Tries to prevent window activation.</summary>
        public async Task ClickDownNA(Coord pos, Key key = Key.LButton, CoordRelation rel = CoordRelation.ActiveWindow) {
            var na = !HasExStyle(WS_EX.NOACTIVATE);
            if (na) SetExStyle(WS_EX.NOACTIVATE, true);
            await Task.Delay(1);
            ClickDown(pos, key, rel);
            await Task.Delay(1);
            if (na) SetExStyle(WS_EX.NOACTIVATE, false);
        }

        /// <summary>Emulates a click at the specified position.</summary>
        public Window ClickUp(Coord pos, Key key = Key.LButton, CoordRelation rel = CoordRelation.ActiveWindow) {
            WM msg;

            if (key == Key.LButton) {
                msg = WM.LBUTTONUP;
            } else if (key == Key.RButton) {
                msg = WM.RBUTTONUP;
            } else if (key == Key.MButton) {
                msg = WM.MBUTTONUP;
            } else {
                throw new ArgumentException("Invalid key");
            }

            if (rel == CoordRelation.Screen) {
                pos = RawArea.Relative(pos);
            } else if (rel == CoordRelation.Mouse) {
                pos = RawArea.Point + Mouse.Position + pos;
            }

            PostMessage(msg, 0, pos.AsValue);
            return this;
        }

        /// <summary>Emulates a click at the specified position. Tries to prevent window activation.</summary>
        public async Task ClickUpNA(Coord pos, Key key = Key.LButton, CoordRelation rel = CoordRelation.ActiveWindow) {
            var na = !HasExStyle(WS_EX.NOACTIVATE);
            if (na) SetExStyle(WS_EX.NOACTIVATE, true);
            await Task.Delay(1);
            ClickUp(pos, key, rel);
            await Task.Delay(1);
            if (na) SetExStyle(WS_EX.NOACTIVATE, false);
        }

        /// <summary>Emulates a mouse move event at the specified position.</summary>
        public Window MouseMove(Coord pos, CoordRelation rel = CoordRelation.ActiveWindow) {
            if (rel == CoordRelation.Screen) {
                pos = RawArea.Relative(pos);
            } else if (rel == CoordRelation.Mouse) {
                pos = RawArea.Point + Mouse.Position + pos;
            }

            PostMessage(WM.MOUSEMOVE, 0, pos.AsValue);
            return this;
        }
        #endregion

        #endregion

        #region static

        /// <summary>Check if a window with the specified handle exists.</summary>
        public static bool HwndExists(WinHandle hwnd) => WinAPI.IsWindow(hwnd.Raw);
        /// <summary>Check if a window with the specified handle is active.</summary>
        public static bool HwndActive(WinHandle hwnd) => hwnd == GetActiveWindow();

        #region find
        /// <summary>Find a window that matches the given description.</summary>
        public static Window Find(IMatchObject match, bool hidden = false) => GetWindows(match, hidden).FirstOrDefault();
        /// <summary>Find a window that matches the given title.</summary>
        public static Window Find(string title, bool hidden = false) => Find(new WinMatch(title: title), hidden);
        /// <summary>Find a window that matches the given .exe name.</summary>
        public static Window FindByExe(string exe, bool hidden = false) => Find(new WinMatch(exe: exe), hidden);
        /// <summary>Find a window that matches the given class.</summary>
        public static Window FindByClass(string className, bool hidden = false) => Find(new WinMatch(className: className), hidden);
        /// <summary>Find a window whose process's id matches the given id</summary>
        public static Window FindByPid(uint pid, bool hidden = false) => Find(new WinMatch(pid: pid), hidden);
        #endregion

        #region get windows
        /// <summary>Find all windows. Includes hidden windows.</summary>
        public static List<Window> GetAllWindows() => GetWindows(hidden: true);

        /// <summary>Find all windows that match the criteria.</summary>
        /// <param name="hidden">Include hidden windows.</param>
        public static List<Window> GetWindows(bool hidden = false) => GetWindows(null, hidden);

        /// <summary>Find all windows that match the criteria.</summary>
        /// <param name="match">Null to match all windows.</param>
        /// <param name="hidden">Include hidden windows.</param>
        public static List<Window> GetWindows(IMatchObject match, bool hidden = false) {
            var windows = new List<Window>();

            if (hidden && WinAPI.EnumWindows(Collector, IntPtr.Zero)) {
                return windows;
            } else if (WinAPI.EnumDesktopWindows(IntPtr.Zero, Collector, IntPtr.Zero)) {
                return windows;
            } else {
                return new List<Window>();
            }

            bool Collector(IntPtr hwnd, int lParam) {
                Window win = new Window(hwnd);

                if (!CheckMatchValidity(win, match, hidden)) return true;

                windows.Add(win);
                return true;
            }
        }

        private static bool CheckMatchValidity(Window win, IMatchObject match, bool hidden) {
            if (!hidden && !win.IsApp) return false;
            if (match != null && !match.Match(win)) return false;

            return true;
        }

        /// <summary>Get the handle of the active window.</summary>
        public static WinHandle GetActiveWindow() => new WinHandle(WinAPI.GetForegroundWindow());

        /// <summary>Get the handle of the topmost window of the given point.</summary>
        public static Window FromPoint(int x, int y) => FromPoint(new Coord(x, y));

        /// <summary>Get the handle of the topmost window of the given point.</summary>
        public static Window FromPoint(Coord point) => new Window(WinAPI.WindowFromPoint(point)).Ancestor;
        #endregion

        #region winwait
        /// <summary>Wait for a matching window to exist.</summary>
        /// <param name="match">Set what kind of window to wait for</param>
        /// <param name="timeout">Time until the wait fails</param>
        /// <param name="ignoreCurrent">If true, only new windows will be considered.</param>
        /// <param name="checkDelay">Set how often the windows are scanned for the target window</param>
        public static async Task<Window> Wait(IMatchObject match, long? timeout = null, bool ignoreCurrent = true, int checkDelay = 10) {
            var watch = Stopwatch.StartNew();
            var existingWindowsSet = new HashSet<Window>();
            foreach (var item in GetWindows(match, true))
                existingWindowsSet.Add(item);

            if (!ignoreCurrent && existingWindowsSet.Count > 0) {
                return existingWindowsSet.First();
            }

            await Task.Delay(checkDelay);
            while (watch.ElapsedMilliseconds < timeout) {
                var newWindows = GetWindows(match, true);

                foreach (var win in newWindows)
                    if (!existingWindowsSet.Contains(win))
                        return win;

                await Task.Delay(checkDelay);
            }

            return null;
        }

        /// <summary>Wait for a matching window to become active.</summary>
        /// <param name="match">Set what kind of window to wait for</param>
        /// <param name="timeout">Time until the wait fails</param>
        /// <param name="ignoreCurrent">If true, only new windows will be considered.</param>
        /// <param name="checkDelay">Set how often the active window is checked for the target window</param>
        public static async Task<Window> WaitActive(IMatchObject match, long? timeout = null, bool ignoreCurrent = true, int checkDelay = 10) {
            var watch = Stopwatch.StartNew();
            var active = Active;

            if (!ignoreCurrent && match.Match(active)) {
                return active;
            }

            await Task.Delay(checkDelay);
            while (watch.ElapsedMilliseconds < timeout) {
                var newActive = Active;
                if (newActive != active && match.Match(newActive))
                    return newActive;
                await Task.Delay(checkDelay);
            }

            return null;
        }
        #endregion

        #endregion

        #region operators
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool operator ==(Window a, Window b) => (a is null && b is null) || !(a is null) && !(b is null) && a.Hwnd == b.Hwnd;
        public static bool operator !=(Window a, Window b) => !(a == b);
        public override bool Equals(object obj) => obj is Window && this == (Window) obj;
        public override int GetHashCode() => -640239398 + Hwnd.GetHashCode();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #endregion
    }
}