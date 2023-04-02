/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.Med.Containers
{
	/// <summary>
	/// Holds global information about the playing state
	/// </summary>
	internal class GlobalPlayingInfo : IDeepCloneable<GlobalPlayingInfo>
	{
		public ushort PlayPositionNumber;
		public ushort PlayBlock;
		public ushort PlayLine;

		public byte Counter;
		public ushort CurrentTrackCount;
		public bool NextBlock;

		public ushort[] PreviousPeriod;
		public byte[] PreviousNotes;
		public byte[] PreviousSamples;
		public byte[] PreviousVolumes;
		public Effect[] Effects;
		public byte[] EffectArgs;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public GlobalPlayingInfo MakeDeepClone()
		{
			GlobalPlayingInfo clone = (GlobalPlayingInfo)MemberwiseClone();

			clone.PreviousPeriod = ArrayHelper.CloneArray(PreviousPeriod);
			clone.PreviousNotes = ArrayHelper.CloneArray(PreviousNotes);
			clone.PreviousSamples = ArrayHelper.CloneArray(PreviousSamples);
			clone.PreviousVolumes = ArrayHelper.CloneArray(PreviousVolumes);
			clone.Effects = ArrayHelper.CloneArray(Effects);
			clone.EffectArgs = ArrayHelper.CloneArray(EffectArgs);

			return clone;
		}
	}
}
