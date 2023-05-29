/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123.Containers
{
	/// <summary>
	/// Data structure for storing IDV3v2 tags.
	/// This structure is not a direct binary mapping with the file contents.
	/// The ID3v2 text frames are allowed to contain multiple strings.
	/// So check for null bytes until you reach the mpg123_string fill.
	/// All text is encoded in UTF-8
	/// </summary>
	internal class Mpg123_Id3V2
	{
		public c_uchar Version;			// < 3 or 4 for ID3v2.3 or ID3v2.4
		public Mpg123_String Title;		// < Title string (pointer into text_list)
		public Mpg123_String Artist;	// < Artist string (pointer into text_list)
		public Mpg123_String Album;		// < Album string (pointer into text_list)
		public Mpg123_String Year;		// < The year as a string (pointer into text_list)
		public Mpg123_String Genre;		// < Genre string (pointer into text_list). The genre string(s) may very well need postprocessing, esp. for ID3v2.3
		public Mpg123_String Comment;	// < Pointer to last encountered comment text with empty description

		// Encountered ID3v2 fields are appended to these lists.
		// There can be multiple occurences, the pointers above always point to the
		// last encountered data
		public Mpg123_Text[] Comment_List;	// < Array of comments
		public size_t Comments;			// < Number of comments
		public Mpg123_Text[] Text;		// < Array of ID3v2 text fields (including USLT)
		public size_t Texts;			// < Number of text fields
		public Mpg123_Text[] Extra;		// < The array of extra (Txxx) fields
		public size_t Extras;			// < Number of extra text (Txxx) fields
		public Mpg123_Picture[] Picture;	// < Array of ID3v2 pictures field (APIC). Only populated if MPG123_PICTURE flag is set!
		public size_t Pictures;			// < Number of picture (APIC) fields
	}
}
