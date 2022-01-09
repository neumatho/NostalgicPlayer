/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.GuiKit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Output.DiskSaver.Settings
{
	/// <summary>
	/// NostalgicPlayer GUI settings interface implementation
	/// </summary>
	public class DiskSaverGuiSettings : IAgentGuiSettings, IWantOutputAgents, IWantSampleConverterAgents
	{
		private AgentInfo[] loadedOutputAgents;
		private AgentInfo[] loadedSampleConverterAgents;

		#region IAgentGuiSettings implementation
		/********************************************************************/
		/// <summary>
		/// Tells which version of NostalgicPlayer this agent is compiled
		/// against
		/// </summary>
		/********************************************************************/
		public int NostalgicPlayerVersion => IAgent.NostalgicPlayer_Current_Version;



		/********************************************************************/
		/// <summary>
		/// Returns an unique ID for this setting agent
		/// </summary>
		/********************************************************************/
		public virtual Guid SettingAgentId => new Guid(Assembly.GetAssembly(GetType()).GetCustomAttribute<GuidAttribute>().Value);



		/********************************************************************/
		/// <summary>
		/// Return a new instance of the settings control
		/// </summary>
		/********************************************************************/
		public ISettingsControl GetSettingsControl()
		{
			return new SettingsControl(loadedOutputAgents, loadedSampleConverterAgents);
		}
		#endregion

		#region IWantOutputAgents implementation
		/********************************************************************/
		/// <summary>
		/// Gives a list with all loaded output agents
		/// </summary>
		/********************************************************************/
		public void SetOutputInfo(AgentInfo[] agents)
		{
			loadedOutputAgents = agents.Where(a => a.TypeId != DiskSaver.Agent1Id).ToArray();
		}
		#endregion

		#region IWantSampleConverterAgents implementation
		/********************************************************************/
		/// <summary>
		/// Gives a list with all loaded sample converter agents
		/// </summary>
		/********************************************************************/
		public void SetSampleConverterInfo(AgentInfo[] agents)
		{
			loadedSampleConverterAgents = agents.Where(a => a.Agent.CreateInstance(a.TypeId) is ISampleSaverAgent).ToArray();
		}
		#endregion
	}
}
