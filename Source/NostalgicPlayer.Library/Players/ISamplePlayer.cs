/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Library.Players
{
	/// <summary>
	/// Interface for players playing sample files
	/// </summary>
	public interface ISamplePlayer : IPlayer
	{
		/// <summary>
		/// Will set a new song position
		/// </summary>
		void SetSongPosition(int position);

		/// <summary>
		/// Event called when the position is changed
		/// </summary>
		event EventHandler PositionChanged;
	}
}
