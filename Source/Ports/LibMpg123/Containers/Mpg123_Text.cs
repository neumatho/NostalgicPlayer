/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibMpg123.Containers
{
	/// <summary>
	/// Sub data structure for ID3v2, for storing various fields (including comments).
	/// This is for ID3v2 COMM, TXXX and all the other text fields.
	/// Only COMM, TXXX and USLT may have a description, only COMM and USLT
	/// have a language.
	/// You should consult the ID3v2 specification for the use of the various text fields
	/// ("frames" in ID3v2 documentation, I use "fields" here to separate from MPEG frames)
	/// </summary>
	public class Mpg123_Text
	{
		/// <summary>
		/// Three-letter language code (not terminated)
		/// </summary>
		public readonly c_uchar[] Lang = new c_uchar[3];
		/// <summary>
		/// The ID3v2 text field ID, like TALB, TPE2, ... (4 characters, no string termination)
		/// </summary>
		public readonly c_uchar[] Id = new c_uchar[4];
		/// <summary>
		/// Empty for the generic comment...
		/// </summary>
		public Mpg123_String Description = new Mpg123_String();
		/// <summary>
		/// ...
		/// </summary>
		public readonly Mpg123_String Text = new Mpg123_String();
	}
}
