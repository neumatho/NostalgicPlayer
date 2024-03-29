﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.ScanSong
{
	/// <summary>
	/// 
	/// </summary>
	internal class ScanSong : ScanSubSong
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual void DoSong(Song sg)
		{
			for (uint sNum = 0; sNum < sg.NumSubSongs(); sNum++)
				DoSubSong(sg.GetSubSong(sNum));
		}
	}
}
