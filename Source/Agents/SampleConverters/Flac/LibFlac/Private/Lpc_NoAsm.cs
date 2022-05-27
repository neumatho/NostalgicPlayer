/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Diagnostics;

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Private
{
	/// <summary>
	/// 
	/// </summary>
	internal class Lpc_NoAsm : ILpc
	{
		/********************************************************************/
		/// <summary>
		/// Generic 32-bit datapath
		/// </summary>
		/********************************************************************/
		public void Restore_Signal(Flac__int32[] residual, uint32_t data_Len, Flac__int32[] qlp_Coeff, uint32_t order, int lp_Quantization, Flac__int32[] data, uint32_t data_Offset)
		{
			Debug.Assert(order > 0);
			Debug.Assert(order <= 32);

			// We do unique versions up to 12th order since that's the subset limit.
			// Also they are roughly ordered to match frequency of occurrence to
			// minimize branching
			if (order <= 12)
			{
				if (order > 8)
				{
					if (order > 10)
					{
						if (order == 12)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[11] * data[data_Offset + i - 12];
								sum += qlp_Coeff[10] * data[data_Offset + i - 11];
								sum += qlp_Coeff[9] * data[data_Offset + i - 10];
								sum += qlp_Coeff[8] * data[data_Offset + i - 9];
								sum += qlp_Coeff[7] * data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								data[data_Offset + i] = residual[i] + (sum >> lp_Quantization);
							}
						}
						else	// order == 11
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[10] * data[data_Offset + i - 11];
								sum += qlp_Coeff[9] * data[data_Offset + i - 10];
								sum += qlp_Coeff[8] * data[data_Offset + i - 9];
								sum += qlp_Coeff[7] * data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								data[data_Offset + i] = residual[i] + (sum >> lp_Quantization);
							}
						}
					}
					else
					{
						if (order == 10)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[9] * data[data_Offset + i - 10];
								sum += qlp_Coeff[8] * data[data_Offset + i - 9];
								sum += qlp_Coeff[7] * data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								data[data_Offset + i] = residual[i] + (sum >> lp_Quantization);
							}
						}
						else	// order == 9
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[8] * data[data_Offset + i - 9];
								sum += qlp_Coeff[7] * data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								data[data_Offset + i] = residual[i] + (sum >> lp_Quantization);
							}
						}
					}
				}
				else if (order > 4)
				{
					if (order > 6)
					{
						if (order == 8)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[7] * data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								data[data_Offset + i] = residual[i] + (sum >> lp_Quantization);
							}
						}
						else	// order == 7
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[6] * data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								data[data_Offset + i] = residual[i] + (sum >> lp_Quantization);
							}
						}
					}
					else
					{
						if (order == 6)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								data[data_Offset + i] = residual[i] + (sum >> lp_Quantization);
							}
						}
						else	// order == 5
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								data[data_Offset + i] = residual[i] + (sum >> lp_Quantization);
							}
						}
					}
				}
				else
				{
					if (order > 2)
					{
						if (order == 4)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								data[data_Offset + i] = residual[i] + (sum >> lp_Quantization);
							}
						}
						else	// order == 3
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								data[data_Offset + i] = residual[i] + (sum >> lp_Quantization);
							}
						}
					}
					else
					{
						if (order == 2)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								data[data_Offset + i] = residual[i] + (sum >> lp_Quantization);
							}
						}
						else	// order == 1
						{
							for (int i = 0; i < data_Len; i++)
								data[data_Offset + i] = residual[i] + ((qlp_Coeff[0] * data[data_Offset + i - 1]) >> lp_Quantization);
						}
					}
				}
			}
			else	// order == 12
			{
				for (int i = 0; i < data_Len; i++)
				{
					Flac__int32 sum = 0;

					switch (order)
					{
						case 32:
						{
							sum += qlp_Coeff[31] * data[data_Offset + i - 32];
							goto case 31;
						}

						case 31:
						{
							sum += qlp_Coeff[30] * data[data_Offset + i - 31];
							goto case 30;
						}

						case 30:
						{
							sum += qlp_Coeff[29] * data[data_Offset + i - 30];
							goto case 29;
						}

						case 29:
						{
							sum += qlp_Coeff[28] * data[data_Offset + i - 29];
							goto case 28;
						}

						case 28:
						{
							sum += qlp_Coeff[27] * data[data_Offset + i - 28];
							goto case 27;
						}

						case 27:
						{
							sum += qlp_Coeff[26] * data[data_Offset + i - 27];
							goto case 26;
						}

						case 26:
						{
							sum += qlp_Coeff[25] * data[data_Offset + i - 26];
							goto case 25;
						}

						case 25:
						{
							sum += qlp_Coeff[24] * data[data_Offset + i - 25];
							goto case 24;
						}

						case 24:
						{
							sum += qlp_Coeff[23] * data[data_Offset + i - 24];
							goto case 23;
						}

						case 23:
						{
							sum += qlp_Coeff[22] * data[data_Offset + i - 23];
							goto case 22;
						}

						case 22:
						{
							sum += qlp_Coeff[21] * data[data_Offset + i - 22];
							goto case 21;
						}

						case 21:
						{
							sum += qlp_Coeff[20] * data[data_Offset + i - 21];
							goto case 20;
						}

						case 20:
						{
							sum += qlp_Coeff[19] * data[data_Offset + i - 20];
							goto case 19;
						}

						case 19:
						{
							sum += qlp_Coeff[18] * data[data_Offset + i - 19];
							goto case 18;
						}

						case 18:
						{
							sum += qlp_Coeff[17] * data[data_Offset + i - 18];
							goto case 17;
						}

						case 17:
						{
							sum += qlp_Coeff[16] * data[data_Offset + i - 17];
							goto case 16;
						}

						case 16:
						{
							sum += qlp_Coeff[15] * data[data_Offset + i - 16];
							goto case 15;
						}

						case 15:
						{
							sum += qlp_Coeff[14] * data[data_Offset + i - 15];
							goto case 14;
						}

						case 14:
						{
							sum += qlp_Coeff[13] * data[data_Offset + i - 14];
							goto case 13;
						}

						case 13:
						{
							sum += qlp_Coeff[12] * data[data_Offset + i - 13];
							sum += qlp_Coeff[11] * data[data_Offset + i - 12];
							sum += qlp_Coeff[10] * data[data_Offset + i - 11];
							sum += qlp_Coeff[9] * data[data_Offset + i - 10];
							sum += qlp_Coeff[8] * data[data_Offset + i - 9];
							sum += qlp_Coeff[7] * data[data_Offset + i - 8];
							sum += qlp_Coeff[6] * data[data_Offset + i - 7];
							sum += qlp_Coeff[5] * data[data_Offset + i - 6];
							sum += qlp_Coeff[4] * data[data_Offset + i - 5];
							sum += qlp_Coeff[3] * data[data_Offset + i - 4];
							sum += qlp_Coeff[2] * data[data_Offset + i - 3];
							sum += qlp_Coeff[1] * data[data_Offset + i - 2];
							sum += qlp_Coeff[0] * data[data_Offset + i - 1];
							break;
						}
					}

					data[data_Offset + i] = residual[i] + (sum >> lp_Quantization);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Generic 64-bit datapath
		/// </summary>
		/********************************************************************/
		public void Restore_Signal_64Bit(Flac__int32[] residual, uint32_t data_Len, Flac__int32[] qlp_Coeff, uint32_t order, int lp_Quantization, Flac__int32[] data, uint32_t data_Offset)
		{
			Debug.Assert(order > 0);
			Debug.Assert(order <= 32);

			// We do unique versions up to 12th order since that's the subset limit.
			// Also they are roughly ordered to match frequency of occurrence to
			// minimize branching
			if (order <= 12)
			{
				if (order > 8)
				{
					if (order > 10)
					{
						if (order == 12)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[11] * (Flac__int64)data[data_Offset + i - 12];
								sum += qlp_Coeff[10] * (Flac__int64)data[data_Offset + i - 11];
								sum += qlp_Coeff[9] * (Flac__int64)data[data_Offset + i - 10];
								sum += qlp_Coeff[8] * (Flac__int64)data[data_Offset + i - 9];
								sum += qlp_Coeff[7] * (Flac__int64)data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * (Flac__int64)data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * (Flac__int64)data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * (Flac__int64)data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * (Flac__int64)data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * (Flac__int64)data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * (Flac__int64)data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1];

								data[data_Offset + i] = (Flac__int32)(residual[i] + (sum >> lp_Quantization));
							}
						}
						else	// order == 11
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[10] * (Flac__int64)data[data_Offset + i - 11];
								sum += qlp_Coeff[9] * (Flac__int64)data[data_Offset + i - 10];
								sum += qlp_Coeff[8] * (Flac__int64)data[data_Offset + i - 9];
								sum += qlp_Coeff[7] * (Flac__int64)data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * (Flac__int64)data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * (Flac__int64)data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * (Flac__int64)data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * (Flac__int64)data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * (Flac__int64)data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * (Flac__int64)data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1];

								data[data_Offset + i] = (Flac__int32)(residual[i] + (sum >> lp_Quantization));
							}
						}
					}
					else
					{
						if (order == 10)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[9] * (Flac__int64)data[data_Offset + i - 10];
								sum += qlp_Coeff[8] * (Flac__int64)data[data_Offset + i - 9];
								sum += qlp_Coeff[7] * (Flac__int64)data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * (Flac__int64)data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * (Flac__int64)data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * (Flac__int64)data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * (Flac__int64)data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * (Flac__int64)data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * (Flac__int64)data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1];

								data[data_Offset + i] = (Flac__int32)(residual[i] + (sum >> lp_Quantization));
							}
						}
						else	// order == 9
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[8] * (Flac__int64)data[data_Offset + i - 9];
								sum += qlp_Coeff[7] * (Flac__int64)data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * (Flac__int64)data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * (Flac__int64)data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * (Flac__int64)data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * (Flac__int64)data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * (Flac__int64)data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * (Flac__int64)data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1];

								data[data_Offset + i] = (Flac__int32)(residual[i] + (sum >> lp_Quantization));
							}
						}
					}
				}
				else if (order > 4)
				{
					if (order > 6)
					{
						if (order == 8)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[7] * (Flac__int64)data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * (Flac__int64)data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * (Flac__int64)data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * (Flac__int64)data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * (Flac__int64)data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * (Flac__int64)data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * (Flac__int64)data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1];

								data[data_Offset + i] = (Flac__int32)(residual[i] + (sum >> lp_Quantization));
							}
						}
						else	// order == 7
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[6] * (Flac__int64)data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * (Flac__int64)data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * (Flac__int64)data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * (Flac__int64)data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * (Flac__int64)data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * (Flac__int64)data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1];

								data[data_Offset + i] = (Flac__int32)(residual[i] + (sum >> lp_Quantization));
							}
						}
					}
					else
					{
						if (order == 6)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[5] * (Flac__int64)data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * (Flac__int64)data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * (Flac__int64)data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * (Flac__int64)data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * (Flac__int64)data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1];

								data[data_Offset + i] = (Flac__int32)(residual[i] + (sum >> lp_Quantization));
							}
						}
						else	// order == 5
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[4] * (Flac__int64)data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * (Flac__int64)data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * (Flac__int64)data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * (Flac__int64)data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1];

								data[data_Offset + i] = (Flac__int32)(residual[i] + (sum >> lp_Quantization));
							}
						}
					}
				}
				else
				{
					if (order > 2)
					{
						if (order == 4)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[3] * (Flac__int64)data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * (Flac__int64)data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * (Flac__int64)data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1];

								data[data_Offset + i] = (Flac__int32)(residual[i] + (sum >> lp_Quantization));
							}
						}
						else	// order == 3
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[2] * (Flac__int64)data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * (Flac__int64)data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1];

								data[data_Offset + i] = (Flac__int32)(residual[i] + (sum >> lp_Quantization));
							}
						}
					}
					else
					{
						if (order == 2)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[1] * (Flac__int64)data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1];

								data[data_Offset + i] = (Flac__int32)(residual[i] + (sum >> lp_Quantization));
							}
						}
						else	// order == 1
						{
							for (int i = 0; i < data_Len; i++)
								data[data_Offset + i] = residual[i] + (Flac__int32)((qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1]) >> lp_Quantization);
						}
					}
				}
			}
			else	// order == 12
			{
				for (int i = 0; i < data_Len; i++)
				{
					Flac__int64 sum = 0;

					switch (order)
					{
						case 32:
						{
							sum += qlp_Coeff[31] * (Flac__int64)data[data_Offset + i - 32];
							goto case 31;
						}

						case 31:
						{
							sum += qlp_Coeff[30] * (Flac__int64)data[data_Offset + i - 31];
							goto case 30;
						}

						case 30:
						{
							sum += qlp_Coeff[29] * (Flac__int64)data[data_Offset + i - 30];
							goto case 29;
						}

						case 29:
						{
							sum += qlp_Coeff[28] * (Flac__int64)data[data_Offset + i - 29];
							goto case 28;
						}

						case 28:
						{
							sum += qlp_Coeff[27] * (Flac__int64)data[data_Offset + i - 28];
							goto case 27;
						}

						case 27:
						{
							sum += qlp_Coeff[26] * (Flac__int64)data[data_Offset + i - 27];
							goto case 26;
						}

						case 26:
						{
							sum += qlp_Coeff[25] * (Flac__int64)data[data_Offset + i - 26];
							goto case 25;
						}

						case 25:
						{
							sum += qlp_Coeff[24] * (Flac__int64)data[data_Offset + i - 25];
							goto case 24;
						}

						case 24:
						{
							sum += qlp_Coeff[23] * (Flac__int64)data[data_Offset + i - 24];
							goto case 23;
						}

						case 23:
						{
							sum += qlp_Coeff[22] * (Flac__int64)data[data_Offset + i - 23];
							goto case 22;
						}

						case 22:
						{
							sum += qlp_Coeff[21] * (Flac__int64)data[data_Offset + i - 22];
							goto case 21;
						}

						case 21:
						{
							sum += qlp_Coeff[20] * (Flac__int64)data[data_Offset + i - 21];
							goto case 20;
						}

						case 20:
						{
							sum += qlp_Coeff[19] * (Flac__int64)data[data_Offset + i - 20];
							goto case 19;
						}

						case 19:
						{
							sum += qlp_Coeff[18] * (Flac__int64)data[data_Offset + i - 19];
							goto case 18;
						}

						case 18:
						{
							sum += qlp_Coeff[17] * (Flac__int64)data[data_Offset + i - 18];
							goto case 17;
						}

						case 17:
						{
							sum += qlp_Coeff[16] * (Flac__int64)data[data_Offset + i - 17];
							goto case 16;
						}

						case 16:
						{
							sum += qlp_Coeff[15] * (Flac__int64)data[data_Offset + i - 16];
							goto case 15;
						}

						case 15:
						{
							sum += qlp_Coeff[14] * (Flac__int64)data[data_Offset + i - 15];
							goto case 14;
						}

						case 14:
						{
							sum += qlp_Coeff[13] * (Flac__int64)data[data_Offset + i - 14];
							goto case 13;
						}

						case 13:
						{
							sum += qlp_Coeff[12] * (Flac__int64)data[data_Offset + i - 13];
							sum += qlp_Coeff[11] * (Flac__int64)data[data_Offset + i - 12];
							sum += qlp_Coeff[10] * (Flac__int64)data[data_Offset + i - 11];
							sum += qlp_Coeff[9] * (Flac__int64)data[data_Offset + i - 10];
							sum += qlp_Coeff[8] * (Flac__int64)data[data_Offset + i - 9];
							sum += qlp_Coeff[7] * (Flac__int64)data[data_Offset + i - 8];
							sum += qlp_Coeff[6] * (Flac__int64)data[data_Offset + i - 7];
							sum += qlp_Coeff[5] * (Flac__int64)data[data_Offset + i - 6];
							sum += qlp_Coeff[4] * (Flac__int64)data[data_Offset + i - 5];
							sum += qlp_Coeff[3] * (Flac__int64)data[data_Offset + i - 4];
							sum += qlp_Coeff[2] * (Flac__int64)data[data_Offset + i - 3];
							sum += qlp_Coeff[1] * (Flac__int64)data[data_Offset + i - 2];
							sum += qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1];
							break;
						}
					}

					data[data_Offset + i] = (Flac__int32)(residual[i] + (sum >> lp_Quantization));
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// For use when the signal is less-or-equal-to 16 bits-per-sample,
		/// or less-or-equal-to 15 bits-per-sample on a side channel (which
		/// requires 1 extra bit)
		/// </summary>
		/********************************************************************/
		public void Restore_Signal_16Bit(Flac__int32[] residual, uint32_t data_Len, Flac__int32[] qlp_Coeff, uint32_t order, int lp_Quantization, Flac__int32[] data, uint32_t data_Offset)
		{
			Restore_Signal(residual, data_Len, qlp_Coeff, order, lp_Quantization, data, data_Offset);
		}
	}
}
