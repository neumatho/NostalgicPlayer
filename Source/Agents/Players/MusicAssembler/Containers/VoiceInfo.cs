/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.MusicAssembler.Containers
{
	/// <summary>
	/// Holds playing information for a single voice
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public int ChannelNumber { get; set; }

		public PositionInfo[] PositionList { get; set; }

		public ushort CurrentPosition { get; set; }
		public ushort CurrentTrackRow { get; set; }
		public sbyte TrackRepeatCounter { get; set; }
		public sbyte RowDelayCounter { get; set; }

		public VoiceFlag Flag { get; set; }

		public Instrument CurrentInstrument { get; set; }

		public byte CurrentNote { get; set; }
		public byte Transpose { get; set; }

		public byte Volume { get; set; }
		public bool DecreaseVolume { get; set; }
		public byte SustainCounter { get; set; }

		public byte ArpeggioCounter { get; set; }
		public byte ArpeggioValueToUse { get; set; }

		public byte PortamentoOrVibratoValue { get; set; }

		public short PortamentoAddValue { get; set; }

		public byte VibratoDelayCounter { get; set; }
		public byte VibratoDirection { get; set; }
		public byte VibratoSpeedCounter { get; set; }
		public short VibratoAddValue { get; set; }

		public ushort WaveLengthModifier { get; set; }
		public bool WaveDirection { get; set; }
		public byte WaveSpeedCounter { get; set; }

		// These are used to hold the Amiga hardware registers.
		// It is needed, because the original player does not set
		// all the hardware registers at the same time
		public sbyte[] SampleData { get; set; }
		public uint SampleStartOffset { get; set; }
		public ushort SampleLength { get; set; }

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
