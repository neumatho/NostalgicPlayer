/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Containers
{
	/// <summary>
	/// Song flags
	/// </summary>
	[Flags]
	internal enum MmdFlag
	{
		FilterOn = 0x01,
		JumpingOn = 0x02,
		Jump8th = 0x04,
		InstrsAtt = 0x08,			// Instruments are attached (this is a module)
		VolHex = 0x10,
		StSlide = 0x20,				// SoundTracker mode for slides
		EightChannel = 0x40,		// OctaMED 8 channel song
		SlowHq = 0x80,				// HQ slows playing speed (V2-V4 compatibility)
	}
}
