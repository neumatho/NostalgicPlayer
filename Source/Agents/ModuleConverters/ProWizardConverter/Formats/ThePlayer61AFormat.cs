using System;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter.Formats
{
	/// <summary>
	/// The Player 6.1A
	/// </summary>
	internal class ThePlayer61AFormat : ThePlayer6xFormatBase
	{
		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT41 + PackedFormat;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			return CheckForThePlayerFormat(moduleStream) == 61;
		}



		/********************************************************************/
		/// <summary>
		/// Return the position list. Note that the returned list must be the
		/// same length as to be played
		/// </summary>
		/********************************************************************/
		protected override Span<byte> GetPositionList(ModuleStream moduleStream)
		{
			return LoadPositionList(moduleStream, 1);
		}



		/********************************************************************/
		/// <summary>
		/// Will convert a single track line to a ProTracker track line
		/// </summary>
		/********************************************************************/
		protected override int ConvertData(ModuleStream moduleStream, byte[] pattern, int writeOffset)
		{
			// Get first byte
			byte temp2 = moduleStream.Read_UINT8();
			int k = ConvertData(moduleStream, temp2, pattern, ref writeOffset);
			if (k == 0)
			{
				// Copy notes (===> FF 01 or FF 41 09 or FF CB 0153 <<===)
				temp2 = moduleStream.Read_UINT8();
				if (temp2 < 0x40)
				{
					// Multi empty rows
					temp2++;

					SetMultipleRows(temp2, 0x00, 0x00, 0x00, 0x00, pattern, ref writeOffset);
					k += temp2;
				}
				else
				{
					ushort temp;
					uint readOffset;

					if (temp2 < 0x80)
					{
						// Number of notes to copy
						temp = (ushort)(temp2 - 0x40);

						// Find new read offset
						readOffset = (uint)moduleStream.Position - moduleStream.Read_UINT8() + 1;
					}
					else
					{
						// Number of notes to copy
						temp = (ushort)(temp2 - 0xc0);

						// Find new read offset
						ushort temp1 = moduleStream.Read_B_UINT16();
						readOffset = (uint)moduleStream.Position - temp1;
					}

					// Remember current file position and seek to the new one
					long oldPosition = moduleStream.Position;
					moduleStream.Seek(readOffset, SeekOrigin.Begin);

					temp++;
					for (; temp != 0; temp--)
					{
						// Get first byte
						temp2 = moduleStream.Read_UINT8();
						int bytesRead = ConvertData(moduleStream, temp2, pattern, ref writeOffset);
						if (bytesRead == 0)
						{
							// Copy notes (===> FF 01 or FF 41 09 or FF CB 0153 <<===)
							temp2 = (byte)(moduleStream.Read_UINT8() + 1);

							SetMultipleRows(temp2, 0x00, 0x00, 0x00, 0x00, pattern, ref writeOffset);
							k += temp2;
						}
						else
							k += bytesRead;
					}

					// Go back to original position
					moduleStream.Seek(oldPosition, SeekOrigin.Begin);
				}
			}

			return k;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will convert a single track line to a ProTracker track line
		/// </summary>
		/********************************************************************/
		private int ConvertData(ModuleStream moduleStream, byte temp2, byte[] pattern, ref int writeOffset)
		{
			int k = 0;

			if (temp2 == 0x7f)
			{
				// One empty line
				pattern[writeOffset] = 0x00;
				pattern[writeOffset + 1] = 0x00;
				pattern[writeOffset + 2] = 0x00;
				pattern[writeOffset + 3] = 0x00;

				writeOffset += 16;
				k++;
			}
			else if (temp2 < 0x60)
			{
				// Just a full note (===> 1A 5F05 <=== note + smp + efx)
				GetNote(temp2, out byte byt1, out byte byt2);

				// Get the sample number, effect and effect value
				byte byt3 = moduleStream.Read_UINT8();
				byte byt4 = moduleStream.Read_UINT8();

				// Check the effect
				CheckEffect(ref byt3, ref byt4);

				// Write pattern data
				pattern[writeOffset] = byt1;
				pattern[writeOffset + 1] = byt2;
				pattern[writeOffset + 2] = byt3;
				pattern[writeOffset + 3] = byt4;

				writeOffset += 16;
				k++;
			}
			else if (temp2 < 0x80)
			{
				if ((temp2 & 0xf0) == 0x60)
				{
					// Only effect (===> 6 310 <=== 6 + efx)
					byte byt3 = (byte)(temp2 & 0x0f);
					byte byt4 = moduleStream.Read_UINT8();

					// Check the effect
					CheckEffect(ref byt3, ref byt4);

					// Write pattern data
					pattern[writeOffset] = 0x00;
					pattern[writeOffset + 1] = 0x00;
					pattern[writeOffset + 2] = byt3;
					pattern[writeOffset + 3] = byt4;
				}
				else
				{
					// Note + sample (===> 7 32 8 <=== 7 + note + smp)
					byte temp1 = moduleStream.Read_UINT8();

					temp2 = (byte)((temp2 << 4) | (temp1 >> 4));
					GetNote(temp2, out byte byt1, out byte byt2);

					// Write pattern data
					pattern[writeOffset] = byt1;
					pattern[writeOffset + 1] = byt2;
					pattern[writeOffset + 2] = (byte)(temp1 << 4);
					pattern[writeOffset + 3] = 0x00;
				}

				writeOffset += 16;
				k++;
			}
			else if (temp2 < 0xd0)
			{
				// Multiple notes and effect (===> A2 1602 84 <<==)
				temp2 -= 0x80;
				GetNote(temp2, out byte byt1, out byte byt2);

				// Get the sample number, effect and effect value
				byte byt3 = moduleStream.Read_UINT8();
				byte byt4 = moduleStream.Read_UINT8();

				// Check the effect
				CheckEffect(ref byt3, ref byt4);

				// Write pattern data
				pattern[writeOffset] = byt1;
				pattern[writeOffset + 1] = byt2;
				pattern[writeOffset + 2] = byt3;
				pattern[writeOffset + 3] = byt4;

				writeOffset += 16;
				k++;

				// Get the value byte
				temp2 = moduleStream.Read_UINT8();

				if (temp2 < 0x80)
				{
					// Empty rows
					SetMultipleRows(temp2, 0x00, 0x00, 0x00, 0x00, pattern, ref writeOffset);
					k += temp2;
				}
				else
				{
					// Copy the same data value times
					temp2 -= 0x80;

					SetMultipleRows(temp2, byt1, byt2, byt3, byt4, pattern, ref writeOffset);
					k += temp2;
				}
			}
			else if (temp2 < 0xf0)
			{
				// Multi effect (===> E 602 84 <===)
				byte byt3 = (byte)(temp2 & 0x0f);
				byte byt4 = moduleStream.Read_UINT8();

				// Check the effect
				CheckEffect(ref byt3, ref byt4);

				// Write pattern data
				pattern[writeOffset] = 0x00;
				pattern[writeOffset + 1] = 0x00;
				pattern[writeOffset + 2] = byt3;
				pattern[writeOffset + 3] = byt4;

				writeOffset += 16;
				k++;

				// Get the value byte
				temp2 = moduleStream.Read_UINT8();

				if (temp2 < 0x80)
				{
					// Empty rows
					SetMultipleRows(temp2, 0x00, 0x00, 0x00, 0x00, pattern, ref writeOffset);
					k += temp2;
				}
				else
				{
					// Copy the same data value times
					temp2 -= 0x80;

					SetMultipleRows(temp2, 0x00, 0x00, byt3, byt4, pattern, ref writeOffset);
					k += temp2;
				}
			}
			else if (temp2 < 0xff)
			{
				// Multi notes (===> F 1A 7 04 <=== note + smp + repeat)
				byte temp1 = moduleStream.Read_UINT8();

				temp2 = (byte)((temp2 << 4) | (temp1 >> 4));
				GetNote(temp2, out byte byt1, out byte byt2);

				// Get the sample number
				byte byt3 = (byte)(temp1 << 4);

				// Write pattern data
				pattern[writeOffset] = byt1;
				pattern[writeOffset + 1] = byt2;
				pattern[writeOffset + 2] = byt3;
				pattern[writeOffset + 3] = 0x00;

				writeOffset += 16;
				k++;

				// Get the value byte
				temp2 = moduleStream.Read_UINT8();

				if (temp2 < 0x80)
				{
					// Empty rows
					SetMultipleRows(temp2, 0x00, 0x00, 0x00, 0x00, pattern, ref writeOffset);
					k += temp2;
				}
				else
				{
					// Copy the same data value times
					temp2 -= 0x80;

					SetMultipleRows(temp2, byt1, byt2, byt3, 0x00, pattern, ref writeOffset);
					k += temp2;
				}
			}

			return k;
		}
		#endregion
	}
}
