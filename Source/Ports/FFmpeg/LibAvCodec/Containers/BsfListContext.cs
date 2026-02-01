/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class BsfListContext : AvClass, IPrivateData
	{
		/// <summary>
		/// 
		/// </summary>
		public AvClass Class => this;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<AvBsfContext> Bsfs;

		/// <summary>
		/// 
		/// </summary>
		public c_int Nb_Bsfs;

		/// <summary>
		/// Index of currently processed BSF
		/// </summary>
		public c_uint Idx;

		/// <summary>
		/// 
		/// </summary>
		public new CPointer<char> Item_Name;
	}
}
