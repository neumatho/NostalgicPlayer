/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// 
	/// </summary>
	internal static class WindowedFir
	{
		// Quantizer scale of window coefs - only required for integer mixing
		public const c_int WFir_QuantBits = 15;
		public const c_double WFir_QuantScale = 1 << WFir_QuantBits;
		public const c_int WFir_16BitShift = WFir_QuantBits;

		// log2(number)-1 of precalculated taps range is [4..12]
		public const c_int WFir_FracBits = 12;
		public const c_int WFir_LutLen = (1 << (WFir_FracBits + 1)) + 1;

		// Number of samples in window
		public const c_int WFir_Log2Width = 3;
		public const c_int WFir_Width = 1 << WFir_Log2Width;

		// Fir interpolation
		public const c_int WFir_FracShift = 16 - (WFir_FracBits + 1 + WFir_Log2Width);
		public const c_int WFir_FracMask = ((1 << (17 - WFir_FracShift)) - 1) & ~(WFir_Width - 1);
		public const c_int WFir_FracHalve = 1 << (16 - (WFir_FracBits + 2));
	}
}
