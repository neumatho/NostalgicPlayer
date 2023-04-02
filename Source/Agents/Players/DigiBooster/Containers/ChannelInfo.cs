/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.DigiBooster.Containers
{
	/// <summary>
	/// Contains work information for one channel
	/// </summary>
	internal class ChannelInfo : IDeepCloneable<ChannelInfo>
	{
		public byte Volume;
		public byte SlideVolumeOld;
		public bool OffEnable;
		public byte SampleOffset;
		public byte RetraceCount;
		public byte OldSampleNumber;
		public byte RobotOldValue;
		public bool RobotEnable;
		public int RobotBytesToPlay;
		public int RobotCurrentPosition;
		public sbyte[][] RobotBuffers;
		public short MainPeriod;
		public byte MainVolume;
		public bool PlayPointer;
		public LastRoundInfo LastRoundInfo = new LastRoundInfo();
		public sbyte LoopPatternPosition;
		public byte LoopSongPosition;
		public byte LoopHowMany;
		public byte BackwardEnabled;		// 0 = Normal (forward), 1 = Backwards once, 2 = Backwards with loop
		public byte PortamentoUpOldValue;
		public byte PortamentoDownOldValue;
		public ushort VibratoPeriod;
		public sbyte VibratoValue;
		public byte VibratoOldValue;
		public byte GlissandoOldValue;
		public bool GlissandoEnable;
		public ushort GlissandoOldPeriod;
		public ushort GlissandoNewPeriod;
		public bool OnOffChannel;
		public ushort OriginalPeriod;
		public byte OldVolume;

		public bool IsActive;
		public sbyte[] SampleData;
		public uint StartOffset;

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
