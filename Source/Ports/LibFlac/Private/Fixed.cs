/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibFlac.Private
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
		public uint32_t Compute_Best_Predictor(Flac__int32[] data, uint32_t dataOffset, uint32_t data_Len, float[] residual_Bits_Per_Sample)
		{
			Flac__uint32 total_Error_0 = 0, total_Error_1 = 0, total_Error_2 = 0, total_Error_3 = 0, total_Error_4 = 0;

			for (uint32_t i = 0; i < data_Len; i++)
			{
				total_Error_0 += Local_Abs(data[dataOffset + i]);
				total_Error_1 += Local_Abs(data[dataOffset + i] - data[dataOffset + i - 1]);
				total_Error_2 += Local_Abs(data[dataOffset + i] - 2 * data[dataOffset + i - 1] + data[dataOffset + i - 2]);
				total_Error_3 += Local_Abs(data[dataOffset + i] - 3 * data[dataOffset + i - 1] + 3 * data[dataOffset + i - 2] - data[dataOffset + i - 3]);
				total_Error_4 += Local_Abs(data[dataOffset + i] - 4 * data[dataOffset + i - 1] + 6 * data[dataOffset + i - 2] - 4 * data[dataOffset + i - 3] + data[dataOffset + i - 4]);
			}

			// Prefer lower order
			uint32_t order;

			if (total_Error_0 <= Math.Min(Math.Min(Math.Min(total_Error_1, total_Error_2), total_Error_3), total_Error_4))
				order = 0;
			else if (total_Error_1 <= Math.Min(Math.Min(total_Error_2, total_Error_3), total_Error_4))
				order = 1;
			else if (total_Error_2 <= Math.Min(total_Error_3, total_Error_4))
				order = 2;
			else if (total_Error_3 <= total_Error_4)
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
		public uint32_t Compute_Best_Predictor_Wide(Flac__int32[] data, uint32_t dataOffset, uint32_t data_Len, float[] residual_Bits_Per_Sample)
		{
			Flac__uint64 total_Error_0 = 0, total_Error_1 = 0, total_Error_2 = 0, total_Error_3 = 0, total_Error_4 = 0;

			for (int i = 0; i < data_Len; i++)
			{
				total_Error_0 += Local_Abs(data[dataOffset + i]);
				total_Error_1 += Local_Abs(data[dataOffset + i] - data[dataOffset + i - 1]);
				total_Error_2 += Local_Abs(data[dataOffset + i] - 2 * data[dataOffset + i - 1] + data[dataOffset + i - 2]);
				total_Error_3 += Local_Abs(data[dataOffset + i] - 3 * data[dataOffset + i - 1] + 3 * data[dataOffset + i - 2] - data[dataOffset + i - 3]);
				total_Error_4 += Local_Abs(data[dataOffset + i] - 4 * data[dataOffset + i - 1] + 6 * data[dataOffset + i - 2] - 4 * data[dataOffset + i - 3] + data[dataOffset + i - 4]);
			}

			// Prefer lower order
			uint32_t order;

			if (total_Error_0 <= Math.Min(Math.Min(Math.Min(total_Error_1, total_Error_2), total_Error_3), total_Error_4))
				order = 0;
			else if (total_Error_1 <= Math.Min(Math.Min(total_Error_2, total_Error_3), total_Error_4))
				order = 1;
			else if (total_Error_2 <= Math.Min(total_Error_3, total_Error_4))
				order = 2;
			else if (total_Error_3 <= total_Error_4)
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
		public uint32_t Compute_Best_Predictor_Limit_Residual(Flac__int32[] data, uint32_t dataOffset, uint32_t data_Len, float[] residual_Bits_Per_Sample)
		{
			Flac__uint64 total_Error_0 = 0, total_Error_1 = 0, total_Error_2 = 0, total_Error_3 = 0, total_Error_4 = 0, smallest_Error = uint64_t.MaxValue;
			Flac__bool order_0_Is_Valid = true, order_1_Is_Valid = true, order_2_Is_Valid = true, order_3_Is_Valid = true, order_4_Is_Valid = true;
			uint32_t order = 0;

			for (int i = -4; i < data_Len; i++)
			{
				Flac__uint64 error_0 = Local_Abs64(data[dataOffset + i]);
				Flac__uint64 error_1 = (i > -4) ? Local_Abs64((Flac__int64)data[dataOffset + i] - data[dataOffset + i - 1]) : 0;
				Flac__uint64 error_2 = (i > -3) ? Local_Abs64(data[dataOffset + i] - 2 * (Flac__int64)data[dataOffset + i - 1] + data[dataOffset + i - 2]) : 0;
				Flac__uint64 error_3 = (i > -2) ? Local_Abs64(data[dataOffset + i] - 3 * (Flac__int64)data[dataOffset + i - 1] + 3 * (Flac__int64)data[dataOffset + i - 2] - data[dataOffset + i - 3]) : 0;
				Flac__uint64 error_4 = (i > -1) ? Local_Abs64(data[dataOffset + i] - 4 * (Flac__int64)data[dataOffset + i - 1] + 6 * (Flac__int64)data[dataOffset + i - 2] - 4 * (Flac__int64)data[dataOffset + i - 3] + data[dataOffset + i - 4]) : 0;

				total_Error_0 += error_0;
				total_Error_1 += error_1;
				total_Error_2 += error_2;
				total_Error_3 += error_3;
				total_Error_4 += error_4;

				// Residual must not be INT32_MIN because abs(INT32_MIN) is undefined
				if (error_0 > int32_t.MaxValue)
					order_0_Is_Valid = false;

				if (error_1 > int32_t.MaxValue)
					order_1_Is_Valid = false;

				if (error_2 > int32_t.MaxValue)
					order_2_Is_Valid = false;

				if (error_3 > int32_t.MaxValue)
					order_3_Is_Valid = false;

				if (error_4 > int32_t.MaxValue)
					order_4_Is_Valid = false;
			}

			Check_Order_Is_Valid(0, order_0_Is_Valid, total_Error_0, ref smallest_Error, ref order, residual_Bits_Per_Sample, data_Len);
			Check_Order_Is_Valid(1, order_1_Is_Valid, total_Error_1, ref smallest_Error, ref order, residual_Bits_Per_Sample, data_Len);
			Check_Order_Is_Valid(2, order_2_Is_Valid, total_Error_2, ref smallest_Error, ref order, residual_Bits_Per_Sample, data_Len);
			Check_Order_Is_Valid(3, order_3_Is_Valid, total_Error_3, ref smallest_Error, ref order, residual_Bits_Per_Sample, data_Len);
			Check_Order_Is_Valid(4, order_4_Is_Valid, total_Error_4, ref smallest_Error, ref order, residual_Bits_Per_Sample, data_Len);

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
		public uint32_t Compute_Best_Predictor_Limit_Residual_33Bit(Flac__int64[] data, uint32_t dataOffset, uint32_t data_Len, float[] residual_Bits_Per_Sample)
		{
			Flac__uint64 total_Error_0 = 0, total_Error_1 = 0, total_Error_2 = 0, total_Error_3 = 0, total_Error_4 = 0, smallest_Error = uint64_t.MaxValue;
			Flac__bool order_0_Is_Valid = true, order_1_Is_Valid = true, order_2_Is_Valid = true, order_3_Is_Valid = true, order_4_Is_Valid = true;
			uint32_t order = 0;

			for (int i = -4; i < data_Len; i++)
			{
				Flac__uint64 error_0 = Local_Abs64(data[dataOffset + i]);
				Flac__uint64 error_1 = (i > -4) ? Local_Abs64(data[dataOffset + i] - data[dataOffset + i - 1]) : 0;
				Flac__uint64 error_2 = (i > -3) ? Local_Abs64(data[dataOffset + i] - 2 * data[dataOffset + i - 1] + data[dataOffset + i - 2]) : 0;
				Flac__uint64 error_3 = (i > -2) ? Local_Abs64(data[dataOffset + i] - 3 * data[dataOffset + i - 1] + 3 * data[dataOffset + i - 2] - data[dataOffset + i - 3]) : 0;
				Flac__uint64 error_4 = (i > -1) ? Local_Abs64(data[dataOffset + i] - 4 * data[dataOffset + i - 1] + 6 * data[dataOffset + i - 2] - 4 * data[dataOffset + i - 3] + data[dataOffset + i - 4]) : 0;

				total_Error_0 += error_0;
				total_Error_1 += error_1;
				total_Error_2 += error_2;
				total_Error_3 += error_3;
				total_Error_4 += error_4;

				// Residual must not be INT32_MIN because abs(INT32_MIN) is undefined
				if (error_0 > int32_t.MaxValue)
					order_0_Is_Valid = false;

				if (error_1 > int32_t.MaxValue)
					order_1_Is_Valid = false;

				if (error_2 > int32_t.MaxValue)
					order_2_Is_Valid = false;

				if (error_3 > int32_t.MaxValue)
					order_3_Is_Valid = false;

				if (error_4 > int32_t.MaxValue)
					order_4_Is_Valid = false;
			}

			Check_Order_Is_Valid(0, order_0_Is_Valid, total_Error_0, ref smallest_Error, ref order, residual_Bits_Per_Sample, data_Len);
			Check_Order_Is_Valid(1, order_1_Is_Valid, total_Error_1, ref smallest_Error, ref order, residual_Bits_Per_Sample, data_Len);
			Check_Order_Is_Valid(2, order_2_Is_Valid, total_Error_2, ref smallest_Error, ref order, residual_Bits_Per_Sample, data_Len);
			Check_Order_Is_Valid(3, order_3_Is_Valid, total_Error_3, ref smallest_Error, ref order, residual_Bits_Per_Sample, data_Len);
			Check_Order_Is_Valid(4, order_4_Is_Valid, total_Error_4, ref smallest_Error, ref order, residual_Bits_Per_Sample, data_Len);

			return order;
		}



		/********************************************************************/
		/// <summary>
		/// Compute the residual signal obtained from subtracting the
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
		/// Compute the residual signal obtained from subtracting the
		/// predicted signal from the original
		/// </summary>
		/********************************************************************/
		public static void Flac__Fixed_Compute_Residual_Wide(Flac__int32[] data, uint32_t dataOffset, uint32_t data_Len, uint32_t order, Flac__int32[] residual)
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
						residual[i] = (Flac__int32)((Flac__int64)data[dataOffset + i] - data[dataOffset + i - 1]);

					break;
				}

				case 2:
				{
					for (int i = 0; i < iData_Len; i++)
						residual[i] = (Flac__int32)(data[dataOffset + i] - 2 * (Flac__int64)data[dataOffset + i - 1] + data[dataOffset + i - 2]);

					break;
				}

				case 3:
				{
					for (int i = 0; i < iData_Len; i++)
						residual[i] = (Flac__int32)(data[dataOffset + i] - 3 * (Flac__int64)data[dataOffset + i - 1] + 3 * (Flac__int64)data[dataOffset + i - 2] - data[dataOffset + i - 3]);

					break;
				}

				case 4:
				{
					for (int i = 0; i < iData_Len; i++)
						residual[i] = (Flac__int32)(data[dataOffset + i] - 4 * (Flac__int64)data[dataOffset + i - 1] + 6 * (Flac__int64)data[dataOffset + i - 2] - 4 * (Flac__int64)data[dataOffset + i - 3] + data[dataOffset + i - 4]);

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
		/// Compute the residual signal obtained from subtracting the
		/// predicted signal from the original
		/// </summary>
		/********************************************************************/
		public static void Flac__Fixed_Compute_Residual_Wide_33Bit(Flac__int64[] data, uint32_t dataOffset, uint32_t data_Len, uint32_t order, Flac__int32[] residual)
		{
			int iData_Len = (int)data_Len;

			switch (order)
			{
				case 0:
				{
					for (int i = 0; i < iData_Len; i++)
						residual[i] = (Flac__int32)data[dataOffset + i];

					break;
				}

				case 1:
				{
					for (int i = 0; i < iData_Len; i++)
						residual[i] = (Flac__int32)(data[dataOffset + i] - data[dataOffset + i - 1]);

					break;
				}

				case 2:
				{
					for (int i = 0; i < iData_Len; i++)
						residual[i] = (Flac__int32)(data[dataOffset + i] - 2 * data[dataOffset + i - 1] + data[dataOffset + i - 2]);

					break;
				}

				case 3:
				{
					for (int i = 0; i < iData_Len; i++)
						residual[i] = (Flac__int32)(data[dataOffset + i] - 3 * data[dataOffset + i - 1] + 3 * data[dataOffset + i - 2] - data[dataOffset + i - 3]);

					break;
				}

				case 4:
				{
					for (int i = 0; i < iData_Len; i++)
						residual[i] = (Flac__int32)(data[dataOffset + i] - 4 * data[dataOffset + i - 1] + 6 * data[dataOffset + i - 2] - 4 * data[dataOffset + i - 3] + data[dataOffset + i - 4]);

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



		/********************************************************************/
		/// <summary>
		/// Restore the original signal by summing the residual and the
		/// predictor
		/// </summary>
		/********************************************************************/
		public static void Flac__Fixed_Restore_Signal_Wide(Flac__int32[] residual, uint32_t data_Len, uint32_t order, Flac__int32[] data, uint32_t dataOffset)
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
						data[dataOffset + i] = (Flac__int32)((Flac__int64)residual[i] + data[dataOffset + i - 1]);

					break;
				}

				case 2:
				{
					for (int i = 0; i < iData_Len; i++)
						data[dataOffset + i] = (Flac__int32)(residual[i] + 2 * (Flac__int64)data[dataOffset + i - 1] - data[dataOffset + i - 2]);

					break;
				}

				case 3:
				{
					for (int i = 0; i < iData_Len; i++)
						data[dataOffset + i] = (Flac__int32)(residual[i] + 3 * (Flac__int64)data[dataOffset + i - 1] - 3 * (Flac__int64)data[dataOffset + i - 2] + data[dataOffset + i - 3]);

					break;
				}

				case 4:
				{
					for (int i = 0; i < iData_Len; i++)
						data[dataOffset + i] = (Flac__int32)(residual[i] + 4 * (Flac__int64)data[dataOffset + i - 1] - 6 * (Flac__int64)data[dataOffset + i - 2] + 4 * (Flac__int64)data[dataOffset + i - 3] - data[dataOffset + i - 4]);

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
		public static void Flac__Fixed_Restore_Signal_Wide_33Bit(Flac__int32[] residual, uint32_t data_Len, uint32_t order, Flac__int64[] data, uint32_t dataOffset)
		{
			int iData_Len = (int)data_Len;

			switch (order)
			{
				case 0:
				{
					for (int i = 0; i < iData_Len; i++)
						data[dataOffset + i] = residual[i];

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



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static uint64_t Local_Abs64(int64_t x)
		{
			return (uint64_t)(x < 0 ? -x : x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Check_Order_Is_Valid(uint32_t macro_Order, Flac__bool order_Is_Valid, Flac__uint64 total_Error, ref Flac__uint64 smallest_Error, ref uint32_t order, float[] residual_Bits_Per_Sample, uint32_t data_Len)
		{
			if (order_Is_Valid && (total_Error < smallest_Error))
			{
				order = macro_Order;
				smallest_Error = total_Error;
				residual_Bits_Per_Sample[macro_Order] = (float)((total_Error > 0) ? Math.Log(Constants.M_LN2 * total_Error / data_Len) / Constants.M_LN2 : 0.0);
			}
			else
				residual_Bits_Per_Sample[macro_Order] = 34.0f;
		}
		#endregion
	}
}
