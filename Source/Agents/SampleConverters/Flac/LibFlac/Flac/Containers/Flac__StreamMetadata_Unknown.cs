/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac.Containers
{
	/// <summary>
	/// Structure that is used when a metadata block of unknown type is loaded.
	/// The contents are opaque. The structure is used only internally to
	/// correctly handle unknown metadata
	/// </summary>
	internal class Flac__StreamMetadata_Unknown : IMetadata
	{
		public Flac__byte[] Data;
	}
}
