/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// This class holds information about a single instrument in a module
	/// </summary>
	public class InstrumentInfo
	{
		/// <summary>
		/// The different flags that can be set for the instrument
		/// </summary>
		[Flags]
		public enum InstrumentFlags
		{
			/// <summary>
			/// Nothing
			/// </summary>
			None = 0x00
		}

		/// <summary>
		/// Number of octaves supported
		/// </summary>
		public const int Octaves = 10;

		/// <summary>
		/// Number of notes per octave
		/// </summary>
		public const int NotesPerOctave = 12;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public InstrumentInfo()
		{
			for (int o = 0; o < Octaves; o++)
			{
				for (int n = 0; n < NotesPerOctave; n++)
					Notes[o, n] = -1;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Holds the name of the instrument
		/// </summary>
		/********************************************************************/
		public string Name
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the different flags
		/// </summary>
		/********************************************************************/
		public InstrumentFlags Flags
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the sample numbers for each note (-1 means not used)
		/// </summary>
		/********************************************************************/
		public int[,] Notes
		{
			get;
		} = new int[Octaves, NotesPerOctave];
	}
}
