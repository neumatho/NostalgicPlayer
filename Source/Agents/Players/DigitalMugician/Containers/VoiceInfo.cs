/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.DigitalMugician.Containers
{
	/// <summary>
	/// Holds playing information for a single channel
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public ushort Track { get; set; }
		public short Transpose { get; set; }
		public ushort LastNote { get; set; }
		public ushort LastInstrument { get; set; }
		public Effect LastEffect { get; set; }
		public ushort LastEffectParam { get; set; }
		public ushort FineTune { get; set; }

		public ushort NotePeriod { get; set; }

		public ushort PitchBendEndNote { get; set; }
		public ushort PitchBendEndPeriod { get; set; }
		public short CurrentPitchBendPeriod { get; set; }

		public ushort PitchIndex { get; set; }
		public ushort ArpeggioIndex { get; set; }

		public ushort VolumeIndex { get; set; }
		public ushort VolumeSpeedCounter { get; set; }

		public byte InstrumentDelay { get; set; }
		public byte InstrumentEffectSpeed { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public VoiceInfo MakeDeepClone()
		{
			return (VoiceInfo)MemberwiseClone();
		}
	}
}
