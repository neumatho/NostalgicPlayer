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
		/// Return the length of the buffer
		/// </summary>
		int Length { get; }

		/// <summary>
		/// Clear the pointer
		/// </summary>
		void SetToNull();

		/// <summary>
		/// Check to see if the pointer is null
		/// </summary>
		bool IsNull { get; }

		/// <summary>
		/// Check to see if the pointer is not null
		/// </summary>
		bool IsNotNull { get; }

		/// <summary>
		/// Cast a pointer from one type to another
		/// </summary>
		CPointer<TTo> Cast<TFrom, TTo>() where TFrom : unmanaged where TTo : unmanaged;

		/// <summary>
		/// Return a pointer from the interface
		/// </summary>
		CPointer<TTo> ToPointer<TTo>() where TTo : unmanaged;
	}
}
