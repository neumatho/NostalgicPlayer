/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.ModTracker.Containers
{
	/// <summary>
	/// The different effects
	/// </summary>
	internal enum ExtraEffect : byte
	{
		SetFilter = 0x00,			// 0xe0
		FineSlideUp = 0x10,			// 0xe1
		FineSlideDown = 0x20,		// 0xe2
		GlissandoCtrl = 0x30,		// 0xe3
		VibratoWaveform = 0x40,		// 0xe4
		SetFineTune = 0x50,			// 0xe5
		JumpToLoop = 0x60,			// 0xe6
		TremoloWaveform = 0x70,		// 0xe7
		KarplusStrong = 0x80,		// 0xe8
		Retrig = 0x90,				// 0xe9
		FineVolSlideUp = 0xa0,		// 0xea
		FineVolSlideDown = 0xb0,	// 0xeb
		NoteCut = 0xc0,				// 0xec
		NoteDelay = 0xd0,			// 0xed
		PatternDelay = 0xe0,		// 0xee
		InvertLoop = 0xf0			// 0xef
	}
}
