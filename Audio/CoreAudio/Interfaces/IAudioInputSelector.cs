﻿// -----------------------------------------
// SoundScribe (TM) and related software.
// 
// Copyright (C) 2007-2011 Vannatech
// http://www.vannatech.com
// All rights reserved.
// 
// This source code is subject to the MIT License.
// http://www.opensource.org/licenses/mit-license.php
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// -----------------------------------------

using System;
using System.Runtime.InteropServices;

namespace WinUtilities.CoreAudio.Interfaces
{
    /// <summary>
	/// Provides access to a hardware multiplexer control (input selector).
    /// </summary>
    /// <remarks>
	/// MSDN Reference: http://msdn.microsoft.com/en-us/library/dd368220.aspx
    /// </remarks>
	public partial interface IAudioInputSelector
    {
		/// <summary>
		/// Gets the local ID of the part that is connected to the selector input that is currently selected.
		/// </summary>
		/// <param name="partId">Receives the local ID of the part that directly links to the currently selected selector input.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int GetSelection(
			[Out] [MarshalAs(UnmanagedType.U4)] out UInt32 partId);

		/// <summary>
		/// Selects one of the inputs of the input selector.
		/// </summary>
		/// <param name="partId">The ID of the new selector input.</param>
		/// <param name="eventContext">A user context value that is passed to the notification callback.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int SetSelection(
			[In] [MarshalAs(UnmanagedType.U4)] UInt32 partId,
			[In, Optional] [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);
    }
}
