/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp
{
	/// <summary>
	/// Reloc65 -- A part of xa65 - 65xx/65816 cross-assembler and utility suite
	/// o65 file relocator. Trimmed down for our needs
	/// </summary>
	internal class Reloc65
	{
		/// <summary>
		/// 16 bit header
		/// </summary>
		private const int HEADER_SIZE = 8 + (9 * 2);

		private static readonly byte[] o65Hdr = [ 1, 0, (byte)'o', (byte)'6', (byte)'5' ];

		private readonly int tBase;
		private int tDiff;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Reloc65(int addr)
		{
			tBase = addr;
		}



		/********************************************************************/
		/// <summary>
		/// Do the relocation
		/// </summary>
		/********************************************************************/
		public bool Reloc(ref CPointer<byte> buf, ref int fSize)
		{
			CPointer<byte> tmpBuf = buf;

			if (CMemory.memcmp(tmpBuf, o65Hdr, 5) != 0)
				return false;

			int mode = GetWord(tmpBuf, 6);

			// 32 bit size not supported + pagewise relocation not support
			if (((mode & 0x2000) != 0) || ((mode & 0x4000) != 0))
				return false;

			int hLen = HEADER_SIZE + Read_Options(tmpBuf + HEADER_SIZE);

			int tBase = GetWord(tmpBuf, 8);
			int tLen = GetWord(tmpBuf, 10);
			tDiff = this.tBase - tBase;

			int dLen = GetWord(tmpBuf, 14);

			CPointer<byte> segT = tmpBuf + hLen;						// Text segment
			CPointer<byte> segD = segT + tLen;							// Data segment
			CPointer<byte> uTab = segD + dLen;							// Undefined references list

			CPointer<byte> rtTab = uTab + Read_Undef(uTab);				// Text relocation table

			CPointer<byte> rdTab = Reloc_Seg(segT, tLen, rtTab);		// Data relocation
			CPointer<byte> exTab = Reloc_Seg(segD, dLen, rdTab);		// Exported globals list

			Reloc_Globals(exTab);

			SetWord(tmpBuf, 8, this.tBase);

			buf = segT;
			fSize = tLen;

			return true;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Read a 16 bit word from a buffer at specific location
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int GetWord(CPointer<byte> buffer, int idx)
		{
			return buffer[idx] | (buffer[idx + 1] << 8);
		}



		/********************************************************************/
		/// <summary>
		/// Write a 16 bit word into a buffer at specific location
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetWord(CPointer<byte> buffer, int idx, int value)
		{
			buffer[idx] = (byte)(value & 0xff);
			buffer[idx + 1] = (byte)((value >> 8) & 0xff);
		}



		/********************************************************************/
		/// <summary>
		/// Get the size of header options section
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int Read_Options(CPointer<byte> buf)
		{
			int l = 0;

			int c = buf[0];
			while (c != 0)
			{
				l += c;
				c = buf[l];
			}

			return ++l;
		}



		/********************************************************************/
		/// <summary>
		/// Get the size of undefined references list
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int Read_Undef(CPointer<byte> buf)
		{
			int l = 2;

			int n = GetWord(buf, 0);
			while (n != 0)
			{
				n--;
				while (buf[l++] == 0)
				{
				}
			}

			return l;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int RelDiff(byte s)
		{
			return s == 2 ? tDiff : 0;
		}



		/********************************************************************/
		/// <summary>
		/// Relocate segment
		/// </summary>
		/********************************************************************/
		private CPointer<byte> Reloc_Seg(CPointer<byte> buf, int len, CPointer<byte> rTab)
		{
			int adr = -1;

			while (rTab[0] != 0)
			{
				if ((rTab[0] & 255) == 255)
				{
					adr += 254;
					rTab++;
				}
				else
				{
					adr += rTab[0] & 255;
					rTab++;
					byte type = (byte)(rTab[0] & 0xe0);
					byte seg = (byte)(rTab[0] & 0x07);
					rTab++;

					switch (type)
					{
						case 0x80:
						{
							int oldVal = GetWord(buf, adr);
							int newVal = oldVal + RelDiff(seg);
							SetWord(buf, adr, newVal);
							break;
						}

						case 0x40:
						{
							int oldVal = buf[adr] + (256 * rTab[0]);
							int newVal = oldVal + RelDiff(seg);
							buf[adr] = (byte)((newVal >> 8) & 255);
							rTab[0] = (byte)(newVal & 255);
							rTab++;
							break;
						}

						case 0x20:
						{
							int oldVal = buf[adr];
							int newVal = oldVal + RelDiff(seg);
							buf[adr] = (byte)(newVal & 255);
							break;
						}
					}

					if (seg == 0)
						rTab += 2;
				}
			}

			return ++rTab;
		}



		/********************************************************************/
		/// <summary>
		/// Relocate exported globals list
		/// </summary>
		/********************************************************************/
		private CPointer<byte> Reloc_Globals(CPointer<byte> buf)
		{
			int n = GetWord(buf, 0);
			buf += 2;

			while (n != 0)
			{
				while (buf[0] != 0)
				{
					buf++;
				}

				buf++;
				byte seg = buf[0];
				int oldVal = GetWord(buf, 1);
				int newVal = oldVal + RelDiff(seg);

				SetWord(buf, 1, newVal);
				buf += 3;
				n--;
			}

			return buf;
		}
		#endregion
	}
}
