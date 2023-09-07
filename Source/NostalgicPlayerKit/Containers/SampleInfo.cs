/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
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
		public enum SampleFlag
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
			Synthesis,

			/// <summary>
			/// Normal sample used as waveform when generating synthesis sound
			/// </summary>
			Hybrid
		}

		/// <summary>
		/// The supported sample sizes
		/// </summary>
		public enum SampleSize : byte
		{
			/// <summary></summary>
			_8Bit = 8,

			/// <summary></summary>
			_16Bit = 16
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
			public Array[] Sample;

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

		private Array sample;
		private Array secondSample;
		private Array[][] multiOctaveAllSamples;
		private MultiOctaveInfo[] multiOctaveSamples;

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
		public SampleFlag Flags
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
		/// Holds the number of bits per sample used
		/// </summary>
		/********************************************************************/
		public SampleSize BitSize
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the frequency of the middle C
		/// </summary>
		/********************************************************************/
		public uint MiddleC => NoteFrequencies == null ? 8287 : NoteFrequencies[5 * 12];



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
		public Array Sample
		{
			get => sample;

			set
			{
				if ((value != null) && (value.GetType() != typeof(sbyte[])) && (value.GetType() != typeof(short[])) && (value.GetType() != typeof(byte[])))
					throw new ArgumentException("Type of array must be either sbyte[], short[] or byte[]", nameof(value));

				sample = value;
			}
		}



		/********************************************************************/
		/// <summary>
		/// If the sample is a stereo sample, this holds the right channel
		/// sample data
		/// </summary>
		/********************************************************************/
		public Array SecondSample
		{
			get => secondSample;

			set
			{
				if ((value != null) && (value.GetType() != typeof(sbyte[])) && (value.GetType() != typeof(short[])) && (value.GetType() != typeof(byte[])))
					throw new ArgumentException("Type of array must be either sbyte[], short[] or byte[]", nameof(value));

				secondSample = value;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Holds the number of samples into the sample array where the real
		/// data starts
		/// </summary>
		/********************************************************************/
		public uint SampleOffset
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
			get => multiOctaveSamples;

			set
			{
				if (value != null)
				{
					foreach (MultiOctaveInfo info in value)
					{
						if ((info.Sample != null) && (info.Sample.GetType() != typeof(sbyte[][])) && (info.Sample.GetType() != typeof(short[][])) && (value.GetType() != typeof(byte[][])))
							throw new ArgumentException("Type of array must be either sbyte[][], short[][] or byte[][]", nameof(value));
					}
				}

				multiOctaveSamples = value;
			}
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
		public Array[][] MultiOctaveAllSamples
		{
			get => multiOctaveAllSamples;

			set
			{
				if (value != null)
				{
					if ((value.GetType() != typeof(sbyte[][][])) && (value.GetType() != typeof(short[][][])) && (value.GetType() != typeof(byte[][][])))
						throw new ArgumentException("Type of array must be either sbyte[][][], short[][][] or byte[][][]", nameof(value));
				}

				multiOctaveAllSamples = value;
			}
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
