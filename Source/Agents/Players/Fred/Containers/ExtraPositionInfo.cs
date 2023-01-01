/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Fred.Containers
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
		public ExtraPositionInfo(ushort position, ushort trackPosition, ushort trackDuration)
		{
			Position = position;
			TrackPosition = trackPosition;
			TrackDuration = trackDuration;
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
		public ushort TrackPosition
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Hold the current duration count
		/// </summary>
		/********************************************************************/
		public ushort TrackDuration
		{
			get;
		}
	}
}
