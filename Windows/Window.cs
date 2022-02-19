using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WinUtilities {

    #region additional structs
    /// <summary>An object that specifies additional borderless settings for all matching windows</summary>
    public struct BorderlessInfo {
        /// <summary>Specifies which windows are affected by this setting</summary>
        public IWinMatch match;
        /// <summary></summary>
        public Area offset;

        /// <summary>An object that specifies additional borderless settings for all matching windows</summary>
        /// <param name="match">Specifies which windows are affected by this setting</param>
        /// <param name="offset">Amount cropped inwards from each edge of the window. Width and height here mean the amount cropped from right and bottom.</param>
        public BorderlessInfo(IWinMatch match, Area offset) {
            this.match = match;
            this.offset = offset;
        }

        /// <summary>An object that specifies additional borderless settings for all matching windows</summary>
        /// <param name="match">Specifies which windows are affected by this setting</param>
        /// <param name="left">Amount cropped inwards from the left edge</param>
        /// <param name="top">Amount cropped inwards from the top edge</param>
        /// <param name="right">Amount cropped inwards from the right edge</param>
        /// <param name="bottom">Amount cropped inwards from the bottom edge</param>
        public BorderlessInfo(IWinMatch match, int left, int top, int right, int bottom) {
            this.match = match;
            offset = new Area(left, top, right, bottom);
        }
    }

    /// <summary>Specifies what mode is used when enumerating windows</summary>
    public enum WinFindMode {
        /// <summary>Enumerates all existing windows</summary>
        All,
        /// <summary>Enumerates top level windows on any virtual desktop</summary>
        TopLevel,
        /// <summary>Enumerates top level windows on current virtual desktop</summary>
        CurrentDesktop
    }

    /// <summary>Determines the method of activating a window</summary>
    public enum WinActivateMode {
        /// <summary>Uses a simple activation logic that is graceful. Activation might fail sometimes.</summary>
        Soft,
        /// <summary>Uses a forceful method of activation that is more reliable</summary>
        Force,
        /// <summary>Tries to first use the soft activation, but will resort to force if it failed. Using this method might make the taskbar flash briefly if the soft activation fails.</summary>
        SoftThenForce
    }
    #endregion

    /// <summary>A wrapper object for a windows window</summary>
    [DataContract]
    public class Window {

        /// <summary>The handle of the window</summary>
        [DataMember]
        public IntPtr Hwnd { get; set; }
        private Process process;
        private string exepath;
        [DataMember]
        private string @class;
        private uint threadID;
        private string exe;
        private uint pid;

        private double? opacity;
        private Color? transcolor;

        private static int borderWidth = WinAPI.GetSystemMetrics(WinAPI.SM.CXSIZEFRAME) + WinAPI.GetBorderPadding();
        private static int borderVisibleWidth = WinAPI.GetSystemMetrics(WinAPI.SM.CXBORDER);

        #region properties

        #region basic
        /// <summary>The title of the window</summary>
        public string Title {
            get {
                if (IsNone) return "";
                int length = WinAPI.GetWindowTextLength(Hwnd);
                StringBuilder title = new StringBuilder(length);
                WinAPI.GetWindowText(Hwnd, title, length + 1);
                return title.ToString();
            }
        }

        /// <summary>The class of the window</summary>
        public string Class {
            get {
                if (@class == null) {
                    @class = IsNone ? "" : WinAPI.GetClassFromHwnd(Hwnd);
                    if (@class == null) return "";
                }

                return @class;
            }
        }

        /// <summary>The name of this window's <see cref="System.Diagnostics.Process"/>' .exe file. The .exe part is excluded</summary>
        public string Exe {
            get {
                if (exe == null)
                    exe = IsNone ? "" : WinAPI.GetExeNameFromPath(ExePath);
                return exe;
            }
        }

        /// <summary>The path of this window's exe file</summary>
        public string ExePath {
            get {
                if (exepath == null) {
                    exepath = IsNone ? "" : WinAPI.GetPathFromPid(PID);
                    if (exepath == null) return "";
                }

                return exepath;
            }
        }

        /// <summary>The process handle of this window's <see cref="System.Diagnostics.Process"/></summary>
        public uint PID {
            get {
                if (pid == 0)
                    pid = IsNone ? 0 : WinAPI.GetPidFromHwnd(Hwnd);
                return pid;
            }
        }

        /// <summary>The <see cref="System.Diagnostics.Process"/> this window belongs to. Getting this info for the first time is slow (1000x slower than other properties) so prefer other ways like ExePath and Exe if possible.</summary>
        public Process Process {
            get {
                if (process == null)
                    process = IsNone ? null : Process.GetProcessById((int) PID);
                return process;
            }
        }

        /// <summary>The ID of the system thread that spawned this window</summary>
        public uint ThreadID {
            get {
                if (threadID == 0)
                    threadID = IsNone ? 0 : WinAPI.GetWindowThreadProcessId(Hwnd, out _);
                return threadID;
            }
        }
        #endregion

        #region state
        /// <summary>Check if the window is not hidden</summary>
        public bool IsVisible => !IsNone && WinAPI.IsWindowVisible(Hwnd);
        /// <summary>Check if the window is interactable</summary>
        public bool IsEnabled => !IsNone && WinAPI.IsWindowEnabled(Hwnd);
        /// <summary>Check if the window is the foreground window</summary>
        public bool IsActive => !IsNone && HwndActive(Hwnd);
        /// <summary>Check if a window with this handle still exists</summary>
        public bool Exists => !IsNone && HwndExists(Hwnd);
        /// <summary>Check if the window resides on the current virtual desktop</summary>
        public bool IsOnCurrentDesktop => !IsNone && SimpleDesktop.IsOnCurrent(this);
        /// <summary>Check if this is a top level window</summary>
        public bool IsTopmost => IsAlwaysOnTop;
        /// <summary>Alternative to <see cref="IsTopmost"/>. Check if this is a top level window</summary>
        public bool IsAlwaysOnTop => !IsNone && HasExStyle(WS_EX.TOPMOST);
        /// <summary>Check if clicks go through the window</summary>
        public bool IsClickThrough => !IsNone && HasExStyle(WS_EX.TRANSPARENT | WS_EX.LAYERED);
        /// <summary>Check if this is a child window of some other window</summary>
        public bool IsChild => !IsNone && HasStyle(WS.CHILD);
        /// <summary>Check if the window is maximized</summary>
        public bool IsMaximized => !IsNone && WinAPI.IsZoomed(Hwnd);
        /// <summary>Check if the window is minimized</summary>
        public bool IsMinimized => !IsNone && WinAPI.IsIconic(Hwnd);
        /// <summary>Check if the window is fullscreen</summary>
        public bool IsFullscreen => !IsNone && !HasStyle(WS.CAPTION) && !HasStyle(WS.BORDER) && Area == Monitor.Area;
        /// <summary>Check if the window is set to borderless mode</summary>
        public bool IsBorderless => !IsNone && !HasStyle(WS.BORDER) && HasRegion;
        /// <summary>Some opacity value less than 1 has been set for this window (from this process)</summary>
        public bool IsTransparent => !IsNone && (opacity != null && opacity != 1) || (FetchCache(Hwnd)?.opacity != null && FetchCache(Hwnd).opacity != 1);
        /// <summary>Transcolor has been set for this window (from this process)</summary>
        public bool HasTranscolor => !IsNone && transcolor != null || FetchCache(Hwnd)?.transcolor != null;
        /// <summary>Check if the window is a proper visible foreground window</summary>
        public bool IsTopLevel {
            get {
                if (IsNone)
                    return false;
                if (!HasStyle(WS.VISIBLE))
                    return false;
                if (TopLevelWhitelist.ContainsKey(this) && TopLevelWhitelist[this].Match(this))
                    return true;
                var ex = ExStyle;
                if (ex.HasFlag(WS_EX.APPWINDOW))
                    return true;
                if (this != Owner)
                    return false;
                if (ex.HasFlag(WS_EX.TOOLWINDOW))
                    return false;
                if (ex.HasFlag(WS_EX.NOREDIRECTIONBITMAP))
                    return false;
                return true;
            }
        }

        /// <summary>Full combination of the associated Window Styles</summary>
        public WS Style => (WS) (long) WinAPI.GetWindowLongPtr(Hwnd, WinAPI.WindowLongFlags.GWL_STYLE);
        /// <summary>Full combination of the associated Window Ex Styles</summary>
        public WS_EX ExStyle => (WS_EX) (long) WinAPI.GetWindowLongPtr(Hwnd, WinAPI.WindowLongFlags.GWL_EXSTYLE);
        /// <summary>The percentage of how see-through the window is. Uses a cached value.</summary>
        public double Opacity => opacity == null ? FetchCache(Hwnd)?.opacity ?? 1 : (double) opacity;
        /// <summary>The color of the window that is rendered as fully transparent. Uses a cached value.</summary>
        public Color Transcolor => transcolor == null ? FetchCache(Hwnd)?.transcolor ?? default : (Color) transcolor;
        /// <summary>Check if a window has a region</summary>
        public bool HasRegion => GetRegionBounds() != null;
        /// <summary>Check the type of the region</summary>
        public WinAPI.RegionType RegionType => WinAPI.GetWindowRgnBox(Hwnd, out _);
        #endregion

        #region positions
        /// <summary>A corrected version of the window's area</summary>
        public Area Area => GetArea();
        /// <summary>The area of the window as given by the OS</summary>
        public Area RawArea => GetRawArea();
        /// <summary>The client area of the window. Excludes the caption and the borders</summary>
        public Area ClientArea => GetClientArea();
        /// <summary>The visible area of the window when in borderless mode</summary>
        public Area BorderlessArea => GetBorderlessArea();
        /// <summary>Get the bounding area of the current region. Relative to raw window coordinates</summary>
        public Area? RegionBounds => GetRegionBounds();
        #endregion

        #region static
        /// <summary>A list of borderless settings that direct window behaviour when setting to borderless mode</summary>
        public static List<BorderlessInfo> BorderlessSettings { get; set; } = new List<BorderlessInfo>();

        /// <summary>A list of matches for known top level windows that are not matched by the IsTopLevel heuristic</summary>
        internal static Dictionary<Window, WinMatch> TopLevelWhitelist { get; set; } = new Dictionary<Window, WinMatch>();

        /// <summary>Contains the cached windows from the last time the windows were enumerated</summary>
        public static Dictionary<IntPtr, Window> CachedWindows { get; private set; } = new Dictionary<IntPtr, Window>();

        /// <summary>A window object that doesn't point to any window</summary>
        public static Window None => new Window(IntPtr.Zero);
        /// <summary>Retrieves the active window</summary>
        public static Window Active => new Window(WinAPI.GetForegroundWindow());
        /// <summary>Retrieves the first window under the mouse</summary>
        public static Window FromMouse => FromPoint(Mouse.Position);
        /// <summary>Retrieves the current process's windows</summary>
        public static List<Window> This => GetWindows(new WinMatch(pid: (uint) Process.GetCurrentProcess().Id), WinFindMode.All);
        #endregion

        #region other
        /// <summary>Check if the hwnd is zero meaning it points to nothing</summary>
        public bool IsNone => Hwnd == IntPtr.Zero;
        /// <summary>Check if the object points to a real window. Also validates deserialized objects in case hwnd values were recycled by the OS by comparing the class name.</summary>
        public bool IsValid => Hwnd != IntPtr.Zero && Exists && Class == new Window(Hwnd).Class;

        /// <summary>The parent of the window</summary>
        public Window Parent => new Window(WinAPI.GetWindowLongPtr(Hwnd, WinAPI.WindowLongFlags.GWLP_HWNDPARENT));
        /// <summary>The topmost window in the window's parent chain</summary>
        public Window Ancestor => new Window(WinAPI.GetAncestor(Hwnd, WinAPI.AncestorFlags.GetRoot));
        /// <summary>The topmost window in the window's parent chain on a deeper level than <see cref="Ancestor"/></summary>
        public Window Owner => new Window(WinAPI.GetAncestor(Hwnd, WinAPI.AncestorFlags.GetRootOwner));
        /// <summary>Retrieves all window of the same process.</summary>
        public List<Window> Siblings => GetWindows(new WinMatch(pid: PID), WinFindMode.All);

        /// <summary>Handle of the <see cref="WinUtilities.Monitor"/> the window is on</summary>
        public Monitor Monitor => Monitor.FromWindow(this);
        /// <summary>Get the id of the virtual desktop the window is on</summary>
        public Guid Desktop => SimpleDesktop.GetDesktopID(this);
        /// <summary>Get a match object that only matches this window</summary>
        public WinMatch AsMatch => new WinMatch(hwnd: Hwnd);
        /// <summary>Get the application specific volume controller</summary>
        public AppVolume Audio => WinAudio.GetApp(PID) ?? WinAudio.GetApp(Exe);
        #endregion

        #endregion

        #region events
        /// <summary>This event triggers every time the Move function is called</summary>
        public static event Action<Window, Area, CoordType> OnMove;
        #endregion

        #region constructors
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        private Window() { }
        public Window(IntPtr hwnd) => Hwnd = hwnd;
        /// <summary>Create a Window object using its string representation, like "Window:123456"</summary>
        public Window(string s) {
            if (!s.StartsWith("Window:"))
                throw new ArgumentException("Given string is not a window identifier");
            Hwnd = new IntPtr(int.Parse(s.Split(':')[1]));
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #endregion

        #region instance

        #region positions
        private Area GetArea() => CalculateRealArea();
        private Area GetArea(CoordType type) {
            if (type == CoordType.Normal)
                return GetArea();
            if (type == CoordType.Client)
                return GetClientArea();
            return GetRawArea();
        }

        private void SetArea(Area area, Area? _raw = null, Area? _client = null) {
            Area? region = GetRegionBounds();
            bool borderless = region != null;
            Area raw = _raw ?? GetRawArea();
            Area client = _client ?? GetClientArea();
            Area real = CalculateRealArea(raw, client, borderless ? region : null);

            OffsetMove(area, real, raw);

            if (borderless) {
                SetRegion(region.Value.AddSize(area - real));
            }
        }

        private Area GetRawArea() {
            WinAPI.RECT rect = new WinAPI.RECT();
            WinAPI.GetWindowRect(Hwnd, ref rect);
            return rect;
        }

        private void SetRawArea(Area area) {
            Area target = area.Round();
            WinAPI.SetWindowPos(Hwnd, IntPtr.Zero, target.IntX, target.IntY, target.IntW, target.IntH, WinAPI.WindowPosFlags.NoZOrder | WinAPI.WindowPosFlags.NoActivate);
        }

        private Area GetClientArea() {
            WinAPI.POINT point = new WinAPI.POINT();
            WinAPI.RECT rect = new WinAPI.RECT();

            WinAPI.ClientToScreen(Hwnd, ref point);
            WinAPI.GetClientRect(Hwnd, ref rect);

            return new Area(point.X, point.Y, rect.Width, rect.Height);
        }

        private void SetClientArea(Area area, Area? _client = null) {
            OffsetMove(area, _client ?? GetClientArea());
        }

        private Area GetBorderlessArea() {
            if (!IsBorderless)
                return CalculateBorderlessArea(GetClientArea());
            return CalculateRegionArea(GetRawArea());
        }

        private void SetBorderlessArea(Area area) {
            if (!IsBorderless)
                OffsetMove(area, GetBorderlessArea());
            SetArea(area);
        }

        /// <summary>Move a window by using an offset</summary>
        private Window OffsetMove(Area pos, Area offset) => OffsetMove(pos, offset, RawArea);
        /// <summary>Move a window by using an offset</summary>
        private Window OffsetMove(Area pos, Area offset, Area raw) {
            pos += raw - offset;
            pos = pos.Round();
            WinAPI.SetWindowPos(Hwnd, IntPtr.Zero, pos.IntX, pos.IntY, pos.IntW, pos.IntH, WinAPI.WindowPosFlags.NoZOrder | WinAPI.WindowPosFlags.NoActivate);
            return this;
        }

        private Area? GetRegionBounds() {
            if (HasStyle(WS.BORDER)) {
                return null;
            }

            for (int i = 0; i < 5; i++) {
                if (WinAPI.GetWindowRgnBox(Hwnd, out WinAPI.RECT rect) != WinAPI.RegionType.Error) {
                    return rect;
                }
            }

            return null;
        }

        /// <summary>Attempt at reusing area information because getting them is somewhat costly</summary>
        private Area CalculateRealArea(Area? raw = null, Area? client = null, Area? region = null) {
            if (IsMaximized) {
                return Monitor.WorkArea;
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
            Area realRaw = raw == null ? GetRawArea() : (Area) raw;
            Area? realRegion = region ?? GetRegionBounds();
            return realRegion != null ? ((Area) realRegion).AddPoint(realRaw) : realRaw;
        }
        #endregion

        #region queries
        /// <summary>Check if the window matches with the given description.</summary>
        public bool Match(IWinMatch match) => match?.Match(this) ?? false;
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
        /// <summary>Set this window as the foreground window</summary>
        public Window Activate(WinActivateMode policy = WinActivateMode.SoftThenForce) {
            if (IsNone)
                return this;
            if (IsActive)
                return this;
            if (policy == WinActivateMode.Soft)
                WinAPI.SetForegroundWindow(Hwnd);
            else if (policy == WinActivateMode.Force)
                ActivateForce();
            else
                _ = ActivateComplex();
            return this;
        }

        /// <summary>Set the window as the foreground window and wait for the operation to finish. Returns true on success.</summary>
        public async Task<bool> ActivateAsync(WinActivateMode policy = WinActivateMode.SoftThenForce) {
            if (IsNone)
                return false;
            if (IsActive)
                return true;
            if (policy == WinActivateMode.Soft)
                return await ActivateSimple(1);
            if (policy == WinActivateMode.SoftThenForce)
                return await ActivateComplex();
            ActivateForce();
            await Task.Delay(1);
            return Active == this || Active == Owner;
        }

        /// <summary>Forceful window activation</summary>
        private Window ActivateForce(bool alternate = false) {
            if (IsNone)
                return this;
            if (IsActive)
                return this;

            uint curthread = WinAPI.GetCurrentThreadId();
            uint forethread = Active.ThreadID;
            uint winthread = ThreadID;

            if (forethread != curthread)
                WinAPI.AttachThreadInput(curthread, forethread, true);
            if (forethread != winthread)
                WinAPI.AttachThreadInput(forethread, winthread, true);

            if (!alternate) {
                WinAPI.SetForegroundWindow(Hwnd);
            } else {
                WinAPI.BringWindowToTop(Hwnd);
                WinAPI.ShowWindow(Hwnd, WinAPI.SW.SHOW);
            }

            if (forethread != curthread)
                WinAPI.AttachThreadInput(curthread, forethread, false);
            if (forethread != winthread)
                WinAPI.AttachThreadInput(forethread, winthread, false);

            return this;
        }

        /// <summary>A complex but more reliable window activation</summary>
        private async Task<bool> ActivateComplex(bool alternate = false) {
            if (IsNone)
                return false;
            int checkDelay = 1;
            var targetThread = ThreadID;
            var currentThread = WinAPI.GetCurrentThreadId();

            if (targetThread != currentThread && WinAPI.IsHungAppWindow(Hwnd))
                return false;
            if (IsMinimized)
                Restore();
            if (await ActivateSimple(checkDelay))
                return true;

            bool isAttachedToFore = false;
            bool isAttachedToTarget = false;
            var foreWin = Active;
            uint foreThread = 0;

            if (!foreWin.IsNone) {
                foreThread = foreWin.ThreadID;
                if (foreThread != 0 && currentThread != foreThread && !WinAPI.IsHungAppWindow(foreWin.Hwnd))
                    isAttachedToFore = WinAPI.AttachThreadInput(currentThread, foreThread, true);
                if (foreThread != 0 && targetThread != 0 && foreThread != targetThread)
                    isAttachedToTarget = WinAPI.AttachThreadInput(foreThread, targetThread, true);
            }

            bool success = false;
            for (int i = 0; i < 4; i++) {
                if (!alternate) {
                    success = await ActivateSimple(checkDelay);
                } else {
                    WinAPI.BringWindowToTop(Hwnd);
                    WinAPI.ShowWindow(Hwnd, WinAPI.SW.SHOW);
                    await Task.Delay(checkDelay);
                    var a = Active;
                    success = a == this || a == Owner;
                }

                if (success) {
                    break;
                }
            }

            if (isAttachedToFore)
                WinAPI.AttachThreadInput(currentThread, foreThread, false);
            if (isAttachedToTarget)
                WinAPI.AttachThreadInput(foreThread, targetThread, false);
            if (success)
                return true;
            return false;
        }

        /// <summary>Activate a window and check if it succeeded</summary>
        private async Task<bool> ActivateSimple(int checkDelay) {
            if (IsNone)
                return false;
            WinAPI.SetForegroundWindow(Hwnd);
            await Task.Delay(checkDelay);
            var newForeWindow = Active;
            if (newForeWindow == this)
                return true;
            if (newForeWindow == Owner)
                return true;
            return false;
        }

        /// <summary>If active, Move the window to the bottom and activate the highest window</summary>
        public Window Deactivate() {
            if (IsNone)
                return this;
            if (!IsActive)
                return this;
            var next = Find(w => w.IsTopLevel && !w.IsTopmost && w.IsOnCurrentDesktop && w != this);

            if (IsTopmost)
                MoveUnder(GetWindows(w => w.IsTopLevel).Last(w => w.IsTopmost) ?? None);
            else
                MoveBottom();

            if (next != None)
                next.Activate();
            else
                Find(WinGroup.Taskbar).Activate();
            return this;
        }
        /// <summary>Enable/disable the window. Disabled windows cannot be interacted with.</summary>
        public Window Enable(bool state) {
            if (IsNone)
                return this;
            WinAPI.EnableWindow(Hwnd, state);
            return this;
        }
        /// <summary>Kill the process associated with the window</summary>
        public Window Kill() {
            if (IsNone)
                return this;
            Process.Kill();
            return this;
        }
        /// <summary>Send a request to close to the window</summary>
        public Window Close() {
            if (IsNone)
                return this;
            Ancestor.PostMessage(WM.CLOSE, 0, 0);
            return this;
        }
        /// <summary>Minimize the window</summary>
        public Window Minimize() {
            if (IsNone)
                return this;
            if (IsMinimized)
                return this;
            WinAPI.ShowWindow(Hwnd, WinAPI.SW.MINIMIZE);
            if (IsActive)
                Deactivate();
            return this;
        }
        /// <summary>Maximize the window</summary>
        public Window Maximize() {
            if (IsNone)
                return this;
            if (IsMaximized)
                return this;
            WinAPI.ShowWindow(Hwnd, WinAPI.SW.MAXIMIZE);
            return this;
        }
        /// <summary>Restore the window from a minimized or a maximized state to normal</summary>
        public Window Restore() {
            if (IsNone)
                return this;
            if (IsMinimized || IsMaximized)
                WinAPI.ShowWindow(Hwnd, WinAPI.SW.RESTORE);
            return this;
        }
        /// <summary>Set window visibility. False hides the window from the user completely. It's more complex than simple transparency.</summary>
        public Window SetVisible(bool state) {
            if (IsNone)
                return this;
            WinAPI.ShowWindow(Hwnd, state ? WinAPI.SW.SHOWNA : WinAPI.SW.HIDE);
            return this;
        }
        /// <summary>Normally hidden windows often have weird alternate behaviour. This version is less prone to that while not 'truly' hiding a window.</summary>
        public Window SetVisibleSoft(bool state) {
            if (IsNone)
                return this;
            SetClickThrough(!state);
            return SetOpacity(state ? 100 : 0);
        }

        /// <summary>Post a message to the window's message pump. Returns true on success.</summary>
        public bool PostMessage(WM msg, int wParam, int lParam) => WinAPI.PostMessage(Hwnd, (uint) msg, (IntPtr) wParam, (IntPtr) lParam);
        /// <summary>Send a message the window's message pump. Waits for a reply from the window.</summary>
        public IntPtr SendMessage(WM msg, int wParam, int lParam) => WinAPI.SendMessage(Hwnd, (uint) msg, (IntPtr) wParam, (IntPtr) lParam);
        /// <summary>Set individual Window Styles on and off</summary>
        public Window SetStyle(WS style, bool state) {
            if (IsNone)
                return this;
            WS newStyle = state ? Style | style : Style & ~style;
            WinAPI.SetWindowLongPtr(Hwnd, WinAPI.WindowLongFlags.GWL_STYLE, (IntPtr) newStyle);
            WinAPI.SetWindowPos(Hwnd, IntPtr.Zero, 0, 0, 0, 0, WinAPI.WindowPosFlags.NoActivate | WinAPI.WindowPosFlags.NoMove | WinAPI.WindowPosFlags.NoSize | WinAPI.WindowPosFlags.NoZOrder | WinAPI.WindowPosFlags.FrameChanged);
            return this;
        }
        /// <summary>Set individual Window Ex Styles on and off</summary>
        public Window SetExStyle(WS_EX style, bool state) {
            if (IsNone)
                return this;
            WS_EX newStyle = state ? ExStyle | style : ExStyle & ~style;
            WinAPI.SetWindowLongPtr(Hwnd, WinAPI.WindowLongFlags.GWL_EXSTYLE, (IntPtr) newStyle);
            WinAPI.SetWindowPos(Hwnd, IntPtr.Zero, 0, 0, 0, 0, WinAPI.WindowPosFlags.NoActivate | WinAPI.WindowPosFlags.NoMove | WinAPI.WindowPosFlags.NoSize | WinAPI.WindowPosFlags.NoZOrder | WinAPI.WindowPosFlags.FrameChanged);
            return this;
        }

        /// <summary>Brings the window to the top of visibility</summary>
        public Window MoveTop() {
            if (IsNone)
                return this;
            WinAPI.SetWindowPos(Hwnd, (IntPtr) WinAPI.HWND_Z.TOP, 0, 0, 0, 0, WinAPI.WindowPosFlags.NoMove | WinAPI.WindowPosFlags.NoSize | WinAPI.WindowPosFlags.NoActivate);
            return this;
        }
        /// <summary>Drop the window to the bottom of visibility</summary>
        public Window MoveBottom() {
            if (IsNone)
                return this;
            WinAPI.SetWindowPos(Hwnd, (IntPtr) WinAPI.HWND_Z.BOTTOM, 0, 0, 0, 0, WinAPI.WindowPosFlags.NoMove | WinAPI.WindowPosFlags.NoSize | WinAPI.WindowPosFlags.NoActivate);
            return this;
        }
        /// <summary>Move this window under the specified window in visibility</summary>
        public Window MoveUnder(Window win) {
            if (IsNone)
                return this;
            WinAPI.SetWindowPos(Hwnd, win.Hwnd, 0, 0, 0, 0, WinAPI.WindowPosFlags.NoMove | WinAPI.WindowPosFlags.NoSize | WinAPI.WindowPosFlags.NoActivate);
            return this;
        }

        /// <summary>Alternative to <see cref="SetTopmost(bool)"/>. Make a window always stay visible.</summary>
        public Window SetAlwaysOnTop(bool state) => SetTopmost(state);
        /// <summary>Make a window always stay visible</summary>
        public Window SetTopmost(bool state) {
            if (IsNone)
                return this;
            IntPtr msg = state ? (IntPtr) WinAPI.HWND_Z.TOPMOST : (IntPtr) WinAPI.HWND_Z.NOTOPMOST;
            WinAPI.SetWindowPos(Hwnd, msg, 0, 0, 0, 0, WinAPI.WindowPosFlags.NoMove | WinAPI.WindowPosFlags.NoSize | WinAPI.WindowPosFlags.NoActivate);
            return this;
        }

        /// <summary>Make clicks phase through the window to the windows below</summary>
        public Window SetClickThrough(bool state) {
            if (IsNone)
                return this;
            if (state) {
                return SetExStyle(WS_EX.TRANSPARENT | WS_EX.LAYERED, true);
            } else {
                return SetExStyle(WS_EX.TRANSPARENT, false);
            }
        }

        /// <summary>Set the degree of see-through of the window in percentages</summary>
        public Window SetOpacity(double percentage) {
            if (IsNone)
                return this;
            if (!HasExStyle(WS_EX.LAYERED)) {
                SetExStyle(WS_EX.LAYERED, true);
            }

            percentage = Math.Min(Math.Max(percentage, 0), 1);
            int alpha = (int) Math.Round(percentage * 255);

            WinAPI.SetLayeredWindowAttributes(Hwnd, 0, (byte) alpha, WinAPI.LayeredWindowFlags.LWA_ALPHA);

            opacity = alpha / 255.0;
            if (FetchCache(Hwnd) is Window win)
                win.opacity = opacity;
            else
                CachedWindows.Add(Hwnd, this);
            return this;
        }

        /// <summary>Set the color of the window that is rendered as fully transparent</summary>
        public Window SetTranscolor(Color color) {
            if (IsNone)
                return this;
            if (!HasExStyle(WS_EX.LAYERED)) {
                SetExStyle(WS_EX.LAYERED, true);
            }

            var c = new WinAPI.COLORREF(color);

            WinAPI.SetLayeredWindowAttributes(Hwnd, c.ColorDWORD, 0, WinAPI.LayeredWindowFlags.LWA_COLORKEY);

            transcolor = color;
            if (FetchCache(Hwnd) is Window win)
                win.transcolor = transcolor;
            else
                CachedWindows.Add(Hwnd, this);
            return this;
        }

        /// <summary>Fully disable transparency. Might improve performance after window transparency has been tweaked.</summary>
        public Window DisableTransparency() {
            if (IsNone)
                return this;
            opacity = null;
            transcolor = null;

            if (FetchCache(Hwnd) is Window win) {
                win.opacity = opacity;
                win.transcolor = transcolor;
            }

            return SetExStyle(WS_EX.LAYERED | WS_EX.TRANSPARENT, false);
        }

        /// <summary>Set the parent window of this window</summary>
        public Window SetParent(Window window) => SetParent(window, out _);

        /// <summary>Set the parent window of this window</summary>
        public Window SetParent(Window window, out Window prevParent) {
            prevParent = null;
            if (IsNone)
                return this;
            prevParent = new Window(WinAPI.SetParent(Hwnd, window?.Hwnd ?? IntPtr.Zero));
            return this;
        }

        /// <summary>Set the owner of this window</summary>
        public Window SetOwner(Window window) {
            if (IsNone)
                return this;
            if (IsChild)
                SetParent(null);
            WinAPI.SetWindowLongPtr(Hwnd, WinAPI.WindowLongFlags.GWLP_HWNDPARENT, window?.Hwnd ?? IntPtr.Zero);
            return this;
        }

        /// <summary>Disable the resizing limits of the window.</summary>
        public Window UnlockSize() {
            if (IsNone)
                return this;
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
            if (x == null || y == null || w == null || h == null) {
                Area current = GetArea(type);
                x = x ?? current.IntX;
                y = y ?? current.IntY;
                w = w ?? current.IntW;
                h = h ?? current.IntH;
            }

            return Move(new Area(x.Value, y.Value, w.Value, h.Value), type);
        }

        /// <summary>Move the window to the new coordinates.</summary>
        /// <param name="point">Location of the window. Null to not move the window.</param>
        /// <param name="size">Size of the window. Null to not resize the window.</param>
        /// <param name="type">Set what the coordinates are relative to.</param>
        public Window Move(Coord? point = null, Coord? size = null, CoordType type = CoordType.Normal) {
            if (point == null || size == null) {
                Area current = GetArea(type);
                point = point ?? current.Point;
                size = size ?? current.Size;
            }

            return Move(new Area(point.Value, size.Value), type);
        }

        /// <summary>Move the window to the new coordinates.</summary>
        /// <param name="area">The target area of the window.</param>
        /// <param name="type">Set what the coordinates are relative to.</param>
        //public Window Move(Area area, CoordType type = CoordType.Normal) => Move(area, type, null);
        public Window Move(Area area, CoordType type = CoordType.Normal) {
            if (IsNone)
                return this;
            if (type == CoordType.Normal) {
                SetArea(area);
            } else if (type == CoordType.Client) {
                SetClientArea(area);
            } else {
                SetRawArea(area);
            }

            if (OnMove != null)
                Task.Run(() => OnMove.Invoke(this, area, type));

            return this;
        }

        /// <summary>Center the window to the specified monitor</summary>
        /// <param name="ignoreTaskbar">Set to true to ignore the space taken by the taskbar when calculating centering</param>
        public Window Center(bool ignoreTaskbar = false) => Center(null, null, ignoreTaskbar);
        /// <summary>Center the window to the specified monitor</summary>
        /// <param name="size">Set the target size of the window before centering</param>
        /// <param name="ignoreTaskbar">Set to true to ignore the space taken by the taskbar when calculating centering</param>
        public Window Center(Coord size, bool ignoreTaskbar = false) => Center(null, size, ignoreTaskbar);
        /// <summary>Center the window to the specified monitor</summary>
        /// <param name="monitor">Index of the target monitor. Zero based indexing.</param>
        /// <param name="size">Set the target size of the window before centering</param>
        /// <param name="ignoreTaskbar">Set to true to ignore the space taken by the taskbar when calculating centering</param>
        public Window Center(int monitor, Coord? size = null, bool ignoreTaskbar = false) => Center(Monitor.FromIndex(monitor), size, ignoreTaskbar);
        /// <summary>Center the window to the specified monitor</summary>
        /// <param name="monitor">Null targets current monitor of the window</param>
        /// <param name="size">Set the target size of the window before centering</param>
        /// <param name="ignoreTaskbar">Set to true to ignore the space taken by the taskbar when calculating centering</param>
        public Window Center(Monitor monitor, Coord? size = null, bool ignoreTaskbar = false) => Center(ignoreTaskbar ? (monitor ?? Monitor).Area : (monitor ?? Monitor).WorkArea, size);
        /// <summary>Center the window to the specified area</summary>
        /// <param name="area">Area in which the window is centered to</param>
        /// <param name="size">Set the target size of the window before centering</param>
        public Window Center(Area area, Coord? size = null) => Move(new Area(area.Center - (size ?? Area.Size) / 2, size ?? Area.Size));
        #endregion

        #region desktops
        /// <summary>Move the window to a virtual desktop with the specifid id</summary>
        public Window MoveToDesktop(Guid desktop) {
            if (IsNone)
                return this;
            SimpleDesktop.MoveWindow(this, desktop);
            return this;
        }
        #endregion

        #region regions
        /// <summary>Set only a specified area of a window visible.</summary>
        /// <param name="region">Relative to raw window coordinates.</param>
        public Window SetRegion(Area region) {
            if (IsNone)
                return this;
            region = region.Round();
            var r = WinAPI.CreateRectRgn((int) region.Left, (int) region.Top, (int) region.Right, (int) region.Bottom);
            WinAPI.SetWindowRgn(Hwnd, r, true);
            WinAPI.DeleteObject(r);
            return this;
        }

        /// <summary>Set only a specified area of a window visible. Has rounded corners.</summary>
        /// <param name="region">Relative to raw window coordinates.</param>
        /// <param name="horizontalRounding">Amount of horizontal rounding</param>
        /// <param name="verticalRounding">Amount of vertical rounding</param>
        public Window SetRoundedRegion(Area region, int horizontalRounding, int verticalRounding) {
            if (IsNone)
                return this;
            region = region.Round();
            var r = WinAPI.CreateRoundRectRgn((int) region.Left, (int) region.Top, (int) region.Right, (int) region.Bottom, horizontalRounding, verticalRounding);
            WinAPI.SetWindowRgn(Hwnd, r, true);
            WinAPI.DeleteObject(r);
            return this;
        }

        /// <summary>Set only a specified area of a window visible. Has an elliptic shape.</summary>
        /// <param name="region">Relative to raw window coordinates.</param>
        public Window SetEllipticRegion(Area region) {
            if (IsNone)
                return this;
            region = region.Round();
            var r = WinAPI.CreateEllipticRgn((int) region.Left, (int) region.Top, (int) region.Right, (int) region.Bottom);
            WinAPI.SetWindowRgn(Hwnd, r, true);
            WinAPI.DeleteObject(r);
            return this;
        }

        /// <summary>Set only a specified area of a window visible. Create a region with multiple areas.</summary>
        /// <param name="regions">Relative to raw window coordinates.</param>
        public Window SetComplexRegion(params Area[] regions) {
            if (IsNone)
                return this;
            if (regions.Length == 0) {
                throw new ArgumentException("Must have at least 2 rectangles specified");
            } else if (regions.Length == 1) {
                return SetRegion(regions[0]);
            }

            IntPtr full = IntPtr.Zero;
            IntPtr[] r = new IntPtr[regions.Length];
            for (int i = 0; i < regions.Length; i++) {
                regions[i] = regions[i].Round();
                r[i] = WinAPI.CreateRectRgn((int) regions[i].Left, (int) regions[i].Top, (int) regions[i].Right, (int) regions[i].Bottom);
            }

            foreach (var item in r) {
                WinAPI.CombineRgn(full, full, item, WinAPI.CombineRgnFlags.Or);
            }

            WinAPI.SetWindowRgn(Hwnd, full, true);

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
            if (IsNone)
                return this;
            if (points.Length < 3) {
                throw new ArgumentException("Must have at least 3 points to make a polygon shape");
            }

            for (int i = 0; i < points.Length; i++) {
                points[i] = points[i].Round();
            }

            var region = WinAPI.CreatePolygonRgn(points.Cast<WinAPI.POINT>().ToArray(), points.Length, fillType);

            WinAPI.SetWindowRgn(Hwnd, region, true);
            WinAPI.DeleteObject(region);
            return this;
        }

        /// <summary>Remove the window's region to display the full window.</summary>
        public Window RemoveRegion() {
            if (IsNone)
                return this;
            WinAPI.SetWindowRgn(Hwnd, IntPtr.Zero, true);
            return this;
        }
        #endregion

        #region borderless
        /// <summary>Set the window to borderless mode.</summary>
        public Window SetBorderless(bool state) {
            if (IsNone)
                return this;
            var style = WS.BORDER | WS.SIZEFRAME | WS.DLGFRAME;

            if (state) {
                var area = Area;
                SetStyle(style, false);
                PostMessage(WM.SIZE, 0, 0);
                SetRegion(CalculateBorderlessRegion(RawArea, ClientArea));
                SetArea(area);
            } else {
                var area = Area;
                SetStyle(style, true);
                PostMessage(WM.SIZE, 0, 0);
                RemoveRegion();
                SetArea(area);
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
        public Image GetImage(bool clientOnly = false) => GetImage(Area.Zero, clientOnly);
        /// <summary>Get a cropped image of the window using BitBlt</summary>
        /// <param name="subArea">Set the capture sub area relative to the full capture area</param>
        /// <param name="clientOnly">Capture only the client area</param>
        public Image GetImage(Area subArea, bool clientOnly = false) {
            if (IsNone)
                return null;
            Area area = Area;
            Area client = ClientArea;
            Area capture;

            if (subArea.IsZero) {
                Area raw = RawArea;
                Area fullArea = clientOnly ? client : area;
                if (!fullArea.Contains(subArea.AddPoint(fullArea.Point)))
                    throw new ArgumentException("Given sub area does not fit within the capture area");
                capture = subArea.AddPoint(fullArea.Point - raw.Point);
            } else {
                capture = area.SetPoint(new Coord(area.X == client.X ? 0 : borderWidth - borderVisibleWidth, 0));
            }

            // get te hDC of the target window
            IntPtr hdcSrc = WinAPI.GetWindowDC(Hwnd);
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
            WinAPI.ReleaseDC(Hwnd, hdcSrc);
            // get a .NET image object for it
            Image img = Image.FromHbitmap(hBitmap);
            // free up the Bitmap object
            WinAPI.DeleteObject(hBitmap);

            return img;
        }

        /// <summary>Get an image of the window using WindowPrint API. Capable of imaging off screen windows.</summary>
        public Image GetImagePrint(bool clientOnly = false) {
            if (IsNone)
                return null;
            Area area = clientOnly ? ClientArea : Area;
            Image img = new Bitmap(area.IntW, area.IntH);
            Graphics g = Graphics.FromImage(img);
            IntPtr dc = g.GetHdc();
            WinAPI.PrintWindow(Hwnd, dc, clientOnly);
            g.ReleaseHdc();
            g.Dispose();
            return img;
        }

        /// <summary>Get an image of the window based on what's visible on the desktop currently</summary>
        /// <param name="clientOnly">Capture only the client area</param>
        public Image GetImageDesktop(bool clientOnly = false) => GetImageDesktop(Area.Zero, clientOnly);
        /// <summary>Get a cropped image of the window based on what's visible on the desktop currently</summary>
        /// <param name="subArea">Set the capture sub area relative to the full capture area</param>
        /// <param name="clientOnly">Capture only the client area</param>
        public Image GetImageDesktop(Area subArea, bool clientOnly = false) {
            if (IsNone)
                return null;
            var area = clientOnly ? ClientArea : Area;

            if (subArea.IsZero) {
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
        public Window Click(Coord pos, Key key = Key.LButton, CoordRelation rel = CoordRelation.Window) {
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
                pos = pos.SetRelative(RawArea);
            } else if (rel == CoordRelation.Mouse) {
                pos = RawArea.Point + Mouse.Position + pos;
            }

            PostMessage(down, 0, pos.AsValue);
            PostMessage(up, 0, pos.AsValue);
            return this;
        }

        /// <summary>Emulates a right click at the specified position.</summary>
        public Window RightClick(Coord pos, CoordRelation rel = CoordRelation.Window) => Click(pos, Key.RButton, rel);

        /// <summary>Emulates a middle click at the specified position.</summary>
        public Window MiddleClick(Coord pos, CoordRelation rel = CoordRelation.Window) => Click(pos, Key.MButton, rel);

        /// <summary>Emulates a click at the specified position. Tries to prevent window activation.</summary>
        public async Task ClickNA(Coord pos, Key key = Key.LButton, CoordRelation rel = CoordRelation.Window) {
            var na = !HasExStyle(WS_EX.NOACTIVATE);
            if (na) SetExStyle(WS_EX.NOACTIVATE, true);
            await Task.Delay(1);
            Click(pos, key, rel);
            await Task.Delay(1);
            if (na) SetExStyle(WS_EX.NOACTIVATE, false);
        }

        /// <summary>Emulates a click at the specified position.</summary>
        public Window DoubleClick(Coord pos, Key key = Key.LButton, CoordRelation rel = CoordRelation.Window) {
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
                pos = pos.SetRelative(RawArea);
            } else if (rel == CoordRelation.Mouse) {
                pos = RawArea.Point + Mouse.Position + pos;
            }

            PostMessage(msg, 0, pos.AsValue);
            return this;
        }

        /// <summary>Emulates a click at the specified position. Tries to prevent window activation.</summary>
        public async Task DoubleClickNA(Coord pos, Key key = Key.LButton, CoordRelation rel = CoordRelation.Window) {
            var na = !HasExStyle(WS_EX.NOACTIVATE);
            if (na) SetExStyle(WS_EX.NOACTIVATE, true);
            await Task.Delay(1);
            DoubleClick(pos, key, rel);
            await Task.Delay(1);
            if (na) SetExStyle(WS_EX.NOACTIVATE, false);
        }

        /// <summary>Emulates a click at the specified position.</summary>
        public Window ClickDown(Coord pos, Key key = Key.LButton, CoordRelation rel = CoordRelation.Window) {
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
                pos = pos.SetRelative(RawArea);
            } else if (rel == CoordRelation.Mouse) {
                pos = RawArea.Point + Mouse.Position + pos;
            }

            PostMessage(msg, 0, pos.AsValue);
            return this;
        }

        /// <summary>Emulates a click at the specified position. Tries to prevent window activation.</summary>
        public async Task ClickDownNA(Coord pos, Key key = Key.LButton, CoordRelation rel = CoordRelation.Window) {
            var na = !HasExStyle(WS_EX.NOACTIVATE);
            if (na) SetExStyle(WS_EX.NOACTIVATE, true);
            await Task.Delay(1);
            ClickDown(pos, key, rel);
            await Task.Delay(1);
            if (na) SetExStyle(WS_EX.NOACTIVATE, false);
        }

        /// <summary>Emulates a click at the specified position.</summary>
        public Window ClickUp(Coord pos, Key key = Key.LButton, CoordRelation rel = CoordRelation.Window) {
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
                pos = pos.SetRelative(RawArea);
            } else if (rel == CoordRelation.Mouse) {
                pos = RawArea.Point + Mouse.Position + pos;
            }

            PostMessage(msg, 0, pos.AsValue);
            return this;
        }

        /// <summary>Emulates a click at the specified position. Tries to prevent window activation.</summary>
        public async Task ClickUpNA(Coord pos, Key key = Key.LButton, CoordRelation rel = CoordRelation.Window) {
            var na = !HasExStyle(WS_EX.NOACTIVATE);
            if (na) SetExStyle(WS_EX.NOACTIVATE, true);
            await Task.Delay(1);
            ClickUp(pos, key, rel);
            await Task.Delay(1);
            if (na) SetExStyle(WS_EX.NOACTIVATE, false);
        }

        /// <summary>Emulates a mouse move event at the specified position.</summary>
        public Window MouseMove(Coord pos, CoordRelation rel = CoordRelation.Window) {
            if (rel == CoordRelation.Screen) {
                pos = pos.SetRelative(RawArea);
            } else if (rel == CoordRelation.Mouse) {
                pos = RawArea.Point + Mouse.Position + pos;
            }

            PostMessage(WM.MOUSEMOVE, 0, pos.AsValue);
            return this;
        }
        #endregion

        #region flashing
        /// <summary>Stop a window from flashing</summary>
        public Window FlashStop() {
            if (IsNone)
                return this;
            var data = new WinAPI.FLASHWINFO {
                hwnd = Hwnd,
                cbSize = (uint) Marshal.SizeOf(typeof(WinAPI.FLASHWINFO)),
                dwFlags = WinAPI.FlashWF.STOP,
                uCount = 0,
                dwTimeout = 0
            };

            WinAPI.FlashWindowEx(data);
            return this;
        }

        /// <summary>Flash a window to attract attention to it until activated</summary>
        public Window Flash(FlashF target = FlashF.Taskbar, int flashDelayMS = 0) {
            if (IsNone)
                return this;
            var data = new WinAPI.FLASHWINFO {
                hwnd = Hwnd,
                cbSize = (uint) Marshal.SizeOf(typeof(WinAPI.FLASHWINFO)),
                dwFlags = WinAPI.FlashWF.TIMERNOFG | GetFlashTarget(target),
                uCount = 0,
                dwTimeout = (uint) flashDelayMS
            };

            WinAPI.FlashWindowEx(data);
            return this;
        }

        /// <summary>Flash a window to attract attention to it <paramref name="count"/> times</summary>
        public Window FlashCount(int count, FlashF target = FlashF.Taskbar, int flashDelayMS = 1000) {
            if (IsNone)
                return this;
            var data = new WinAPI.FLASHWINFO {
                hwnd = Hwnd,
                cbSize = (uint) Marshal.SizeOf(typeof(WinAPI.FLASHWINFO)),
                dwFlags = WinAPI.FlashWF.TIMERNOFG | GetFlashTarget(target),
                uCount = 0,
                dwTimeout = (uint) flashDelayMS
            };

            WinAPI.FlashWindowEx(data);
            Disable();
            return this;

            async void Disable() {
                await Task.Delay(count * flashDelayMS * 2 - flashDelayMS);
                data.dwFlags = WinAPI.FlashWF.STOP;
                data.dwTimeout = 0;
                WinAPI.FlashWindowEx(data);
            }
        }

        /// <summary>Flash a window to attract attention to it for <paramref name="duration"/> milliseconds</summary>
        public Window Flash(int duration, FlashF target = FlashF.Taskbar, int flashDelayMS = 0) {
            if (IsNone)
                return this;
            var data = new WinAPI.FLASHWINFO {
                hwnd = Hwnd,
                cbSize = (uint) Marshal.SizeOf(typeof(WinAPI.FLASHWINFO)),
                dwFlags = WinAPI.FlashWF.TIMERNOFG | GetFlashTarget(target),
                uCount = 0,
                dwTimeout = (uint) flashDelayMS
            };

            WinAPI.FlashWindowEx(data);
            Disable();
            return this;

            async void Disable() {
                await Task.Delay(duration);
                data.dwFlags = WinAPI.FlashWF.STOP;
                data.dwTimeout = 0;
                WinAPI.FlashWindowEx(data);
            }
        }

        private WinAPI.FlashWF GetFlashTarget(FlashF target) {
            switch (target) {
            case FlashF.Titlebar:
                return WinAPI.FlashWF.CAPTION;
            case FlashF.Both:
                return WinAPI.FlashWF.ALL;
            case FlashF.Taskbar:
                return WinAPI.FlashWF.TRAY;
            default:
                return WinAPI.FlashWF.TRAY;
            }
        }
        #endregion

        #endregion

        #region static

        #region find
        /// <summary>Find a top level window that matches the given predicate</summary>
        /// <returns><see cref="None"/> if nothing was found</returns>
        public static Window FindTopLevel(Func<Window, bool> predicate) => Find(w => w.IsTopLevel && predicate.Invoke(w));
        /// <summary>Find a window that matches the given predicate</summary>
        /// <returns><see cref="None"/> if nothing was found</returns>
        public static Window Find(Func<Window, bool> predicate) => Find(predicate, null);
        /// <summary>Find a window that matches the given predicate. Can ignore specific windows and speed up search with a HashSet.</summary>
        /// <returns><see cref="None"/> if nothing was found</returns>
        public static Window Find(Func<Window, bool> predicate, HashSet<Window> ignore) {
            Window found = None;
            var windows = new Dictionary<IntPtr, Window>();
            WinAPI.EnumWindows(Collector, IntPtr.Zero);
            CachedWindows = windows;
            return found;

            bool Collector(IntPtr hwnd, int lParam) {
                Window window = CachedWindows.ContainsKey(hwnd) ? CachedWindows[hwnd] : new Window(hwnd);
                windows.Add(hwnd, window);
                if (!found.IsNone)
                    return true;
                if (ignore?.Contains(window) ?? false)
                    return true;
                if (predicate(window))
                    found = window;
                return true;
            }
        }

        /// <summary>Find a window that matches the given description</summary>
        /// <returns><see cref="None"/> if nothing was found</returns>
        public static Window Find(IWinMatch match, WinFindMode mode = WinFindMode.TopLevel) => Find(w => MatchFilter(w, match, mode));
        /// <summary>Find a window that matches the given title</summary>
        /// <returns><see cref="None"/> if nothing was found</returns>
        public static Window Find(string title, WinFindMode mode = WinFindMode.TopLevel) => Find(new WinMatch(title: title), mode);
        /// <summary>Find a window that matches the given .exe name</summary>
        /// <returns><see cref="None"/> if nothing was found</returns>
        public static Window FindByExe(string exe, WinFindMode mode = WinFindMode.TopLevel) => Find(new WinMatch(exe: exe), mode);
        /// <summary>Find a window that matches the given class</summary>
        /// <returns><see cref="None"/> if nothing was found</returns>
        public static Window FindByClass(string className, WinFindMode mode = WinFindMode.TopLevel) => Find(new WinMatch(className: className), mode);
        /// <summary>Find a window whose process's id matches the given id</summary>
        /// <returns><see cref="None"/> if nothing was found</returns>
        public static Window FindByPid(uint pid, WinFindMode mode = WinFindMode.TopLevel) => Find(new WinMatch(pid: pid), mode);

        /// <summary>Find a matching window from a cached list of windows</summary>
        /// <returns><see cref="None"/> if nothing was found</returns>
        public static Window FindCached(Func<Window, bool> predicate) => CachedWindows.First(pair => predicate(pair.Value)).Value;
        /// <summary>Find a matching window from a cached list of windows</summary>
        /// <returns><see cref="None"/> if nothing was found</returns>
        public static Window FindCached(IWinMatch match, WinFindMode mode) => CachedWindows.First(pair => MatchFilter(pair.Value, match, mode)).Value;
        #endregion

        #region get windows
        /// <summary>Get all existing windows</summary>
        public static List<Window> GetWindows() {
            var result = new List<Window>();
            var windows = new Dictionary<IntPtr, Window>();
            WinAPI.EnumWindows(Collector, IntPtr.Zero);
            CachedWindows = windows;
            return result;

            bool Collector(IntPtr hwnd, int lParam) {
                var window = CachedWindows.ContainsKey(hwnd) ? CachedWindows[hwnd] : new Window(hwnd);
                windows.Add(hwnd, window);
                result.Add(window);
                return true;
            }
        }

        /// <summary>Find all windows that match the given predicate</summary>
        public static List<Window> GetWindows(Func<Window, bool> predicate) => GetWindows().Where(predicate).ToList();
        /// <summary>Find all windows depending on the mode used</summary>
        public static List<Window> GetWindows(WinFindMode mode) => GetWindows(w => MatchFilter(w, null, mode));
        /// <summary>Find all windows that match the given condition</summary>
        public static List<Window> GetWindows(IWinMatch match, WinFindMode mode = WinFindMode.TopLevel) => GetWindows(w => MatchFilter(w, match, mode));

        /// <summary>Find all windows that match the given predicate using a cached list of windows</summary>
        public static List<Window> GetWindowsCached(Func<Window, bool> predicate) => CachedWindows.Where(pair => predicate(pair.Value)).Select(x => x.Value).ToList();
        /// <summary>Find all windows that match the given condition using a cached list of windows</summary>
        public static List<Window> GetWindowsCached(IWinMatch match, WinFindMode mode) => CachedWindows.Where(pair => MatchFilter(pair.Value, match, mode)).Select(x => x.Value).ToList();

        private static bool MatchFilter(Window win, IWinMatch match, WinFindMode mode) {
            if (mode != WinFindMode.All && !win.IsTopLevel)
                return false;
            if (mode == WinFindMode.CurrentDesktop && !win.IsOnCurrentDesktop)
                return false;
            if (match != null && !match.Match(win))
                return false;
            return true;
        }
        #endregion

        #region winwait
        /// <summary>Wait for a matching window to exist</summary>
        /// <param name="match">Set what kind of window to wait for</param>
        /// <param name="timeout">Time until the wait fails</param>
        /// <param name="ignoreCurrent">If true, only new windows will be considered</param>
        /// <param name="checkDelay">Set how often the windows are scanned for the target window</param>
        public static async Task<Window> Wait(IWinMatch match, long? timeout = null, bool ignoreCurrent = true, int checkDelay = 10) {
            var watch = Stopwatch.StartNew();
            var existingWindowsSet = new HashSet<Window>();
            foreach (var item in GetWindows(match, WinFindMode.All))
                existingWindowsSet.Add(item);

            if (!ignoreCurrent && existingWindowsSet.Count > 0) {
                return existingWindowsSet.First();
            }

            await Task.Delay(checkDelay);
            while (watch.ElapsedMilliseconds < timeout) {
                var newWindows = GetWindows(match, WinFindMode.All);

                foreach (var win in newWindows)
                    if (!existingWindowsSet.Contains(win))
                        return win;

                await Task.Delay(checkDelay);
            }

            return null;
        }

        /// <summary>Wait for a matching window to become active</summary>
        /// <param name="match">Set what kind of window to wait for</param>
        /// <param name="timeout">Time until the wait fails</param>
        /// <param name="ignoreCurrent">If true, only new windows will be considered</param>
        /// <param name="checkDelay">Set how often the active window is checked for the target window</param>
        public static async Task<Window> WaitActive(IWinMatch match, long? timeout = null, bool ignoreCurrent = true, int checkDelay = 10) {
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

        #region other
        /// <summary>Add a window to a list of known top level windows (ignored if already seen as TopLevel)</summary>
        public static void AddKnownTopLevel(Window window) {
            if (!window || window.IsTopLevel)
                return;
            var match = new WinMatch(hwnd: window.Hwnd, className: window.Class, exe: window.Exe, type: WinMatchType.Full);
            if (!TopLevelWhitelist.ContainsKey(window))
                TopLevelWhitelist.Add(window, match);
            else
                TopLevelWhitelist[window] = match;
        }
        /// <summary>Check if given window is null or <see cref="None"/></summary>
        public static bool IsNullOrNone(Window window) => window == null || window.IsNone;
        /// <summary>Removes all entries from the list of cached windows</summary>
        public static void ClearCache() => CachedWindows = new Dictionary<IntPtr, Window>();
        /// <summary>Refresh the cache so it contains the freshest information of windows. Can be used occasionally to prevent the very unlikely window handle collisions.</summary>
        public static void RefreshCache() => CachedWindows = GetWindows().ToDictionary(w => w.Hwnd);
        /// <summary>Fetch a matching window from cache if found, otherwise return null</summary>
        public static Window FetchCache(IntPtr handle) => CachedWindows.ContainsKey(handle) ? CachedWindows[handle] : null;
        /// <summary>Get the handle of the topmost window of the given point</summary>
        public static Window FromPoint(int x, int y) => FromPoint(new Coord(x, y));
        /// <summary>Get the handle of the topmost window of the given point</summary>
        public static Window FromPoint(Coord point) => new Window(WinAPI.WindowFromPoint(point)).Ancestor;
        /// <summary>Check if a window with the specified handle exists</summary>
        private static bool HwndExists(IntPtr hwnd) => WinAPI.IsWindow(hwnd);
        /// <summary>Check if a window with the specified handle is active</summary>
        private static bool HwndActive(IntPtr hwnd) => hwnd == WinAPI.GetForegroundWindow();
        #endregion

        #endregion

        #region structs
        /// <summary>Select window flash target</summary>
        public enum FlashF {
            /// <summary>Flashes the taskbar button</summary>
            Taskbar,
            /// <summary>Flashes the window titlebar</summary>
            Titlebar,
            /// <summary>Flashes the taskbar button and the titlebar</summary>
            Both
        }
        #endregion

        #region operators
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static implicit operator bool(Window a) => a != null && a.Exists;
        public static bool operator ==(Window a, Window b) => (a is null && b is null) || !(a is null) && !(b is null) && a.Hwnd == b.Hwnd;
        public static bool operator !=(Window a, Window b) => !(a == b);
        public override bool Equals(object obj) => obj is Window && this == (Window) obj;
        public override int GetHashCode() => -640239398 + Hwnd.GetHashCode();
        public override string ToString() => $"Window:{(Hwnd == IntPtr.Zero ? "None" : Hwnd.ToString())}";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #endregion
    }
}