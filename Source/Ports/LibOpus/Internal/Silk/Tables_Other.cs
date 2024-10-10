﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Tables_Other
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly opus_int16[] Silk_Stereo_Pred_Quant_Q13 =
		[
			-13732, -10050, -8266, -7526, -6500, -5000, -2950,  -820,
			   820,   2950,  5000,  6500,  7526,  8266, 10050, 13732
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly opus_uint8[] Silk_Stereo_Pred_Joint_iCDF =
		[
			249, 247, 246, 245, 244,
			234, 210, 202, 201, 200,
			197, 174,  82,  59,  56,
			 55,  54,  46,  22,  12,
			 11,  10,   9,   7,   0
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly opus_uint8[] Silk_Stereo_Only_Code_Mid_iCDF =
		[
			64, 0
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static readonly opus_uint8[] Silk_LBRR_Flags_2_iCDF =
		[
			203, 150, 0
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static readonly opus_uint8[] Silk_LBRR_Flags_3_iCDF =
		[
			215, 195, 166, 125, 110, 82, 0
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly Pointer<opus_uint8>[] Silk_LBRR_Flags_iCDF_Ptr =
		[
			Silk_LBRR_Flags_2_iCDF,
			Silk_LBRR_Flags_3_iCDF
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly opus_uint8[] Silk_Lsb_iCDF =
		[
			120, 0
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly opus_uint8[] Silk_LTPscale_iCDF =
		[
			128, 64, 0
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly opus_uint8[] Silk_Type_Offset_VAD_iCDF =
		[
			232, 158, 10, 0
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly opus_uint8[] Silk_Type_Offset_No_VAD_iCDF =
		[
			230, 0
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly opus_int16[] Silk_LTPScales_Table_Q14 =
		[
			15565, 12288, 8192
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly opus_uint8[] Silk_Uniform3_iCDF =
		[
			171, 85, 0
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly opus_uint8[] Silk_Uniform4_iCDF =
		[
			192, 128, 64, 0
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly opus_uint8[] Silk_Uniform5_iCDF =
		[
			205, 154, 102, 51, 0
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly opus_uint8[] Silk_Uniform6_iCDF =
		[
			213, 171, 128, 85, 43, 0
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly opus_uint8[] Silk_Uniform8_iCDF =
		[
			224, 192, 160, 128, 96, 64, 32, 0
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly opus_uint8[] Silk_NLSF_Interpolation_Factor_iCDF =
		[
			243, 221, 192, 181, 0
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly opus_int16[][] Silk_Quantization_Offsets_Q10 =
		[
			[ Constants.Offset_Uvl_Q10, Constants.Offset_Uvh_Q10 ],
			[ Constants.Offset_Vl_Q10, Constants.Offset_Vh_Q10 ]
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly opus_uint8[] Silk_NLSF_EXT_iCDF =
		[
			100, 40, 16, 7, 3, 1, 0
		];
	}
}
