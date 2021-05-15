/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Shared.MikMod.Containers
{
	/// <summary>
	/// Module flags (UF_)
	/// </summary>
	[Flags]
	public enum ModuleFlag : ushort
	{
		/// <summary></summary>
		None = 0,

		/// <summary>
		/// XM periods / fine tuning
		/// </summary>
		XmPeriods = 0x0001,

		/// <summary>
		/// Linear periods (XmPeriods must be set)
		/// </summary>
		Linear = 0x0002,

		/// <summary>
		/// Instruments are used
		/// </summary>
		Inst = 0x0004,

		/// <summary>
		/// New note actions used (set NumVoices rather than NumChn)
		/// </summary>
		Nna = 0x0008,

		/// <summary>
		/// Uses old S3M volume slides
		/// </summary>
		S3MSlides = 0x0010,

		/// <summary>
		/// Continue volume slides in the background
		/// </summary>
		BgSlides = 0x0020,

		/// <summary>
		/// Can use >255 BPM
		/// </summary>
		HighBpm = 0x0040,

		/// <summary>
		/// XM-type (i.e. illogical) pattern break semantics
		/// </summary>
		NoWrap = 0x0080,

		/// <summary>
		/// IT: Need arpeggio memory
		/// </summary>
		ArpMem = 0x0100,

		/// <summary>
		/// Emulate some FT2 replay quirks
		/// </summary>
		Ft2Quirks = 0x0200,

		/// <summary>
		/// Module uses panning effects or have non-tracker default initial panning
		/// </summary>
		Panning = 0x0400,

		/// <summary>
		/// Module uses Farandole tempo calculations
		/// </summary>
		FarTempo = 0x0800
	}
}
