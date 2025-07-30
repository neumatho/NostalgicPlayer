/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.IffSmus.Instruments;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.IffSmus.Containers
{
	/// <summary>
	/// Holds global information about the playing state
	/// </summary>
	internal class GlobalPlayingInfo : IDeepCloneable<GlobalPlayingInfo>
	{
		public short RepeatCount { get; set; }
		public int CurrentTime { get; set; }

		public byte Flag { get; set; }
		public ushort SpeedCounter { get; set; }
		public ushort CurrentTempo { get; set; }
		public ushort CalculatedTempo { get; set; }
		public ushort CalculatedSpeed { get; set; }

		public ushort MaxVolume { get; set; }
		public ushort NewVolume { get; set; }
		public ushort CurrentVolume { get; set; }

		public Instrument[] CurrentInstruments { get; set; }
		public short[] InstrumentNumbers { get; set; }

		// The total note duration is these two numbers added together
		public uint[] HoldNoteDurationCounters { get; set; }
		public uint[] ReleaseNoteDurationCounters { get; set; }

		public int[] CurrentTrackPositions { get; set; }

		public SynthesisPlayInfo[] SynthesisPlayInfo { get; set; }
		public SampledSoundPlayInfo[] SamplePlayInfo { get; set; }
		public FormPlayInfo[] FormPlayInfo { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public GlobalPlayingInfo MakeDeepClone()
		{
			GlobalPlayingInfo clone = (GlobalPlayingInfo)MemberwiseClone();

			clone.CurrentInstruments = ArrayHelper.CloneArray(CurrentInstruments);
			clone.InstrumentNumbers = ArrayHelper.CloneArray(InstrumentNumbers);

			clone.HoldNoteDurationCounters = ArrayHelper.CloneArray(HoldNoteDurationCounters);
			clone.ReleaseNoteDurationCounters = ArrayHelper.CloneArray(ReleaseNoteDurationCounters);

			clone.CurrentTrackPositions = ArrayHelper.CloneArray(CurrentTrackPositions);

			clone.SynthesisPlayInfo = ArrayHelper.CloneObjectArray(SynthesisPlayInfo);
			clone.SamplePlayInfo = ArrayHelper.CloneObjectArray(SamplePlayInfo);
			clone.FormPlayInfo = ArrayHelper.CloneObjectArray(FormPlayInfo);

			return clone;
		}
	}
}
