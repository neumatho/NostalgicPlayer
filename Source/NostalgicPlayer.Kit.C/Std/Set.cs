/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.C.Std
{
	/// <summary>
	/// C# port of the C++ standard library std::set
	/// (see the C++ standard, [set]).
	///
	/// A sorted associative container that holds unique values. The values
	/// are kept ordered by the comparison object (a std::less like
	/// IComparer‹Key›, the default being Comparer‹Key›.Default), so an
	/// in-order walk from <see cref="begin"/> to <see cref="end"/> visits
	/// the elements in ascending order. Search, insertion and removal have
	/// logarithmic complexity, as the container is implemented as a
	/// balanced (red-black) tree (see <see cref="Rb_Tree{TKey,TValue}"/>,
	/// which is shared with <see cref="map{Key,T}"/>).
	///
	/// Unlike a C++ std::set, this is a reference type (a C# class), so
	/// assigning one set‹Key› variable to another makes both refer to the
	/// same container. To make an independent copy (the behavior of C++
	/// copy construction and copy assignment), use the copy constructor
	/// set‹Key›(set‹Key›) or <see cref="MakeDeepClone"/>. To copy the
	/// contents into an already existing container, use
	/// <see cref="CopyTo"/>. To hand the contents over to a new container,
	/// leaving this one empty (the behavior of C++ move construction), use
	/// <see cref="MoveFrom"/> or Utility.move.
	///
	/// As with a C++ std::set, the container is node based: inserting an
	/// element never invalidates iterators or references to other
	/// elements, and erasing an element only invalidates iterators and
	/// references to that element. Iterators are bidirectional and can be
	/// moved with the ++ and -- operators.
	///
	/// Every operation that stores a value in the container makes an
	/// independent copy of it, whether it stores one element (insert and
	/// emplace) or many (copy construction, the items forms of the
	/// constructor and insert, and <see cref="CopyTo"/>). If a value
	/// implements IDeepCloneable‹Key›, the copy is a clone, and if it
	/// implements ICopyTo‹Key›, it is copied into a new instance of its
	/// own type (see <see cref="Copy_Insert{T}"/>), so that mutable
	/// reference type elements never become shared with the caller or
	/// between containers.
	///
	/// Notable differences from C++:
	/// - The stored value is not const, as C# has no const. A stored value
	///   must not be mutated, as that would break the ordering of the tree
	/// </summary>
#pragma warning disable CS8981
	public class set<Key> : IEquatable<set<Key>>, IEnumerable<Key>, IDeepCloneable<set<Key>>, ICopyTo<set<Key>>, IMoveable<set<Key>>
	{
#pragma warning restore CS8981
		private static readonly Func<Key, Key> keyOfValue = element => element;

		private readonly Rb_Tree<Key, Key> tree;

		/********************************************************************/
		/// <summary>
		/// Constructs an empty container using the default ordering
		/// (C++ set())
		/// </summary>
		/********************************************************************/
		public set()
		{
			tree = new Rb_Tree<Key, Key>(Comparer<Key>.Default, keyOfValue);
		}



		/********************************************************************/
		/// <summary>
		/// Constructs an empty container using the given ordering
		/// (C++ set(const Compare＆ comp))
		/// </summary>
		/********************************************************************/
		public set(IComparer<Key> comparer)
		{
			tree = new Rb_Tree<Key, Key>(comparer, keyOfValue);
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the container with the given items
		/// (C++ set(std::initializer_list‹value_type›)). If several
		/// items are equivalent, only the first one is kept
		/// </summary>
		/********************************************************************/
		public set(Key[] items)
		{
			tree = new Rb_Tree<Key, Key>(Comparer<Key>.Default, keyOfValue);

			if (items != null)
			{
				foreach (Key item in items)
					Insert_Internal(item);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Copy constructor. Constructs an independent container with a
		/// copy of the contents of the given container
		/// (C++ set(const set＆)). Values implementing IDeepCloneable‹Key›
		/// or ICopyTo‹Key› are copied, so that the two containers do not
		/// share element instances (see <see cref="Insert_Internal"/>)
		/// </summary>
		/********************************************************************/
		public set(set<Key> other)
		{
			if (other == null)
				throw new ArgumentNullException(nameof(other));

			tree = new Rb_Tree<Key, Key>(other.tree.Comparer, keyOfValue);

			for (Rb_Node<Key> n = other.tree.First_Node(); n != other.tree.Nil; n = other.tree.Next_Node(n))
				Insert_Internal(n.value);
		}

		#region Iterators
		/********************************************************************/
		/// <summary>
		/// Returns an iterator to the first element of the container
		/// (C++ begin()). If the container is empty, the returned iterator
		/// equals <see cref="end"/>
		/// </summary>
		/********************************************************************/
		public iterator begin()
		{
			return new iterator(this, tree.First_Node());
		}



		/********************************************************************/
		/// <summary>
		/// Returns an iterator to one past the last element of the
		/// container (C++ end())
		/// </summary>
		/********************************************************************/
		public iterator end()
		{
			return new iterator(this, tree.Nil);
		}
		#endregion

		#region Capacity
		/********************************************************************/
		/// <summary>
		/// Checks if the container has no elements (C++ empty())
		/// </summary>
		/********************************************************************/
		public bool empty()
		{
			return tree.Count == 0;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the number of elements in the container (C++ size())
		/// </summary>
		/********************************************************************/
		public size_t size()
		{
			return (size_t)tree.Count;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the maximum number of elements the container is able to
		/// hold (C++ max_size())
		/// </summary>
		/********************************************************************/
		public size_t max_size()
		{
			return (size_t)int.MaxValue;
		}
		#endregion

		#region Modifiers
		/********************************************************************/
		/// <summary>
		/// Erases all elements from the container. After this call, size()
		/// is zero (C++ clear())
		/// </summary>
		/********************************************************************/
		public void clear()
		{
			tree.Clear();
		}



		/********************************************************************/
		/// <summary>
		/// Inserts a copy of the given value if the container does not
		/// already hold an equivalent value
		/// (C++ insert(const value_type＆)).
		/// Returns a pair holding an iterator to the inserted element, or
		/// to the element that prevented the insertion, and a bool that is
		/// true if the insertion took place
		/// </summary>
		/********************************************************************/
		public pair<iterator, bool> insert(Key value)
		{
			(Rb_Node<Key> node, bool inserted) = Insert_Internal(value);

			return new pair<iterator, bool>(new iterator(this, node), inserted);
		}



		/********************************************************************/
		/// <summary>
		/// Inserts a copy of the given value if the container does not
		/// already hold an equivalent value
		/// (C++ insert(const_iterator hint, const value_type＆)). The hint
		/// is only an optimization and does not change the result. Returns
		/// an iterator to the inserted element, or to the element that
		/// prevented the insertion
		/// </summary>
		/********************************************************************/
		public iterator insert(iterator hint, Key value)
		{
			(Rb_Node<Key> node, _) = Insert_Internal(value);

			return new iterator(this, node);
		}



		/********************************************************************/
		/// <summary>
		/// Inserts a copy of each of the given items, skipping every item
		/// that is already present (C++ insert(InputIt first, InputIt
		/// last) and the initializer list overload)
		/// </summary>
		/********************************************************************/
		public void insert(Key[] items)
		{
			if (items != null)
			{
				foreach (Key item in items)
					Insert_Internal(item);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Inserts a new element constructed as a copy of the given value
		/// if the container does not already hold an equivalent value
		/// (C++ emplace(Args＆＆... args)). Returns a pair holding an
		/// iterator to the element and a bool that is true if the
		/// insertion took place
		/// </summary>
		/********************************************************************/
		public pair<iterator, bool> emplace(Key value)
		{
			(Rb_Node<Key> node, bool inserted) = Insert_Internal(value);

			return new pair<iterator, bool>(new iterator(this, node), inserted);
		}



		/********************************************************************/
		/// <summary>
		/// Inserts a new element constructed as a copy of the given value
		/// if the container does not already hold an equivalent value
		/// (C++ emplace_hint(const_iterator hint, Args＆＆... args)). The
		/// hint is only an optimization and does not change the result.
		/// Returns an iterator to the element
		/// </summary>
		/********************************************************************/
		public iterator emplace_hint(iterator hint, Key value)
		{
			(Rb_Node<Key> node, _) = Insert_Internal(value);

			return new iterator(this, node);
		}



		/********************************************************************/
		/// <summary>
		/// Removes the element at the given position (C++ erase(iterator
		/// pos)). Returns an iterator to the element after the removed
		/// element
		/// </summary>
		/********************************************************************/
		public iterator erase(iterator pos)
		{
			Rb_Node<Key> z = pos.Current_Node;

			if (z == tree.Nil)
				return end();

			Rb_Node<Key> next = tree.Next_Node(z);

			tree.Erase_Node(z);

			return new iterator(this, next);
		}



		/********************************************************************/
		/// <summary>
		/// Removes the element with the given value, if present
		/// (C++ erase(const Key＆ key)). Returns the number of removed
		/// elements, which is either zero or one
		/// </summary>
		/********************************************************************/
		public size_t erase(Key key)
		{
			Rb_Node<Key> z = tree.Find_Node(key);

			if (z == tree.Nil)
				return (size_t)0;

			tree.Erase_Node(z);

			return (size_t)1;
		}



		/********************************************************************/
		/// <summary>
		/// Removes the elements in the range [first, last)
		/// (C++ erase(iterator first, iterator last)). Returns an iterator
		/// to the element that followed the last removed element
		/// </summary>
		/********************************************************************/
		public iterator erase(iterator first, iterator last)
		{
			Rb_Node<Key> stop = last.Current_Node;
			Rb_Node<Key> cur = first.Current_Node;

			while (cur != stop)
			{
				Rb_Node<Key> next = tree.Next_Node(cur);
				tree.Erase_Node(cur);
				cur = next;
			}

			return new iterator(this, stop);
		}



		/********************************************************************/
		/// <summary>
		/// Exchanges the contents of the container with those of the other
		/// container (C++ swap(set＆ other))
		/// </summary>
		/********************************************************************/
		public void swap(set<Key> other)
		{
			if (other == null)
				throw new ArgumentNullException(nameof(other));

			tree.Swap(other.tree);
		}
		#endregion

		#region Lookup
		/********************************************************************/
		/// <summary>
		/// Returns the number of elements with the given value, which is
		/// either zero or one (C++ count(const Key＆ key))
		/// </summary>
		/********************************************************************/
		public size_t count(Key key)
		{
			return tree.Find_Node(key) != tree.Nil ? (size_t)1 : (size_t)0;
		}



		/********************************************************************/
		/// <summary>
		/// Finds the element with the given value (C++ find(const Key＆
		/// key)). Returns an iterator to the element, or <see cref="end"/>
		/// when no such element exists
		/// </summary>
		/********************************************************************/
		public iterator find(Key key)
		{
			return new iterator(this, tree.Find_Node(key));
		}



		/********************************************************************/
		/// <summary>
		/// Checks if the container has an element with the given value
		/// (C++ contains(const Key＆ key))
		/// </summary>
		/********************************************************************/
		public bool contains(Key key)
		{
			return tree.Find_Node(key) != tree.Nil;
		}



		/********************************************************************/
		/// <summary>
		/// Returns an iterator to the first element that is not less than
		/// the given value (C++ lower_bound(const Key＆ key)), or
		/// <see cref="end"/> if no such element exists
		/// </summary>
		/********************************************************************/
		public iterator lower_bound(Key key)
		{
			return new iterator(this, tree.Bound_Node(key, false));
		}



		/********************************************************************/
		/// <summary>
		/// Returns an iterator to the first element greater than the given
		/// value (C++ upper_bound(const Key＆ key)), or <see cref="end"/>
		/// if no such element exists
		/// </summary>
		/********************************************************************/
		public iterator upper_bound(Key key)
		{
			return new iterator(this, tree.Bound_Node(key, true));
		}



		/********************************************************************/
		/// <summary>
		/// Returns the range of elements with the given value
		/// (C++ equal_range(const Key＆ key)). As values are unique, the
		/// range holds at most one element. The returned pair is
		/// [lower_bound, upper_bound)
		/// </summary>
		/********************************************************************/
		public pair<iterator, iterator> equal_range(Key key)
		{
			return new pair<iterator, iterator>(lower_bound(key), upper_bound(key));
		}
		#endregion

		#region Observers
		/********************************************************************/
		/// <summary>
		/// Returns the comparison object that orders the values
		/// (C++ key_comp())
		/// </summary>
		/********************************************************************/
		public IComparer<Key> key_comp()
		{
			return tree.Comparer;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the comparison object that orders the elements
		/// (C++ value_comp()). As the value of an element is its key, this
		/// is the same object as the one <see cref="key_comp"/> returns
		/// </summary>
		/********************************************************************/
		public IComparer<Key> value_comp()
		{
			return tree.Comparer;
		}
		#endregion

		#region Comparison operators
		/********************************************************************/
		/// <summary>
		/// Checks if the contents of the two containers are equal
		/// (C++ operator==)
		/// </summary>
		/********************************************************************/
		public static bool operator ==(set<Key> left, set<Key> right)
		{
			if (ReferenceEquals(left, right))
				return true;

			if ((left is null) || (right is null))
				return false;

			return left.Equals(right);
		}



		/********************************************************************/
		/// <summary>
		/// Checks if the contents of the two containers are not equal
		/// (C++ operator!=)
		/// </summary>
		/********************************************************************/
		public static bool operator !=(set<Key> left, set<Key> right)
		{
			return !(left == right);
		}



		/********************************************************************/
		/// <summary>
		/// Compares the contents of the two containers lexicographically
		/// (C++ operator‹)
		/// </summary>
		/********************************************************************/
		public static bool operator <(set<Key> left, set<Key> right)
		{
			return Compare(left, right) < 0;
		}



		/********************************************************************/
		/// <summary>
		/// Compares the contents of the two containers lexicographically
		/// (C++ operator‹=)
		/// </summary>
		/********************************************************************/
		public static bool operator <=(set<Key> left, set<Key> right)
		{
			return Compare(left, right) <= 0;
		}



		/********************************************************************/
		/// <summary>
		/// Compares the contents of the two containers lexicographically
		/// (C++ operator›)
		/// </summary>
		/********************************************************************/
		public static bool operator >(set<Key> left, set<Key> right)
		{
			return Compare(left, right) > 0;
		}



		/********************************************************************/
		/// <summary>
		/// Compares the contents of the two containers lexicographically
		/// (C++ operator›=)
		/// </summary>
		/********************************************************************/
		public static bool operator >=(set<Key> left, set<Key> right)
		{
			return Compare(left, right) >= 0;
		}
		#endregion

		#region IEquatable implementation
		/********************************************************************/
		/// <summary>
		/// Checks if the contents of this container are equal to the
		/// contents of the other container
		/// </summary>
		/********************************************************************/
		public bool Equals(set<Key> other)
		{
			if (other is null)
				return false;

			if (ReferenceEquals(this, other))
				return true;

			if (tree.Count != other.tree.Count)
				return false;

			EqualityComparer<Key> valueComparer = EqualityComparer<Key>.Default;

			Rb_Node<Key> a = tree.First_Node();
			Rb_Node<Key> b = other.tree.First_Node();

			while ((a != tree.Nil) && (b != other.tree.Nil))
			{
				if (!valueComparer.Equals(a.value, b.value))
					return false;

				a = tree.Next_Node(a);
				b = other.tree.Next_Node(b);
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Checks if the contents of this container are equal to the
		/// contents of the other container
		/// </summary>
		/********************************************************************/
		public override bool Equals(object obj)
		{
			return Equals(obj as set<Key>);
		}



		/********************************************************************/
		/// <summary>
		/// Returns a hash code based on the contents of the container
		/// </summary>
		/********************************************************************/
		public override int GetHashCode()
		{
			HashCode hash = new HashCode();

			for (Rb_Node<Key> n = tree.First_Node(); n != tree.Nil; n = tree.Next_Node(n))
				hash.Add(n.value);

			return hash.ToHashCode();
		}
		#endregion

		#region IEnumerable implementation
		/********************************************************************/
		/// <summary>
		/// Returns an enumerator that walks the elements in ascending
		/// order, allowing the container to be used in a foreach loop
		/// </summary>
		/********************************************************************/
		public IEnumerator<Key> GetEnumerator()
		{
			for (Rb_Node<Key> n = tree.First_Node(); n != tree.Nil; n = tree.Next_Node(n))
				yield return n.value;
		}



		/********************************************************************/
		/// <summary>
		/// Returns an enumerator that walks the elements in ascending
		/// order
		/// </summary>
		/********************************************************************/
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion

		#region IDeepCloneable implementation
		/********************************************************************/
		/// <summary>
		/// Returns an independent copy of the container, holding a copy of
		/// each element. This is the same as using the copy constructor
		/// set‹Key›(set‹Key›)
		/// </summary>
		/********************************************************************/
		public virtual set<Key> MakeDeepClone()
		{
			return new set<Key>(this);
		}
		#endregion

		#region ICopyTo implementation
		/********************************************************************/
		/// <summary>
		/// Replaces the contents of the given container with a copy of the
		/// contents of this one, holding a copy of each value. The
		/// ordering is copied as well. This is the same as C++ copy
		/// assignment (destination = *this), so it invalidates all
		/// iterators and references that refer to elements of the
		/// destination
		/// </summary>
		/********************************************************************/
		public void CopyTo(set<Key> destination)
		{
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));

			if (ReferenceEquals(destination, this))
				return;

			destination.tree.Comparer = tree.Comparer;
			destination.clear();

			for (Rb_Node<Key> n = tree.First_Node(); n != tree.Nil; n = tree.Next_Node(n))
				destination.Insert_Internal(n.value);
		}
		#endregion

		#region IMoveable implementation
		/********************************************************************/
		/// <summary>
		/// Returns a container that has taken over the contents of this
		/// one, leaving this one empty. This is what C++ move construction
		/// (set(set＆＆)) does: the elements and the ordering are handed
		/// over as they are and not copied, so the iterators and
		/// references that refer to elements stay valid and now refer to
		/// elements of the returned container
		/// </summary>
		/********************************************************************/
		public virtual set<Key> MoveFrom()
		{
			set<Key> moved = new set<Key>(tree.Comparer);

			moved.tree.Take_From(tree);

			return moved;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Returns the value to store in a new element, which is an
		/// independent copy of the given value. If the value implements
		/// IDeepCloneable‹U›, its MakeDeepClone() is used to make a new
		/// instance, and if it implements ICopyTo‹U›, it is copied into a
		/// new instance of its own type. Otherwise the value is returned
		/// as is, which is the correct behavior for value types and
		/// immutable reference types (see <see cref="Copy_Insert{T}"/>)
		/// </summary>
		/********************************************************************/
		private static U Create_Value<U>(U value)
		{
			return Copy_Insert<U>.Create(value);
		}



		/********************************************************************/
		/// <summary>
		/// Inserts an element holding a copy of the given value if no
		/// equivalent element exists. Returns the node and a flag that
		/// says if a node was created. An existing node is never modified
		/// </summary>
		/********************************************************************/
		private (Rb_Node<Key> node, bool inserted) Insert_Internal(Key key)
		{
			Rb_Node<Key> existing = tree.Find_Insert_Position(key, out Rb_Node<Key> parent, out bool asLeftChild);

			if (existing != tree.Nil)
				return (existing, false);

			return (tree.Insert_Node(parent, asLeftChild, Create_Value(key)), true);
		}



		/********************************************************************/
		/// <summary>
		/// Compares the contents of the two containers lexicographically,
		/// returning a negative value, zero or a positive value
		/// </summary>
		/********************************************************************/
		private static int Compare(set<Key> left, set<Key> right)
		{
			if (ReferenceEquals(left, right))
				return 0;

			if (left is null)
				return -1;

			if (right is null)
				return 1;

			Comparer<Key> valueComparer = Comparer<Key>.Default;

			Rb_Node<Key> a = left.tree.First_Node();
			Rb_Node<Key> b = right.tree.First_Node();

			while ((a != left.tree.Nil) && (b != right.tree.Nil))
			{
				int c = valueComparer.Compare(a.value, b.value);
				if (c != 0)
					return c;

				a = left.tree.Next_Node(a);
				b = right.tree.Next_Node(b);
			}

			return left.tree.Count.CompareTo(right.tree.Count);
		}
		#endregion

		/// <summary>
		/// A bidirectional iterator over the elements of a set‹Key›
		/// (C++ set::iterator). It refers to a single element and stays
		/// valid as long as that element is not erased. Use the ++ and --
		/// operators to move to the next or previous item, and the
		/// <see cref="Value"/> property to read the referred element.
		///
		/// As in C++, where a set iterator refers to a const element, the
		/// element cannot be replaced through the iterator, as that would
		/// break the ordering of the tree
		/// </summary>
#pragma warning disable CS8981
		public struct iterator : IEquatable<iterator>
		{
#pragma warning restore CS8981
			private readonly set<Key> owner;
			private readonly Rb_Node<Key> node;

			/****************************************************************/
			/// <summary>
			/// Constructs an iterator that refers to the given node of
			/// the given container
			/// </summary>
			/****************************************************************/
			internal iterator(set<Key> owner, Rb_Node<Key> node)
			{
				this.owner = owner;
				this.node = node;
			}



			/****************************************************************/
			/// <summary>
			/// Returns the node the iterator refers to, for the use of
			/// the enclosing container
			/// </summary>
			/****************************************************************/
			internal Rb_Node<Key> Current_Node => node;



			/****************************************************************/
			/// <summary>
			/// Returns the referred element (C++ *iterator). Reading this
			/// on the end iterator is undefined
			/// </summary>
			/****************************************************************/
			public Key Value => node.value;



			/****************************************************************/
			/// <summary>
			/// Advances the iterator to the next element (C++ operator++)
			/// </summary>
			/****************************************************************/
			public static iterator operator ++(iterator it)
			{
				return new iterator(it.owner, it.owner.tree.Next_Node(it.node));
			}



			/****************************************************************/
			/// <summary>
			/// Moves the iterator to the previous element
			/// (C++ operator--)
			/// </summary>
			/****************************************************************/
			public static iterator operator --(iterator it)
			{
				return new iterator(it.owner, it.owner.tree.Prev_Node(it.node));
			}



			/****************************************************************/
			/// <summary>
			/// Returns an iterator to the next element (C++ std::next).
			/// Unlike the ++ operator, this can be applied to a value
			/// that is not a variable, such as the result of a method
			/// call
			/// </summary>
			/****************************************************************/
			public iterator Next()
			{
				return new iterator(owner, owner.tree.Next_Node(node));
			}



			/****************************************************************/
			/// <summary>
			/// Returns an iterator to the previous element
			/// (C++ std::prev). Unlike the -- operator, this can be
			/// applied to a value that is not a variable, such as the
			/// result of a method call
			/// </summary>
			/****************************************************************/
			public iterator Prev()
			{
				return new iterator(owner, owner.tree.Prev_Node(node));
			}



			/****************************************************************/
			/// <summary>
			/// Checks if the two iterators refer to the same element
			/// (C++ operator==)
			/// </summary>
			/****************************************************************/
			public static bool operator ==(iterator left, iterator right)
			{
				return left.Equals(right);
			}



			/****************************************************************/
			/// <summary>
			/// Checks if the two iterators refer to different elements
			/// (C++ operator!=)
			/// </summary>
			/****************************************************************/
			public static bool operator !=(iterator left, iterator right)
			{
				return !left.Equals(right);
			}



			/****************************************************************/
			/// <summary>
			/// Checks if this iterator refers to the same element as
			/// another iterator
			/// </summary>
			/****************************************************************/
			public bool Equals(iterator other)
			{
				return ReferenceEquals(node, other.node);
			}



			/****************************************************************/
			/// <summary>
			/// Checks if the given object is an iterator that refers to
			/// the same element as this one
			/// </summary>
			/****************************************************************/
			public override bool Equals(object obj)
			{
				return (obj is iterator other) && Equals(other);
			}



			/****************************************************************/
			/// <summary>
			/// Returns a hash code for the iterator
			/// </summary>
			/****************************************************************/
			public override int GetHashCode()
			{
				return node == null ? 0 : node.GetHashCode();
			}
		}
	}
}
