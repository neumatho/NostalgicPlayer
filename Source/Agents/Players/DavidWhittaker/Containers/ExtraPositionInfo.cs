/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DavidWhittaker.Containers
{
	/// <summary>
	/// Contains extra information for each position
	/// </summary>
	internal class ExtraPositionInfo
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ExtraPositionInfo(ushort position, int trackPosition, ushort speedCounter)
		{
			Position = position;
			TrackPosition = trackPosition;
			SpeedCounter = speedCounter;
		}



		/********************************************************************/
		/// <summary>
		/// Hold the position
		/// </summary>
		/********************************************************************/
		public ushort Position
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Hold the position in the track
		/// </summary>
		/********************************************************************/
		public int TrackPosition
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Hold the current speed count
		/// </summary>
		/********************************************************************/
		public ushort SpeedCounter
		{
			get;
		}
	}
}
