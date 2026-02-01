/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class DecodedFrames : ICopyTo<DecodedFrames>
	{
		/// <summary>
		/// 
		/// </summary>
		public CPointer<AvFrame> F;

		/// <summary>
		/// 
		/// </summary>
		public size_t Nb_F;

		/// <summary>
		/// 
		/// </summary>
		public size_t Nb_F_Allocated;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void CopyTo(DecodedFrames destination)
		{
			destination.F = F;
			destination.Nb_F = Nb_F;
			destination.Nb_F_Allocated = Nb_F_Allocated;
		}
	}
}
