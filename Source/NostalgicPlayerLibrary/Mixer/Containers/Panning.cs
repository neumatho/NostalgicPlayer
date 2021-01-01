/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.NostalgicPlayerLibrary.Mixer.Containers
{
	/// <summary>
	/// This enum only holds some points in the panning
	/// </summary>
	internal enum Panning
	{
		Left = 0,
		Center = 128,
		Right = 256,
		Surround = 512		// Panning value for Dolby Surround
	}
}
