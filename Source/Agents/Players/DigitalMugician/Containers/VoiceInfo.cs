/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.DigitalMugician.Containers
{
	/// <summary>
	/// Holds playinh information for a single channel
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public ushort Track;
		public short Transpose;
		public ushort LastNote;
		public ushort LastInstrument;
		public Effect LastEffect;
		public ushort LastEffectParam;
		public ushort FineTune;

		public ushort NotePeriod;

		public ushort PitchBendEndNote;
		public ushort PitchBendEndPeriod;
		public short CurrentPitchBendPeriod;

		public ushort PitchIndex;
		public ushort ArpeggioIndex;

		public ushort VolumeIndex;
		public ushort VolumeSpeedCounter;

		public byte InstrumentDelay;
		public byte InstrumentEffectSpeed;

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
