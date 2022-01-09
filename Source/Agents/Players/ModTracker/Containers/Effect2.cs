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
	/// The different effects
	/// </summary>
	internal enum Effect2 : byte
	{
		Arpeggio = 0x00,				// 0x00
		SlideUp,						// 0x01
		SlideDown,						// 0x02
		ModulateVolume,					// 0x03
		ModulatePeriod,					// 0x04
		ModulateVolumePeriod,			// 0x05
		ModulateVolumeSlideUp,			// 0x06
		ModulatePeriodSlideUp,			// 0x07
		ModulateVolumePeriodSlideUp,	// 0x08
		ModulateVolumeSlideDown,		// 0x09
		ModulatePeriodSlideDown,		// 0x0a
		ModulateVolumePeriodSlideDown,	// 0x0b
		SetVolume,						// 0x0c
		VolumeSlide,					// 0x0d
		AutoVolumeSlide,				// 0x0e
		SetSpeed						// 0x0f
	}
}
