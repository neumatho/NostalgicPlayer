/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Demuxer
{
	/// <summary>
	/// ASF decryption
	/// </summary>
	internal static class AsfCrypt
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void FF_AsfCrypt_Dec(CPointer<uint8_t> key, CPointer<uint8_t> data, c_int len)//XX 148
		{
			c_int num_Qwords = len >> 3;
			CPointer<uint8_t> qwords = data;
			CPointer<uint8_t> rc4Buff = new CPointer<uint8_t>(8 * 8);
			CPointer<uint32_t> ms_Keys = new CPointer<uint32_t>(12);

			if (len < 16)
			{
				for (c_int i = 0; i < len; i++)
					data[i] ^= key[i];

				return;
			}

			AvDes des = Des.Av_Des_Alloc();
			AvRc4 rc4 = Rc4.Av_Rc4_Alloc();

			if ((des == null) || (rc4 == null))
			{
				Mem.Av_FreeP(ref des);
				Mem.Av_FreeP(ref rc4);

				return;
			}

			Rc4.Av_Rc4_Init(rc4, key, 12 * 8, 1);
			Rc4.Av_Rc4_Crypt(rc4, rc4Buff, null, rc4Buff.Length, null, 1);

			MultiSwap_Init(rc4Buff, ms_Keys);

			uint64_t packetKey = IntReadWrite.Av_RN64(qwords + (num_Qwords * 8) - 8);
			packetKey ^= IntReadWrite.Av_RN64(rc4Buff + (7 * 8));

			CPointer<uint8_t> packetKey_Arr = new CPointer<uint8_t>(8);
			IntReadWrite.Av_WN64(packetKey_Arr, packetKey);

			Des.Av_Des_Init(des, key + 12, 64, 1);
			Des.Av_Des_Crypt(des, packetKey_Arr, packetKey_Arr, 1, null, 1);

			packetKey = IntReadWrite.Av_RN64(packetKey_Arr);
			packetKey ^= IntReadWrite.Av_RN64(rc4Buff + (6 * 8));

			IntReadWrite.Av_WN64(packetKey_Arr, packetKey);

			Rc4.Av_Rc4_Init(rc4, packetKey_Arr, 64, 1);
			Rc4.Av_Rc4_Crypt(rc4, data, data, len, null, 1);

			uint64_t ms_State = 0;

			for (c_int i = 0; i < (num_Qwords - 1); i++, qwords += 8)
				ms_State = MultiSwap_Enc(ms_Keys, ms_State, IntReadWrite.Av_RL64(qwords));

			MultiSwap_Invert_Keys(ms_Keys);

			packetKey = (packetKey << 32) | (packetKey >> 32);
			packetKey = BSwap.Av_Le2Ne64(packetKey);
			packetKey = MultiSwap_Dec(ms_Keys, ms_State, packetKey);

			IntReadWrite.Av_WL64(qwords, packetKey);

			Mem.Av_Free(rc4);
			Mem.Av_Free(des);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Find multiplicative inverse modulo 2 ^ 32
		/// </summary>
		/********************************************************************/
		private static uint32_t Inverse(uint32_t v)//XX 36
		{
			// v ^ 3 gives the inverse (mod 16), could also be implemented
			// as table etc. (only lowest 4 bits matter!)
			uint32_t inverse = v * v * v;

			// Uses a fixpoint-iteration that doubles the number
			// of correct lowest bits each time
			inverse *= 2 - (v * inverse);
			inverse *= 2 - (v * inverse);
			inverse *= 2 - (v * inverse);

			return inverse;
		}



		/********************************************************************/
		/// <summary>
		/// Read keys from keybuf into keys
		/// </summary>
		/********************************************************************/
		private static void MultiSwap_Init(CPointer<uint8_t> keyBuf, CPointer<uint32_t> keys)//XX 55
		{
			for (c_int i = 0; i < 12; i++)
				keys[i] = IntReadWrite.Av_RL32(keyBuf + (i << 2)) | 1;
		}



		/********************************************************************/
		/// <summary>
		/// Invert the keys so that encryption become decryption keys and
		/// the other way round
		/// </summary>
		/********************************************************************/
		private static void MultiSwap_Invert_Keys(CPointer<uint32_t> keys)//XX 67
		{
			for (c_int i = 0; i < 5; i++)
				keys[i] = Inverse(keys[i]);

			for (c_int i = 6; i < 11; i++)
				keys[i] = Inverse(keys[i]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static uint32_t MultiSwap_Step(CPointer<uint32_t> keys, uint32_t v)//XX 76
		{
			v *= keys[0];

			for (c_int i = 1; i < 5; i++)
			{
				v = (v >> 16) | (v << 16);
				v *= keys[i];
			}

			v += keys[5];

			return v;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static uint32_t MultiSwap_Inv_Step(CPointer<uint32_t> keys, uint32_t v)//XX 88
		{
			v -= keys[5];

			for (c_int i = 4; i > 0; i--)
			{
				v *= keys[i];
				v = (v >> 16) | (v << 16);
			}

			v *= keys[0];

			return v;
		}



		/********************************************************************/
		/// <summary>
		/// "MultiSwap" encryption
		/// </summary>
		/********************************************************************/
		private static uint64_t MultiSwap_Enc(CPointer<uint32_t> keys, uint64_t key, uint64_t data)//XX 108
		{
			uint32_t a = (uint32_t)data;
			uint32_t b = (uint32_t)(data >> 32);

			a += (uint32_t)key;
			uint32_t tmp = MultiSwap_Step(keys, a);
			b += tmp;
			uint32_t c = (uint32_t)((key >> 32) + tmp);
			tmp = MultiSwap_Step(keys + 6, b);
			c += tmp;

			return ((uint64_t)c << 32) | tmp;
		}



		/********************************************************************/
		/// <summary>
		/// "MultiSwap" decryption
		/// </summary>
		/********************************************************************/
		private static uint64_t MultiSwap_Dec(CPointer<uint32_t> keys, uint64_t key, uint64_t data)//XX 132
		{
			uint32_t c = (uint32_t)(data >> 32);
			uint32_t tmp = (uint32_t)data;
			c -= tmp;
			uint32_t b = MultiSwap_Inv_Step(keys + 6, tmp);
			tmp = (uint32_t)(c - (key >> 32));
			b -= tmp;
			uint32_t a = MultiSwap_Inv_Step(keys, tmp);
			a -= (uint32_t)key;

			return ((uint64_t)b << 32) | a;
		}
		#endregion
	}
}
