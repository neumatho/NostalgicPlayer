/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class EncodeContext : AvCodecInternal
	{
		/// <summary>
		/// 
		/// </summary>
		public AvCodecInternal Avci => this;

		// <summary>
		// This is set to AV_PKT_FLAG_KEY for encoders that encode intra-only
		// formats (i.e. whose codec descriptor has AV_CODEC_PROP_INTRA_ONLY set).
		// This is used to set said flag generically for said encoders
		// </summary>
//		public AvFrameFlag Intra_Only_Flag;

		// <summary>
		// An audio frame with less than required samples has been submitted (and
		// potentially padded with silence). Reject all subsequent frames
		// </summary>
//		public c_int Last_Audio_Frame;
	}
}
