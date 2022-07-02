/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Private
{
	/// <summary>
	/// Common interface for all LPC implementations
	/// </summary>
	internal interface ILpc
	{
		/// <summary>
		/// Compute the autocorrelation for lags between 0 and lag-1.
		/// Assumes data[] outside of [0,data_len-1] == 0.
		/// Asserts that lag > 0.
		/// </summary>
		void Compute_Autocorrelation(Flac__real[] data, uint32_t data_Len, uint32_t lag, Flac__real[] autoc);

		/// <summary>
		/// Compute the residual signal obtained from subtracting the
		/// predicted signal from the original
		/// </summary>
		void Compute_Residual_From_Qlp_Coefficients(Flac__int32[] data, uint32_t data_Offset, uint32_t data_Len, Flac__int32[] qlp_Coeff, uint32_t order, int lp_Quantization, Flac__int32[] residual);

		/// <summary>
		/// Generic 64-bit datapath
		/// </summary>
		void Compute_Residual_From_Qlp_Coefficients_64Bit(Flac__int32[] data, uint32_t data_Offset, uint32_t data_Len, Flac__int32[] qlp_Coeff, uint32_t order, int lp_Quantization, Flac__int32[] residual);

		/// <summary>
		/// For use when the signal is less-or-equal-to 16 bits-per-sample,
		/// or less-or-equal-to 15 bits-per-sample on a side channel (which
		/// requires 1 extra bit)
		/// </summary>
		void Compute_Residual_From_Qlp_Coefficients_16Bit(Flac__int32[] data, uint32_t data_Offset, uint32_t data_Len, Flac__int32[] qlp_Coeff, uint32_t order, int lp_Quantization, Flac__int32[] residual);

		/// <summary>
		/// Restore the original signal by summing the residual and the
		/// predictor
		/// </summary>
		void Restore_Signal(Flac__int32[] residual, uint32_t data_Len, Flac__int32[] qlp_Coeff, uint32_t order, int lp_Quantization, Flac__int32[] data, uint32_t data_Offset);

		/// <summary>
		/// Generic 64-bit datapath
		/// </summary>
		void Restore_Signal_64Bit(Flac__int32[] residual, uint32_t data_Len, Flac__int32[] qlp_Coeff, uint32_t order, int lp_Quantization, Flac__int32[] data, uint32_t data_Offset);

		/// <summary>
		/// For use when the signal is less-or-equal-to 16 bits-per-sample,
		/// or less-or-equal-to 15 bits-per-sample on a side channel (which
		/// requires 1 extra bit)
		/// </summary>
		void Restore_Signal_16Bit(Flac__int32[] residual, uint32_t data_Len, Flac__int32[] qlp_Coeff, uint32_t order, int lp_Quantization, Flac__int32[] data, uint32_t data_Offset);
	}
}
