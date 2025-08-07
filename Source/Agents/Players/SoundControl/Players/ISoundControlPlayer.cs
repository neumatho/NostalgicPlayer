/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Agent.Player.SoundControl.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.SoundControl.Players
{
	/// <summary>
	/// Interface for the different SoundControl players
	/// </summary>
	internal interface ISoundControlPlayer
	{
		/// <summary>
		/// Initializes the player
		/// </summary>
		void InitPlayer();

		/// <summary>
		/// Initializes the current song
		/// </summary>
		void InitSound(int songNumber);

		/// <summary>
		/// This is the main player method
		/// </summary>
		bool Play();

		/// <summary>
		/// Return the calculated period table
		/// </summary>
		IEnumerable<ushort> PeriodTable { get; }

		/// <summary>
		/// Return the number of sub songs
		/// </summary>
		int NumberOfSubSongs { get; }

		/// <summary>
		/// Return the number of positions
		/// </summary>
		int NumberOfPositions { get; }

		/// <summary>
		/// Return the current song position
		/// </summary>
		ushort SongPosition { get; }

		/// <summary>
		/// Get or set a snapshot
		/// </summary>
		Snapshot Snapshot { get; set; }
	}
}
