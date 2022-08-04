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
			PingPong = 0x02,

			/// <summary>
			/// The sample is in stereo
			/// </summary>
			Stereo = 0x04,

			/// <summary>
			/// The sample contains multiple samples for different octaves
			/// </summary>
			MultiOctave = 0x08
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

		/// <summary>
		/// Information about a single octave for multiple octave samples
		/// </summary>
		public struct MultiOctaveInfo
		{
			/// <summary>
			/// The sample itself
			/// 
			/// The first index is the channel number
			/// </summary>
			public sbyte[][] Sample;

			/// <summary>
			/// How many notes to add to the playing note
			/// </summary>
			public int NoteAdd;

			/// <summary>
			/// Holds the start offset to the loop point in samples
			/// </summary>
			public uint LoopStart;

			/// <summary>
			/// Holds the loop length in samples
			/// </summary>
			public uint LoopLength;
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
		public byte BitSize
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the frequency of the middle C (C-4)
		/// </summary>
		/********************************************************************/
		public uint MiddleC
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the volume of the sample (0-256)
		/// </summary>
		/********************************************************************/
		public ushort Volume
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the panning value (0-255). -1 means no panning
		/// </summary>
		/********************************************************************/
		public short Panning
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
		/// If the sample is a stereo sample, this holds the right channel
		/// sample data
		/// </summary>
		/********************************************************************/
		public sbyte[] SecondSample
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// If the sample is a multi octave sample, this property need to be
		/// filled instead of the two above.
		/// 
		/// The index is the octave number (0-7). Note that all 8 indexes
		/// need to be filled out
		/// </summary>
		/********************************************************************/
		public MultiOctaveInfo[] MultiOctaveSamples
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// If the sample is a multi octave sample, this property holds all
		/// the octave samples for each channel. This is needed when saving
		/// the sample, as not all octave samples may be included in the
		/// above property.
		///
		/// The first index is the channel and the second index is the sample
		/// number
		/// </summary>
		/********************************************************************/
		public sbyte[][][] MultiOctaveAllSamples
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the length of the sample in samples for one channel
		/// </summary>
		/********************************************************************/
		public uint Length
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the start offset to the loop point in samples
		/// </summary>
		/********************************************************************/
		public uint LoopStart
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the loop length in samples
		/// </summary>
		/********************************************************************/
		public uint LoopLength
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the frequency for each note for 10 octaves.
		/// 
		/// This is used by some visuals to find out, which note is playing
		/// by the frequency set
		/// </summary>
		/********************************************************************/
		public uint[] NoteFrequencies
		{
			get; set;
		}
	}
}
