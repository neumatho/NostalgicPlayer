/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibOgg;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class VorbisBlockInternal : IVorbisBlockInternal
	{
		public c_float ampmax;

		/// <summary>
		/// Initialized, must be freed;
		/// blob[PACKETBLOBS/2] points to the oggpack_buffer in the main vorbis_block
		/// </summary>
		public readonly OggPack[] packetblob = new OggPack[Constants.PacketBlobs];
	}
}
