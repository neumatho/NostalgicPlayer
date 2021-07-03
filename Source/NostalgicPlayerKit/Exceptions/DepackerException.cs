using System;

namespace Polycode.NostalgicPlayer.Kit.Exceptions
{
	/// <summary>
	/// Throw this exception, if something went wrong while depacking
	/// </summary>
	public class DepackerException : Exception
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DepackerException(string agentName, string message) : base(message)
		{
			AgentName = agentName;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the name of the agent that failed
		/// </summary>
		/********************************************************************/
		public string AgentName
		{
			get;
		}
	}
}
