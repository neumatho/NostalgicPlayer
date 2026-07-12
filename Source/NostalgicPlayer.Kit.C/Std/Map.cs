/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.C.Std.Exceptions;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.C.Std
{
	/// <summary>
	/// C# port of the C++ standard library std::map
	/// (see the C++ standard, [map]).
	///
	/// A sorted associative container that holds key-value pairs with unique
	/// keys. The keys are kept ordered by the comparison object (a std::less
	/// like IComparer‹Key›, the default being Comparer‹Key›.Default), so an
	/// in-order walk from <see cref="begin"/> to <see cref="end"/> visits the
	/// elements in ascending key order. Search, insertion and removal have
	/// logarithmic complexity, as the container is implemented as a balanced
	/// (red-black) tree.
	///
	/// Unlike a C++ std::map, this is a reference type (a C# class), so
	/// assigning one map‹Key, T› variable to another makes both refer to the
	/// same container. To make an independent copy (the behavior of C++ copy
	/// construction and copy assignment), use the copy constructor
	/// map‹Key, T›(map‹Key, T›).
	///
	/// As with a C++ std::map, the container is node based: inserting an
	/// element never invalidates iterators or references to other elements,
	/// and erasing an element only invalidates iterators and references to
	/// that element. Iterators are bidirectional and can be moved with the
	/// ++ and -- operators.
	///
	/// The copy constructor makes an independent copy of each source element.
	/// If a key or a mapped value implements IDeepCloneable‹T›, its
	/// MakeDeepClone() is used to obtain that copy, so that mutable reference
	/// type elements do not become shared between containers. The single
	/// element operations (insert, insert_or_assign, emplace, try_emplace and
	/// operator[]) store the given instance directly.
	///
	/// Notable differences from C++:
	/// - The value type is pair‹Key, T› rather than pair‹const Key, T›, as C#
	///   has no const. The key of a stored element must not be mutated, as
	///   that would break the ordering of the tree.
	/// - The elements are value initialized to default(T). For a reference
	///   type this means null, where C++ would have default constructed an
	///   object
	/// </summary>
#pragma warning disable CS8981
	public class map<Key, T> : IEquatable<map<Key, T>>, IEnumerable<pair<Key, T>>
	{
#pragma warning restore CS8981
		internal sealed class Node
		{
			public Node parent;
			public Node left;
			public Node right;
			public bool red;
			public pair<Key, T> value;
		}

		private Node nil;
		private Node root;
		private int nodeCount;
		private IComparer<Key> comparer;

		/********************************************************************/
		/// <summary>
		/// Constructs an empty container using the default key ordering
		/// (C++ map())
		/// </summary>
		/********************************************************************/
		public map()
		{
			comparer = Comparer<Key>.Default;
			Init_Empty();
		}



		/********************************************************************/
		/// <summary>
		/// Constructs an empty container using the given key ordering
		/// (C++ map(const Compare＆ comp))
		/// </summary>
		/********************************************************************/
		public map(IComparer<Key> comparer)
		{
			this.comparer = comparer ?? Comparer<Key>.Default;
			Init_Empty();
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the container with the given items
		/// (C++ map(std::initializer_list‹value_type›)). If several items
		/// share the same key, only the first one is kept
		/// </summary>
		/********************************************************************/
		public map(pair<Key, T>[] items)
		{
			comparer = Comparer<Key>.Default;
			Init_Empty();

			if (items != null)
			{
				foreach (pair<Key, T> item in items)
					Insert_Internal(Clone_Value(item.first), Clone_Value(item.second));
			}
		}



		/********************************************************************/
		/// <summary>
		/// Copy constructor. Constructs an independent container with a copy
		/// of the contents of the given container (C++ map(const map＆)). Keys
		/// and mapped values implementing IDeepCloneable‹T› are deep cloned,
		/// so that the two containers do not share element instances
		/// </summary>
		/********************************************************************/
		public map(map<Key, T> other)
		{
			if (other == null)
				throw new ArgumentNullException(nameof(other));

			comparer = other.comparer;
			Init_Empty();

			for (Node n = other.Minimum(other.root); n != other.nil; n = other.Successor(n))
				Insert_Internal(Clone_Value(n.value.first), Clone_Value(n.value.second));
		}

		#region Element access
		/********************************************************************/
		/// <summary>
		/// Returns a reference to the mapped value of the element with the
		/// given key, with bounds checking (C++ at(const Key＆ key)). Throws
		/// out_of_range if the container has no such element
		/// </summary>
		/********************************************************************/
		public ref T at(Key key)
		{
			Node n = Find_Node(key);

			if (n == nil)
				throw new out_of_range("map.at: key not found");

			return ref n.value.second;
		}



		/********************************************************************/
		/// <summary>
		/// Returns a reference to the mapped value of the element with the
		/// given key, inserting a new element with a default value
		/// if no such element exists (C++ operator[](const Key＆ key))
		/// </summary>
		/********************************************************************/
		public ref T this[Key key]
		{
			get
			{
				(Node node, _) = Insert_Internal(key, default);

				return ref node.value.second;
			}
		}
		#endregion

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
			return new iterator(this, Minimum(root));
		}



		/********************************************************************/
		/// <summary>
		/// Returns an iterator to one past the last element of the container
		/// (C++ end())
		/// </summary>
		/********************************************************************/
		public iterator end()
		{
			return new iterator(this, nil);
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
			return nodeCount == 0;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the number of elements in the container (C++ size())
		/// </summary>
		/********************************************************************/
		public size_t size()
		{
			return (size_t)nodeCount;
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
		/// Erases all elements from the container. After this call, size() is
		/// zero (C++ clear())
		/// </summary>
		/********************************************************************/
		public void clear()
		{
			root = nil;
			nodeCount = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Inserts the given element if the container does not contain
		/// an element with an equivalent key (C++ insert(const value_type＆)).
		/// Returns a pair holding an iterator to the inserted element, or to
		/// the element that prevented the insertion, and a bool that is true
		/// if the insertion took place
		/// </summary>
		/********************************************************************/
		public pair<iterator, bool> insert(pair<Key, T> value)
		{
			(Node node, bool inserted) = Insert_Internal(value.first, value.second);

			return new pair<iterator, bool>(new iterator(this, node), inserted);
		}



		/********************************************************************/
		/// <summary>
		/// Inserts the given items, skipping every item whose key is already
		/// present (C++ insert(InputIt first, InputIt last) and the
		/// initializer list overload)
		/// </summary>
		/********************************************************************/
		public void insert(pair<Key, T>[] items)
		{
			if (items != null)
			{
				foreach (pair<Key, T> item in items)
					Insert_Internal(item.first, item.second);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Inserts a new element with the given key and value, or assigns the
		/// given value to the element if the key is already present
		/// (C++ insert_or_assign(const Key＆ key, M＆＆ obj)). Returns a pair
		/// holding an iterator to the element and a bool that is true if the
		/// insertion took place
		/// </summary>
		/********************************************************************/
		public pair<iterator, bool> insert_or_assign(Key key, T value)
		{
			(Node node, bool inserted) = Insert_Internal(key, value);

			if (!inserted)
				node.value.second = value;

			return new pair<iterator, bool>(new iterator(this, node), inserted);
		}



		/********************************************************************/
		/// <summary>
		/// Inserts a new element with the given key and value, or assigns
		/// the given value to the element if the key is already present
		/// (C++ insert_or_assign(const_iterator hint, const Key＆ key,
		/// M＆＆ obj)). The hint is only an optimization and does not change
		/// the result. Returns an iterator to the inserted or updated element
		/// </summary>
		/********************************************************************/
		public iterator insert_or_assign(iterator hint, Key key, T value)
		{
			(Node node, bool inserted) = Insert_Internal(key, value);

			if (!inserted)
				node.value.second = value;

			return new iterator(this, node);
		}



		/********************************************************************/
		/// <summary>
		/// Inserts a new element constructed from the given key and value if
		/// the container does not already contain an element with an
		/// equivalent key (C++ emplace(Args＆＆... args)). Returns a pair
		/// holding an iterator to the element and a bool that is true if the
		/// insertion took place
		/// </summary>
		/********************************************************************/
		public pair<iterator, bool> emplace(Key key, T value)
		{
			(Node node, bool inserted) = Insert_Internal(key, value);

			return new pair<iterator, bool>(new iterator(this, node), inserted);
		}



		/********************************************************************/
		/// <summary>
		/// Inserts an element with the given key and value if the container
		/// does not already contain an element with an equivalent key. Unlike
		/// emplace, the value is not consumed if no insertion takes place
		/// (C++ try_emplace(const Key＆ key, Args＆＆... args)). Returns a pair
		/// holding an iterator to the element and a bool that is true if the
		/// insertion took place
		/// </summary>
		/********************************************************************/
		public pair<iterator, bool> try_emplace(Key key, T value)
		{
			(Node node, bool inserted) = Insert_Internal(key, value);

			return new pair<iterator, bool>(new iterator(this, node), inserted);
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
			Node z = pos.Current_Node;

			if (z == nil)
				return end();

			Node next = Successor(z);

			Delete_Node(z);

			return new iterator(this, next);
		}



		/********************************************************************/
		/// <summary>
		/// Removes the element with the given key, if present
		/// (C++ erase(const Key＆ key)). Returns the number of removed
		/// elements, which is either zero or one
		/// </summary>
		/********************************************************************/
		public size_t erase(Key key)
		{
			Node z = Find_Node(key);

			if (z == nil)
				return (size_t)0;

			Delete_Node(z);

			return (size_t)1;
		}



		/********************************************************************/
		/// <summary>
		/// Removes the elements in the range [first, last)
		/// (C++ erase(iterator first, iterator last)). Returns an iterator to
		/// the element that followed the last removed element
		/// </summary>
		/********************************************************************/
		public iterator erase(iterator first, iterator last)
		{
			Node stop = last.Current_Node;
			Node cur = first.Current_Node;

			while (cur != stop)
			{
				Node next = Successor(cur);
				Delete_Node(cur);
				cur = next;
			}

			return new iterator(this, stop);
		}



		/********************************************************************/
		/// <summary>
		/// Exchanges the contents of the container with those of the other
		/// container (C++ swap(map＆ other))
		/// </summary>
		/********************************************************************/
		public void swap(map<Key, T> other)
		{
			if (other == null)
				throw new ArgumentNullException(nameof(other));

			(nil, other.nil) = (other.nil, nil);
			(root, other.root) = (other.root, root);
			(nodeCount, other.nodeCount) = (other.nodeCount, nodeCount);
			(comparer, other.comparer) = (other.comparer, comparer);
		}
		#endregion

		#region Lookup
		/********************************************************************/
		/// <summary>
		/// Returns the number of elements with the given key, which is either
		/// zero or one (C++ count(const Key＆ key))
		/// </summary>
		/********************************************************************/
		public size_t count(Key key)
		{
			return Find_Node(key) != nil ? (size_t)1 : (size_t)0;
		}



		/********************************************************************/
		/// <summary>
		/// Finds the element with the given key (C++ find(const Key＆ key)).
		/// Returns an iterator to the element, or <see cref="end"/> when
		/// no such element exists
		/// </summary>
		/********************************************************************/
		public iterator find(Key key)
		{
			return new iterator(this, Find_Node(key));
		}



		/********************************************************************/
		/// <summary>
		/// Checks if the container has an element with the given key
		/// (C++ contains(const Key＆ key))
		/// </summary>
		/********************************************************************/
		public bool contains(Key key)
		{
			return Find_Node(key) != nil;
		}



		/********************************************************************/
		/// <summary>
		/// Returns an iterator to the first element with a key that is not
		/// less than the given key (C++ lower_bound(const Key＆ key)), or
		/// <see cref="end"/> if no such element exists
		/// </summary>
		/********************************************************************/
		public iterator lower_bound(Key key)
		{
			return new iterator(this, Bound_Node(key, false));
		}



		/********************************************************************/
		/// <summary>
		/// Returns an iterator to the first element with a key greater
		/// than the given key (C++ upper_bound(const Key＆ key)), or
		/// <see cref="end"/> if no such element exists
		/// </summary>
		/********************************************************************/
		public iterator upper_bound(Key key)
		{
			return new iterator(this, Bound_Node(key, true));
		}



		/********************************************************************/
		/// <summary>
		/// Returns the range of elements with the given key
		/// (C++ equal_range(const Key＆ key)). As keys are unique, the range
		/// holds at most one element. The returned pair is
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
		/// Returns the comparison object that orders the keys
		/// (C++ key_comp())
		/// </summary>
		/********************************************************************/
		public IComparer<Key> key_comp()
		{
			return comparer;
		}
		#endregion

		#region Comparison operators
		/********************************************************************/
		/// <summary>
		/// Checks if the contents of the two containers are equal
		/// (C++ operator==)
		/// </summary>
		/********************************************************************/
		public static bool operator ==(map<Key, T> left, map<Key, T> right)
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
		public static bool operator !=(map<Key, T> left, map<Key, T> right)
		{
			return !(left == right);
		}



		/********************************************************************/
		/// <summary>
		/// Compares the contents of the two containers lexicographically
		/// (C++ operator‹)
		/// </summary>
		/********************************************************************/
		public static bool operator <(map<Key, T> left, map<Key, T> right)
		{
			return Compare(left, right) < 0;
		}



		/********************************************************************/
		/// <summary>
		/// Compares the contents of the two containers lexicographically
		/// (C++ operator‹=)
		/// </summary>
		/********************************************************************/
		public static bool operator <=(map<Key, T> left, map<Key, T> right)
		{
			return Compare(left, right) <= 0;
		}



		/********************************************************************/
		/// <summary>
		/// Compares the contents of the two containers lexicographically
		/// (C++ operator›)
		/// </summary>
		/********************************************************************/
		public static bool operator >(map<Key, T> left, map<Key, T> right)
		{
			return Compare(left, right) > 0;
		}



		/********************************************************************/
		/// <summary>
		/// Compares the contents of the two containers lexicographically
		/// (C++ operator›=)
		/// </summary>
		/********************************************************************/
		public static bool operator >=(map<Key, T> left, map<Key, T> right)
		{
			return Compare(left, right) >= 0;
		}
		#endregion

		#region IEquatable implementation
		/********************************************************************/
		/// <summary>
		/// Checks if the contents of this container are equal to the contents
		/// of the other container
		/// </summary>
		/********************************************************************/
		public bool Equals(map<Key, T> other)
		{
			if (other is null)
				return false;

			if (ReferenceEquals(this, other))
				return true;

			if (nodeCount != other.nodeCount)
				return false;

			EqualityComparer<Key> keyComparer = EqualityComparer<Key>.Default;
			EqualityComparer<T> valueComparer = EqualityComparer<T>.Default;

			Node a = Minimum(root);
			Node b = other.Minimum(other.root);

			while ((a != nil) && (b != other.nil))
			{
				if (!keyComparer.Equals(a.value.first, b.value.first))
					return false;

				if (!valueComparer.Equals(a.value.second, b.value.second))
					return false;

				a = Successor(a);
				b = other.Successor(b);
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Checks if the contents of this container are equal to the contents
		/// of the other container
		/// </summary>
		/********************************************************************/
		public override bool Equals(object obj)
		{
			return Equals(obj as map<Key, T>);
		}



		/********************************************************************/
		/// <summary>
		/// Returns a hash code based on the contents of the container
		/// </summary>
		/********************************************************************/
		public override int GetHashCode()
		{
			HashCode hash = new HashCode();

			for (Node n = Minimum(root); n != nil; n = Successor(n))
			{
				hash.Add(n.value.first);
				hash.Add(n.value.second);
			}

			return hash.ToHashCode();
		}
		#endregion

		#region IEnumerable implementation
		/********************************************************************/
		/// <summary>
		/// Returns an enumerator that walks the elements in ascending key
		/// order, allowing the container to be used in a foreach loop
		/// </summary>
		/********************************************************************/
		public IEnumerator<pair<Key, T>> GetEnumerator()
		{
			for (Node n = Minimum(root); n != nil; n = Successor(n))
				yield return n.value;
		}



		/********************************************************************/
		/// <summary>
		/// Returns an enumerator that walks the elements in ascending key
		/// order
		/// </summary>
		/********************************************************************/
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Initializes the tree to an empty state, creating the sentinel node
		/// that stands in for a null child (as in a classic red-black tree)
		/// </summary>
		/********************************************************************/
		private void Init_Empty()
		{
			nil = new Node();
			nil.red = false;
			nil.parent = nil;
			nil.left = nil;
			nil.right = nil;

			root = nil;
			nodeCount = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Returns an independent copy of the given value. If the value
		/// implements IDeepCloneable‹U›, its MakeDeepClone() is used to make
		/// a new instance. Otherwise the value is returned as is, which is
		/// the correct behavior for value types and immutable reference types
		/// </summary>
		/********************************************************************/
		private static U Clone_Value<U>(U value)
		{
			return value is IDeepCloneable<U> cloneable ? cloneable.MakeDeepClone() : value;
		}



		/********************************************************************/
		/// <summary>
		/// Compares the two keys, returning a negative value, zero or a
		/// positive value
		/// </summary>
		/********************************************************************/
		private int Cmp(Key a, Key b)
		{
			return comparer.Compare(a, b);
		}



		/********************************************************************/
		/// <summary>
		/// Inserts an element with the given key and value if no element with
		/// an equivalent key exists. Returns the node and a flag that
		/// says if a node was created. An existing node is never modified
		/// </summary>
		/********************************************************************/
		private (Node node, bool inserted) Insert_Internal(Key key, T value)
		{
			Node y = nil;
			Node x = root;
			int c = 0;

			while (x != nil)
			{
				y = x;
				c = Cmp(key, x.value.first);

				if (c == 0)
					return (x, false);

				x = c < 0 ? x.left : x.right;
			}

			Node z = new Node
			{
				value = new pair<Key, T>(key, value),
				parent = y,
				left = nil,
				right = nil,
				red = true
			};

			if (y == nil)
				root = z;
			else if (c < 0)
				y.left = z;
			else
				y.right = z;

			Insert_Fixup(z);
			nodeCount++;

			return (z, true);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the node with the given key, or the sentinel if no such node
		/// exists
		/// </summary>
		/********************************************************************/
		private Node Find_Node(Key key)
		{
			Node x = root;

			while (x != nil)
			{
				int c = Cmp(key, x.value.first);

				if (c == 0)
					return x;

				x = c < 0 ? x.left : x.right;
			}

			return nil;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the first node whose key is not less than the given key
		/// (upper == false) or greater than the given key (upper == true), or
		/// the sentinel if no such node exists
		/// </summary>
		/********************************************************************/
		private Node Bound_Node(Key key, bool upper)
		{
			Node x = root;
			Node result = nil;

			while (x != nil)
			{
				int c = Cmp(x.value.first, key);
				bool goLeft = upper ? c > 0 : c >= 0;

				if (goLeft)
				{
					result = x;
					x = x.left;
				}
				else
					x = x.right;
			}

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the leftmost (smallest) node of the subtree rooted at the
		/// given node
		/// </summary>
		/********************************************************************/
		private Node Minimum(Node x)
		{
			if (x == nil)
				return nil;

			while (x.left != nil)
				x = x.left;

			return x;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the rightmost (largest) node of the subtree rooted at the
		/// given node
		/// </summary>
		/********************************************************************/
		private Node Maximum(Node x)
		{
			if (x == nil)
				return nil;

			while (x.right != nil)
				x = x.right;

			return x;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the in-order successor of the given node, or the sentinel
		/// if the node is the last one
		/// </summary>
		/********************************************************************/
		private Node Successor(Node x)
		{
			if (x == nil)
				return nil;

			if (x.right != nil)
				return Minimum(x.right);

			Node y = x.parent;

			while ((y != nil) && (x == y.right))
			{
				x = y;
				y = y.parent;
			}

			return y;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the in-order predecessor of a node. The predecessor
		/// of the sentinel (the end iterator) is the largest node in the tree
		/// </summary>
		/********************************************************************/
		private Node Predecessor(Node x)
		{
			if (x == nil)
				return Maximum(root);

			if (x.left != nil)
				return Maximum(x.left);

			Node y = x.parent;

			while ((y != nil) && (x == y.left))
			{
				x = y;
				y = y.parent;
			}

			return y;
		}



		/********************************************************************/
		/// <summary>
		/// Rotates the subtree rooted at the given node to the left
		/// </summary>
		/********************************************************************/
		private void Left_Rotate(Node x)
		{
			Node y = x.right;

			x.right = y.left;

			if (y.left != nil)
				y.left.parent = x;

			y.parent = x.parent;

			if (x.parent == nil)
				root = y;
			else if (x == x.parent.left)
				x.parent.left = y;
			else
				x.parent.right = y;

			y.left = x;
			x.parent = y;
		}



		/********************************************************************/
		/// <summary>
		/// Rotates the subtree rooted at the given node to the right
		/// </summary>
		/********************************************************************/
		private void Right_Rotate(Node x)
		{
			Node y = x.left;

			x.left = y.right;

			if (y.right != nil)
				y.right.parent = x;

			y.parent = x.parent;

			if (x.parent == nil)
				root = y;
			else if (x == x.parent.right)
				x.parent.right = y;
			else
				x.parent.left = y;

			y.right = x;
			x.parent = y;
		}



		/********************************************************************/
		/// <summary>
		/// Restores the red-black properties after a node has been inserted
		/// </summary>
		/********************************************************************/
		private void Insert_Fixup(Node z)
		{
			while (z.parent.red)
			{
				if (z.parent == z.parent.parent.left)
				{
					Node y = z.parent.parent.right;

					if (y.red)
					{
						z.parent.red = false;
						y.red = false;
						z.parent.parent.red = true;
						z = z.parent.parent;
					}
					else
					{
						if (z == z.parent.right)
						{
							z = z.parent;
							Left_Rotate(z);
						}

						z.parent.red = false;
						z.parent.parent.red = true;
						Right_Rotate(z.parent.parent);
					}
				}
				else
				{
					Node y = z.parent.parent.left;

					if (y.red)
					{
						z.parent.red = false;
						y.red = false;
						z.parent.parent.red = true;
						z = z.parent.parent;
					}
					else
					{
						if (z == z.parent.left)
						{
							z = z.parent;
							Right_Rotate(z);
						}

						z.parent.red = false;
						z.parent.parent.red = true;
						Left_Rotate(z.parent.parent);
					}
				}
			}

			root.red = false;
		}



		/********************************************************************/
		/// <summary>
		/// Replaces the subtree rooted at u with the subtree rooted at v,
		/// updating the parent links
		/// </summary>
		/********************************************************************/
		private void Transplant(Node u, Node v)
		{
			if (u.parent == nil)
				root = v;
			else if (u == u.parent.left)
				u.parent.left = v;
			else
				u.parent.right = v;

			v.parent = u.parent;
		}



		/********************************************************************/
		/// <summary>
		/// Removes the given node from the tree, rebalancing as needed
		/// </summary>
		/********************************************************************/
		private void Delete_Node(Node z)
		{
			Node y = z;
			bool yWasBlack = !y.red;
			Node x;

			if (z.left == nil)
			{
				x = z.right;
				Transplant(z, z.right);
			}
			else if (z.right == nil)
			{
				x = z.left;
				Transplant(z, z.left);
			}
			else
			{
				y = Minimum(z.right);
				yWasBlack = !y.red;
				x = y.right;

				if (y.parent == z)
					x.parent = y;
				else
				{
					Transplant(y, y.right);
					y.right = z.right;
					y.right.parent = y;
				}

				Transplant(z, y);
				y.left = z.left;
				y.left.parent = y;
				y.red = z.red;
			}

			if (yWasBlack)
				Delete_Fixup(x);

			nodeCount--;
		}



		/********************************************************************/
		/// <summary>
		/// Restores the red-black properties after a node has been removed
		/// </summary>
		/********************************************************************/
		private void Delete_Fixup(Node x)
		{
			while ((x != root) && !x.red)
			{
				if (x == x.parent.left)
				{
					Node w = x.parent.right;

					if (w.red)
					{
						w.red = false;
						x.parent.red = true;
						Left_Rotate(x.parent);
						w = x.parent.right;
					}

					if (!w.left.red && !w.right.red)
					{
						w.red = true;
						x = x.parent;
					}
					else
					{
						if (!w.right.red)
						{
							w.left.red = false;
							w.red = true;
							Right_Rotate(w);
							w = x.parent.right;
						}

						w.red = x.parent.red;
						x.parent.red = false;
						w.right.red = false;
						Left_Rotate(x.parent);
						x = root;
					}
				}
				else
				{
					Node w = x.parent.left;

					if (w.red)
					{
						w.red = false;
						x.parent.red = true;
						Right_Rotate(x.parent);
						w = x.parent.left;
					}

					if (!w.right.red && !w.left.red)
					{
						w.red = true;
						x = x.parent;
					}
					else
					{
						if (!w.left.red)
						{
							w.right.red = false;
							w.red = true;
							Left_Rotate(w);
							w = x.parent.left;
						}

						w.red = x.parent.red;
						x.parent.red = false;
						w.left.red = false;
						Right_Rotate(x.parent);
						x = root;
					}
				}
			}

			x.red = false;
		}



		/********************************************************************/
		/// <summary>
		/// Compares the contents of the two containers lexicographically,
		/// returning a negative value, zero or a positive value
		/// </summary>
		/********************************************************************/
		private static int Compare(map<Key, T> left, map<Key, T> right)
		{
			if (ReferenceEquals(left, right))
				return 0;

			if (left is null)
				return -1;

			if (right is null)
				return 1;

			Comparer<Key> keyComparer = Comparer<Key>.Default;
			Comparer<T> valueComparer = Comparer<T>.Default;

			Node a = left.Minimum(left.root);
			Node b = right.Minimum(right.root);

			while ((a != left.nil) && (b != right.nil))
			{
				int c = keyComparer.Compare(a.value.first, b.value.first);
				if (c != 0)
					return c;

				c = valueComparer.Compare(a.value.second, b.value.second);
				if (c != 0)
					return c;

				a = left.Successor(a);
				b = right.Successor(b);
			}

			return left.nodeCount.CompareTo(right.nodeCount);
		}
		#endregion

		/// <summary>
		/// A bidirectional iterator over the elements of a map‹Key, T›
		/// (C++ map::iterator). It refers to a single element and stays valid
		/// as long as that element is not erased. Use the ++ and -- operators
		/// to move to the next or previous item, and the <see cref="first"/>
		/// and <see cref="second"/> properties to access the key and the
		/// mapped value of the referred element
		/// </summary>
#pragma warning disable CS8981
		public struct iterator : IEquatable<iterator>
		{
#pragma warning restore CS8981
			private readonly map<Key, T> owner;
			private Node node;

			/****************************************************************/
			/// <summary>
			/// Constructs an iterator that refers to the given node of the
			/// given container
			/// </summary>
			/****************************************************************/
			internal iterator(map<Key, T> owner, Node node)
			{
				this.owner = owner;
				this.node = node;
			}



			/****************************************************************/
			/// <summary>
			/// Returns the node the iterator refers to, for the use of the
			/// enclosing container
			/// </summary>
			/****************************************************************/
			internal Node Current_Node => node;



			/****************************************************************/
			/// <summary>
			/// Returns the key of the referred element (C++ iterator-›first).
			/// Reading this on the end iterator is undefined
			/// </summary>
			/****************************************************************/
			public Key first => node.value.first;



			/****************************************************************/
			/// <summary>
			/// Gets or sets the mapped value of the referred element
			/// (C++ iterator-›second). Accessing this on the end iterator is
			/// undefined
			/// </summary>
			/****************************************************************/
			public T second
			{
				get => node.value.second;
				set => node.value.second = value;
			}



			/****************************************************************/
			/// <summary>
			/// Advances the iterator to the next element (C++ operator++)
			/// </summary>
			/****************************************************************/
			public static iterator operator ++(iterator it)
			{
				return new iterator(it.owner, it.owner.Successor(it.node));
			}



			/****************************************************************/
			/// <summary>
			/// Moves the iterator to the previous element (C++ operator--)
			/// </summary>
			/****************************************************************/
			public static iterator operator --(iterator it)
			{
				return new iterator(it.owner, it.owner.Predecessor(it.node));
			}



			/****************************************************************/
			/// <summary>
			/// Returns an iterator to the next element (C++ std::next).
			/// Unlike the ++ operator, this can be applied to a value that is
			/// not a variable, such as the result of a method call
			/// </summary>
			/****************************************************************/
			public iterator Next()
			{
				return new iterator(owner, owner.Successor(node));
			}



			/****************************************************************/
			/// <summary>
			/// Returns an iterator to the previous element (C++ std::prev).
			/// Unlike the -- operator, this can be applied to a value that is
			/// not a variable, such as the result of a method call
			/// </summary>
			/****************************************************************/
			public iterator Prev()
			{
				return new iterator(owner, owner.Predecessor(node));
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
			/// Checks if the given object is an iterator that refers to the
			/// same element as this one
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
