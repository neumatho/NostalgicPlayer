/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123.Containers
{
	/// <summary>
	/// Data structure for ID3v1 tags (the last 128 bytes of a file).
	/// Don't take anything for granted (like string termination)!
	/// Also note the change ID3v1.1 did: comment[28] = 0; comment[29] = track_number
	/// It is your task to support ID3v1 only or ID3v1.1 ...
	/// </summary>
	internal class Mpg123_Id3V1
	{
		public c_uchar[] Tag = new c_uchar[3];	// < Always the string "TAG", the classic intro
		public c_uchar[] Title = new c_uchar[30];	// < Title string
		public c_uchar[] Artist = new c_uchar[30];// < Artist string
		public c_uchar[] Album = new c_uchar[30];	// < Album string
		public c_uchar[] Year = new c_uchar[4];	// < Year string
		public c_uchar[] Comment = new c_uchar[30];// < Comment string
		public c_uchar Genre;					// < Genre index
	}
}
