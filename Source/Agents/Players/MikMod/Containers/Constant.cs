/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.MikMod.Containers
{
	/// <summary>
	/// Different constants
	/// </summary>
	internal static class Constant
	{
		public const int HighOctave = 2;			// Number of above-range octaves
		public const int Octave = 12;				// Number of notes in an octave

		public const int LogFac = 2 * 16;

		public const int SlBufSize = 2048;			// Size of the loader buffer in words

		public const int Pos_None = -2;				// No loop position defined
	}
}
