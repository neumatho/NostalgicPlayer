/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp
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
		private const int HEADER_SIZE = 8 + 9 * 2;

		private static readonly byte[] o65Hdr = { 1, 0, (byte)'o', (byte)'6', (byte)'5' };

		private int tBase;
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
		public bool Reloc(ref byte[] buf, ref int fSize)
		{
			byte[] tmpBuf = buf;

			if (!tmpBuf.AsSpan(0, o65Hdr.Length).SequenceEqual(o65Hdr))
				return false;

			int mode = GetWord(tmpBuf, 6);

			// 32 bit size not supported + pagewise relocation not support
			if (((mode & 0x2000) != 0) || ((mode & 0x4000) != 0))
				return false;

			int hLen = HEADER_SIZE + Read_Options(tmpBuf, HEADER_SIZE);

			int tBase = GetWord(tmpBuf, 8);
			int tLen = GetWord(tmpBuf, 10);
			tDiff = this.tBase - tBase;

			int dLen = GetWord(tmpBuf, 14);

			int segT = hLen;										// Text segment
			int segD = segT + tLen;									// Data segment
			int uTab = segD + dLen;									// Undefined references list

			int rtTab = uTab + Read_Undef(tmpBuf, uTab);			// Text relocation table

			int rdTab = Reloc_Seg(tmpBuf, segT, tLen, rtTab);		// Data relocation
			int exTab = Reloc_Seg(tmpBuf, segD, dLen, rdTab);		// Exported globals list

			Reloc_Globals(tmpBuf, exTab);

			SetWord(tmpBuf, 8, this.tBase);

			buf = tmpBuf.AsSpan(segT).ToArray();
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
		private int GetWord(byte[] buffer, int idx)
		{
			return buffer[idx] | (buffer[idx + 1] << 8);
		}



		/********************************************************************/
		/// <summary>
		/// Write a 16 bit word into a buffer at specific location
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetWord(byte[] buffer, int idx, int value)
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
		private int Read_Options(byte[] buf, int offset)
		{
			int l = 0;

			int c = buf[offset];
			while (c != 0)
			{
				l += c;
				c = buf[offset + l];
			}

			return ++l;
		}



		/********************************************************************/
		/// <summary>
		/// Get the size of undefined references list
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int Read_Undef(byte[] buf, int offset)
		{
			int l = 2;

			int n = GetWord(buf, offset);
			while (n != 0)
			{
				n--;
				while (buf[offset + l++] == 0)
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
		private int Reloc_Seg(byte[] buf, int offset, int len, int rTab)
		{
			int adr = -1;

			while (buf[rTab] != 0)
			{
				if ((buf[rTab] & 255) == 255)
				{
					adr += 254;
					rTab++;
				}
				else
				{
					adr += buf[rTab] & 255;
					rTab++;
					byte type = (byte)(buf[rTab] & 0xe0);
					byte seg = (byte)(buf[rTab] & 0x07);
					rTab++;

					switch (type)
					{
						case 0x80:
						{
							int oldVal = GetWord(buf, offset + adr);
							int newVal = oldVal + RelDiff(seg);
							SetWord(buf, offset + adr, newVal);
							break;
						}

						case 0x40:
						{
							int oldVal = buf[offset + adr] + 256 * buf[rTab];
							int newVal = oldVal + RelDiff(seg);
							buf[offset + adr] = (byte)((newVal >> 8) & 255);
							buf[rTab] = (byte)(newVal & 255);
							rTab++;
							break;
						}

						case 0x20:
						{
							int oldVal = buf[offset + adr];
							int newVal = oldVal + RelDiff(seg);
							buf[offset + adr] = (byte)(newVal & 255);
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
		private int Reloc_Globals(byte[] buf, int offset)
		{
			int n = GetWord(buf, offset);
			offset += 2;

			while (n != 0)
			{
				while (buf[offset++] != 0)
				{
				}

				byte seg = buf[offset];
				int oldVal = GetWord(buf, offset + 1);
				int newVal = oldVal + RelDiff(seg);

				SetWord(buf, offset + 1, newVal);
				offset += 3;
				n--;
			}

			return offset;
		}
		#endregion
	}
}
