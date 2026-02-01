/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class Sha
	{
		private static readonly uint32_t[] k256 =
		[
			0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5,
			0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
			0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3,
			0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
			0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc,
			0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
			0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7,
			0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
			0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13,
			0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
			0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3,
			0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
			0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5,
			0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
			0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208,
			0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static AvSha Av_Sha_Alloc()//XX 46
		{
			return Mem.Av_MAlloczObj<AvSha>();
		}



		/********************************************************************/
		/// <summary>
		/// Initialize SHA-1 or SHA-2 hashing
		/// </summary>
		/********************************************************************/
		public static c_int Av_Sha_Init(AvSha ctx, c_int bits)//XX 274
		{
			ctx.Digest_Len = (uint8_t)(bits >> 5);

			switch (bits)
			{
				// SHA-1
				case 160:
				{
					ctx.State[0] = 0x67452301;
					ctx.State[1] = 0xefcdab89;
					ctx.State[2] = 0x98badcfe;
					ctx.State[3] = 0x10325476;
					ctx.State[4] = 0xc3d2e1f0;
					ctx.Transform = Sha1_Transform;
					break;
				}

				// SHA-224
				case 224:
				{
					ctx.State[0] = 0xc1059ed8;
					ctx.State[1] = 0x367cd507;
					ctx.State[2] = 0x3070dd17;
					ctx.State[3] = 0xf70e5939;
					ctx.State[4] = 0xffc00b31;
					ctx.State[5] = 0x68581511;
					ctx.State[6] = 0x64f98fa7;
					ctx.State[7] = 0xbefa4fa4;
					ctx.Transform = Sha256_Transform;
					break;
				}

				// SHA-256
				case 256:
				{
					ctx.State[0] = 0x6a09e667;
					ctx.State[1] = 0xbb67ae85;
					ctx.State[2] = 0x3c6ef372;
					ctx.State[3] = 0xa54ff53a;
					ctx.State[4] = 0x510e527f;
					ctx.State[5] = 0x9b05688c;
					ctx.State[6] = 0x1f83d9ab;
					ctx.State[7] = 0x5be0cd19;
					ctx.Transform = Sha256_Transform;
					break;
				}

				default:
					return Error.EINVAL;
			}

			ctx.Count = 0;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Update hash value
		/// </summary>
		/********************************************************************/
		public static void Av_Sha_Update(AvSha ctx, Span<uint8_t> data, size_t len)//XX 315
		{
			c_uint j = (c_uint)(ctx.Count & 63);
			ctx.Count += len;

			for (size_t i = 0; i < len; i++)
			{
				ctx.Buffer[j++] = data[(int)i];

				if (j == 64)
				{
					ctx.Transform(ctx.State, ctx.Buffer);
					j = 0;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Finish hashing and output digest value
		/// </summary>
		/********************************************************************/
		public static void Av_Sha_Final(AvSha ctx, CPointer<uint8_t> digest)//XX 347
		{
			uint64_t finalCount = BSwap.Av_Be2Ne64(ctx.Count << 3);

			Av_Sha_Update(ctx, [ 0x80 ], 1);

			while ((ctx.Count & 63) != 56)
				Av_Sha_Update(ctx, [ 0x00 ], 1);

			// Should cause a transform()
			Av_Sha_Update(ctx, BitConverter.GetBytes(finalCount), 8);

			for (c_int i = 0; i < ctx.Digest_Len; i++)
				IntReadWrite.Av_WB32(digest + (i * 4), ctx.State[i]);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_uint Rol(c_uint value, c_int bits)
		{
			return (value << bits) | (value >> (32 - bits));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_uint Ch(c_uint x, c_uint y, c_uint z)
		{
			return (x & (y ^ z)) ^ z;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_uint Maj(c_uint z, c_uint y, c_uint x)
		{
			return ((x | y) & z) | (x & y);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_uint Sigma0_256(c_uint x)
		{
			return Rol(x, 30) ^ Rol(x, 19) ^ Rol(x, 10);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_uint Sigma1_256(c_uint x)
		{
			return Rol(x, 26) ^ Rol(x, 21) ^ Rol(x, 7);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_uint Sigma0_256_(c_uint x)
		{
			return Rol(x, 25) ^ Rol(x, 14) ^ (x >> 3);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_uint Sigma1_256_(c_uint x)
		{
			return Rol(x, 15) ^ Rol(x, 13) ^ (x >> 10);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static uint32_t Blk0(c_uint i, uint32_t[] block, CPointer<uint8_t> buffer)
		{
			return block[i] = IntReadWrite.Av_RB32(buffer + 4 * i);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static uint32_t Blk(c_uint i, uint32_t[] block)
		{
			return block[i] = block[i - 16]+ Sigma0_256_(block[i - 15]) + Sigma1_256_(block[i - 2]) + block[i - 7];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Sha1_Transform(CPointer<uint32_t> state, CPointer<uint8_t> buffer)
		{
			uint32_t[] block = new uint32_t[80];

			c_uint a = state[0];
			c_uint b = state[1];
			c_uint c = state[2];
			c_uint d = state[3];
			c_uint e = state[4];

			for (c_int i = 0; i < 80; i++)
			{
				c_int t;

				if (i < 16)
					t = (c_int)IntReadWrite.Av_RB32(buffer + 4 * i);
				else
					t = (c_int)Rol(block[i - 3] ^ block[i - 8] ^ block[i - 14] ^ block[i - 16], 1);

				block[i] = (c_uint)t;
				t += (c_int)(e + Rol(a, 5));

				if (i < 40)
				{
					if (i < 20)
						t += (c_int)(((b & (c ^ d)) ^ d) + 0x5a827999);
					else
						t += (c_int)((b ^ c ^ d) + 0x6ed9eba1);
				}
				else
				{
					if (i < 60)
						t += (c_int)((((b | c) & d) | (b & c)) + 0x8f1bbcdc);
					else
						t += (c_int)((b ^ c ^ d) + 0xca62c1d6);
				}

				e = d;
				d = c;
				c = Rol(b, 30);
				b = a;
				a = (c_uint)t;
			}

			state[0] += a;
			state[1] += b;
			state[2] += c;
			state[3] += d;
			state[4] += e;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Sha256_Transform(CPointer<uint32_t> state, CPointer<uint8_t> buffer)
		{
			uint32_t[] block = new uint32_t[64];

			c_uint a = state[0];
			c_uint b = state[1];
			c_uint c = state[2];
			c_uint d = state[3];
			c_uint e = state[4];
			c_uint f = state[5];
			c_uint g = state[6];
			c_uint h = state[7];

			for (c_uint i = 0; i < 64; i++)
			{
				uint32_t t1;

				if (i < 16)
					t1 = Blk0(i, block, buffer);
				else
					t1 = Blk(i, block);

				t1 += h + Sigma1_256(e) + Ch(e, f, g) + k256[i];
				uint32_t t2 = Sigma0_256(a) + Maj(a, b, c);

				h = g;
				g = f;
				f = e;
				e = d + t1;
				d = c;
				c = b;
				b = a;
				a = t1 + t2;
			}

			state[0] += a;
			state[1] += b;
			state[2] += c;
			state[3] += d;
			state[4] += e;
			state[5] += f;
			state[6] += g;
			state[7] += h;
		}
		#endregion
	}
}
