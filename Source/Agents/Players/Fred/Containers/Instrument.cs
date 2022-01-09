/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Fred.Containers
{
	/// <summary>
	/// Holds information about a single instrument
	/// </summary>
	internal class Instrument
	{
		public string Name;
		public ushort RepeatLen;
		public ushort Length;
		public ushort Period;
		public byte VibDelay;
		public sbyte VibSpeed;
		public sbyte VibAmpl;
		public byte EnvVol;
		public byte AttackSpeed;
		public byte AttackVolume;
		public byte DecaySpeed;
		public byte DecayVolume;
		public byte SustainDelay;
		public byte ReleaseSpeed;
		public byte ReleaseVolume;
		public sbyte[] Arpeggio = new sbyte[16];
		public byte ArpSpeed;
		public InstrumentType InstType;
		public sbyte PulseRateMin;
		public sbyte PulseRatePlus;
		public byte PulseSpeed;
		public byte PulseStart;
		public byte PulseEnd;
		public byte PulseDelay;
		public SynchronizeFlag InstSync;
		public byte Blend;
		public byte BlendDelay;
		public byte PulseShotCounter;
		public byte BlendShotCounter;
		public byte ArpCount;

		public sbyte[] SampleAddr;
	}
}
