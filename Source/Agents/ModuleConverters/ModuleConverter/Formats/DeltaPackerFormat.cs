/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter.Formats
{
	/// <summary>
	/// Can convert Delta Packer to Delta Music 2.0 format
	/// </summary>
	internal class DeltaPackerFormat : ModuleConverterAgentBase
	{
		private int trackLength;

		private long arpeggioOffset;
		private ushort arpeggioLength;

		private ushort[] instrumentOffsets;
		private long instrumentOffset;
		private int instrumentLength;

		private ushort[] waveformOffsets;
		private long waveformOffset;

		private long blockOffset;
		private uint blockLength;

		private long sampleOffset;

		#region IModuleConverterAgent implementation
		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			long fileSize = moduleStream.Length;
			if (fileSize < 16)
				return AgentResult.Unknown;

			// Check the track lists. Each voice has a restart position and the
			// length of its list in bytes. Each position uses 2 bytes
			moduleStream.Seek(0, SeekOrigin.Begin);

			trackLength = 0;

			for (int i = 0; i < 4; i++)
			{
				ushort restart = moduleStream.Read_B_UINT16();
				ushort length = moduleStream.Read_B_UINT16();

				if ((length == 0) || ((length & 1) != 0) || ((restart & 1) != 0) || (restart >= length))
					return AgentResult.Unknown;

				trackLength += length;
			}

			// Check the arpeggios. Delta Music 2.0 has 64 tables of 16 bytes
			// each, but only the tables up to the last one used are stored
			moduleStream.Seek(16 + trackLength, SeekOrigin.Begin);

			arpeggioLength = moduleStream.Read_B_UINT16();
			if ((arpeggioLength == 0) || ((arpeggioLength % 16) != 0) || (arpeggioLength > (64 * 16)))
				return AgentResult.Unknown;

			arpeggioOffset = moduleStream.Position;

			// Check the instrument offsets. Each instrument uses 88 bytes and
			// the last offset is the highest one
			moduleStream.Seek(arpeggioLength, SeekOrigin.Current);

			ushort offsetLength = moduleStream.Read_B_UINT16();
			if ((offsetLength == 0) || ((offsetLength & 1) != 0) || (offsetLength > (127 * 2)))
				return AgentResult.Unknown;

			instrumentOffsets = new ushort[offsetLength / 2];
			moduleStream.ReadArray_B_UINT16s(instrumentOffsets, 0, instrumentOffsets.Length);

			if (moduleStream.EndOfStream)
				return AgentResult.Unknown;

			ushort lastOffset = instrumentOffsets[instrumentOffsets.Length - 1];
			if (lastOffset > (127 * 88))
				return AgentResult.Unknown;

			foreach (ushort offset in instrumentOffsets)
			{
				if (((offset % 88) != 0) || (offset > lastOffset))
					return AgentResult.Unknown;
			}

			instrumentOffset = moduleStream.Position;
			instrumentLength = lastOffset + 88;

			// Check the waveforms. Only a part of each waveform is stored and
			// the offsets point into the waveform data
			moduleStream.Seek(instrumentLength, SeekOrigin.Current);

			// The player of the packer needs at least one waveform
			offsetLength = moduleStream.Read_B_UINT16();
			if ((offsetLength == 0) || ((offsetLength & 1) != 0))
				return AgentResult.Unknown;

			waveformOffsets = new ushort[offsetLength / 2];
			moduleStream.ReadArray_B_UINT16s(waveformOffsets, 0, waveformOffsets.Length);

			ushort waveformLength = moduleStream.Read_B_UINT16();

			if (moduleStream.EndOfStream)
				return AgentResult.Unknown;

			foreach (ushort offset in waveformOffsets)
			{
				if (((offset & 1) != 0) || (offset > waveformLength))
					return AgentResult.Unknown;
			}

			waveformOffset = moduleStream.Position;

			// Check the blocks. Each block has 16 rows of 4 bytes
			moduleStream.Seek(waveformLength, SeekOrigin.Current);

			blockLength = moduleStream.Read_B_UINT32();
			if ((blockLength == 0) || ((blockLength % 64) != 0))
				return AgentResult.Unknown;

			blockOffset = moduleStream.Position;

			// The offsets to the 8 samples follow the blocks
			sampleOffset = blockOffset + blockLength;
			if ((sampleOffset + (8 * 4)) > fileSize)
				return AgentResult.Unknown;

			// At last, check that all positions in the track lists use a block,
			// which is stored in the module
			byte[] trackData = new byte[trackLength];

			moduleStream.Seek(16, SeekOrigin.Begin);
			moduleStream.ReadInto(trackData, 0, trackLength);

			uint numberOfBlocks = blockLength / 64;

			for (int i = 0; i < trackLength; i += 2)
			{
				if (trackData[i] >= numberOfBlocks)
					return AgentResult.Unknown;
			}

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Convert the module and store the result in the stream given
		/// </summary>
		/********************************************************************/
		public override AgentResult Convert(PlayerFileInfo fileInfo, ConverterStream converterStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			ModuleStream moduleStream = fileInfo.ModuleStream;

			// A Delta Music 2.0 module starts with the player code, which is
			// not stored in the packed module. Only the start speed is read
			// from there. The player of the packer does not set it, so it
			// starts with 0
			byte[] emptyBuffer = new byte[0xbbb];

			converterStream.Write(emptyBuffer, 0, 0xbbb);
			converterStream.Write_UINT8(0);
			converterStream.Write(emptyBuffer, 0, 0xbc6 - (0xbbb + 1));

			// Write the ID mark
			converterStream.WriteMark(".FNL");

			// Write the arpeggios and fill up with empty tables
			moduleStream.Seek(arpeggioOffset, SeekOrigin.Begin);
			StreamHelper.CopyData(moduleStream, converterStream, arpeggioLength);

			converterStream.Write(emptyBuffer, 0, (64 * 16) - arpeggioLength);

			// Write the track lists, which are stored in the same way
			moduleStream.Seek(0, SeekOrigin.Begin);
			StreamHelper.CopyData(moduleStream, converterStream, 16 + trackLength);

			// Write the blocks
			converterStream.Write_B_UINT32(blockLength);

			moduleStream.Seek(blockOffset, SeekOrigin.Begin);
			StreamHelper.CopyData(moduleStream, converterStream, (int)blockLength);

			// Write the instrument offsets. There is room for 127 offsets plus
			// an end mark, which is the length of the instrument data. All
			// unused offsets are set to the end mark
			converterStream.WriteArray_B_UINT16s(instrumentOffsets, instrumentOffsets.Length);

			for (int i = instrumentOffsets.Length; i < 128; i++)
				converterStream.Write_B_UINT16((ushort)instrumentLength);

			// Write the instruments
			moduleStream.Seek(instrumentOffset, SeekOrigin.Begin);
			StreamHelper.CopyData(moduleStream, converterStream, instrumentLength);

			// Write the waveforms. Each of them uses 256 bytes in Delta Music
			// 2.0. The packer does not always store all of the waveform, which
			// is played, but the player of the packer plays the waveform from
			// its place in the module, so the bytes after it are taken as they
			// are
			converterStream.Write_B_UINT32((uint)(waveformOffsets.Length * 256));

			foreach (ushort offset in waveformOffsets)
			{
				moduleStream.Seek(waveformOffset + offset, SeekOrigin.Begin);
				StreamHelper.CopyDataForceLength(moduleStream, converterStream, 256);
			}

			// Write unknown data, which is skipped by the player
			converterStream.Write(emptyBuffer, 0, 64);

			// Write the sample offsets, which are stored in the same way
			moduleStream.Seek(sampleOffset, SeekOrigin.Begin);
			StreamHelper.CopyData(moduleStream, converterStream, 8 * 4);

			// And at last, the sample data, which goes to the end of the file
			converterStream.SetSampleDataMarker(moduleStream, (int)(moduleStream.Length - moduleStream.Position));

			return AgentResult.Ok;
		}
		#endregion
	}
}
