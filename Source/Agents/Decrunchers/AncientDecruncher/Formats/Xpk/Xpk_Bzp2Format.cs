/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Formats.Streams.Xpk;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Formats.Xpk
{
	/// <summary>
	/// Can decrunch XPK (BZP2) crunched files
	/// </summary>
	internal class Xpk_Bzp2Format : XpkFormatBase
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Xpk_Bzp2Format(string agentName) : base(agentName)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Return a stream holding the decrunched data
		/// </summary>
		/********************************************************************/
		public override DecruncherStream OpenStream(Stream crunchedDataStream)
		{
			return new Xpk_Bzp2Stream(agentName, crunchedDataStream);
		}



		/********************************************************************/
		/// <summary>
		/// Return the cruncher ID
		/// </summary>
		/********************************************************************/
		protected override uint CruncherId => 0x425a5032;		// BZP2
	}
}
