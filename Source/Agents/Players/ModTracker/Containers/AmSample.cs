/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.ModTracker.Containers
{
	/// <summary>
	/// StarTrekker AM sample structure
	/// </summary>
	internal class AmSample
	{
		public ushort Mark;
		public ushort StartAmp;
		public ushort Attack1Level;
		public ushort Attack1Speed;
		public ushort Attack2Level;
		public ushort Attack2Speed;
		public ushort SustainLevel;
		public ushort DecaySpeed;
		public ushort SustainTime;
		public ushort ReleaseSpeed;
		public ushort Waveform;
		public ushort PitchFall;
		public short VibAmp;
		public ushort VibSpeed;
		public ushort baseFreq;
	}
}
