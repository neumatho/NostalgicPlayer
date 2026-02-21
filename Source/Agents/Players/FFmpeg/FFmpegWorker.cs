/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibSwResample;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibSwResample.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.FFmpeg
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class FFmpegWorker : SamplePlayerWithDurationAgentBase
	{
		private AvIoContext context;
		private AvFormatContext formatContext;
		private AvCodecContext decoderContext;

		private int streamIndex;
		private AvPacket packet;
		private AvFrame frame;

		private SwrContext swrContext;

		private int[] channelMapping;
		private int channels;
		private int frequency;

		private int inputTaken;

		private string songName;
		private string artist;
		private string trackNum;
		private string album;
		private string genre;
		private string copyright;
		private string[] comment;
		private string publisher;
		private string encoder;
		private PictureInfo[] pictures;

		private int bitRate;

		private const int InfoBitRateLine = 6;

		#region Identify
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => FFmpegIdentifier.FileExtensions;



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			return AgentResult.Unknown;
		}
		#endregion

		#region Loading
		/********************************************************************/
		/// <summary>
		/// Will load the header information from the file
		/// </summary>
		/********************************************************************/
		public override AgentResult LoadHeaderInfo(ModuleStream moduleStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			return AgentResult.Ok;
		}
		#endregion

		#region Initialization and cleanup
		/********************************************************************/
		/// <summary>
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public override bool InitPlayer(ModuleStream moduleStream, out string errorMessage)
		{
			if (!base.InitPlayer(moduleStream, out errorMessage))
				return false;

			// Enable debug logging temporarily to diagnose duration issues
			Log.Av_Log_Set_Level(Log.Av_Log_Debug);

			context = AvIoBuf.Avio_Alloc_Context(null, 0, 0, moduleStream, FFmpegReader.ReadFromStream, null, FFmpegReader.SeekInStream);
			if (context == null)
			{
				errorMessage = Resources.IDS_FFMPEG_ERR_INITIALIZE;
				return false;
			}

			formatContext = Options_Format.AvFormat_Alloc_Context();
			if (formatContext == null)
			{
				errorMessage = Resources.IDS_FFMPEG_ERR_INITIALIZE;
				return false;
			}

			formatContext.Pb = context;
			formatContext.Flags |= AvFmtFlag.Custom_Io;

			int err = Demux.AvFormat_Open_Input(ref formatContext, null, null, null);
			if (err < 0)
			{
				errorMessage = Error.Av_Err2Str(err).ToString();
				return false;
			}

			err = Demux.AvFormat_Find_Stream_Info(formatContext, null);
			if (err < 0)
			{
				errorMessage = Error.Av_Err2Str(err).ToString();
				return false;
			}

			streamIndex = AvFormat.Av_Find_Best_Stream(formatContext, AvMediaType.Audio, -1, -1, out _, 0);
			if (streamIndex < 0)
			{
				errorMessage = Error.Av_Err2Str(err).ToString();
				return false;
			}

			AvStream stream = formatContext.Streams[streamIndex];
			AvCodec decoder = AllCodec.AvCodec_Find_Decoder(stream.CodecPar.Codec_Id);
			if (decoder == null)
			{
				errorMessage = Resources.IDS_FFMPEG_ERR_INITIALIZE;
				return false;
			}

			decoderContext = Options_Codec.AvCodec_Alloc_Context3(decoder);
			if (decoderContext == null)
			{
				errorMessage = Resources.IDS_FFMPEG_ERR_INITIALIZE;
				return false;
			}

			err = Codec_Par.AvCodec_Parameters_To_Context(decoderContext, stream.CodecPar);
			if (err < 0)
			{
				errorMessage = Error.Av_Err2Str(err).ToString();
				return false;
			}

			AvDictionary dict = null;
			err = AvCodec_.AvCodec_Open2(decoderContext, decoder, ref dict);
			if (err < 0)
			{
				errorMessage = Error.Av_Err2Str(err).ToString();
				return false;
			}

			// Get player data
			channels = decoderContext.Ch_Layout.Nb_Channels;
			frequency = decoderContext.Sample_Rate;
			bitRate = (int)(decoderContext.Bit_Rate / 1000);

			if (channels > 8)
			{
				errorMessage = string.Format(Resources.IDS_FFMPEG_ERR_ILLEGAL_CHANNELS, channels);
				return false;
			}

			// Build channel mapping table if possible
			BuildChannelMapping(decoderContext.Ch_Layout);

			// Get meta data
			ExtractMetadata();
			ExtractPictures();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the player
		/// </summary>
		/********************************************************************/
		public override void CleanupPlayer()
		{
			SwResample.Swr_Free(ref swrContext);

			Demux.AvFormat_Close_Input(ref formatContext);
			Options_Codec.AvCodec_Free_Context(ref decoderContext);
			AvIoBuf.AvIo_Context_Free(ref context);

			base.CleanupPlayer();
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the player to start the sample from start
		/// </summary>
		/********************************************************************/
		public override bool InitSound(out string errorMessage)
		{
			if (!base.InitSound(out errorMessage))
				return false;

			packet = Packet.Av_Packet_Alloc();
			if (packet == null)
			{
				errorMessage = Resources.IDS_FFMPEG_ERR_INITIALIZE;
				return false;
			}

			frame = Frame.Av_Frame_Alloc();
			if (frame == null)
			{
				errorMessage = Resources.IDS_FFMPEG_ERR_INITIALIZE;
				return false;
			}

			inputTaken = 0;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the current song
		/// </summary>
		/********************************************************************/
		public override void CleanupSound()
		{
			Packet.Av_Packet_Free(ref packet);
			Frame.Av_Frame_Free(ref frame);
		}
		#endregion

		#region Playing
		/********************************************************************/
		/// <summary>
		/// Will load and decode a data block and store it in the buffer
		/// given
		/// </summary>
		/********************************************************************/
		public override int LoadDataBlock(int[][] outputBuffer, int countInFrames)
		{
			// Load the next block of data
			int filledInFrames = LoadData(outputBuffer, countInFrames);

			if (filledInFrames == 0)
			{
				OnEndReached();

				// Loop the sample
				CleanupSound();
				InitSound(out _);
			}

			// Has the bit rate changed
			double seconds = frame.Nb_Samples / (double)frame.Sample_Rate;
			int newBitRate = (int)Math.Round(((packet.Size * 8) / seconds) / 1000, MidpointRounding.AwayFromZero);

			if ((newBitRate >= 0) && (newBitRate != bitRate))
			{
				bitRate = newBitRate;

				OnModuleInfoChanged(InfoBitRateLine, bitRate.ToString());
			}

			return filledInFrames;
		}
		#endregion

		#region Information
		/********************************************************************/
		/// <summary>
		/// Return the title
		/// </summary>
		/********************************************************************/
		public override string Title => songName;



		/********************************************************************/
		/// <summary>
		/// Return the name of the author
		/// </summary>
		/********************************************************************/
		public override string Author => artist;



		/********************************************************************/
		/// <summary>
		/// Return the comment separated in lines
		/// </summary>
		/********************************************************************/
		public override string[] Comment => comment;



		/********************************************************************/
		/// <summary>
		/// Return all pictures available
		/// </summary>
		/********************************************************************/
		public override PictureInfo[] Pictures => pictures;



		/********************************************************************/
		/// <summary>
		/// Return which speakers the player uses.
		/// 
		/// Note that the outputBuffer in LoadDataBlock match the defined
		/// order in SpeakerFlag enum
		/// </summary>
		/********************************************************************/
		public override SpeakerFlag SpeakerFlags => Tables.ChannelToSpeaker[channels - 1];



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the sample uses
		/// </summary>
		/********************************************************************/
		public override int ChannelCount => channels;



		/********************************************************************/
		/// <summary>
		/// Return the frequency the sample is stored with
		/// </summary>
		/********************************************************************/
		public override int Frequency => frequency;



		/********************************************************************/
		/// <summary>
		/// Returns the description and value on the line given. If the line
		/// is out of range, false is returned
		/// </summary>
		/********************************************************************/
		public override bool GetInformationString(int line, out string description, out string value)
		{
			// Find out which line to take
			switch (line)
			{
				// Track number
				case 0:
				{
					description = Resources.IDS_FFMPEG_INFODESCLINE0;
					value = trackNum;
					break;
				}

				// Album
				case 1:
				{
					description = Resources.IDS_FFMPEG_INFODESCLINE1;
					value = album;
					break;
				}

				// Genre
				case 2:
				{
					description = Resources.IDS_FFMPEG_INFODESCLINE2;
					value = genre;
					break;
				}

				// Copyright
				case 3:
				{
					description = Resources.IDS_FFMPEG_INFODESCLINE3;
					value = copyright;
					break;
				}

				// Publisher
				case 4:
				{
					description = Resources.IDS_FFMPEG_INFODESCLINE4;
					value = publisher;
					break;
				}

				// Encoder
				case 5:
				{
					description = Resources.IDS_FFMPEG_INFODESCLINE5;
					value = encoder;
					break;
				}

				// Bit rate
				case 6:
				{
					description = Resources.IDS_FFMPEG_INFODESCLINE6;
					value = bitRate.ToString();
					break;
				}

				// Frequency
				case 7:
				{
					description = Resources.IDS_FFMPEG_INFODESCLINE7;
					value = frequency.ToString();
					break;
				}

				default:
				{
					description = null;
					value = null;

					return false;
				}
			}

			return true;
		}
		#endregion

		#region Duration calculation
		/********************************************************************/
		/// <summary>
		/// Return the total time of the sample
		/// </summary>
		/********************************************************************/
		protected override TimeSpan GetTotalDuration()
		{
			long duration = formatContext.Duration;
			if (duration == UtilConstants.Av_NoPts_Value)
				return TimeSpan.Zero;

			double containerSeconds = (double)duration / UtilConstants.Av_Time_Base;

			return new TimeSpan((long)(containerSeconds * TimeSpan.TicksPerSecond));
		}



		/********************************************************************/
		/// <summary>
		/// Set the position in the playing sample to the time given
		/// </summary>
		/********************************************************************/
		protected override void SetPosition(TimeSpan time)
		{
			double seconds = time.TotalSeconds;

			Seek.Av_Seek_Frame(formatContext, -1, (long)(seconds * UtilConstants.Av_Time_Base), AvSeekFlag.Any);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Build a channel mapping table
		/// </summary>
		/********************************************************************/
		private void BuildChannelMapping(AvChannelLayout channelLayout)
		{
			if (channelLayout.Order == AvChannelOrder.Unspec)
				return;		// We don't have the layout yet

			Dictionary<SpeakerFlag, int> speakerToOutputChannel = BuildSpeakerMap(SpeakerFlags);

			channelMapping = new int[channelLayout.Nb_Channels];

			for (uint i = 0; i < channelMapping.Length; i++)
			{
				AvChannel channel = Channel_Layout.Av_Channel_Layout_Channel_From_Index(channelLayout, i);
				SpeakerFlag speaker = MapFromChannelToSpeaker(channel);

				if (!speakerToOutputChannel.TryGetValue(speaker, out channelMapping[i]))
					channelMapping[i] = -1;
			}

			InitSampleConversion(channelLayout);
		}



		/********************************************************************/
		/// <summary>
		/// Convert speaker flags into a channel mapping
		/// </summary>
		/********************************************************************/
		private Dictionary<SpeakerFlag, int> BuildSpeakerMap(SpeakerFlag speakers)
		{
			Dictionary<SpeakerFlag, int> map = new();

			int channel = 0;

			foreach (SpeakerFlag flag in Enum.GetValues<SpeakerFlag>())
			{
				if (speakers.HasFlag(flag))
					map.Add(flag, channel++);
			}

			return map;
		}



		/********************************************************************/
		/// <summary>
		/// Convert AvChannel to speaker flag
		/// </summary>
		/********************************************************************/
		private SpeakerFlag MapFromChannelToSpeaker(AvChannel channel)
		{
			switch (channel)
			{
				case AvChannel.Front_Left:
					return SpeakerFlag.FrontLeft;

				case AvChannel.Front_Right:
					return SpeakerFlag.FrontRight;

				case AvChannel.Front_Center:
					return SpeakerFlag.FrontCenter;

				case AvChannel.Low_Frequency:
					return SpeakerFlag.LowFrequency;

				case AvChannel.Back_Left:
					return SpeakerFlag.BackLeft;

				case AvChannel.Back_Right:
					return SpeakerFlag.BackRight;

				case AvChannel.Front_Left_Of_Center:
					return SpeakerFlag.FrontLeftOfCenter;

				case AvChannel.Front_Right_Of_Center:
					return SpeakerFlag.FrontRightOfCenter;

				case AvChannel.Back_Center:
					return SpeakerFlag.BackCenter;

				case AvChannel.Side_Left:
					return SpeakerFlag.SideLeft;

				case AvChannel.Side_Right:
					return SpeakerFlag.SideRight;

				case AvChannel.Top_Center:
					return SpeakerFlag.TopCenter;

				case AvChannel.Top_Front_Left:
					return SpeakerFlag.TopFrontLeft;

				case AvChannel.Top_Front_Center:
					return SpeakerFlag.TopFrontCenter;

				case AvChannel.Top_Front_Right:
					return SpeakerFlag.TopFrontRight;

				case AvChannel.Top_Back_Left:
					return SpeakerFlag.TopBackLeft;

				case AvChannel.Top_Back_Center:
					return SpeakerFlag.TopBackCenter;

				case AvChannel.Top_Back_Right:
					return SpeakerFlag.TopBackRight;

				default:
					throw new NotSupportedException($"Channel {channel} is not supported");
			}
		}



		/********************************************************************/
		/// <summary>
		/// Extract metadata from container
		/// </summary>
		/********************************************************************/
		private void ExtractMetadata()
		{
			AvDictionary metadata = formatContext.Metadata;

			trackNum = Resources.IDS_FFMPEG_INFO_UNKNOWN;
			album = Resources.IDS_FFMPEG_INFO_UNKNOWN;
			genre = Resources.IDS_FFMPEG_INFO_UNKNOWN;
			copyright = Resources.IDS_FFMPEG_INFO_UNKNOWN;
			comment = [];
			publisher = Resources.IDS_FFMPEG_INFO_UNKNOWN;
			encoder = Resources.IDS_FFMPEG_INFO_UNKNOWN;

			for (int i = 0; i < metadata.Count; i++)
			{
				AvDictionaryEntry entry = metadata.Elems[i];
				string value = entry.Value.ToString();

				switch (entry.Key.ToString())
				{
					case "title":
					{
						songName = value;
						break;
					}

					case "artist":
					{
						artist = value;
						break;
					}

					case "track":
					{
						trackNum = value;
						break;
					}

					case "album":
					{
						album = value;
						break;
					}

					case "genre":
					{
						genre = value;
						break;
					}

					case "copyright":
					{
						copyright = value;
						break;
					}

					case "comment":
					{
						comment = value.Split('\n');
						break;
					}

					case "publisher":
					{
						publisher = value;
						break;
					}

					case "encoder":
					{
						encoder = value;
						break;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Extract attached pictures from streams
		/// </summary>
		/********************************************************************/
		private void ExtractPictures()
		{
			List<PictureInfo> collectedPictures = new List<PictureInfo>();

			for (int i = 0; i < formatContext.Nb_Streams; i++)
			{
				AvStream stream = formatContext.Streams[i];

				if ((stream.Disposition & AvDisposition.Attached_Pic) != 0)
				{
					PictureInfo info = ParsePicture(stream);
					if (info != null)
						collectedPictures.Add(info);
				}
			}

			pictures = collectedPictures.Count > 0 ? collectedPictures.ToArray() : null;
		}



		/********************************************************************/
		/// <summary>
		/// Parse the picture stream
		/// </summary>
		/********************************************************************/
		private PictureInfo ParsePicture(AvStream pictureStream)
		{
			try
			{
				AvPacket picturePacket = pictureStream.Attached_Pic;

				AvDictionary metadata = pictureStream.Metadata;

				string description = null;
				string type = null;

				for (int i = 0; i < metadata.Count; i++)
				{
					AvDictionaryEntry entry = metadata.Elems[i];
					string value = entry.Value.ToString();

					switch (entry.Key.ToString())
					{
						case "comment":
						{
							type = value;
							break;
						}

						case "title":
						{
							description = value;
							break;
						}
					}
				}

				if (string.IsNullOrEmpty(type))
					type = Resources.IDS_FFMPEG_INFO_UNKNOWN;

				if (string.IsNullOrEmpty(description))
					description = type;
				else
					description = $"{type}: {description}";

				return new PictureInfo(picturePacket.Data.ToArray(), description);
			}
			catch (Exception)
			{
				return null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Read next block of data
		/// </summary>
		/********************************************************************/
		private int LoadData(int[][] outputBuffer, int countInFrames)
		{
			int err = 0;
			int offset = 0;
			int totalInFrames = 0;

			while ((countInFrames > 0) && (err == 0))
			{
				if (inputTaken == frame.Nb_Samples)
				{
					Frame.Av_Frame_Unref(frame);
					inputTaken = 0;

					// This part is reversed on what you normally do, so it can
					// be hard to read. This is how ffmpeg works.
					//
					// - Read one frame from the demuxer (input source)
					// - Send that frame to the decoder
					// - Receive one or more frames with the decoded data
					for (;;)
					{
						err = AvCodec_.AvCodec_Receive_Frame(decoderContext, frame);

						if ((err == Error.EAGAIN) || (err == Error.EOF))
						{
							Packet.Av_Packet_Unref(packet);

							while ((err = Demux.Av_Read_Frame(formatContext, packet)) >= 0)
							{
								if (packet.Stream_Index == streamIndex)
								{
									int err2 = Decode.AvCodec_Send_Packet(decoderContext, packet);
									if (err2 < 0)
										Packet.Av_Packet_Unref(packet);

									break;
								}

								Packet.Av_Packet_Unref(packet);
							}

							if (err < 0)
								break;
						}
						else
							break;
					}

					// If channel mapping was not created when initializing the player,
					// (because the layout information is not stored in the container),
					// try with the information from the frame
					if ((channelMapping == null) && (frame.Ch_Layout.Nb_Channels > 0))
					{
						BuildChannelMapping(frame.Ch_Layout);

						// If it still is not possible to create the channel layout,
						// use default layout
						if (channelMapping == null)
						{
							AvChannelLayout layout = new AvChannelLayout();
							Channel_Layout.Av_Channel_Layout_Default(ref layout, channels);

							BuildChannelMapping(layout);
						}
					}
				}

				int todoInFrames = Math.Min(countInFrames, frame.Nb_Samples - inputTaken);
				ConvertSamples(outputBuffer, offset, todoInFrames);

				offset += todoInFrames;
				countInFrames -= todoInFrames;
				totalInFrames += todoInFrames;
				inputTaken += todoInFrames;
			}

			return totalInFrames;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the FFmpeg sample conversion
		/// </summary>
		/********************************************************************/
		private void InitSampleConversion(AvChannelLayout channelLayout)
		{
			int err = SwResample.Swr_Alloc_Set_Opts2(ref swrContext, channelLayout, AvSampleFormat.S32P, decoderContext.Sample_Rate, channelLayout, decoderContext.Sample_Fmt, decoderContext.Sample_Rate, 0, null);
			if (err < 0)
				return;

			err = SwResample.Swr_Init(swrContext);
			if (err < 0)
				swrContext = null;
		}



		/********************************************************************/
		/// <summary>
		/// Convert the input to 32-bit output
		/// </summary>
		/********************************************************************/
		private void ConvertSamples(int[][] outputBuffer, int offset, int todo)
		{
			if ((todo == 0) || (channelMapping == null) || (swrContext == null))
				return;

			int inputBytesPerSample = SampleFmt.Av_Get_Bytes_Per_Sample(frame.Format.Sample);
			int inputByteOffset = inputTaken * inputBytesPerSample;

			CPointer<CPointer<byte>> inBuf = new CPointer<CPointer<byte>>(channels);
			CPointer<CPointer<byte>> outBuf = new CPointer<CPointer<byte>>(channels);

			for (int i = 0; i < channels; i++)
			{
				inBuf[i] = frame.Data[i].Slice(inputByteOffset);

				int outputIndex = channelMapping[i];
				if (outputIndex == -1)
					continue;

				outBuf[i] = new CPointer<int>(outputBuffer[outputIndex], offset).Cast<int, byte>();
			}

			SwResample.Swr_Convert(swrContext, outBuf, todo, inBuf, todo);
		}
		#endregion
	}
}
