using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WinUtilities {
    /// <summary>A window match object for checking and finding windows</summary>
    public interface IMatchObject {

        /// <summary>Get whitelisted matches as a list</summary>
        WinMatch[] AsList { get; }
        /// <summary>Reverse the result of the match</summary>
        bool IsReverse { get; set; }
        /// <summary>Get a reversed match</summary>
        IMatchObject AsReverse { get; }

        /// <summary>Check if the given window matches</summary>
        bool Match(Window window);
        /// <summary>Check if the given info matches</summary>
        bool Match(WindowInfo info);
        /// <summary>Check if the given properties match</summary>
        bool Match(WinHandle? hwnd, string title, string className, string exe, uint pid);


        /// <summary>Perform an action on all matching windows</summary>
        void ForAll(Action<Window> action, bool hidden = false);

        /// <summary>Perform an action on all matching windows. Return false to stop enumerating windows.</summary>
        /// <returns>True if all found windows were enumerated</returns>
        bool ForAll(Func<Window, bool> action, bool hidden = false);

        /// <summary>Perform an async action on all matching windows one at a time. Return false to stop enumerating windows.</summary>
        /// <returns>True if all found windows were enumerated</returns>
        Task<bool> ForAll(Func<Window, Task<bool>> action, bool hidden = false);
    }
}
