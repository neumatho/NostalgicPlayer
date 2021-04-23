﻿/******************************************************************************/
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
	/// Different constants
	/// </summary>
	public static class SharedConstant
	{
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

		/// <summary>
		/// Special 'end of song' pattern
		/// </summary>
		public const ushort Last_Pattern = 0xffff;
	}
}