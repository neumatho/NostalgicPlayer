/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac.Containers
{
	/// <summary>
	/// Fixed subframe
	/// </summary>
	internal class Flac__SubFrame_Fixed : ISubFrame
	{
		/// <summary>
		/// The residual coding method
		/// </summary>
		public Flac__EntropyCodingMethod Entropy_Coding_Method = new Flac__EntropyCodingMethod();

		/// <summary>
		/// The polynomial order
		/// </summary>
		public uint32_t Order;

		/// <summary>
		/// Warmup samples to prime the predictor, length == order
		/// </summary>
		public Flac__int32[] Warmup = new Flac__int32[Constants.Flac__Max_Fixed_Order];

		/// <summary>
		/// The residual signal, length == (blocksize minus order) samples
		/// </summary>
		public Flac__int32[] Residual;
	}
}