﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Add some extra methods for a module duration player
	/// </summary>
	public interface IModuleDuration : IDuration
	{
		/// <summary>
		/// Initialize player to play the given sub-song
		/// </summary>
		bool SetSubSong(int subSong, out string errorMessage);
	}
}
