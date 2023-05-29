/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123.Containers
{
	/// <summary>
	/// Data structure for storing strings in a safer way than a standard C-String.
	/// Can also hold a number of null-terminated strings
	/// </summary>
	internal class Mpg123_String
	{
		public c_uchar[] P;				// < Pointer to the string data
		public size_t Size;				// < Raw number of bytes allocated
		public size_t Fill;				// < Number of used bytes (including closing zero byte)
	}
}
