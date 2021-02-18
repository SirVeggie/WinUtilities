using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WinUtilities {

    /// <summary>A condition that matches some windows</summary>
    [DataContract]
    public struct WinMatch : IWinMatch {

        private string _title;
        private string _class;
        private string _exe;

        private Regex rTitle;
        private Regex rClass;
        private Regex rExe;

        private static readonly RegexOptions rOptions = RegexOptions.IgnoreCase;

        #region properties
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

        /// <summary>Matched desktop guid</summary>
        [DataMember]
        public Guid Desktop { get; set; }

        /// <summary>Reverse the result of the match</summary>
        [DataMember]
        public bool IsReverse { get; set; }
        /// <summary>Get a reversed match</summary>
        public IWinMatch AsReverse {
            get {
                var match = this;
                match.IsReverse ^= true;
                return match;
            }
        }

        /// <summary>Get whitelisted matches as a list</summary>
        public WinMatch[] AsList => new WinMatch[] { this };

        /// <summary>Specifies how the strings are matched</summary>
        [DataMember]
        public WinMatchType Type { get; set; }
        #endregion

        /// <summary>Create a new match condition</summary>
        public WinMatch(WinHandle? hwnd = null, string title = null, string className = null, string exe = null, uint pid = 0, Guid desktop = default, WinMatchType type = WinMatchType.RegEx) {
            Hwnd = hwnd;
            _title = title;
            _class = className;
            _exe = exe;
            PID = pid;
            Desktop = desktop;

            IsReverse = false;
            Type = type;

            rTitle = new Regex(title ?? "", rOptions);
            rClass = new Regex(className ?? "", rOptions);
            rExe = new Regex(exe ?? "", rOptions);
        }

        #region methods
        /// <summary>Check if the given window matches</summary>
        public bool Match(Window win) => Match(new WindowInfo(win));

        /// <summary>Check if the given info matches</summary>
        public bool Match(WindowInfo info) {
            bool result = (Hwnd == null || Hwnd == info.Hwnd)
                       && (Exe == null || MatchSingle(info.Exe, Exe, rExe))
                       && (Class == null || MatchSingle(info.Class, Class, rClass))
                       && (PID == 0 || PID == info.PID)
                       && (Title == null || MatchSingle(info.Title, Title, rTitle))
                       && (Desktop == Guid.Empty || Desktop == info.Desktop);
            return IsReverse ^ result;
        }

        /// <summary>Perform an action on all matching windows</summary>
        public void ForAll(Action<Window> action, WinFindMode mode = WinFindMode.TopLevel) => MatchActions.ForAll(this, action, mode);

        /// <summary>Perform an action on all matching windows. Return false to stop enumerating windows.</summary>
        /// <returns>True if all found windows were enumerated</returns>
        public bool ForAll(Func<Window, bool> action, WinFindMode mode = WinFindMode.TopLevel) => MatchActions.ForAll(this, action, mode);

        /// <summary>Perform an async action on all matching windows one at a time. Return false to stop enumerating windows.</summary>
        /// <returns>True if all found windows were enumerated</returns>
        public async Task<bool> ForAll(Func<Window, Task<bool>> action, WinFindMode mode = WinFindMode.TopLevel) => await MatchActions.ForAll(this, action, mode);
        #endregion

        #region single matching
        /// <summary>Check if the window handle matches</summary>
        public bool MatchHwnd(WinHandle? hwnd) => IsReverse ^ (Hwnd == null || Hwnd == hwnd);
        /// <summary>Check if the window title matches</summary>
        public bool MatchTitle(string title) => IsReverse ^ MatchSingle(title, Title, rTitle);
        /// <summary>Check if the window class matches</summary>
        public bool MatchClass(string className) => IsReverse ^ MatchSingle(className, Class, rClass);
        /// <summary>Check if the window exe matches</summary>
        public bool MatchExe(string exe) => IsReverse ^ MatchSingle(exe, Exe, rExe);
        /// <summary>Check if the window process id matches</summary>
        public bool MatchPID(uint pid) => IsReverse ^ (PID == 0 || PID == pid);

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
