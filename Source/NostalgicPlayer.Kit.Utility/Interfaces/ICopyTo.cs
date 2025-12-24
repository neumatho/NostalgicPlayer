/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Utility.Interfaces
{
	/// <summary>
	/// If you support to copy all the data from one instance to another,
	/// implement this interface
	/// </summary>
	public interface ICopyTo<T>
	{
		/// <summary>
		/// Make a deep copy of the current object into another object
		/// </summary>
		void CopyTo(T destination);
	}
}
