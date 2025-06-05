/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers.Chunks;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Formats
{
	/// <summary>
	/// Will save a MTM file
	/// </summary>
	internal class MtmSaver : IFormatSaver
	{
		#region IFormatSaver implementation
		/********************************************************************/
		/// <summary>
		/// Save the module into MTM format
		/// </summary>
		/********************************************************************/
		public bool SaveModule(Mo3Module module, ModuleStream moduleStream, ConverterStream converterStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			List<byte[]> comment = BuildCommentList(module);

			DecodeSampleInfo[] decodeSampleInfo = Mo3SampleWriter.PrepareSamples(module, moduleStream, true);
			if (decodeSampleInfo == null)
			{
				errorMessage = Resources.IDS_ERR_LOADING_SAMPLES;
				return false;
			}

			WriteMark(module, converterStream);
			WriteSongName(module, converterStream);
			WriteHeader(module, comment, converterStream);
			WriteSampleInfo(module, converterStream);
			WritePositionList(module, converterStream);
			WriteTracks(module, converterStream);
			WriteSequence(module, converterStream);
			WriteComment(comment, converterStream);
			Mo3SampleWriter.WriteSamples(decodeSampleInfo, converterStream);

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Build list with comments
		/// </summary>
		/********************************************************************/
		private List<byte[]> BuildCommentList(Mo3Module module)
		{
			// MTM stores each line in the comments as 40 bytes long.
			// MO3 have new lines characters (0x0d)
			List<byte[]> comments = new List<byte[]>();

			byte[] originalComment = module.Header.SongMessage;
			int todo = originalComment.Length;
			int startIndex = 0;

			while (todo > 0)
			{
				int toCopy;

				int index = Array.FindIndex(originalComment, startIndex, (x => x == 0x0d));
				if (index != -1)
					toCopy = index - startIndex;
				else
					toCopy = originalComment.Length - startIndex;

				byte[] convertedComment = new byte[40];
				Array.Copy(originalComment, startIndex, convertedComment, 0, toCopy);
				comments.Add(convertedComment);

				startIndex = index + 1;
				todo -= (toCopy + 1);
			}

			return comments;
		}



		/********************************************************************/
		/// <summary>
		/// Find the right mark to write
		/// </summary>
		/********************************************************************/
		private void WriteMark(Mo3Module module, ConverterStream converterStream)
		{
			converterStream.WriteMark("MTM");

			VersChunk versChunk = module.FindChunk<VersChunk>();
			if (versChunk != null)
				converterStream.Write_UINT8((byte)versChunk.Cwtv);
			else
				converterStream.Write_UINT8(0x10);
		}



		/********************************************************************/
		/// <summary>
		/// Write back the song name
		/// </summary>
		/********************************************************************/
		private void WriteSongName(Mo3Module module, ConverterStream converterStream)
		{
			converterStream.WriteString(module.Header.SongName, 20);
		}



		/********************************************************************/
		/// <summary>
		/// Write back the header
		/// </summary>
		/********************************************************************/
		private void WriteHeader(Mo3Module module, List<byte[]> comment, ConverterStream converterStream)
		{
			FileHeader header = module.Header;

			// Since MTM does not save track 0 (it is considered as empty), but MO3
			// does, we need to decrement the number by one
			converterStream.Write_L_UINT16((ushort)(header.NumTracks - 1));

			// MTM holds the max pattern number, while MO3 holds the number of patterns,
			// so here we also need to decrement by one
			converterStream.Write_UINT8((byte)(header.NumPatterns - 1));

			// And again, MTM holds the last order to play, while MO3 holds the
			// number of orders
			converterStream.Write_UINT8((byte)(header.NumOrders - 1));

			converterStream.Write_L_UINT16((ushort)(comment.Count * 40));
			converterStream.Write_UINT8((byte)header.NumSamples);
			converterStream.Write_UINT8(0);     // Attributes - always 0
			converterStream.Write_UINT8((byte)module.PatternInfo.RowLengths[0]);
			converterStream.Write_UINT8(header.NumChannels);

			// Convert the voice pan
			for (int i = 0; i < 32; i++)
				converterStream.Write_UINT8((byte)(header.ChnPan[i] >> 4));
		}



		/********************************************************************/
		/// <summary>
		/// Write all the sample information
		/// </summary>
		/********************************************************************/
		private void WriteSampleInfo(Mo3Module module, ConverterStream converterStream)
		{
			for (int i = 0; i < module.Header.NumSamples; i++)
			{
				Sample sample = module.Samples[i];

				converterStream.WriteString(sample.SampleName, 22);
				converterStream.Write_L_UINT32(sample.Length);

				if ((sample.Flags & SampleInfoFlag.Loop) != 0)
				{
					converterStream.Write_L_UINT32(sample.LoopStart);
					converterStream.Write_L_UINT32(sample.LoopEnd);
				}
				else
				{
					converterStream.Write_B_UINT32(0);
					converterStream.Write_B_UINT32(0);
				}

				converterStream.Write_UINT8((byte)((sample.FreqFineTune - 256) >> 4));
				converterStream.Write_UINT8(sample.DefaultVolume);

				if ((sample.Flags & SampleInfoFlag._16Bit) != 0)
					converterStream.Write_UINT8(1);
				else
					converterStream.Write_UINT8(0);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Write the position list and its information
		/// </summary>
		/********************************************************************/
		private void WritePositionList(Mo3Module module, ConverterStream converterStream)
		{
			converterStream.Write(module.PatternInfo.PositionList, 0, module.Header.NumOrders);

			if (module.Header.NumOrders < 128)
				converterStream.Write(Enumerable.Repeat<byte>(0, 128 - module.Header.NumOrders).ToArray());
		}



		/********************************************************************/
		/// <summary>
		/// Will convert and write all the tracks
		/// </summary>
		/********************************************************************/
		private void WriteTracks(Mo3Module module, ConverterStream converterStream)
		{
			byte[] singleTrackData = new byte[3 * 64];

			// Skip first track, since it will always be empty and is
			// not written in MTM modules
			for (int i = 1; i < module.Header.NumTracks; i++)
			{
				Track trk = module.Tracks[i];
				ConvertTrack(singleTrackData, trk);

				converterStream.Write(singleTrackData, 0, singleTrackData.Length);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will convert the given track
		/// </summary>
		/********************************************************************/
		private void ConvertTrack(byte[] trackData, Track trk)
		{
			int rows = Math.Min(trk.Rows.Count, 64);

			for (int i = 0; i < rows; i++)
			{
				TrackRow trkRow = trk.Rows[i];

				if (trkRow.Note != 0)
					trackData[i * 3 + 0] = (byte)((trkRow.Note - 24) << 2);
				else
					trackData[i * 3 + 0] = 0x00;

				if (trkRow.Instrument != 0)
				{
					trackData[i * 3 + 0] |= (byte)((trkRow.Instrument & 0x30) >> 4);
					trackData[i * 3 + 1] = (byte)((trkRow.Instrument & 0x0f) << 4);
				}
				else
					trackData[i * 3 + 1] = 0x00;

				if (trkRow.Effects != null)
				{
					(Effect, byte) eff = trkRow.Effects[0];

					byte effNum = (byte)(((byte)eff.Item1) - 3);
					byte effVal = eff.Item2;

					trackData[i * 3 + 1] |= effNum;
					trackData[i * 3 + 2] = effVal;
				}
				else
					trackData[i * 3 + 2] = 0x00;
			}

			for (int i = rows; i < 64; i++)
			{
				trackData[i * 3 + 0] = 0x00;
				trackData[i * 3 + 1] = 0x00;
				trackData[i * 3 + 2] = 0x00;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will write the sequence list
		/// </summary>
		/********************************************************************/
		private void WriteSequence(Mo3Module module, ConverterStream converterStream)
		{
			for (int i = 0; i < module.Header.NumPatterns; i++)
			{
				for (int j = 0; j < module.Header.NumChannels; j++)
					converterStream.Write_L_UINT16(module.PatternInfo.Sequences[i, j]);

				if (module.Header.NumChannels < 32)
					converterStream.Write(Enumerable.Repeat<byte>(0, (32 - module.Header.NumChannels) * 2).ToArray());
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will write the comment
		/// </summary>
		/********************************************************************/
		private void WriteComment(List<byte[]> comment, ConverterStream converterStream)
		{
			foreach (byte[] commentLine in comment)
				converterStream.Write(commentLine, 0, commentLine.Length);
		}
		#endregion
	}
}
