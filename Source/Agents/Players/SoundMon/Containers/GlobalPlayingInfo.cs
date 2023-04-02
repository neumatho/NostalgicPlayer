/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.SoundMon.Containers
{
	/// <summary>
	/// Holds global information about the playing state
	/// </summary>
	internal class GlobalPlayingInfo : IDeepCloneable<GlobalPlayingInfo>
	{
		public ushort BpStep;
		public byte VibIndex;
		public byte ArpCount;
		public byte BpCount;
		public byte BpDelay;
		public sbyte St;
		public sbyte Tr;
		public byte BpPatCount;
		public byte BpRepCount;
		public byte NewPos;
		public bool PosFlag;
		public bool FirstRepeat;

		public sbyte[] WaveTables;
		public sbyte[][] SynthBuffer;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public GlobalPlayingInfo MakeDeepClone()
		{
			GlobalPlayingInfo clone = (GlobalPlayingInfo)MemberwiseClone();

			clone.WaveTables = ArrayHelper.CloneArray(WaveTables);
			clone.SynthBuffer = ArrayHelper.CloneArray(SynthBuffer);

			return clone;
		}
	}
}
