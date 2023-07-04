/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format
{
	/// <summary>
	/// FLAC CUESHEET track index structure
	/// </summary>
	public class Flac__StreamMetadata_CueSheet_Index
	{
		/// <summary>
		/// Offset in samples, relative to the track offset, of the index
		/// point
		/// </summary>
		public Flac__uint64 Offset;

		/// <summary>
		/// The index point number
		/// </summary>
		public Flac__byte Number;
	}
}