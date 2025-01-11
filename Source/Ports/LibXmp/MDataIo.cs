/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// 
	/// </summary>
	internal partial class MemIo
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint8 MRead8(out c_int err)
		{
			uint8[] x = [ 0xff ];

			size_t r = MRead(x, 1, 1);
			err = (r == 1) ? 0 : Constants.EOF;

			return x[0];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int8 MRead8S(out c_int err)
		{
			c_int r = MGetC();
			err = (r < 0) ? Constants.EOF : 0;

			return (int8)r;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint16 MRead16L(out c_int err)
		{
			ptrdiff_t can_Read = Can_Read_();

			if (can_Read >= 2)
			{
				uint16 n = DataIo.ReadMem16L(m.Start + m.Pos);
				m.Pos += 2;
				err = 0;

				return n;
			}
			else
			{
				m.Pos += can_Read;
				err = Constants.EOF;

				return 0xffff;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint16 MRead16B(out c_int err)
		{
			ptrdiff_t can_Read = Can_Read_();

			if (can_Read >= 2)
			{
				uint16 n = DataIo.ReadMem16B(m.Start + m.Pos);
				m.Pos += 2;
				err = 0;

				return n;
			}
			else
			{
				m.Pos += can_Read;
				err = Constants.EOF;

				return 0xffff;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint32 MRead24L(out c_int err)
		{
			ptrdiff_t can_Read = Can_Read_();

			if (can_Read >= 3)
			{
				uint32 n = DataIo.ReadMem24L(m.Start + m.Pos);
				m.Pos += 3;
				err = 0;

				return n;
			}
			else
			{
				m.Pos += can_Read;
				err = Constants.EOF;

				return 0xffffffff;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint32 MRead24B(out c_int err)
		{
			ptrdiff_t can_Read = Can_Read_();

			if (can_Read >= 3)
			{
				uint32 n = DataIo.ReadMem24B(m.Start + m.Pos);
				m.Pos += 3;
				err = 0;

				return n;
			}
			else
			{
				m.Pos += can_Read;
				err = Constants.EOF;

				return 0xffffffff;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint32 MRead32L(out c_int err)
		{
			ptrdiff_t can_Read = Can_Read_();

			if (can_Read >= 4)
			{
				uint32 n = DataIo.ReadMem32L(m.Start + m.Pos);
				m.Pos += 4;
				err = 0;

				return n;
			}
			else
			{
				m.Pos += can_Read;
				err = Constants.EOF;

				return 0xffffffff;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint32 MRead32B(out c_int err)
		{
			ptrdiff_t can_Read = Can_Read_();

			if (can_Read >= 4)
			{
				uint32 n = DataIo.ReadMem32B(m.Start + m.Pos);
				m.Pos += 4;
				err = 0;

				return n;
			}
			else
			{
				m.Pos += can_Read;
				err = Constants.EOF;

				return 0xffffffff;
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ptrdiff_t Can_Read_()
		{
			if (m.Size >= 0)
				return m.Pos >= 0 ? m.Size - m.Pos : 0;

			return c_int.MaxValue;
		}
		#endregion
	}
}
