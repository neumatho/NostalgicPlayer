/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SoundControl.Players.Containers
{
	/// <summary>
	/// Contains information about a single sub-song.
	/// Only used by the SoundControl 4.0/5.0 player
	/// </summary>
	internal class SongInfo40_50
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SongInfo40_50(ushort startPosition, ushort endPosition, ushort speed)
		{
			StartPosition = startPosition;
			EndPosition = endPosition;
			Speed = speed;
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



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ushort Speed { get; }
	}
}
