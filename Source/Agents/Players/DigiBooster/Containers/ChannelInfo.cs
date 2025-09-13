/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.DigiBooster.Containers
{
	/// <summary>
	/// Contains work information for one channel
	/// </summary>
	internal class ChannelInfo : IDeepCloneable<ChannelInfo>
	{
		public byte Volume { get; set; }
		public byte SlideVolumeOld { get; set; }
		public bool OffEnable { get; set; }
		public byte SampleOffset { get; set; }
		public byte RetraceCount { get; set; }
		public byte OldSampleNumber { get; set; }
		public byte RobotOldValue { get; set; }
		public bool RobotEnable { get; set; }
		public int RobotBytesToPlay { get; set; }
		public int RobotCurrentPosition { get; set; }
		public sbyte[][] RobotBuffers { get; set; }
		public short MainPeriod { get; set; }
		public byte MainVolume { get; set; }
		public bool PlayPointer { get; set; }
		public LastRoundInfo LastRoundInfo { get; set; } = new LastRoundInfo();
		public sbyte LoopPatternPosition { get; set; }
		public byte LoopSongPosition { get; set; }
		public byte LoopHowMany { get; set; }
		public byte BackwardEnabled { get; set; }		// 0 = Normal (forward), 1 = Backwards once, 2 = Backwards with loop
		public byte PortamentoUpOldValue { get; set; }
		public byte PortamentoDownOldValue { get; set; }
		public ushort VibratoPeriod { get; set; }
		public sbyte VibratoValue { get; set; }
		public byte VibratoOldValue { get; set; }
		public byte GlissandoOldValue { get; set; }
		public bool GlissandoEnable { get; set; }
		public ushort GlissandoOldPeriod { get; set; }
		public ushort GlissandoNewPeriod { get; set; }
		public bool OnOffChannel { get; set; }
		public ushort OriginalPeriod { get; set; }
		public byte OldVolume { get; set; }

		public bool IsActive { get; set; }
		public sbyte[] SampleData { get; set; }
		public uint StartOffset { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public ChannelInfo MakeDeepClone()
		{
			ChannelInfo clone = (ChannelInfo)MemberwiseClone();

			if (RobotBuffers != null)
				clone.RobotBuffers = ArrayHelper.CloneArray(RobotBuffers);

			clone.LastRoundInfo = LastRoundInfo.MakeDeepClone();

			return clone;
		}
	}
}
