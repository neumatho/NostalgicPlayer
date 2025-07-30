/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter.Formats
{
	/// <summary>
	/// Promizer base class for version 1.0c and 1.8a
	/// </summary>
	internal abstract class Promizer1xFormatBase : Promizer01Format
	{
		protected struct Offsets
		{
			public uint Offset1 { get; set; }
			public uint Offset2 { get; set; }
			public uint Offset3 { get; set; }
		}

		private Offsets offsets;

		private byte numberOfPositions;
		private byte[] positionList;

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Prepare conversion by initialize what is needed etc.
		/// </summary>
		/********************************************************************/
		protected override bool PrepareConversion(ModuleStream moduleStream)
		{
			// Remember the offsets
			offsets = GetOffsets();

			CreatePositionList(moduleStream);

			// Seek to sample information here, so we can reuse the GetSamples() method from derived class
			moduleStream.Seek(offsets.Offset2, SeekOrigin.Begin);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return the position list. Note that the returned list must be the
		/// same length as to be played
		/// </summary>
		/********************************************************************/
		protected override Span<byte> GetPositionList(ModuleStream moduleStream)
		{
			return positionList;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<byte[]> GetPatterns(ModuleStream moduleStream)
		{
			byte[] pattern = new byte[1024];

			// Find base and notes table offsets
			uint bas = offsets.Offset2 + 31 * 8 + 2;
			uint notesTable = bas + 512;

			// Read offset table
			uint[] patternOffsets = new uint[128];
			moduleStream.Seek(bas, SeekOrigin.Begin);
			moduleStream.ReadArray_B_UINT32s(patternOffsets, 0, 128);

			// Get number of notes
			moduleStream.Seek(offsets.Offset1, SeekOrigin.Begin);
			uint numberOfNotes = moduleStream.Read_B_UINT32();

			// Read note offset table
			ushort[] singleNoteOffsets = new ushort[numberOfNotes / 2];
			moduleStream.Seek(notesTable, SeekOrigin.Begin);
			moduleStream.ReadArray_B_UINT16s(singleNoteOffsets, 0, singleNoteOffsets.Length);

			uint refOffset = (uint)moduleStream.Position;

			// Convert the pattern data
			sbyte lastPattern = -1;

			for (int i = 0; i < numberOfPositions; i++)
			{
				if (positionList[i] > lastPattern)
				{
					uint writeOffset = 0;
					lastPattern++;

					// Clear the pattern data
					Array.Clear(pattern);

					// Find offset to pattern
					uint offset = patternOffsets[i] / 2;

					for (int j = 0; j < 64; j++)
					{
						bool breakFlag = false;

						for (int k = 0; k < 4; k++)
						{
							byte byt1, byt2, byt3, byt4;

							if ((notesTable + offset * 2) >= refOffset)
							{
								offset++;
								byt1 = 0;
								byt2 = 0;
								byt3 = 0;
								byt4 = 0;
							}
							else
							{
								moduleStream.Seek(singleNoteOffsets[offset++] * 4 + refOffset, SeekOrigin.Begin);
								byt1 = moduleStream.Read_UINT8();
								byt2 = moduleStream.Read_UINT8();
								byt3 = moduleStream.Read_UINT8();
								byt4 = moduleStream.Read_UINT8();
							}

							// Store the pattern data
							pattern[writeOffset++] = byt1;
							pattern[writeOffset++] = byt2;
							pattern[writeOffset++] = byt3;
							pattern[writeOffset++] = byt4;

							// Did we reach a break effect?
							byt3 &= 0x0f;
							if ((byt3 == 0x0d) || (byt3 == 0x0b))
								breakFlag = true;
						}

						if (breakFlag)
							break;
					}

					// Adjust fine tunes
					AdjustFineTunes(pattern);

					yield return pattern;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Write all the samples
		/// </summary>
		/********************************************************************/
		protected override bool WriteSampleData(PlayerFileInfo fileInfo, ModuleStream moduleStream, ConverterStream converterStream)
		{
			moduleStream.Seek(offsets.Offset3, SeekOrigin.Begin);
			uint sampleDataOffset = moduleStream.Read_B_UINT32();
			moduleStream.Seek(offsets.Offset3 + sampleDataOffset + 4, SeekOrigin.Begin);

			return SaveMarksForAllSamples(moduleStream, converterStream);
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Return needed offsets in the module
		/// </summary>
		/********************************************************************/
		protected abstract Offsets GetOffsets();
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format and return
		/// its version. -1 for unknown
		/// </summary>
		/********************************************************************/
		protected int CheckForPromizerFormat(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 0x1168)
				return -1;

			// Check some of the code
			moduleStream.Seek(14, SeekOrigin.Begin);
			if (moduleStream.Read_B_UINT32() != 0x48e780c0)
				return -1;

			// Is it module 1.0 or 1.8?
			int formatVersion;

			moduleStream.Seek(21, SeekOrigin.Begin);
			if (moduleStream.Read_UINT8() == 0xce)
				formatVersion = 10;
			else
				formatVersion = 18;

			uint baseOffset = GetOffsets().Offset3;

			// Check sample information
			moduleStream.Seek(baseOffset + 8, SeekOrigin.Begin);

			uint samplesSize = 0;

			for (int i = 0; i < 31; i++)
			{
				// Get sample size
				ushort temp = moduleStream.Read_B_UINT16();

				// Check volume & fine tune
				if ((moduleStream.Read_UINT8() > 0x0f) || (moduleStream.Read_UINT8() > 0x40))
					return -1;

				// Check sample size
				if (temp != 0)
				{
					samplesSize += temp * 2U;

					// Check loop
					if ((moduleStream.Read_B_UINT16() + moduleStream.Read_B_UINT16()) > temp)
						return -1;
				}
				else
					moduleStream.Seek(4, SeekOrigin.Current);
			}

			// Check the module length
			moduleStream.Seek(baseOffset, SeekOrigin.Begin);
			uint temp1 = moduleStream.Read_B_UINT32();

			if ((baseOffset + temp1 + 4 + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
				return -1;

			return formatVersion;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Create position list and find the number of patterns needed
		/// </summary>
		/********************************************************************/
		private void CreatePositionList(ModuleStream moduleStream)
		{
			moduleStream.Seek(offsets.Offset3 + 0x100, SeekOrigin.Begin);
			numberOfPositions = (byte)(moduleStream.Read_B_UINT16() / 4);

			positionList = BuildPositionList(numberOfPositions, (pos) => moduleStream.Read_B_UINT32());
		}
		#endregion
	}
}
