/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// 
	/// </summary>
	public static class Codec_Par
	{
		/********************************************************************/
		/// <summary>
		/// Allocate a new AVCodecParameters and set its fields to default
		/// values (unknown/invalid/0). The returned struct must be freed
		/// with avcodec_parameters_free()
		/// </summary>
		/********************************************************************/
		public static AvCodecParameters AvCodec_Parameters_Alloc()//XX 57
		{
			AvCodecParameters par = Mem.Av_MAlloczObj<AvCodecParameters>();

			if (par == null)
				return null;

			Codec_Parameters_Reset(par);

			return par;
		}



		/********************************************************************/
		/// <summary>
		/// Free an AVCodecParameters instance and everything associated with
		/// it and write NULL to the supplied pointer
		/// </summary>
		/********************************************************************/
		public static void AvCodec_Parameters_Free(ref AvCodecParameters pPar)//XX 67
		{
			AvCodecParameters par = pPar;

			if (par == null)
				return;

			Codec_Parameters_Reset(par);

			Mem.Av_FreeP(ref pPar);
		}



		/********************************************************************/
		/// <summary>
		/// Copy the contents of src to dst. Any allocated fields in dst are
		/// freed and replaced with newly allocated duplicates of the
		/// corresponding fields in src
		/// </summary>
		/********************************************************************/
		public static c_int AvCodec_Parameters_Copy(AvCodecParameters dst, AvCodecParameters src)//XX 107
		{
			Codec_Parameters_Reset(dst);

			src.CopyTo(dst);

			dst.Ch_Layout.Clear();
			dst.ExtraData = null;
			dst.Coded_Side_Data.SetToNull();
			dst.Nb_Coded_Side_Data = 0;

			if (src.ExtraData != null)
			{
				dst.ExtraData = src.ExtraData.MakeDeepClone();
/*				dst.ExtraData = Mem.Av_MAllocz<uint8_t>((size_t)src.ExtraData_Size + Defs.Av_Input_Buffer_Padding_Size);

				if (dst.ExtraData.IsNull)
					return Error.ENOMEM;

				CMemory.memcpy(dst.ExtraData, src.ExtraData, (size_t)src.ExtraData_Size);
				dst.ExtraData_Size = src.ExtraData_Size;*/
			}

			c_int ret = Codec_Parameters_Copy_Side_Data(ref dst.Coded_Side_Data, ref dst.Nb_Coded_Side_Data, src.Coded_Side_Data, src.Nb_Coded_Side_Data);

			if (ret < 0)
				return ret;

			ret = Channel_Layout.Av_Channel_Layout_Copy(dst.Ch_Layout, src.Ch_Layout);

			if (ret < 0)
				return ret;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Fill the parameters struct based on the values from the supplied
		/// codec context. Any allocated fields in par are freed and
		/// replaced with duplicates of the corresponding fields in codec
		/// </summary>
		/********************************************************************/
		public static c_int AvCodec_Parameters_From_Context(AvCodecParameters par, AvCodecContext codec)//XX 138
		{
			c_int ret;

			Codec_Parameters_Reset(par);

			par.Codec_Type = codec.Codec_Type;
			par.Codec_Id = codec.Codec_Id;
			par.Codec_Tag = codec.Codec_Tag;

			par.Bit_Rate = codec.Bit_Rate;
			par.Bits_Per_Coded_Sample = codec.Bits_Per_Coded_Sample;
			par.Bits_Per_Raw_Sample = codec.Bits_Per_Raw_Sample;
			par.Profile = codec.Profile;
			par.Level = codec.Level;

			switch (par.Codec_Type)
			{
				case AvMediaType.Video:
				{
					par.Format.Pixel = codec.Pix_Fmt;
					par.Width = codec.PictureSize.Width;
					par.Height = codec.PictureSize.Height;
					par.Field_Order = codec.Field_Order;
					par.Color_Range = codec.Color_Range;
					par.Color_Primaries = codec.Color_Primaries;
					par.Color_Trc = codec.Color_Trc;
					par.Color_Space = codec.ColorSpace;
					par.Chroma_Location = codec.Chroma_Sample_Location;
					par.Sample_Aspect_Ratio = codec.Sample_Aspect_Ratio;
					par.Video_Delay = codec.Has_B_Frames;
					par.FrameRate = codec.FrameRate;
					par.Alpha_Mode = codec.Alpha_Mode;
					break;
				}

				case AvMediaType.Audio:
				{
					par.Format.Sample = codec.Sample_Fmt;

					ret = Channel_Layout.Av_Channel_Layout_Copy(par.Ch_Layout, codec.Ch_Layout);

					if (ret < 0)
						return ret;

					par.Sample_Rate = codec.Sample_Rate;
					par.Block_Align = codec.Block_Align;
					par.Frame_Size = codec.Frame_Size;
					par.Initial_Padding = codec.Initial_Padding;
					par.Trailing_Padding = codec.Trailing_Padding;
					par.Seek_Preroll = codec.Seek_Preroll;
					break;
				}

				case AvMediaType.Subtitle:
				{
					par.Width = codec.PictureSize.Width;
					par.Height = codec.PictureSize.Height;
					break;
				}
			}

			if (codec.ExtraData != null)
			{
/*				par.ExtraData = Mem.Av_MAllocz<uint8_t>((size_t)codec.ExtraData_Size + Defs.Av_Input_Buffer_Padding_Size);

				if (par.ExtraData.IsNull)
					return Error.ENOMEM;

				CMemory.memcpy(par.ExtraData, codec.ExtraData, (size_t)codec.ExtraData_Size);
				par.ExtraData_Size = codec.ExtraData_Size;
*/
				par.ExtraData = codec.ExtraData.MakeDeepClone();
			}

			ret = Codec_Parameters_Copy_Side_Data(ref par.Coded_Side_Data, ref par.Nb_Coded_Side_Data, codec.Coded_Side_Data.Array, (c_int)codec.Coded_Side_Data.Count);

			if (ret < 0)
				return ret;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Fill the codec context based on the values from the supplied
		/// codec parameters. Any allocated fields in codec that have a
		/// corresponding field in par are freed and replaced with duplicates
		/// of the corresponding field in par. Fields in codec that do not
		/// have a counterpart in par are not touched
		/// </summary>
		/********************************************************************/
		public static c_int AvCodec_Parameters_To_Context(AvCodecContext codec, AvCodecParameters par)//XX 205
		{
			c_int ret;

			codec.Codec_Type = par.Codec_Type;
			codec.Codec_Id = par.Codec_Id;
			codec.Codec_Tag = par.Codec_Tag;

			codec.Bit_Rate = par.Bit_Rate;
			codec.Bits_Per_Coded_Sample = par.Bits_Per_Coded_Sample;
			codec.Bits_Per_Raw_Sample = par.Bits_Per_Raw_Sample;
			codec.Profile = par.Profile;
			codec.Level = par.Level;

			switch (par.Codec_Type)
			{
				case AvMediaType.Video:
				{
					codec.Pix_Fmt = par.Format.Pixel;
					codec.PictureSize = new SizeInfo { Width = par.Width, Height = par.Height };
					codec.Field_Order = par.Field_Order;
					codec.Color_Range = par.Color_Range;
					codec.Color_Primaries = par.Color_Primaries;
					codec.Color_Trc = par.Color_Trc;
					codec.ColorSpace = par.Color_Space;
					codec.Chroma_Sample_Location = par.Chroma_Location;
					codec.Sample_Aspect_Ratio = par.Sample_Aspect_Ratio;
					codec.Has_B_Frames = par.Video_Delay;
					codec.FrameRate = par.FrameRate;
					codec.Alpha_Mode = par.Alpha_Mode;
					break;
				}

				case AvMediaType.Audio:
				{
					codec.Sample_Fmt = par.Format.Sample;

					ret = Channel_Layout.Av_Channel_Layout_Copy(codec.Ch_Layout, par.Ch_Layout);

					if (ret < 0)
						return ret;

					codec.Sample_Rate = par.Sample_Rate;
					codec.Block_Align = par.Block_Align;
					codec.Frame_Size = par.Frame_Size;
					codec.Delay = codec.Initial_Padding = par.Initial_Padding;
					codec.Trailing_Padding = par.Trailing_Padding;
					codec.Seek_Preroll = par.Seek_Preroll;
					break;
				}

				case AvMediaType.Subtitle:
				{
					codec.PictureSize = new SizeInfo { Width = par.Width, Height = par.Height };
					break;
				}
			}

			Mem.Av_FreeP(ref codec.ExtraData);

			if (par.ExtraData != null)
			{
/*				codec.ExtraData = Mem.Av_MAllocz<uint8_t>((size_t)par.ExtraData_Size + Defs.Av_Input_Buffer_Padding_Size);

				if (codec.ExtraData.IsNull)
					return Error.ENOMEM;

				CMemory.memcpy(codec.ExtraData, par.ExtraData, (size_t)par.ExtraData_Size);

				codec.ExtraData_Size = par.ExtraData_Size;
*/
				codec.ExtraData = par.ExtraData.MakeDeepClone();
			}

			c_int tmpCount = (c_int)codec.Coded_Side_Data.Count;
			Packet.Av_Packet_Side_Data_Free(ref codec.Coded_Side_Data.Array, ref tmpCount);

			ret = Codec_Parameters_Copy_Side_Data(ref codec.Coded_Side_Data.Array, ref tmpCount, par.Coded_Side_Data, par.Nb_Coded_Side_Data);
			codec.Coded_Side_Data.Count = (c_uint)tmpCount;

			if (ret < 0)
				return ret;

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Codec_Parameters_Reset(AvCodecParameters par)//XX 32
		{
			Mem.Av_FreeP(ref par.ExtraData);
			Channel_Layout.Av_Channel_Layout_Uninit(par.Ch_Layout);
			Packet.Av_Packet_Side_Data_Free(ref par.Coded_Side_Data, ref par.Nb_Coded_Side_Data);

			par.Clear();

			par.Codec_Type = AvMediaType.Unknown;
			par.Codec_Id = AvCodecId.None;
			par.Format.Pixel = AvPixelFormat.None;
			par.Format.Sample = AvSampleFormat.None;
			par.Ch_Layout.Order = AvChannelOrder.Unspec;
			par.Field_Order = AvFieldOrder.Unknown;
			par.Color_Range = AvColorRange.Unspecified;
			par.Color_Primaries = AvColorPrimaries.Unspecified;
			par.Color_Trc = AvColorTransferCharacteristic.Unspecified;
			par.Color_Space = AvColorSpace.Unspecified;
			par.Chroma_Location = AvChromaLocation.Unspecified;
			par.Sample_Aspect_Ratio = new AvRational(0, 1);
			par.FrameRate = new AvRational(0, 1);
			par.Profile = AvProfileType.Unknown;
			par.Level = AvLevel.Unknown;
			par.Alpha_Mode = AvAlphaMode.Unspecified;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Codec_Parameters_Copy_Side_Data(ref CPointer<AvPacketSideData> pDst, ref c_int pNb_Dst, CPointer<AvPacketSideData> src, c_int nb_Src)//XX 78
		{
			CPointer<AvPacketSideData> dst;
			c_int nb_Dst = pNb_Dst;

			if (src.IsNull)
				return 0;

			pDst = dst = Mem.Av_CAllocObj<AvPacketSideData>((size_t)nb_Src);

			if (dst.IsNull)
				return Error.ENOMEM;

			for (c_int i = 0; i < nb_Src; i++)
			{
				AvPacketSideData src_Sd = src[i];
				AvPacketSideData dst_Sd = dst[i];

				dst_Sd.Data = src_Sd.Data.MakeDeepClone();

				if (dst_Sd.Data == null)
					return Error.ENOMEM;

				dst_Sd.Type = src_Sd.Type;

				pNb_Dst = ++nb_Dst;
			}

			return 0;
		}
		#endregion
	}
}
