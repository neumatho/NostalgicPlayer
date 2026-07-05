/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C.Std.Exceptions;

namespace Polycode.NostalgicPlayer.Kit.C.Std
{
	/// <summary>
	/// Shared implementation used by the Variant‹...› classes. Not part of
	/// the public C++ interface
	/// </summary>
	internal static class Variant_Internal
	{
		// The value used to indicate that a variant does not hold a value
		// (C++ std::variant_npos)
		public const size_t Npos = ~0UL;

		/********************************************************************/
		/// <summary>
		/// Returns the index of the given type among the alternatives, or
		/// Npos if the type is not one of the alternatives
		/// </summary>
		/********************************************************************/
		public static size_t Index_Of(Type type, Type[] alternatives)
		{
			for (int i = 0; i < alternatives.Length; i++)
			{
				if (alternatives[i] == type)
					return (size_t)i;
			}

			return Npos;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the currently held value as type T, throwing
		/// bad_variant_access if T is not the alternative currently held
		/// </summary>
		/********************************************************************/
		public static T Get<T>(object value, size_t currentIndex, Type[] alternatives)
		{
			size_t wanted = Index_Of(typeof(T), alternatives);

			if ((wanted == Npos) || (wanted != currentIndex))
				throw new bad_variant_access();

			return value is null ? default : (T)value;
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if the alternative of type T is currently held
		/// </summary>
		/********************************************************************/
		public static bool Holds<T>(size_t currentIndex, Type[] alternatives)
		{
			size_t wanted = Index_Of(typeof(T), alternatives);

			return (wanted != Npos) && (wanted == currentIndex);
		}



		/********************************************************************/
		/// <summary>
		/// Retrieves the held value if the alternative of type T is
		/// currently held
		/// </summary>
		/********************************************************************/
		public static bool Get_If<T>(object value, size_t currentIndex, Type[] alternatives, out T result)
		{
			if (Holds<T>(currentIndex, alternatives))
			{
				result = value is null ? default : (T)value;
				return true;
			}

			result = default;
			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the index that the alternative of the given type should
		/// be stored at, throwing bad_variant_access if the type is not one
		/// of the alternatives
		/// </summary>
		/********************************************************************/
		public static size_t Emplace_Index(Type type, Type[] alternatives)
		{
			size_t wanted = Index_Of(type, alternatives);

			if (wanted == Npos)
				throw new bad_variant_access("the type is not an alternative of this variant");

			return wanted;
		}
	}



	/// <summary>
	/// Holds the free (non-member) helpers that C++ places in the std
	/// namespace and that do not need to be tied to a specific variant arity
	/// </summary>
#pragma warning disable CS8981
	public static class variant
	{
		/// <summary>
		/// The largest possible value of size_t, used as the return value of
		/// index() when a variant is valueless (C++ std::variant_npos)
		/// </summary>
		public const size_t variant_npos = ~0UL;
	}



	/// <summary>
	/// C# port of the C++ standard library std::variant
	/// (see the C++ standard, [variant]).
	///
	/// A type-safe union that at any given time holds a value of one of its
	/// alternative types (T0 or T1).
	///
	/// Notable differences from C++:
	/// - C++ uses a variadic template. Since C# does not support that, a
	///   separate class is provided for each supported number of alternatives.
	/// - The non-member functions std::get, std::holds_alternative,
	///   std::get_if and std::visit are provided as instance methods here,
	///   because C# has neither free functions nor partial type argument
	///   inference.
	/// - get‹I› and other index based operations from C++ are not available,
	///   as C# does not support non-type (value) template arguments. Use the
	///   type based get‹T› instead.
	/// - The alternative types must be distinct C# types. C++ allows
	///   duplicate alternatives disambiguated by index, but this port keys
	///   everything on the type.
	/// Unlike C++ std::variant (a value type), this is a reference type (class),
	/// so it can be used as a generic type constraint. Assignment therefore
	/// shares the same instance; make an explicit deep copy if an independent
	/// variant is needed
	/// </summary>
	public class variant<T0, T1> : IEquatable<variant<T0, T1>>
	{
		private static readonly Type[] alternatives = [ typeof(T0), typeof(T1) ];

		private object value;
		private size_t typeIndex;

		/********************************************************************/
		/// <summary>
		/// Constructs an empty variant. This exists so the type can be used
		/// as a generic argument with a new() constraint; give it a value
		/// through a converting constructor or emplace before using it
		/// </summary>
		/********************************************************************/
		public variant()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the variant holding the given T0 value
		/// </summary>
		/********************************************************************/
		public variant(T0 v)
		{
			value = v;
			typeIndex = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the variant holding the given T1 value
		/// </summary>
		/********************************************************************/
		public variant(T1 v)
		{
			value = v;
			typeIndex = 1;
		}



		/********************************************************************/
		/// <summary>
		/// Converting construction/assignment from a T0 value
		/// (C++ converting constructor)
		/// </summary>
		/********************************************************************/
		public static implicit operator variant<T0, T1>(T0 v)
		{
			return new variant<T0, T1>(v);
		}



		/********************************************************************/
		/// <summary>
		/// Converting construction/assignment from a T1 value
		/// (C++ converting constructor)
		/// </summary>
		/********************************************************************/
		public static implicit operator variant<T0, T1>(T1 v)
		{
			return new variant<T0, T1>(v);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the zero-based index of the alternative currently held
		/// (C++ index())
		/// </summary>
		/********************************************************************/
		public size_t index()
		{
			return typeIndex;
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if the variant does not hold a value. This port
		/// never produces a valueless variant, so this always returns false
		/// (C++ valueless_by_exception())
		/// </summary>
		/********************************************************************/
		public bool valueless_by_exception()
		{
			return typeIndex == variant.variant_npos;
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if the alternative of type T is currently held
		/// (C++ std::holds_alternative)
		/// </summary>
		/********************************************************************/
		public bool holds_alternative<T>()
		{
			return Variant_Internal.Holds<T>(typeIndex, alternatives);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the currently held value as type T, throwing
		/// bad_variant_access if T is not the alternative currently held
		/// (C++ std::get)
		/// </summary>
		/********************************************************************/
		public T get<T>()
		{
			return Variant_Internal.Get<T>(value, typeIndex, alternatives);
		}



		/********************************************************************/
		/// <summary>
		/// Retrieves the held value if the alternative of type T is
		/// currently held, returning false otherwise (C++ std::get_if)
		/// </summary>
		/********************************************************************/
		public bool get_if<T>(out T result)
		{
			return Variant_Internal.Get_If(value, typeIndex, alternatives, out result);
		}



		/********************************************************************/
		/// <summary>
		/// Replaces the held value with the given value of type T
		/// (C++ emplace)
		/// </summary>
		/********************************************************************/
		public T emplace<T>(T v)
		{
			typeIndex = Variant_Internal.Emplace_Index(typeof(T), alternatives);
			value = v;

			return v;
		}



		/********************************************************************/
		/// <summary>
		/// Applies the matching function to the held value and returns its
		/// result (C++ std::visit)
		/// </summary>
		/********************************************************************/
		public R visit<R>(Func<T0, R> f0, Func<T1, R> f1)
		{
			switch (typeIndex)
			{
				case 0:
					return f0(value is null ? default : (T0)value);

				case 1:
					return f1(value is null ? default : (T1)value);

				default:
					throw new bad_variant_access();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Applies the matching action to the held value (C++ std::visit)
		/// </summary>
		/********************************************************************/
		public void visit(Action<T0> f0, Action<T1> f1)
		{
			switch (typeIndex)
			{
				case 0:
				{
					f0(value is null ? default : (T0)value);
					break;
				}

				case 1:
				{
					f1(value is null ? default : (T1)value);
					break;
				}

				default:
					throw new bad_variant_access();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Swaps the contents of this variant with another (C++ swap())
		/// </summary>
		/********************************************************************/
		public void swap(ref variant<T0, T1> other)
		{
			(value, other.value) = (other.value, value);
			(typeIndex, other.typeIndex) = (other.typeIndex, typeIndex);
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if both variants hold the same alternative with
		/// equal values (C++ operator==)
		/// </summary>
		/********************************************************************/
		public bool Equals(variant<T0, T1> other)
		{
			if (other is null)
				return false;

			return (typeIndex == other.typeIndex) && object.Equals(value, other.value);
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if the given object is a variant that is equal to
		/// this one
		/// </summary>
		/********************************************************************/
		public override bool Equals(object obj)
		{
			return obj is variant<T0, T1> other && Equals(other);
		}



		/********************************************************************/
		/// <summary>
		/// Returns a hash code based on the held alternative and value
		/// </summary>
		/********************************************************************/
		public override int GetHashCode()
		{
			return HashCode.Combine(typeIndex, value);
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if both variants hold the same alternative with
		/// equal values (C++ operator==)
		/// </summary>
		/********************************************************************/
		public static bool operator ==(variant<T0, T1> left, variant<T0, T1> right)
		{
			if (left is null)
				return right is null;

			return left.Equals(right);
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if the variants differ (C++ operator!=)
		/// </summary>
		/********************************************************************/
		public static bool operator !=(variant<T0, T1> left, variant<T0, T1> right)
		{
			return !left.Equals(right);
		}



		/********************************************************************/
		/// <summary>
		/// Returns a string representation of the held value
		/// </summary>
		/********************************************************************/
		public override string ToString()
		{
			return value?.ToString() ?? string.Empty;
		}
	}



	/// <summary>
	/// C# port of the C++ standard library std::variant with three
	/// alternatives. See <see cref="variant{T0,T1}"/> for the general remarks
	/// </summary>
	public class variant<T0, T1, T2> : IEquatable<variant<T0, T1, T2>>
	{
		private static readonly Type[] alternatives = [ typeof(T0), typeof(T1), typeof(T2) ];

		private object value;
		private size_t typeIndex;

		/********************************************************************/
		/// <summary>
		/// Constructs an empty variant. This exists so the type can be used
		/// as a generic argument with a new() constraint; give it a value
		/// through a converting constructor or emplace before using it
		/// </summary>
		/********************************************************************/
		public variant()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the variant holding the given T0 value
		/// </summary>
		/********************************************************************/
		public variant(T0 v)
		{
			value = v;
			typeIndex = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the variant holding the given T1 value
		/// </summary>
		/********************************************************************/
		public variant(T1 v)
		{
			value = v;
			typeIndex = 1;
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the variant holding the given T2 value
		/// </summary>
		/********************************************************************/
		public variant(T2 v)
		{
			value = v;
			typeIndex = 2;
		}



		/********************************************************************/
		/// <summary>
		/// Converting construction/assignment from a T0 value
		/// </summary>
		/********************************************************************/
		public static implicit operator variant<T0, T1, T2>(T0 v)
		{
			return new variant<T0, T1, T2>(v);
		}



		/********************************************************************/
		/// <summary>
		/// Converting construction/assignment from a T1 value
		/// </summary>
		/********************************************************************/
		public static implicit operator variant<T0, T1, T2>(T1 v)
		{
			return new variant<T0, T1, T2>(v);
		}



		/********************************************************************/
		/// <summary>
		/// Converting construction/assignment from a T2 value
		/// </summary>
		/********************************************************************/
		public static implicit operator variant<T0, T1, T2>(T2 v)
		{
			return new variant<T0, T1, T2>(v);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the zero-based index of the alternative currently held
		/// </summary>
		/********************************************************************/
		public size_t index()
		{
			return typeIndex;
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if the variant does not hold a value
		/// </summary>
		/********************************************************************/
		public bool valueless_by_exception()
		{
			return typeIndex == variant.variant_npos;
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if the alternative of type T is currently held
		/// </summary>
		/********************************************************************/
		public bool holds_alternative<T>()
		{
			return Variant_Internal.Holds<T>(typeIndex, alternatives);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the currently held value as type T
		/// </summary>
		/********************************************************************/
		public T get<T>()
		{
			return Variant_Internal.Get<T>(value, typeIndex, alternatives);
		}



		/********************************************************************/
		/// <summary>
		/// Retrieves the held value if the alternative of type T is held
		/// </summary>
		/********************************************************************/
		public bool get_if<T>(out T result)
		{
			return Variant_Internal.Get_If(value, typeIndex, alternatives, out result);
		}



		/********************************************************************/
		/// <summary>
		/// Replaces the held value with the given value of type T
		/// </summary>
		/********************************************************************/
		public T emplace<T>(T v)
		{
			typeIndex = Variant_Internal.Emplace_Index(typeof(T), alternatives);
			value = v;

			return v;
		}



		/********************************************************************/
		/// <summary>
		/// Applies the matching function to the held value and returns its
		/// result
		/// </summary>
		/********************************************************************/
		public R visit<R>(Func<T0, R> f0, Func<T1, R> f1, Func<T2, R> f2)
		{
			switch (typeIndex)
			{
				case 0:
					return f0(value is null ? default : (T0)value);

				case 1:
					return f1(value is null ? default : (T1)value);

				case 2:
					return f2(value is null ? default : (T2)value);

				default:
					throw new bad_variant_access();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Applies the matching action to the held value
		/// </summary>
		/********************************************************************/
		public void visit(Action<T0> f0, Action<T1> f1, Action<T2> f2)
		{
			switch (typeIndex)
			{
				case 0:
				{
					f0(value is null ? default : (T0)value);
					break;
				}

				case 1:
				{
					f1(value is null ? default : (T1)value);
					break;
				}

				case 2:
				{
					f2(value is null ? default : (T2)value);
					break;
				}

				default:
					throw new bad_variant_access();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Swaps the contents of this variant with another
		/// </summary>
		/********************************************************************/
		public void swap(ref variant<T0, T1, T2> other)
		{
			(value, other.value) = (other.value, value);
			(typeIndex, other.typeIndex) = (other.typeIndex, typeIndex);
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if both variants hold the same alternative with
		/// equal values
		/// </summary>
		/********************************************************************/
		public bool Equals(variant<T0, T1, T2> other)
		{
			if (other is null)
				return false;

			return (typeIndex == other.typeIndex) && object.Equals(value, other.value);
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if the given object is a variant that is equal to
		/// this one
		/// </summary>
		/********************************************************************/
		public override bool Equals(object obj)
		{
			return obj is variant<T0, T1, T2> other && Equals(other);
		}



		/********************************************************************/
		/// <summary>
		/// Returns a hash code based on the held alternative and value
		/// </summary>
		/********************************************************************/
		public override int GetHashCode()
		{
			return HashCode.Combine(typeIndex, value);
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if both variants are equal
		/// </summary>
		/********************************************************************/
		public static bool operator ==(variant<T0, T1, T2> left, variant<T0, T1, T2> right)
		{
			if (left is null)
				return right is null;

			return left.Equals(right);
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if the variants differ
		/// </summary>
		/********************************************************************/
		public static bool operator !=(variant<T0, T1, T2> left, variant<T0, T1, T2> right)
		{
			return !left.Equals(right);
		}



		/********************************************************************/
		/// <summary>
		/// Returns a string representation of the held value
		/// </summary>
		/********************************************************************/
		public override string ToString()
		{
			return value?.ToString() ?? string.Empty;
		}
	}



	/// <summary>
	/// C# port of the C++ standard library std::variant with four
	/// alternatives. See <see cref="variant{T0,T1}"/> for the general remarks
	/// </summary>
	public class variant<T0, T1, T2, T3> : IEquatable<variant<T0, T1, T2, T3>>
	{
		private static readonly Type[] alternatives = [ typeof(T0), typeof(T1), typeof(T2), typeof(T3) ];

		private object value;
		private size_t typeIndex;

		/********************************************************************/
		/// <summary>
		/// Constructs an empty variant. This exists so the type can be used
		/// as a generic argument with a new() constraint; give it a value
		/// through a converting constructor or emplace before using it
		/// </summary>
		/********************************************************************/
		public variant()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the variant holding the given T0 value
		/// </summary>
		/********************************************************************/
		public variant(T0 v)
		{
			value = v;
			typeIndex = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the variant holding the given T1 value
		/// </summary>
		/********************************************************************/
		public variant(T1 v)
		{
			value = v;
			typeIndex = 1;
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the variant holding the given T2 value
		/// </summary>
		/********************************************************************/
		public variant(T2 v)
		{
			value = v;
			typeIndex = 2;
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the variant holding the given T3 value
		/// </summary>
		/********************************************************************/
		public variant(T3 v)
		{
			value = v;
			typeIndex = 3;
		}



		/********************************************************************/
		/// <summary>
		/// Converting construction/assignment from a T0 value
		/// </summary>
		/********************************************************************/
		public static implicit operator variant<T0, T1, T2, T3>(T0 v)
		{
			return new variant<T0, T1, T2, T3>(v);
		}



		/********************************************************************/
		/// <summary>
		/// Converting construction/assignment from a T1 value
		/// </summary>
		/********************************************************************/
		public static implicit operator variant<T0, T1, T2, T3>(T1 v)
		{
			return new variant<T0, T1, T2, T3>(v);
		}



		/********************************************************************/
		/// <summary>
		/// Converting construction/assignment from a T2 value
		/// </summary>
		/********************************************************************/
		public static implicit operator variant<T0, T1, T2, T3>(T2 v)
		{
			return new variant<T0, T1, T2, T3>(v);
		}



		/********************************************************************/
		/// <summary>
		/// Converting construction/assignment from a T3 value
		/// </summary>
		/********************************************************************/
		public static implicit operator variant<T0, T1, T2, T3>(T3 v)
		{
			return new variant<T0, T1, T2, T3>(v);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the zero-based index of the alternative currently held
		/// </summary>
		/********************************************************************/
		public size_t index()
		{
			return typeIndex;
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if the variant does not hold a value
		/// </summary>
		/********************************************************************/
		public bool valueless_by_exception()
		{
			return typeIndex == variant.variant_npos;
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if the alternative of type T is currently held
		/// </summary>
		/********************************************************************/
		public bool holds_alternative<T>()
		{
			return Variant_Internal.Holds<T>(typeIndex, alternatives);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the currently held value as type T
		/// </summary>
		/********************************************************************/
		public T get<T>()
		{
			return Variant_Internal.Get<T>(value, typeIndex, alternatives);
		}



		/********************************************************************/
		/// <summary>
		/// Retrieves the held value if the alternative of type T is held
		/// </summary>
		/********************************************************************/
		public bool get_if<T>(out T result)
		{
			return Variant_Internal.Get_If(value, typeIndex, alternatives, out result);
		}



		/********************************************************************/
		/// <summary>
		/// Replaces the held value with the given value of type T
		/// </summary>
		/********************************************************************/
		public T emplace<T>(T v)
		{
			typeIndex = Variant_Internal.Emplace_Index(typeof(T), alternatives);
			value = v;

			return v;
		}



		/********************************************************************/
		/// <summary>
		/// Applies the matching function to the held value and returns its
		/// result
		/// </summary>
		/********************************************************************/
		public R visit<R>(Func<T0, R> f0, Func<T1, R> f1, Func<T2, R> f2, Func<T3, R> f3)
		{
			switch (typeIndex)
			{
				case 0:
					return f0(value is null ? default : (T0)value);

				case 1:
					return f1(value is null ? default : (T1)value);

				case 2:
					return f2(value is null ? default : (T2)value);

				case 3:
					return f3(value is null ? default : (T3)value);

				default:
					throw new bad_variant_access();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Applies the matching action to the held value
		/// </summary>
		/********************************************************************/
		public void visit(Action<T0> f0, Action<T1> f1, Action<T2> f2, Action<T3> f3)
		{
			switch (typeIndex)
			{
				case 0:
				{
					f0(value is null ? default : (T0)value);
					break;
				}

				case 1:
				{
					f1(value is null ? default : (T1)value);
					break;
				}

				case 2:
				{
					f2(value is null ? default : (T2)value);
					break;
				}

				case 3:
				{
					f3(value is null ? default : (T3)value);
					break;
				}

				default:
					throw new bad_variant_access();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Swaps the contents of this variant with another
		/// </summary>
		/********************************************************************/
		public void swap(ref variant<T0, T1, T2, T3> other)
		{
			(value, other.value) = (other.value, value);
			(typeIndex, other.typeIndex) = (other.typeIndex, typeIndex);
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if both variants hold the same alternative with
		/// equal values
		/// </summary>
		/********************************************************************/
		public bool Equals(variant<T0, T1, T2, T3> other)
		{
			if (other is null)
				return false;

			return (typeIndex == other.typeIndex) && object.Equals(value, other.value);
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if the given object is a variant that is equal to
		/// this one
		/// </summary>
		/********************************************************************/
		public override bool Equals(object obj)
		{
			return obj is variant<T0, T1, T2, T3> other && Equals(other);
		}



		/********************************************************************/
		/// <summary>
		/// Returns a hash code based on the held alternative and value
		/// </summary>
		/********************************************************************/
		public override int GetHashCode()
		{
			return HashCode.Combine(typeIndex, value);
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if both variants are equal
		/// </summary>
		/********************************************************************/
		public static bool operator ==(variant<T0, T1, T2, T3> left, variant<T0, T1, T2, T3> right)
		{
			if (left is null)
				return right is null;

			return left.Equals(right);
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if the variants differ
		/// </summary>
		/********************************************************************/
		public static bool operator !=(variant<T0, T1, T2, T3> left, variant<T0, T1, T2, T3> right)
		{
			return !left.Equals(right);
		}



		/********************************************************************/
		/// <summary>
		/// Returns a string representation of the held value
		/// </summary>
		/********************************************************************/
		public override string ToString()
		{
			return value?.ToString() ?? string.Empty;
		}
	}
#pragma warning restore CS8981
}
