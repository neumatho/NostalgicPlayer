/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Shared.MikMod.Containers
{
	/// <summary>
	/// Different constants
	/// </summary>
	public static class SharedConstant
	{
		/// <summary>
		/// Number of notes in an octave
		/// </summary>
		public const int Octave = 12;

		/// <summary>
		/// Number of above-range octaves
		/// </summary>
		public const int HighOctave = 2;

		/// <summary></summary>
		public const int InstNotes = 120;
		/// <summary></summary>
		public const int EnvPoints = 32;

		/// <summary></summary>
		public const int Pan_Left = 0;
		/// <summary></summary>
		public const int Pan_HalfLeft = 64;
		/// <summary></summary>
		public const int Pan_Center = 128;
		/// <summary></summary>
		public const int Pan_HalfRight = 192;
		/// <summary></summary>
		public const int Pan_Right = 255;
		/// <summary>
		/// Panning value for Dolby Surround
		/// </summary>
		public const int Pan_Surround = 512;

		/// <summary></summary>
		public const int UF_MaxChan = 64;

		/// <summary></summary>
		public const int UF_MaxMacro = 0x10;

		/// <summary></summary>
		public const int UF_MaxFilter = 0x100;

		/// <summary></summary>
		public const byte Filt_Cut = 0x80;

		/// <summary></summary>
		public const byte Filt_Resonant = 0x81;

		/// <summary>
		/// Special 'end of song' pattern
		/// </summary>
		public const ushort Last_Pattern = 0xffff;
	}
}
