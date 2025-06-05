/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter.Formats
{
	/// <summary>
	/// The Player 4.0A
	/// </summary>
	internal class ThePlayer41AFormat : ThePlayer4xFormatBase
	{
		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT38;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			return CheckForThePlayerFormat(moduleStream, "P41A");
		}
		#endregion

		#region ThePlayer4xFormatBase implementation
		/********************************************************************/
		/// <summary>
		/// Check the sample information and return the total size of all
		/// the samples
		/// </summary>
		/********************************************************************/
		protected override uint CheckSampleInfo(ModuleStream moduleStream, byte sampleCount, out uint firstAddress, out ushort lastLength)
		{
			int lastSampleStart = -1;
			uint samplesSize = 0;

			firstAddress = 0;
			lastLength = 0;

			moduleStream.Seek(20, SeekOrigin.Begin);

			for (int i = 0; i < sampleCount; i++)
			{
				int sampleStart = moduleStream.Read_B_INT32();
				ushort sampleLength = moduleStream.Read_B_UINT16();
				moduleStream.Seek(6, SeekOrigin.Current);
				ushort volume = moduleStream.Read_B_UINT16();
				ushort fineTune = moduleStream.Read_B_UINT16();

				if (i == 0)
					firstAddress = (uint)sampleStart;

				lastLength = sampleLength;

				// Check fine tune
				if ((fineTune % 74) != 0)
					return 0;

				// Is fine tune > 0xf?
				if (fineTune > 0x456)
					continue;

				// Check volume
				if (volume > 0x40)
					return 0;

				// Check sample length
				if (sampleLength >= 0x8000)
					return 0;

				// Check sample start
				if (sampleStart > lastSampleStart)
				{
					lastSampleStart = sampleStart;
					samplesSize += sampleLength * 2U;
				}
			}

			return samplesSize;
		}



		/********************************************************************/
		/// <summary>
		/// Read the next sample information and return it
		/// </summary>
		/********************************************************************/
		protected override SampleInfo ReadSampleInfo(ModuleStream moduleStream, out uint startOffset)
		{
			startOffset = moduleStream.Read_B_UINT32();
			ushort length = moduleStream.Read_B_UINT16();
			uint loopStart = moduleStream.Read_B_UINT32();
			ushort loopLength = moduleStream.Read_B_UINT16();
			ushort volume = moduleStream.Read_B_UINT16();
			ushort fineTune = moduleStream.Read_B_UINT16();

			// Is fine tune > 0xf?
			if (fineTune > 0x456)
			{
				return new SampleInfo
				{
					Name = null,
					Length = 0,
					LoopStart = 0,
					LoopLength = 1,
					Volume = 0,
					FineTune = 0
				};
			}

			return new SampleInfo
			{
				Name = null,
				Length = length,
				LoopStart = (ushort)((loopStart - startOffset) / 2),
				LoopLength = loopLength,
				Volume = (byte)volume,
				FineTune = (byte)(fineTune / 74)
			};
		}
		#endregion
	}
}
