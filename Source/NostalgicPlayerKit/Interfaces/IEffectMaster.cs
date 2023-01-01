/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Module players can implement this interface to add extra effects on the mixed output
	/// </summary>
	public interface IEffectMaster
	{
		/// <summary>
		/// Return a map between a channel and a group number. All channels
		/// in the same group will be mixed together and added the same
		/// effects.
		///
		/// If a channel is not mapped to a group, no extra effects will be
		/// added other than the global effects if any.
		///
		/// Returning null is the same as returning an empty list, so you
		/// can do that, if you do not support channel groups in your player
		/// </summary>
		IReadOnlyDictionary<int, int> GetChannelGroups();

		/// <summary>
		/// Will add effects to a channel group
		/// </summary>
		void AddChannelGroupEffects(int group, int[] dest, int todo, uint mixerFrequency, bool stereo);

		/// <summary>
		/// Will add effects to the final mixed output
		/// </summary>
		void AddGlobalEffects(int[] dest, int todo, uint mixerFrequency, bool stereo);
	}
}
