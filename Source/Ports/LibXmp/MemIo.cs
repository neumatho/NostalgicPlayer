/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// 
	/// </summary>
	internal partial class MemIo
	{
		private class MFile
		{
			public ref CPointer<uint8> Start => ref _Start;
			private CPointer<uint8> _Start;
			public ptrdiff_t Pos { get; set; }
			public ptrdiff_t Size { get; set; }
			public ref CPointer<uint8> Ptr_Free => ref _Ptr_Free;
			private CPointer<uint8> _Ptr_Free;
		}

		private MFile m;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private MemIo(MFile mFile)
		{
			m = mFile;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int MGetC()
		{
			if (Can_Read() >= 1)
				return m.Start[m.Pos++];

			return Constants.EOF;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public size_t MRead(CPointer<uint8> buf, size_t size, size_t num)
		{
			size_t should_Read = size * num;
			ptrdiff_t can_Read = Can_Read();

			if ((size == 0) || (num == 0) || (can_Read <= 0))
				return 0;

			if ((ptrdiff_t)should_Read > can_Read)
			{
				CMemory.MemCpy(buf, m.Start + m.Pos, (int)can_Read);
				m.Pos += can_Read;

				return (size_t)can_Read / size;
			}
			else
			{
				CMemory.MemCpy(buf, m.Start + m.Pos, (int)should_Read);
				m.Pos += (ptrdiff_t)should_Read;

				return num;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int MSeek(c_long offset, SeekOrigin whence)
		{
			ptrdiff_t ofs = offset;

			switch (whence)
			{
				case SeekOrigin.Begin:
					break;

				case SeekOrigin.Current:
				{
					ofs += m.Pos;
					break;
				}

				case SeekOrigin.End:
				{
					ofs += m.Size;
					break;
				}

				default:
					return -1;
			}

			if (ofs < 0)
				return -1;

			if (ofs > m.Size)
				ofs = m.Size;

			m.Pos = ofs;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_long MTell()
		{
			return (c_long)m.Pos;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool MEof()
		{
			return Can_Read() <= 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static MemIo MCOpen(CPointer<uint8> ptr, c_long size)
		{
			MFile m = new MFile();
			if (m == null)
				return null;

			m.Start = ptr;
			m.Pos = 0;
			m.Size = size;
			m.Ptr_Free = null;

			return new MemIo(m);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static MemIo MOpen(MemIo from)
		{
			MFile m = new MFile();
			if (m == null)
				return null;

			m.Start = from.m.Start;
			m.Pos = from.m.Pos;
			m.Size = from.m.Size;
			m.Ptr_Free = null;

			return new MemIo(m);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int MClose()
		{
			if (m.Ptr_Free != null)
			{
				m.Ptr_Free = null;
				m.Start = null;
			}

			m = null;

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ptrdiff_t Can_Read()
		{
			return m.Pos >= 0 ? m.Size - m.Pos : 0;
		}
		#endregion
	}
}
