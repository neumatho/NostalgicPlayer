/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// Instrument envelope-specific flags
	/// </summary>
	[Flags]
	internal enum EnvelopeFlags : uint8
	{
		/// <summary>
		/// Env is enabled
		/// </summary>
		Enabled = 0x01,

		/// <summary>
		/// Env loop
		/// </summary>
		Loop = 0x02,

		/// <summary>
		/// Env sustain
		/// </summary>
		Sustain = 0x04,

		/// <summary>
		/// Env carry
		/// </summary>
		Carry = 0x08,

		/// <summary>
		/// Filter env enabled (this has to be combined with ENV_ENABLED in the pitch envelope's flags)
		/// </summary>
		Filter = 0x10
	}
}
