/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.NostalgicPlayerKit.Bases;
using Polycode.NostalgicPlayer.NostalgicPlayerKit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.JamCracker
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class JamCracker : AgentBase
	{
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
				return Properties.Resources.IDS_NAME;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description
		{
			get
			{
				return Properties.Resources.IDS_DESCRIPTION;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance()
		{
			return new JamCrackerWorker();
		}
		#endregion
	}
}
