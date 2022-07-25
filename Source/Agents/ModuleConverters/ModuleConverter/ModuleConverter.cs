/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
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
		public override AgentSupportInfo[] AgentInformation
		{
			get
			{
				return new AgentSupportInfo[]
				{
					new AgentSupportInfo(Resources.IDS_MODCONV_NAME_AGENT1, Resources.IDS_MODCONV_DESCRIPTION_AGENT1, agent1Id),
					new AgentSupportInfo(Resources.IDS_MODCONV_NAME_AGENT2, Resources.IDS_MODCONV_DESCRIPTION_AGENT2, agent2Id),
					new AgentSupportInfo(Resources.IDS_MODCONV_NAME_AGENT3, Resources.IDS_MODCONV_DESCRIPTION_AGENT3, agent3Id)
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
			if (typeId == agent1Id)
				return new FutureComposer13Format();

			if (typeId == agent2Id)
				return new SoundFx1xFormat();

			if (typeId == agent3Id)
				return new FredEditorFinalFormat();

			return null;
		}
		#endregion
	}
}
