/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// Instrument-specific flags
	/// </summary>
	[Flags]
	internal enum InstrumentFlags : uint8
	{
		/// <summary>
		/// Panning enabled
		/// </summary>
		SetPanning = 0x01,

		/// <summary>
		/// Instrument is muted
		/// </summary>
		Mute = 0x02
	}
}
