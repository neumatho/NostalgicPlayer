/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
global using mixsample_t = System.Int32;

using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundBase;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Basic mixer constants
	/// </summary>
	internal static class Mixer
	{
		public static readonly c_int Mixing_Filter_Precision = MixSample.MixSampleIntTraits.Filter_Precision_Bits;

		public static readonly c_int Mixing_Attentuation = MixSample.MixSampleIntTraits.Mix_Headroom_Bits;

		public static readonly c_float Mixing_ScaleF = MixSample.MixSampleIntTraits.Mix_Scale;

		public const c_int MixBufferSize = 512;
		public const c_int NumMixInputBuffers = 4;

		/// <summary>
		/// Fractional bits in volume ramp variables
		/// </summary>
		public const c_int VolumeRampPrecision = 12;

		/// <summary>
		/// While we could directly use the previous value in various places such as the interpolation wrap-around handling at loop points,
		/// choosing a higher value (e.g. 16) will reduce CPU usage when using many extremely short (length ‹ 16) samples
		/// </summary>
		public const uint8 InterpolationLookaheadBufferSize = 16;

		/// <summary>
		/// Maximum size of a sampling point of a sample, in bytes.
		/// The biggest sampling point size is currently 16-bit stereo = 2 * 2 bytes
		/// </summary>
		public const uint8 MaxSamplingPointSize = 4;
	}
}
