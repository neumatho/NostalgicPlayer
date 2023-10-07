/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum Xmp_Sample_Flag
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// 16 bit sample
		/// </summary>
		_16Bit = (1 << 0),

		/// <summary>
		/// Sample is looped
		/// </summary>
		Loop = (1 << 1),

		/// <summary>
		/// Bidirectional sample loop
		/// </summary>
		Loop_BiDir = (1 << 2),

		/// <summary>
		/// Backwards sample loop
		/// </summary>
		Loop_Reverse = (1 << 3),

		/// <summary>
		/// Play full sample before looping
		/// </summary>
		Loop_Full = (1 << 4),

		/// <summary>
		/// Sample has sustain loop
		/// </summary>
		SLoop = (1 << 5),

		/// <summary>
		/// Bidirectional sustain loop
		/// </summary>
		SLoop_BiDir = (1 << 6),

		/// <summary>
		/// Interlaced stereo sample
		/// </summary>
		Stereo = (1 << 7),

		/// <summary>
		/// Adlib sample
		/// </summary>
		Adlib = (1 << 14),

		/// <summary>
		/// Data contains synth patch
		/// </summary>
		Synth = (1 << 15)
	}
}
