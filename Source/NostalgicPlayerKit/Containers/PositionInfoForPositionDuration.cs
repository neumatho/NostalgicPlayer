/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// Holds information about a single position
	/// </summary>
	public class PositionInfoForPositionDuration : PositionInfo
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		internal PositionInfoForPositionDuration(TimeSpan time, float playingFrequency, bool amigaFilter, int[] positionSubSongs, ISnapshot snapshot) : base(time, playingFrequency, amigaFilter, snapshot)
		{
			PositionSubSongs = new int[positionSubSongs.Length];
			Array.Copy(positionSubSongs, PositionSubSongs, positionSubSongs.Length);
		}



		/********************************************************************/
		/// <summary>
		/// Holds an array over all original player positions and which
		/// sub-song each position belongs to
		/// </summary>
		/********************************************************************/
		internal int[] PositionSubSongs
		{
			get;
		}
	}
}
