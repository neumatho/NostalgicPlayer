/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibMpg123.Containers
{
	/// <summary>
	/// Data structure for storing IDV3v2 tags.
	/// This structure is not a direct binary mapping with the file contents.
	/// The ID3v2 text frames are allowed to contain multiple strings.
	/// So check for null bytes until you reach the mpg123_string fill.
	/// All text is encoded in UTF-8
	/// </summary>
	public class Mpg123_Id3V2
	{
		/// <summary>
		/// 3 or 4 for ID3v2.3 or ID3v2.4
		/// </summary>
		public c_uchar Version;
		/// <summary>
		/// Title string (pointer into text_list)
		/// </summary>
		public Mpg123_String Title;
		/// <summary>
		/// Artist string (pointer into text_list)
		/// </summary>
		public Mpg123_String Artist;
		/// <summary>
		/// Album string (pointer into text_list)
		/// </summary>
		public Mpg123_String Album;
		/// <summary>
		/// The year as a string (pointer into text_list)
		/// </summary>
		public Mpg123_String Year;
		/// <summary>
		/// Genre string (pointer into text_list). The genre string(s) may very well need postprocessing, esp. for ID3v2.3
		/// </summary>
		public Mpg123_String Genre;
		/// <summary>
		/// Pointer to last encountered comment text with empty description
		/// </summary>
		public Mpg123_String Comment;

		// Encountered ID3v2 fields are appended to these lists.
		// There can be multiple occurences, the pointers above always point to the
		// last encountered data

		/// <summary>
		/// Array of comments
		/// </summary>
		public Mpg123_Text[] Comment_List;
		/// <summary>
		/// Number of comments
		/// </summary>
		public size_t Comments;
		/// <summary>
		/// Array of ID3v2 text fields (including USLT)
		/// </summary>
		public Mpg123_Text[] Text;
		/// <summary>
		/// Number of text fields
		/// </summary>
		public size_t Texts;
		/// <summary>
		/// The array of extra (Txxx) fields
		/// </summary>
		public Mpg123_Text[] Extra;
		/// <summary>
		/// Number of extra text (Txxx) fields
		/// </summary>
		public size_t Extras;
		/// <summary>
		/// Array of ID3v2 pictures field (APIC). Only populated if MPG123_PICTURE flag is set!
		/// </summary>
		public Mpg123_Picture[] Picture;
		/// <summary>
		/// Number of picture (APIC) fields
		/// </summary>
		public size_t Pictures;
	}
}
