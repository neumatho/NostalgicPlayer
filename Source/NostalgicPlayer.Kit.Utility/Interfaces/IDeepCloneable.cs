/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Utility.Interfaces
{
	/// <summary>
	/// If you want to implement a deep clone method, you can derive from this interface
	/// </summary>
	public interface IDeepCloneable<T>
	{
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		T MakeDeepClone();
	}
}
