/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Agent.Player.Tfmx.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("C5673A24-1C37-49AB-B919-6EAFA9654EF8")]

namespace Polycode.NostalgicPlayer.Agent.Player.Tfmx
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class Tfmx : AgentBase, IPlayerAgentMultipleFormatIdentify
	{
		private static readonly Guid agent1Id = Guid.Parse("E41B630B-0C68-41D5-9D09-2771581A0D22");
		private static readonly Guid agent2Id = Guid.Parse("E9333E11-4CD4-4631-B758-507C4607AB8A");
		private static readonly Guid agent3Id = Guid.Parse("AFB99395-AA4B-4F35-BA5E-4B1513615B51");

		private static readonly Dictionary<ModuleType, Guid> moduleTypeLookup = new Dictionary<ModuleType, Guid>
		{
			{ ModuleType.Tfmx15, agent1Id },
			{ ModuleType.TfmxPro, agent2Id },
			{ ModuleType.Tfmx7V, agent3Id }
		};

		internal static readonly string[] fileExtensions = [ "tfx", "mdat", "tfm" ];

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_TFMX_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_TFMX_DESCRIPTION;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation =>
		[
			new AgentSupportInfo(Resources.IDS_TFMX_NAME_AGENT1, Resources.IDS_TFMX_DESCRIPTION_AGENT1, agent1Id),
			new AgentSupportInfo(Resources.IDS_TFMX_NAME_AGENT2, Resources.IDS_TFMX_DESCRIPTION_AGENT2, agent2Id),
			new AgentSupportInfo(Resources.IDS_TFMX_NAME_AGENT3, Resources.IDS_TFMX_DESCRIPTION_AGENT3, agent3Id)
		];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new TfmxWorker();
		}
		#endregion

		#region IAgentMultipleFormatIdentify implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify all the formats that
		/// can be returned in IdentifyFormat()
		/// </summary>
		/********************************************************************/
		public string[] FileExtensions => fileExtensions;



		/********************************************************************/
		/// <summary>
		/// Try to identify which format are used in the given stream and
		/// return the format Guid if found
		/// </summary>
		/********************************************************************/
		public IdentifyFormatInfo IdentifyFormat(PlayerFileInfo fileInfo)
		{
			ModuleType moduleType = TfmxWorker.TestModule(fileInfo);
			if (moduleType == ModuleType.Unknown)
				return null;

			return new IdentifyFormatInfo(new TfmxWorker(moduleType), moduleTypeLookup[moduleType]);
		}
		#endregion
	}
}
