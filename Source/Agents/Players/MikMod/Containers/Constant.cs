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

		public const int Pan_Left = 0;
		public const int Pan_HalfLeft = 64;
		public const int Pan_Center = 128;
		public const int Pan_HalfRight = 192;
		public const int Pan_Right = 255;
		public const int Pan_Surround = 512;		// Panning value for Dolby Surround

		public const int LogFac = 2 * 16;

		public const int SlBufSize = 2048;			// Size of the loader buffer in words

		public const int Pos_None = -2;				// No loop position defined
		public const ushort Last_Pattern = 0xffff;	// Special 'end of song' pattern
	}
}
