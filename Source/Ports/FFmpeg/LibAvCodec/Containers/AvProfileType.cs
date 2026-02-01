/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public enum AvProfileType
	{
		/// <summary>
		/// 
		/// </summary>
		Unknown = -99,

		/// <summary>
		/// 
		/// </summary>
		Reserved = -100,

		/// <summary>
		/// 
		/// </summary>
		Aac_Main = 0,

		/// <summary>
		/// 
		/// </summary>
		Aac_Low = 1,

		/// <summary>
		/// 
		/// </summary>
		Aac_Ssr = 2,

		/// <summary>
		/// 
		/// </summary>
		Aac_Ltp = 3,

		/// <summary>
		/// 
		/// </summary>
		Aac_He = 4,

		/// <summary>
		/// 
		/// </summary>
		Aac_He_V2 = 28,

		/// <summary>
		/// 
		/// </summary>
		Aac_Ld = 22,

		/// <summary>
		/// 
		/// </summary>
		Aac_Eld = 38,

		/// <summary>
		/// 
		/// </summary>
		Aac_Usac = 41,

		/// <summary>
		/// 
		/// </summary>
		Mpeg2_Aac_Low = 128,

		/// <summary>
		/// 
		/// </summary>
		Mpeg2_Aac_He = 131,

		/// <summary>
		/// 
		/// </summary>
		DnxHd = 0,

		/// <summary>
		/// 
		/// </summary>
		DnxHr_Lb = 1,

		/// <summary>
		/// 
		/// </summary>
		DnxHr_Sq = 2,

		/// <summary>
		/// 
		/// </summary>
		DnxHr_Hq = 3,

		/// <summary>
		/// 
		/// </summary>
		DnxHr_Hqx = 4,

		/// <summary>
		/// 
		/// </summary>
		DnxHr_444 = 5,

		/// <summary>
		/// 
		/// </summary>
		Dts = 20,

		/// <summary>
		/// 
		/// </summary>
		Dts_Es = 30,

		/// <summary>
		/// 
		/// </summary>
		Dts_96_24 = 40,

		/// <summary>
		/// 
		/// </summary>
		Dts_Hd_Hra = 50,

		/// <summary>
		/// 
		/// </summary>
		Dts_Hd_Ma = 60,

		/// <summary>
		/// 
		/// </summary>
		Dts_Express = 70,

		/// <summary>
		/// 
		/// </summary>
		Dts_Hd_Ma_X = 61,

		/// <summary>
		/// 
		/// </summary>
		Dts_Hd_Ma_X_Imax = 62,

		/// <summary>
		/// 
		/// </summary>
		Eac3_Ddp_Atmos = 30,

		/// <summary>
		/// 
		/// </summary>
		TrueHd_Atmos = 30,

		/// <summary>
		/// 
		/// </summary>
		Mpeg2_422 = 0,

		/// <summary>
		/// 
		/// </summary>
		Mpeg2_High = 1,

		/// <summary>
		/// 
		/// </summary>
		Mpeg2_Ss = 2,

		/// <summary>
		/// 
		/// </summary>
		Mpeg2_Snr_Scalable = 3,

		/// <summary>
		/// 
		/// </summary>
		Mpeg2_Main = 4,

		/// <summary>
		/// 
		/// </summary>
		Mpeg2_Simple = 5,

		/// <summary>
		/// 
		/// </summary>
		H264_Constrained = 1 << 9,		// 512

		/// <summary>
		/// 
		/// </summary>
		H264_Intra = 1 << 11,			// 2048

		/// <summary>
		/// 
		/// </summary>
		H264_Baseline = 66,

		/// <summary>
		/// 
		/// </summary>
		H264_Constrained_Baseline = 66 | (1 << 9),

		/// <summary>
		/// 
		/// </summary>
		H264_Main = 77,

		/// <summary>
		/// 
		/// </summary>
		H264_Extended = 88,

		/// <summary>
		/// 
		/// </summary>
		H264_High = 100,

		/// <summary>
		/// 
		/// </summary>
		H264_High_10 = 110,

		/// <summary>
		/// 
		/// </summary>
		H264_High_10_Intra = 110 | (1 << 11),

		/// <summary>
		/// 
		/// </summary>
		H264_Multiview_High = 118,

		/// <summary>
		/// 
		/// </summary>
		H264_High_422 = 122,

		/// <summary>
		/// 
		/// </summary>
		H264_High_422_Intra = 122 | (1 << 11),

		/// <summary>
		/// 
		/// </summary>
		H264_Stereo_High = 128,

		/// <summary>
		/// 
		/// </summary>
		H264_High_444 = 144,

		/// <summary>
		/// 
		/// </summary>
		H264_High_444_Predictive = 244,

		/// <summary>
		/// 
		/// </summary>
		H264_High_444_Intra = 244 | (1 << 11),

		/// <summary>
		/// 
		/// </summary>
		H264_Cavlc_444 = 44,

		/// <summary>
		/// 
		/// </summary>
		Vc1_Simple = 0,

		/// <summary>
		/// 
		/// </summary>
		Vc1_Main = 1,

		/// <summary>
		/// 
		/// </summary>
		Vc1_Complex = 2,

		/// <summary>
		/// 
		/// </summary>
		Vc1_Advanced = 3,

		/// <summary>
		/// 
		/// </summary>
		Mpeg4_Simple = 0,

		/// <summary>
		/// 
		/// </summary>
		Mpeg4_Simple_Scalable = 1,

		/// <summary>
		/// 
		/// </summary>
		Mpeg4_Core = 2,

		/// <summary>
		/// 
		/// </summary>
		Mpeg4_Main = 3,

		/// <summary>
		/// 
		/// </summary>
		Mpeg4_N_Bit = 4,

		/// <summary>
		/// 
		/// </summary>
		Mpeg4_Scalable_Texture = 5,

		/// <summary>
		/// 
		/// </summary>
		Mpeg4_Simple_Face_Animation = 6,

		/// <summary>
		/// 
		/// </summary>
		Mpeg4_Basic_Animated_Texture = 7,

		/// <summary>
		/// 
		/// </summary>
		Mpeg4_Hybrid = 8,

		/// <summary>
		/// 
		/// </summary>
		Mpeg4_Advanced_Real_Time = 9,

		/// <summary>
		/// 
		/// </summary>
		Mpeg4_Core_Scalable = 10,

		/// <summary>
		/// 
		/// </summary>
		Mpeg4_Advanced_Coding = 11,

		/// <summary>
		/// 
		/// </summary>
		Mpeg4_Advanced_Core = 12,

		/// <summary>
		/// 
		/// </summary>
		Mpeg4_Advanced_Scalable_Texture = 13,

		/// <summary>
		/// 
		/// </summary>
		Mpeg4_Simple_Studio = 14,

		/// <summary>
		/// 
		/// </summary>
		Mpeg4_Advanced_Simple = 15,

		/// <summary>
		/// 
		/// </summary>
		Jpeg2000_CStream_Restriction_0 = 1,

		/// <summary>
		/// 
		/// </summary>
		Jpeg2000_CStream_Restriction_1 = 2,

		/// <summary>
		/// 
		/// </summary>
		Jpeg2000_CStream_No_Restriction = 32768,

		/// <summary>
		/// 
		/// </summary>
		Jpeg2000_DCinema_2K = 3,

		/// <summary>
		/// 
		/// </summary>
		Jpeg2000_DCinema_4K = 4,

		/// <summary>
		/// 
		/// </summary>
		Vp9_0 = 0,

		/// <summary>
		/// 
		/// </summary>
		Vp9_1 = 1,

		/// <summary>
		/// 
		/// </summary>
		Vp9_2 = 2,

		/// <summary>
		/// 
		/// </summary>
		Vp9_3 = 3,

		/// <summary>
		/// 
		/// </summary>
		Hevc_Main = 1,

		/// <summary>
		/// 
		/// </summary>
		Hevc_Main_10 = 2,

		/// <summary>
		/// 
		/// </summary>
		Hevc_Main_Still_Picture = 3,

		/// <summary>
		/// 
		/// </summary>
		Hevc_Rext = 4,

		/// <summary>
		/// 
		/// </summary>
		Hevc_Multiview_Main = 6,

		/// <summary>
		/// 
		/// </summary>
		Hevc_Scc = 9,

		/// <summary>
		/// 
		/// </summary>
		Vvc_Main_10 = 1,

		/// <summary>
		/// 
		/// </summary>
		Vvc_Main_10_444 = 33,

		/// <summary>
		/// 
		/// </summary>
		Av1_Main = 0,

		/// <summary>
		/// 
		/// </summary>
		Av1_High = 1,

		/// <summary>
		/// 
		/// </summary>
		Av1_Professional = 2,

		/// <summary>
		/// 
		/// </summary>
		Mjpeg_Huffman_Baseline_Dct = 0xC0,

		/// <summary>
		/// 
		/// </summary>
		Mjpeg_Huffman_Extended_Sequential_Dct = 0xC1,

		/// <summary>
		/// 
		/// </summary>
		Mjpeg_Huffman_Progressive_Dct = 0xC2,

		/// <summary>
		/// 
		/// </summary>
		Mjpeg_Huffman_Lossless = 0xC3,

		/// <summary>
		/// 
		/// </summary>
		Mjpeg_Jpeg_Ls = 0xF7,

		/// <summary>
		/// 
		/// </summary>
		Sbc_Msbc = 1,

		/// <summary>
		/// 
		/// </summary>
		ProRes_Proxy = 0,

		/// <summary>
		/// 
		/// </summary>
		ProRes_Lt = 1,

		/// <summary>
		/// 
		/// </summary>
		ProRes_Standard = 2,

		/// <summary>
		/// 
		/// </summary>
		ProRes_Hq = 3,

		/// <summary>
		/// 
		/// </summary>
		ProRes_4444 = 4,

		/// <summary>
		/// 
		/// </summary>
		ProRes_Xq = 5,

		/// <summary>
		/// 
		/// </summary>
		ProRes_Raw = 0,

		/// <summary>
		/// 
		/// </summary>
		ProRes_Raw_Hq = 1,

		/// <summary>
		/// 
		/// </summary>
		Arib_Profile_A = 0,

		/// <summary>
		/// 
		/// </summary>
		Arib_Profile_C = 1,

		/// <summary>
		/// 
		/// </summary>
		Klva_Sync = 0,

		/// <summary>
		/// 
		/// </summary>
		Klva_Async = 1,

		/// <summary>
		/// 
		/// </summary>
		Evc_Baseline = 0,

		/// <summary>
		/// 
		/// </summary>
		Evc_Main = 1,

		/// <summary>
		/// 
		/// </summary>
		Apv_422_10 = 33,

		/// <summary>
		/// 
		/// </summary>
		Apv_422_12 = 44,

		/// <summary>
		/// 
		/// </summary>
		Apv_444_10 = 55,

		/// <summary>
		/// 
		/// </summary>
		Apv_444_12 = 66,

		/// <summary>
		/// 
		/// </summary>
		Apv_4444_10 = 77,

		/// <summary>
		/// 
		/// </summary>
		Apv_4444_12 = 88,

		/// <summary>
		/// 
		/// </summary>
		Apv_400_10 = 99
	}
}
