/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Lha.Containers;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Lha
{
	internal partial class LhaCore
	{
		private class DecodeOption
		{
			public Func<ushort> DecodeC;
			public Func<ushort> DecodeP;
			public Action DecodeStart;
		}

		private int origSize;
		private int compSize;

		private uint count;
		private uint loc;

		private ushort dicBit;
		private uint dicSiz;
		private uint dicSiz1;
		private uint offset;

		private byte[] dText;

		private DecodeOption decodeSet;

		private uint decodeI, decodeJ, decodeK;

		private int bufIndex;
		private int bytesLeft;
		private int decodeMode;

		/********************************************************************/
		/// <summary>
		/// Initialize decoder
		/// </summary>
		/********************************************************************/
		public void InitializeDecoder(ushort dicBit, int decruncedSize, int crunchedSize, int method)
		{
			origSize = decruncedSize;
			compSize = crunchedSize;
			this.dicBit = dicBit;

			switch (method)
			{
				case Constants.LzHuff1_Method_Num:
				{
					decodeSet = new DecodeOption { DecodeC = DecodeCDyn, DecodeP = DecodePSt0, DecodeStart = DecodeStartFix };
					break;
				}

				case Constants.LzHuff2_Method_Num:
				{
					decodeSet = new DecodeOption { DecodeC = DecodeCDyn, DecodeP = DecodePDyn, DecodeStart = DecodeStartDyn };
					break;
				}

				case Constants.LzHuff3_Method_Num:
				{
					decodeSet = new DecodeOption { DecodeC = DecodeCSt0, DecodeP = DecodePSt0, DecodeStart = DecodeStartSt0 };
					break;
				}

				case Constants.LzHuff4_Method_Num:
				{
					decodeSet = new DecodeOption { DecodeC = DecodeCSt1, DecodeP = DecodePSt1, DecodeStart = DecodeStartSt1 };
					break;
				}

				case Constants.LzHuff5_Method_Num:
				{
					decodeSet = new DecodeOption { DecodeC = DecodeCSt1, DecodeP = DecodePSt1, DecodeStart = DecodeStartSt1 };
					break;
				}

				case Constants.LzHuff6_Method_Num:
				{
					decodeSet = new DecodeOption { DecodeC = DecodeCSt1, DecodeP = DecodePSt1, DecodeStart = DecodeStartSt1 };
					break;
				}

				case Constants.LzHuff7_Method_Num:
				{
					decodeSet = new DecodeOption { DecodeC = DecodeCSt1, DecodeP = DecodePSt1, DecodeStart = DecodeStartSt1 };
					break;
				}

				case Constants.Larc_Method_Num:
				{
					decodeSet = new DecodeOption { DecodeC = DecodeCLzs, DecodeP = DecodePLzs, DecodeStart = DecodeStartLzs };
					break;
				}

				case Constants.Larc5_Method_Num:
				{
					decodeSet = new DecodeOption { DecodeC = DecodeCLz5, DecodeP = DecodePLz5, DecodeStart = DecodeStartLz5 };
					break;
				}
			}

			dicSiz = (uint)1 << dicBit;

			dText = new byte[dicSiz];
			Array.Fill<byte>(dText, 0x20);

			decodeSet.DecodeStart();

			dicSiz1 = dicSiz - 1;
			offset = (uint)(method == Constants.Larc_Method_Num ? 0x100 - 2 : 0x100 - 3);
			count = 0;
			loc = 0;
			bytesLeft = 0;
			decodeMode = 0;
			bufIndex = 0;
			decodeK = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Read some decrunched data
		/// </summary>
		/********************************************************************/
		public int Decode(int todo, byte[] destinationBuffer, int destOffset)
		{
			int bytesCopied = 0;

			if (bytesLeft == 0)
			{
				while (count < origSize)
				{
					if (decodeMode == 0)
					{
						uint c = decodeSet.DecodeC();
						if (c <= byte.MaxValue)
						{
							dText[loc++] = (byte)c;
							count++;

							if (loc == dicSiz)
								goto BufferFilled;
						}
						else
						{
							decodeJ = c - offset;
							decodeI = (loc - decodeSet.DecodeP() - 1) & dicSiz1;
							decodeK = 0;

							count += decodeJ;
							decodeMode = 1;
						}
					}

					if (decodeMode == 1)
					{
						while (decodeK < decodeJ)
						{
							uint c = dText[(decodeI + decodeK) & dicSiz1];
							dText[loc++] = (byte)c;
							decodeK++;

							if (loc == dicSiz)
								goto BufferFilled;
						}

						decodeMode = 0;
					}
				}

BufferFilled:
				crc.CalcCrc(dText, 0, loc);

				bytesLeft = (int)loc;
				bufIndex = 0;

				loc = 0;
			}

			if (bytesLeft != 0)
			{
				int toCopy = Math.Min(bytesLeft, todo);
				Array.Copy(dText, bufIndex, destinationBuffer, destOffset, toCopy);

				bufIndex += toCopy;
				bytesLeft -= toCopy;
				bytesCopied += toCopy;
			}

			return bytesCopied;
		}
	}
}
