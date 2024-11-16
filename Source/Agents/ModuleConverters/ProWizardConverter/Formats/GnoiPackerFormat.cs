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
	/// GnoiPacker
	/// </summary>
	internal class GnoiPackerFormat : ProWizardForPcBase
	{
		private byte numberOfPositions;
		private byte restartPosition;
		private byte numberOfSamples;
		private bool hasPositionPadding;

		private ushort realNumberOfPatterns;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT53;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 28)
				return false;

			// Check some counts
			moduleStream.Seek(22, SeekOrigin.Begin);

			byte sampleCount = moduleStream.Read_UINT8();
			if ((sampleCount == 0) || (sampleCount > 31))
				return false;

			numberOfPatterns = moduleStream.Read_UINT8();
			if ((numberOfPatterns == 0) || (numberOfPatterns > 64))
				return false;

			// Check the mark
			if (moduleStream.Read_B_UINT32() != 0x2d47442d)		// -GD-
				return false;

			// Check sample information
			for (int i = 0; i < sampleCount; i++)
			{
				ushort sampleSize = (ushort)(moduleStream.Read_B_UINT16() * 2);
				byte volume = (byte)moduleStream.Read_B_UINT16();
				ushort loopStart = (ushort)(moduleStream.Read_B_UINT16() * 2);
				ushort loopSize = (ushort)(moduleStream.Read_B_UINT16() * 2);

				if (!TestSample(sampleSize, loopStart, loopSize, volume, 0))
					return false;
			}

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
			moduleStream.Seek(20, SeekOrigin.Begin);

			numberOfPositions = moduleStream.Read_UINT8();
			restartPosition = moduleStream.Read_UINT8();
			numberOfSamples = moduleStream.Read_UINT8();

			hasPositionPadding = (numberOfPositions & 0x80) != 0;
			numberOfPositions &= 0x7f;

			if (hasPositionPadding)
				numberOfPositions--;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name of the module if any
		/// </summary>
		/********************************************************************/
		protected override byte[] GetModuleName(ModuleStream moduleStream)
		{
			byte[] moduleName = new byte[20];

			moduleStream.Seek(0, SeekOrigin.Begin);
			moduleStream.ReadInto(moduleName, 0, 20);

			return moduleName;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
			moduleStream.Seek(28, SeekOrigin.Begin);

			for (int i = 0; i < numberOfSamples; i++)
			{
				ushort length = moduleStream.Read_B_UINT16();
				byte volume = (byte)moduleStream.Read_B_UINT16();
				ushort loopStart = moduleStream.Read_B_UINT16();
				ushort loopLength = moduleStream.Read_B_UINT16();

				yield return new SampleInfo
				{
					Name = null,
					Length = length,
					LoopStart = loopStart,
					LoopLength = loopLength,
					Volume = volume,
					FineTune = 0
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

			moduleStream.ReadInto(positionList, 0, numberOfPositions);

			if (hasPositionPadding)
				moduleStream.Seek(1, SeekOrigin.Current);

			realNumberOfPatterns = numberOfPatterns;
			FindHighestPatternNumber(positionList);

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

			for (int i = 0; i < numberOfPatterns; i++)
			{
				for (int j = 0; j < 64 * 4; j++)
				{
					byte byt1 = moduleStream.Read_UINT8();
					byte byt2 = moduleStream.Read_UINT8();
					byte byt3 = moduleStream.Read_UINT8();

					pattern[j * 4] = (byte)((byt1 >> 3) & 0x10);
					pattern[j * 4 + 2] = byt2;
					pattern[j * 4 + 3] = byt3;

					byt1 &= 0x7f;
					if (byt1 != 0)
					{
						byt1--;

						pattern[j * 4] |= periods[byt1, 0];
						pattern[j * 4 + 1] = periods[byt1, 1];
					}
					else
						pattern[j * 4 + 1] = 0x00;
				}

				yield return pattern;
			}

			moduleStream.Seek((realNumberOfPatterns - numberOfPatterns) * 768, SeekOrigin.Current);
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
