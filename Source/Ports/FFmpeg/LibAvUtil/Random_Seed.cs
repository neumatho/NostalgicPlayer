/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class Random_Seed
	{
		/********************************************************************/
		/// <summary>
		/// Generate cryptographically secure random data, i.e. suitable for
		/// use as encryption keys and similar
		/// </summary>
		/********************************************************************/
		public static c_int Av_Random_Bytes(CPointer<uint8_t> buf, size_t len)//XX 159
		{
			try
			{
				RandomNumberGenerator.Fill(buf.AsSpan().Slice(0, (int)len));
			}
			catch
			{
				return -1;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Get a seed to use in conjunction with random functions.
		/// This function tries to provide a good seed at a best effort
		/// bases. Its possible to call this function multiple times if more
		/// bits are needed.
		/// It can be quite slow, which is why it should only be used as seed
		/// for a faster PRNG. The quality of the seed depends on the
		/// platform
		/// </summary>
		/********************************************************************/
		public static uint32_t Av_Get_Random_Seed()//XX 196
		{
			uint8_t[] seed = new uint8_t[4];

			if (Av_Random_Bytes(seed, (size_t)seed.Length) < 0)
				return Get_Generic_Seed();

			return (uint32_t)((seed[0] << 24) | (seed[1] << 16) | (seed[2] << 8) | seed[3]);
		}

		#region Private methods
		private static uint64_t get_Generic_Seed_i = 0;
		private static readonly uint32_t[] get_Generic_Seed_Buffer = new uint32_t[512];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal static uint32_t Get_Generic_Seed()//XX 75
		{
			AvSha sha = new AvSha();
			clock_t last_t = 0;
			clock_t last_td = 0;
			clock_t init_t = 0;
			CPointer<c_uchar> digest = new CPointer<c_uchar>(20);
			uint64_t last_i = get_Generic_Seed_i;
			c_int[] repeats = new c_int[3];

			if (UnitTest.IsUnitTestEnabled())
			{
				CMemory.memset(get_Generic_Seed_Buffer, 0U, (size_t)get_Generic_Seed_Buffer.Length);
				last_i = get_Generic_Seed_i = 0;
			}
			else
			{
				get_Generic_Seed_Buffer[13] ^= (uint32_t)Timer.Av_Read_Time();
				get_Generic_Seed_Buffer[41] ^= (uint32_t)(Timer.Av_Read_Time() >> 32);
			}

			for (;;)
			{
				clock_t t = CThread.clock();
				c_int incremented_i = 0;
				c_int cur_td = (c_int)(t - last_t);

				if ((last_t + 2 * last_td + (CThread.Clocks_Per_Sec > 1000 ? 1 : 0)) < t)
				{
					// If the timer incremented by more than 2*last_td at once,
					// we may e.g. have had a context switch. If the timer resolution
					// is high (CLOCKS_PER_SEC > 1000), require that the timer
					// incremented by more than 1. If the timer resolution is low,
					// it is enough that the timer incremented at all
					get_Generic_Seed_Buffer[++get_Generic_Seed_i & 511] += (uint32_t)(cur_td % 3294638521U);
					incremented_i = 1;
				}
				else if ((t != last_t) && (repeats[0] > 0) && (repeats[1] > 0) && (repeats[2] > 0) && (repeats[0] != repeats[1]) && (repeats[0] != repeats[2]))
				{
					// If the timer resolution is high, and we get the same timer
					// value multiple times, use variances in the number of repeats
					// of each timer value as entropy. If we get a different number of
					// repeats than the last two unique cases, count that as entropy
					// and proceed to the next index
					get_Generic_Seed_Buffer[++get_Generic_Seed_i & 511] += (uint32_t)((repeats[0] + repeats[1] + repeats[2]) % 3294638521U);
					incremented_i = 1;
				}
				else
					get_Generic_Seed_Buffer[get_Generic_Seed_i & 511] = (uint32_t)(1664525 * get_Generic_Seed_Buffer[get_Generic_Seed_i & 511] + 1013904223 + (cur_td % 3294638521U));

				if ((incremented_i != 0) && ((uint64_t)(t - init_t) >= (CThread.Clocks_Per_Sec >> 5)))
				{
					if ((last_i != 0) && ((get_Generic_Seed_i - last_i) > 4) || ((get_Generic_Seed_i - last_i) > 64) || UnitTest.IsUnitTestEnabled() && ((get_Generic_Seed_i - last_i) > 8))
						break;
				}

				if (t == last_t)
					repeats[0]++;
				else
				{
					// If we got a new unique number of repeats, update the history
					if (repeats[0] != repeats[1])
					{
						repeats[2] = repeats[1];
						repeats[1] = repeats[0];
					}

					repeats[0] = 0;
				}

				last_t = t;
				last_td = cur_td;

				if (init_t == 0)
					init_t = t;
			}

			if (UnitTest.IsUnitTestEnabled())
				get_Generic_Seed_Buffer[0] = get_Generic_Seed_Buffer[1] = 0;
			else
				get_Generic_Seed_Buffer[111] += (uint8_t)Timer.Av_Read_Time();

			Sha.Av_Sha_Init(sha, 160);
			Sha.Av_Sha_Update(sha, MemoryMarshal.Cast<uint32_t, uint8_t>(get_Generic_Seed_Buffer), (size_t)(get_Generic_Seed_Buffer.Length * 4));
			Sha.Av_Sha_Final(sha, digest);

			return IntReadWrite.Av_RB32(digest) + IntReadWrite.Av_RB32(digest + 16);
		}
		#endregion
	}
}
