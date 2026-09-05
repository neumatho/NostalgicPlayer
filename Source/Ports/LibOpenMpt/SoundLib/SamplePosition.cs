/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using value_t = System.Int64;
using unsigned_value_t = System.UInt64;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Sample position and sample position increment value
	/// </summary>
	internal struct SamplePosition
	{
		public const uint32 FractMax = 0xffffffffU;

		private value_t v = 0;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SamplePosition()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SamplePosition(value_t pos)
		{
			v = pos;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SamplePosition(int32 intPart, uint32 fractPart)
		{
			v = ((value_t)intPart * (1L << 32)) | fractPart;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SamplePosition Ratio(uint32 dividend, uint32 divisor)
		{
			return new SamplePosition(((int64)dividend << 32) / divisor);
		}



		/********************************************************************/
		/// <summary>
		/// Set integer and fractional part
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(int32 intPart, uint32 fractPart = 0)
		{
			v = ((int64)intPart << 32) | fractPart;
		}



		/********************************************************************/
		/// <summary>
		/// Set integer, keep fractional part
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetInt(int32 intPart)
		{
			v = ((value_t)intPart << 32) | GetFract();
		}



		/********************************************************************/
		/// <summary>
		/// Get integer part (as sample length / position)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SmpLength GetUInt()
		{
			return (SmpLength)((unsigned_value_t)v >> 32);
		}



		/********************************************************************/
		/// <summary>
		/// Get integer part
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int32 GetInt()
		{
			return (int32)((unsigned_value_t)v >> 32);
		}



		/********************************************************************/
		/// <summary>
		/// Get fractional part
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint32 GetFract()
		{
			return (uint32)v;
		}



		/********************************************************************/
		/// <summary>
		/// Get the inverted fractional part
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SamplePosition GetInvertedFract()
		{
			return new SamplePosition(0x100000000L - GetFract());
		}



		/********************************************************************/
		/// <summary>
		/// Get the raw fixed-point value
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int64 GetRaw()
		{
			return v;
		}



		/********************************************************************/
		/// <summary>
		/// Negate the current value
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Negate()
		{
			v = -v;
		}



		/********************************************************************/
		/// <summary>
		/// Multiply and divide by given integer scalars
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MulDiv(uint32 mul, uint32 div)
		{
			v = (v * mul) / div;
		}



		/********************************************************************/
		/// <summary>
		/// Check if value is 1.0
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsUnity()
		{
			return v == 0x100000000L;
		}



		/********************************************************************/
		/// <summary>
		/// Check if value is 0
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsZero()
		{
			return v == 0;
		}



		/********************************************************************/
		/// <summary>
		/// Check if value › 0
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsPositive()
		{
			return v > 0;
		}



		/********************************************************************/
		/// <summary>
		/// Check if value ‹ 0
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsNegative()
		{
			return v < 0;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SamplePosition operator + (SamplePosition me, SamplePosition other)
		{
			return new SamplePosition(me.v + other.v);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SamplePosition operator - (SamplePosition me, SamplePosition other)
		{
			return new SamplePosition(me.v - other.v);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SamplePosition operator * (SamplePosition me, uint32 other)
		{
			return new SamplePosition(me.v * other);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static value_t operator / (SamplePosition me, SamplePosition other)
		{
			return me.v / other.v;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator == (SamplePosition me, SamplePosition other)
		{
			return me.v == other.v;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator != (SamplePosition me, SamplePosition other)
		{
			return !(me == other);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator > (SamplePosition me, SamplePosition other)
		{
			return me.v > other.v;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator < (SamplePosition me, SamplePosition other)
		{
			return me.v < other.v;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >= (SamplePosition me, SamplePosition other)
		{
			return me.v >= other.v;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <= (SamplePosition me, SamplePosition other)
		{
			return me.v <= other.v;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public override bool Equals(object obj)
		{
			return (obj is SamplePosition other) && (this == other);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(SamplePosition other)
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
			return v.GetHashCode();
		}
	}
}
