/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.JamCracker.Containers
{
	/// <summary>
	/// Voice info structure
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public ushort WaveOffset { get; set; } = 0;
		public ushort Dmacon { get; set; }
		public short InsNum { get; set; }
		public ushort InsLen { get; set; }
		public sbyte[] InsAddress { get; set; }
		public sbyte[] RealInsAddress { get; set; }
		public sbyte[] WaveBuffer { get; set; } = new sbyte[0x40];
		public int PerIndex { get; set; }
		public ushort[] Pers { get; set; } = new ushort[3];
		public short Por { get; set; }
		public short DeltaPor { get; set; }
		public short PorLevel { get; set; }
		public short Vib { get; set; }
		public short DeltaVib { get; set; }
		public short Vol { get; set; }
		public short DeltaVol { get; set; }
		public ushort VolLevel { get; set; }
		public ushort Phase { get; set; }
		public short DeltaPhase { get; set; }
		public byte VibCnt { get; set; }
		public byte VibMax { get; set; }
		public byte Flags { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public VoiceInfo MakeDeepClone()
		{
			VoiceInfo clone = (VoiceInfo)MemberwiseClone();

			if (InsAddress != null)
				clone.InsAddress = ArrayHelper.CloneArray(InsAddress);

			if (RealInsAddress != null)
				clone.RealInsAddress = ArrayHelper.CloneArray(RealInsAddress);

			clone.WaveBuffer = ArrayHelper.CloneArray(WaveBuffer);
			clone.Pers = ArrayHelper.CloneArray(Pers);

			return clone;
		}
	}
}
