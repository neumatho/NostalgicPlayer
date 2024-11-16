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
	/// Base class for Unic Tracker and Laxity Tracker
	/// </summary>
	internal abstract class UnicTrackerFormatBase : ProWizardConverterWorker31SamplesBase
	{
		private byte restartPosition;
		private ushort realNumberOfPatterns;

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the position list. Note that the returned list must be the
		/// same length as to be played
		/// </summary>
		/********************************************************************/
		protected override Span<byte> GetPositionList(ModuleStream moduleStream)
		{
			byte numberOfPositions = moduleStream.Read_UINT8();

			restartPosition = moduleStream.Read_UINT8();
			if (restartPosition == 0x00)
				restartPosition = 0x7f;

			byte[] positionList = new byte[numberOfPositions];
			moduleStream.ReadInto(positionList, 0, numberOfPositions);

			moduleStream.Seek(128 - numberOfPositions, SeekOrigin.Current);

			// We don't want to save unused patterns
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
					byte byt1;

					// Get sample number
					byte temp = moduleStream.Read_UINT8();

					if ((temp & 0x40) != 0)
						byt1 = 0x10;
					else
						byt1 = 0x00;

					// Low 4 bit of sample number, effect + effect value
					byte byt3 = moduleStream.Read_UINT8();
					byte byt4 = moduleStream.Read_UINT8();

					// Get note
					byte byt2 = (byte)(temp & 0x3f);
					if ((byt2 != 0x00) && (byt2 != 0x3f))
					{
						byt2--;
						byt1 |= periods[byt2, 0];
						byt2 = periods[byt2, 1];
					}
					else
						byt2 = 0x00;

					// Convert the pattern break effect argument from hex to decimal
					if ((byt3 & 0x0f) == 0x0d)
						byt4 = (byte)((byt4 / 10) * 0x10 + (byt4 % 10));

					// Beast-Busters.unic uses 800 for sub-song stop marker.
					// Convert this to F00 instead
					if (((byt3 & 0x0f) == 0x08) && (byt4 == 0x00))
						byt3 = (byte)((byt3 & 0xf0) | 0x0f);

					// Copy the pattern data
					pattern[j * 4] = byt1;
					pattern[j * 4 + 1] = byt2;
					pattern[j * 4 + 2] = byt3;
					pattern[j * 4 + 3] = byt4;
				}

				yield return pattern;
			}

			// If extra patterns are stored, skip them
			if (numberOfPatterns < realNumberOfPatterns)
				moduleStream.Seek((realNumberOfPatterns - numberOfPatterns) * 0x300, SeekOrigin.Current);
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

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Will check the patterns to see if they are in ProTracker format
		/// </summary>
		/********************************************************************/
		protected bool CheckPatterns(ModuleStream moduleStream)
		{
			bool gotNote = false;

			for (int i = 0; i < numberOfPatterns * 128; i++)
			{
				// Get the note
				uint temp = moduleStream.Read_B_UINT32();
				if (temp != 0)
				{
					gotNote = true;
					temp &= 0x0fff0000;

					if (temp != 0)
					{
						if ((temp < 0x00710000) || (temp > 0x03580000))
							return false;
					}
				}
			}

			return gotNote;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected IEnumerable<SampleInfo> GetAllSamples(ModuleStream moduleStream, bool setFineTune)
		{
			for (int i = 0; i < 31; i++)
			{
				byte[] name = new byte[22];
				moduleStream.ReadInto(name, 0, 20);

				byte fineTune = 0x00;

				if (setFineTune)
					fineTune = (byte)((-(sbyte)moduleStream.Read_B_UINT16()) & 0x0f);
				else
					moduleStream.Seek(2, SeekOrigin.Current);

				ushort length = moduleStream.Read_B_UINT16();
				byte volume = (byte)moduleStream.Read_B_UINT16();
				ushort loopStart = moduleStream.Read_B_UINT16();
				ushort loopLength = moduleStream.Read_B_UINT16();

				if (((loopStart * 2) + loopLength) <= length)
					loopStart *= 2;

				yield return new SampleInfo
				{
					Name = name,
					Length = length,
					LoopStart = loopStart,
					LoopLength = loopLength,
					Volume = volume,
					FineTune = fineTune
				};
			}
		}
		#endregion
	}
}
