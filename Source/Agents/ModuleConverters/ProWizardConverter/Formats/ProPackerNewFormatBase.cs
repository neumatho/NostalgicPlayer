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
	/// ProPacker base class for version 2.1 and 3.0
	/// </summary>
	internal abstract class ProPackerNewFormatBase : CryptoburnersFormatBase
	{
		private uint numberOfTracks;
		private byte numberOfPositions;
		private byte restartPosition;

		private byte[][] trackTable;
		private ushort[] referenceTable;
		private byte[] positionList;

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Prepare conversion by initialize what is needed etc.
		/// </summary>
		/********************************************************************/
		protected override bool PrepareConversion(ModuleStream moduleStream)
		{
			// Get number of positions and restart position
			moduleStream.Seek(0xf8, SeekOrigin.Begin);

			numberOfPositions = moduleStream.Read_UINT8();
			restartPosition = moduleStream.Read_UINT8();

			CreateTrackTable(moduleStream);
			CreateReferenceTable(moduleStream);
			CreatePositionList();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
			moduleStream.Seek(0, SeekOrigin.Begin);

			for (int i = 0; i < 31; i++)
			{
				ushort length = moduleStream.Read_B_UINT16();
				byte fineTune = moduleStream.Read_UINT8();
				byte volume = moduleStream.Read_UINT8();
				ushort loopStart = moduleStream.Read_B_UINT16();
				ushort loopLength = moduleStream.Read_B_UINT16();

				yield return new SampleInfo
				{
					Name = null,
					Length = length,
					LoopStart = loopStart,
					LoopLength = loopLength,
					Volume = volume,
					FineTune = fineTune
				};
			}
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
		/// Return the restart position
		/// </summary>
		/********************************************************************/
		protected override byte GetRestartPosition(ModuleStream moduleStream)
		{
			return restartPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Write all the samples
		/// </summary>
		/********************************************************************/
		protected override bool WriteSampleData(PlayerFileInfo fileInfo, ModuleStream moduleStream, ConverterStream converterStream)
		{
			moduleStream.Seek(0x2fa + numberOfTracks, SeekOrigin.Begin);
			uint sampleDataOffset = moduleStream.Read_B_UINT32();
			moduleStream.Seek(sampleDataOffset, SeekOrigin.Current);

			return SaveMarksForAllSamples(moduleStream, converterStream);
		}
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format and return
		/// its version. -1 for unknown
		/// </summary>
		/********************************************************************/
		protected int CheckForProPackerFormat(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 0x2fa)
				return -1;

			// Check first value in track "data"
			moduleStream.Seek(0x2fa, SeekOrigin.Begin);
			if (moduleStream.Read_B_UINT16() != 0)
				return -1;

			// Do some common Cryptoburners check
			if (!Check(moduleStream, 0, 0xf8, out uint samplesSize))
				return -1;

			// Find the highest track number
			moduleStream.Seek(0xfa, SeekOrigin.Begin);

			numberOfTracks = 0;
			for (int i = 0; i < 512; i++)
			{
				byte temp = moduleStream.Read_UINT8();
				if (temp > numberOfTracks)
					numberOfTracks = temp;
			}

			numberOfTracks = (numberOfTracks + 1) * 64 * 2;

			// Check module length
			uint trackOffset = 0x2fa + numberOfTracks;
			moduleStream.Seek(trackOffset, SeekOrigin.Begin);
			uint tempVal = moduleStream.Read_B_UINT32();
			uint temp1 = tempVal + 4 + samplesSize;

			if ((temp1 & 0x1) != 0)
				return -1;

			if (temp1 > (moduleStream.Length + MaxNumberOfMissingBytes))
				return -1;

			// Get reference table size
			temp1 = tempVal;
			if (temp1 == 0)
				return -1;

			temp1 /= 4;

			// Check notes in the reference table
			if (temp1 > (64 * 2))
				temp1 = 64 * 2;

			ushort count = 0;
			for (int i = 0; i < temp1; i++)
			{
				ushort temp = moduleStream.Read_B_UINT16();
				if (temp != 0)
				{
					if ((temp < 0x71) || (temp > 0x1358))
						return -1;

					count++;
				}

				moduleStream.Seek(2, SeekOrigin.Current);
			}

			if (count == 0)
				return -1;

			// Check the track data and find out which version of ProPacker it is
			temp1 = (numberOfTracks / 128) * 64;
			if (temp1 > (64 * 4))
				temp1 = 64 * 4;

			moduleStream.Seek(0x2fa, SeekOrigin.Begin);

			count = 0;
			for (int i = 0; i < temp1; i++)
			{
				// Get the reference value
				ushort temp = moduleStream.Read_B_UINT16();
				if (temp >= 0x1800)
					return -1;

				if ((temp % 4) != 0)
					count++;
			}

			int formatVersion;

			if (count != 0)
				formatVersion = 21;
			else
				formatVersion = 30;

			return formatVersion;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected IEnumerable<byte[]> GetAllPatterns(ModuleStream moduleStream, uint referenceMultiply)
		{
			byte[] pattern = new byte[1024];

			// Convert the pattern data
			uint refStart = 0x2fa + numberOfTracks + 4;
			sbyte lastPattern = -1;

			for (int i = 0; i < numberOfPositions; i++)
			{
				if (positionList[i] > lastPattern)
				{
					lastPattern++;

					for (int j = 0; j < 4; j++)
					{
						// Get the track offset
						uint trackOffset = trackTable[j][i] * 64U;

						// Copy the track
						for (int k = 0; k < 64; k++)
						{
							// Get the reference offset
							uint refOffset = referenceTable[trackOffset++];
							refOffset *= referenceMultiply;
							refOffset += refStart;

							// Copy the pattern data
							moduleStream.Seek(refOffset, SeekOrigin.Begin);

							pattern[k * 16 + j * 4] = moduleStream.Read_UINT8();
							pattern[k * 16 + j * 4 + 1] = moduleStream.Read_UINT8();
							pattern[k * 16 + j * 4 + 2] = moduleStream.Read_UINT8();
							pattern[k * 16 + j * 4 + 3] = moduleStream.Read_UINT8();
						}
					}

					yield return pattern;
				}
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Load and build the track table
		/// </summary>
		/********************************************************************/
		private void CreateTrackTable(ModuleStream moduleStream)
		{
			trackTable = new byte[4][];

			trackTable[0] = new byte[128];
			moduleStream.ReadInto(trackTable[0], 0, 128);

			trackTable[1] = new byte[128];
			moduleStream.ReadInto(trackTable[1], 0, 128);

			trackTable[2] = new byte[128];
			moduleStream.ReadInto(trackTable[2], 0, 128);

			trackTable[3] = new byte[128];
			moduleStream.ReadInto(trackTable[3], 0, 128);
		}



		/********************************************************************/
		/// <summary>
		/// Load and build the reference table
		/// </summary>
		/********************************************************************/
		private void CreateReferenceTable(ModuleStream moduleStream)
		{
			referenceTable = new ushort[numberOfTracks / 2];

			moduleStream.Seek(0x2fa, SeekOrigin.Begin);
			moduleStream.ReadArray_B_UINT16s(referenceTable, 0, referenceTable.Length);
		}



		/********************************************************************/
		/// <summary>
		/// Create position list and find the number of patterns needed
		/// </summary>
		/********************************************************************/
		private void CreatePositionList()
		{
			positionList = BuildPositionList(numberOfPositions, (pos) => ((uint)trackTable[0][pos] << 24) | ((uint)trackTable[1][pos] << 16) | ((uint)trackTable[2][pos] << 8) | trackTable[3][pos]);
		}
		#endregion
	}
}
