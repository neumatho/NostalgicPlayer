/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Endian
{
	#region Ieee754Binary32LE
	/// <summary>
	/// 32-bit IEEE 754 floating point value stored in little-endian format
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
	internal struct Ieee754Binary32LE : IEndianNumber<c_float>, IEquatable<Ieee754Binary32LE>
	{
		private byte _b0, _b1, _b2, _b3;

		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public c_float Value
		{
			get
			{
				Span<byte> data = stackalloc byte[4];
				MemoryMarshal.Write(data, in this);

				return BinaryPrimitives.ReadSingleLittleEndian(data);
			}

			set
			{
				Span<byte> data = stackalloc byte[4];
				BinaryPrimitives.WriteSingleLittleEndian(data, value);
				this = MemoryMarshal.Read<Ieee754Binary32LE>(data);
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static implicit operator c_float(Ieee754Binary32LE x)
		{
			return x.Value;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static explicit operator Ieee754Binary32LE(c_float f)
		{
			return new Ieee754Binary32LE { Value = f };
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static bool operator == (Ieee754Binary32LE me, Ieee754Binary32LE other)
		{
			return (me._b0 == other._b0) && (me._b1 == other._b1) && (me._b2 == other._b2) && (me._b3 == other._b3);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator != (Ieee754Binary32LE me, Ieee754Binary32LE other)
		{
			return !(me == other);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public override bool Equals(object obj)
		{
			return (obj is Ieee754Binary32LE other) && (this == other);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public bool Equals(Ieee754Binary32LE other)
		{
			return this == other;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public override int GetHashCode()
		{
			return HashCode.Combine(_b0, _b1, _b2, _b3);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public override string ToString()
		{
			return Value.ToString();
		}
	}
	#endregion
}
