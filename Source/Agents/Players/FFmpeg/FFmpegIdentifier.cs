/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.FFmpeg
{
	internal class FFmpegIdentifier
	{
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public static readonly string[] FileExtensions =
		[
			"wma"
		];



		/********************************************************************/
		/// <summary>
		/// Tests the module to see which type of module it is
		/// </summary>
		/********************************************************************/
		public static Guid? TestModule(PlayerFileInfo fileInfo)
		{
			Log.Av_Log_Set_Level(Log.Av_Log_Quiet);

			fileInfo.ModuleStream.Seek(0, SeekOrigin.Begin);

			AvIoContext ctx = null;
			AvFormatContext fmt = null;

			try
			{
				ctx = AvIoBuf.Avio_Alloc_Context(null, 0, 0, fileInfo.ModuleStream, FFmpegReader.ReadFromStream, null, FFmpegReader.SeekInStream);
				if (ctx == null)
					return null;

				fmt = Options_Format.AvFormat_Alloc_Context();
				if (fmt == null)
					return null;

				fmt.Pb = ctx;
				fmt.Flags |= AvFmtFlag.Custom_Io;

				int err = Demux.AvFormat_Open_Input(ref fmt, null, null, null);
				if (err < 0)
					return null;

				// Check streams. Only audio and attached picture streams are supported
				AvCodecId audioCodec = AvCodecId.None;

				for (int i = 0; i < fmt.Nb_Streams; i++)
				{
					AvStream stream = fmt.Streams[i];
					AvCodecParameters codecPar = stream.CodecPar;

					if (codecPar.Codec_Type == AvMediaType.Audio)
					{
						if (audioCodec != AvCodecId.None)
							return null;

						audioCodec = codecPar.Codec_Id;
					}
					else if (codecPar.Codec_Type == AvMediaType.Video)
					{
						if ((stream.Disposition & AvDisposition.Attached_Pic) == 0)
							return null;
					}
					else
						return null;
				}

				switch (audioCodec)
				{
					case AvCodecId.WmaV2:
						return FFmpeg.Agent2Id;
				}

				return null;
			}
			finally
			{
				Demux.AvFormat_Close_Input(ref fmt);
				AvIoBuf.AvIo_Context_Free(ref ctx);
			}
		}
	}
}
