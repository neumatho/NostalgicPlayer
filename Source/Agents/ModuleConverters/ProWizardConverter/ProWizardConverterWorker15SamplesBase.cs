/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter
{
	/// <summary>
	/// Base class for all the formats
	/// </summary>
	internal abstract class ProWizardConverterWorker15SamplesBase : ProWizardConverterWorkerBase
	{
		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			if (CheckModule(moduleStream))
				return AgentResult.Ok;

			return AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Return the size of the converted module without samples if
		/// possible. 0 means unknown
		/// </summary>
		/********************************************************************/
		public override int ConvertedModuleLength(PlayerFileInfo fileInfo)
		{
			if (numberOfPatterns == 0)
				return 0;

			return 600 + numberOfPatterns * 1024;
		}



		/********************************************************************/
		/// <summary>
		/// Convert the module and store the result in the stream given
		/// </summary>
		/********************************************************************/
		public override AgentResult Convert(PlayerFileInfo fileInfo, ConverterStream converterStream, out string errorMessage)
		{
			byte[] zeroBuf = new byte[128];

			ModuleStream moduleStream = fileInfo.ModuleStream;

			// First do any preparations
			sampleLengths = new uint[15];

			if (!PrepareConversion(moduleStream))
			{
				errorMessage = Resources.IDS_ERR_LOADING_HEADERINFO;
				return AgentResult.Error;
			}

			// Write module name
			byte[] name = GetModuleName(moduleStream);
			converterStream.Write(name ?? zeroBuf, 0, 20);

			// Write sample information
			int sampleCount = 0;

			foreach (SampleInfo sampleInfo in GetSamples(moduleStream))
			{
				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_ERR_LOADING_SAMPLEINFO;
					return AgentResult.Error;
				}

				converterStream.Write(sampleInfo.Name ?? zeroBuf, 0, 22);
				converterStream.Write_B_UINT16(sampleInfo.Length);
				converterStream.Write_B_UINT16(sampleInfo.Volume);
				converterStream.Write_B_UINT16(sampleInfo.LoopStart);
				converterStream.Write_B_UINT16(sampleInfo.LoopLength);

				sampleLengths[sampleCount++] = (uint)sampleInfo.Length * 2;
			}

			// If no samples has been returned, something is wrong
			if (sampleCount == 0)
			{
				errorMessage = Resources.IDS_ERR_LOADING_SAMPLEINFO;
				return AgentResult.Error;
			}

			// Write empty samples for the rest
			for (int i = sampleCount; i < 15; i++)
			{
				// Write sample name + size + fine tune + volume + repeat point
				converterStream.Write(zeroBuf, 0, 28);

				// Write repeat length
				converterStream.Write_B_UINT16(1);
			}

			// Write song length
			Span<byte> positionList = GetPositionList(moduleStream);
			if ((positionList == null) || (positionList.Length == 0) || moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_ERR_LOADING_HEADERINFO;
				return AgentResult.Error;
			}

			converterStream.Write_UINT8((byte)positionList.Length);

			// Write restart position
			converterStream.Write_UINT8(GetRestartPosition(moduleStream));

			// Write position list
			converterStream.Write(positionList);
			if (positionList.Length < 128)
				converterStream.Write(zeroBuf, 0, 128 - positionList.Length);

			// Write the patterns
			foreach (byte[] patternData in GetPatterns(moduleStream))
			{
				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_ERR_LOADING_PATTERNS;
					return AgentResult.Error;
				}

				converterStream.Write(patternData);
			}

			// At last, write the sample data
			if (!WriteSampleData(moduleStream, converterStream))
			{
				errorMessage = Resources.IDS_ERR_LOADING_SAMPLES;
				return AgentResult.Error;
			}

			errorMessage = string.Empty;

			return AgentResult.Ok;
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Return the restart position
		/// </summary>
		/********************************************************************/
		protected override byte GetRestartPosition(ModuleStream moduleStream)
		{
			return 0x00;
		}
		#endregion
	}
}
