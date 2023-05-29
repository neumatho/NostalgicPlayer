/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123
{
	/// <summary>
	/// 
	/// </summary>
	internal class GetBits
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void BackBits(Mpg123_Handle fr, c_int nob)
		{
			fr.Bits_Avail += nob;
			fr.BitIndex -= nob;
			fr.WordPointerIndex += fr.BitIndex >> 3;
			fr.BitIndex &= 7;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public c_int GetBitOffset(Mpg123_Handle fr)
		{
			return (-fr.BitIndex) & 0x7;
		}



		/********************************************************************/
		/// <summary>
		/// Precomputing the bytes to be read is error-prone, and some over-
		/// read is even expected for Huffman. Just play safe and return
		/// zeros in case of overflow. This assumes you made bitindex zero
		/// already
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public c_uint GetByte(Mpg123_Handle fr)
		{
			fr.Bits_Avail -= 8;
			return fr.Bits_Avail >= 0 ? fr.WordPointer[fr.WordPointerIndex++] : 0U;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public c_uint GetBits_(Mpg123_Handle fr, c_int number_Of_Bits)
		{
			fr.Bits_Avail -= number_Of_Bits;

			// Safety catch until we got the nasty code fully figured out.
			// No, that catch stays here, even if we think we got it figured out!
			if (fr.Bits_Avail < 0)
				return 0;

			c_ulong rVal;

			{
				rVal = fr.WordPointer[fr.WordPointerIndex];
				rVal <<= 8;
				rVal |= fr.WordPointer[fr.WordPointerIndex + 1];
				rVal <<= 8;
				rVal |= fr.WordPointer[fr.WordPointerIndex + 2];

				rVal <<= fr.BitIndex;
				rVal &= 0xffffff;

				fr.BitIndex += number_Of_Bits;

				rVal >>= (24 - number_Of_Bits);

				fr.WordPointerIndex += fr.BitIndex >> 3;
				fr.BitIndex &= 7;
			}

			return rVal;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SkipBits(Mpg123_Handle fr, c_int nob)
		{
			fr.UlTmp = fr.WordPointer[fr.WordPointerIndex];
			fr.UlTmp <<= 8;
			fr.UlTmp |= fr.WordPointer[fr.WordPointerIndex + 1];
			fr.UlTmp <<= 8;
			fr.UlTmp |= fr.WordPointer[fr.WordPointerIndex + 2];
			fr.UlTmp <<= fr.BitIndex;
			fr.UlTmp &= 0xffffff;
			fr.BitIndex += nob;
			fr.Bits_Avail -= nob;
			fr.UlTmp >>= (24 - nob);
			fr.WordPointerIndex += fr.BitIndex >> 3;
			fr.BitIndex &= 7;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public c_uint GetBits_Fast(Mpg123_Handle fr, c_int nob)
		{
			fr.UlTmp = (c_uchar)(fr.WordPointer[fr.WordPointerIndex] << fr.BitIndex);
			fr.UlTmp |= ((c_ulong)fr.WordPointer[fr.WordPointerIndex + 1] << fr.BitIndex) >> 8;
			fr.UlTmp <<= nob;
			fr.UlTmp >>= 8;
			fr.BitIndex += nob;
			fr.Bits_Avail -= nob;
			fr.WordPointerIndex += fr.BitIndex >> 3;
			fr.BitIndex &= 7;

			return fr.UlTmp;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public c_uint Get1Bit(Mpg123_Handle fr)
		{
			fr.UcTmp = (byte)(fr.WordPointer[fr.WordPointerIndex] << fr.BitIndex);
			++fr.BitIndex;
			--fr.Bits_Avail;
			fr.WordPointerIndex += fr.BitIndex >> 3;
			fr.BitIndex &= 7;

			return (c_uint)(fr.UcTmp >> 7);
		}
	}
}
