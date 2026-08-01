/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Reflection;

namespace Polycode.NostalgicPlayer.Kit.C.Std
{
	/// <summary>
	/// Creates the elements that a container adds when it is not given a value
	/// to copy, which C++ calls default insertion (the std::array
	/// declaration, the std::vector operations vector(size_type) and
	/// resize(size_type), and std::map::operator[], which value initializes
	/// the mapped type).
	///
	/// In C++ those elements are default constructed, which is why the
	/// element type has to be default constructible for the operations to
	/// compile at all. C# cannot express that requirement without putting a
	/// new() constraint on the container itself, which would rule out useful
	/// element types such as string, so the element type is inspected once
	/// instead
	/// </summary>
	internal static class Default_Insert<T>
	{
		private static readonly Func<T> factory = Make_Factory();

		/********************************************************************/
		/// <summary>
		/// Returns a single default constructed element, or default(T) when the
		/// element type cannot be default constructed
		/// </summary>
		/********************************************************************/
		public static T Create()
		{
			return factory == null ? default : factory();
		}



		/********************************************************************/
		/// <summary>
		/// Default constructs the given number of elements of the buffer,
		/// starting at the given index. Nothing is done when the element type
		/// cannot be default constructed, as every position of the buffer
		/// already holds default(T)
		/// </summary>
		/********************************************************************/
		public static void Fill(T[] buffer, int index, int count)
		{
			if (factory == null)
				return;

			for (int i = 0; i < count; i++)
				buffer[index + i] = factory();
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Returns the method that creates a single default constructed
		/// element, or null when the element type does not need one or cannot
		/// have one
		/// </summary>
		/********************************************************************/
		private static Func<T> Make_Factory()
		{
			Type type = typeof(T);

			// A value type is already value initialized by the runtime, which
			// is exactly what C++ does for the same element type
			if (type.IsValueType)
				return null;

			// An abstract class or an interface can never be instantiated
			if (type.IsAbstract)
				return null;

			// A reference type without an accessible parameterless
			// constructor, such as string or an array type, cannot be default
			// constructed. In C++ such an element type would not compile, so
			// leaving the elements as null is the closest we can get
			ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);

			return ctor != null ? Activator.CreateInstance<T> : null;
		}
		#endregion
	}
}
