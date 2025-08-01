﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Med
{
	/// <summary>
	/// Different tables needed
	/// </summary>
	internal static class Tables
	{
		/********************************************************************/
		/// <summary>
		/// Periods for MED 1.12
		/// </summary>
		/********************************************************************/
		public static readonly ushort[] Periods112 =
		[
			856, 808, 762, 720, 678, 640, 604, 570, 538, 508, 480, 453,
			428, 404, 381, 360, 339, 320, 302, 285, 269, 254, 240, 226,
			214, 202, 190, 180, 170, 160, 151, 143, 135, 127, 120, 113,
			107, 101,  95,  90,  85,  80,  75,  72,  68,  64,  60,  57
		];



		/********************************************************************/
		/// <summary>
		/// Periods for MED 2.00
		/// </summary>
		/********************************************************************/
		public static readonly ushort[] Periods200 =
		[
			856, 808, 762, 720, 678, 640, 604, 570, 538, 508, 480, 453,
			428, 404, 381, 360, 339, 320, 302, 285, 269, 254, 240, 226,
			214, 202, 190, 180, 170, 160, 151, 143, 135, 127, 120, 113,
			214, 202, 190, 180, 170, 160, 151, 143, 135, 127, 120, 113,
			214, 202, 190, 180, 170, 160, 151, 143, 135, 127, 120, 113,
			214, 202, 190, 180, 170, 160, 151, 143, 135, 127, 120, 113
		];



		/********************************************************************/
		/// <summary>
		/// These values are the SoundTracker tempos (approx.)
		/// </summary>
		/********************************************************************/
		public static readonly ushort[] SoundTrackerTempos =
		[
			0x0f00, 2417, 4833, 7250, 9666, 12083, 14500, 16916, 19332, 21436, 24163
		];



		/********************************************************************/
		/// <summary>
		/// Multiple octave shift counts
		/// </summary>
		/********************************************************************/
		public static readonly byte[] ShiftCount =
		[
			4, 3, 2, 1, 1, 0, 2, 2, 1, 1, 0, 0
		];



		/********************************************************************/
		/// <summary>
		/// Multiple octave multiply values
		/// </summary>
		/********************************************************************/
		public static readonly byte[] MultiplyLengthCount =
		[
			15, 7, 3, 1, 1, 0, 3, 3, 1, 1, 0, 0
		];



		/********************************************************************/
		/// <summary>
		/// Multiple octave start note
		/// </summary>
		/********************************************************************/
		public static readonly byte[] OctaveStart =
		[
			12, 12, 12, 12, 24, 24, 0, 12, 12, 24, 24, 36
		];
	}
}
