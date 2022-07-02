namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Private
{
	internal interface IFixed
	{
		/// <summary>
		/// Compute the best fixed predictor and the expected bits-per-sample
		/// of the residual signal for each order
		/// </summary>
		uint32_t Compute_Best_Predictor(Flac__int32[] data, uint32_t offset, uint32_t data_Len, float[] residual_Bits_Per_Sample);

		/// <summary>
		/// Compute the best fixed predictor and the expected bits-per-sample
		/// of the residual signal for each order. This version uses 64-bit
		/// integers which is statistically necessary when bits-per-sample +
		/// log2(blockSize) > 30
		/// </summary>
		uint32_t Compute_Best_Predictor_Wide(Flac__int32[] data, uint32_t offset, uint32_t data_Len, float[] residual_Bits_Per_Sample);
	}
}
