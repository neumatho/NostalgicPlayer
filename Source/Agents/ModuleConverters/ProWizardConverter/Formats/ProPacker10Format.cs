/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter.Formats
{
	/// <summary>
	/// ProPacker 1.0
	/// </summary>
	internal class ProPacker10Format : CryptoburnersFormatBase
	{
		private uint numberOfTracks;
		private byte numberOfPositions;
		private byte restartPosition;

		private byte[][] trackTable;
		private byte[] positionList;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT28;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 0x2fa)
				return false;

			// Do some common Cryptoburners check
			if (!Check(moduleStream, 0, 0xf8, out uint samplesSize))
				return false;

			// The first two track numbers in list 2 may not be 0
			moduleStream.Seek(0x17a, SeekOrigin.Begin);

			if ((moduleStream.Read_UINT8() == 0) || (moduleStream.Read_UINT8() == 0))
				return false;

			// Find the highest track number
			moduleStream.Seek(0xfa, SeekOrigin.Begin);

			numberOfTracks = 0;
			for (int i = 0; i < 512; i++)
			{
				byte temp = moduleStream.Read_UINT8();
				if (temp > numberOfTracks)
					numberOfTracks = temp;
			}

			numberOfTracks = (numberOfTracks + 1) * 64;

			// Check notes in the tracks
			moduleStream.Seek(0x2fa, SeekOrigin.Begin);

			for (int j = 0; j < numberOfTracks; j++)
			{
				ushort temp1 = moduleStream.Read_B_UINT16();
				if (temp1 != 0)
				{
					if ((temp1 < 0x71) || (temp1 > 0x1358))
						return false;
				}

				moduleStream.Seek(2, SeekOrigin.Current);
			}

			// Check module length
			if ((0x2fa + numberOfTracks * 4 + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
				return false;

			return true;
		}



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
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<byte[]> GetPatterns(ModuleStream moduleStream)
		{
			byte[] pattern = new byte[1024];

			// Convert the pattern data
			sbyte lastPattern = -1;

			for (int i = 0; i < numberOfPositions; i++)
			{
				if (positionList[i] > lastPattern)
				{
					lastPattern++;

					for (int j = 0; j < 4; j++)
					{
						// Get the track offset
						uint trackOffset = 0x2fa + trackTable[j][i] * 256U;
						moduleStream.Seek(trackOffset, SeekOrigin.Begin);

						// Copy the track
						for (int k = 0; k < 64; k++)
						{
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



		/********************************************************************/
		/// <summary>
		/// Write all the samples
		/// </summary>
		/********************************************************************/
		protected override bool WriteSampleData(ModuleStream moduleStream, ConverterStream converterStream)
		{
			moduleStream.Seek(0x2fa + numberOfTracks * 4, SeekOrigin.Begin);

			return SaveMarksForAllSamples(moduleStream, converterStream);
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
