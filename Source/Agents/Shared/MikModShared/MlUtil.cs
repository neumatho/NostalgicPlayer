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

		/// <summary>
		/// For removing empty channels
		/// </summary>
		public byte[] remap = new byte[SharedConstant.UF_MaxChan];

		/// <summary>
		/// Lookup table for pattern jumps after blank pattern removal
		/// </summary>
		public byte[] posLookup;

		/// <summary></summary>
		public ushort posLookupCnt;
		/// <summary></summary>
		public ushort[] origPositions = null;

		/// <summary>
		/// Resonant filters in use
		/// </summary>
		public bool filters;

		/// <summary>
		/// Active midi macro number for Sxx,xx<80h
		/// </summary>
		public byte activeMacro;

		/// <summary>
		/// Midi macro settings
		/// </summary>
		public byte[] filterMacros = new byte[SharedConstant.UF_MaxMacro];

		/// <summary>
		/// Computed filter settings
		/// </summary>
		public Filter[] filterSettings = new Filter[SharedConstant.UF_MaxFilter];

		/// <summary>
		/// Remap value for linear period modules
		/// </summary>
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
		public void CreateOrders(Module of, bool curious)
		{
			int curi = curious ? 1 : 0;

			of.NumPos = 0;

			Array.Clear(of.Positions, 0, posLookupCnt);
			Array.Fill<byte>(posLookup, 255, 0, 256);

			for (int t = 0; t < posLookupCnt; t++)
			{
				int order = origPositions[t];
				if (order == 255)
					order = SharedConstant.Last_Pattern;

				of.Positions[of.NumPos] = (ushort)order;
				posLookup[t] = (byte)of.NumPos;		// Bug fix for freaky S3Ms / ITs

				if (origPositions[t] < 254)
					of.NumPos++;
				else
				{
					// End of song special order
					if ((order == SharedConstant.Last_Pattern) && ((curi--) == 0))
						break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Convert a S3M/IT effect command to a unimod command
		/// </summary>
		/********************************************************************/
		public void ProcessCmd(Module of, byte cmd, byte inf, ProcessFlags flags, MUniTrk uniTrk)
		{
			byte lo = (byte)(inf & 0xf);

			// Process S3M / IT specific command structure
			if (cmd != 255)
			{
				switch (cmd)
				{
					// Axx set speed to xx
					case 1:
					{
						uniTrk.UniEffect(Command.UniS3MEffectA, inf);
						break;
					}

					// Bxx position jump
					case 2:
					{
						if (inf < posLookupCnt)
						{
							// Switch to curious mode if necessary, for example
							// sympex.it, deep joy.it
							if (((sbyte)posLookup[inf] < 0) && (origPositions[inf] != 255))
								CreateOrders(of, true);

							if (!((sbyte)posLookup[inf] < 0))
								uniTrk.UniPtEffect(0xb, posLookup[inf], of.Flags);
						}
						break;
					}

					// Cxx pattern break to row xx
					case 3:
					{
						if (((flags & ProcessFlags.OldStyle) != 0) && (((flags & ProcessFlags.It) == 0)))
							uniTrk.UniPtEffect(0xd, (byte)(((inf >> 4) * 10) + (inf & 0xf)), of.Flags);
						else
							uniTrk.UniPtEffect(0xd, inf, of.Flags);

						break;
					}

					// Dxy volume slide
					case 4:
					{
						uniTrk.UniEffect(Command.UniS3MEffectD, inf);
						break;
					}

					// Exy tone slide down
					case 5:
					{
						uniTrk.UniEffect(Command.UniS3MEffectE, inf);
						break;
					}

					// Fxy tone slide up
					case 6:
					{
						uniTrk.UniEffect(Command.UniS3MEffectF, inf);
						break;
					}

					// Gxx tone portamento, speed xx
					case 7:
					{
						if (((flags & ProcessFlags.OldStyle) != 0))
							uniTrk.UniPtEffect(0x3, inf, of.Flags);
						else
							uniTrk.UniEffect(Command.UniItEffectG, inf);

						break;
					}

					// Hxx vibrato
					case 8:
					{
						if (((flags & ProcessFlags.OldStyle) != 0))
						{
							if ((flags & ProcessFlags.It) != 0)
								uniTrk.UniEffect(Command.UniItEffectH_Old, inf);
							else
								uniTrk.UniEffect(Command.UniS3MEffectH, inf);
						}
						else
							uniTrk.UniEffect(Command.UniItEffectH, inf);

						break;
					}

					// Ixy tremor, ontime x, offtime y
					case 9:
					{
						if (((flags & ProcessFlags.OldStyle) != 0))
							uniTrk.UniEffect(Command.UniS3MEffectI, inf);
						else
							uniTrk.UniEffect(Command.UniItEffectI, inf);

						break;
					}

					// Jxy arpeggio
					case 0xa:
					{
						uniTrk.UniPtEffect(0x0, inf, of.Flags);
						break;
					}

					// Kxy dual command H00 & Dxy
					case 0xb:
					{
						if (((flags & ProcessFlags.OldStyle) != 0))
						{
							if ((flags & ProcessFlags.It) != 0)
								uniTrk.UniEffect(Command.UniItEffectH_Old, 0);
							else
								uniTrk.UniEffect(Command.UniS3MEffectH, 0);
						}
						else
							uniTrk.UniEffect(Command.UniItEffectH, 0);

						uniTrk.UniEffect(Command.UniS3MEffectD, inf);
						break;
					}

					// Lxy dual command G00 & Dxy
					case 0xc:
					{
						if (((flags & ProcessFlags.OldStyle) != 0))
							uniTrk.UniPtEffect(0x3, 0, of.Flags);
						else
							uniTrk.UniEffect(Command.UniItEffectG, 0);

						uniTrk.UniEffect(Command.UniS3MEffectD, inf);
						break;
					}

					// Mxx set channel volume
					case 0xd:
					{
						// Ignore invalid values > 64
						if (inf <= 0x40)
							uniTrk.UniEffect(Command.UniItEffectM, inf);

						break;
					}

					// Nxy slide channel volume
					case 0xe:
					{
						uniTrk.UniEffect(Command.UniItEffectN, inf);
						break;
					}

					// Oxx set sample offset xx00h
					case 0xf:
					{
						uniTrk.UniPtEffect(0x9, inf, of.Flags);
						break;
					}

					// Pxy slide panning commands
					case 0x10:
					{
						uniTrk.UniEffect(Command.UniItEffectP, inf);
						break;
					}

					// Qxy retrig (+ volume slide)
					case 0x11:
					{
						uniTrk.UniWriteByte((byte)Command.UniS3MEffectQ);

						if ((inf != 0) && (lo == 0) && ((flags & ProcessFlags.OldStyle) == 0))
							uniTrk.UniWriteByte(1);
						else
							uniTrk.UniWriteByte(inf);

						break;
					}

					// Rxy tremolo speed x, depth y
					case 0x12:
					{
						uniTrk.UniEffect(Command.UniS3MEffectR, inf);
						break;
					}

					// Sxx special commands
					case 0x13:
					{
						if (inf >= 0xf0)
						{
							// Change resonant filter settings if necessary
							if ((filters) && ((inf & 0xf) != activeMacro))
							{
								activeMacro = (byte)(inf & 0xf);

								for (inf = 0; inf < 0x80; inf++)
									filterSettings[inf].FilterVal = filterMacros[activeMacro];
							}
						}
						else
						{
							// Scream Tracker does not have samples larger than
							// 64 Kb, thus doesn't need the SAx effect
							if (((flags & ProcessFlags.Scream) != 0) && ((inf & 0xf0) == 0xa0))
								break;

							uniTrk.UniEffect(Command.UniItEffectS0, inf);
						}
						break;
					}

					// Txx tempo
					case 0x14:
					{
						if (inf > 0x20)
							uniTrk.UniEffect(Command.UniS3MEffectT, inf);
						else
						{
							if ((flags & ProcessFlags.OldStyle) == 0)
							{
								// IT tempo slide
								uniTrk.UniEffect(Command.UniItEffectT, inf);
							}
						}
						break;
					}

					// Uxy fine vibrato speed x, depth y
					case 0x15:
					{
						if (((flags & ProcessFlags.OldStyle) != 0))
						{
							if ((flags & ProcessFlags.It) != 0)
								uniTrk.UniEffect(Command.UniItEffectU_Old, inf);
							else
								uniTrk.UniEffect(Command.UniS3MEffectU, inf);
						}
						else
							uniTrk.UniEffect(Command.UniItEffectU, inf);

						break;
					}

					// Vxx set global volume
					case 0x16:
					{
						uniTrk.UniEffect(Command.UniXmEffectG, inf);
						break;
					}

					// Wxy global volume slide
					case 0x17:
					{
						uniTrk.UniEffect(Command.UniItEffectW, inf);
						break;
					}

					// Xxx amiga command 8xx
					case 0x18:
					{
						if ((flags & ProcessFlags.OldStyle) != 0)
						{
							if (inf > 128)
								uniTrk.UniEffect(Command.UniItEffectS0, 0x91);		// Surround
							else
								uniTrk.UniPtEffect(0x8, (byte)((inf == 128) ? 255 : (inf << 1)), of.Flags);
						}
						else
							uniTrk.UniPtEffect(0x8, inf, of.Flags);

						break;
					}

					// Yxy panbrello, speed x, depth y
					case 0x19:
					{
						uniTrk.UniEffect(Command.UniItEffectY, inf);
						break;
					}

					// Zxx midi/resonant filters
					case 0x1a:
					{
						if (filterSettings[inf].FilterVal != 0)
						{
							uniTrk.UniWriteByte((byte)Command.UniItEffectZ);
							uniTrk.UniWriteByte(filterSettings[inf].FilterVal);
							uniTrk.UniWriteByte(filterSettings[inf].Inf);
						}
						break;
					}
				}
			}
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
