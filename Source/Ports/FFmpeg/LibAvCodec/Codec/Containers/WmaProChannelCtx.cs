/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Codec.Containers
{
	/// <summary>
	/// Frame specific decoder context for a single channel
	/// </summary>
	internal class WmaProChannelCtx
	{
		/// <summary>
		/// Length of the previous block
		/// </summary>
		public int16_t Prev_Block_Len;

		/// <summary>
		/// 
		/// </summary>
		public uint8_t Transmit_Coefs;

		/// <summary>
		/// 
		/// </summary>
		public uint8_t Num_Subframes;

		/// <summary>
		/// Subframe length in samples
		/// </summary>
		public readonly uint16_t[] Subframe_Len = new uint16_t[WmaConstants.Max_Subframes];

		/// <summary>
		/// Subframe positions in the current frame
		/// </summary>
		public readonly uint16_t[] Subframe_Offset = new uint16_t[WmaConstants.Max_Subframes];

		/// <summary>
		/// Current subframe number
		/// </summary>
		public uint8_t Cur_Subframe;

		/// <summary>
		/// Number of already processed samples
		/// </summary>
		public uint16_t Decoded_Samples;

		/// <summary>
		/// Channel is part of a group
		/// </summary>
		public uint8_t Grouped;

		/// <summary>
		/// Quantization step for the current subframe
		/// </summary>
		public c_int Quant_Step;

		/// <summary>
		/// Share scale factors between subframes
		/// </summary>
		public int8_t Reuse_Sf;

		/// <summary>
		/// Scaling step for the current subframe
		/// </summary>
		public int8_t Scale_Factor_Step;

		/// <summary>
		/// Maximum scale factor for the current subframe
		/// </summary>
		public c_int Max_Scale_Factor;

		/// <summary>
		/// Resampled and (previously) transmitted scale factor values
		/// </summary>
		public readonly c_int[][] Saved_Scale_Factors = ArrayHelper.Initialize2Arrays<c_int>(2, WmaConstants.Max_Bands);

		/// <summary>
		/// Index for the transmitted scale factor values (used for resampling)
		/// </summary>
		public int8_t Scale_Factor_Idx;

		/// <summary>
		/// Pointer to the scale factor values used for decoding
		/// </summary>
		public CPointer<c_int> Scale_Factors;

		/// <summary>
		/// Index in sf_offsets for the scale factor reference block
		/// </summary>
		public uint8_t Table_Idx;

		/// <summary>
		/// Pointer to the subframe decode buffer
		/// </summary>
		public CPointer<c_float> Coeffs;

		/// <summary>
		/// Number of vector coded coefficients
		/// </summary>
		public uint16_t Num_Vec_Coeffs;

		/// <summary>
		/// Output buffer
		/// </summary>
		public readonly CPointer<c_float> Out = new CPointer<c_float>(WmaConstants.WmaPro_Block_Max_Size + (WmaConstants.WmaPro_Block_Max_Size / 2));
	}
}
