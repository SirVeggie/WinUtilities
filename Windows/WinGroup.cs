using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WinUtilities {

    /// <summary>A match object that can have multiple match conditions</summary>
    [DataContract]
    public struct WinGroup : IWinMatch {

        [DataMember]
        private List<IWinMatch> whitelist;
        [DataMember]
        private List<IWinMatch> blacklist;

        #region properties
        /// <summary>List of conditions to match</summary>
        public List<IWinMatch> Whitelist {
            get {
                if (whitelist == null)
                    whitelist = new List<IWinMatch>();
                return whitelist;
            }
            set => whitelist = value;
        }

        /// <summary>List of conditions that prevent a whitelist match</summary>
        public List<IWinMatch> Blacklist {
            get {
                if (blacklist == null)
                    blacklist = new List<IWinMatch>();
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
        public IWinMatch AsReverse {
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
        /// <summary>Matches the taskbar</summary>
        public static WinMatch Taskbar { get; } = new WinMatch(className: "Shell_TrayWnd");
        /// <summary>Matches Steam games installed in a normal install folder</summary>
        public static WinMatch SteamGame { get; } = new WinMatch(exePath: "steamapps");
        /// <summary>Matches file explorer windows aka folders</summary>
        public static WinMatch Folder { get; } = new WinMatch(className: "CabinetWClass", exe: "explorer");
        #endregion

        /// <summary>A group of window descriptions that can match a variety of windows.</summary>
        public WinGroup(params IWinMatch[] matchlist) {
            IsReverse = false;
            whitelist = new List<IWinMatch>();
            blacklist = new List<IWinMatch>();
            whitelist.AddRange(matchlist);
        }

        #region methods
        /// <summary>Add window descriptions to the Whitelist.</summary>
        public void Add(params IWinMatch[] whitelist) => Whitelist.AddRange(whitelist);

        /// <summary>Add windows to the Whitelist.</summary>
        public void Add(params Window[] windows) => Add(windows.Select(w => new WinMatch(hwnd: w.Hwnd)).Cast<IWinMatch>().ToArray());

        /// <summary>Add window descriptions to the Blacklist.</summary>
        public void AddBlacklist(params IWinMatch[] blacklist) => Blacklist.AddRange(blacklist);

        /// <summary>Add windows to the Blacklist.</summary>
        public void AddBlacklist(params Window[] windows) => AddBlacklist(windows.Select(w => new WinMatch(hwnd: w.Hwnd)).Cast<IWinMatch>().ToArray());

        /// <summary>Check if a window matches this description group.</summary>
        /// <remarks>The Match is true if the window matches the Whitelist but not the Blacklist.</remarks>
        public bool Match(Window win) => Match(new WindowInfo(win));

        /// <summary>Check if a window matches this description group.</summary>
        /// <remarks>The Match is true if the info matches the Whitelist but not the Blacklist.</remarks>
        public bool Match(WindowInfo info) => IsReverse ^ (MatchList(Blacklist, info) ? false : MatchList(Whitelist, info));

        /// <summary>A window from this group is the active window</summary>
        public bool IsActive() => Window.Active.Match(this);

        private bool MatchList(List<IWinMatch> list, WindowInfo info) {
            for (int i = 0; i < list.Count; i++) {
                if (list[i].Match(info)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Remove all matching items from the group's whitelist. Returns true if any items were deleted.</summary>
        public bool Remove(Func<WinMatch, bool> predicate) => RemoveFromList(predicate, whitelist);
        /// <summary>Remove all matching items from the group's blacklist. Returns true if any items were deleted.</summary>
        public bool RemoveBlacklist(Func<WinMatch, bool> predicate) => RemoveFromList(predicate, blacklist);

        private bool RemoveFromList(Func<WinMatch, bool> predicate, List<IWinMatch> list) {
            if (predicate == null)
                throw new ArgumentNullException("Predicate can't be null");
            bool changed = false;
            List<int> deleted = new List<int>();

            for (int i = 0; i < list.Count; i++) {
                var match = whitelist[i];
                if (match is WinMatch wm) {
                    if (predicate(wm)) {
                        changed = true;
                        deleted.Add(i);
                    }
                } else {
                    var group = (WinGroup) match;
                    if (group.Remove(predicate))
                        changed = true;
                    if (group.Size == 0)
                        deleted.Add(i);
                }
            }

            foreach (int i in deleted) {
                list.RemoveAt(i);
            }

            return changed;
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
    }
}
