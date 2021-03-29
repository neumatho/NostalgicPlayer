/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("2630FAE0-312B-4FC5-9C71-7D1016EC9562")]

namespace Polycode.NostalgicPlayer.Agent.Player.ModTracker
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class ModTracker : AgentBase
	{
		internal static readonly Guid Agent1Id = Guid.Parse("F59956F2-6A97-46BB-BC07-487CD10A20A7");
		internal static readonly Guid Agent2Id = Guid.Parse("2CCD4798-AE8E-4110-A22A-312F47792BEA");
		internal static readonly Guid Agent3Id = Guid.Parse("0352C1B7-2C80-4830-A7E2-0AA03885367D");
		internal static readonly Guid Agent4Id = Guid.Parse("19763654-250B-4EFD-B6D8-59A4120F12C8");
		internal static readonly Guid Agent5Id = Guid.Parse("62C2E7CA-EC5D-4BE5-A05D-86D0BC3927EE");
		internal static readonly Guid Agent6Id = Guid.Parse("27B74F65-2033-4883-B935-75B04C2C3AF8");
		internal static readonly Guid Agent7Id = Guid.Parse("5B37208E-AE9A-4DCA-8AB5-9D644199A252");
		internal static readonly Guid Agent8Id = Guid.Parse("EC50140C-0E21-40D3-B435-39A014A13C21");
		internal static readonly Guid Agent9Id = Guid.Parse("A5FDC6F5-DB8B-4066-9566-466E1DA8642C");
		internal static readonly Guid Agent10Id = Guid.Parse("AF2E4860-5731-4007-828E-34B84DF689D2");
		internal static readonly Guid Agent11Id = Guid.Parse("0A47F13A-78A4-4CD7-9A31-DBBC5107FC7C");
		internal static readonly Guid Agent12Id = Guid.Parse("622FC871-244D-4E46-9423-35609EDFCF48");
		internal static readonly Guid Agent13Id = Guid.Parse("2C7EEE56-803D-49E5-936D-6AD8FC14B013");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name
		{
			get
			{
				return Resources.IDS_NAME;
			}
		}



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
					new AgentSupportInfo(Resources.IDS_MOD_NAME_AGENT1, Resources.IDS_MOD_DESCRIPTION_AGENT1, Agent1Id),
					new AgentSupportInfo(Resources.IDS_MOD_NAME_AGENT2, Resources.IDS_MOD_DESCRIPTION_AGENT2, Agent2Id),
					new AgentSupportInfo(Resources.IDS_MOD_NAME_AGENT3, Resources.IDS_MOD_DESCRIPTION_AGENT3, Agent3Id),
					new AgentSupportInfo(Resources.IDS_MOD_NAME_AGENT4, Resources.IDS_MOD_DESCRIPTION_AGENT4, Agent4Id),
					new AgentSupportInfo(Resources.IDS_MOD_NAME_AGENT5, Resources.IDS_MOD_DESCRIPTION_AGENT5, Agent5Id),
					new AgentSupportInfo(Resources.IDS_MOD_NAME_AGENT6, Resources.IDS_MOD_DESCRIPTION_AGENT6, Agent6Id),
					new AgentSupportInfo(Resources.IDS_MOD_NAME_AGENT7, Resources.IDS_MOD_DESCRIPTION_AGENT7, Agent7Id),
					new AgentSupportInfo(Resources.IDS_MOD_NAME_AGENT8, Resources.IDS_MOD_DESCRIPTION_AGENT8, Agent8Id),
					new AgentSupportInfo(Resources.IDS_MOD_NAME_AGENT9, Resources.IDS_MOD_DESCRIPTION_AGENT9, Agent9Id),
					new AgentSupportInfo(Resources.IDS_MOD_NAME_AGENT10, Resources.IDS_MOD_DESCRIPTION_AGENT10, Agent10Id),
					new AgentSupportInfo(Resources.IDS_MOD_NAME_AGENT11, Resources.IDS_MOD_DESCRIPTION_AGENT11, Agent11Id),
					new AgentSupportInfo(Resources.IDS_MOD_NAME_AGENT12, Resources.IDS_MOD_DESCRIPTION_AGENT12, Agent12Id),
					new AgentSupportInfo(Resources.IDS_MTM_NAME_AGENT13, Resources.IDS_MTM_DESCRIPTION_AGENT13, Agent13Id)
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
			return new ModTrackerWorker(typeId);
		}
		#endregion
	}
}
