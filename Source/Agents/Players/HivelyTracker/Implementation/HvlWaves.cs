/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.HivelyTracker.Implementation
{
	/// <summary>
	/// Responsible for generation of different wave forms
	/// </summary>
	internal class HvlWaves
	{
		/// <summary>
		/// Restructured how wave forms are stored in memory by Thomas Neumann
		/// </summary>
		public class Waves
		{
			public readonly sbyte[] Sawtooths = new sbyte[0x04 + 0x08 + 0x10 + 0x20 + 0x40 + 0x80];
			public readonly sbyte[] Triangles = new sbyte[0x04 + 0x08 + 0x10 + 0x20 + 0x40 + 0x80];
			public readonly sbyte[] Squares = new sbyte[0x80 * 0x20];
			public readonly sbyte[] WhiteNoise = new sbyte[0x280 * 3];
		}

		// 31 lowpasses, 1 normal, 31 highpasses
		public readonly Waves[] filterSets = ArrayHelper.InitializeArray<Waves>(31 + 1 + 31);

		public readonly int[] PanningLeft = new int[256];
		public readonly int[] PanningRight = new int[256];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public HvlWaves()
		{
			Generate();
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Generates all the wave tables
		/// </summary>
		/********************************************************************/
		private void Generate()
		{
			GeneratePanningTables();
			GenerateSawtooth(filterSets[31].Sawtooths, Tables.OffsetTable[0], 0x04);
			GenerateSawtooth(filterSets[31].Sawtooths, Tables.OffsetTable[1], 0x08);
			GenerateSawtooth(filterSets[31].Sawtooths, Tables.OffsetTable[2], 0x10);
			GenerateSawtooth(filterSets[31].Sawtooths, Tables.OffsetTable[3], 0x20);
			GenerateSawtooth(filterSets[31].Sawtooths, Tables.OffsetTable[4], 0x40);
			GenerateSawtooth(filterSets[31].Sawtooths, Tables.OffsetTable[5], 0x80);

			GenerateTriangle(filterSets[31].Triangles, Tables.OffsetTable[0], 0x04);
			GenerateTriangle(filterSets[31].Triangles, Tables.OffsetTable[1], 0x08);
			GenerateTriangle(filterSets[31].Triangles, Tables.OffsetTable[2], 0x10);
			GenerateTriangle(filterSets[31].Triangles, Tables.OffsetTable[3], 0x20);
			GenerateTriangle(filterSets[31].Triangles, Tables.OffsetTable[4], 0x40);
			GenerateTriangle(filterSets[31].Triangles, Tables.OffsetTable[5], 0x80);

			GenerateSquare(filterSets[31].Squares);

			GenerateWhiteNoise(filterSets[31].WhiteNoise, 0x280 * 3);

			GenerateFilterWaveforms();
		}



		/********************************************************************/
		/// <summary>
		/// Generates panning tables
		/// </summary>
		/********************************************************************/
		private void GeneratePanningTables()
		{
			double aa = (3.14159265 * 2.0) / 4.0;	// Quarter of the way through the sine wave == top peak
			double ab = 0.0;						// Start of the climb from zero

			for (int i = 0; i < 256; i++)
			{
				PanningLeft[i] = (int)(Math.Sin(aa) * 255.0);
				PanningRight[i] = (int)(Math.Sin(ab) * 255.0);

				aa += (3.14159265 * 2.0 / 4.0) / 256.0;
				ab += (3.14159265 * 2.0 / 4.0) / 256.0;
			}

			PanningLeft[255] = 0;
			PanningRight[0] = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Generates a sawtooth wave table
		/// </summary>
		/********************************************************************/
		private void GenerateSawtooth(sbyte[] buffer, int offset, int len)
		{
			int add = 256 / (len - 1);
			int val = -128;

			for (int i = 0; i < len; i++, val += add)
				buffer[offset++] = (sbyte)val;
		}



		/********************************************************************/
		/// <summary>
		/// Generates a triangle wave table
		/// </summary>
		/********************************************************************/
		private void GenerateTriangle(sbyte[] buffer, int offset, int len)
		{
			int d2 = len;
			int d5 = len >> 2;
			int d1 = 128 / d5;
			int d4 = -(d2 >> 1);
			int val = 0;

			for (int i = 0; i < d5; i++)
			{
				buffer[offset++] = (sbyte)val;
				val += d1;
			}

			buffer[offset++] = 0x7f;

			if (d5 != 1)
			{
				val = 128;

				for (int i = 0; i < d5 - 1; i++)
				{
					val -= d1;
					buffer[offset++] = (sbyte)val;
				}
			}

			int offset2 = offset + d4;
			for (int i = 0; i < d5 * 2; i++)
			{
				sbyte c = buffer[offset2++];

				if (c == 0x7f)
					c = -128;
				else
					c = (sbyte)-c;

				buffer[offset++] = c;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Generates a square wave table
		/// </summary>
		/********************************************************************/
		private void GenerateSquare(sbyte[] buffer)
		{
			int offset = 0;

			for (int i = 1; i <= 0x20; i++)
			{
				for (int j = 0; j < (0x40 - i) * 2; j++)
					buffer[offset++] = -128;

				for (int j = 0; j < i * 2; j++)
					buffer[offset++] = 127;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Generates a white noise wave table
		/// </summary>
		/********************************************************************/
		private void GenerateWhiteNoise(sbyte[] buffer, int len)
		{
			uint ays = 0x41595321;
			int offset = 0;

			do
			{
				byte s = (byte)ays;

				if ((ays & 0x100) != 0)
				{
					s = 0x7f;

					if ((ays & 0x8000) != 0)
						s = 0x80;
				}

				buffer[offset++] = (sbyte)s;
				len--;

				ays = (ays >> 5) | (ays << 27);
				ays = (ays & 0xffffff00) | ((ays & 0xff) ^ 0x9a);
				ushort bx = (ushort)ays;
				ays = (ays << 2) | (ays >> 30);
				ushort ax = (ushort)ays;
				bx += ax;
				ax ^= bx;
				ays = (ays & 0xffff0000) | ax;
				ays = (ays >> 3) | (ays << 29);
			}
			while (len != 0);
		}



		/********************************************************************/
		/// <summary>
		/// Generates the filter waveform tables
		/// </summary>
		/********************************************************************/
		private void GenerateFilterWaveforms()
		{
			Waves src = filterSets[31];
			int filterTableOffset = 0;

			for (int i = 0, freq = 25; i < 31; i++, freq += 9)
			{
				GenerateFilter(src.Sawtooths, Tables.OffsetTable[0], 0x04, freq, ref filterTableOffset, filterSets[i].Sawtooths, filterSets[i + 32].Sawtooths);
				GenerateFilter(src.Sawtooths, Tables.OffsetTable[1], 0x08, freq, ref filterTableOffset, filterSets[i].Sawtooths, filterSets[i + 32].Sawtooths);
				GenerateFilter(src.Sawtooths, Tables.OffsetTable[2], 0x10, freq, ref filterTableOffset, filterSets[i].Sawtooths, filterSets[i + 32].Sawtooths);
				GenerateFilter(src.Sawtooths, Tables.OffsetTable[3], 0x20, freq, ref filterTableOffset, filterSets[i].Sawtooths, filterSets[i + 32].Sawtooths);
				GenerateFilter(src.Sawtooths, Tables.OffsetTable[4], 0x40, freq, ref filterTableOffset, filterSets[i].Sawtooths, filterSets[i + 32].Sawtooths);
				GenerateFilter(src.Sawtooths, Tables.OffsetTable[5], 0x80, freq, ref filterTableOffset, filterSets[i].Sawtooths, filterSets[i + 32].Sawtooths);

				GenerateFilter(src.Triangles, Tables.OffsetTable[0], 0x04, freq, ref filterTableOffset, filterSets[i].Triangles, filterSets[i + 32].Triangles);
				GenerateFilter(src.Triangles, Tables.OffsetTable[1], 0x08, freq, ref filterTableOffset, filterSets[i].Triangles, filterSets[i + 32].Triangles);
				GenerateFilter(src.Triangles, Tables.OffsetTable[2], 0x10, freq, ref filterTableOffset, filterSets[i].Triangles, filterSets[i + 32].Triangles);
				GenerateFilter(src.Triangles, Tables.OffsetTable[3], 0x20, freq, ref filterTableOffset, filterSets[i].Triangles, filterSets[i + 32].Triangles);
				GenerateFilter(src.Triangles, Tables.OffsetTable[4], 0x40, freq, ref filterTableOffset, filterSets[i].Triangles, filterSets[i + 32].Triangles);
				GenerateFilter(src.Triangles, Tables.OffsetTable[5], 0x80, freq, ref filterTableOffset, filterSets[i].Triangles, filterSets[i + 32].Triangles);

				GenerateFilter(src.WhiteNoise, 0, 0x280 * 3, freq, ref filterTableOffset, filterSets[i].WhiteNoise, filterSets[i + 32].WhiteNoise);

				for (int j = 0; j < 32; j++)
					GenerateFilter(src.Squares, j * 0x80, 0x80, freq, ref filterTableOffset, filterSets[i].Squares, filterSets[i + 32].Squares);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Generates the filter on a single waveform
		/// </summary>
		/********************************************************************/
		private void GenerateFilter(sbyte[] input, int offset, int len, int freq, ref int filterTableOffset, sbyte[] outputLow, sbyte[] outputHigh)
		{
			int mid = Tables.MidFilterTable[filterTableOffset] << 8;
			int low = Tables.LowFilterTable[filterTableOffset++] << 8;

			for (int i = 0; i < len; i++)
			{
				int @in = input[offset + i] << 16;
				int high = ClipShifted8(@in - mid - low);

				int fre = (high >> 8) * freq;
				mid = ClipShifted8(mid + fre);

				fre = (mid >> 8) * freq;
				low = ClipShifted8(low + fre);

				outputHigh[offset + i] = (sbyte)(high >> 16);
				outputLow[offset + i] = (sbyte)(low >> 16);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Test for bounds values
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int ClipShifted8(int @in)
		{
			short top = (short)(@in >> 16);

			if (top > 127)
				@in = 127 << 16;
			else if (top < -128)
				@in = -(128 << 16);

			return @in;
		}
		#endregion
	}
}
