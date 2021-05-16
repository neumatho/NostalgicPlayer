/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Agent.Shared.MikMod.Containers;

namespace Polycode.NostalgicPlayer.Agent.Shared.MikMod
{
	/// <summary>
	/// Utility class with helper methods used by different loaders
	/// </summary>
	public class MlUtil
	{
		// S3M/IT variables

		/// <summary></summary>
		public byte[] remap = new byte[SharedConstant.UF_MaxChan];

		/// <summary></summary>
		public int[] noteIndex = null;
		private int noteIndexCount = 0;

		/********************************************************************/
		/// <summary>
		/// Allocates enough memory to hold linear period information
		/// </summary>
		/********************************************************************/
		public bool AllocLinear(Module of)
		{
			if (of.NumSmp > noteIndexCount)
			{
				int[] newNoteIndex = new int[of.NumSmp];

				if (noteIndex != null)
					Array.Copy(noteIndex, newNoteIndex, noteIndexCount);

				noteIndex = newNoteIndex;
				noteIndexCount = of.NumSmp;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Free the linear array memory
		/// </summary>
		/********************************************************************/
		public void FreeLinear()
		{
			noteIndex = null;
			noteIndexCount = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Will convert the speed to a fine tune value
		/// </summary>
		/********************************************************************/
		public int SpeedToFineTune(Module of, uint speed, int sample)
		{
			int tmp;
			int cTmp = 0;
			int note = 1;
			int ft = 0;

			speed >>= 1;
			while ((tmp = (int)GetFrequency(of.Flags, GetLinearPeriod((ushort)(note << 1), 0))) < speed)
			{
				cTmp = tmp;
				note++;
			}

			if (tmp != speed)
			{
				if ((tmp - speed) < (speed - cTmp))
				{
					while (tmp > speed)
						tmp = (int)GetFrequency(of.Flags, GetLinearPeriod((ushort)(note << 1), (uint)(--ft)));
				}
				else
				{
					note--;
					while (cTmp < speed)
						cTmp = (int)GetFrequency(of.Flags, GetLinearPeriod((ushort)(note << 1), (uint)(++ft)));
				}
			}

			noteIndex[sample] = note - 4 * SharedConstant.Octave;

			return ft;
		}



		/********************************************************************/
		/// <summary>
		/// XM linear period to frequency conversion
		/// </summary>
		/********************************************************************/
		public static uint GetFrequency(ModuleFlag flags, uint period)
		{
			if ((flags & ModuleFlag.Linear) != 0)
			{
				int shift = ((int)period / 768) - SharedConstant.HighOctave;

				if (shift >= 0)
					return SharedLookupTables.LinTab[period % 768] >> shift;
				else
					return SharedLookupTables.LinTab[period % 768] << (-shift);
			}
			else
				return (uint)((8363L * 1712L) / (period != 0 ? period : 1));
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the note period and return it
		/// </summary>
		/********************************************************************/
		public static ushort GetLinearPeriod(ushort note, uint fine)
		{
			ushort t = (ushort)(((20L + 2 * SharedConstant.HighOctave) * SharedConstant.Octave + 2 - note) * 32L - (fine >> 1));

			return t;
		}
	}
}
