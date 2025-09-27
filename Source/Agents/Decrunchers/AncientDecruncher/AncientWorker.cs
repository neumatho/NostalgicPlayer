/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Streams;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Exceptions;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Ports.LibAncient;
using Polycode.NostalgicPlayer.Ports.LibAncient.Exceptions;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher
{
    /// <summary>
    /// Worker class for all formats
    /// </summary>
    internal class AncientWorker : FileDecruncherAgentBase
    {
		private readonly string agentName;
		private readonly Decompressor decompressor;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AncientWorker(string agentName, Decompressor decompressor = null)
		{
			this.agentName = agentName;
			this.decompressor = decompressor;
		}

		#region IFileDecruncherAgent implementation
		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(Stream crunchedDataStream)
		{
			return AgentResult.Error;
		}



		/********************************************************************/
		/// <summary>
		/// Return a stream holding the decrunched data
		/// </summary>
		/********************************************************************/
		public override DecruncherStream OpenStream(Stream crunchedDataStream)
		{
			try
			{
				return new AncientDecruncherStream(new AncientStream(agentName, decompressor));
			}
			catch (DecompressionException)
			{
				throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);
			}
			catch (VerificationException)
			{
				throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CHECKSUM_MISMATCH);
			}
		}
		#endregion
    }
}
