/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// 
	/// </summary>
	public static class Options_Codec
	{
		private static readonly AvClass av_Codec_Context_Class = new AvClass//XX 78
		{
			Class_Name = "AVCodecContext".ToCharPointer(),
			Item_Name = Context_To_Name,
			Option = Options_Table.AvCodec_Options,
			Version = Version.Version_Int,
			Log_Level_Offset_Name = nameof(AvCodecContext.Log_Level_Offset),
			Child_Next = Codec_Child_Next,
			Child_Class_Iterate = Codec_Child_Class_Iterate,
			Category = AvClassCategory.Encoder,
			Get_Category = Get_Category
		};

		/********************************************************************/
		/// <summary>
		/// Allocate an AVCodecContext and set its fields to default values.
		/// The resulting struct should be freed with avcodec_free_context()
		/// </summary>
		/********************************************************************/
		public static AvCodecContext AvCodec_Alloc_Context3(AvCodec codec)//XX 149
		{
			AvCodecContext avCtx = Mem.Av_MAllocObj<AvCodecContext>();

			if (avCtx == null)
				return null;

			if (Init_Context_Defaults(avCtx, codec) < 0)
			{
				Mem.Av_Free(avCtx);

				return null;
			}

			return avCtx;
		}



		/********************************************************************/
		/// <summary>
		/// Free the codec context and everything associated with it and
		/// write NULL to the provided pointer
		/// </summary>
		/********************************************************************/
		public static void AvCodec_Free_Context(ref AvCodecContext pavCtx)//XX 164
		{
			AvCodecContext avCtx = pavCtx;

			if (avCtx == null)
				return;

			AvCodec_.FF_Codec_Close(avCtx);

			Mem.Av_FreeP(ref avCtx.ExtraData);
			Mem.Av_FreeP(ref avCtx.Subtitle_Header);
			Mem.Av_FreeP(ref avCtx.Intra_Matrix);
			Mem.Av_FreeP(ref avCtx.Chroma_Intra_Matrix);
			Mem.Av_FreeP(ref avCtx.Inter_Matrix);
			Mem.Av_FreeP(ref avCtx.Rc_Override);
			Channel_Layout.Av_Channel_Layout_Uninit(avCtx.Ch_Layout);

			Mem.Av_FreeP(ref pavCtx);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static CPointer<char> Context_To_Name(IClass ptr)//XX 42
		{
			AvCodecContext avc = (AvCodecContext)ptr;

			if ((avc != null) && (avc.Codec != null))
				return avc.Codec.Name;
			else
				return "NULL".ToCharPointer();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static IEnumerable<IOptionContext> Codec_Child_Next(IOptionContext obj)//XX 51
		{
			AvCodecContext s = (AvCodecContext)obj;

			if ((s.Codec != null) && (s.Codec.Priv_Class != null) && (s.Priv_Data != null))
				yield return s.Priv_Data;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static IEnumerable<AvClass> Codec_Child_Class_Iterate()//XX 59
		{
			// Find next codec with priv options
			foreach (AvCodec c in AllCodec.Av_Codec_Iterate())
			{
				if (c.Priv_Class != null)
					yield return c.Priv_Class;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static AvClassCategory Get_Category(IClass ptr)//XX 69
		{
			AvCodecContext avCtx = (AvCodecContext)ptr;

			if ((avCtx.Codec != null) && Codec_Internal.FF_Codec_Is_Decoder(avCtx.Codec))
				return AvClassCategory.Decoder;
			else
				return AvClassCategory.Encoder;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Init_Context_Defaults(AvCodecContext s, AvCodec codec)//XX 90
		{
			FFCodec codec2 = Codec_Internal.FFCodec(codec);
			AvOptFlag flags = AvOptFlag.None;

			s.Clear();

			av_Codec_Context_Class.CopyTo(s.Av_Class);

			s.Codec_Type = codec != null ? codec.Type : AvMediaType.Unknown;

			if (codec != null)
			{
				s.Codec = codec;
				s.Codec_Id = codec.Id;
			}

			if (s.Codec_Type == AvMediaType.Audio)
				flags = AvOptFlag.Audio_Param;
			else if (s.Codec_Type == AvMediaType.Video)
				flags = AvOptFlag.Video_Param;
			else if (s.Codec_Type == AvMediaType.Subtitle)
				flags = AvOptFlag.Subtitle_Param;

			Opt.Av_Opt_Set_Defaults2(s, flags, flags);

			Channel_Layout.Av_Channel_Layout_Uninit(s.Ch_Layout);

			s.Time_Base = new AvRational(0, 1);
			s.FrameRate = new AvRational(0, 1);
			s.Pkt_TimeBase = new AvRational(0, 1);
			s.Get_Buffer2 = Get_Buffer.AvCodec_Default_Get_Buffer2;
			s.Get_Format = Decode.AvCodec_Default_Get_Format;
			s.Get_Encode_Buffer = Encode.AvCodec_Default_Get_Encode_Buffer;
			s.Execute = AvCodec_.AvCodec_Default_Execute;
			s.Execute2 = AvCodec_.AvCodec_Default_Execute2;
			s.Sample_Aspect_Ratio = new AvRational(0, 1);
			s.Ch_Layout.Order = AvChannelOrder.Unspec;
			s.Pix_Fmt = AvPixelFormat.None;
			s.Sw_Pix_Fmt = AvPixelFormat.None;
			s.Sample_Fmt = AvSampleFormat.None;

			if ((codec != null) && (codec2.Priv_Data_Alloc != null))
			{
				s.Priv_Data = codec2.Priv_Data_Alloc();

				if (s.Priv_Data == null)
					return Error.ENOMEM;

				if (codec.Priv_Class != null)
				{
					codec.Priv_Class.CopyTo(s.Priv_Data);

					Opt.Av_Opt_Set_Defaults(s.Priv_Data);
				}
			}

			if ((codec != null) && (codec2.Defaults != null))
			{
				foreach (FFCodecDefault d in codec2.Defaults)
					Opt.Av_Opt_Set(s, d.Key, d.Value, AvOptSearch.None);
			}

			return 0;
		}
		#endregion
	}
}
