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
	/// IT / S3M extended SS effects
	/// </summary>
	public enum SsCommand : byte
	{
		/// <summary></summary>
		Glissando = 1,
		/// <summary></summary>
		FineTune,
		/// <summary></summary>
		VibWave,
		/// <summary></summary>
		TremWave,
		/// <summary></summary>
		PanWave,
		/// <summary></summary>
		FrameDelay,
		/// <summary></summary>
		S7Effects,
		/// <summary></summary>
		Panning,
		/// <summary></summary>
		Surround,
		/// <summary></summary>
		HiOffset,
		/// <summary></summary>
		PatLoop,
		/// <summary></summary>
		NoteCut,
		/// <summary></summary>
		NoteDelay,
		/// <summary></summary>
		PatDelay
	}
}
