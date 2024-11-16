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
	/// Heatseeker mc1.0
	/// </summary>
	internal class HeatseekerFormat : CryptoburnersFormatBase
	{
		private byte numberOfPositions;
		private byte restartPosition;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT10;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 375)
				return false;

			// Do some common Cryptoburners check
			if (!Check(moduleStream, 0, 0xf8, out _))
				return false;

			// Get the number of positions
			moduleStream.Seek(0xf8, SeekOrigin.Begin);
			byte positionListLength = moduleStream.Read_UINT8();

			// Check highest pattern number
			if (FindHighestPatternNumber(moduleStream, 0xfa, positionListLength) > 64)
				return false;

			// Check pattern data
			moduleStream.Seek(0x17a, SeekOrigin.Begin);

			ushort emptyNum = 0;
			for (int i = 0; i < 512; i++)
			{
				// Get first part of pattern data
				ushort temp = moduleStream.Read_B_UINT16();

				if (temp == 0x8000)
					emptyNum++;
				else
				{
					if ((temp != 0x0000) && (temp != 0xc000))
					{
						// Check period
						if ((temp < 0x71) || (temp > 0x1358))
							return false;
					}
				}

				moduleStream.Seek(2, SeekOrigin.Current);
			}

			if (emptyNum == 0)
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
			byte[] positionList = new byte[numberOfPositions];

			moduleStream.Seek(0xfa, SeekOrigin.Begin);
			moduleStream.ReadInto(positionList, 0, numberOfPositions);

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
			moduleStream.Seek(0x17a, SeekOrigin.Begin);

			// Allocate temporary buffer holding each track
			byte[][] tracks = new byte[numberOfPatterns * 4][];

			// Allocate array to hold track numbers used for each pattern
			int[,] patternTracks = new int[numberOfPatterns, 4];
			int trackCount = 0;

			for (int i = 0; i < numberOfPatterns; i++)
			{
				// Loop the voices
				for (int j = 0; j < 4; j++)
				{
					byte[] trackData = new byte[256];
					int trackOffset = 0;

					patternTracks[i, j] = trackCount;

					// Loop the rows
					for (int k = 0; k < 64; k++)
					{
						byte byt1 = moduleStream.Read_UINT8();
						byte byt2 = moduleStream.Read_UINT8();
						byte byt3 = moduleStream.Read_UINT8();
						byte byt4 = moduleStream.Read_UINT8();

						// Check for commands
						ushort temp = (ushort)((byt1 << 8) | byt2);

						// Copy track
						if (temp == 0xc000)
						{
							int trackNumber = ((byt3 << 8) | byt4) / 4;
							patternTracks[i, j] = patternTracks[trackNumber / 4, trackNumber % 4];

							trackData = null;
							break;
						}

						// Normal note?
						if (temp < 0x8000)
						{
							trackData[trackOffset++] = byt1;
							trackData[trackOffset++] = byt2;
							trackData[trackOffset++] = byt3;
							trackData[trackOffset++] = byt4;
						}
						else
						{
							// Empty lines
							k += byt4;
							trackOffset += (byt4 + 1) * 4;
						}
					}

					if (trackData != null)
						tracks[trackCount++] = trackData;
				}
			}

			// Now all the tracks has been read, so build up the patterns and return them
			byte[] pattern = new byte[1024];

			for (int i = 0; i < numberOfPatterns; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					int trackNumber = patternTracks[i, j];
					byte[] trackData = tracks[trackNumber];

					int patternOffset = j * 4;
					int trackOffset = 0;

					for (int k = 0; k < 64; k++)
					{
						pattern[patternOffset] = trackData[trackOffset++];
						pattern[patternOffset + 1] = trackData[trackOffset++];
						pattern[patternOffset + 2] = trackData[trackOffset++];
						pattern[patternOffset + 3] = trackData[trackOffset++];

						patternOffset += 16;
					}
				}

				yield return pattern;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Write all the samples
		/// </summary>
		/********************************************************************/
		protected override bool WriteSampleData(ModuleStream moduleStream, ConverterStream converterStream)
		{
			return SaveMarksForAllSamples(moduleStream, converterStream);
		}
		#endregion
	}
}
