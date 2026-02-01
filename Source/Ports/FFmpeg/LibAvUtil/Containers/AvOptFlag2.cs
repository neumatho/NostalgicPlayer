/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum AvOptFlag2
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// Accept to parse a value without a key; the key will then be returned
		/// as NULL
		/// </summary>
		Implicit_Key = 1
	}
}
