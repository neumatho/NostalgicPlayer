/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibMpg123.Containers
{
	/// <summary>
	/// Sub data structure for ID3v2, for storing picture data including comment.
	/// This is for the ID3v2 APIC field. You should consult the ID3v2 specification
	/// for the use of the APIC field ("frames" in ID3v2 documentation, I use "fields"
	/// here to separate from MPEG frames)
	/// </summary>
	public class Mpg123_Picture
	{
		/// <summary>
		/// Pic type value
		/// </summary>
		public Mpg123_Id3_Pic_Type Type;
		/// <summary>
		/// Description string
		/// </summary>
		public Mpg123_String Description = new Mpg123_String();
		/// <summary>
		/// MIME type
		/// </summary>
		public Mpg123_String Mime_Type = new Mpg123_String();
		/// <summary>
		/// Size in bytes
		/// </summary>
		public size_t Size;
		/// <summary>
		/// Pointer to the image data
		/// </summary>
		public c_uchar[] Data;
	}
}
