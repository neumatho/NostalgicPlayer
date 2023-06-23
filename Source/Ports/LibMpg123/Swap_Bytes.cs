/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.LibMpg123
{
	/// <summary>
	/// Swap byte order of samples in a buffer
	/// </summary>
	internal static class Swap_Bytes
	{
		/********************************************************************/
		/// <summary>
		/// Convert samplecount elements of samplesize bytes each in buffer
		/// buf
		/// </summary>
		/********************************************************************/
		public static void Swap_Bytes_(c_uchar[] buf, size_t sampleSize, size_t sampleCount)
		{
			size_t p = 0;
			size_t pEnd = sampleSize * sampleCount;

			if (sampleSize < 2)
				return;

			switch (sampleSize)
			{
				case 2:	// AB -> BA
				{
					for (; p < pEnd; p++)
						Swap(buf, p + 0, p + 1);

					break;
				}

				case 3:	// ABC -> CBA
				{
					for (; p < pEnd; p += 3)
						Swap(buf, p + 0, p + 2);

					break;
				}

				case 4:	// ABCD -> DCBA
				{
					for (; p < pEnd; p += 4)
					{
						Swap(buf, p + 0, p + 3);
						Swap(buf, p + 1, p + 2);
					}
					break;
				}

				case 8:	// ABCDEFGH -> HGFEDCBA
				{
					for (; p < pEnd; p += 8)
					{
						Swap(buf, p + 0, p + 7);
						Swap(buf, p + 1, p + 6);
						Swap(buf, p + 2, p + 5);
						Swap(buf, p + 3, p + 4);
					}
					break;
				}

				default:
				{
					// All the weird choices with the full nested loop
					for (; p < pEnd; p += sampleSize)
					{
						for (size_t j = 0; j < sampleSize / 2; ++j)
							Swap(buf, p + j, p + sampleSize - j - 1);
					}
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
		private static void Swap(c_uchar[] buf, size_t a, size_t b)
		{
			(buf[b], buf[a]) = (buf[a], buf[b]);
		}
		#endregion
	}
}
