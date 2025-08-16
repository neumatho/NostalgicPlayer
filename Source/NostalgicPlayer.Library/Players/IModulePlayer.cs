/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers.Events;

namespace Polycode.NostalgicPlayer.Library.Players
{
	/// <summary>
	/// Interface for players playing module files
	/// </summary>
	public interface IModulePlayer : IPlayer
	{
		/// <summary>
		/// Will select the song you want to play. If songNumber is -1, the
		/// default song will be selected
		/// </summary>
		bool SelectSong(int songNumber, out string errorMessage);

		/// <summary>
		/// Will set a new song position
		/// </summary>
		void SetSongPosition(int position);

		/// <summary>
		/// Event called when the position is changed
		/// </summary>
		event EventHandler PositionChanged;

		/// <summary>
		/// Event called when the player change sub-song
		/// </summary>
		public event SubSongChangedEventHandler SubSongChanged;
	}
}
