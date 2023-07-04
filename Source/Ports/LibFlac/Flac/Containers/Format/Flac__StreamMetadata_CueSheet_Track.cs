/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format
{
	/// <summary>
	/// FLAC CUESHEET track structure
	/// </summary>
	public class Flac__StreamMetadata_CueSheet_Track
	{
		/// <summary>
		/// Track offset in samples, relative to the beginning of the FLAC audio stream
		/// </summary>
		public Flac__uint64 Offset;

		/// <summary>
		/// The track number
		/// </summary>
		public Flac__byte Number;

		/// <summary>
		/// Track ISRC. This is a 12-digit alphanumeric code plus a trailing NUL byte
		/// </summary>
		public byte[] Isrc = new byte[13];

		/// <summary>
		/// The track type: 0 for audio, 1 for non-audio
		/// </summary>
		public uint32_t Type;

		/// <summary>
		/// The pre-emphasis flag: 0 for no pre-emphasis, 1 for pre-emphasis
		/// </summary>
		public uint32_t Pre_Emphasis;

		/// <summary>
		/// The number of track index points
		/// </summary>
		public Flac__byte Num_Indices;

		/// <summary>
		/// Null if Num_Indices == 0, else pointer to array of index points
		/// </summary>
		public Flac__StreamMetadata_CueSheet_Index[] Indices;
	}
}
