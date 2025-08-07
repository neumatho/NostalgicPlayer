/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SoundControl.Players.Containers
{
	/// <summary>
	/// Contains information about a single sub-song.
	/// Only used by the SoundControl 3.x player
	/// </summary>
	internal class SongInfo3x
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SongInfo3x(ushort startPosition, ushort endPosition)
		{
			StartPosition = startPosition;
			EndPosition = endPosition;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ushort StartPosition { get; }



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ushort EndPosition { get; }
	}
}
