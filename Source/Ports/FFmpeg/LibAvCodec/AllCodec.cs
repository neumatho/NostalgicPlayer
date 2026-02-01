/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Codec;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// 
	/// </summary>
	public static class AllCodec
	{
		private static readonly FFCodec[] codec_List =
		[
			WmaDec.FF_Wma2_Decoder
		];

		private delegate bool Test_Delegate(AvCodec codec);

		private static readonly pthread_once_t av_Codec_Static_Init_Done = CThread.pthread_once_init();

		/********************************************************************/
		/// <summary>
		/// Iterate over all registered codecs
		/// </summary>
		/********************************************************************/
		public static IEnumerable<AvCodec> Av_Codec_Iterate()//XX 988
		{
			CThread.pthread_once(av_Codec_Static_Init_Done, Av_Codec_Init_Static);

			return codec_List.Select(c => c.P);
		}



		/********************************************************************/
		/// <summary>
		/// Find a registered encoder with a matching codec ID
		/// </summary>
		/********************************************************************/
		public static AvCodec AvCodec_Find_Encoder(AvCodecId id)//XX 1032
		{
			return Find_Codec(id, Codec_Internal.FF_Codec_Is_Encoder);
		}



		/********************************************************************/
		/// <summary>
		/// Find a registered decoder with a matching codec ID
		/// </summary>
		/********************************************************************/
		public static AvCodec AvCodec_Find_Decoder(AvCodecId id)//XX 1037
		{
			return Find_Codec(id, Codec_Internal.FF_Codec_Is_Decoder);
		}



		/********************************************************************/
		/// <summary>
		/// Find a registered decoder with the specified name
		/// </summary>
		/********************************************************************/
		public static AvCodec AvCodec_Find_Decoder_By_Name(CPointer<char> name)//XX 1065
		{
			return Find_Codec_By_Name(name, Codec_Internal.FF_Codec_Is_Decoder);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Av_Codec_Init_Static()
		{
			foreach (FFCodec codec in codec_List)
			{
				if (codec.Get_Supported_Config == null)
					continue;

				switch (codec.P.Type)
				{
					case AvMediaType.Video:
						throw new NotImplementedException("Codecs of type video not supported");

					case AvMediaType.Audio:
					{
						object value;

						codec.Get_Supported_Config(null, codec.P, AvCodecConfig.Sample_Format, 0, out value, out _);
						codec.P.Sample_Fmts = (AvSampleFormat[])value;

						codec.Get_Supported_Config(null, codec.P, AvCodecConfig.Sample_Rate, 0, out value, out _);
						codec.P.Supported_SampleRates = (CPointer<c_int>)value;

						codec.Get_Supported_Config(null, codec.P, AvCodecConfig.Channel_Layout, 0, out value, out _);
						codec.P.Ch_Layouts = (CPointer<AvChannelLayout>)value;
						break;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static AvCodecId Remap_Deprecated_Codec_Id(AvCodecId id)//XX 1002
		{
			switch (id)
			{
				// This is for future deprecated codec ids, its empty since
				// last major bump but will fill up again over time, please don't remove it
				default:
					return id;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static AvCodec Find_Codec(AvCodecId id, Test_Delegate x)//XX 1011
		{
			AvCodec experimental = null;

			id = Remap_Deprecated_Codec_Id(id);

			foreach (AvCodec p in Av_Codec_Iterate())
			{
				if (!x(p))
					continue;

				if (p.Id == id)
				{
					if (p.Capabilities.HasFlag(AvCodecCap.Experimental) && (experimental == null))
						experimental = p;
					else
						return p;
				}
			}

			return experimental;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static AvCodec Find_Codec_By_Name(CPointer<char> name, Test_Delegate x)//XX 1042
		{
			if (name.IsNull)
				return null;

			foreach (AvCodec p in Av_Codec_Iterate())
			{
				if (!x(p))
					continue;

				if (CString.strcmp(name, p.Name) == 0)
					return p;
			}

			return null;
		}
		#endregion
	}
}
