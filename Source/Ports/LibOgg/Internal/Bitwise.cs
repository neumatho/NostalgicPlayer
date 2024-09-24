/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOgg.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOgg.Internal
{
	/// <summary>
	/// Packing variable sized words into an octet stream
	/// </summary>
	internal static class Bitwise
	{
		internal const int Buffer_Increment = 256;

		private delegate void Write(OggPack_Buffer b, c_ulong value, c_int bits);

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void OggPack_WriteInit(out OggPack_Buffer b)
		{
			b = new OggPack_Buffer();

			b.Ptr = b.Buffer = Memory.Ogg_MAlloc<byte>(Buffer_Increment);
			b.Buffer[0] = 0x00;
			b.Storage = Buffer_Increment;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void OggPackB_WriteInit(out OggPack_Buffer b)
		{
			OggPack_WriteInit(out b);
		}



		/********************************************************************/
		/// <summary>
		/// Takes only up to 32 bits
		/// </summary>
		/********************************************************************/
		public static void OggPack_Write(OggPack_Buffer b, c_ulong value, c_int bits)
		{
			if ((bits < 0) || (bits > 32))
				goto Err;

			if ((b.EndByte >= (b.Storage - 4)))
			{
				if (b.Ptr.IsNull)
					return;

				if (b.Storage > (c_long.MaxValue - Buffer_Increment))
					goto Err;

				Pointer<byte> ret = Memory.Ogg_Realloc(b.Buffer, (size_t)b.Storage + Buffer_Increment);
				if (ret.IsNull)
					goto Err;

				b.Buffer = ret;
				b.Storage += Buffer_Increment;
				b.Ptr = b.Buffer + b.EndByte;
			}

			value &= Tables.Mask[bits];
			bits += b.EndBit;

			b.Ptr[0] |= (byte)(value << b.EndBit);

			if (bits >= 8)
			{
				b.Ptr[1] = (byte)(value >> (8 - b.EndBit));

				if (bits >= 16)
				{
					b.Ptr[2] = (byte)(value >> (16 - b.EndBit));

					if (bits >= 24)
					{
						b.Ptr[3] = (byte)(value >> (24 - b.EndBit));

						if (bits >= 32)
						{
							if (b.EndBit != 0)
								b.Ptr[4] = (byte)(value >> (32 - b.EndBit));
							else
								b.Ptr[4] = 0;
						}
					}
				}
			}

			b.EndByte += bits / 8;
			b.Ptr += bits / 8;
			b.EndBit = bits & 7;

			return;

			Err:
			OggPack_WriteClear(b);
		}



		/********************************************************************/
		/// <summary>
		/// Takes only up to 32 bits
		/// </summary>
		/********************************************************************/
		public static void OggPackB_Write(OggPack_Buffer b, c_ulong value, c_int bits)
		{
			if ((bits < 0) || (bits > 32))
				goto Err;

			if ((b.EndByte >= (b.Storage - 4)))
			{
				if (b.Ptr.IsNull)
					return;

				if (b.Storage > (c_long.MaxValue - Buffer_Increment))
					goto Err;

				Pointer<byte> ret = Memory.Ogg_Realloc(b.Buffer, (size_t)b.Storage + Buffer_Increment);
				if (ret.IsNull)
					goto Err;

				b.Buffer = ret;
				b.Storage += Buffer_Increment;
				b.Ptr = b.Buffer + b.EndByte;
			}

			value = (value & Tables.Mask[bits]) << (32 - bits);
			bits += b.EndBit;

			b.Ptr[0] |= (byte)(value >> (24 + b.EndBit));

			if (bits >= 8)
			{
				b.Ptr[1] = (byte)(value >> (16 + b.EndBit));

				if (bits >= 16)
				{
					b.Ptr[2] = (byte)(value >> (8 + b.EndBit));

					if (bits >= 24)
					{
						b.Ptr[3] = (byte)(value >> b.EndBit);

						if (bits >= 32)
						{
							if (b.EndBit != 0)
								b.Ptr[4] = (byte)(value << (8 - b.EndBit));
							else
								b.Ptr[4] = 0;
						}
					}
				}
			}

			b.EndByte += bits / 8;
			b.Ptr += bits / 8;
			b.EndBit = bits & 7;

			return;

			Err:
			OggPack_WriteClear(b);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void OggPack_WriteCopy(OggPack_Buffer b, Pointer<byte> source, c_long bits)
		{
			OggPack_WriteCopy_Helper(b, source, bits, OggPack_Write, false);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void OggPackB_WriteCopy(OggPack_Buffer b, Pointer<byte> source, c_long bits)
		{
			OggPack_WriteCopy_Helper(b, source, bits, OggPackB_Write, true);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void OggPack_Reset(OggPack_Buffer b)
		{
			if (b.Ptr.IsNull)
				return;

			b.Ptr = b.Buffer;
			b.Buffer[0] = 0;
			b.EndBit = b.EndByte = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void OggPackB_Reset(OggPack_Buffer b)
		{
			OggPack_Reset(b);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void OggPack_WriteClear(OggPack_Buffer b)
		{
			if (!b.Buffer.IsNull)
				Memory.Ogg_Free(b.Buffer);

			b.Clear();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void OggPackB_WriteClear(OggPack_Buffer b)
		{
			OggPack_WriteClear(b);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void OggPack_ReadInit(out OggPack_Buffer b, Pointer<byte> buf, c_int bytes)
		{
			b = new OggPack_Buffer();

			b.Buffer = b.Ptr = buf;
			b.Storage = bytes;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void OggPackB_ReadInit(out OggPack_Buffer b, Pointer<byte> buf, c_int bytes)
		{
			OggPack_ReadInit(out b, buf, bytes);
		}



		/********************************************************************/
		/// <summary>
		/// Read in bits without advancing the bitptr; bits less or equal to
		/// 32
		/// </summary>
		/********************************************************************/
		public static c_long OggPack_Look(OggPack_Buffer b, c_int bits)
		{
			if ((bits < 0) || (bits > 32))
				return -1;

			c_ulong m = Tables.Mask[bits];
			bits += b.EndBit;

			if (b.EndByte >= (b.Storage - 4))
			{
				// Not the main path
				if (b.EndByte > (b.Storage - ((bits + 7) >> 3)))
					return -1;

				// Special case to avoid reading b->ptr[0], which might be past the end of
				// the buffer; also skips some useless accounting
				if (bits == 0)
					return 0;
			}

			c_ulong ret = (c_ulong)b.Ptr[0] >> b.EndBit;

			if (bits > 8)
			{
				ret |= (c_ulong)(b.Ptr[1] << (8 - b.EndBit));

				if (bits > 16)
				{
					ret |= (c_ulong)(b.Ptr[2] << (16 - b.EndBit));

					if (bits > 24)
					{
						ret |= (c_ulong)(b.Ptr[3] << (24 - b.EndBit));

						if ((bits > 32) && (b.EndBit != 0))
							ret |= (c_ulong)(b.Ptr[4] << (32 - b.EndBit));
					}
				}
			}

			return (c_long)(m & ret);
		}



		/********************************************************************/
		/// <summary>
		/// Read in bits without advancing the bitptr; bits less or equal to
		/// 32
		/// </summary>
		/********************************************************************/
		public static c_long OggPackB_Look(OggPack_Buffer b, c_int bits)
		{
			c_int m = 32 - bits;

			if ((m < 0) || (m > 32))
				return -1;

			bits += b.EndBit;

			if (b.EndByte >= (b.Storage - 4))
			{
				// Not the main path
				if (b.EndByte > (b.Storage - ((bits + 7) >> 3)))
					return -1;

				// Special case to avoid reading b->ptr[0], which might be past the end of
				// the buffer; also skips some useless accounting
				if (bits == 0)
					return 0;
			}

			c_ulong ret = (c_ulong)b.Ptr[0] << (24 + b.EndBit);

			if (bits > 8)
			{
				ret |= (c_ulong)(b.Ptr[1] << (16 + b.EndBit));

				if (bits > 16)
				{
					ret |= (c_ulong)(b.Ptr[2] << (8 + b.EndBit));

					if (bits > 24)
					{
						ret |= (c_ulong)(b.Ptr[3] << b.EndBit);

						if ((bits > 32) && (b.EndBit != 0))
							ret |= (c_ulong)(b.Ptr[4] >> (8 - b.EndBit));
					}
				}
			}

			return (c_long)(((ret & 0xffffffff) >> (m >> 1)) >> ((m + 1) >> 1));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_long OggPack_Look1(OggPack_Buffer b)
		{
			if (b.EndByte >= b.Storage)
				return -1;

			return (b.Ptr[0] >> b.EndBit) & 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_long OggPackB_Look1(OggPack_Buffer b)
		{
			if (b.EndByte >= b.Storage)
				return -1;

			return (b.Ptr[0] >> (7 - b.EndBit)) & 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void OggPack_Adv(OggPack_Buffer b, c_int bits)
		{
			bits += b.EndBit;

			if (b.EndByte > (b.Storage - ((bits + 7) >> 3)))
				goto Overflow;

			b.Ptr += bits / 8;
			b.EndByte += bits / 8;
			b.EndBit = bits & 7;

			return;

			Overflow:
			b.Ptr.SetToNull();
			b.EndByte = b.Storage;
			b.EndBit = 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void OggPackB_Adv(OggPack_Buffer b, c_int bits)
		{
			OggPack_Adv(b, bits);
		}



		/********************************************************************/
		/// <summary>
		/// Bits less or equal to 32
		/// </summary>
		/********************************************************************/
		public static c_long OggPack_Read(OggPack_Buffer b, c_int bits)
		{
			if ((bits < 0) || (bits > 32))
				goto Err;

			c_ulong m = Tables.Mask[bits];
			bits += b.EndBit;

			if (b.EndByte >= (b.Storage - 4))
			{
				// Not the main path
				if (b.EndByte > (b.Storage - ((bits + 7) >> 3)))
					goto Overflow;

				// Special case to avoid reading b->ptr[0], which might be past the end of
				// the buffer; also skips some useless accounting
				if (bits == 0)
					return 0;
			}

			c_ulong ret = (c_ulong)b.Ptr[0] >> b.EndBit;

			if (bits > 8)
			{
				ret |= (c_ulong)(b.Ptr[1] << (8 - b.EndBit));

				if (bits > 16)
				{
					ret |= (c_ulong)(b.Ptr[2] << (16 - b.EndBit));

					if (bits > 24)
					{
						ret |= (c_ulong)(b.Ptr[3] << (24 - b.EndBit));

						if ((bits > 32) && (b.EndBit != 0))
							ret |= (c_ulong)(b.Ptr[4] << (32 - b.EndBit));
					}
				}
			}

			ret &= m;

			b.Ptr += bits / 8;
			b.EndByte += bits / 8;
			b.EndBit = bits & 7;

			return (c_long)ret;

			Overflow:
			Err:
			b.Ptr.SetToNull();
			b.EndByte = b.Storage;
			b.EndBit = 1;

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// Bits less or equal to 32
		/// </summary>
		/********************************************************************/
		public static c_long OggPackB_Read(OggPack_Buffer b, c_int bits)
		{
			c_long m = 32 - bits;

			if ((m < 0) || (m > 32))
				goto Err;

			bits += b.EndBit;

			if ((b.EndByte + 4) >= b.Storage)
			{
				// Not the main path
				if (b.EndByte > (b.Storage - ((bits + 7) >> 3)))
					goto Overflow;

				// Special case to avoid reading b->ptr[0], which might be past the end of
				// the buffer; also skips some useless accounting
				if (bits == 0)
					return 0;
			}

			c_ulong ret = (c_ulong)b.Ptr[0] << (24 + b.EndBit);

			if (bits > 8)
			{
				ret |= (c_ulong)(b.Ptr[1] << (16 + b.EndBit));

				if (bits > 16)
				{
					ret |= (c_ulong)(b.Ptr[2] << (8 + b.EndBit));

					if (bits > 24)
					{
						ret |= (c_ulong)(b.Ptr[3] << b.EndBit);

						if ((bits > 32) && (b.EndBit != 0))
							ret |= (c_ulong)(b.Ptr[4] >> (8 - b.EndBit));
					}
				}
			}

			ret = ((ret & 0xffffffff) >> (m >> 1)) >> ((m + 1) >> 1);

			b.Ptr += bits / 8;
			b.EndByte += bits / 8;
			b.EndBit = bits & 7;

			return (c_long)ret;

			Overflow:
			Err:
			b.Ptr.SetToNull();
			b.EndByte = b.Storage;
			b.EndBit = 1;

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_long OggPack_Read1(OggPack_Buffer b)
		{
			if (b.EndByte >= b.Storage)
				goto Overflow;

			c_long ret = (b.Ptr[0] >> b.EndBit) & 1;

			b.EndBit++;

			if (b.EndBit > 7)
			{
				b.EndBit = 0;
				b.Ptr++;
				b.EndByte++;
			}

			return ret;

			Overflow:
			b.Ptr.SetToNull();
			b.EndByte = b.Storage;
			b.EndBit = 1;

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_long OggPackB_Read1(OggPack_Buffer b)
		{
			if (b.EndByte >= b.Storage)
				goto Overflow;

			c_long ret = (b.Ptr[0] >> (7 - b.EndBit)) & 1;

			b.EndBit++;

			if (b.EndBit > 7)
			{
				b.EndBit = 0;
				b.Ptr++;
				b.EndByte++;
			}

			return ret;

			Overflow:
			b.Ptr.SetToNull();
			b.EndByte = b.Storage;
			b.EndBit = 1;

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_long OggPack_Bytes(OggPack_Buffer b)
		{
			return b.EndByte + (b.EndBit + 7) / 8;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_long OggPackB_Bytes(OggPack_Buffer b)
		{
			return OggPack_Bytes(b);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Pointer<byte> OggPack_GetBuffer(OggPack_Buffer b)
		{
			return b.Buffer;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Pointer<byte> OggPackB_GetBuffer(OggPack_Buffer b)
		{
			return OggPack_GetBuffer(b);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void OggPack_WriteCopy_Helper(OggPack_Buffer b, Pointer<byte> source, c_long bits, Write w, bool msb)
		{
			Pointer<byte> ptr = source;

			c_long bytes = bits / 8;
			c_long pBytes = (b.EndBit + bits) / 8;
			bits -= bytes * 8;

			// Expand storage up-front
			if ((b.EndByte + pBytes) >= b.Storage)
			{
				if (b.Ptr.IsNull)
					goto Err;

				if (b.Storage > (b.EndByte + pBytes + Buffer_Increment))
					goto Err;

				b.Storage = b.EndByte + pBytes + Buffer_Increment;

				Pointer<byte> ret = Memory.Ogg_Realloc(b.Buffer, (size_t)b.Storage);
				if (ret.IsNull)
					goto Err;

				b.Buffer = ret;
				b.Ptr = b.Buffer + b.EndByte;
			}

			// Copy whole octets
			if (b.EndBit != 0)
			{
				// Unaligned copy. Do it the hard way
				for (c_int i = 0; i < bytes; i++)
					w(b, ptr[i], 8);
			}
			else
			{
				// Aligned block copy
				CMemory.MemMove(b.Ptr, source, bytes);

				b.Ptr += bytes;
				b.EndByte += bytes;
				b.Ptr[0] = 0;
			}

			// Copy trailing bits
			if (bits != 0)
			{
				if (msb)
					w(b, (c_ulong)(ptr[bytes] >> (8 - bits)), bits);
				else
					w(b, ptr[bytes], bits);
			}

			return;

			Err:
			OggPack_WriteClear(b);
		}
		#endregion
	}
}
