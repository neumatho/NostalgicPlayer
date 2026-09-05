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
	internal enum MixerFlags : uint32
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// Soft panning mode (this is forced with mixmode RC3 and later)
		/// </summary>
		SoftPanning = 0x10,

		/// <summary>
		/// Currently unused (should be used by Amiga MOD loaders)
		/// </summary>
		MaxDefaultPan = 0x80000,

		/// <summary>
		/// Notes are not played on muted channels
		/// </summary>
		MuteChnMode = 0x100000
	}
}
