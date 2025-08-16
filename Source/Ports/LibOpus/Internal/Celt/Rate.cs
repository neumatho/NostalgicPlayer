/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Rate
	{
		private const int Alloc_Steps = 6;

		private static readonly byte[] log2_frac_table =
		[
			 0,
			 8, 13,
			16, 19, 21, 23,
			24, 26, 27, 28, 29, 30, 31, 32,
			32, 33, 34, 34, 35, 36, 36, 37, 37
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Get_Pulses(c_int i)
		{
			return i < 8 ? i : (8 + (i & 7)) << ((i >> 3) - 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Bits2Pulses(CeltMode m, c_int band, c_int LM, c_int bits)
		{
			LM++;
			CPointer<byte> cache = m.cache.bits + m.cache.index[LM * m.nbEBands + band];

			c_int lo = 0;
			c_int hi = cache[0];
			bits--;

			for (c_int i = 0; i < Constants.Log_Max_Pseudo; i++)
			{
				c_int mid = (lo + hi + 1) >> 1;

				// OPT: Make sure this is implemented with a conditional move
				if (cache[mid] >= bits)
					hi = mid;
				else
					lo = mid;
			}

			if ((bits - (lo == 0 ? -1 : cache[lo])) <= (cache[hi] - bits))
				return lo;
			else
				return hi;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Pulses2Bits(CeltMode m, c_int band, c_int LM, c_int pulses)
		{
			LM++;
			CPointer<byte> cache = m.cache.bits + m.cache.index[LM * m.nbEBands + band];

			return pulses == 0 ? 0 : cache[pulses] + 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_int Interp_Bits2Pulses(CeltMode m, c_int start, c_int end, c_int skip_start, CPointer<c_int> bits1, CPointer<c_int> bits2, CPointer<c_int> thresh, CPointer<c_int> cap,
												opus_int32 total, out opus_int32 _balance, c_int skip_rsv, ref c_int intensity, c_int intensity_rsv, ref bool dual_stereo, c_int dual_stereo_rsv,
												CPointer<c_int> bits, CPointer<c_int> ebits, CPointer<bool> fine_priority, c_int C, c_int LM, Ec_Ctx ec, bool encode, c_int prev, c_int signalBandwidth)
		{
			opus_int32 psum;
			c_int j;
			c_int codedBands = -1;
			opus_int32 left, percoeef;
			bool done;

			c_int alloc_floor = C << Constants.BitRes;
			c_int stereo = C > 1 ? 1 : 0;

			c_int logM = LM << Constants.BitRes;
			c_int lo = 0;
			c_int hi = 1 << Alloc_Steps;

			for (c_int i = 0; i < Alloc_Steps; i++)
			{
				c_int mid = (lo + hi) >> 1;
				psum = 0;
				done = false;

				for (j = end; j-- > start;)
				{
					c_int tmp = bits1[j] + (mid * bits2[j] >> Alloc_Steps);

					if ((tmp >= thresh[j]) || done)
					{
						done = true;

						// Don't allocate more than we can actually use
						psum += Arch.IMIN(tmp, cap[j]);
					}
					else
					{
						if (tmp >= alloc_floor)
							psum += alloc_floor;
					}
				}

				if (psum > total)
					hi = mid;
				else
					lo = mid;
			}

			psum = 0;
			done = false;

			for (j = end; j-- > start;)
			{
				c_int tmp = bits1[j] + (lo * bits2[j] >> Alloc_Steps);

				if ((tmp < thresh[j]) && !done)
				{
					if (tmp >= alloc_floor)
						tmp = alloc_floor;
					else
						tmp = 0;
				}
				else
					done = true;

				// Don't allocate more than we can actually use
				tmp = Arch.IMIN(tmp, cap[j]);
				bits[j] = tmp;
				psum += tmp;
			}

			// Decide which bands to skip, working backwards from the end
			for (codedBands = end;; codedBands--)
			{
				j = codedBands - 1;

				// Never skip the first band, nor a band that has been boosted by
				// dynalloc.
				//
				// In the first case, we'd be coding a bit to signal we're going to waste
				// all the other bits.
				//
				// In the second case, we'd be coding a bit to redistribute all the bits
				// we just signaled should be concentrated in this band
				if (j <= skip_start)
				{
					// Give the bit we reserved to end skipping back
					total += skip_rsv;
					break;
				}

				// Figure out how many left-over bits we would be adding to this band.
				// This can include bits we've stolen back from higher, skipped bands
				left = total - psum;
				percoeef = (c_int)EntCode.Celt_UDiv((opus_uint32)left, (opus_uint32)(m.eBands[codedBands] - m.eBands[start]));
				left -= (m.eBands[codedBands] - m.eBands[start]) * percoeef;
				c_int rem = Arch.IMAX(left - (m.eBands[j] - m.eBands[start]), 0);
				c_int band_width = m.eBands[codedBands] - m.eBands[j];
				c_int band_bits = bits[j] + percoeef * band_width + rem;

				// Only code a skip decision if we're above the threshold for this band.
				// Otherwise it is force-skipped.
				// This ensures that we have enough bits to code the skip flag
				if (band_bits >= Arch.IMAX(thresh[j], alloc_floor + (1 << Constants.BitRes)))
				{
					if (encode)
					{
						// This if() block is the only part of the allocation function that
						// is not a mandatory part of the bitstream: any bands we choose to
						// skip here must be explicitly signaled
						c_int depth_threshold;

						// We choose a threshold with some hysteresis to keep bands from
						// fluctuating in and out, but we try not to fold below a certain point
						if (codedBands > 17)
							depth_threshold = j < prev ? 7 : 9;
						else
							depth_threshold = 0;

						if ((codedBands <= (start + 2)) || ((band_bits > (depth_threshold * band_width << LM << Constants.BitRes) >> 4) && (j <= signalBandwidth)))
						{
							EntEnc.Ec_Enc_Bit_Logp(ec, true, 1);
							break;
						}

						EntEnc.Ec_Enc_Bit_Logp(ec, false, 1);
					}
					else if (EntDec.Ec_Dec_Bit_Logp(ec, 1))
						break;

					// We used a bit to skip this band
					psum += 1 << Constants.BitRes;
					band_bits -= 1 << Constants.BitRes;
				}

				// Reclaim the bits originally allocated to this band
				psum -= bits[j] + intensity_rsv;

				if (intensity_rsv > 0)
					intensity_rsv = log2_frac_table[j - start];

				psum += intensity_rsv;

				if (band_bits >= alloc_floor)
				{
					// If we have enough for a fine energy bit per channel, use it
					psum += alloc_floor;
					bits[j] = alloc_floor;
				}
				else
				{
					// Otherwise this band gets nothing at all
					bits[j] = 0;
				}
			}

			// Code the intensity and dual stereo parameters
			if (intensity_rsv > 0)
			{
				if (encode)
				{
					intensity = Arch.IMIN(intensity, codedBands);
					EntEnc.Ec_Enc_UInt(ec, (opus_uint32)(intensity - start), (opus_uint32)(codedBands + 1 - start));
				}
				else
					intensity = (c_int)(start + EntDec.Ec_Dec_UInt(ec, (opus_uint32)(codedBands + 1 - start)));
			}
			else
				intensity = 0;

			if (intensity <= start)
			{
				total += dual_stereo_rsv;
				dual_stereo_rsv = 0;
			}

			if (dual_stereo_rsv > 0)
			{
				if (encode)
					EntEnc.Ec_Enc_Bit_Logp(ec, dual_stereo, 1);
				else
					dual_stereo = EntDec.Ec_Dec_Bit_Logp(ec, 1);
			}
			else
				dual_stereo = false;

			// Allocate the remaining bits
			left = total - psum;
			percoeef = (c_int)EntCode.Celt_UDiv((opus_uint32)left, (opus_uint32)(m.eBands[codedBands] - m.eBands[start]));
			left -= (m.eBands[codedBands] - m.eBands[start]) * percoeef;

			for (j = start; j < codedBands; j++)
				bits[j] += percoeef * (m.eBands[j + 1] - m.eBands[j]);

			for (j = start; j < codedBands; j++)
			{
				c_int tmp = Arch.IMIN(left, m.eBands[j + 1] - m.eBands[j]);
				bits[j] += tmp;
				left -= tmp;
			}

			opus_int32 balance = 0;

			for (j = start; j < codedBands; j++)
			{
				c_int N0 = m.eBands[j + 1] - m.eBands[j];
				c_int N = N0 << LM;
				opus_int32 bit = bits[j] + balance;
				opus_int32 excess;

				if (N > 1)
				{
					excess = Arch.IMAX(bit - cap[j], 0);
					bits[j] = bit - excess;

					// Compensate for the extra DoF in stereo
					c_int den = C * N + ((C == 2) && (N > 2) && !dual_stereo && (j < intensity) ? 1 : 0);

					c_int NClogN = den * (m.logN[j] + logM);

					// Offset for the number of fine bits by log2(N)/2 + FINE_OFFSET
					// compared to their "fair share" of total/N
					c_int offset = (NClogN >> 1) - den * Constants.Fine_Offset;

					// N=2 is the only point that doesn't match the curve
					if (N == 2)
						offset += den << Constants.BitRes >> 2;

					// Changing the offset for allocating the second and third
					// fine energy bit
					if ((bits[j] + offset) < (den * 2 << Constants.BitRes))
						offset += NClogN >> 2;
					else if ((bits[j] + offset) < (den * 3 << Constants.BitRes))
						offset += NClogN >> 3;

					// Divide with rounding
					ebits[j] = Arch.IMAX(0, bits[j] + offset + (den << (Constants.BitRes - 1)));
					ebits[j] = (c_int)EntCode.Celt_UDiv((opus_uint32)ebits[j], (opus_uint32)den) >> Constants.BitRes;

					// Make sure not to bust
					if ((C * ebits[j]) > (bits[j] >> Constants.BitRes))
						ebits[j] = bits[j] >> stereo >> Constants.BitRes;

					// More than that is useless because that's about as far as PVQ can go
					ebits[j] = Arch.IMIN(ebits[j], Constants.Max_Fine_Bits);

					// If we rounded down or capped this band, make it a candidate for the
					// final fine energy pass
					fine_priority[j] = (ebits[j] * (den << Constants.BitRes)) >= (bits[j] + offset);

					// Remove the allocated fine bits; the rest are assigned to PVQ
					bits[j] -= C * ebits[j] << Constants.BitRes;
				}
				else
				{
					// For N=1, all bits go to fine energy except for a single sign bit
					excess = Arch.IMAX(0, bit - (C << Constants.BitRes));
					bits[j] = bit - excess;
					ebits[j] = 0;
					fine_priority[j] = true;
				}

				// Fine energy can't take advantage of the re-balancing in
				//  quant_all_bands().
				// Instead, do the re-balancing here
				if (excess > 0)
				{
					c_int extra_fine = Arch.IMIN(excess >> (stereo + Constants.BitRes), Constants.Max_Fine_Bits - ebits[j]);
					ebits[j] += extra_fine;
					c_int extra_bits = extra_fine * C << Constants.BitRes;
					fine_priority[j] = extra_bits >= (excess - balance);
					excess -= extra_bits;
				}

				balance = excess;
			}

			// Save any remaining bits over the cap for the rebalancing in
			// quant_all_bands()
			_balance = balance;

			// The skipped bands use all their bits for fine energy
			for (; j < end; j++)
			{
				ebits[j] = bits[j] >> stereo >> Constants.BitRes;
				bits[j] = 0;
				fine_priority[j] = ebits[j] < 1;
			}

			return codedBands;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Clt_Compute_Allocation(CeltMode m, c_int start, c_int end, CPointer<c_int> offsets, CPointer<c_int> cap, c_int alloc_trim, ref c_int intensity, ref bool dual_stereo,
													opus_int32 total, out opus_int32 balance, CPointer<c_int> pulses, CPointer<c_int> ebits, CPointer<bool> fine_priority,
													c_int C, c_int LM, Ec_Ctx ec, bool encode, c_int prev, c_int signalBandwidth)
		{
			total = Arch.IMAX(total, 0);
			c_int len = m.nbEBands;
			c_int skip_start = start;

			// Reserve a bit to signal the end of manually skipped bands
			c_int skip_rsv = total >= 1 << Constants.BitRes ? 1 << Constants.BitRes : 0;
			total -= skip_rsv;

			// Reserve bits for the intensity and dual stereo parameters
			c_int intensity_rsv = 0, dual_stereo_rsv = 0;

			if (C == 2)
			{
				intensity_rsv = log2_frac_table[end - start];

				if (intensity_rsv > total)
					intensity_rsv = 0;
				else
				{
					total -= intensity_rsv;
					dual_stereo_rsv = total >= 1 << Constants.BitRes ? 1 << Constants.BitRes : 0;
					total -= dual_stereo_rsv;
				}
			}

			c_int[] bits1 = new c_int[len];
			c_int[] bits2 = new c_int[len];
			c_int[] thresh = new c_int[len];
			c_int[] trim_offset = new c_int[len];

			for (c_int j = start; j < end; j++)
			{
				// Below this threshold, we're sure not to allocate any PVQ bits
				thresh[j] = Arch.IMAX(C << Constants.BitRes, (3 * (m.eBands[j + 1] - m.eBands[j]) << LM << Constants.BitRes) >> 4);

				// Tilt of the allocation curve
				trim_offset[j] = C * (m.eBands[j + 1] - m.eBands[j]) * (alloc_trim - 5 - LM) * (end - j - 1) * (1 << (LM + Constants.BitRes)) >> 6;

				// Giving less resolution to single-coefficient bands because they get
				// more benefit from having one coarse value per coefficient
				if (((m.eBands[j + 1] - m.eBands[j]) << LM) == 1)
					trim_offset[j] -= C << Constants.BitRes;
			}

			c_int lo = 1;
			c_int hi = m.nbAllocVectors - 1;

			do
			{
				bool done = false;
				c_int psum = 0;
				c_int mid = (lo + hi) >> 1;

				for (c_int j = end; j-- > start;)
				{
					c_int N = m.eBands[j + 1] - m.eBands[j];
					c_int bitsj = C * N * m.allocVectors[mid * len + j] << LM >> 2;

					if (bitsj > 0)
						bitsj = Arch.IMAX(0, bitsj + trim_offset[j]);

					bitsj += offsets[j];

					if ((bitsj >= thresh[j]) || done)
					{
						done = true;

						// Don't allocate more than we can actually use
						psum += Arch.IMIN(bitsj, cap[j]);
					}
					else
					{
						if (bitsj >= (C << Constants.BitRes))
							psum += C << Constants.BitRes;
					}
				}

				if (psum > total)
					hi = mid - 1;
				else
					lo = mid + 1;
			}
			while (lo <= hi);

			hi = lo--;

			for (c_int j = start; j < end; j++)
			{
				c_int N = m.eBands[j + 1] - m.eBands[j];
				c_int bits1j = C * N * m.allocVectors[lo * len + j] << LM >> 2;
				c_int bits2j = hi >= m.nbAllocVectors ? cap[j] : C * N * m.allocVectors[hi * len + j] << LM >> 2;

				if (bits1j > 0)
					bits1j = Arch.IMAX(0, bits1j + trim_offset[j]);

				if (bits2j > 0)
					bits2j = Arch.IMAX(0, bits2j + trim_offset[j]);

				if (lo > 0)
					bits1j += offsets[j];

				bits2j += offsets[j];

				if (offsets[j] > 0)
					skip_start = j;

				bits2j = Arch.IMAX(0, bits2j - bits1j);
				bits1[j] = bits1j;
				bits2[j] = bits2j;
			}

			c_int codedBands = Interp_Bits2Pulses(m, start, end, skip_start, bits1, bits2, thresh, cap, total, out balance, skip_rsv, ref intensity, intensity_rsv, ref dual_stereo, dual_stereo_rsv, pulses, ebits, fine_priority, C, LM, ec, encode, prev, signalBandwidth);

			return codedBands;
		}
	}
}
