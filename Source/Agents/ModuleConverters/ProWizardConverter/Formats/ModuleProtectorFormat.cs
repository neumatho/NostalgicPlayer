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
	/// Module Protector
	/// </summary>
	internal class ModuleProtectorFormat : CryptoburnersFormatBase
	{
		private int startOffset;

		private byte numberOfPositions;
		private byte restartPosition;
		private ushort realNumberOfPatterns;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT14;
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

			// Some Module Protector modules have an ID in the beginning. Check for this
			moduleStream.Seek(0, SeekOrigin.Begin);

			int offset = 0;
			if (moduleStream.Read_B_UINT32() == 0x54524b31)		// TRK1
				offset = 4;

			// Do some common Cryptoburners check
			if (!Check(moduleStream, offset, offset + 0xf8, out _))
				return false;

			// Check highest pattern number
			if (FindHighestPatternNumber(moduleStream, offset + 0xfa, 128) > 64)		// Always scan all 128 positions. Grapevine #3 stores pattern numbers after the position length, which are bigger than those are played
				return false;

			// Check sample information
			moduleStream.Seek(offset, SeekOrigin.Begin);

			uint samplesSize = 0;
			ushort temp;

			for (int i = 0; i < 31; i++)
			{
				temp = moduleStream.Read_B_UINT16();
				samplesSize += temp * 2U;

				if (temp == 0)
				{
					// Check loop length
					moduleStream.Seek(4, SeekOrigin.Current);

					if (moduleStream.Read_B_UINT16() != 0x0001)
						return false;
				}
				else
					moduleStream.Seek(6, SeekOrigin.Current);
			}

			// Check periods in the patterns
			ushort periodNum = 0;
			for (int i = 0; i < numberOfPatterns; i++)
			{
				moduleStream.Seek(offset + 0x17a + i * 1024, SeekOrigin.Begin);

				// Check first 16 notes in each pattern
				for (int j = 0; j < 16 * 4; j++)
				{
					temp = (ushort)(moduleStream.Read_B_UINT16() & 0x0fff);
					if (temp != 0)
					{
						byte hi = (byte)((temp & 0xff00) >> 8);
						byte lo = (byte)(temp & 0x00ff);
						bool found = false;

						for (int k = 0; k < 36; k++)
						{
							// Try to find the period in the period table
							if ((periods[k, 0] == hi) && (periods[k, 1] == lo))
							{
								found = true;
								break;
							}
						}

						if (!found)
							return false;

						periodNum++;
					}

					moduleStream.Seek(2, SeekOrigin.Current);
				}
			}

			if (periodNum == 0)
				return false;

			// Check the module length
			if ((0x17a + numberOfPatterns * 1024 + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
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
			startOffset = 0;
			if (moduleStream.Read_B_UINT32() == 0x54524b31)		// TRK1
				startOffset = 4;

			// Get number of positions and restart position
			moduleStream.Seek(startOffset + 0xf8, SeekOrigin.Begin);

			numberOfPositions = moduleStream.Read_UINT8();
			restartPosition = moduleStream.Read_UINT8();

			// We don't want to save unused patterns. See Grapevine #3 as example
			realNumberOfPatterns = numberOfPatterns;
			FindHighestPatternNumber(moduleStream, startOffset + 0xfa, numberOfPositions);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
			moduleStream.Seek(startOffset, SeekOrigin.Begin);

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

			moduleStream.Seek(startOffset + 0xfa, SeekOrigin.Begin);
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
			moduleStream.Seek(startOffset + 0x17a, SeekOrigin.Begin);
			if (moduleStream.Read_B_UINT32() != 0)
				moduleStream.Seek(-4, SeekOrigin.Current);

			byte[] pattern = new byte[1024];

			for (int i = 0; i < numberOfPatterns; i++)
			{
				moduleStream.ReadInto(pattern, 0, 1024);

				yield return pattern;
			}

			// If extra patterns are stored, skip them
			if (numberOfPatterns < realNumberOfPatterns)
				moduleStream.Seek((realNumberOfPatterns - numberOfPatterns) * 1024, SeekOrigin.Current);
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
