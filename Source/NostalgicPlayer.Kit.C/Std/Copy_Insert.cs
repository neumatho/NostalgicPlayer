/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Concurrent;
using System.Reflection;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.C.Std
{
	/// <summary>
	/// Makes the independent copy of a value that a container stores when it
	/// is given a value to copy, which C++ calls copy insertion (the copy
	/// constructors, copy assignment, std::array::fill, the value taking
	/// std::vector operations and std::map::insert, among others).
	///
	/// In C++ such an element is copy constructed when the container makes a
	/// new element, and copy assigned when it already holds an element that
	/// the value goes into. C# has neither operation, so the type of the
	/// value is asked instead, in this order:
	///
	/// A value implementing IDeepCloneable‹T› is asked for a clone of
	/// itself.
	///
	/// A value implementing ICopyTo‹T› copies itself into the element that
	/// the container already holds, which is what C++ copy assignment does,
	/// so that the pointers, iterators and references that refer to the
	/// element keep on referring to it (see <see cref="Assign"/>). Only when
	/// there is no element to copy into is a new instance made, which needs
	/// the type of the value to have a public parameterless constructor,
	/// just like default insertion does (see <see cref="Default_Insert{T}"/>).
	///
	/// Any other value is stored as it is, which is the correct behavior for
	/// value types and immutable reference types
	/// </summary>
	internal static class Copy_Insert<T>
	{
		private static readonly ConcurrentDictionary<Type, Func<object>> factories = new ConcurrentDictionary<Type, Func<object>>();

		/********************************************************************/
		/// <summary>
		/// Returns the value to store in a new element, which is an
		/// independent copy of the given value, or the value itself when its
		/// type does not offer a way to copy it
		/// </summary>
		/********************************************************************/
		public static T Create(T value)
		{
			return Assign(default, value);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the value to store in an element that already holds the
		/// given instance. When the value can copy itself into that
		/// instance, it does so and the instance is kept, so that everything
		/// referring to the element sees the new value
		/// </summary>
		/********************************************************************/
		public static T Assign(T destination, T value)
		{
			if (value is IDeepCloneable<T> cloneable)
				return cloneable.MakeDeepClone();

			if (value is ICopyTo<T> copyable)
			{
				Type type = value.GetType();

				// A value type is copied by the assignment itself, so it
				// never needs an instance to copy into
				if (type.IsValueType)
					return value;

				// The instance already stored can only be kept when it is of
				// the same type as the value, as it has to be able to hold
				// everything that the value copies into it
				if ((destination != null) && (destination.GetType() == type))
				{
					if (!ReferenceEquals(destination, value))
						copyable.CopyTo(destination);

					return destination;
				}

				T instance = Create_Instance(type);

				if (instance != null)
				{
					copyable.CopyTo(instance);
					return instance;
				}
			}

			return value;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Returns a new instance of the given type to copy a value into, or
		/// default(T) when the type cannot be given one
		/// </summary>
		/********************************************************************/
		private static T Create_Instance(Type type)
		{
			Func<object> factory = factories.GetOrAdd(type, Make_Factory);

			return factory == null ? default : (T)factory();
		}



		/********************************************************************/
		/// <summary>
		/// Returns the method that creates a new instance of the given type,
		/// or null when the type cannot have one
		/// </summary>
		/********************************************************************/
		private static Func<object> Make_Factory(Type type)
		{
			// A type without an accessible parameterless constructor cannot
			// be given an instance to copy into. In C++ such an element type
			// would not compile, so sharing the instance is the closest we
			// can get
			ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);

			return ctor != null ? () => Activator.CreateInstance(type) : null;
		}
		#endregion
	}
}
