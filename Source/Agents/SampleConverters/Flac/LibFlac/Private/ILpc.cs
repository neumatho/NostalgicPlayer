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
		/// Generic 32-bit datapath
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
