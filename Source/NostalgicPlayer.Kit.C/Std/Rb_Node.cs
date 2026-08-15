/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.C.Std
{
	/// <summary>
	/// A single node of an <see cref="Rb_Tree{TKey,TValue}"/>, holding one
	/// element of the container that the tree implements.
	///
	/// A node is only ever made by the tree itself, and the tree keeps it
	/// alive until the element is erased, so an iterator of the container
	/// can hold on to it and stay valid for as long as C++ says that it
	/// does
	/// </summary>
	internal sealed class Rb_Node<TValue>
	{
		public Rb_Node<TValue> parent;
		public Rb_Node<TValue> left;
		public Rb_Node<TValue> right;
		public bool red;
		public TValue value;
	}
}
