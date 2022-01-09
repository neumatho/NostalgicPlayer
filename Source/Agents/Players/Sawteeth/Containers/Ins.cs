/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Sawteeth.Containers
{
	/// <summary>
	/// Ins structure
	/// </summary>
	internal class Ins
	{
		public string Name;

		public InsStep[] Steps;

		// Filter - Amp
		public TimeLev[] Amp;
		public TimeLev[] Filter;

		public byte AmpPoints;
		public byte FilterPoints;

		public byte FilterMode;
		public byte ClipMode;
		public byte Boost;

		public byte Sps;				// PAL-screen per step
		public byte Res;				// Resonance

		public byte VibS;
		public byte VibD;

		public byte PwmS;
		public byte PwmD;

		public byte Loop;
		public byte Len;
	}
}
