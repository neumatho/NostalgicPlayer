/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter.Formats
{
	/// <summary>
	/// The Player 5.0A
	/// </summary>
	internal class ThePlayer50AFormat : ThePlayer5x_6xFormatBase
	{
		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT39 + PackedFormat;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			return CheckForThePlayerFormat(moduleStream) == 50;
		}



		/********************************************************************/
		/// <summary>
		/// Return the position list. Note that the returned list must be the
		/// same length as to be played
		/// </summary>
		/********************************************************************/
		protected override Span<byte> GetPositionList(ModuleStream moduleStream)
		{
			return LoadPositionList(moduleStream, 2);
		}
		#endregion

		#region ThePlayer5x_6xFormatBase implementation
		/********************************************************************/
		/// <summary>
		/// Will convert a single track line to a ProTracker track line
		/// </summary>
		/********************************************************************/
		protected override int ConvertData(ModuleStream moduleStream, byte[] pattern, int writeOffset)
		{
			int k;

			// Get first byte
			byte temp2 = moduleStream.Read_UINT8();
			if (temp2 == 0x80)
			{
				// Copy notes
				ushort temp = (ushort)(moduleStream.Read_UINT8() + 1);		// Number of notes to copy
				ushort temp1 = moduleStream.Read_B_UINT16();				// Offset to subtract

				// Remember current file position and seek to the new one
				long oldPosition = moduleStream.Position;
				moduleStream.Seek(oldPosition - temp1, SeekOrigin.Begin);

				k = 0;

				for (; temp != 0; temp--)
				{
					// Get first byte
					temp2 = moduleStream.Read_UINT8();
					k += ConvertData(moduleStream, temp2, pattern, ref writeOffset);
				}

				// Go back to original position
				moduleStream.Seek(oldPosition, SeekOrigin.Begin);
			}
			else
				k = ConvertData(moduleStream, temp2, pattern, ref writeOffset);

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

			if (temp2 > 0x80)
			{
				// Multiple notes
				temp2 = unchecked((byte)((-(sbyte)temp2) - 1));
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

				if (temp2 <= 64)
				{
					// Empty rows
					SetMultipleRows(temp2, 0x00, 0x00, 0x00, 0x00, pattern, ref writeOffset);
					k += temp2;
				}
				else
				{
					// Copy the same data value times
					temp2 = (byte)-(sbyte)temp2;

					SetMultipleRows(temp2, byt1, byt2, byt3, byt4, pattern, ref writeOffset);
					k += temp2;
				}
			}
			else
			{
				// Single note
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

			return k;
		}
		#endregion
	}
}
