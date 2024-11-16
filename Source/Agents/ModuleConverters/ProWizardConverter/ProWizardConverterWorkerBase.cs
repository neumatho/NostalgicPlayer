/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter
{
	/// <summary>
	/// Base class for all the formats
	/// </summary>
	internal abstract class ProWizardConverterWorkerBase : ModuleConverterAgentBase
	{
		protected class SampleInfo
		{
			public byte[] Name;
			public ushort Length;
			public ushort LoopStart;
			public ushort LoopLength;
			public byte Volume;
			public byte FineTune;
		}

		#region Periods
		protected static readonly byte[,] periods = new byte[45, 2]
		{
			{ 0x3, 0x58 }, { 0x3, 0x28 }, { 0x2, 0xfa }, { 0x2, 0xd0 }, { 0x2, 0xa6 }, { 0x2, 0x80 }, { 0x2, 0x5c }, { 0x2, 0x3a }, { 0x2, 0x1a },
			{ 0x1, 0xfc }, { 0x1, 0xe0 }, { 0x1, 0xc5 }, { 0x1, 0xac }, { 0x1, 0x94 }, { 0x1, 0x7d }, { 0x1, 0x68 }, { 0x1, 0x53 }, { 0x1, 0x40 },
			{ 0x1, 0x2e }, { 0x1, 0x1d }, { 0x1, 0x0d }, { 0x0, 0xfe }, { 0x0, 0xf0 }, { 0x0, 0xe2 }, { 0x0, 0xd6 }, { 0x0, 0xca }, { 0x0, 0xbe },
			{ 0x0, 0xb4 }, { 0x0, 0xaa }, { 0x0, 0xa0 }, { 0x0, 0x97 }, { 0x0, 0x8f }, { 0x0, 0x87 }, { 0x0, 0x7f }, { 0x0, 0x78 }, { 0x0, 0x71 },
			{ 0x0, 0x71 }, { 0x0, 0x71 }, { 0x0, 0x71 }, { 0x0, 0x71 }, { 0x0, 0x71 }, { 0x0, 0x71 }, { 0x0, 0x71 }, { 0x0, 0x71 }, { 0x0, 0x71 }
		};
		#endregion

		protected const int MaxNumberOfMissingBytes = 256;

		protected uint[] sampleLengths;
		protected ushort numberOfPatterns;

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected abstract bool CheckModule(ModuleStream moduleStream);



		/********************************************************************/
		/// <summary>
		/// Prepare conversion by initialize what is needed etc.
		/// </summary>
		/********************************************************************/
		protected virtual bool PrepareConversion(ModuleStream moduleStream)
		{
			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name of the module if any
		/// </summary>
		/********************************************************************/
		protected virtual byte[] GetModuleName(ModuleStream moduleStream)
		{
			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected abstract IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream);



		/********************************************************************/
		/// <summary>
		/// Return the position list. Note that the returned list must be the
		/// same length as to be played
		/// </summary>
		/********************************************************************/
		protected abstract Span<byte> GetPositionList(ModuleStream moduleStream);



		/********************************************************************/
		/// <summary>
		/// Return the restart position
		/// </summary>
		/********************************************************************/
		protected abstract byte GetRestartPosition(ModuleStream moduleStream);



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected abstract IEnumerable<byte[]> GetPatterns(ModuleStream moduleStream);



		/********************************************************************/
		/// <summary>
		/// Write all the samples
		/// </summary>
		/********************************************************************/
		protected abstract bool WriteSampleData(ModuleStream moduleStream, ConverterStream converterStream);
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Find the highest pattern number by looking in the position list
		/// at the offset given
		/// </summary>
		/********************************************************************/
		protected ushort FindHighestPatternNumber(ModuleStream moduleStream, int positionListOffset, int positionListLength)
		{
			moduleStream.Seek(positionListOffset, SeekOrigin.Begin);

			return FindHighestPatternNumber(moduleStream, positionListLength);
		}



		/********************************************************************/
		/// <summary>
		/// Find the highest pattern number by looking in the position list
		/// at the offset given
		/// </summary>
		/********************************************************************/
		protected ushort FindHighestPatternNumber(ModuleStream moduleStream, int positionListLength)
		{
			byte[] positionList = new byte[positionListLength];
			moduleStream.ReadInto(positionList, 0, positionListLength);

			return FindHighestPatternNumber(positionList);
		}



		/********************************************************************/
		/// <summary>
		/// Find the highest pattern number by looking in the position list
		/// at the offset given
		/// </summary>
		/********************************************************************/
		protected ushort FindHighestPatternNumber(Span<byte> positionList)
		{
			numberOfPatterns = 0;
			foreach (byte patternNumber in positionList)
			{
				if (patternNumber > numberOfPatterns)
					numberOfPatterns = patternNumber;
			}

			numberOfPatterns++;

			return numberOfPatterns;
		}



		/********************************************************************/
		/// <summary>
		/// Create position list and find the number of patterns needed
		/// </summary>
		/********************************************************************/
		protected byte[] BuildPositionList<T>(int numberOfPositions, Func<int, T> getNumber) where T : struct
		{
			return BuildPositionList(numberOfPositions, out _, getNumber);
		}



		/********************************************************************/
		/// <summary>
		/// Create position list and find the number of patterns needed
		/// </summary>
		/********************************************************************/
		protected byte[] BuildPositionList<T>(int numberOfPositions, out List<T> usedNumbers, Func<int, T> getNumber) where T : struct
		{
			byte[] positionList = new byte[numberOfPositions];

			// Begin to create the position table
			usedNumbers = new List<T>();
			numberOfPatterns = 0;

			for (int i = 0; i < numberOfPositions; i++)
			{
				T nextNumber = getNumber(i);

				// Check to see if this track combination has already been used previously
				bool found = false;

				for (int j = 0; j < usedNumbers.Count; j++)
				{
					if (usedNumbers[j].Equals(nextNumber))
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
					usedNumbers.Add(nextNumber);
				}
			}

			return positionList;
		}



		/********************************************************************/
		/// <summary>
		/// Will save marks for all the samples. Note that the position of
		/// the stream has to be on the first sample
		/// </summary>
		/********************************************************************/
		protected bool SaveMarksForAllSamples(ModuleStream moduleStream, ConverterStream converterStream)
		{
			for (int i = 0; i < sampleLengths.Length; i++)
			{
				int length = (int)sampleLengths[i];
				if (length != 0)
				{
					// Check to see if we miss too much from the last sample
					if (moduleStream.Length - moduleStream.Position < (length - MaxNumberOfMissingBytes))
						return false;

					moduleStream.SetSampleDataInfo(i, length);
					converterStream.WriteSampleDataMarker(i, length);
				}
			}

			return true;
		}
		#endregion
	}
}
