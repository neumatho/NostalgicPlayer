/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter.Formats
{
	/// <summary>
	/// Images Music System
	///
	/// Based on the loader from LibXmp
	/// </summary>
	internal class MagneticFieldsPackerFormat : ProWizardConverterWorker31SamplesBase
	{
		private byte numberOfPositions;
		private byte restartPosition;

		private byte[] positionList;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT71;
		#endregion


		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 384)
				return false;

			// Check restart byte
			moduleStream.Seek(249, SeekOrigin.Begin);

			if (moduleStream.Read_UINT8() != 0x7f)
				return false;

			// Check sample information
			moduleStream.Seek(0, SeekOrigin.Begin);

			for (int i = 0; i < 31; i++)
			{
				// Check size
				ushort len = moduleStream.Read_B_UINT16();
				if (len > 0x7fff)
					return false;

				// Check fine tune
				if ((moduleStream.Read_UINT8() & 0xf0) != 0)
					return false;

				// Check volume
				if (moduleStream.Read_UINT8() > 64)
					return false;

				// Check loop start
				ushort loopStart = moduleStream.Read_B_UINT16();
				if (loopStart > len)
					return false;

				// Check loop length
				ushort loopLength = moduleStream.Read_B_UINT16();
				if ((loopStart + loopLength - 1) > len)
					return false;

				if ((len > 0) && (loopLength == 0))
					return false;
			}

			byte positionLength = moduleStream.Read_UINT8();

			moduleStream.Seek(378, SeekOrigin.Begin);

			ushort temp = moduleStream.Read_B_UINT16();
			if (positionLength != temp)
				return false;

			if (temp != moduleStream.Read_B_UINT16())
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

			moduleStream.Seek(0, SeekOrigin.Begin);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
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
			positionList = new byte[numberOfPositions];

			moduleStream.Seek(0xfa, SeekOrigin.Begin);
			moduleStream.ReadInto(positionList, 0, numberOfPositions);

			moduleStream.Seek(128 - numberOfPositions, SeekOrigin.Current);

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
			ushort size1 = moduleStream.Read_B_UINT16();
			moduleStream.Seek(2, SeekOrigin.Current);

			ushort[,] patternTable = new ushort[4, size1];

			for (int i = 0; i < size1; i++)
			{
				for (int j = 0; j < 4; j++)
					patternTable[j, i] = moduleStream.Read_B_UINT16();
			}

			byte[] inputBuffer = new byte[moduleStream.Length - moduleStream.Position];
			moduleStream.ReadInto(inputBuffer, 0, inputBuffer.Length);

			// Convert all the patterns
			Dictionary<int, byte[]> convertedPatterns = new Dictionary<int, byte[]>();

			for (int i = 0; i < numberOfPositions; i++)
			{
				int patternNumber = positionList[i];

				if (!convertedPatterns.TryGetValue(patternNumber, out byte[] outputBuffer))
				{
					outputBuffer = ConvertPattern(inputBuffer, [ patternTable[0, i], patternTable[1, i], patternTable[2, i], patternTable[3, i] ]);
					if (outputBuffer == null)
						yield return null;

					convertedPatterns[patternNumber] = outputBuffer;
				}
			}

			// And return them
			for (int i = 0; i < numberOfPatterns; i++)
			{
				if (convertedPatterns.TryGetValue(i, out byte[] outputBuffer))
					yield return outputBuffer;
				else
					yield return new byte[1024];    // If pattern is not used, return an empty pattern
			}
		}



		/********************************************************************/
		/// <summary>
		/// Write all the samples
		/// </summary>
		/********************************************************************/
		protected override bool WriteSampleData(PlayerFileInfo fileInfo, ModuleStream moduleStream, ConverterStream converterStream)
		{
			ModuleStream sampleStream = fileInfo.Loader.OpenExtraFileByExtension("smp");
			if (sampleStream == null)
			{
				// Try with a sample set, as used in Kid Chaos and Kid Vicious
				foreach (string fileName in fileInfo.Loader.GetPossibleFileNames("smp"))
				{
					string newFileName = fileName;

					int index = newFileName.LastIndexOf('-');
					if (index != -1)
					{
						int index2 = newFileName.IndexOf('.', index + 1);

						if (index2 != -1)
							newFileName = newFileName.Substring(0, index).TrimEnd() + newFileName.Substring(index2);
						else
							newFileName = newFileName.Substring(0, index).TrimEnd();
					}

					newFileName += ".set";

					sampleStream = fileInfo.Loader.OpenExtraFileByFileName(newFileName, true);
					if (sampleStream != null)
						break;
				}

				if (sampleStream == null)
				{
					// No sample file found
					return false;
				}
			}

			try
			{
				for (int i = 0; i < sampleLengths.Length; i++)
				{
					int length = (int)sampleLengths[i];
					if (length != 0)
					{
						// Check to see if we miss too much from the last sample
						if (sampleStream.Length - sampleStream.Position < (length - MaxNumberOfMissingBytes))
							return false;

						Helpers.CopyData(sampleStream, converterStream, length);
					}
				}
			}
			finally
			{
				sampleStream.Dispose();
			}

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Convert a single pattern
		/// </summary>
		/********************************************************************/
		private byte[] ConvertPattern(byte[] inputBuffer, ushort[] patternStartOffsets)
		{
			byte[] outputBuffer = new byte[1024];

			for (int j = 0; j < 4; j++)
			{
				int startInputOffset = patternStartOffsets[j];
				if (startInputOffset >= (inputBuffer.Length - 4))
					return null;

				for (int row = 0, k = 0; k < 4; k++)
				{
					for (int x = 0; x < 4; x++)
					{
						for (int y = 0; y < 4; y++, row++)
						{
							int inputOffset = startInputOffset + inputBuffer[startInputOffset + k] + x;
							if (inputOffset >= inputBuffer.Length)
								return null;

							inputOffset = startInputOffset + inputBuffer[inputOffset] + y;
							if (inputOffset >= inputBuffer.Length)
								return null;

							inputOffset = startInputOffset + inputBuffer[inputOffset] * 2;
							if (inputOffset >= inputBuffer.Length)
								return null;

							int outputOffset = row * 16 + j * 4;

							outputBuffer[outputOffset++] = inputBuffer[inputOffset++];
							outputBuffer[outputOffset++] = inputBuffer[inputOffset++];
							outputBuffer[outputOffset++] = inputBuffer[inputOffset++];
							outputBuffer[outputOffset] = inputBuffer[inputOffset];
						}
					}
				}
			}

			return outputBuffer;
		}
		#endregion
	}
}
