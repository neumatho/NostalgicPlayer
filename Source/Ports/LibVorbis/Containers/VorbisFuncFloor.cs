/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.CKit;
using Polycode.NostalgicPlayer.Ports.LibOgg;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class VorbisFuncFloor
	{
		public delegate IVorbisInfoFloor Unpack_Del(VorbisInfo vi, OggPack opb);
		public delegate IVorbisLookFloor Look_Del(VorbisDspState vd, IVorbisInfoFloor i);
		public delegate void Free_Info_Del(IVorbisInfoFloor i);
		public delegate void Free_Look_Del(IVorbisLookFloor i);
		public delegate CPointer<byte> Inverse1_Del(VorbisBlock vb, IVorbisLookFloor i);
		public delegate c_int Inverse2_Del(VorbisBlock vb, IVorbisLookFloor i, CPointer<byte> memo, CPointer<c_float> @out);

		public required Unpack_Del Unpack;
		public required Look_Del Look;
		public required Free_Info_Del Free_Info;
		public required Free_Look_Del Free_Look;
		public required Inverse1_Del Inverse1;
		public required Inverse2_Del Inverse2;
	}
}
