/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.MusicAssembler.Containers
{
	/// <summary>
	/// Holds playing information for a single voice
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public int ChannelNumber;

		public PositionInfo[] PositionList;

		public ushort CurrentPosition;
		public ushort CurrentTrackRow;
		public sbyte TrackRepeatCounter;
		public sbyte RowDelayCounter;

		public VoiceFlag Flag;

		public Instrument CurrentInstrument;

		public byte CurrentNote;
		public byte Transpose;

		public byte Volume;
		public bool DecreaseVolume;
		public byte SustainCounter;

		public byte ArpeggioCounter;
		public byte ArpeggioValueToUse;

		public byte PortamentoOrVibratoValue;

		public short PortamentoAddValue;

		public byte VibratoDelayCounter;
		public byte VibratoDirection;
		public byte VibratoSpeedCounter;
		public short VibratoAddValue;

		public ushort WaveLengthModifier;
		public bool WaveDirection;
		public byte WaveSpeedCounter;

		// These are used to hold the Amiga hardware registers.
		// It is needed, because the original player does not set
		// all the hardware registers at the same time
		public sbyte[] SampleData;
		public uint SampleStartOffset;
		public ushort SampleLength;

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
