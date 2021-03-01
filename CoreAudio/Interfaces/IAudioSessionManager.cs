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
	/// Enables a client to access the session controls and volume controls for both cross-process and process-specific audio sessions.
    /// </summary>
    /// <remarks>
	/// MSDN Reference: http://msdn.microsoft.com/en-us/library/dd370948.aspx
    /// </remarks>
	public partial interface IAudioSessionManager
    {
        // Note: Any changes to this interface should be repeated in IAudioSessionManager2.

		/// <summary>
		/// Retrieves an audio session control.
		/// </summary>
		/// <param name="sessionId">A new or existing session ID.</param>
		/// <param name="streamFlags">Audio session flags.</param>
		/// <param name="sessionControl">Receives an <see cref="IAudioSessionControl"/> interface for the audio session.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int GetAudioSessionControl(
			[In, Optional] [MarshalAs(UnmanagedType.LPStruct)] Guid sessionId,
			[In] [MarshalAs(UnmanagedType.U4)] UInt32 streamFlags,
			[Out] [MarshalAs(UnmanagedType.Interface)] out IAudioSessionControl sessionControl);

		/// <summary>
		/// Retrieves a simple audio volume control.
		/// </summary>
		/// <param name="sessionId">A new or existing session ID.</param>
		/// <param name="streamFlags">Audio session flags.</param>
		/// <param name="audioVolume">Receives an <see cref="ISimpleAudioVolume"/> interface for the audio session.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int GetSimpleAudioVolume(
			[In, Optional] [MarshalAs(UnmanagedType.LPStruct)] Guid sessionId,
			[In] [MarshalAs(UnmanagedType.U4)] UInt32 streamFlags,
			[Out] [MarshalAs(UnmanagedType.Interface)] out ISimpleAudioVolume audioVolume);
    }
}
