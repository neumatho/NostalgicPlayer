/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Players
{
	/// <summary>
	/// Interface for players that streams
	/// </summary>
	public interface IStreamingPlayer : IPlayer
	{
		/// <summary>
		/// Event called when the position is changed
		/// </summary>
		event EventHandler PositionChanged;
	}
}
