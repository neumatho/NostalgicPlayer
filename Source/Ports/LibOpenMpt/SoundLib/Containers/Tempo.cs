/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// Fixed-point tempo value with four fractional digits.
	/// C# cannot inherit from a struct, so instead of deriving from
	/// FPInt (as the original OpenMPT does), this type wraps an FPInt
	/// and forwards to it
	/// </summary>
	internal struct Tempo : IEquatable<Tempo>
	{
		/// <summary>
		/// The fractional factor, i.e. the scale of the stored value
		/// </summary>
		public const uint32 FractFact = 10000;

		private FPInt<uint32> value;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Tempo()
		{
			value = new FPInt<uint32>(FractFact);
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Tempo(uint32 intTempo, uint32 fractTempo) : this(new FPInt<uint32>(FractFact, intTempo, fractTempo))
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Tempo(c_double f) : this(new FPInt<uint32>(FractFact, f))
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor wrapping an existing FPInt
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Tempo(FPInt<uint32> other)
		{
			value = other;
		}



		/********************************************************************/
		/// <summary>
		/// Set the integer and fractional part
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(uint32 intTempo, uint32 fractTempo = 0)
		{
			value.Set(intTempo, fractTempo);
		}



		/********************************************************************/
		/// <summary>
		/// Set the raw internal representation directly
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetRaw(uint32 raw)
		{
			value.SetRaw(raw);
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve the integer part of the stored value
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint32 GetInt()
		{
			return value.GetInt();
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve the raw internal representation of the stored value
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly uint32_t GetRaw()
		{
			return value.GetRaw();
		}



		/********************************************************************/
		/// <summary>
		/// Formats the stored value as a floating-point value
		/// </summary>
		/********************************************************************/
		public readonly c_double ToDouble()
		{
			return value.ToDouble();
		}



		/********************************************************************/
		/// <summary>
		/// Add two values together
		/// </summary>
		/********************************************************************/
		public static Tempo operator +(Tempo a, Tempo b)
		{
			return new Tempo(a.value + b.value);
		}



		/********************************************************************/
		/// <summary>
		/// Subtract one value from another
		/// </summary>
		/********************************************************************/
		public static Tempo operator -(Tempo a, Tempo b)
		{
			return new Tempo(a.value - b.value);
		}



		/********************************************************************/
		/// <summary>
		/// Compare two values for equality
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Tempo a, Tempo b)
		{
			return a.value == b.value;
		}



		/********************************************************************/
		/// <summary>
		/// Compare two values for inequality
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Tempo a, Tempo b)
		{
			return !(a == b);
		}



		/********************************************************************/
		/// <summary>
		/// Check if one value is less than another
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <(Tempo a, Tempo b)
		{
			return a.value < b.value;
		}



		/********************************************************************/
		/// <summary>
		/// Check if one value is greater than another
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >(Tempo a, Tempo b)
		{
			return a.value > b.value;
		}



		/********************************************************************/
		/// <summary>
		/// Check if one value is less than or equal to another
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <=(Tempo a, Tempo b)
		{
			return a.value <= b.value;
		}



		/********************************************************************/
		/// <summary>
		/// Check if one value is greater than or equal to another
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >=(Tempo a, Tempo b)
		{
			return a.value >= b.value;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static implicit operator Tempo(uint32 tempo)
		{
			return new Tempo(tempo);
		}



		/********************************************************************/
		/// <summary>
		/// Compare this value with another object
		/// </summary>
		/********************************************************************/
		public override bool Equals(object obj)
		{
			return (obj is Tempo other) && (this == other);
		}



		/********************************************************************/
		/// <summary>
		/// Compare this value with another one
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Tempo other)
		{
			return this == other;
		}



		/********************************************************************/
		/// <summary>
		/// Return a hash code for this value
		/// </summary>
		/********************************************************************/
		public override readonly int GetHashCode()
		{
			return value.GetHashCode();
		}
	}
}
