/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.RonKlaren.Containers
{
	/// <summary>
	/// Holds playing information for a single voice
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public int ChannelNumber;

		public PositionList PositionList;
		public int TrackListPosition;
		public byte[] TrackData;
		public int TrackDataPosition;
		public ushort TrackRepeatCounter;
		public byte WaitCounter;

		public Instrument Instrument;
		public short InstrumentNumber;

		public sbyte[] ArpeggioValues;
		public ushort ArpeggioPosition;

		public byte CurrentNote;
		public short Transpose;
		public ushort Period;

		public ushort PortamentoEndPeriod;
		public byte PortamentoIncrement;

		public byte VibratoDelay;
		public ushort VibratoPosition;

		public ushort AdsrState;
		public byte AdsrSpeed;
		public sbyte AdsrSpeedCounter;
		public byte Volume;

		public byte PhaseSpeedCounter;

		// These are used to hold the Amiga hardware registers.
		// It is needed, because the original player first set
		// the hardware registers at the next frame. This is
		// needed to avoid some clicks in the sound
		public bool SetHardware;
		public short SampleNumber;
		public sbyte[] SampleData;
		public uint SampleLength;
		public bool SetLoop;

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
