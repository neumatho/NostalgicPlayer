/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Agent.Player.SoundControl.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("5EA02FB3-C95D-4AA6-B584-10EECC08B62F")]

namespace Polycode.NostalgicPlayer.Agent.Player.SoundControl
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class SoundControl : AgentBase, IPlayerAgentMultipleFormatIdentify
	{
		private static readonly Guid agent1Id = Guid.Parse("7060D60E-56F0-4D86-9838-0FBD31782F60");
		private static readonly Guid agent2Id = Guid.Parse("07DFF8E8-B0E3-4D46-AAC0-426741EC3403");
		private static readonly Guid agent3Id = Guid.Parse("72376E9F-10B5-4922-8997-0DAB33D8EC73");

		private static readonly Dictionary<ModuleType, Guid> moduleTypeLookup = new Dictionary<ModuleType, Guid>
		{
			{ ModuleType.SoundControl3x, agent1Id },
			{ ModuleType.SoundControl40, agent2Id },
			{ ModuleType.SoundControl50, agent3Id }
		};

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_DESCRIPTION;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation =>
		[
			new AgentSupportInfo(Resources.IDS_SC_NAME_AGENT1, Resources.IDS_SC_DESCRIPTION_AGENT1, agent1Id),
			new AgentSupportInfo(Resources.IDS_SC_NAME_AGENT2, Resources.IDS_SC_DESCRIPTION_AGENT2, agent2Id),
			new AgentSupportInfo(Resources.IDS_SC_NAME_AGENT3, Resources.IDS_SC_DESCRIPTION_AGENT3, agent3Id)
		];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new SoundControlWorker();
		}
		#endregion

		#region IAgentMultipleFormatIdentify implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify all the formats that
		/// can be returned in IdentifyFormat()
		/// </summary>
		/********************************************************************/
		public string[] FileExtensions => SoundControlIdentifier.FileExtensions;



		/********************************************************************/
		/// <summary>
		/// Try to identify which format are used in the given stream and
		/// return the format Guid if found
		/// </summary>
		/********************************************************************/
		public IdentifyFormatInfo IdentifyFormat(PlayerFileInfo fileInfo)
		{
			ModuleType moduleType = SoundControlIdentifier.TestModule(fileInfo);
			if (moduleType == ModuleType.Unknown)
				return null;

			return new IdentifyFormatInfo(new SoundControlWorker(moduleType), moduleTypeLookup[moduleType]);
		}
		#endregion
	}
}
