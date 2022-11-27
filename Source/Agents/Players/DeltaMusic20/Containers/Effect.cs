/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DeltaMusic20.Containers
{
	/// <summary>
	/// The different effects
	/// </summary>
	internal enum Effect
	{
		None = 0x00,						// 00
		SetSpeed,							// 01
		SetFilter,							// 02
		SetBendRateUp,						// 03
		SetBendRateDown,					// 04
		SetPortamento,						// 05
		SetVolume,							// 06
		SetGlobalVolume,					// 07
		SetArp								// 08
	}
}