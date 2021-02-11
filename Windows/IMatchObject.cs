using System;
using System.Collections.Generic;

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
        /// <summary>Check if the given properties match</summary>
        bool Match(WinHandle? hwnd, string title, string className, string exe, uint pid);
    }
}
