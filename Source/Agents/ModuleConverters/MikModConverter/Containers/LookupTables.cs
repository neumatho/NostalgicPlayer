/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Shared.MikMod.Containers;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.MikModConverter.Containers
{
	/// <summary>
	/// Holds different lookup tables
	/// </summary>
	internal static class LookupTables
	{
		// MOD format period table
		public static readonly ushort[] NPerTab = new ushort[7 * SharedConstant.Octave]
		{
			// Octaves 6 -> 0
			// C     C#      D     D#      E      F     F#      G     G#      A     A#      B */
			0x6b0, 0x650, 0x5f4, 0x5a0, 0x54c, 0x500, 0x4b8, 0x474, 0x434, 0x3f8, 0x3c0, 0x38a,
			0x358, 0x328, 0x2fa, 0x2d0, 0x2a6, 0x280, 0x25c, 0x23a, 0x21a, 0x1fc, 0x1e0, 0x1c5,
			0x1ac, 0x194, 0x17d, 0x168, 0x153, 0x140, 0x12e, 0x11d, 0x10d, 0x0fe, 0x0f0, 0x0e2,
			0x0d6, 0x0ca, 0x0be, 0x0b4, 0x0aa, 0x0a0, 0x097, 0x08f, 0x087, 0x07f, 0x078, 0x071,
			0x06b, 0x065, 0x05f, 0x05a, 0x055, 0x050, 0x04b, 0x047, 0x043, 0x03f, 0x03c, 0x038,
			0x035, 0x032, 0x02f, 0x02d, 0x02a, 0x028, 0x025, 0x023, 0x021, 0x01f, 0x01e, 0x01c,
			0x01b, 0x019, 0x018, 0x016, 0x015, 0x014, 0x013, 0x012, 0x011, 0x010, 0x00f, 0x00e
		};
	}
}
