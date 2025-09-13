/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.RonKlaren.Containers
{
	/// <summary>
	/// Holds playing information for a single voice
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public int ChannelNumber { get; set; }

		public PositionList PositionList { get; set; }
		public int TrackListPosition { get; set; }
		public byte[] TrackData { get; set; }
		public int TrackDataPosition { get; set; }
		public ushort TrackRepeatCounter { get; set; }
		public byte WaitCounter { get; set; }

		public Instrument Instrument { get; set; }
		public short InstrumentNumber { get; set; }

		public sbyte[] ArpeggioValues { get; set; }
		public ushort ArpeggioPosition { get; set; }

		public byte CurrentNote { get; set; }
		public short Transpose { get; set; }
		public ushort Period { get; set; }

		public ushort PortamentoEndPeriod { get; set; }
		public byte PortamentoIncrement { get; set; }

		public byte VibratoDelay { get; set; }
		public ushort VibratoPosition { get; set; }

		public ushort AdsrState { get; set; }
		public byte AdsrSpeed { get; set; }
		public sbyte AdsrSpeedCounter { get; set; }
		public byte Volume { get; set; }

		public byte PhaseSpeedCounter { get; set; }

		// These are used to hold the Amiga hardware registers.
		// It is needed, because the original player first set
		// the hardware registers at the next frame. This is
		// needed to avoid some clicks in the sound
		public bool SetHardware { get; set; }
		public short SampleNumber { get; set; }
		public sbyte[] SampleData { get; set; }
		public uint SampleLength { get; set; }
		public bool SetLoop { get; set; }

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
