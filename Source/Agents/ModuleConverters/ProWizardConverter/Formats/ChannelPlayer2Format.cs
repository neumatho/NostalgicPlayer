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
	/// Channel Player 2
	/// </summary>
	internal class ChannelPlayer2Format : ChannelPlayerFormatBase
	{
		protected int trackTableOffset;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT3;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			return CheckForChannelPlayerFormat(moduleStream) == 2;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<byte[]> GetPatterns(ModuleStream moduleStream)
		{
			byte[] pattern = new byte[1024];
			ushort[] trackNumbers = new ushort[4];

			int patternStartOffset = sampleInfoLength + positionListLength;
			int trackOffset = trackTableOffset;
			sbyte lastPattern = -1;

			for (int i = 0; i < numberOfPositions; i++)
			{
				if ((sbyte)positionList[i] > lastPattern)
				{
					lastPattern++;

					moduleStream.Seek(trackOffset, SeekOrigin.Begin);
					moduleStream.ReadArray_B_UINT16s(trackNumbers, 0, 4);
					trackOffset += 8;

					// Take each voice
					for (int j = 0; j < 4; j++)
					{
						// Get the pattern offset
						int patternOffset = patternStartOffset + trackNumbers[j];
						moduleStream.Seek(patternOffset, SeekOrigin.Begin);

						// Take all the rows
						for (int k = 0; k < 64; k++)
						{
							byte byte1, byte2, byte3, byte4;

							byte byt = moduleStream.Read_UINT8();

							if (byt == 0x80)
							{
								// No note
								byte1 = 0x00;
								byte2 = 0x00;
								byte3 = (byte)(moduleStream.Read_UINT8() & 0x0f);
								byte4 = 0x00;
							}
							else
							{
								if (byt > 0x80)
								{
									// Without effect value
									//
									// Get sample
									byte1 = (byte)((byt & 0x01) << 4);
									byte3 = moduleStream.Read_UINT8();	// Sample number + effect

									// Get note
									byte2 = (byte)((byt - 0x80) & 0xfe);
									if (byte2 != 0)
									{
										byte2 -= 2;
										byte2 /= 2;

										byte1 |= periods[byte2, 0];
										byte2 = periods[byte2, 1];
									}

									byte4 = 0x00;
								}
								else
								{
									// Full note
									//
									// Get sample
									byte1 = (byte)((byt & 0x01) << 4);
									byte3 = moduleStream.Read_UINT8();	// Sample number + effect

									// Get note
									byte2 = (byte)(byt & 0xfe);
									if (byte2 != 0)
									{
										byte2 -= 2;
										byte2 /= 2;

										byte1 |= periods[byte2, 0];
										byte2 = periods[byte2, 1];
									}

									// Get effect value
									byte4 = moduleStream.Read_UINT8();

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
					trackOffset += 8;
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
			moduleStream.Seek(0, SeekOrigin.Begin);
			trackTableOffset = ((moduleStream.Read_B_UINT16() & 0xfff0) >> 4) * 16 + 10;
			moduleStream.Seek(trackTableOffset, SeekOrigin.Begin);

			// Begin to create the position table
			List<ulong> trackCombinations = new List<ulong>();

			// Add the first track combination directly in the list
			trackCombinations.Add(moduleStream.Read_B_UINT64());
			numberOfPatterns = 1;

			numberOfPositions = positionListLength / 8;
			for (int i = 1; i < numberOfPositions; i++)
			{
				ulong trackNumbers = moduleStream.Read_B_UINT64();

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
