/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Base interface for all players that can calculate the duration
	/// </summary>
	public interface IDurationPlayer
	{
		/// <summary>
		/// Defines the number of seconds between each position
		/// </summary>
		public const float NumberOfSecondsBetweenEachSnapshot = 3.0f;

		/// <summary>
		/// Calculate the duration for all sub-songs
		/// </summary>
		DurationInfo[] CalculateDuration();

		/// <summary>
		/// Will tell the player to change its current state to match the
		/// position given
		/// </summary>
		void SetSongPosition(PositionInfo positionInfo);

		/// <summary>
		/// Return the time into the song when restarting
		/// </summary>
		TimeSpan GetRestartTime();
	}
}
