/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.QuadraComposer.Containers
{
	/// <summary>
	/// Contains work information for one channel
	/// </summary>
	internal class ChannelInfo
	{
		public TrackLine TrackLine;
		public uint Loop;
		public uint LoopLength;
		public ushort Period;
		public ushort Volume;
		public uint Length;
		public uint Start;
		public short NoteNr;
		public ushort WantedPeriod;
		public bool PortDirection;
		public byte VibratoWave;
		public byte GlissandoControl;
		public byte VibratoCommand;
		public ushort VibratoPosition;
		public byte TremoloWave;
		public byte TremoloCommand;
		public ushort TremoloPosition;
		public byte SampleOffset;
		public byte Retrig;
		public ushort PortSpeed;
		public byte FineTune;
		public sbyte[] SampleData;
	}
}
