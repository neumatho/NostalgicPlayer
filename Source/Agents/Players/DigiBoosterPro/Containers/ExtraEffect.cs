/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers
{
	/// <summary>
	/// The different effects
	/// </summary>
	internal enum ExtraEffect : uint8_t
	{
		FinePortamentoUp = 0x1,				// E1
		FinePortamentoDown,					// E2
		PlayBackwards,						// E3
		ChannelControlA,					// E4
		SetLoop = 0x6,						// E6
		SetSampleOffset,					// E7
		SetPanning,							// E8
		RetrigNote,							// E9
		FineVolumeSlideUp,					// EA
		FineVolumeSlideDown,				// EB
		NoteCut,							// EC
		NoteDelay,							// ED
		PatternDelay						// EE
	}
}
