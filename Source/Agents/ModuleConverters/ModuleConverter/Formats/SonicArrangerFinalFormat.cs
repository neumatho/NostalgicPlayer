/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter.Formats
{
	/// <summary>
	/// Can convert Sonic Arranger (Final) to Sonic Arranger format
	/// </summary>
	internal class SonicArrangerFinalFormat : ModuleConverterAgentBase
	{
		private class InstrumentInfo
		{
			public ushort SampleNumber;
			public ushort OneshotLength;
			public ushort RepeatLength;
		}

		private int moduleOffset;

		#region IModuleConverterAgent implementation
		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the code to see if it's a Sonic Arranger module
			moduleStream.Seek(0, SeekOrigin.Begin);

			// Find out if there is a replay or not in the beginning
			uint check = moduleStream.Read_B_UINT32();

			if ((check & 0xffff0000) == 0x4efa0000)
			{
				short initOffset = (short)((check & 0x0000ffff) + 2);

				// Check the module size
				long fileSize = moduleStream.Length;
				if (fileSize < 0x1630)
					return AgentResult.Unknown;

				// There should be 7 JMP instructions in the beginning
				for (int i = 1; i < 7; i++)
				{
					if (moduleStream.Read_B_UINT16() != 0x4efa)
						return AgentResult.Unknown;

					moduleStream.Seek(2, SeekOrigin.Current);
				}

				// Seek to the init routine
				moduleStream.Seek(initOffset, SeekOrigin.Begin);

				// Check it
				if (moduleStream.Read_B_UINT32() != 0x48e7fffe)
					return AgentResult.Unknown;

				if (moduleStream.Read_B_UINT16() != 0x41fa)
					return AgentResult.Unknown;

				moduleOffset = (int)(moduleStream.Position + moduleStream.Read_B_UINT16());
				if (moduleOffset >= moduleStream.Length)
					return AgentResult.Unknown;

				return AgentResult.Ok;
			}

			if ((check & 0xffff0000) == 0)
			{
				if (check == 0)
					return AgentResult.Unknown;

				uint check1 = check & 0xf8;
				if (check1 != check)
					return AgentResult.Unknown;

				check = moduleStream.Read_B_UINT32();
				if ((check == 0) || ((check & 0xffff0000) != 0))
					return AgentResult.Unknown;

				check1 = check - check1;
				if ((check1 % 12) != 0)
					return AgentResult.Unknown;

				if ((check1 / 12) > 32)
					return AgentResult.Unknown;

				check1 = moduleStream.Read_B_UINT32();
				if ((check1 == 0) || ((check1 & 0xffff0000) != 0) || (check > check1))
					return AgentResult.Unknown;

				uint check2 = check1 - check;
				if ((check2 & 0xfff0) != check2)
					return AgentResult.Unknown;

				check = moduleStream.Read_B_UINT32();
				if ((check == 0) || ((check & 0xffff0000) != 0) || (check1 > check))
					return AgentResult.Unknown;

				check1 = moduleStream.Read_B_UINT32();
				if ((check1 == 0) || ((check1 & 0xffff0000) != 0) || (check > check1))
					return AgentResult.Unknown;

				check = moduleStream.Read_B_UINT32();
				if ((check == 0) || ((check & 0xffff0000) != 0) || (check1 > check))
					return AgentResult.Unknown;

				check1 = moduleStream.Read_B_UINT32();
				if ((check1 == 0) || ((check1 & 0xffff0000) != 0) || (check > check1))
					return AgentResult.Unknown;

				moduleOffset = 0;

				return AgentResult.Ok;
			}

			return AgentResult.Unknown;
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

			// Seek to module data
			moduleStream.Seek(moduleOffset, SeekOrigin.Begin);

			// Read all the offsets
			uint[] offsets = new uint[8];
			moduleStream.ReadArray_B_UINT32s(offsets, 0, 8);

			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_ERR_LOADING_HEADER;
				return AgentResult.Error;
			}

			// Start to write the ID mark
			byte[] mark = Encoding.ASCII.GetBytes("SOARV1.0");
			converterStream.Write(mark, 0, mark.Length);

			// Write all the different parts
			if (!WriteSubSongs(offsets, moduleStream, converterStream))
			{
				errorMessage = Resources.IDS_ERR_LOADING_HEADER;
				return AgentResult.Error;
			}

			if (!WritePositionInformation(offsets, moduleStream, converterStream))
			{
				errorMessage = Resources.IDS_ERR_LOADING_HEADER;
				return AgentResult.Error;
			}

			if (!WriteTrackRows(offsets, moduleStream, converterStream))
			{
				errorMessage = Resources.IDS_ERR_LOADING_TRACKS;
				return AgentResult.Error;
			}

			if (!WriteInstrumentInformation(offsets, moduleStream, converterStream, out InstrumentInfo[] instInfo))
			{
				errorMessage = Resources.IDS_ERR_LOADING_INSTRUMENTS;
				return AgentResult.Error;
			}

			if (!WriteSampleInformation(offsets, instInfo, moduleStream, converterStream))
			{
				errorMessage = Resources.IDS_ERR_LOADING_SAMPLEINFO;
				return AgentResult.Error;
			}

			if (!WriteSampleData(offsets, moduleStream, converterStream))
			{
				errorMessage = Resources.IDS_ERR_LOADING_SAMPLES;
				return AgentResult.Error;
			}

			if (!WriteWaveformData(offsets, moduleStream, converterStream))
			{
				errorMessage = Resources.IDS_ERR_LOADING_WAVEFORM;
				return AgentResult.Error;
			}

			if (!WriteAdsrTables(offsets, moduleStream, converterStream))
			{
				errorMessage = Resources.IDS_ERR_LOADING_WAVEFORM;
				return AgentResult.Error;
			}

			if (!WriteAmfTables(offsets, moduleStream, converterStream))
			{
				errorMessage = Resources.IDS_ERR_LOADING_WAVEFORM;
				return AgentResult.Error;
			}

			WriteEditorData(converterStream);

			return AgentResult.Ok;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Helper method to copy some data
		/// </summary>
		/********************************************************************/
		private bool CopyData(ModuleStream moduleStream, ConverterStream converterStream, uint startOffset, uint count, int elementSize)
		{
			if (count > 0)
			{
				moduleStream.Seek(moduleOffset + startOffset, SeekOrigin.Begin);

				byte[] buffer = new byte[elementSize];

				for (uint i = 0; i < count; i++)
				{
					int read = moduleStream.Read(buffer, 0, elementSize);
					if (read != elementSize)
						return false;

					converterStream.Write(buffer, 0, elementSize);
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Write the sub-songs part
		/// </summary>
		/********************************************************************/
		private bool WriteSubSongs(uint[] offsets, ModuleStream moduleStream, ConverterStream converterStream)
		{
			uint count = (offsets[1] - offsets[0]) / 12;
			if (count == 0)
				return false;

			converterStream.Write_B_UINT32(0x5354424c);		// STBL
			converterStream.Write_B_UINT32(count);

			return CopyData(moduleStream, converterStream, offsets[0], count, 12);
		}



		/********************************************************************/
		/// <summary>
		/// Write the position information part
		/// </summary>
		/********************************************************************/
		private bool WritePositionInformation(uint[] offsets, ModuleStream moduleStream, ConverterStream converterStream)
		{
			uint count = (offsets[2] - offsets[1]) / 16;
			if (count == 0)
				return false;

			converterStream.Write_B_UINT32(0x4f565442);		// OVTB
			converterStream.Write_B_UINT32(count);

			return CopyData(moduleStream, converterStream, offsets[1], count, 16);
		}



		/********************************************************************/
		/// <summary>
		/// Write the track rows part
		/// </summary>
		/********************************************************************/
		private bool WriteTrackRows(uint[] offsets, ModuleStream moduleStream, ConverterStream converterStream)
		{
			uint count = (offsets[3] - offsets[2]) / 4;
			if (count == 0)
				return false;

			converterStream.Write_B_UINT32(0x4e54424c);		// NTBL
			converterStream.Write_B_UINT32(count);

			return CopyData(moduleStream, converterStream, offsets[2], count, 4);
		}



		/********************************************************************/
		/// <summary>
		/// Write the instrument information part
		/// </summary>
		/********************************************************************/
		private bool WriteInstrumentInformation(uint[] offsets, ModuleStream moduleStream, ConverterStream converterStream, out InstrumentInfo[] instInfo)
		{
			instInfo = null;

			uint count = (offsets[4] - offsets[3]) / 152;
			if (count == 0)
			{
				return false;
			}

			List<InstrumentInfo> list = new List<InstrumentInfo>();

			converterStream.Write_B_UINT32(0x494e5354);		// INST
			converterStream.Write_B_UINT32(count);

			moduleStream.Seek(moduleOffset + offsets[3], SeekOrigin.Begin);

			byte[] buffer = new byte[144];

			for (uint i = 0; i < count; i++)
			{
				ushort type = moduleStream.Read_B_UINT16();		// Instrument type
				converterStream.Write_B_UINT16(type);

				ushort number = moduleStream.Read_B_UINT16();	// Sample/waveform number
				converterStream.Write_B_UINT16(number);

				ushort oneshot = moduleStream.Read_B_UINT16();	// Oneshot length
				converterStream.Write_B_UINT16(oneshot);

				ushort repeat = moduleStream.Read_B_UINT16();	// Repeat length
				converterStream.Write_B_UINT16(repeat);

				if (moduleStream.EndOfStream)
					return false;

				if (type == 0)
					list.Add(new InstrumentInfo { SampleNumber = number, OneshotLength = oneshot, RepeatLength = repeat });

				int read = moduleStream.Read(buffer, 0, buffer.Length);
				if (read != buffer.Length)
					return false;

				converterStream.Write(buffer, 0, buffer.Length);
			}

			instInfo = list.OrderBy(x => x.SampleNumber).ToArray();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Write the sample information part
		/// </summary>
		/********************************************************************/
		private bool WriteSampleInformation(uint[] offsets, InstrumentInfo[] instInfo, ModuleStream moduleStream, ConverterStream converterStream)
		{
			moduleStream.Seek(moduleOffset + offsets[7], SeekOrigin.Begin);

			uint count = moduleStream.Read_B_UINT32();
			if (moduleStream.EndOfStream)
				return false;

			converterStream.Write_B_UINT32(0x53443842);		// SD8B
			converterStream.Write_B_UINT32(count);

			if (count == 0)
				return true;

			// Fill in holes in the instrument info
			List<InstrumentInfo> newInstList = new List<InstrumentInfo>();

			for (int i = 0, j = 0; i < count; i++)
			{
				if (instInfo[j].SampleNumber == i)
					newInstList.Add(instInfo[j++]);
				else
					newInstList.Add(null);
			}

			// Sample lengths
			foreach (InstrumentInfo inst in newInstList)
				converterStream.Write_B_UINT32(inst?.OneshotLength ?? 0);

			// Repeat lengths
			foreach (InstrumentInfo inst in newInstList)
				converterStream.Write_B_UINT32(inst?.RepeatLength ?? 0);

			// Sample names
			byte[] buffer = new byte[30];

			for (uint i = 0; i < count; i++)
				converterStream.Write(buffer, 0, buffer.Length);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Write the sample data
		/// </summary>
		/********************************************************************/
		private bool WriteSampleData(uint[] offsets, ModuleStream moduleStream, ConverterStream converterStream)
		{
			moduleStream.Seek(moduleOffset + offsets[7], SeekOrigin.Begin);

			uint count = moduleStream.Read_B_UINT32();
			if (moduleStream.EndOfStream)
				return false;

			if (count == 0)
				return true;

			uint[] sampleLengths = new uint[count];
			moduleStream.ReadArray_B_UINT32s(sampleLengths, 0, (int)count);
			converterStream.WriteArray_B_UINT32s(sampleLengths, (int)count);

			if (moduleStream.EndOfStream)
				return false;

			for (int i = 0; i < count; i++)
			{
				uint sampleLen = sampleLengths[i];

				if (sampleLen > 0)
				{
					moduleStream.SetSampleDataInfo(i, (int)sampleLen);
					converterStream.WriteSampleDataMarker(i, (int)sampleLen);

					if (moduleStream.EndOfStream)
						return false;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Write the waveform part
		/// </summary>
		/********************************************************************/
		private bool WriteWaveformData(uint[] offsets, ModuleStream moduleStream, ConverterStream converterStream)
		{
			uint count = (offsets[5] - offsets[4]) / 128;

			converterStream.Write_B_UINT32(0x53595754);		// SYWT
			converterStream.Write_B_UINT32(count);

			return CopyData(moduleStream, converterStream, offsets[4], count, 128);
		}



		/********************************************************************/
		/// <summary>
		/// Write the ADSR tables part
		/// </summary>
		/********************************************************************/
		private bool WriteAdsrTables(uint[] offsets, ModuleStream moduleStream, ConverterStream converterStream)
		{
			uint count = (offsets[6] - offsets[5]) / 128;

			converterStream.Write_B_UINT32(0x53594152);		// SYAR
			converterStream.Write_B_UINT32(count);

			return CopyData(moduleStream, converterStream, offsets[5], count, 128);
		}



		/********************************************************************/
		/// <summary>
		/// Write the AMF tables part
		/// </summary>
		/********************************************************************/
		private bool WriteAmfTables(uint[] offsets, ModuleStream moduleStream, ConverterStream converterStream)
		{
			uint count = (offsets[7] - offsets[6]) / 128;

			converterStream.Write_B_UINT32(0x53594146);		// SYAF
			converterStream.Write_B_UINT32(count);

			return CopyData(moduleStream, converterStream, offsets[6], count, 128);
		}



		/********************************************************************/
		/// <summary>
		/// Write the editor data part
		/// </summary>
		/********************************************************************/
		private void WriteEditorData(ConverterStream converterStream)
		{
			byte[] mark = Encoding.ASCII.GetBytes("EDATV1.1");
			converterStream.Write(mark, 0, mark.Length);

			// Enable voices
			converterStream.Write_UINT8(1);
			converterStream.Write_UINT8(1);
			converterStream.Write_UINT8(1);
			converterStream.Write_UINT8(1);

			converterStream.Write_B_UINT16(0);		// Play position
			converterStream.Write_B_UINT16(0);		// Selected position
			converterStream.Write_B_UINT16(0);		// Selected song
			converterStream.Write_B_UINT16(0);		// ???
			converterStream.Write_B_UINT16(0);		// Pattern editor voice
			converterStream.Write_B_UINT16(0);		// Pattern editor row
		}
		#endregion
	}
}
