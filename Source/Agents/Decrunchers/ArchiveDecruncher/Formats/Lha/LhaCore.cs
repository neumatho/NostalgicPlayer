/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Lha
{
	/// <summary>
	/// The core functionality of Lha
	/// </summary>
	internal partial class LhaCore
	{
		private readonly string agentName;
		private readonly ReaderStream stream;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public LhaCore(string agentName, ReaderStream stream)
		{
			this.agentName = agentName;
			this.stream = stream;
		}
	}
}
