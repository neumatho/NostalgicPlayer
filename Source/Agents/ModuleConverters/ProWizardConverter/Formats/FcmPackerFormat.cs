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
	/// FC-M Packer
	/// </summary>
	internal class FcmPackerFormat : ProWizardConverterWorker31SamplesBase
	{
		private const uint HeaderId = 0x46432d4d;			// FC-M
		private const uint NameHeaderId = 0x4e414d45;		// NAME
		private const uint InstHeaderId = 0x494e5354;		// INST
		private const uint LongHeaderId = 0x4c4f4e47;		// LONG
		private const uint PattHeaderId = 0x50415454;		// PATT
		private const uint SongHeaderId = 0x534f4e47;		// SONG
		private const uint SampHeaderId = 0x53414d50;		// SAMP

		private byte numberOfPositions;
		private byte restartPosition;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT8;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 34)
				return false;

			// Check the mark
			moduleStream.Seek(0, SeekOrigin.Begin);
			if (moduleStream.Read_B_UINT32() != HeaderId)
				return false;

			// Find sample information (just to check if its there)
			if (!FindPart(moduleStream, InstHeaderId))
				return false;

			// Find the position table size
			if (!FindPart(moduleStream, LongHeaderId))
				return false;

			byte positionListLength = moduleStream.Read_UINT8();

			// Find position list
			if (!FindPart(moduleStream, PattHeaderId))
				return false;

			// Find highest pattern number
			FindHighestPatternNumber(moduleStream, positionListLength);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Prepare conversion by initialize what is needed etc.
		/// </summary>
		/********************************************************************/
		protected override bool PrepareConversion(ModuleStream moduleStream)
		{
			// Get the position list length and restart position
			if (!FindPart(moduleStream, LongHeaderId))
				return false;

			// Read the information
			numberOfPositions = moduleStream.Read_UINT8();
			restartPosition = moduleStream.Read_UINT8();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name of the module if any
		/// </summary>
		/********************************************************************/
		protected override byte[] GetModuleName(ModuleStream moduleStream)
		{
			if (!FindPart(moduleStream, NameHeaderId))
				return null;

			byte[] moduleName = new byte[20];

			moduleStream.Read(moduleName, 0, 20);

			return moduleName;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
			if (!FindPart(moduleStream, InstHeaderId))
				yield break;

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
			if (!FindPart(moduleStream, PattHeaderId))
				return null;

			byte[] positionList = new byte[numberOfPositions];
			moduleStream.Read(positionList, 0, numberOfPositions);

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
			if (!FindPart(moduleStream, SongHeaderId))
				yield break;

			byte[] pattern = new byte[1024];

			for (int i = 0; i < numberOfPatterns; i++)
			{
				moduleStream.Read(pattern, 0, 1024);

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
			if (!FindPart(moduleStream, SampHeaderId))
				return false;

			return SaveMarksForAllSamples(moduleStream, converterStream);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will search the module after a specific chunk
		/// </summary>
		/********************************************************************/
		private bool FindPart(ModuleStream moduleStream, uint chunk)
		{
			byte[] buffer = new byte[1024];
			int bufferLeft = 0;

			int count = 0;
			int length = (int)moduleStream.Length - 3;

			moduleStream.Seek(0, SeekOrigin.Begin);

			while (count < length)
			{
				int bufferFilled = bufferLeft = moduleStream.Read(buffer, bufferLeft, buffer.Length - bufferLeft);
				if (bufferLeft == 0)
					return false;

				int bufferPosition = 0;

				while (bufferLeft >= 4)
				{
					uint checkChunk = (uint)((buffer[bufferPosition] << 24) | (buffer[bufferPosition + 1] << 16) | (buffer[bufferPosition + 2] << 8) | buffer[bufferPosition + 3]);
					if (checkChunk == chunk)
					{
						// Found it, make sure the stream are positioned right after the chunk
						moduleStream.Seek(count + bufferPosition + 4, SeekOrigin.Begin);

						return true;
					}

					bufferPosition += 2;
					bufferLeft -= 2;
				}

				Array.Copy(buffer, bufferPosition, buffer, 0, bufferLeft);
				count += bufferFilled - bufferLeft;
			}

			return false;
		}
		#endregion
	}
}
