/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter.Formats
{
	/// <summary>
	/// Channel Player 1
	/// </summary>
	internal class ChannelPlayer1Format : ChannelPlayerFormatBase
	{
		private int trackTableOffset;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT2;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			return CheckForChannelPlayerFormat(moduleStream) == 1;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<byte[]> GetPatterns(ModuleStream moduleStream)
		{
			byte[] pattern = new byte[1024];
			byte[] trackNumbers = new byte[4];

			int patternStartOffset = sampleInfoLength + positionListLength;
			sbyte lastPattern = -1;

			for (int i = 0; i < numberOfPositions; i++)
			{
				if ((sbyte)positionList[i] > lastPattern)
				{
					lastPattern++;

					moduleStream.Seek(trackTableOffset, SeekOrigin.Begin);
					moduleStream.ReadInto(trackNumbers, 0, 4);
					trackTableOffset += 4;

					// Do we have 4 zeros as the track numbers?
					if ((trackNumbers[0] == 0) && (trackNumbers[1] == 0) && (trackNumbers[2] == 0) && (trackNumbers[3] == 0))
						break;

					// Take each voice
					for (int j = 0; j < 4; j++)
					{
						// Get the pattern offset
						int patternOffset = patternStartOffset + trackNumbers[j] * 0xc0;
						moduleStream.Seek(patternOffset, SeekOrigin.Begin);

						// Take all the rows
						for (int k = 0; k < 64; k++)
						{
							// Get sample
							byte readByte = moduleStream.Read_UINT8();
							byte byte1 = (byte)((readByte & 0x01) << 4);
							byte byte3 = moduleStream.Read_UINT8();		// Sample number + effect

							// Get note
							byte byte2 = (byte)(readByte & 0xfe);
							if (byte2 != 0)
							{
								byte2 -= 2;
								byte2 /= 2;

								byte1 |= periods[byte2, 0];
								byte2 = periods[byte2, 1];
							}

							// Get effect value
							byte byte4 = moduleStream.Read_UINT8();

							// Fix some of the effects
							switch (byte3 & 0x0f)
							{
								// Pattern break
								case 0xd:
								{
									// Convert to decimal
									byte4 = (byte)(((byte4 / 10) * 16) + (byte4 % 10));
									break;
								}

								// Volume slide + TonePlusVol + VibratoPlusVol
								case 0xa:
								case 0x5:
								case 0x6:
								{
									// Convert from signed value
									if (byte4 >= 0xf1)
										byte4 = unchecked((byte)(-(sbyte)byte4));
									else
										byte4 <<= 4;

									break;
								}
							}

							// Write the pattern data
							pattern[k * 16 + j * 4] = byte1;
							pattern[k * 16 + j * 4 + 1] = byte2;
							pattern[k * 16 + j * 4 + 2] = byte3;
							pattern[k * 16 + j * 4 + 3] = byte4;
						}
					}

					yield return pattern;
				}
				else
					trackTableOffset += 4;
			}
		}
		#endregion

		#region ChannelPlayerFormatBase implementation
		/********************************************************************/
		/// <summary>
		/// Build the position table structure
		/// </summary>
		/********************************************************************/
		protected override void CreatePositionTable(ModuleStream moduleStream)
		{
			positionList = new byte[128];

			// Jump to the track offset table
			trackTableOffset = ((sampleInfoLength & 0xfff0) >> 4) * 16 + 10;
			moduleStream.Seek(trackTableOffset, SeekOrigin.Begin);

			// Begin to create the position table
			List<uint> trackCombinations = new List<uint>();

			// Add the first track combination directly in the list
			trackCombinations.Add(moduleStream.Read_B_UINT32());
			numberOfPatterns = 1;

			numberOfPositions = positionListLength / 4;
			for (int i = 1; i < numberOfPositions; i++)
			{
				uint trackNumbers = moduleStream.Read_B_UINT32();

				// Check to see if this track combination has already been used previously
				bool found = false;

				for (int j = 0; j < trackCombinations.Count; j++)
				{
					if (trackCombinations[j] == trackNumbers)
					{
						// Found an equal track combination
						positionList[i] = (byte)j;
						found = true;
						break;
					}
				}

				if (!found)
				{
					positionList[i] = (byte)numberOfPatterns++;
					trackCombinations.Add(trackNumbers);
				}
			}
		}
		#endregion
	}
}
