/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Utility.Interfaces
{
	/// <summary>
	/// If you support handing all the data of one instance over to a new
	/// instance, implement this interface
	/// </summary>
	public interface IMoveable<T>
	{
		/// <summary>
		/// Move the data of the current object into a new object, leaving
		/// the current one empty
		/// </summary>
		T MoveFrom();
	}
}
