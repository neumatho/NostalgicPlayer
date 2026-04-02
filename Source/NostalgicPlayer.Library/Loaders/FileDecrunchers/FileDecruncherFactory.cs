/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Library.Agent;

namespace Polycode.NostalgicPlayer.Library.Loaders.FileDecrunchers
{
	/// <summary>
	/// Factory class to create file decruncher instances
	/// </summary>
	internal class FileDecruncherFactory
	{
		private readonly IAgentManager agentManager;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FileDecruncherFactory(IAgentManager agentManager)
		{
			this.agentManager = agentManager;
		}



		/********************************************************************/
		/// <summary>
		/// Create a new instance of the single file decruncher
		/// </summary>
		/********************************************************************/
		public SingleFileDecruncher CreateSingleFileDecruncher()
		{
			return new SingleFileDecruncher(agentManager);
		}
	}
}
