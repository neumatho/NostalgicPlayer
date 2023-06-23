/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibMpg123.Containers
{
	/// <summary>
	/// Data structure for ID3v1 tags (the last 128 bytes of a file).
	/// Don't take anything for granted (like string termination)!
	/// Also note the change ID3v1.1 did: comment[28] = 0; comment[29] = track_number
	/// It is your task to support ID3v1 only or ID3v1.1 ...
	/// </summary>
	public class Mpg123_Id3V1
	{
		/// <summary>
		/// Always the string "TAG", the classic intro
		/// </summary>
		public c_uchar[] Tag = new c_uchar[3];
		/// <summary>
		/// Title string
		/// </summary>
		public c_uchar[] Title = new c_uchar[30];
		/// <summary>
		/// Artist string
		/// </summary>
		public c_uchar[] Artist = new c_uchar[30];
		/// <summary>
		/// Album string
		/// </summary>
		public c_uchar[] Album = new c_uchar[30];
		/// <summary>
		/// Year string
		/// </summary>
		public c_uchar[] Year = new c_uchar[4];
		/// <summary>
		/// Comment string
		/// </summary>
		public c_uchar[] Comment = new c_uchar[30];
		/// <summary>
		/// Genre index
		/// </summary>
		public c_uchar Genre;
	}
}
