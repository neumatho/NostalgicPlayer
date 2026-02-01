/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Demuxer.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat
{
	/// <summary>
	/// RIFF demuxing functions and data
	/// </summary>
	public static class RiffDec
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int FF_GuidCmp(CPointer<uint8_t> g1, FF_Asf_Guid g2)
		{
			return CMemory.memcmp(g1, g2.Data, 16);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int FF_GuidCmp(FF_Asf_Guid g1, FF_Asf_Guid g2)
		{
			return FF_GuidCmp(g1.Data, g2);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int FF_Get_Guid(AvIoContext s, out FF_Asf_Guid g)//XX 33
		{
			g = new FF_Asf_Guid(new uint8_t[16]);
			
			c_int ret = AvIoBuf.FFIo_Read_Size(s, g.Data, g.Data.Length);

			if (ret < 0)
			{
				CMemory.memset<uint8_t>(g.Data, 0, 16);

				return ret;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// "big_endian" values are needed for RIFX file format
		/// </summary>
		/********************************************************************/
		public static c_int FF_Get_Wav_Header(AvFormatContext s, AvIoContext pb, AvCodecParameters par, c_int size, c_int big_Endian)//XX 95
		{
			c_int id, channels = 0, ret;
			uint64_t bitRate = 0;

			if (size < 14)
			{
				Log.AvPriv_Request_Sample(s, "wav header size < 14");

				return Error.InvalidData;
			}

			Channel_Layout.Av_Channel_Layout_Uninit(par.Ch_Layout);

			par.Codec_Type = AvMediaType.Audio;

			if (big_Endian == 0)
			{
				id = (c_int)AvIoBuf.AvIo_RL16(pb);

				if (id != 0x0165)
				{
					channels = (c_int)AvIoBuf.AvIo_RL16(pb);
					par.Sample_Rate = (c_int)AvIoBuf.AvIo_RL32(pb);
					bitRate = AvIoBuf.AvIo_RL32(pb) * 8UL;
					par.Block_Align = (c_int)AvIoBuf.AvIo_RL16(pb);
				}
			}
			else
			{
				id = (c_int)AvIoBuf.AvIo_RB16(pb);
				channels = (c_int)AvIoBuf.AvIo_RB16(pb);
				par.Sample_Rate = (c_int)AvIoBuf.AvIo_RB32(pb);
				bitRate = AvIoBuf.AvIo_RB32(pb) * 8UL;
				par.Block_Align = (c_int)AvIoBuf.AvIo_RB16(pb);
			}

			if (size == 14)		// We're dealing with plain vanilla WAVEFORMAT
				par.Bits_Per_Coded_Sample = 8;
			else
			{
				if (big_Endian == 0)
					par.Bits_Per_Coded_Sample = (c_int)AvIoBuf.AvIo_RL16(pb);
				else
					par.Bits_Per_Coded_Sample = (c_int)AvIoBuf.AvIo_RB16(pb);
			}

			if (id == 0xfffe)
				par.Codec_Tag = 0;
			else
			{
				par.Codec_Tag = (uint32_t)id;
				par.Codec_Id = FF_Wav_Codec_Get_Id((c_uint)id, par.Bits_Per_Coded_Sample);
			}

			if ((size >= 18) && (id != 0x0165))	// We're obviously dealing with WAVEFORMATEX
			{
				c_int cbSize = (c_int)AvIoBuf.AvIo_RL16(pb);	// cbSize

				if (big_Endian != 0)
				{
					Log.AvPriv_Report_Missing_Feature(s, "WAVEFORMATEX support for RIFX files");

					return Error.PatchWelcome;
				}

				size -= 18;
				cbSize = Macros.FFMin(size, cbSize);

				if ((cbSize >= 22) && (id == 0xfffe))	// WAVEFORMATEXTENSIBLE
				{
					Parse_WaveFormatEx(s, pb, par);

					cbSize -= 22;
					size -= 22;
				}

				if (cbSize > 0)
				{
					ret = Demux_Utils.FF_Get_ExtraData(s, par, pb, cbSize);

					if (ret < 0)
						return ret;

					size -= cbSize;
				}

				// It is possible for the chunk to contain garbage at the end
				if (size > 0)
					AvIoBuf.AvIo_Skip(pb, size);
			}
			else if ((id == 0x0165) && (size >= 32))
			{
				size -= 4;

				ret = Demux_Utils.FF_Get_ExtraData(s, par, pb, size);

				if (ret < 0)
					return ret;

				CPointer<uint8_t> extraData = ((DataBufferContext)par.ExtraData).Data;

				c_int nb_Streams = (c_int)IntReadWrite.Av_RL16(extraData + 4);
				par.Sample_Rate = (c_int)IntReadWrite.Av_RL32(extraData + 12);
				channels = 0;
				bitRate = 0;

				if (size < (8 + (nb_Streams * 20)))
					return Error.InvalidData;

				for (c_int i = 0; i < nb_Streams; i++)
					channels += extraData[8 + (i * 20) + 17];
			}

			par.Bit_Rate = (int64_t)bitRate;

			if (par.Sample_Rate <= 0)
			{
				Log.Av_Log(s, Log.Av_Log_Error, "Invalid sample rate: %d\n", par.Sample_Rate);

				return Error.InvalidData;
			}

			if (par.Codec_Id == AvCodecId.Aac_Latm)
			{
				// Channels and sample_rate values are those prior to applying SBR
				// and/or PS
				channels = 0;
				par.Sample_Rate = 0;
			}

			// Override bits_per_coded_sample for G.726
			if ((par.Codec_Id == AvCodecId.Adpcm_G726) && (par.Sample_Rate != 0))
				par.Bits_Per_Coded_Sample = (c_int)(par.Bit_Rate / par.Sample_Rate);

			// Ignore WAVEFORMATEXTENSIBLE layout if different from channel count
			if (channels != par.Ch_Layout.Nb_Channels)
			{
				Channel_Layout.Av_Channel_Layout_Uninit(par.Ch_Layout);

				par.Ch_Layout.Order = AvChannelOrder.Unspec;
				par.Ch_Layout.Nb_Channels = channels;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static AvCodecId FF_Wav_Codec_Get_Id(c_uint tag, c_int bps)//XX 207
		{
			AvCodecId id = Utils_Format.FF_Codec_Get_Id(Riff.FF_Codec_Wav_Tags, tag);

			if (id <= 0)
				return id;

			if (id == AvCodecId.Pcm_S16Le)
				id = Utils_Format.FF_Get_Pcm_Codec_Id(bps, 0, 0, ~1);
			else if (id == AvCodecId.Pcm_F32Le)
				id = Utils_Format.FF_Get_Pcm_Codec_Id(bps, 1, 0, 0);

			if ((id == AvCodecId.Adpcm_Ima_Wav) && (bps == 8))
				id = AvCodecId.Adpcm_Zork;

			return id;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static AvCodecId FF_Codec_Guid_Get_Id(CPointer<AvCodecGuid> guids, FF_Asf_Guid guid)//XX 45
		{
			for (c_int i = 0; i < guids.Length; i++)
			{
				if (FF_GuidCmp(guids[i].Guid, guid) == 0)
					return guids[i].Id;
			}

			return AvCodecId.None;
		}



		/********************************************************************/
		/// <summary>
		/// We could be given one of the three possible structures here:
		/// WAVEFORMAT, PCMWAVEFORMAT or WAVEFORMATEX. Each structure is an
		/// expansion of the previous one with the fields added at the
		/// bottom. PCMWAVEFORMAT adds 'WORD wBitsPerSample' and WAVEFORMATEX
		/// adds 'WORD  cbSize' and basically makes itself an openended
		/// structure
		/// </summary>
		/********************************************************************/
		private static void Parse_WaveFormatEx(IClass logCtx, AvIoContext pb, AvCodecParameters par)//XX 62
		{
			c_int bps = (c_int)AvIoBuf.AvIo_RL16(pb);

			if (bps != 0)
				par.Bits_Per_Coded_Sample = bps;

			uint64_t mask = AvIoBuf.AvIo_RL32(pb);		// dwChannelMask
			Channel_Layout.Av_Channel_Layout_From_Mask(par.Ch_Layout, (AvChannelMask)mask);

			FF_Get_Guid(pb, out FF_Asf_Guid subFormat);

			if ((CMemory.memcmp(subFormat.Data + 4, Riff.FF_Ambisonic_Base_Guid, 12) == 0) ||
				(CMemory.memcmp(subFormat.Data + 4, Riff.FF_Broken_Base_Guid, 12) == 0) ||
				(CMemory.memcmp(subFormat.Data + 4, Riff.FF_MediaSubType_Base_Guid, 12) == 0))
			{
				par.Codec_Tag = IntReadWrite.Av_RL32(subFormat.Data);
				par.Codec_Id = FF_Wav_Codec_Get_Id(par.Codec_Tag, par.Bits_Per_Coded_Sample);
			}
			else
			{
				par.Codec_Id = FF_Codec_Guid_Get_Id(Riff.FF_Codec_Wav_Guids, subFormat);

				if (par.Codec_Id == AvCodecId.None)
				{
					Log.Av_Log(logCtx, Log.Av_Log_Warning, "unknown subformat:" + Riff.FF_Pri_Guid + "\n", Riff.FF_Arg_Guid(subFormat));
				}
			}
		}
		#endregion
	}
}
