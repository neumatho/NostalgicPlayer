/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.DeltaMusic20.Containers
{
	/// <summary>
	/// Contains work information for one channel
	/// </summary>
	internal class ChannelInfo : IDeepCloneable<ChannelInfo>
	{
		public Instrument Instrument { get; set; }
		public Track[] Track { get; set; }
		public BlockLine[] Block { get; set; }
		public short CurrentTrackPosition { get; set; }
		public ushort NextTrackPosition { get; set; }
		public ushort BlockPosition { get; set; }
		public byte SoundTableDelay { get; set; }
		public byte SoundTablePosition { get; set; }
		public ushort FinalPeriod { get; set; }
		public ushort Period { get; set; }
		public byte Note { get; set; }
		public byte MaxVolume { get; set; }
		public short PitchBend { get; set; }
		public short ActualVolume { get; set; }
		public ushort VolumePosition { get; set; }
		public byte VolumeSustain { get; set; }
		public byte Portamento { get; set; }
		public bool VibratoDirection { get; set; }
		public ushort VibratoPeriod { get; set; }
		public byte VibratoDelay { get; set; }
		public sbyte Transpose { get; set; }
		public sbyte[] Arpeggio { get; set; }
		public ushort ArpeggioPosition { get; set; }
		public byte VibratoPosition { get; set; }
		public byte VibratoSustain { get; set; }
		public ushort TrackLoopPosition { get; set; }
		public ushort TrackLength { get; set; }
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
