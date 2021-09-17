/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Event;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers
{
	/// <summary>
	/// An instance of this structure is used to transport emulator
	/// settings to and from the interface class
	/// </summary>
	internal class Sid2Config
	{
		public Sid2Clock ClockDefault;			// Intended tune speed when unknown
		public bool ClockForced;
		public Sid2Clock ClockSpeed;			// User requested emulation speed
		public Sid2Env Environment;
		public bool ForceDualSids;
		public bool EmulateStereo;
		public uint Frequency;
		public byte Optimization;
		public Sid2PlayBack PlayBack;
		public int Precision;
		public Sid2Model SidDefault;			// Intended SID model when unknown
		public ISidUnknown SidEmulation;
		public Sid2Model SidModel;				// User requested SID model
		public bool SidSamples;
		public uint LeftVolume;
		public uint RightVolume;
		public Sid2Sample SampleFormat;
		public ushort PowerOnDelay;
		public uint Sid2CrcCount;				// Max SID writes to form CRC
		public IEvent SidFirstAccess;
	}
}
