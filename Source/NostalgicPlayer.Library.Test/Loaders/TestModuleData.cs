/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Text;

namespace Polycode.NostalgicPlayer.Library.Test.Loaders
{
	/// <summary>
	/// Builds the test module used by the loader/converter tests and holds
	/// the values a player is expected to read back again
	/// </summary>
	internal static class TestModuleData
	{
		/// <summary>
		/// Mark of the original (not converted) module format
		/// </summary>
		public const string OriginalMark = "TST1";

		/// <summary>
		/// Mark of the format the first converter creates
		/// </summary>
		public const string FirstConvertedMark = "CNV2";

		/// <summary>
		/// Mark of the format the second converter creates
		/// </summary>
		public const string SecondConvertedMark = "CNV3";

		/// <summary>
		/// The original format name all the test converters report
		/// </summary>
		public const string OriginalFormat = "Test Original Format";

		/// <summary>
		/// Number of samples stored in the test module
		/// </summary>
		public const int NumberOfSamples = 3;

		/// <summary>
		/// Number of bytes of music data stored before the first sample
		/// </summary>
		public const int SongDataLength = 10;

		/// <summary>
		/// Number of bytes of music data stored between sample 1 and sample 2
		/// </summary>
		public const int MoreSongDataLength = 16;

		/// <summary>
		/// The length of each sample. They are all different on purpose, so a
		/// test cannot pass by accident if the lengths get mixed up
		/// </summary>
		public static readonly int[] SampleLengths = [ 16, 24, 12 ];

		private static readonly byte[] sampleStartValues = [ 0x10, 0x20, 0x40 ];

		private const byte SongDataStartValue = 0xa0;
		private const byte MoreSongDataStartValue = 0xb0;

		/********************************************************************/
		/// <summary>
		/// Build the original module. The data is stored in the order:
		/// music data, sample data, sample data, more music data, sample data
		/// </summary>
		/********************************************************************/
		public static byte[] BuildOriginalModule()
		{
			List<byte> moduleData = new List<byte>();

			moduleData.AddRange(Encoding.ASCII.GetBytes(OriginalMark));

			foreach (int length in SampleLengths)
			{
				// Store the lengths as big endian 16-bit values
				moduleData.Add((byte)(length >> 8));
				moduleData.Add((byte)(length & 0xff));
			}

			moduleData.AddRange(GetSongData(0));
			moduleData.AddRange(GetSampleDataAsBytes(0));
			moduleData.AddRange(GetSampleDataAsBytes(1));
			moduleData.AddRange(GetMoreSongData(0));
			moduleData.AddRange(GetSampleDataAsBytes(2));

			return moduleData.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Return the offset of the music data stored before the first
		/// sample, as a player sees it in the converted module
		/// </summary>
		/********************************************************************/
		public static int GetConvertedSongDataOffset()
		{
			// The header holds the mark followed by the sample lengths as
			// big endian 32-bit values
			return 4 + (NumberOfSamples * 4);
		}



		/********************************************************************/
		/// <summary>
		/// Return the offset of the given sample, as a player sees it in the
		/// converted module
		/// </summary>
		/********************************************************************/
		public static int GetConvertedSampleOffset(int sampleNumber)
		{
			int offset = GetConvertedSongDataOffset() + SongDataLength;

			for (int i = 0; i < sampleNumber; i++)
			{
				offset += SampleLengths[i];

				// The second block of music data is stored between sample 1
				// and sample 2
				if (i == 1)
					offset += MoreSongDataLength;
			}

			return offset;
		}



		/********************************************************************/
		/// <summary>
		/// Return the offset of the music data stored between sample 1 and
		/// sample 2, as a player sees it in the converted module
		/// </summary>
		/********************************************************************/
		public static int GetConvertedMoreSongDataOffset()
		{
			return GetConvertedSampleOffset(1) + SampleLengths[1];
		}



		/********************************************************************/
		/// <summary>
		/// Return the music data stored before the first sample. Each
		/// converter adds one to every music byte, so numberOfConversions
		/// tells how many converters the data has been through
		/// </summary>
		/********************************************************************/
		public static byte[] GetSongData(int numberOfConversions)
		{
			return BuildRunningData(SongDataStartValue + numberOfConversions, SongDataLength);
		}



		/********************************************************************/
		/// <summary>
		/// Return the music data stored between sample 1 and sample 2. Each
		/// converter adds one to every music byte, so numberOfConversions
		/// tells how many converters the data has been through
		/// </summary>
		/********************************************************************/
		public static byte[] GetMoreSongData(int numberOfConversions)
		{
			return BuildRunningData(MoreSongDataStartValue + numberOfConversions, MoreSongDataLength);
		}



		/********************************************************************/
		/// <summary>
		/// Return the data for the given sample. Sample data is never
		/// changed by a converter
		/// </summary>
		/********************************************************************/
		public static sbyte[] GetSampleData(int sampleNumber)
		{
			byte[] data = GetSampleDataAsBytes(sampleNumber);
			sbyte[] sampleData = new sbyte[data.Length];

			for (int i = 0; i < data.Length; i++)
				sampleData[i] = (sbyte)data[i];

			return sampleData;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Return the data for the given sample as unsigned bytes
		/// </summary>
		/********************************************************************/
		private static byte[] GetSampleDataAsBytes(int sampleNumber)
		{
			return BuildRunningData(sampleStartValues[sampleNumber], SampleLengths[sampleNumber]);
		}



		/********************************************************************/
		/// <summary>
		/// Build a block of data where each byte is one higher than the
		/// previous one
		/// </summary>
		/********************************************************************/
		private static byte[] BuildRunningData(int startValue, int length)
		{
			byte[] data = new byte[length];

			for (int i = 0; i < length; i++)
				data[i] = (byte)(startValue + i);

			return data;
		}
		#endregion
	}
}
