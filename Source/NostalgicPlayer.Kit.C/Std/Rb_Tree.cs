/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;

namespace Polycode.NostalgicPlayer.Kit.C.Std
{
	/// <summary>
	/// The balanced (red-black) tree that the ordered associative
	/// containers are built on, holding elements of type TValue that are
	/// ordered by a key of type TKey.
	///
	/// This is the shared implementation behind <see cref="map{Key,T}"/>
	/// and <see cref="set{Key}"/>, in the same way that a C++ standard
	/// library implements its ordered associative containers on top of a
	/// single tree. Which part of an element is its key is decided by the
	/// container, which hands a key extractor to the constructor: a map
	/// stores a pair and takes the key of it, while a set stores the key
	/// itself.
	///
	/// The tree only handles the nodes and their ordering. Making the
	/// value that goes into a new node is left to the container, as only
	/// it knows how its elements are to be copied (see
	/// <see cref="Copy_Insert{T}"/>), which is why inserting is split into
	/// <see cref="Find_Insert_Position"/> and <see cref="Insert_Node"/>.
	///
	/// A null child is stood in for by a sentinel node, as in a classic
	/// red-black tree, and that same sentinel is the one past the last
	/// element position that the containers hand out as their end
	/// iterator
	/// </summary>
	internal sealed class Rb_Tree<TKey, TValue>
	{
		private readonly Func<TValue, TKey> keyOfValue;

		private IComparer<TKey> comparer;
		private Rb_Node<TValue> nil;
		private Rb_Node<TValue> root;
		private int nodeCount;

		/********************************************************************/
		/// <summary>
		/// Constructs an empty tree that orders its elements with the
		/// given comparison object, using the given method to find the key
		/// of an element
		/// </summary>
		/********************************************************************/
		public Rb_Tree(IComparer<TKey> comparer, Func<TValue, TKey> keyOfValue)
		{
			this.comparer = comparer ?? Comparer<TKey>.Default;
			this.keyOfValue = keyOfValue;

			Init_Empty();
		}



		/********************************************************************/
		/// <summary>
		/// Returns the sentinel node. It stands in for a null child, and
		/// is at the same time the one past the last element position
		/// </summary>
		/********************************************************************/
		public Rb_Node<TValue> Nil => nil;



		/********************************************************************/
		/// <summary>
		/// Returns the number of elements in the tree
		/// </summary>
		/********************************************************************/
		public int Count => nodeCount;



		/********************************************************************/
		/// <summary>
		/// Gets or sets the comparison object that orders the keys
		/// </summary>
		/********************************************************************/
		public IComparer<TKey> Comparer
		{
			get => comparer;
			set => comparer = value;
		}



		/********************************************************************/
		/// <summary>
		/// Removes all elements from the tree
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			root = nil;
			nodeCount = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Exchanges the contents and the key ordering of this tree with
		/// those of the other tree
		/// </summary>
		/********************************************************************/
		public void Swap(Rb_Tree<TKey, TValue> other)
		{
			(nil, other.nil) = (other.nil, nil);
			(root, other.root) = (other.root, root);
			(nodeCount, other.nodeCount) = (other.nodeCount, nodeCount);
			(comparer, other.comparer) = (other.comparer, comparer);
		}



		/********************************************************************/
		/// <summary>
		/// Takes the contents of the given tree over as they are, leaving
		/// that tree empty. The nodes are handed over and not copied, so
		/// everything that refers to a node keeps on referring to it. The
		/// key ordering of this tree is kept
		/// </summary>
		/********************************************************************/
		public void Take_From(Rb_Tree<TKey, TValue> source)
		{
			nil = source.nil;
			root = source.root;
			nodeCount = source.nodeCount;

			source.Init_Empty();
		}



		/********************************************************************/
		/// <summary>
		/// Returns the first node of the tree in ascending key order, or
		/// the sentinel if the tree is empty
		/// </summary>
		/********************************************************************/
		public Rb_Node<TValue> First_Node()
		{
			return Minimum(root);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the node that follows the given one in ascending key
		/// order, or the sentinel if the node is the last one
		/// </summary>
		/********************************************************************/
		public Rb_Node<TValue> Next_Node(Rb_Node<TValue> x)
		{
			if (x == nil)
				return nil;

			if (x.right != nil)
				return Minimum(x.right);

			Rb_Node<TValue> y = x.parent;

			while ((y != nil) && (x == y.right))
			{
				x = y;
				y = y.parent;
			}

			return y;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the node that comes before the given one in ascending
		/// key order. The node before the sentinel (the one past the last
		/// element position) is the last node of the tree
		/// </summary>
		/********************************************************************/
		public Rb_Node<TValue> Prev_Node(Rb_Node<TValue> x)
		{
			if (x == nil)
				return Maximum(root);

			if (x.left != nil)
				return Maximum(x.left);

			Rb_Node<TValue> y = x.parent;

			while ((y != nil) && (x == y.left))
			{
				x = y;
				y = y.parent;
			}

			return y;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the node with the given key, or the sentinel if no such
		/// node exists
		/// </summary>
		/********************************************************************/
		public Rb_Node<TValue> Find_Node(TKey key)
		{
			Rb_Node<TValue> x = root;

			while (x != nil)
			{
				int c = Cmp(key, keyOfValue(x.value));

				if (c == 0)
					return x;

				x = c < 0 ? x.left : x.right;
			}

			return nil;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the first node whose key is not less than the given key
		/// (upper == false) or greater than the given key (upper == true),
		/// or the sentinel if no such node exists
		/// </summary>
		/********************************************************************/
		public Rb_Node<TValue> Bound_Node(TKey key, bool upper)
		{
			Rb_Node<TValue> x = root;
			Rb_Node<TValue> result = nil;

			while (x != nil)
			{
				int c = Cmp(keyOfValue(x.value), key);
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
		/// Looks for the place where a node with the given key belongs.
		/// Returns the node that already holds an equivalent key, or the
		/// sentinel when there is none. Only in the latter case do the
		/// given parent and asLeftChild tell where a new node is to be
		/// hung, which is what <see cref="Insert_Node"/> takes
		/// </summary>
		/********************************************************************/
		public Rb_Node<TValue> Find_Insert_Position(TKey key, out Rb_Node<TValue> parent, out bool asLeftChild)
		{
			Rb_Node<TValue> y = nil;
			Rb_Node<TValue> x = root;
			int c = 0;

			while (x != nil)
			{
				y = x;
				c = Cmp(key, keyOfValue(x.value));

				if (c == 0)
				{
					parent = nil;
					asLeftChild = false;

					return x;
				}

				x = c < 0 ? x.left : x.right;
			}

			parent = y;
			asLeftChild = c < 0;

			return nil;
		}



		/********************************************************************/
		/// <summary>
		/// Hangs a new node holding the given value at the place that
		/// <see cref="Find_Insert_Position"/> found, rebalancing as
		/// needed. Returns the new node
		/// </summary>
		/********************************************************************/
		public Rb_Node<TValue> Insert_Node(Rb_Node<TValue> parent, bool asLeftChild, TValue value)
		{
			Rb_Node<TValue> z = new Rb_Node<TValue>
			{
				value = value,
				parent = parent,
				left = nil,
				right = nil,
				red = true
			};

			if (parent == nil)
				root = z;
			else if (asLeftChild)
				parent.left = z;
			else
				parent.right = z;

			Insert_Fixup(z);
			nodeCount++;

			return z;
		}



		/********************************************************************/
		/// <summary>
		/// Removes the given node from the tree, rebalancing as needed
		/// </summary>
		/********************************************************************/
		public void Erase_Node(Rb_Node<TValue> z)
		{
			Rb_Node<TValue> y = z;
			bool yWasBlack = !y.red;
			Rb_Node<TValue> x;

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

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Initializes the tree to an empty state, creating the sentinel
		/// node that stands in for a null child
		/// </summary>
		/********************************************************************/
		private void Init_Empty()
		{
			nil = new Rb_Node<TValue>();
			nil.red = false;
			nil.parent = nil;
			nil.left = nil;
			nil.right = nil;

			root = nil;
			nodeCount = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Compares the two keys, returning a negative value, zero or a
		/// positive value
		/// </summary>
		/********************************************************************/
		private int Cmp(TKey a, TKey b)
		{
			return comparer.Compare(a, b);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the leftmost (smallest) node of the subtree rooted at
		/// the given node
		/// </summary>
		/********************************************************************/
		private Rb_Node<TValue> Minimum(Rb_Node<TValue> x)
		{
			if (x == nil)
				return nil;

			while (x.left != nil)
				x = x.left;

			return x;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the rightmost (largest) node of the subtree rooted at
		/// the given node
		/// </summary>
		/********************************************************************/
		private Rb_Node<TValue> Maximum(Rb_Node<TValue> x)
		{
			if (x == nil)
				return nil;

			while (x.right != nil)
				x = x.right;

			return x;
		}



		/********************************************************************/
		/// <summary>
		/// Rotates the subtree rooted at the given node to the left
		/// </summary>
		/********************************************************************/
		private void Left_Rotate(Rb_Node<TValue> x)
		{
			Rb_Node<TValue> y = x.right;

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
		private void Right_Rotate(Rb_Node<TValue> x)
		{
			Rb_Node<TValue> y = x.left;

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
		/// Restores the red-black properties after a node has been
		/// inserted
		/// </summary>
		/********************************************************************/
		private void Insert_Fixup(Rb_Node<TValue> z)
		{
			while (z.parent.red)
			{
				if (z.parent == z.parent.parent.left)
				{
					Rb_Node<TValue> y = z.parent.parent.right;

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
					Rb_Node<TValue> y = z.parent.parent.left;

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
		private void Transplant(Rb_Node<TValue> u, Rb_Node<TValue> v)
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
		/// Restores the red-black properties after a node has been removed
		/// </summary>
		/********************************************************************/
		private void Delete_Fixup(Rb_Node<TValue> x)
		{
			while ((x != root) && !x.red)
			{
				if (x == x.parent.left)
				{
					Rb_Node<TValue> w = x.parent.right;

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
					Rb_Node<TValue> w = x.parent.left;

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
		#endregion
	}
}
