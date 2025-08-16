/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.Bases
{
	/// <summary>
	/// Base class that have some default implementations that can be used
	/// </summary>
	public abstract class AgentBase : IAgent
	{
		/********************************************************************/
		/// <summary>
		/// Tells which version of NostalgicPlayer this agent is compiled
		/// against
		/// </summary>
		/********************************************************************/
		public int NostalgicPlayerVersion => IAgent.NostalgicPlayer_Current_Version;



		/********************************************************************/
		/// <summary>
		/// Returns an unique ID for this agent
		/// </summary>
		/********************************************************************/
		public virtual Guid AgentId => new Guid(Assembly.GetAssembly(GetType()).GetCustomAttribute<GuidAttribute>().Value);



		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public abstract string Name
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent. Only needed for players
		/// and module converters
		/// </summary>
		/********************************************************************/
		public virtual string Description => string.Empty;



		/********************************************************************/
		/// <summary>
		/// Returns the version of this agent
		/// </summary>
		/********************************************************************/
		public virtual Version Version => Assembly.GetAssembly(GetType()).GetName().Version;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public abstract AgentSupportInfo[] AgentInformation
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		///
		/// May only return workers of same type
		/// </summary>
		/********************************************************************/
		public abstract IAgentWorker CreateInstance(Guid typeId);
	}
}
