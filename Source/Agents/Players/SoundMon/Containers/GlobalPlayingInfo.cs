/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SoundMon.Containers
{
	/// <summary>
	/// Holds global information about the playing state
	/// </summary>
	internal class GlobalPlayingInfo : IDeepCloneable<GlobalPlayingInfo>
	{
		public ushort BpStep { get; set; }
		public byte VibIndex { get; set; }
		public byte ArpCount { get; set; }
		public byte BpCount { get; set; }
		public byte BpDelay { get; set; }
		public sbyte St { get; set; }
		public sbyte Tr { get; set; }
		public byte BpPatCount { get; set; }
		public byte BpRepCount { get; set; }
		public byte NewPos { get; set; }
		public bool PosFlag { get; set; }
		public bool FirstRepeat { get; set; }

		public sbyte[] WaveTables { get; set; }
		public sbyte[][] SynthBuffer { get; set; }

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
