/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter.Formats
{
	/// <summary>
	/// The Player 3.0A
	/// </summary>
	internal class ThePlayer30AFormat : ThePlayer2x_3xFormatBase
	{
		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT67;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			return CheckForThePlayerFormat(moduleStream, 0x50333041);		// P30A
		}
		#endregion

		#region ThePlayer2x_3xFormatBase implementation
		/********************************************************************/
		/// <summary>
		/// Convert a single row on a single channel and writes it in the
		/// pattern
		/// </summary>
		/********************************************************************/
		protected override void ConvertPatternData(ModuleStream moduleStream, byte c1, byte c2, byte c3, byte c4, int channel, byte[] pattern, int[] voiceRowIndex)
		{
			if (c1 != 0x80)
			{
				var patternData = ConvertData(c1, c2, c3);
				StoreData(patternData.byt1, patternData.byt2, patternData.byt3, patternData.byt4, channel, voiceRowIndex[channel], pattern);
			}
			else
			{
				var patternData = ConvertData((byte)(c1 & 0x7f), c2, c3);

				for (int c = 0, row = voiceRowIndex[channel]; c <= c4; c++, row++)
					StoreData(patternData.byt1, patternData.byt2, patternData.byt3, patternData.byt4, channel, row, pattern);
			}

			if ((c4 > 0x00) && (c4 < 0x80))
				voiceRowIndex[channel] += c4;

			voiceRowIndex[channel]++;
		}



		/********************************************************************/
		/// <summary>
		/// Convert the effect 5, 6 and A value
		/// pattern
		/// </summary>
		/********************************************************************/
		protected override byte ConvertEffect56AValue(byte c3)
		{
			return (byte)(c3 > 0x7f ? (c3 << 4) & 0xf0 : c3);
		}
		#endregion
	}
}
