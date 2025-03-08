/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation
{
	/// <summary>
	/// Handles all the effects
	/// </summary>
	internal class EffectMaster : IEffectMaster, IDeepCloneable<EffectMaster>
	{
		private SubSong subSong;

		private List<EffectGroup> groups;
		private Dictionary<int, int> trackGroups;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public EffectMaster(SubSong ss)
		{
			subSong = ss;
			groups = new List<EffectGroup>();
			trackGroups = new Dictionary<int, int>();

			AddGroup();
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of groups added including global group
		/// </summary>
		/********************************************************************/
		public int GetNumGroups()
		{
			return groups.Count;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the global effect group
		/// </summary>
		/********************************************************************/
		public EffectGroup GlobalGroup => groups[0];



		/********************************************************************/
		/// <summary>
		/// Return the group at the position given
		/// </summary>
		/********************************************************************/
		public EffectGroup GetGroup(int grp)
		{
			if ((grp < 0) || (grp >= groups.Count))
				return null;

			return groups[grp];
		}



		/********************************************************************/
		/// <summary>
		/// Assign a track to a group
		/// </summary>
		/********************************************************************/
		public void SetTrackGroup(TrackNum trk, int grp)
		{
			trackGroups[trk] = grp;
		}



		/********************************************************************/
		/// <summary>
		/// Change the parent sub-song
		/// </summary>
		/********************************************************************/
		public void SetParent(SubSong ss)
		{
			subSong = ss;

			foreach (EffectGroup grp in groups)
				grp.SetParent(ss);
		}



		/********************************************************************/
		/// <summary>
		/// Initialize all effect groups
		/// </summary>
		/********************************************************************/
		public void Initialize()
		{
			// Make sure all channels are mapped to a group, if
			// extra groups are added
			if (groups.Count > 1)
			{
				for (int i = 0; i < subSong.GetNumChannels(); i++)
				{
					if (!trackGroups.ContainsKey(i))
						trackGroups[i] = 0;
				}
			}

			foreach (EffectGroup group in groups)
				group.Initialize();
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup all effect groups
		/// </summary>
		/********************************************************************/
		public void Cleanup()
		{
			foreach (EffectGroup group in groups)
				group.Cleanup();
		}



		/********************************************************************/
		/// <summary>
		/// Add a new effect group
		/// </summary>
		/********************************************************************/
		public void AddGroup()
		{
			groups.Add(new EffectGroup(subSong));
		}

		#region IEffectMaster implementation
		/********************************************************************/
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
		/********************************************************************/
		public IReadOnlyDictionary<int, int> GetChannelGroups()
		{
			return trackGroups;
		}



		/********************************************************************/
		/// <summary>
		/// Will add effects to a channel group
		/// </summary>
		/********************************************************************/
		public void AddChannelGroupEffects(int group, int[][] dest, int todoInFrames, uint mixerFrequency)
		{
			EffectGroup effectGroup = GetGroup(group);
			effectGroup?.DoEffects(dest, todoInFrames, mixerFrequency);
		}



		/********************************************************************/
		/// <summary>
		/// Will add effects to the final mixed output
		/// </summary>
		/********************************************************************/
		public void AddGlobalEffects(int[][] dest, int todoInFrames, uint mixerFrequency)
		{
			if (groups.Count == 1)
				GlobalGroup.DoEffects(dest, todoInFrames, mixerFrequency);
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public EffectMaster MakeDeepClone()
		{
			EffectMaster clone = (EffectMaster)MemberwiseClone();

			clone.trackGroups = new Dictionary<int, int>(trackGroups);

			clone.groups = new List<EffectGroup>();

			foreach (EffectGroup grp in groups)
				clone.groups.Add(grp.MakeDeepClone());

			return clone;
		}
	}
}
