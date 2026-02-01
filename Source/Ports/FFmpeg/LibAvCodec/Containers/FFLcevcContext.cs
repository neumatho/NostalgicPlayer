/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
global using Lcevc_DecodeHandler = Polycode.NostalgicPlayer.Kit.C.CPointer<System.UInt32>;

using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class FFLcevcContext : RefCount, IContext
	{
		/// <summary>
		/// 
		/// </summary>
		public Lcevc_DecodeHandler Decoder;

		/// <summary>
		/// 
		/// </summary>
		public c_int Initialized;
	}
}
