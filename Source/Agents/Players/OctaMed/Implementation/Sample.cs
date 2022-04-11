/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation
{
	/// <summary>
	/// 
	/// </summary>
	internal class Sample
	{
		private static readonly int[] multiOctaveCount = { 1, 5, 3, 2, 4, 6, 7 };

		private static readonly int[][] multiOctaveBufferIndex =
		{
			new[] { 4, 4, 3, 2, 2, 1 },				// 5
			new[] { 2, 2, 2, 2, 2, 1 },				// 3
			new[] { 1, 1, 0, 0, 0, 0 },				// 2
			new[] { 3, 3, 2, 2, 2, 1 },				// 4
			new[] { 5, 5, 5, 4, 3, 2 },				// 6
			new[] { 6, 6, 6, 5, 4, 3 }				// 7
		};

		private static readonly int[][] multiOctaveStart =
		{
			new[] {  12,  12,   0, -12, -12, -24 },	// 5
			new[] {   0,   0,   0,   0,   0, -12 },	// 3
			new[] {   0,   0, -12, -12, -12, -12 },	// 2
			new[] {   0,   0, -12, -12, -12, -24 },	// 4
			new[] {   0,   0,   0, -12, -24, -36 },	// 6
			new[] {   0,   0,   0, -12, -24, -36 }	// 7
		};

		private readonly OctaMedWorker worker;

		private readonly sbyte[][][] data = new sbyte[2][][];
		private uint dataLen;							// Length of one sample
		private uint length;							// Sample length in samples
		private bool isStereo;
		private bool is16Bit;
		private ushort channels;						// 1 = Mono, 2 = Stereo
		private int sampleType;
		private int numberOfOctaves;					// Holds the number of octaves the sample contains

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="worker">Pointer to the core object</param>
		/********************************************************************/
		public Sample(OctaMedWorker worker)
		{
			this.worker = worker;

			dataLen = sizeof(sbyte);
			channels = 1;
		}



		/********************************************************************/
		/// <summary>
		/// Set the sample properties
		/// </summary>
		/********************************************************************/
		public void SetProp(uint len, bool stereo, bool sixtBit, short type)
		{
			SetStereo(stereo);
			Set16Bit(sixtBit);
			SetMultiOctave(type);
			SetLength(len);
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
		/// Return the buffer to the sample data
		/// </summary>
		/********************************************************************/
		public sbyte[] GetSampleBuffer(ushort ch, int octave)
		{
			if (octave >= numberOfOctaves)
				return null;

			if (data[ch] == null)
				return null;

			return data[ch][octave];
		}



		/********************************************************************/
		/// <summary>
		/// Return the buffer to play
		/// </summary>
		/********************************************************************/
		public sbyte[] GetPlayBuffer(ushort ch, NoteNum note, ref uint repeat, ref uint repLen)
		{
			if (sampleType == 0)
				return data[ch][0];

			int octave = note / 12;
			if (octave > 5)
				octave = 5;

			int bufNum = multiOctaveBufferIndex[sampleType - 1][octave];
			repeat <<= bufNum;
			repLen <<= bufNum;

			return data[ch][bufNum];
		}



		/********************************************************************/
		/// <summary>
		/// Return a value to add to the given note
		/// </summary>
		/********************************************************************/
		public int GetNoteDifference(NoteNum note)
		{
			if (!IsMultiOctave())
				return 0;

			int octave = note / 12;
			if (octave > 5)
				octave = 5;

			return multiOctaveStart[sampleType - 1][octave];
		}



		/********************************************************************/
		/// <summary>
		/// Tells if the sample is a multi octave sample
		/// </summary>
		/********************************************************************/
		public bool IsMultiOctave()
		{
			return numberOfOctaves > 1;
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
		/// Set the stereo flag
		/// </summary>
		/********************************************************************/
		private void SetStereo(bool stereo)
		{
			if (stereo == isStereo)
				return;

			if (stereo)
			{
				// No conversion implemented
				isStereo = true;
				channels = 2;
			}
			else
			{
				// We don't mix the stereo sample together to a mono sample
				isStereo = false;
				channels = 1;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set the 16-bit flag
		/// </summary>
		/********************************************************************/
		private void Set16Bit(bool sixtBit)
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
		/// Set the multi octave flag
		/// </summary>
		/********************************************************************/
		private void SetMultiOctave(short type)
		{
			sampleType = type > 6 ? 0 : (type & 0x0f);
			numberOfOctaves = multiOctaveCount[sampleType];

			for (ushort chCnt = 0; chCnt < channels; chCnt++)
				data[chCnt] = new sbyte[numberOfOctaves][];
		}



		/********************************************************************/
		/// <summary>
		/// Set the sample length
		/// </summary>
		/********************************************************************/
		private void SetLength(uint newLen)
		{
			for (ushort chCnt = 0; chCnt < channels; chCnt++)
			{
				if (newLen != length)
					InitSampleBuffers(chCnt, dataLen * newLen);
			}

			length = newLen;

			ValidateLoop();
		}



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
		/// Set sample buffers
		/// </summary>
		/********************************************************************/
		private void InitSampleBuffers(ushort ch, uint len)
		{
			if (len == 0)
				data[ch] = null;
			else
			{
				if (numberOfOctaves == 1)
					data[ch][0] = new sbyte[len];
				else
				{
					uint baseLen = len / (uint)((1 << numberOfOctaves) - 1);

					for (int i = 0; i < numberOfOctaves; i++)
					{
						data[ch][i] = new sbyte[baseLen];
						baseLen <<= 1;
					}
				}
			}
		}
		#endregion
	}
}
