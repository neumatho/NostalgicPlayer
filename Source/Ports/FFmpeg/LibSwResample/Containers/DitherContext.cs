/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibSwResample.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class DitherContext
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
		/// <summary>
		/// 
		/// </summary>
		public SwrDitherType Method;

		/// <summary>
		/// 
		/// </summary>
		public c_int Noise_Pos;

		/// <summary>
		/// 
		/// </summary>
		public c_float Scale;

		/// <summary>
		/// Noise scale
		/// </summary>
		public c_float Noise_Scale;

		/// <summary>
		/// Noise shaping dither taps
		/// </summary>
		public c_int Ns_Taps;

		/// <summary>
		/// Noise shaping dither scale
		/// </summary>
		public c_float Ns_Scale;

		/// <summary>
		/// Noise shaping dither scale^-1
		/// </summary>
		public c_float Ns_Scale_1;

		/// <summary>
		/// Noise shaping dither position
		/// </summary>
		public c_int Ns_Pos;

		/// <summary>
		/// Noise shaping filter coefficients
		/// </summary>
		public readonly c_double[] Ns_Coeffs = new c_double[SwrConstants.Ns_Taps];

		/// <summary>
		/// 
		/// </summary>
		public readonly c_double[][] Ns_Errors = ArrayHelper.Initialize2Arrays<c_double>(SwrConstants.Swr_Ch_Max, 2 * SwrConstants.Ns_Taps);

		/// <summary>
		/// Noise used for dithering
		/// </summary>
		public readonly AudioData Noise = new AudioData();

		/// <summary>
		/// Temporary storage when writing into the input buffer isn't possible
		/// </summary>
		public readonly AudioData Temp = new AudioData();

		/// <summary>
		/// The number of used output bits, needed to scale dither correctly
		/// </summary>
		public c_int Output_Sample_Bits;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
	}
}
