/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.Block;

namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.ScanSong
{
	/// <summary>
	/// 
	/// </summary>
	internal class ScanBlock : ScanTrack
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual void DoBlock(MedBlock blk)
		{
			for (TrackNum trk = 0; trk < blk.Tracks(); trk++)
				DoTrack(blk, trk);
		}
	}
}
