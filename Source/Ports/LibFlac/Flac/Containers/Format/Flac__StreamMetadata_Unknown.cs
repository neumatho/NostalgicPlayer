/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format
{
	/// <summary>
	/// Structure that is used when a metadata block of unknown type is loaded.
	/// The contents are opaque. The structure is used only internally to
	/// correctly handle unknown metadata
	/// </summary>
	public class Flac__StreamMetadata_Unknown : IMetadata
	{
		/// <summary></summary>
		public Flac__byte[] Data;
	}
}
