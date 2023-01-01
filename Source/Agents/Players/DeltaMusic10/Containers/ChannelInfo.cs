/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.DeltaMusic10.Containers
{
	/// <summary>
	/// Contains work information for one channel
	/// </summary>
	internal class ChannelInfo
	{
		public IChannel Hardware;
		public Instrument SoundData;
		public ushort Period;
		public byte[] SoundTable;
		public byte SoundTableCounter;
		public byte SoundTableDelay;
		public Track[] Track;
		public ushort TrackCounter;
		public BlockLine[] Block;
		public uint BlockCounter;
		public byte VibratoWait;
		public byte VibratoLength;
		public byte VibratoPosition;
		public byte VibratoCompare;
		public ushort VibratoFrequency;
		public byte FrequencyData;
		public byte ActualVolume;
		public byte AttackDelay;
		public byte DecayDelay;
		public ushort Sustain;
		public byte ReleaseDelay;
		public byte PlaySpeed;
		public short BendRateFrequency;
		public sbyte Transpose;
		public byte Status;
		public byte ArpeggioCounter;
		public Effect EffectNumber;
		public byte EffectData;
		public bool RetriggerSound;
	}
}
