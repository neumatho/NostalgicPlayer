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
	internal class DecodeContext : AvCodecInternal
	{
		/// <summary>
		/// 
		/// </summary>
		public AvCodecInternal Avci => this;

		/// <summary>
		/// This is set to AV_FRAME_FLAG_KEY for decoders of intra-only formats
		/// (those whose codec descriptor has AV_CODEC_PROP_INTRA_ONLY set)
		/// to set the flag generically
		/// </summary>
		public AvFrameFlag Intra_Only_Flag;

		/// <summary>
		/// This is set to AV_PICTURE_TYPE_I for intra only video decoders
		/// and to AV_PICTURE_TYPE_NONE for other decoders. It is used to set
		/// the AVFrame's pict_type before the decoder receives it
		/// </summary>
		public AvPictureType Initial_Pict_Type;

		/// <summary>
		/// To prevent infinite loop on errors when draining
		/// </summary>
		public c_int Nb_Draining_Errors;

		/// <summary>
		/// The caller has submitted a NULL packet on input
		/// </summary>
		public c_int Draining_Started;

		/// <summary>
		/// Number of incorrect PTS values so far
		/// </summary>
		public int64_t Pts_Correction_Num_Faulty_Pts;

		/// <summary>
		/// Number of incorrect DTS values so far
		/// </summary>
		public int64_t Pts_Correction_Num_Faulty_Dts;

		/// <summary>
		/// PTS of the last frame
		/// </summary>
		public int64_t Pts_Correction_Last_Pts;

		/// <summary>
		/// DTS of the last frame
		/// </summary>
		public int64_t Pts_Correction_Last_Dts;

		/// <summary>
		/// Bitmask indicating for which side data types we prefer user-supplied
		/// (global or attached to packets) side data over bytestream
		/// </summary>
		public uint64_t Side_Data_Pref_Mask;

		/// <summary>
		/// 
		/// </summary>
		public FFLcevcContext Lcevc;

		/// <summary>
		/// 
		/// </summary>
		public c_int Lcevc_Frame;

		/// <summary>
		/// 
		/// </summary>
		public c_int Width;

		/// <summary>
		/// 
		/// </summary>
		public c_int Height;
	}
}
