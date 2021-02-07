using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace WinUtilities {

    /// <summary>Specifies how string matching is performed</summary>
    public enum WinMatchType {
        /// <summary>Uses regex expression matching</summary>
        RegEx,
        /// <summary>The string must match</summary>
        Full,
        /// <summary>The string must be contained in the target</summary>
        Partial
    }

    /// <summary>A condition that matches some windows</summary>
    [DataContract]
    public struct WinMatch : IMatchObject {

        #region properties
        private string _title;
        private string _class;
        private string _exe;

        /// <summary>Matched window handle</summary>
        [DataMember]
        public WinHandle? Hwnd { get; set; }
        /// <summary>Matched window title</summary>
        [DataMember]
        public string Title {
            get => _title;
            set {
                _title = value;
                rTitle = new Regex(value ?? "", rOptions);
            }
        }
        /// <summary>Matched window class</summary>
        [DataMember]
        public string Class {
            get => _class;
            set {
                _class = value;
                rClass = new Regex(value ?? "", rOptions);
            }
        }
        /// <summary>Matched window exe</summary>
        [DataMember]
        public string Exe {
            get => _exe;
            set {
                _exe = value;
                rExe = new Regex(value ?? "", rOptions);
            }
        }
        /// <summary>Matched window process id</summary>
        [DataMember]
        public uint PID { get; set; }

        /// <summary>Reverse the result of the match</summary>
        [DataMember]
        public bool Reverse { get; set; }
        /// <summary>Get a reversed match</summary>
        public IMatchObject AsReverse {
            get {
                var match = this;
                match.Reverse ^= true;
                return match;
            }
        }

        /// <summary>Get whitelisted matches as a list</summary>
        public WinMatch[] AsList => new WinMatch[] { this };

        /// <summary>Specifies how the strings are matched</summary>
        [DataMember]
        public WinMatchType Type { get; set; }

        private Regex rTitle;
        private Regex rClass;
        private Regex rExe;

        private static readonly RegexOptions rOptions = RegexOptions.IgnoreCase;
        #endregion

        /// <summary>Create a new match condition</summary>
        public WinMatch(WinHandle? hwnd = null, string title = null, string className = null, string exe = null, uint pid = 0, WinMatchType type = WinMatchType.RegEx) {
            Hwnd = hwnd;
            _title = title;
            _class = className;
            _exe = exe;
            PID = pid;

            Reverse = false;
            Type = type;

            rTitle = new Regex(title ?? "", rOptions);
            rClass = new Regex(className ?? "", rOptions);
            rExe = new Regex(exe ?? "", rOptions);
        }

        /// <summary>Check if the given window matches</summary>
        public bool Match(Window win) {
            return Match(Hwnd != null ? (WinHandle?) win.Hwnd : null, Title != null ? win.Title : null, Class != null ? win.Class : null, Exe != null ? win.Exe : null, PID != 0 ? win.PID : 0);
        }

        /// <summary>Check if the given properties match</summary>
        public bool Match(WinHandle? hwnd, string title, string className, string exe, uint pid) {
            return Reverse ^ (MatchHwnd(hwnd) && MatchTitle(title) && MatchClass(className) && MatchExe(exe) && MatchPID(pid));
        }

        #region single matching
        /// <summary>Check if the window handle matches</summary>
        public bool MatchHwnd(WinHandle? hwnd) => Reverse ^ (Hwnd == null || Hwnd == hwnd);
        /// <summary>Check if the window title matches</summary>
        public bool MatchTitle(string title) => Reverse ^ MatchSingle(title, Title, rTitle);
        /// <summary>Check if the window class matches</summary>
        public bool MatchClass(string className) => Reverse ^ MatchSingle(className, Class, rClass);
        /// <summary>Check if the window exe matches</summary>
        public bool MatchExe(string exe) => Reverse ^ MatchSingle(exe, Exe, rExe);
        /// <summary>Check if the window process id matches</summary>
        public bool MatchPID(uint pid) => Reverse ^ (PID == 0 || PID == pid);

        private bool MatchSingle(string window, string match, Regex regex) {
            if (match == null) {
                return true;
            } else if (Type == WinMatchType.RegEx) {
                return regex.IsMatch(window);
            } else if (Type == WinMatchType.Full) {
                return match == window;
            } else {
                return window.Contains(match);
            }
        }
        #endregion

        /// <summary>Get a match object that matches the given window</summary>
        public static explicit operator WinMatch(Window window) => new WinMatch(window.Hwnd);
    }
}
