/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Code for IT sample decompression
	/// </summary>
	internal class ItDecompression
	{
		/// <summary>
		/// Block size (in bytes) in which samples are being processed
		/// </summary>
		private const size_t BlockSize = 0x8000;

		private BitReader bitFile;

		/// <summary>
		/// Sample that is being processed
		/// </summary>
		private readonly ModSample mptSample;

		/// <summary>
		/// Number of samples so far written on this channel
		/// </summary>
		private SmpLength writtenSamples = 0;

		/// <summary>
		/// Absolut write position in sample (for stereo samples)
		/// </summary>
		private SmpLength writePos = 0;

		/// <summary>
		/// Length of currently processed block
		/// </summary>
		private SmpLength curLength = 0;

		/// <summary>
		/// Integrator memory
		/// </summary>
		private c_uint mem1 = 0;
		private c_uint mem2 = 0;

		/// <summary>
		/// Use IT 2.15 compression (double deltas)
		/// </summary>
		private readonly bool is215;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ItDecompression(FileReader file, ModSample sample, bool it215)
		{
			mptSample = sample;
			is215 = it215;

			for (uint8 chn = 0; chn < mptSample.GetNumChannels(); chn++)
			{
				writtenSamples = writePos = 0;

				while ((writtenSamples < sample.nLength) && file.CanRead(sizeof(uint16)))
				{
					uint16 compressedSize = file.ReadUInt16LE();

					if (compressedSize == 0)
						continue;	// Malformed sample?

					bitFile = new BitReader(file.ReadChunk(compressedSize));

					// Initialise bit reader
					mem1 = mem2 = 0;

					try
					{
						if (mptSample.GetElementarySampleSize() > 1)
							Uncompress<It16BitParams, int16>(mptSample.Sample16() + chn);
						else
							Uncompress<It8BitParams, int8>(mptSample.Sample8() + chn);
					}
					catch (BitReader.EofException)
					{
						// Data is not sufficient to decode the block
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Uncompress<TProperties, T>(CPointer<T> target) where TProperties : IItBitParams<T> where T : IBinaryInteger<T>
		{
			curLength = Math.Min(mptSample.nLength - writtenSamples, (SmpLength)(BlockSize / (size_t)Marshal.SizeOf<T>()));

			c_int width = TProperties.DefWidth;

			while (curLength > 0)
			{
				if (width > TProperties.DefWidth)
				{
					// Error!
					return;
				}

				c_int v = (c_int)bitFile.ReadBits(width);
				c_int topBit = 1 << (width - 1);

				if (width <= 6)
				{
					// Mode A: 1 to 6 bits
					if (v == topBit)
						ChangeWidth(ref width, (c_int)bitFile.ReadBits(TProperties.FetchA));
					else
						Write(v, topBit, target);
				}
				else if (width < TProperties.DefWidth)
				{
					// Mode B: 7 to 8 / 16 bits
					if ((v >= (topBit + TProperties.LowerB)) && (v <= (topBit + TProperties.UpperB)))
						ChangeWidth(ref width, v - (topBit + TProperties.LowerB));
					else
						Write(v, topBit, target);
				}
				else
				{
					// Mode C: 9 / 17 bits
					if ((v & topBit) != 0)
						width = (v & ~topBit) + 1;
					else
						Write((v & ~topBit), 0, target);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ChangeWidth(ref c_int curWidth, c_int width)
		{
			width++;

			if (width >= curWidth)
				width++;

			curWidth = width;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Write<T>(c_int v, c_int topBit, CPointer<T> target) where T : IBinaryInteger<T>
		{
			if ((v & topBit) != 0)
				v -= (topBit << 1);

			mem1 = (c_uint)(mem1 + v);
			mem2 += mem1;

			target[writePos] = T.CreateTruncating((c_int)(is215 ? mem2 : mem1));

			writtenSamples++;
			writePos += mptSample.GetNumChannels();
			curLength--;
		}
	}
}
