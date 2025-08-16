/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOgg;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class VorbisFuncResidue
	{
		public delegate IVorbisInfoResidue Unpack_Del(VorbisInfo vi, OggPack opb);
		public delegate IVorbisLookResidue Look_Del(VorbisDspState vd, IVorbisInfoResidue i);
		public delegate void Free_Info_Del(IVorbisInfoResidue i);
		public delegate void Free_Look_Del(IVorbisLookResidue i);
		public delegate c_int Inverse_Del(VorbisBlock vb, IVorbisLookResidue vl, CPointer<c_float>[] @in, CPointer<bool> nonzero, c_int ch);

		public required Unpack_Del Unpack;
		public required Look_Del Look;
		public required Free_Info_Del Free_Info;
		public required Free_Look_Del Free_Look;
		public required Inverse_Del Inverse;
	}
}
