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
	internal class ChannelInfo : IDeepCloneable<ChannelInfo>
	{
		public Instrument SoundData { get; set; }
		public ushort Period { get; set; }
		public byte[] SoundTable { get; set; }
		public byte SoundTableCounter { get; set; }
		public byte SoundTableDelay { get; set; }
		public Track[] Track { get; set; }
		public ushort TrackCounter { get; set; }
		public BlockLine[] Block { get; set; }
		public uint BlockCounter { get; set; }
		public byte VibratoWait { get; set; }
		public byte VibratoLength { get; set; }
		public byte VibratoPosition { get; set; }
		public byte VibratoCompare { get; set; }
		public ushort VibratoFrequency { get; set; }
		public byte FrequencyData { get; set; }
		public byte ActualVolume { get; set; }
		public byte AttackDelay { get; set; }
		public byte DecayDelay { get; set; }
		public ushort Sustain { get; set; }
		public byte ReleaseDelay { get; set; }
		public byte PlaySpeed { get; set; }
		public short BendRateFrequency { get; set; }
		public sbyte Transpose { get; set; }
		public byte Status { get; set; }
		public byte ArpeggioCounter { get; set; }
		public Effect EffectNumber { get; set; }
		public byte EffectData { get; set; }
		public bool RetriggerSound { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public ChannelInfo MakeDeepClone()
		{
			return (ChannelInfo)MemberwiseClone();
		}
	}
}
