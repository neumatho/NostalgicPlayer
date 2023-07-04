/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format
{
	/// <summary>
	/// An enumeration of the PICTURE types
	/// </summary>
	public enum Flac__StreamMetadata_Picture_Type
	{
		/// <summary>
		/// Other
		/// </summary>
		Other = 0,

		/// <summary>
		/// 32x32 pixels 'file icon' (PNG only)
		/// </summary>
		File_Icon_Standard = 1,

		/// <summary>
		/// Other file icon
		/// </summary>
		File_Icon = 2,

		/// <summary>
		/// Cover (front)
		/// </summary>
		Front_Cover = 3,

		/// <summary>
		/// Cover (back)
		/// </summary>
		Back_Cover = 4,

		/// <summary>
		/// Leaflet page
		/// </summary>
		Leaflet_Page = 5,

		/// <summary>
		/// Media (e.g. label side of CD)
		/// </summary>
		Media = 6,

		/// <summary>
		/// Lead artist/lead performer/soloist
		/// </summary>
		Lead_Artist = 7,

		/// <summary>
		/// Artist/performer
		/// </summary>
		Artist = 8,

		/// <summary>
		/// Conductor
		/// </summary>
		Conductor = 9,

		/// <summary>
		/// Band/orchestra
		/// </summary>
		Band = 10,

		/// <summary>
		/// Composer
		/// </summary>
		Composer = 11,

		/// <summary>
		/// Lyricist/text writer
		/// </summary>
		Lyricist = 12,

		/// <summary>
		/// Recoding location
		/// </summary>
		Recording_Location = 13,

		/// <summary>
		/// During recoding
		/// </summary>
		During_Recoding = 14,

		/// <summary>
		/// During performance
		/// </summary>
		During_Performance = 15,

		/// <summary>
		/// Movie/video screen capture
		/// </summary>
		Video_Screen_Capture = 16,

		/// <summary>
		/// A bright coloured fish
		/// </summary>
		Fish = 17,

		/// <summary>
		/// Illustration
		/// </summary>
		Illustration = 18,

		/// <summary>
		/// Band/artist logotype
		/// </summary>
		Band_LogoType = 19,

		/// <summary>
		/// Publisher/studio logotype
		/// </summary>
		Publisher_LogoType = 20,

		/// <summary></summary>
		Undefined
	}
}
