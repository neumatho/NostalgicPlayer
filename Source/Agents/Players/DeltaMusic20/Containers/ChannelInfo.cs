﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.DeltaMusic20.Containers
{
	/// <summary>
	/// Contains work information for one channel
	/// </summary>
	internal class ChannelInfo : IDeepCloneable<ChannelInfo>
	{
		public Instrument Instrument;
		public Track[] Track;
		public BlockLine[] Block;
		public short CurrentTrackPosition;
		public ushort NextTrackPosition;
		public ushort BlockPosition;
		public byte SoundTableDelay;
		public byte SoundTablePosition;
		public ushort FinalPeriod;
		public ushort Period;
		public byte Note;
		public byte MaxVolume;
		public short PitchBend;
		public short ActualVolume;
		public ushort VolumePosition;
		public byte VolumeSustain;
		public byte Portamento;
		public bool VibratoDirection;
		public ushort VibratoPeriod;
		public byte VibratoDelay;
		public sbyte Transpose;
		public sbyte[] Arpeggio;
		public ushort ArpeggioPosition;
		public byte VibratoPosition;
		public byte VibratoSustain;
		public ushort TrackLoopPosition;
		public ushort TrackLength;
		public bool RetriggerSound;

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
