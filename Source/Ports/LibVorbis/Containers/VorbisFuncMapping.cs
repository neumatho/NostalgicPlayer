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
	internal class VorbisFuncMapping
	{
		public delegate IVorbisInfoMapping Unpack_Del(VorbisInfo vi, OggPack opb);
		public delegate void Free_Info_Del(IVorbisInfoMapping i);
		public delegate c_int Inverse_Del(VorbisBlock vb, IVorbisInfoMapping l);

		public required Unpack_Del Unpack;
		public required Free_Info_Del Free_Info;
		public required Inverse_Del Inverse;
	}
}
