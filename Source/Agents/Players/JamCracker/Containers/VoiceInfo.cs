/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.JamCracker.Containers
{
	/// <summary>
	/// Voice info structure
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public ushort WaveOffset = 0;
		public ushort Dmacon;
		public short InsNum;
		public ushort InsLen;
		public sbyte[] InsAddress;
		public sbyte[] RealInsAddress;
		public sbyte[] WaveBuffer = new sbyte[0x40];
		public int PerIndex;
		public ushort[] Pers = new ushort[3];
		public short Por;
		public short DeltaPor;
		public short PorLevel;
		public short Vib;
		public short DeltaVib;
		public short Vol;
		public short DeltaVol;
		public ushort VolLevel;
		public ushort Phase;
		public short DeltaPhase;
		public byte VibCnt;
		public byte VibMax;
		public byte Flags;

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
