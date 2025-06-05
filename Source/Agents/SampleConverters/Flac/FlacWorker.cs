/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Ports.LibFlac.Flac;
using Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Decoder;
using Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Encoder;
using Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format;

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class FlacWorker : ISampleLoaderAgent, ISampleSaverAgent
	{
		private Stream_Decoder flacDecoder;

		private Flac__StreamMetadata_StreamInfo streamInfo;

		private string vendor;
		private Dictionary<string, string> comments;
		private List<PictureInfo> pictures;

		private int[] decodedDataBuffer;
		private int bufferFilled;
		private int bufferOffset;

		private Stream_Encoder flacEncoder;

		#region ISampleLoaderAgent implementation
		/********************************************************************/
		/// <summary>
		/// Return the file extensions that is supported by the loader
		/// </summary>
		/********************************************************************/
		public string[] FileExtensions => [ "flac" ];



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public AgentResult Identify(ModuleStream moduleStream)
		{
			// Seek to the start of the file
			moduleStream.Seek(0, SeekOrigin.Begin);

			// Check the ID
			if (moduleStream.ReadMark() == "fLaC")
				return AgentResult.Ok;

			return AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the description and value on the line given. If the line
		/// is out of range, false is returned
		/// </summary>
		/********************************************************************/
		public bool GetInformationString(int line, out string description, out string value)
		{
			// Find out which line to take
			switch (line)
			{
				// Vendor
				case 0:
				{
					description = Resources.IDS_FLAC_INFODESCLINE0;
					value = string.IsNullOrEmpty(vendor) ? Resources.IDS_FLAC_UNKNOWN : vendor;
					break;
				}

				// Track number
				case 1:
				{
					description = Resources.IDS_FLAC_INFODESCLINE1;
					value = comments.TryGetValue("tracknumber", out string val) ? val : Resources.IDS_FLAC_UNKNOWN;
					break;
				}

				// Album
				case 2:
				{
					description = Resources.IDS_FLAC_INFODESCLINE2;
					value = comments.TryGetValue("album", out string val) ? val : Resources.IDS_FLAC_UNKNOWN;
					break;
				}

				// Copyright
				case 3:
				{
					description = Resources.IDS_FLAC_INFODESCLINE3;
					value = comments.TryGetValue("copyright", out string val) ? val : Resources.IDS_FLAC_UNKNOWN;
					break;
				}

				// Date
				case 4:
				{
					description = Resources.IDS_FLAC_INFODESCLINE4;
					value = comments.TryGetValue("date", out string val) ? val : Resources.IDS_FLAC_UNKNOWN;
					break;
				}

				// Genre
				case 5:
				{
					description = Resources.IDS_FLAC_INFODESCLINE5;
					value = comments.TryGetValue("genre", out string val) ? val : Resources.IDS_FLAC_UNKNOWN;
					break;
				}

				// Comment
				case 6:
				{
					description = Resources.IDS_FLAC_INFODESCLINE6;
					value = comments.TryGetValue("comment", out string val) ? val : Resources.IDS_FLAC_NONE;
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



		/********************************************************************/
		/// <summary>
		/// Initialize the loader
		/// </summary>
		/********************************************************************/
		public bool InitLoader(out string errorMessage)
		{
			errorMessage = string.Empty;

			comments = new Dictionary<string, string>();

			flacDecoder = Stream_Decoder.Flac__Stream_Decoder_New();
			flacDecoder.Flac__Stream_Decoder_Set_Md5_Checking(false);
			flacDecoder.Flac__Stream_Decoder_Set_Metadata_Respond(Flac__MetadataType.Vorbis_Comment);
			flacDecoder.Flac__Stream_Decoder_Set_Metadata_Respond(Flac__MetadataType.Picture);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the loader
		/// </summary>
		/********************************************************************/
		public void CleanupLoader()
		{
			if (flacDecoder != null)
			{
				flacDecoder.Flac__Stream_Decoder_Delete();
				flacDecoder = null;
			}

			decodedDataBuffer = null;
			streamInfo = null;
			comments = null;
		}



		/********************************************************************/
		/// <summary>
		/// Load the sample header
		/// </summary>
		/********************************************************************/
		public bool LoadHeader(ModuleStream moduleStream, out LoadSampleFormatInfo formatInfo, out string errorMessage)
		{
			errorMessage = string.Empty;
			formatInfo = null;

			if (flacDecoder.Flac__Stream_Decoder_Init_File(moduleStream, true, WriteCallback, MetadataCallback, ErrorCallback, null) != Flac__StreamDecoderInitStatus.Ok)
			{
				errorMessage = Resources.IDS_FLAC_ERR_DECODER_INITIALIZE_FAILED;
				return false;
			}

			pictures = new List<PictureInfo>();

			if (!flacDecoder.Flac__Stream_Decoder_Process_Until_End_Of_Metadata())
			{
				errorMessage = Resources.IDS_FLAC_ERR_DECODER_READ_METADATA;
				return false;
			}

			if (streamInfo.Channels > 8)
			{
				errorMessage = string.Format(Resources.IDS_FLAC_ERR_ILLEGAL_CHANNELS, streamInfo.Channels);
				return false;
			}

			formatInfo = new LoadSampleFormatInfo
			{
				Bits = (int)streamInfo.Bits_Per_Sample,
				Channels = (int)streamInfo.Channels,
				Speakers = FindSpeakers(),
				Frequency = (int)streamInfo.Sample_Rate,
				Pictures = pictures.Count > 0 ? pictures.ToArray() : null
			};

			if (comments.TryGetValue("title", out string val))
				formatInfo.Name = val;

			if (comments.TryGetValue("artist", out val))
				formatInfo.Author = val;

			pictures = null;

			decodedDataBuffer = new int[streamInfo.Max_BlockSize * streamInfo.Channels];
			bufferFilled = 0;
			bufferOffset = 0;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load some part of the sample data
		/// </summary>
		/********************************************************************/
		public int LoadData(ModuleStream moduleStream, int[] buffer, int length, LoadSampleFormatInfo formatInfo)
		{
			int filled = 0;

			while (length > 0)
			{
				if ((bufferFilled - bufferOffset) == 0)
				{
					// Fill the buffer
					if (!flacDecoder.Flac__Stream_Decoder_Process_Single())
						break;
				}

				int todo = Math.Min(length, bufferFilled - bufferOffset);
				if (todo == 0)
					break;

				Array.Copy(decodedDataBuffer, bufferOffset, buffer, filled, todo);

				bufferOffset += todo;
				filled += todo;
				length -= todo;
			}

			return filled;
		}



		/********************************************************************/
		/// <summary>
		/// Calculates how many samples that will be returned
		/// </summary>
		/********************************************************************/
		public long GetTotalSampleLength(LoadSampleFormatInfo formatInfo)
		{
			return (long)flacDecoder.Flac__Stream_Decoder_Get_Total_Samples() * streamInfo.Channels;
		}



		/********************************************************************/
		/// <summary>
		/// Sets the file position to the sample position given
		/// </summary>
		/********************************************************************/
		public void SetSamplePosition(ModuleStream moduleStream, long position, LoadSampleFormatInfo formatInfo)
		{
			flacDecoder.Flac__Stream_Decoder_Seek_Absolute((ulong)position / streamInfo.Channels);
		}
		#endregion

		#region ISampleSaverAgent implementation
		/********************************************************************/
		/// <summary>
		/// Return some flags telling what the saver supports
		/// </summary>
		/********************************************************************/
		public SampleSaverSupportFlag SaverSupportFlags => SampleSaverSupportFlag.Support8Bit | SampleSaverSupportFlag.Support16Bit | SampleSaverSupportFlag.Support32Bit | SampleSaverSupportFlag.SupportMono | SampleSaverSupportFlag.SupportStereo;



		/********************************************************************/
		/// <summary>
		/// Return the file extension that is used by the saver
		/// </summary>
		/********************************************************************/
		public string FileExtension => "flac";



		/********************************************************************/
		/// <summary>
		/// Initialize the saver so it is prepared to save the sample
		/// </summary>
		/********************************************************************/
		public virtual bool InitSaver(SaveSampleFormatInfo formatInfo, out string errorMessage)
		{
			errorMessage = string.Empty;

			// Initialize variables
			flacEncoder = Stream_Encoder.Flac__Stream_Encoder_New();
			flacEncoder.Flac__Stream_Encoder_Set_Bits_Per_Sample(formatInfo.Bits);
			flacEncoder.Flac__Stream_Encoder_Set_Channels((uint)formatInfo.Channels);
			flacEncoder.Flac__Stream_Encoder_Set_Sample_Rate(formatInfo.Frequency);
			flacEncoder.Flac__Stream_Encoder_Set_Num_Threads(4);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the saver
		/// </summary>
		/********************************************************************/
		public virtual void CleanupSaver()
		{
			if (flacEncoder != null)
			{
				flacEncoder.Flac__Stream_Encoder_Delete();
				flacEncoder = null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Save the header of the sample
		/// </summary>
		/********************************************************************/
		public void SaveHeader(Stream stream)
		{
			if (flacEncoder.Flac__Stream_Encoder_Init_File(stream, true, null, null) != Flac__StreamEncoderInitStatus.Ok)
				throw new Exception(Resources.IDS_FLAC_ERR_ENCODER_INITIALIZE_FAILED);
		}



		/********************************************************************/
		/// <summary>
		/// Save a part of the sample
		/// </summary>
		/********************************************************************/
		public void SaveData(Stream stream, int[] buffer, int length)
		{
			if (length > 0)
			{
				int[] convertedBuffer = new int[length];
				uint bits = flacEncoder.Flac__Stream_Encoder_Get_Bits_Per_Sample();
				uint channels = flacEncoder.Flac__Stream_Encoder_Get_Channels();

				if (bits == 8)
				{
					// Convert to 8-bit
					for (int i = 0; i < length; i++)
						convertedBuffer[i] = buffer[i] >> 24;
				}
				else if (bits == 16)
				{
					// Convert to 16-bit
					for (int i = 0; i < length; i++)
						convertedBuffer[i] = buffer[i] >> 16;
				}
				else if (bits == 32)
				{
					// Just copy the data
					Array.Copy(buffer, convertedBuffer, length);
				}

				if (!flacEncoder.Flac__Stream_Encoder_Process_Interleaved(convertedBuffer, (uint)length / channels))
					throw new Exception(Resources.IDS_FLAC_ERR_ENCODER_WRITE_SAMPLES_FAILED);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Save the tail of the sample
		/// </summary>
		/********************************************************************/
		public void SaveTail(Stream stream)
		{
			if (!flacEncoder.Flac__Stream_Encoder_Finish())
				throw new Exception(Resources.IDS_FLAC_ERR_ENCODER_WRITE_SAMPLES_FAILED);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Find out which speakers are used in the sample
		/// </summary>
		/********************************************************************/
		private SpeakerFlag FindSpeakers()
		{
			switch (streamInfo.Channels)
			{
				case 1:
					return SpeakerFlag.FrontCenter;

				case 2:
					return SpeakerFlag.FrontLeft | SpeakerFlag.FrontRight;

				case 3:
					return SpeakerFlag.FrontLeft | SpeakerFlag.FrontRight | SpeakerFlag.FrontCenter;

				case 4:
					return SpeakerFlag.FrontLeft | SpeakerFlag.FrontRight | SpeakerFlag.BackLeft | SpeakerFlag.BackRight;

				case 5:
					return SpeakerFlag.FrontLeft | SpeakerFlag.FrontRight | SpeakerFlag.FrontCenter | SpeakerFlag.BackLeft | SpeakerFlag.BackRight;

				case 6:
					return SpeakerFlag.FrontLeft | SpeakerFlag.FrontRight | SpeakerFlag.FrontCenter | SpeakerFlag.LowFrequency | SpeakerFlag.BackLeft | SpeakerFlag.BackRight;

				case 7:
					return SpeakerFlag.FrontLeft | SpeakerFlag.FrontRight | SpeakerFlag.FrontCenter | SpeakerFlag.LowFrequency | SpeakerFlag.BackCenter | SpeakerFlag.SideLeft | SpeakerFlag.SideRight;

				case 8:
					return SpeakerFlag.FrontLeft | SpeakerFlag.FrontRight | SpeakerFlag.FrontCenter | SpeakerFlag.LowFrequency | SpeakerFlag.BackLeft | SpeakerFlag.BackRight | SpeakerFlag.SideLeft | SpeakerFlag.SideRight;

				default:
					return 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called for each metadata frame
		/// </summary>
		/********************************************************************/
		private void MetadataCallback(Stream_Decoder decoder, Flac__StreamMetadata metadata, object client_data)
		{
			switch (metadata.Type)
			{
				case Flac__MetadataType.StreamInfo:
				{
					streamInfo = (Flac__StreamMetadata_StreamInfo)metadata.Data;
					break;
				}

				case Flac__MetadataType.Vorbis_Comment:
				{
					ParseVorbisComment((Flac__StreamMetadata_VorbisComment)metadata.Data);
					break;
				}

				case Flac__MetadataType.Picture:
				{
					ParsePicture((Flac__StreamMetadata_Picture)metadata.Data);
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a block of samples has been decoded
		/// </summary>
		/********************************************************************/
		private Flac__StreamDecoderWriteStatus WriteCallback(Stream_Decoder decoder, Flac__Frame frame, int[][] buffer, object client_data)
		{
			int blockSize = (int)(frame.Header.BlockSize * streamInfo.Channels);

			// Move data if our buffer if needed
			if (((decodedDataBuffer.Length - bufferFilled) < blockSize) && (bufferOffset > 0))
			{
				if ((bufferFilled - bufferOffset) > 0)
					Array.Copy(decodedDataBuffer, bufferOffset, decodedDataBuffer, 0, bufferFilled - bufferOffset);

				bufferFilled -= bufferOffset;
				bufferOffset = 0;
			}

			// Just an extra check, just to be sure, but it shouldn't be necessary. Resize the buffer if needed
			if ((decodedDataBuffer.Length - bufferFilled) < blockSize)
				Array.Resize(ref decodedDataBuffer, bufferFilled + (int)(frame.Header.BlockSize * streamInfo.Channels));

			// Store the read data into our buffer
			int shift = 32 - (int)streamInfo.Bits_Per_Sample;

			for (int c = 0; c < streamInfo.Channels; c++)
			{
				int[] sourceBuffer = buffer[c];

				for (int i = 0, cnt = (int)frame.Header.BlockSize, j = bufferOffset + c; i < cnt; i++, j += (int)streamInfo.Channels)
					decodedDataBuffer[j] = sourceBuffer[i] << shift;
			}

			bufferFilled += blockSize;

			return Flac__StreamDecoderWriteStatus.Continue;
		}



		/********************************************************************/
		/// <summary>
		/// Is called every time an error occurs
		/// </summary>
		/********************************************************************/
		private void ErrorCallback(Stream_Decoder decoder, Flac__StreamDecoderErrorStatus status, object client_data)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Parse the Vorbis comment metadata block
		/// </summary>
		/********************************************************************/
		private void ParseVorbisComment(Flac__StreamMetadata_VorbisComment vorbisComment)
		{
			Encoding encoder = Encoding.UTF8;

			if (vorbisComment.Vendor_String.Length > 0)
				vendor = encoder.GetString(vorbisComment.Vendor_String.Entry, 0, (int)vorbisComment.Vendor_String.Length);
			else
				vendor = null;

			if (vorbisComment.Num_Comments > 0)
			{
				for (int i = (int)vorbisComment.Num_Comments - 1; i >= 0; i--)
				{
					if (vorbisComment.Comments[i].Entry != null)
					{
						string pair = encoder.GetString(vorbisComment.Comments[i].Entry, 0, (int)vorbisComment.Comments[i].Length);
						int index = pair.IndexOf('=');
						if (index > 0)
							comments[pair.Substring(0, index).ToLower()] = pair.Substring(index + 1);
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse the Vorbis picture metadata block
		/// </summary>
		/********************************************************************/
		private void ParsePicture(Flac__StreamMetadata_Picture picture)
		{
			string mimeType = Encoding.ASCII.GetString(picture.Mime_Type, 0, picture.Mime_Type.Length - 1);
			if (mimeType == "-->")	// URL, we do not support that
				return;

			string type = GetTypeDescription(picture.Type);

			string description = Encoding.UTF8.GetString(picture.Description, 0, picture.Description.Length - 1);
			if (string.IsNullOrEmpty(description))
				description = type;
			else
				description = $"{type}: {description}";

			pictures.Add(new PictureInfo(picture.Data, description));
		}



		/********************************************************************/
		/// <summary>
		/// Find the picture type string
		/// </summary>
		/********************************************************************/
		private string GetTypeDescription(Flac__StreamMetadata_Picture_Type type)
		{
			switch (type)
			{
				case Flac__StreamMetadata_Picture_Type.Other:
					return Resources.IDS_FLAC_PICTURE_TYPE_OTHER;

				case Flac__StreamMetadata_Picture_Type.File_Icon_Standard:
					return Resources.IDS_FLAC_PICTURE_TYPE_FILE_ICON_STANDARD;

				case Flac__StreamMetadata_Picture_Type.File_Icon:
					return Resources.IDS_FLAC_PICTURE_TYPE_FILE_ICON;

				case Flac__StreamMetadata_Picture_Type.Front_Cover:
					return Resources.IDS_FLAC_PICTURE_TYPE_FRONT_COVER;

				case Flac__StreamMetadata_Picture_Type.Back_Cover:
					return Resources.IDS_FLAC_PICTURE_TYPE_BACK_COVER;

				case Flac__StreamMetadata_Picture_Type.Leaflet_Page:
					return Resources.IDS_FLAC_PICTURE_TYPE_LEAFLET_PAGE;

				case Flac__StreamMetadata_Picture_Type.Media:
					return Resources.IDS_FLAC_PICTURE_TYPE_MEDIA;

				case Flac__StreamMetadata_Picture_Type.Lead_Artist:
					return Resources.IDS_FLAC_PICTURE_TYPE_LEAD_ARTIST;

				case Flac__StreamMetadata_Picture_Type.Artist:
					return Resources.IDS_FLAC_PICTURE_TYPE_ARTIST;

				case Flac__StreamMetadata_Picture_Type.Conductor:
					return Resources.IDS_FLAC_PICTURE_TYPE_CONDUCTOR;

				case Flac__StreamMetadata_Picture_Type.Band:
					return Resources.IDS_FLAC_PICTURE_TYPE_BAND;

				case Flac__StreamMetadata_Picture_Type.Composer:
					return Resources.IDS_FLAC_PICTURE_TYPE_COMPOSER;

				case Flac__StreamMetadata_Picture_Type.Lyricist:
					return Resources.IDS_FLAC_PICTURE_TYPE_LYRICIST;

				case Flac__StreamMetadata_Picture_Type.Recording_Location:
					return Resources.IDS_FLAC_PICTURE_TYPE_RECORDING_LOCATION;

				case Flac__StreamMetadata_Picture_Type.During_Recoding:
					return Resources.IDS_FLAC_PICTURE_TYPE_DURING_RECORDING;

				case Flac__StreamMetadata_Picture_Type.During_Performance:
					return Resources.IDS_FLAC_PICTURE_TYPE_DURING_PERFORMANCE;

				case Flac__StreamMetadata_Picture_Type.Video_Screen_Capture:
					return Resources.IDS_FLAC_PICTURE_TYPE_VIDEO_SCREEN_CAPTURE;

				case Flac__StreamMetadata_Picture_Type.Fish:
					return Resources.IDS_FLAC_PICTURE_TYPE_FISH;

				case Flac__StreamMetadata_Picture_Type.Illustration:
					return Resources.IDS_FLAC_PICTURE_TYPE_ILLUSTRATION;

				case Flac__StreamMetadata_Picture_Type.Band_LogoType:
					return Resources.IDS_FLAC_PICTURE_TYPE_BAND_LOGO;

				case Flac__StreamMetadata_Picture_Type.Publisher_LogoType:
					return Resources.IDS_FLAC_PICTURE_TYPE_PUBLISHER_LOGO;
			}

			return string.Empty;
		}
		#endregion
	}
}
