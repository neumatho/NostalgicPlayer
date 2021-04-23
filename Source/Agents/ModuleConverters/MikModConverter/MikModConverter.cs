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
using Polycode.NostalgicPlayer.Agent.ModuleConverter.MikModConverter.Formats;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("ED2854DF-91A7-4174-B100-FB169AAE5AF6")]

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.MikModConverter
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class MikModConverter : AgentBase
	{
		private static readonly Guid agent14Id = Guid.Parse("E37DF813-DCBD-4A32-AA07-5EF1AF6DD037");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_MIKCONV_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_MIKCONV_DESCRIPTION;



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
					new AgentSupportInfo(Resources.IDS_MIKCONV_NAME_AGENT14, Resources.IDS_MIKCONV_DESCRIPTION_AGENT14, agent14Id)
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
			if (typeId == agent14Id)
				return new MikModConverterWorker_UniMod();

			return null;
		}
		#endregion
	}
}
