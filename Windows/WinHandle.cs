using System;
using System.Runtime.Serialization;

namespace WinUtilities {

    /// <summary>A wrapper object for a window handle</summary>
    [DataContract]
    public struct WinHandle {

        /// <summary>Pointer to the window</summary>
        [DataMember]
        public IntPtr Raw { get; }
        /// <summary>The object points to a nonexistent window</summary>
        public bool IsZero => Raw == IntPtr.Zero;
        /// <summary>The object points to a real window</summary>
        public bool IsValid => Raw != IntPtr.Zero;

        /// <summary>A handle that points to nothing</summary>
        public static WinHandle Zero => new WinHandle(IntPtr.Zero);
        /// <summary>A handle that points to all windows. Used with some win32 API calls.</summary>
        public static WinHandle Broadcast => new WinHandle((IntPtr) 0xffff);

        /// <summary>Create wrapper for a window handle</summary>
        public WinHandle(IntPtr Raw) {
            this.Raw = Raw;
        }

        #region operators
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool operator ==(WinHandle a, WinHandle b) => a.Raw == b.Raw && a.Raw != IntPtr.Zero;
        public static bool operator !=(WinHandle a, WinHandle b) => !(a == b);
        public override bool Equals(object obj) => obj is WinHandle handle && this == handle;
        public override int GetHashCode() => -638417062 + Raw.GetHashCode();
        public override string ToString() => Raw.ToString();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #endregion
    }
}
