/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// RC4 encryption/decryption/pseudo-random number generator
	/// </summary>
	public static class Rc4
	{
		/********************************************************************/
		/// <summary>
		/// Allocate an AVRC4 context
		/// </summary>
		/********************************************************************/
		public static AvRc4 Av_Rc4_Alloc()//XX 29
		{
			return Mem.Av_MAlloczObj<AvRc4>();
		}



		/********************************************************************/
		/// <summary>
		/// Initializes an AVRC4 context
		/// </summary>
		/********************************************************************/
		public static c_int Av_Rc4_Init(AvRc4 r, CPointer<uint8_t> key, c_int key_Bits, c_int decrypt)//XX 34
		{
			CPointer<uint8_t> state = r.State;
			c_int keyLen = key_Bits >> 3;

			if ((key_Bits & 7) != 0)
				return Error.EINVAL;

			for (c_int i = 0; i < 256; i++)
				state[i] = (uint8_t)i;

			uint8_t y = 0;

			// j is i % keylen
			for (c_int j = 0, i = 0; i < 256; i++, j++)
			{
				if (j == keyLen)
					j = 0;

				y += (uint8_t)(state[i] + key[j]);

				Macros.FFSwap(ref state[i], ref state[y]);
			}

			r.X = 1;
			r.Y = state[1];

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Encrypts / decrypts using the RC4 algorithm
		/// </summary>
		/********************************************************************/
		public static void Av_Rc4_Crypt(AvRc4 r, CPointer<uint8_t> dst, CPointer<uint8_t> src, c_int count, CPointer<uint8_t> iv, c_int decrypt)//XX 55
		{
			uint8_t x = (uint8_t)r.X;
			uint8_t y = (uint8_t)r.Y;
			CPointer<uint8_t> state = r.State;

			while (count-- > 0)
			{
				uint8_t sum = (uint8_t)(state[x] + state[y]);

				Macros.FFSwap(ref state[x], ref state[y]);

				dst[0, 1] = (uint8_t)(src.IsNotNull ? src[0, 1] ^ state[sum] : state[sum]);
				x++;
				y += state[x];
			}

			r.X = x;
			r.Y = y;
		}
	}
}
