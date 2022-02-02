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
	/// Provides access to a hardware bass-level control.
    /// </summary>
    /// <remarks>
	/// MSDN Reference: http://msdn.microsoft.com/en-us/library/dd370857.aspx
    /// </remarks>
	public partial interface IAudioBass : IPerChannelDbLevel
    {
		// The IAudioBass interface inherits from IPerChannelDbLevel but does not define additional methods or properties.
		// Note: Although, derived classes normally do not work for COM interfaces, it does when no additional methods are defined in the subclass.
    }
}
