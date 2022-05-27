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
	internal static class Fixed
	{
		/********************************************************************/
		/// <summary>
		/// 
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
	}
}
