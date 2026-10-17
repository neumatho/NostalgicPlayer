/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io_Read;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io;
using FileReader = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.FileReader;
using Utility = Polycode.NostalgicPlayer.Kit.C.Std.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// An extended FileReader to read bit-oriented rather than byte-oriented streams
	/// 
	/// Notes: The current implementation can only read bit widths up to 32 bits, and
	/// it always reads bits starting from the least significant bit, as this is all
	/// that is required by the class users at the moment
	/// </summary>
	internal class BitReader
	{
		#region EofException
		public class EofException : ArgumentOutOfRangeException
		{
			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public EofException() : base("Truncated bit buffer")
			{
			}
		}
		#endregion

		private readonly FileReader fileReader;

		private size_t m_BufPos = 0;
		private size_t m_BufSize = 0;

		/// <summary>
		/// Current bit buffer
		/// </summary>
		private uint32 bitBuf = 0;

		/// <summary>
		/// Currently available number of bits
		/// </summary>
		private c_int m_BitNum = 0;

		private readonly c_byte[] buffer = new c_byte[Base.BufferSize_Tiny];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public BitReader(FileCursor other)
		{
			fileReader = new FileReader(Utility.move(other));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint32 ReadBits(c_int numBits)
		{
			while (m_BitNum < numBits)
			{
				// Fetch more bits
				if (m_BufPos >= m_BufSize)
				{
					m_BufSize = fileReader.ReadRaw(new MptSpan<c_byte>(buffer)).Size();
					m_BufPos = 0;

					if (m_BufSize == 0)
						throw new EofException();
				}

				bitBuf |= (uint32)buffer[m_BufPos++] << m_BitNum;
				m_BitNum += 8;
			}

			uint32 v = (uint32)(bitBuf & ((1 << numBits) - 1));
			bitBuf >>= numBits;
			m_BitNum -= numBits;

			return v;
		}
	}
}
