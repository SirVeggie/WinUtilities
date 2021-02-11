using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace WinUtilities.Windows {

    /// <summary>An info object used for caching window information. Dynamically retrieves missing window info when requested. When info is requested once, it is cached forever.</summary>
    public class WindowInfo {

        public Window Window { get; }

        #region basic
        private string title;
        public string Title {
            get {
                if (title == null)
                    title = Window.Title;
                return title;
            }
        }

        private string @class;
        public string Class {
            get {
                if (@class == null)
                    @class = Window.Class;
                return @class;
            }
        }

        private string exe;
        public string Exe {
            get {
                if (exe == null)
                    exe = Window.Exe;
                return exe;
            }
        }

        private string exepath;
        public string ExePath {
            get {
                if (exepath == null)
                    exepath = Window.ExePath;
                return exepath;
            }
        }

        private uint pid;
        public uint PID {
            get {
                if (pid == 0)
                    pid = Window.PID;
                return pid;
            }
        }

        private Process process;
        public Process Process {
            get {
                if (process == null)
                    process = Window.Process;
                return process;
            }
        }

        private int threadid;
        public int ThreadID {
            get {
                if (threadid == 0)
                    threadid = Window.ThreadID;
                return threadid;
            }
        }
        #endregion

        #region areas
        private Area area = Area.NaN;
        public Area Area {
            get {
                if (area.IsNaN)
                    area = Window.Area;
                return area;
            }
        }

        private Area rawarea = Area.NaN;
        public Area RawArea {
            get {
                if (rawarea.IsNaN)
                    rawarea = Window.RawArea;
                return rawarea;
            }
        }

        private Area clientarea = Area.NaN;
        public Area ClientArea {
            get {
                if (clientarea.IsNaN)
                    clientarea = Window.ClientArea;
                return clientarea;
            }
        }

        private Area borderlessarea = Area.NaN;
        public Area BorderlessArea {
            get {
                if (borderlessarea.IsNaN)
                    borderlessarea = Window.BorderlessArea;
                return borderlessarea;
            }
        }
        #endregion

        #region states
        private bool isvisibleChecked;
        private bool isvisible;
        public bool IsVisible {
            get {
                if (!isvisibleChecked) {
                    isvisibleChecked = true;
                    isvisible = Window.IsVisible;
                }
                return isvisible;
            }
        }

        private bool isenabledChecked;
        private bool isenabled;
        public bool IsEnabled {
            get {
                if (!isenabledChecked) {
                    isenabledChecked = true;
                    isenabled = Window.IsEnabled;
                }
                return isenabled;
            }
        }

        private bool isactiveChecked;
        private bool isactive;
        public bool IsActive {
            get {
                if (!isactiveChecked) {
                    isactiveChecked = true;
                    isactive = Window.IsActive;
                }
                return isactive;
            }
        }

        private bool existsChecked;
        private bool exists;
        public bool Exists {
            get {
                if (!existsChecked) {
                    existsChecked = true;
                    exists = Window.Exists;
                }
                return exists;
            }
        }

        private bool isoncurrentdesktopChecked;
        private bool isoncurrentdesktop;
        public bool IsOnCurrentDesktop {
            get {
                if (!isoncurrentdesktopChecked) {
                    isoncurrentdesktopChecked = true;
                    isoncurrentdesktop = Window.IsOnCurrentDesktop;
                }
                return isoncurrentdesktop;
            }
        }

        private bool isalwaysontopChecked;
        private bool isalwaysontop;
        public bool IsAlwaysOnTop {
            get {
                if (!isalwaysontopChecked) {
                    isalwaysontopChecked = true;
                    isalwaysontop = Window.IsAlwaysOnTop;
                }
                return isalwaysontop;
            }
        }

        private bool isclickthroughChecked;
        private bool isclickthrough;
        public bool IsClickThrough {
            get {
                if (!isclickthroughChecked) {
                    isclickthroughChecked = true;
                    isclickthrough = Window.IsClickThrough;
                }
                return isclickthrough;
            }
        }

        private bool ischildChecked;
        private bool ischild;
        public bool IsChild {
            get {
                if (!ischildChecked) {
                    ischildChecked = true;
                    ischild = Window.IsChild;
                }
                return ischild;
            }
        }

        private bool ismaxChecked;
        private bool ismax;
        public bool IsMaximized {
            get {
                if (!ismaxChecked) {
                    ismaxChecked = true;
                    ismax = Window.IsMaximized;
                }
                return ismax;
            }
        }

        private bool isminChecked;
        private bool ismin;
        public bool IsMinimized {
            get {
                if (!isminChecked) {
                    isminChecked = true;
                    ismin = Window.IsMinimized;
                }
                return ismin;
            }
        }

        private bool isfullChecked;
        private bool isfull;
        public bool IsFullscreen {
            get {
                if (!isfullChecked) {
                    isfullChecked = true;
                    isfull = Window.IsFullscreen;
                }
                return isfull;
            }
        }

        private bool isborderlessChecked;
        private bool isborderless;
        public bool IsBorderless {
            get {
                if (!isborderlessChecked) {
                    isborderlessChecked = true;
                    isborderless = Window.IsBorderless;
                }
                return isborderless;
            }
        }

        private bool istoplevelChecked;
        private bool istoplevel;
        public bool IsTopLevel {
            get {
                if (!istoplevelChecked) {
                    istoplevelChecked = true;
                    istoplevel = Window.IsTopLevel;
                }
                return istoplevel;
            }
        }

        private WS style;
        public WS Style {
            get {
                if (style == 0)
                    style = Window.Style;
                return style;
            }
        }

        private WS_EX exstyle;
        public WS_EX ExStyle {
            get {
                if (exstyle == 0)
                    exstyle = Window.ExStyle;
                return exstyle;
            }
        }

        private bool hasregionChecked;
        private bool hasregion;
        public bool HasRegion {
            get {
                if (!hasregionChecked) {
                    hasregionChecked = true;
                    hasregion = Window.HasRegion;
                }
                return hasregion;
            }
        }

        private bool regiontypeChecked;
        private WinAPI.RegionType regiontype;
        public WinAPI.RegionType RegionType {
            get {
                if (!regiontypeChecked) {
                    regiontypeChecked = true;
                    regiontype = Window.RegionType;
                }
                return regiontype;
            }
        }

        private Area regionbounds = Area.NaN;
        public Area RegionBounds {
            get {
                if (regionbounds.IsNaN)
                    regionbounds = Window.RegionBounds;
                return regionbounds;
            }
        }
        #endregion

        #region other
        private bool isnoneChecked;
        private bool isnone;
        public bool IsNone {
            get {
                if (!isnoneChecked) {
                    isnoneChecked = true;
                    isnone = Window.IsNone;
                }
                return isnone;
            }
        }

        private bool isvalidChecked;
        private bool isvalid;
        public bool IsValid {
            get {
                if (!isvalidChecked) {
                    isvalidChecked = true;
                    isvalid = Window.IsValid;
                }
                return isvalid;
            }
        }

        private Window parent;
        public Window Parent {
            get {
                if (parent == null)
                    parent = Window.Parent;
                return parent;
            }
        }

        private Window ancestor;
        public Window Ancestor {
            get {
                if (ancestor == null)
                    ancestor = Window.Ancestor;
                return ancestor;
            }
        }

        private Window owner;
        public Window Owner {
            get {
                if (owner == null)
                    owner = Window.Owner;
                return owner;
            }
        }

        private List<Window> siblings;
        public List<Window> Siblings {
            get {
                if (siblings == null)
                    siblings = Window.Siblings;
                return siblings;
            }
        }

        private Monitor monitor;
        public Monitor Monitor {
            get {
                if (monitor == null)
                    monitor = Window.Monitor;
                return monitor;
            }
        }

        private Guid desktop;
        public Guid Desktop {
            get {
                if (desktop == Guid.Empty)
                    desktop = Window.Desktop;
                return desktop;
            }
        }

        public WinMatch AsMatch => Window.AsMatch;
        #endregion

        /// <summary></summary>
        public WindowInfo(Window window) {
            Window = window;
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member