/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter.Formats;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("8574CAD5-2892-499F-8B26-50D16EA70C09")]

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class ModuleConverter : AgentBase
	{
		private static readonly Guid agent1Id = Guid.Parse("99F6809B-0FA7-4814-895E-A5A4632EFE96");
		private static readonly Guid agent2Id = Guid.Parse("0C8D0CEE-EA9D-4132-95ED-DFE72D5D8FB6");
		private static readonly Guid agent3Id = Guid.Parse("44F2292A-BBA1-42C7-B9EC-1ACED9A42BF8");
		private static readonly Guid agent4Id = Guid.Parse("CED4726A-EF3B-40C6-8F93-865F0CE5321E");
		private static readonly Guid agent5Id = Guid.Parse("E03F718C-8FA9-4843-9EBE-6D69EC3A421D");
		private static readonly Guid agent6aId = Guid.Parse("D6F10415-FCC6-4CDF-8756-EA786786711B");
		private static readonly Guid agent6bId = Guid.Parse("6221A135-3F32-4499-AB4B-32BFAB93F0EF");
		private static readonly Guid agent7Id = Guid.Parse("83A34BAD-CA67-4123-845D-9D749AA2EB9B");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_MODCONV_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_MODCONV_DESCRIPTION;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation =>
		[
			new AgentSupportInfo(Resources.IDS_MODCONV_NAME_AGENT1, Resources.IDS_MODCONV_DESCRIPTION_AGENT1, agent1Id),
			new AgentSupportInfo(Resources.IDS_MODCONV_NAME_AGENT2, Resources.IDS_MODCONV_DESCRIPTION_AGENT2, agent2Id),
			new AgentSupportInfo(Resources.IDS_MODCONV_NAME_AGENT3, Resources.IDS_MODCONV_DESCRIPTION_AGENT3, agent3Id),
			new AgentSupportInfo(Resources.IDS_MODCONV_NAME_AGENT4, Resources.IDS_MODCONV_DESCRIPTION_AGENT4, agent4Id),
			new AgentSupportInfo(Resources.IDS_MODCONV_NAME_AGENT5, Resources.IDS_MODCONV_DESCRIPTION_AGENT5, agent5Id),
			new AgentSupportInfo(Resources.IDS_MODCONV_NAME_AGENT6a, Resources.IDS_MODCONV_DESCRIPTION_AGENT6a, agent6aId),
			new AgentSupportInfo(Resources.IDS_MODCONV_NAME_AGENT6b, Resources.IDS_MODCONV_DESCRIPTION_AGENT6b, agent6bId),
			new AgentSupportInfo(Resources.IDS_MODCONV_NAME_AGENT7, Resources.IDS_MODCONV_DESCRIPTION_AGENT7, agent7Id)
		];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			switch (typeId)
			{
				case var id when id == agent1Id:
					return new FutureComposer13Format();

				case var id when id == agent2Id:
					return new SoundFx1xFormat();

				case var id when id == agent3Id:
					return new FredEditorFinalFormat();

				case var id when id == agent4Id:
					return new Med4Format();

				case var id when id == agent5Id:
					return new UmxFormat();

				case var id when id == agent6aId:
					return new Sc68Format();

				case var id when id == agent6bId:
					return new Sc68FormatArchive(Resources.IDS_MODCONV_NAME_AGENT6b);

				case var id when id == agent7Id:
					return new SonicArrangerFinalFormat();
			}

			return null;
		}
		#endregion
	}
}
