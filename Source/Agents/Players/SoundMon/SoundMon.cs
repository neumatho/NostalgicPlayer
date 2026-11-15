/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Agent.Player.SoundMon.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("18DAA6F3-AD3B-4C3D-9F77-245467B989DE")]

namespace Polycode.NostalgicPlayer.Agent.Player.SoundMon
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class SoundMon : AgentBase, IPlayerAgentMultipleFormatIdentify
	{
		private static readonly Guid agent1Id = Guid.Parse("A88BED61-BA23-40FE-B910-433695FCA66B");
		private static readonly Guid agent2Id = Guid.Parse("90C4EC36-531B-46FF-9FD3-BC5745AD75BF");
		private static readonly Guid agent3Id = Guid.Parse("BF031743-9E4C-4BB2-8C26-19F23823B9C0");
		private static readonly Guid agent4Id = Guid.Parse("F47863B0-DB7E-4DAD-90E5-97EFF54F4455");

		private static readonly Dictionary<ModuleType, Guid> moduleTypeLookup = new Dictionary<ModuleType, Guid>
		{
			{ ModuleType.SoundMon10, agent1Id },
			{ ModuleType.SoundMon11, agent2Id },
			{ ModuleType.SoundMon21, agent3Id },
			{ ModuleType.SoundMon22, agent4Id }
		};

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_BP_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_BP_DESCRIPTION;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation =>
		[
			new AgentSupportInfo(Resources.IDS_BP_NAME_AGENT1, Resources.IDS_BP_DESCRIPTION_AGENT1, agent1Id),
			new AgentSupportInfo(Resources.IDS_BP_NAME_AGENT2, Resources.IDS_BP_DESCRIPTION_AGENT2, agent2Id),
			new AgentSupportInfo(Resources.IDS_BP_NAME_AGENT3, Resources.IDS_BP_DESCRIPTION_AGENT3, agent3Id),
			new AgentSupportInfo(Resources.IDS_BP_NAME_AGENT4, Resources.IDS_BP_DESCRIPTION_AGENT4, agent4Id)
		];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new SoundMonWorker();
		}
		#endregion

		#region IPlayerAgentMultipleFormatIdentify implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify all the formats that
		/// can be returned in IdentifyFormat()
		/// </summary>
		/********************************************************************/
		public string[] FileExtensions => SoundMonIdentifier.FileExtensions;



		/********************************************************************/
		/// <summary>
		/// Try to identify which format are used in the given stream and
		/// return the format Guid if found
		/// </summary>
		/********************************************************************/
		public IdentifyFormatInfo IdentifyFormat(PlayerFileInfo fileInfo)
		{
			ModuleType moduleType = SoundMonIdentifier.TestModule(fileInfo);
			if (moduleType == ModuleType.Unknown)
				return null;

			return new IdentifyFormatInfo(new SoundMonWorker(moduleType), moduleTypeLookup[moduleType]);
		}
		#endregion
	}
}
