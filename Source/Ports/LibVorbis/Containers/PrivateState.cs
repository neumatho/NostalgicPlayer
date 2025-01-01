/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibVorbis.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class PrivateState : IBackendState
	{
		public readonly c_int[] window = new c_int[2];
		public readonly IVorbisLookTransform[][] transform = new IVorbisLookTransform[2][];
		public c_int modebits;
		public IVorbisLookFloor[] flr;
		public IVorbisLookResidue[] residue;

		public ogg_int64_t sample_count;
	}
}
