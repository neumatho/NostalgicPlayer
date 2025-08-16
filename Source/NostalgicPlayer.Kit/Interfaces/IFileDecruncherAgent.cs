/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Agents of this type, can decrunch a single format
	/// </summary>
	public interface IFileDecruncherAgent : IAgentWorker
	{
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		AgentResult Identify(Stream crunchedDataStream);

		/// <summary>
		/// Return a stream holding the decrunched data
		/// </summary>
		DecruncherStream OpenStream(Stream crunchedDataStream);
	}
}
