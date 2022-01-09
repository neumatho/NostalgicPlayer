/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.Ahx
{
	/// <summary>
	/// Responsible for generation of different wave forms
	/// </summary>
	internal class AhxWaves
	{
		/// <summary>
		/// Restructured how wave forms are stored in memory by Thomas Neumann
		/// </summary>
		public class Waves
		{
			public readonly sbyte[] Sawtooths = new sbyte[0x04 + 0x08 + 0x10 + 0x20 + 0x40 + 0x80];
			public readonly sbyte[] Triangles = new sbyte[0x04 + 0x08 + 0x10 + 0x20 + 0x40 + 0x80];
			public readonly sbyte[] Squares = new sbyte[0x80 * 0x20];
			public readonly sbyte[] WhiteNoiseBig = new sbyte[0x280 * 3];
		}

		public readonly Waves[] filterSets = Helpers.InitializeArray<Waves>(31 + 1 + 31);

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AhxWaves()
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

			GenerateWhiteNoise(filterSets[31].WhiteNoiseBig, 0x280 * 3);

			GenerateFilterWaveforms();
		}



		/********************************************************************/
		/// <summary>
		/// Generates a sawtooth wave table
		/// </summary>
		/********************************************************************/
		private void GenerateSawtooth(sbyte[] buffer, int offset, int len)
		{
			int edi = offset;
			int ebx = 256 / (len - 1);
			int eax = -128;

			for (int ecx = 0; ecx < len; ecx++)
			{
				buffer[edi++] = (sbyte)eax;
				eax += ebx;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Generates a triangle wave table
		/// </summary>
		/********************************************************************/
		private void GenerateTriangle(sbyte[] buffer, int offset, int len)
		{
			int d2 = len;
			int d5 = d2 >> 2;
			int d1 = 128 / d5;
			int d4 = -(d2 >> 1);
			int edi = offset;
			int eax = 0;

			for (int ecx = 0; ecx < d5; ecx++)
			{
				buffer[edi++] = (sbyte)eax;
				eax += d1;
			}

			buffer[edi++] = 0x7f;

			if (d5 != 1)
			{
				eax = 128;

				for (int ecx = 0; ecx < d5 - 1; ecx++)
				{
					eax -= d1;
					buffer[edi++] = (sbyte)eax;
				}
			}

			int esi = edi + d4;
			for (int ecx = 0; ecx < d5 * 2; ecx++)
			{
				buffer[edi++] = buffer[esi++];

				if (buffer[edi - 1] == 0x7f)
					buffer[edi - 1] = -128;
				else
					buffer[edi - 1] = (sbyte)-buffer[edi - 1];
			}
		}



		/********************************************************************/
		/// <summary>
		/// Generates a square wave table
		/// </summary>
		/********************************************************************/
		private void GenerateSquare(sbyte[] buffer)
		{
			int edi = 0;

			for (int ebx = 1; ebx <= 0x20; ebx++)
			{
				for (int ecx = 0; ecx < (0x40 - ebx) * 2; ecx++)
					buffer[edi++] = -128;

				for (int ecx = 0; ecx < ebx * 2; ecx++)
					buffer[edi++] = 127;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Generates a white noise wave table
		/// </summary>
		/********************************************************************/
		private void GenerateWhiteNoise(sbyte[] buffer, int len)
		{
			Array.Copy(Tables.WhiteNoiseTable, 0, buffer, 0, len);
		}



		/********************************************************************/
		/// <summary>
		/// Generates the filter waveform tables
		/// </summary>
		/********************************************************************/
		private void GenerateFilterWaveforms()
		{
			Waves src = filterSets[31];

			for (int temp = 0, freq = 8; temp < 31; temp++, freq += 3)
			{
				float fre = freq * 1.25f / 100.0f;

				GenerateFilter(src.Sawtooths, Tables.OffsetTable[0], 0x04, fre, filterSets[temp].Sawtooths, filterSets[temp + 32].Sawtooths);
				GenerateFilter(src.Sawtooths, Tables.OffsetTable[1], 0x08, fre, filterSets[temp].Sawtooths, filterSets[temp + 32].Sawtooths);
				GenerateFilter(src.Sawtooths, Tables.OffsetTable[2], 0x10, fre, filterSets[temp].Sawtooths, filterSets[temp + 32].Sawtooths);
				GenerateFilter(src.Sawtooths, Tables.OffsetTable[3], 0x20, fre, filterSets[temp].Sawtooths, filterSets[temp + 32].Sawtooths);
				GenerateFilter(src.Sawtooths, Tables.OffsetTable[4], 0x40, fre, filterSets[temp].Sawtooths, filterSets[temp + 32].Sawtooths);
				GenerateFilter(src.Sawtooths, Tables.OffsetTable[5], 0x80, fre, filterSets[temp].Sawtooths, filterSets[temp + 32].Sawtooths);

				GenerateFilter(src.Triangles, Tables.OffsetTable[0], 0x04, fre, filterSets[temp].Triangles, filterSets[temp + 32].Triangles);
				GenerateFilter(src.Triangles, Tables.OffsetTable[1], 0x08, fre, filterSets[temp].Triangles, filterSets[temp + 32].Triangles);
				GenerateFilter(src.Triangles, Tables.OffsetTable[2], 0x10, fre, filterSets[temp].Triangles, filterSets[temp + 32].Triangles);
				GenerateFilter(src.Triangles, Tables.OffsetTable[3], 0x20, fre, filterSets[temp].Triangles, filterSets[temp + 32].Triangles);
				GenerateFilter(src.Triangles, Tables.OffsetTable[4], 0x40, fre, filterSets[temp].Triangles, filterSets[temp + 32].Triangles);
				GenerateFilter(src.Triangles, Tables.OffsetTable[5], 0x80, fre, filterSets[temp].Triangles, filterSets[temp + 32].Triangles);

				GenerateFilter(src.WhiteNoiseBig, 0, 0x280 * 3, fre, filterSets[temp].WhiteNoiseBig, filterSets[temp + 32].WhiteNoiseBig);

				for (int i = 0; i < 32; i++)
					GenerateFilter(src.Squares, i * 0x80, 0x80, fre, filterSets[temp].Squares, filterSets[temp + 32].Squares);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Generates the filter on a single waveform
		/// </summary>
		/********************************************************************/
		private void GenerateFilter(sbyte[] input, int offset, int len, float fre, sbyte[] outputLow, sbyte[] outputHigh)
		{
			float mid = 0.0f;
			float low = 0.0f;

			for (int i = 0; i < len; i++)
			{
				float high = input[offset + i] - mid - low;
				Clip(ref high);

				mid += high * fre;
				Clip(ref mid);

				low += mid * fre;
				Clip(ref low);
			}

			for (int i = 0; i < len; i++)
			{
				float high = input[offset + i] - mid - low;
				Clip(ref high);

				mid += high * fre;
				Clip(ref mid);

				low += mid * fre;
				Clip(ref low);

				outputLow[offset + i] = (sbyte)low;
				outputHigh[offset + i] = (sbyte)high;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Test for bounds values
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Clip(ref float x)
		{
			if (x > 127.0f)
			{
				x = 127.0f;
				return;
			}

			if (x < -128.0f)
				x = -128.0f;
		}
		#endregion
	}
}
