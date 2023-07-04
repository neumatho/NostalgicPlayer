/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format
{
	/// <summary>
	/// FLAC APPLICATION structure
	/// </summary>
	public class Flac__StreamMetadata_Application : IMetadata
	{
		/// <summary></summary>
		public Flac__byte[] Id = new Flac__byte[4];
		/// <summary></summary>
		public Flac__byte[] Data;
	}
}
