/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("B2F53581-6F2F-4C8F-942F-94190BDFE442")]

namespace Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class MusiclineEditor : AgentBase
	{
		internal static readonly Guid Agent1Id = Guid.Parse("4E21401E-17D9-4634-81C2-2235D61F0921");
		internal static readonly Guid Agent2Id = Guid.Parse("655578CF-AFDE-4F58-B569-1C5A21ECAFFA");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_MLE_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_MLE_DESCRIPTION;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation =>
		[
			new AgentSupportInfo(Resources.IDS_MLE_NAME_AGENT1, Resources.IDS_MLE_DESCRIPTION_AGENT1, Agent1Id),
			new AgentSupportInfo(Resources.IDS_MLE_NAME_AGENT2, Resources.IDS_MLE_DESCRIPTION_AGENT2, Agent2Id)
		];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new MusiclineEditorWorker(typeId);
		}
		#endregion
	}
}
