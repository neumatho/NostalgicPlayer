/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("465B504C-44CE-4ECE-BD75-A36F80FB7BBF")]

namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class OctaMed : AgentBase, IPlayerAgentMultipleFormatIdentify
	{
		private static readonly Guid agent1Id = Guid.Parse("0DCA54F5-C418-47CD-8FCB-1A3CAEA1DD94");
		private static readonly Guid agent2Id = Guid.Parse("669C0F1F-C2D0-41F2-9005-6BD64D02F92A");
		private static readonly Guid agent3Id = Guid.Parse("BDC291A4-5D79-4FA5-9050-16786DE0B6BB");
		private static readonly Guid agent4Id = Guid.Parse("55DC3437-30F8-4AD6-A70D-83EE29059F5B");
		private static readonly Guid agent5Id = Guid.Parse("1185BBC2-85D7-4AF7-9B55-3C90A778765C");
		private static readonly Guid agent6Id = Guid.Parse("0A0B04B0-288F-4CEB-9B0D-42BCD79FCE54");

		private static readonly Dictionary<ModuleType, Guid> moduleTypeLookup = new Dictionary<ModuleType, Guid>
		{
			{ ModuleType.Med210_MMD0, agent1Id },
			{ ModuleType.OctaMed, agent2Id },
			{ ModuleType.OctaMed_Professional4, agent3Id },
			{ ModuleType.OctaMed_Professional6, agent4Id },
			{ ModuleType.OctaMed_SoundStudio , agent5Id },
			{ ModuleType.MedPacker , agent6Id },
		};

		internal static readonly string[] fileExtensions = { "med", "mmd0", "mmd1", "mmd2", "mmd3", "mmdc", "omed", "ocss", "md0", "md1", "md2", "md3" };

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_MED_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_MED_DESCRIPTION;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation
		{
			get
			{
				return new AgentSupportInfo[]
				{
					new AgentSupportInfo(Resources.IDS_MED_NAME_AGENT1, Resources.IDS_MED_DESCRIPTION_AGENT1, agent1Id),
					new AgentSupportInfo(Resources.IDS_MED_NAME_AGENT2, Resources.IDS_MED_DESCRIPTION_AGENT2, agent2Id),
					new AgentSupportInfo(Resources.IDS_MED_NAME_AGENT3, Resources.IDS_MED_DESCRIPTION_AGENT3, agent3Id),
					new AgentSupportInfo(Resources.IDS_MED_NAME_AGENT4, Resources.IDS_MED_DESCRIPTION_AGENT4, agent4Id),
					new AgentSupportInfo(Resources.IDS_MED_NAME_AGENT5, Resources.IDS_MED_DESCRIPTION_AGENT5, agent5Id),
					new AgentSupportInfo(Resources.IDS_MED_NAME_AGENT6, Resources.IDS_MED_DESCRIPTION_AGENT6, agent6Id)
				};
			}
		}



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new OctaMedWorker();
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
			ModuleType moduleType = OctaMedWorker.TestModule(fileInfo);
			if (moduleType == ModuleType.Unknown)
				return null;

			return new IdentifyFormatInfo(new OctaMedWorker(moduleType), moduleTypeLookup[moduleType]);
		}
		#endregion
	}
}
