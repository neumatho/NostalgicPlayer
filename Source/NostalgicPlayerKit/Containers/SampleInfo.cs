/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// This class holds information about a single sample in a module
	/// </summary>
	public class SampleInfo
	{
		/// <summary>
		/// The different flags that can be set for the sample
		/// </summary>
		[Flags]
		public enum SampleFlags
		{
			/// <summary>
			/// Nothing
			/// </summary>
			None = 0x00,

			/// <summary>
			/// The sample is looping
			/// </summary>
			Loop = 0x01,

			/// <summary>
			/// The sample has ping-pong loop (set this together with Loop)
			/// </summary>
			PingPong = 0x02
		}

		/// <summary>
		/// The different types a sample can be
		/// </summary>
		public enum SampleType
		{
			/// <summary>
			/// A normal sample
			/// </summary>
			Sample,

			/// <summary>
			/// Synthesis generated sample
			/// </summary>
			Synth,

			/// <summary>
			/// Normal sample used as waveform when generating synthesis sound
			/// </summary>
			Hybrid
		}

		/********************************************************************/
		/// <summary>
		/// Holds the name of the sample
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
		public SampleFlags Flags
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the type of the sample
		/// </summary>
		/********************************************************************/
		public SampleType Type
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the number of bits per sample used (only 8 and 16 supported)
		/// </summary>
		/********************************************************************/
		public int BitSize
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the frequency of the middle C (C-4)
		/// </summary>
		/********************************************************************/
		public int MiddleC
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the volume of the sample (0-256)
		/// </summary>
		/********************************************************************/
		public int Volume
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the panning value (0-255). -1 means no panning
		/// </summary>
		/********************************************************************/
		public int Panning
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the sample itself
		/// </summary>
		/********************************************************************/
		public sbyte[] Sample
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the length of the sample in samples
		/// </summary>
		/********************************************************************/
		public int Length
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the start offset to the loop point in samples
		/// </summary>
		/********************************************************************/
		public int LoopStart
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the loop length in samples
		/// </summary>
		/********************************************************************/
		public int LoopLength
		{
			get; set;
		}
	}
}
