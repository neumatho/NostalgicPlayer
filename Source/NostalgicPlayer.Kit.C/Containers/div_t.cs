/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.C.Containers
{
#pragma warning disable CS8981
	/// <summary>
	/// Structure returned by div(), holding the quotient and
	/// remainder of an integral division
	/// </summary>
	public struct div_t
	{
#pragma warning restore CS8981
		/// <summary>
		/// The quotient
		/// </summary>
		public c_int quot;

		/// <summary>
		/// The remainder
		/// </summary>
		public c_int rem;
	}
}
