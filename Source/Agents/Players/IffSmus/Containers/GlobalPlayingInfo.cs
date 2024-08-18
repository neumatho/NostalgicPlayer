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
		public short RepeatCount;
		public int CurrentTime;

		public byte Flag;
		public ushort SpeedCounter;
		public ushort CurrentTempo;
		public ushort CalculatedTempo;
		public ushort CalculatedSpeed;

		public ushort MaxVolume;
		public ushort NewVolume;
		public ushort CurrentVolume;

		public Instrument[] CurrentInstruments;
		public short[] InstrumentNumbers;

		// The total note duration is these two numbers added together
		public uint[] HoldNoteDurationCounters;
		public uint[] ReleaseNoteDurationCounters;

		public int[] CurrentTrackPositions;

		public SynthesisPlayInfo[] SynthesisPlayInfo;
		public SampledSoundPlayInfo[] SamplePlayInfo;
		public FormPlayInfo[] FormPlayInfo;

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
