/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.DeltaMusic20.Containers
{
	/// <summary>
	/// Contains work information for one channel
	/// </summary>
	internal class ChannelInfo
	{
		public IChannel Hardware;
		public Instrument Instrument;
		public Track[] Track;
		public BlockLine[] Block;
		public ushort CurrentTrackPosition;
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
		public bool PositionChangedByUser;
	}
}
