/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	internal enum DspFlags : uint32
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// Apply reverb
		/// </summary>
		Reverb = 0x20
	}
}
