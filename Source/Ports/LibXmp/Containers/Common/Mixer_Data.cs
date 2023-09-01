/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common
{
	/// <summary>
	/// 
	/// </summary>
	internal class Mixer_Data
	{
		/// <summary>
		/// Sampling rate
		/// </summary>
		public c_int Freq;

		/// <summary>
		/// Sample format
		/// </summary>
		public Xmp_Format Format;

		/// <summary>
		/// Amplification multiplier
		/// </summary>
		public c_int Amplify;

		/// <summary>
		/// Percentage of channel separation
		/// </summary>
		public c_int Mix;

		/// <summary>
		/// Interpolation type
		/// </summary>
		public Xmp_Interp Interp;

		/// <summary>
		/// DSP effect flags
		/// </summary>
		public Xmp_Dsp Dsp;

		/// <summary>
		/// Output buffer
		/// </summary>
		public int8[] Buffer;

		/// <summary>
		/// Temporary buffer for 32 bit samples
		/// </summary>
		public int32[] Buf32;

		/// <summary>
		/// Default softmixer voices number
		/// </summary>
		public c_int NumVoc;

		/// <summary>
		/// 
		/// </summary>
		public c_int TickSize;

		/// <summary>
		/// Anticlick control, right channel
		/// </summary>
		public c_int DtRight;

		/// <summary>
		/// Anticlick control, left channel
		/// </summary>
		public c_int DtLeft;

		/// <summary>
		/// Adjustment for IT bidirectional loops
		/// </summary>
		public c_int BiDir_Adjust;

		/// <summary>
		/// Period base
		/// </summary>
		public c_double PBase;
	}
}
