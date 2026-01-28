/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.C
{
	/// <summary>
	/// Interface to all pointers
	/// </summary>
	public interface IPointer
	{
		/// <summary>
		/// Case a pointer from one type to another
		/// </summary>
		CPointer<TTo> Cast<TFrom, TTo>() where TFrom : unmanaged where TTo : unmanaged;
	}
}
