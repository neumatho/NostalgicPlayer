/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum AvSeek
	{

		/// <summary>
		/// 
		/// </summary>
		Set = 0,

		/// <summary>
		/// 
		/// </summary>
		Cur = 1,

		/// <summary>
		/// 
		/// </summary>
		End = 2,

		/// <summary>
		/// Passing this as the "whence" parameter to a seek function causes it to
		/// return the filesize without seeking anywhere. Supporting this is optional.
		/// If it is not supported then the seek function will return ‹0
		/// </summary>
		Size = 0x10000,

		/// <summary>
		/// OR'ing this flag into the "whence" parameter to a seek function causes it to
		/// seek by any means (like reopening and linear reading) or other normally unreasonable
		/// means that can be extremely slow.
		/// This is the default and therefore ignored by the seek code since 2010
		/// </summary>
		Force = 0x20000
	}
}
