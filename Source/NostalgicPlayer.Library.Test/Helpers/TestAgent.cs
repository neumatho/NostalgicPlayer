/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Library.Test.Helpers
{
	/// <summary>
	/// Agent which creates the worker given in the constructor. Used to wrap
	/// the test converters and the test player
	/// </summary>
	internal class TestAgent : IAgent
	{
		private readonly Func<IAgentWorker> createWorker;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private TestAgent(string name, Guid agentId, Func<IAgentWorker> createWorker)
		{
			Name = name;
			AgentId = agentId;

			this.createWorker = createWorker;
		}



		/********************************************************************/
		/// <summary>
		/// Will create an agent with a single type and wrap it in the
		/// AgentInfo the agent manager hands over to the loader
		/// </summary>
		/********************************************************************/
		public static AgentInfo CreateAgentInfo(string name, Func<IAgentWorker> createWorker)
		{
			Guid agentId = Guid.NewGuid();
			TestAgent agent = new TestAgent(name, agentId, createWorker);

			return new AgentInfo(agent, name, agent.Description, agent.Version, agentId, false, false);
		}



		/********************************************************************/
		/// <summary>
		/// Return the NostalgicPlayer version this agent is compatible with
		/// </summary>
		/********************************************************************/
		public int NostalgicPlayerVersion => IAgent.NostalgicPlayer_Current_Version;



		/********************************************************************/
		/// <summary>
		/// Returns an unique ID for this agent
		/// </summary>
		/********************************************************************/
		public Guid AgentId
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public string Name
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public string Description => $"{Name} description";



		/********************************************************************/
		/// <summary>
		/// Returns the version of this agent
		/// </summary>
		/********************************************************************/
		public Version Version => new Version(1, 0);



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public AgentSupportInfo[] AgentInformation => [ new AgentSupportInfo(Name, Description, AgentId) ];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public IAgentWorker CreateInstance(Guid typeId)
		{
			return createWorker();
		}
	}
}
