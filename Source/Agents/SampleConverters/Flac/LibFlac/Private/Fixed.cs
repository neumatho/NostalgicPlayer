/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac.Containers;

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Private
{
	/// <summary>
	/// 
	/// </summary>
	internal class Fixed : IFixed
	{
		/********************************************************************/
		/// <summary>
		/// Compute the best fixed predictor and the expected bits-per-sample
		/// of the residual signal for each order
		/// </summary>
		/********************************************************************/
		public uint32_t Compute_Best_Predictor(Flac__int32[] data, uint32_t offset, uint32_t data_Len, float[] residual_Bits_Per_Sample)
		{
			Flac__int32 last_Error_0 = data[offset - 1];
			Flac__int32 last_Error_1 = data[offset - 1] - data[offset - 2];
			Flac__int32 last_Error_2 = last_Error_1 - (data[offset - 2] - data[offset - 3]);
			Flac__int32 last_Error_3 = last_Error_2 - (data[offset - 2] - 2 * data[offset - 3] + data[offset - 4]);
			Flac__uint32 total_Error_0 = 0, total_Error_1 = 0, total_Error_2 = 0, total_Error_3 = 0, total_Error_4 = 0;

			for (uint32_t i = 0; i < data_Len; i++)
			{
				Flac__int32 error = data[offset + i];
				total_Error_0 += Local_Abs(error);
				Flac__int32 save = error;

				error -= last_Error_0;
				total_Error_1 += Local_Abs(error);
				last_Error_0 = save;
				save = error;

				error -= last_Error_1;
				total_Error_2 += Local_Abs(error);
				last_Error_1 = save;
				save = error;

				error -= last_Error_2;
				total_Error_3 += Local_Abs(error);
				last_Error_2 = save;
				save = error;

				error -= last_Error_3;
				total_Error_4 += Local_Abs(error);
				last_Error_3 = save;
			}

			uint32_t order;

			if (total_Error_0 < Math.Min(Math.Min(Math.Min(total_Error_1, total_Error_2), total_Error_3), total_Error_4))
				order = 0;
			else if (total_Error_1 < Math.Min(Math.Min(total_Error_2, total_Error_3), total_Error_4))
				order = 1;
			else if (total_Error_2 < Math.Min(total_Error_3, total_Error_4))
				order = 2;
			else if (total_Error_3 < total_Error_4)
				order = 3;
			else
				order = 4;

			// Estimate the expected number of bits per residual signal sample.
			// 'total_Error*' is linearly related to the variance of the residual
			// signal, so we use it directly to compute E(|x|)
			Debug.Assert((data_Len > 0) || (total_Error_0 == 0));
			Debug.Assert((data_Len > 0) || (total_Error_1 == 0));
			Debug.Assert((data_Len > 0) || (total_Error_2 == 0));
			Debug.Assert((data_Len > 0) || (total_Error_3 == 0));
			Debug.Assert((data_Len > 0) || (total_Error_4 == 0));

			residual_Bits_Per_Sample[0] = (float)(total_Error_0 > 0 ? Math.Log(Constants.M_LN2 * total_Error_0 / data_Len) / Constants.M_LN2 : 0.0);
			residual_Bits_Per_Sample[1] = (float)(total_Error_1 > 0 ? Math.Log(Constants.M_LN2 * total_Error_1 / data_Len) / Constants.M_LN2 : 0.0);
			residual_Bits_Per_Sample[2] = (float)(total_Error_2 > 0 ? Math.Log(Constants.M_LN2 * total_Error_2 / data_Len) / Constants.M_LN2 : 0.0);
			residual_Bits_Per_Sample[3] = (float)(total_Error_3 > 0 ? Math.Log(Constants.M_LN2 * total_Error_3 / data_Len) / Constants.M_LN2 : 0.0);
			residual_Bits_Per_Sample[4] = (float)(total_Error_4 > 0 ? Math.Log(Constants.M_LN2 * total_Error_4 / data_Len) / Constants.M_LN2 : 0.0);

			return order;
		}



		/********************************************************************/
		/// <summary>
		/// Compute the best fixed predictor and the expected bits-per-sample
		/// of the residual signal for each order. This version uses 64-bit
		/// integers which is statistically necessary when bits-per-sample +
		/// log2(blockSize) > 30
		/// </summary>
		/********************************************************************/
		public uint32_t Compute_Best_Predictor_Wide(Flac__int32[] data, uint32_t offset, uint32_t data_Len, float[] residual_Bits_Per_Sample)
		{
			Flac__int32 last_Error_0 = data[offset - 1];
			Flac__int32 last_Error_1 = data[offset - 1] - data[offset - 2];
			Flac__int32 last_Error_2 = last_Error_1 - (data[offset - 2] - data[offset - 3]);
			Flac__int32 last_Error_3 = last_Error_2 - (data[offset - 2] - 2 * data[offset - 3] + data[offset - 4]);

			// total_Error_* are 64-bits to avoid overflow when encoding
			// erratic signals when the bits-per-sample and blockSize are
			// large
			Flac__uint64 total_Error_0 = 0, total_Error_1 = 0, total_Error_2 = 0, total_Error_3 = 0, total_Error_4 = 0;

			for (uint32_t i = 0; i < data_Len; i++)
			{
				Flac__int32 error = data[offset + i];
				total_Error_0 += Local_Abs(error);
				Flac__int32 save = error;

				error -= last_Error_0;
				total_Error_1 += Local_Abs(error);
				last_Error_0 = save;
				save = error;

				error -= last_Error_1;
				total_Error_2 += Local_Abs(error);
				last_Error_1 = save;
				save = error;

				error -= last_Error_2;
				total_Error_3 += Local_Abs(error);
				last_Error_2 = save;
				save = error;

				error -= last_Error_3;
				total_Error_4 += Local_Abs(error);
				last_Error_3 = save;
			}

			uint32_t order;

			if (total_Error_0 < Math.Min(Math.Min(Math.Min(total_Error_1, total_Error_2), total_Error_3), total_Error_4))
				order = 0;
			else if (total_Error_1 < Math.Min(Math.Min(total_Error_2, total_Error_3), total_Error_4))
				order = 1;
			else if (total_Error_2 < Math.Min(total_Error_3, total_Error_4))
				order = 2;
			else if (total_Error_3 < total_Error_4)
				order = 3;
			else
				order = 4;

			// Estimate the expected number of bits per residual signal sample.
			// 'total_Error*' is linearly related to the variance of the residual
			// signal, so we use it directly to compute E(|x|)
			Debug.Assert((data_Len > 0) || (total_Error_0 == 0));
			Debug.Assert((data_Len > 0) || (total_Error_1 == 0));
			Debug.Assert((data_Len > 0) || (total_Error_2 == 0));
			Debug.Assert((data_Len > 0) || (total_Error_3 == 0));
			Debug.Assert((data_Len > 0) || (total_Error_4 == 0));

			residual_Bits_Per_Sample[0] = (float)(total_Error_0 > 0 ? Math.Log(Constants.M_LN2 * total_Error_0 / data_Len) / Constants.M_LN2 : 0.0);
			residual_Bits_Per_Sample[1] = (float)(total_Error_1 > 0 ? Math.Log(Constants.M_LN2 * total_Error_1 / data_Len) / Constants.M_LN2 : 0.0);
			residual_Bits_Per_Sample[2] = (float)(total_Error_2 > 0 ? Math.Log(Constants.M_LN2 * total_Error_2 / data_Len) / Constants.M_LN2 : 0.0);
			residual_Bits_Per_Sample[3] = (float)(total_Error_3 > 0 ? Math.Log(Constants.M_LN2 * total_Error_3 / data_Len) / Constants.M_LN2 : 0.0);
			residual_Bits_Per_Sample[4] = (float)(total_Error_4 > 0 ? Math.Log(Constants.M_LN2 * total_Error_4 / data_Len) / Constants.M_LN2 : 0.0);

			return order;
		}



		/********************************************************************/
		/// <summary>
		/// Compute the residual signal obtained from sutracting the
		/// predicted signal from the original
		/// </summary>
		/********************************************************************/
		public static void Flac__Fixed_Compute_Residual(Flac__int32[] data, uint32_t dataOffset, uint32_t data_Len, uint32_t order, Flac__int32[] residual)
		{
			int iData_Len = (int)data_Len;

			switch (order)
			{
				case 0:
				{
					Array.Copy(data, dataOffset, residual, 0, data_Len);
					break;
				}

				case 1:
				{
					for (int i = 0; i < iData_Len; i++)
						residual[i] = data[dataOffset + i] - data[dataOffset + i - 1];

					break;
				}

				case 2:
				{
					for (int i = 0; i < iData_Len; i++)
						residual[i] = data[dataOffset + i] - 2 * data[dataOffset + i - 1] + data[dataOffset + i - 2];

					break;
				}

				case 3:
				{
					for (int i = 0; i < iData_Len; i++)
						residual[i] = data[dataOffset + i] - 3 * data[dataOffset + i - 1] + 3 * data[dataOffset + i - 2] - data[dataOffset + i - 3];

					break;
				}

				case 4:
				{
					for (int i = 0; i < iData_Len; i++)
						residual[i] = data[dataOffset + i] - 4 * data[dataOffset + i - 1] + 6 * data[dataOffset + i - 2] - 4 * data[dataOffset + i - 3] + data[dataOffset + i - 4];

					break;
				}

				default:
				{
					Debug.Assert(false);
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Restore the original signal by summing the residual and the
		/// predictor
		/// </summary>
		/********************************************************************/
		public static void Flac__Fixed_Restore_Signal(Flac__int32[] residual, uint32_t data_Len, uint32_t order, Flac__int32[] data, uint32_t dataOffset)
		{
			int iData_Len = (int)data_Len;

			switch (order)
			{
				case 0:
				{
					Array.Copy(residual, 0, data, dataOffset, data_Len);
					break;
				}

				case 1:
				{
					for (int i = 0; i < iData_Len; i++)
						data[dataOffset + i] = residual[i] + data[dataOffset + i - 1];

					break;
				}

				case 2:
				{
					for (int i = 0; i < iData_Len; i++)
						data[dataOffset + i] = residual[i] + 2 * data[dataOffset + i - 1] - data[dataOffset + i - 2];

					break;
				}

				case 3:
				{
					for (int i = 0; i < iData_Len; i++)
						data[dataOffset + i] = residual[i] + 3 * data[dataOffset + i - 1] - 3 * data[dataOffset + i - 2] + data[dataOffset + i - 3];

					break;
				}

				case 4:
				{
					for (int i = 0; i < iData_Len; i++)
						data[dataOffset + i] = residual[i] + 4 * data[dataOffset + i - 1] - 6 * data[dataOffset + i - 2] + 4 * data[dataOffset + i - 3] - data[dataOffset + i - 4];

					break;
				}

				default:
				{
					Debug.Assert(false);
					break;
				}
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private uint32_t Local_Abs(int32_t x)
		{
			return (uint32_t)(x < 0 ? -x : x);
		}
		#endregion
	}
}
