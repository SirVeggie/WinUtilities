using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace WinUtilities {

    /// <summary>A match object that can have multiple match conditions</summary>
    [DataContract]
    public struct WinGroup : IMatchObject {

        [DataMember]
        private List<IMatchObject> whitelist;
        [DataMember]
        private List<IMatchObject> blacklist;

        #region properties
        /// <summary>List of conditions to match</summary>
        public List<IMatchObject> Whitelist {
            get {
                if (whitelist == null)
                    whitelist = new List<IMatchObject>();
                return whitelist;
            }
            set => whitelist = value;
        }

        /// <summary>List of conditions that prevent a whitelist match</summary>
        public List<IMatchObject> Blacklist {
            get {
                if (blacklist == null)
                    blacklist = new List<IMatchObject>();
                return blacklist;
            }
            set => blacklist = value;
        }

        /// <summary>Number of conditions in the whitelist</summary>
        public int Size => Whitelist.Count;

        /// <summary>Reverse the result of the match</summary>
        [DataMember]
        public bool IsReverse { get; set; }
        /// <summary>Get a reversed match</summary>
        public IMatchObject AsReverse {
            get {
                var match = this;
                match.IsReverse ^= true;
                return match;
            }
        }

        /// <summary>Get whitelisted matches as a list</summary>
        public WinMatch[] AsList => Whitelist.SelectMany(m => m.AsList).ToArray();
        #endregion

        #region predefined
        /// <summary>Matches the desktop</summary>
        public static WinGroup Desktop { get; } = new WinGroup(
            new WinMatch(className: "WorkerW", type: WinMatchType.Full),
            new WinMatch(className: "Progman", type: WinMatchType.Full));
        #endregion

        /// <summary>A group of window descriptions that can match a variety of windows.</summary>
        public WinGroup(params IMatchObject[] matchlist) {
            IsReverse = false;
            whitelist = new List<IMatchObject>();
            blacklist = new List<IMatchObject>();
            whitelist.AddRange(matchlist);
        }

        /// <summary>Add window descriptions to the Whitelist.</summary>
        public void Add(params IMatchObject[] whitelist) => Whitelist.AddRange(whitelist);

        /// <summary>Add windows to the Whitelist.</summary>
        public void Add(params Window[] windows) => Add(windows.Select(w => new WinMatch(hwnd: w.Hwnd)).Cast<IMatchObject>().ToArray());

        /// <summary>Add window descriptions to the Blacklist.</summary>
        public void AddBlacklist(params IMatchObject[] blacklist) => Blacklist.AddRange(blacklist);

        /// <summary>Add windows to the Blacklist.</summary>
        public void AddBlacklist(params Window[] windows) => AddBlacklist(windows.Select(w => new WinMatch(hwnd: w.Hwnd)).Cast<IMatchObject>().ToArray());

        #region matching
        /// <summary>Check if a window matches this description group.</summary>
        /// <remarks>The Match is true if the window matches the Whitelist but not the Blacklist.</remarks>
        public bool Match(Window win) => Match(win.Hwnd, win.Title, win.Class, win.Exe, win.PID);

        /// <summary>Check if a window matches this description group.</summary>
        /// <remarks>The Match is true if the info matches the Whitelist but not the Blacklist.</remarks>
        public bool Match(WindowInfo info) => IsReverse ^ (MatchList(Blacklist, info) ? false : MatchList(Whitelist, info));

        /// <summary>Check if a window matches this description group.</summary>
        /// <remarks>The Match is true if the window matches the Whitelist but not the Blacklist.</remarks>
        public bool Match(WinHandle? hwnd, string title, string className, string exe, uint pid) {
            if (MatchList(Blacklist, hwnd, title, className, exe, pid)) {
                return IsReverse ^ false;
            }

            return IsReverse ^ MatchList(Whitelist, hwnd, title, className, exe, pid);
        }

        private bool MatchList(List<IMatchObject> list, WindowInfo info) {
            for (int i = 0; i < list.Count; i++) {
                if (list[i].Match(info)) {
                    return true;
                }
            }

            return false;
        }

        private bool MatchList(List<IMatchObject> list, WinHandle? hwnd, string title, string className, string exe, uint pid) {
            for (int i = 0; i < list.Count; i++) {
                if (list[i].Match(hwnd, title, className, exe, pid)) {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region actions
        /// <summary>Perform an action on all matching windows</summary>
        public void ForAll(Action<Window> action, bool hidden = false) {
            var windows = Window.GetWindows(this, hidden);

            foreach (var win in windows) {
                action(win);
            }
        }

        /// <summary>Perform an action on all matching windows. Return false to stop enumerating windows.</summary>
        /// <returns>True if all found windows were enumerated</returns>
        public bool ForAll(Func<Window, bool> action, bool hidden = false) {
            var windows = Window.GetWindows(this, hidden);

            foreach (var win in windows) {
                if (!action(win)) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Perform an async action on all matching windows one at a time. Return false to stop enumerating windows.</summary>
        /// <returns>True if all found windows were enumerated</returns>
        public async Task<bool> ForAll(Func<Window, Task<bool>> action, bool hidden = false) {
            var windows = Window.GetWindows(this, hidden);

            foreach (var win in windows) {
                if (!await action(win)) {
                    return false;
                }
            }

            return true;
        }
        #endregion
    }
}
