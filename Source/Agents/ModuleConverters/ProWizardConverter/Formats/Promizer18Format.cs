/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter.Formats
{
	/// <summary>
	/// Promizer 1.8a
	/// </summary>
	internal class Promizer18Format : Promizer1xFormatBase
	{
		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT25;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			return CheckForPromizerFormat(moduleStream) == 18;
		}
		#endregion

		#region Promizer1xFormatBase implementation
		/********************************************************************/
		/// <summary>
		/// Return needed offsets in the module
		/// </summary>
		/********************************************************************/
		protected override Offsets GetOffsets()
		{
			return new Offsets
			{
				Offset1 = 0x116c,
				Offset2 = 0x1170,
				Offset3 = 0x1168
			};
		}
		#endregion
	}
}
