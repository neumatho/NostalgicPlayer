/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac.Containers
{
	/// <summary>
	/// FLAC APPLICATION structure
	/// </summary>
	internal class Flac__StreamMetadata_Application : IMetadata
	{
		public Flac__byte[] Id = new Flac__byte[4];
		public Flac__byte[] Data;
	}
}
