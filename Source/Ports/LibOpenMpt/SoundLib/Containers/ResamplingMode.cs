/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// Resampling modes
	/// </summary>
	internal enum ResamplingMode : uint8
	{
		// ATTENTION: Do not change ANY of these values, as they get written out to files in per instrument interpolation settings
		// and old files have these exact values in them which should not change meaning

		/// <summary>
		/// 1 tap, no AA
		/// </summary>
		Nearest = 0,

		/// <summary>
		/// 2 tap, no AA
		/// </summary>
		Linear = 1,

		/// <summary>
		/// 4 tap, no AA
		/// </summary>
		Cubic = 2,

		/// <summary>
		/// 8 tap, no AA (yes, index 4) (XMMS-ModPlug)
		/// </summary>
		Sinc8 = 4,

		/// <summary>
		/// 8 tap, with AA (yes, index 3) (Polyphase)
		/// </summary>
		Sinc8Lp = 3,

		/// <summary>
		/// Only used for instrument settings, not used inside the mixer
		/// </summary>
		Default = 5,

		/// <summary>
		/// Not explicitely user-selectable
		/// </summary>
		Amiga = 0xff
	}
}
