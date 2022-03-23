/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Runtime.InteropServices;

namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation
{
	/// <summary>
	/// 
	/// </summary>
	internal class Sample
	{
		private readonly OctaMedWorker worker;

		private readonly sbyte[][] data = new sbyte[2][];
		private uint dataLen;							// Length of one sample
		private uint length;							// Sample length in samples
		private bool isStereo;
		private bool is16Bit;
		private ushort channels;						// 1 = Mono, 2 = Stereo, 0 = None

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="worker">Pointer to the core object</param>
		/// <param name="length">The length in samples</param>
		/// <param name="stereo">Indicate if the sample is in stereo or mono</param>
		/// <param name="sixtBit">Indicate if the sample is in 16-bit or 8-bit</param>
		/********************************************************************/
		public Sample(OctaMedWorker worker, uint length = 0, bool stereo = false, bool sixtBit = false)
		{
			this.worker = worker;

			dataLen = sixtBit ? (uint)2 * sizeof(sbyte) : sizeof(sbyte);
			this.length = length;		// Sample length in samples
			isStereo = stereo;
			is16Bit = sixtBit;
			channels = 0;				// Null samples have a special channel value

			data[0] = null;
			data[1] = null;

			if (length != 0)
			{
				data[0] = new sbyte[ByteLength()];
				channels = 1;

				if (stereo)
				{
					data[1] = new sbyte[ByteLength()];
					channels = 2;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set the sample properties
		/// </summary>
		/********************************************************************/
		public void SetProp(uint length, bool stereo, bool sixtBit)
		{
			SetStereo(stereo);
			Set16Bit(sixtBit);
			SetLength(length);
		}



		/********************************************************************/
		/// <summary>
		/// Set the sample length
		/// </summary>
		/********************************************************************/
		public void SetLength(uint newLen)
		{
			int numChannels = isStereo ? 2 : 1;

			for (int chCnt = 0; chCnt < numChannels; chCnt++)
			{
				if (newLen != length)
				{
					if (newLen > 0)
					{
						sbyte[] newData = new sbyte[dataLen * newLen];
						data[chCnt] = newData;
					}
					else
						data[chCnt] = null;
				}
			}

			length = newLen;
			if (length == 0)
				channels = 0;		// Implies: no data
			else
				channels = (ushort)numChannels;

			ValidateLoop();
		}



		/********************************************************************/
		/// <summary>
		/// Set the stereo flag
		/// </summary>
		/********************************************************************/
		public void SetStereo(bool stereo)
		{
			if (stereo == isStereo)
				return;

			if (stereo)
			{
				// No conversion implemented
				isStereo = true;
			}
			else
			{
				// We don't mix the stereo sample together to a mono sample
				isStereo = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set the 16-bit flag
		/// </summary>
		/********************************************************************/
		public void Set16Bit(bool sixtBit)
		{
			if (is16Bit == sixtBit)
				return;

			if (length != 0)
			{
				// No conversion implemented
			}

			dataLen = sixtBit ? (uint)sizeof(short) : sizeof(sbyte);
			is16Bit = sixtBit;
		}



		/********************************************************************/
		/// <summary>
		/// Will set a single sample at a given channel
		/// </summary>
		/********************************************************************/
		public void SetData8(uint pos, ushort ch, sbyte smp)
		{
			data[ch][pos] = smp;
		}



		/********************************************************************/
		/// <summary>
		/// Will set a single sample at a given channel
		/// </summary>
		/********************************************************************/
		public void SetData16(uint pos, ushort ch, short smp)
		{
			Span<short> castBuffer = MemoryMarshal.Cast<sbyte, short>(data[ch]);
			castBuffer[(int)pos] = smp;
		}



		/********************************************************************/
		/// <summary>
		/// Return the length of the sample
		/// </summary>
		/********************************************************************/
		public uint GetLength()
		{
			return length;
		}



		/********************************************************************/
		/// <summary>
		/// Return the address to the sample data
		/// </summary>
		/********************************************************************/
		public sbyte[] GetSampleAddress(ushort ch)
		{
			return data[ch];
		}



		/********************************************************************/
		/// <summary>
		/// Tells if the sample is in stereo or mono
		/// </summary>
		/********************************************************************/
		public bool IsStereo()
		{
			return isStereo;
		}



		/********************************************************************/
		/// <summary>
		/// Tells if the sample is in 16 or 8 bit
		/// </summary>
		/********************************************************************/
		public bool Is16Bit()
		{
			return is16Bit;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Instr GetInstrument()
		{
			if (worker.sg != null)
				return worker.sg.Sample2Instrument(this);

			return null;
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual bool IsSynthSound()
		{
			return false;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ValidateLoop()
		{
			Instr instr = GetInstrument();
			if (instr != null)
				instr.ValidateLoop();
		}



		/********************************************************************/
		/// <summary>
		/// Return the length of the sample in bytes
		/// </summary>
		/********************************************************************/
		private uint ByteLength()
		{
			return length * dataLen;
		}
		#endregion
	}
}
