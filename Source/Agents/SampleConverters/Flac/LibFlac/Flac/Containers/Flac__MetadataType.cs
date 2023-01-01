/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac.Containers
{
	/// <summary>
	/// An enumeration of the available metadata block types
	/// </summary>
	internal enum Flac__MetadataType
	{
		/// <summary>
		/// STREAMINFO block
		/// </summary>
		StreamInfo = 0,

		/// <summary>
		/// PADDING block
		/// </summary>
		Padding = 1,

		/// <summary>
		/// APPLICATION block
		/// </summary>
		Application = 2,

		/// <summary>
		/// SEEKTABLE block
		/// </summary>
		SeekTable = 3,

		/// <summary>
		/// VORBISCOMMENT block (a.k.a. FLAC tags)
		/// </summary>
		Vorbis_Comment = 4,

		/// <summary>
		/// CUESHEET block
		/// </summary>
		CueSheet = 5,

		/// <summary>
		/// PICTURE block
		/// </summary>
		Picture = 6,

		/// <summary>
		/// Marker to denote beginning of undefined type range; this number will increase as new metadata types are added
		/// </summary>
		Undefined = 7,

		/// <summary>
		/// No type will ever be greater than this. There is not enough room in the protocol block
		/// </summary>
		Max_Metadata_Type = Constants.Flac__Max_Metadata_Type_Code
	}
}
