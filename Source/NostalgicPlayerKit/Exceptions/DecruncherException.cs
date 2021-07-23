using System;

namespace Polycode.NostalgicPlayer.Kit.Exceptions
{
	/// <summary>
	/// Throw this exception, if something went wrong while decrunching
	/// </summary>
	public class DecruncherException : Exception
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DecruncherException(string agentName, string message) : base(message)
		{
			AgentName = agentName;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DecruncherException(string agentName, string message, Exception ex) : base(message, ex)
		{
			AgentName = agentName;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the name of the agent that failed decrunching
		/// </summary>
		/********************************************************************/
		public string AgentName
		{
			get;
		}
	}
}
