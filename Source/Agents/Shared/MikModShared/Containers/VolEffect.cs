/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Shared.MikMod.Containers
{
	/// <summary>
	/// IT volume column effects
	/// </summary>
	public enum VolEffect : byte
	{
		/// <summary></summary>
		None,
		/// <summary></summary>
		Volume = 1,
		/// <summary></summary>
		Panning,
		/// <summary></summary>
		VolSlide,
		/// <summary></summary>
		PitchSlideDn,
		/// <summary></summary>
		PitchSlideUp,
		/// <summary></summary>
		Portamento,
		/// <summary></summary>
		Vibrato
	}
}
