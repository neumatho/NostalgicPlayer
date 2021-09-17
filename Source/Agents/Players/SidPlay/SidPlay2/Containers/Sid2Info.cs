/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Event;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers
{
	internal class Sid2Info
	{
		public uint Channels;
		public double CpuFrequency;
		public ushort DriverAddr;
		public ushort DriverLength;
		public SidTuneInfo TuneInfo;

		// Load, config and stop calls will reset this
		// and remove all pending events! 10th sec resolution
		public IEventContext EventContext;
		public uint MaxSids;
		public Sid2Env Environment;
		public ushort PowerOnDelay;
		public uint Sid2Crc;
		public uint Sid2CrcCount;				// Number of SID writes forming CRC
	}
}
