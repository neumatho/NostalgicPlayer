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
	/// NoisePacker 2
	/// </summary>
	internal class NoisePacker2Format : NoisePacker1Format
	{
		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT16;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			return CheckForNoisePackerFormat(moduleStream) == 2;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
			moduleStream.Seek(8, SeekOrigin.Begin);

			for (int i = 0; i < numberOfSamples; i++)
			{
				moduleStream.Seek(4, SeekOrigin.Current);
				ushort length = moduleStream.Read_B_UINT16();
				byte fineTune = moduleStream.Read_UINT8();
				byte volume = moduleStream.Read_UINT8();
				moduleStream.Seek(4, SeekOrigin.Current);
				ushort loopLength = moduleStream.Read_B_UINT16();
				ushort loopStart = moduleStream.Read_B_UINT16();

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
		#endregion

		#region ChannelPlayerFormatBase implementation
		/********************************************************************/
		/// <summary>
		/// Check the pattern data
		/// </summary>
		/********************************************************************/
		protected override bool CheckPatternData(ModuleStream moduleStream, ushort sampleCount, ushort trackLength, ref int formatVersion)
		{
			for (int i = 0; i < trackLength; i += 3)
			{
				// Check note
				byte temp1 = moduleStream.Read_UINT8();
				if (temp1 > 0x49)
					return false;

				// Check sample number
				byte temp2 = moduleStream.Read_UINT8();
				if ((((temp1 << 4) & 0x10) | ((temp2 >> 4) & 0x0f)) > sampleCount)
					return false;

				// Check effect
				byte temp3 = moduleStream.Read_UINT8();
				if (((temp2 & 0x0f) == 0) && (temp3 != 0x00))
					return false;
			}

			return true;
		}
		#endregion
	}
}
