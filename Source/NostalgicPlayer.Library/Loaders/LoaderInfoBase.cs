/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Library.Players;

namespace Polycode.NostalgicPlayer.Library.Loaders
{
	/// <summary>
	/// Base class for loader information
	/// </summary>
	public abstract class LoaderInfoBase
	{
		/********************************************************************/
		/// <summary>
		/// Return the source (file name or url) of the module loaded
		/// </summary>
		/********************************************************************/
		public abstract string Source
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return the player instance
		/// </summary>
		/********************************************************************/
		public IPlayer Player
		{
			get; protected set;
		}



		/********************************************************************/
		/// <summary>
		/// Return information about the player agent
		/// </summary>
		/********************************************************************/
		public AgentInfo PlayerAgentInfo
		{
			get; protected set;
		}



		/********************************************************************/
		/// <summary>
		/// Return the agent instance
		/// </summary>
		/********************************************************************/
		internal abstract IAgentWorker WorkerAgent
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return the format loaded
		/// </summary>
		/********************************************************************/
		internal abstract string Format
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return the format description
		/// </summary>
		/********************************************************************/
		internal abstract string FormatDescription
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name of the player
		/// </summary>
		/********************************************************************/
		internal abstract string PlayerName
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return the description of the player
		/// </summary>
		/********************************************************************/
		internal abstract string PlayerDescription
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return the size of the module loaded
		/// </summary>
		/********************************************************************/
		internal abstract long ModuleSize
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return the size of the module crunched. Is zero if not crunched.
		/// If -1, it means the crunched length is unknown
		/// </summary>
		/********************************************************************/
		internal abstract long CrunchedSize
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return a list of all the algorithms used to decrunch the module.
		/// If null, no decruncher has been used
		/// </summary>
		/********************************************************************/
		internal abstract string[] DecruncherAlgorithms
		{
			get;
		}
	}
}
