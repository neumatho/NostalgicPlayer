/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac.Containers
{
	/// <summary>
	/// Constant subframe
	/// </summary>
	internal class Flac__SubFrame_Lpc : ISubFrame
	{
		/// <summary>
		/// The residual coding method
		/// </summary>
		public Flac__EntropyCodingMethod Entropy_Coding_Method = new Flac__EntropyCodingMethod();

		/// <summary>
		/// The FIR order
		/// </summary>
		public uint32_t Order;

		/// <summary>
		/// Quantized FIR filter coefficient precision in bits
		/// </summary>
		public uint32_t Qlp_Coeff_Precision;

		/// <summary>
		/// The qlp coeff shift needed
		/// </summary>
		public int Quantization_Level;

		/// <summary>
		/// FIR filter coefficients
		/// </summary>
		public Flac__int32[] Qlp_Coeff = new Flac__int32[Constants.Flac__Max_Lpc_Order];

		/// <summary>
		/// Warmup samples to prime the predictor, length == order
		/// </summary>
		public Flac__int32[] Warmup = new Flac__int32[Constants.Flac__Max_Lpc_Order];

		/// <summary>
		/// The residual signal, length == (blocksize minus order) samples
		/// </summary>
		public Flac__int32[] Residual;
	}
}