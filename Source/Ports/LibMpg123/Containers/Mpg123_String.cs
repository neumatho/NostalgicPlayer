/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibMpg123.Containers
{
	/// <summary>
	/// Data structure for storing strings in a safer way than a standard C-String.
	/// Can also hold a number of null-terminated strings
	/// </summary>
	public class Mpg123_String
	{
		/// <summary>
		/// Pointer to the string data
		/// </summary>
		public c_uchar[] P;
		/// <summary>
		/// Raw number of bytes allocated
		/// </summary>
		public size_t Size;
		/// <summary>
		/// Number of used bytes (including closing zero byte)
		/// </summary>
		public size_t Fill;
	}
}
