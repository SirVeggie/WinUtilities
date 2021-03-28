using System;
using System.Threading.Tasks;

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

    /// <summary>A window match object for checking and finding windows</summary>
    public interface IWinMatch {

        /// <summary>Get whitelisted matches as a list</summary>
        WinMatch[] AsList { get; }
        /// <summary>Reverse the result of the match</summary>
        bool IsReverse { get; set; }
        /// <summary>Get a reversed match</summary>
        IWinMatch AsReverse { get; }

        /// <summary>Check if the given window matches</summary>
        bool Match(Window window);
        /// <summary>Check if the given info matches</summary>
        bool Match(WindowInfo info);

        /// <summary>Perform an action on all matching windows</summary>
        void ForAll(Action<Window> action, WinFindMode mode = WinFindMode.TopLevel);
        /// <summary>Perform an action on all matching windows. Return false to stop enumerating windows.</summary>
        /// <returns>True if all found windows were enumerated</returns>
        bool ForAll(Func<Window, bool> action, WinFindMode mode = WinFindMode.TopLevel);
        /// <summary>Perform an async action on all matching windows one at a time. Return false to stop enumerating windows.</summary>
        /// <returns>True if all found windows were enumerated</returns>
        Task<bool> ForAll(Func<Window, Task<bool>> action, WinFindMode mode = WinFindMode.TopLevel);

        /// <summary>A window from this group is the active window</summary>
        bool IsActive();
    }

    internal static class MatchActions {
        // <summary>Perform an action on all matching windows</summary>
        public static void ForAll(IWinMatch match, Action<Window> action, WinFindMode mode = WinFindMode.TopLevel) {
            var windows = Window.GetWindows(match, mode);

            foreach (var win in windows) {
                action(win);
            }
        }

        /// <summary>Perform an action on all matching windows. Return false to stop enumerating windows.</summary>
        /// <returns>True if all found windows were enumerated</returns>
        public static bool ForAll(IWinMatch match, Func<Window, bool> action, WinFindMode mode = WinFindMode.TopLevel) {
            var windows = Window.GetWindows(match, mode);

            foreach (var win in windows) {
                if (!action(win)) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Perform an async action on all matching windows one at a time. Return false to stop enumerating windows.</summary>
        /// <returns>True if all found windows were enumerated</returns>
        public static async Task<bool> ForAll(IWinMatch match, Func<Window, Task<bool>> action, WinFindMode mode = WinFindMode.TopLevel) {
            var windows = Window.GetWindows(match, mode);

            foreach (var win in windows) {
                if (!await action(win)) {
                    return false;
                }
            }

            return true;
        }
    }
}
