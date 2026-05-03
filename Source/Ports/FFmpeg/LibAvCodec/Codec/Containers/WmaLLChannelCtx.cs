/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Codec.Containers
{
	/// <summary>
	/// Frame-specific decoder context for a single channel
	/// </summary>
	internal class WmaLLChannelCtx
	{
		/// <summary>
		/// Length of the previous block
		/// </summary>
		public int16_t Prev_Block_Len;

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
		public readonly uint16_t[] Subframe_Offsets = new uint16_t[WmaConstants.Max_Subframes];

		/// <summary>
		/// Current subframe number
		/// </summary>
		public uint8_t Cur_Subframe;

		/// <summary>
		/// Number of already processed samples
		/// </summary>
		public uint16_t Decoded_Samples;

		/// <summary>
		/// Number of transient samples from the beginning of the transient zone
		/// </summary>
		public c_int Transient_Counter;
	}
}
