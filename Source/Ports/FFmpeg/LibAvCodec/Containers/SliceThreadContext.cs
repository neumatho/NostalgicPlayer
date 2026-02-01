/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Interfaces;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class SliceThreadContext : IContext
	{
		/// <summary>
		/// 
		/// </summary>
		public AvSliceThread Thread;

		/// <summary>
		/// 
		/// </summary>
		public CodecFunc.Execute_Func_Delegate Func;

		/// <summary>
		/// 
		/// </summary>
		public CodecFunc.Execute2_Func_Delegate Func2;

		/// <summary>
		/// 
		/// </summary>
		public readonly CodecFunc.Main_Func_Delegate MainFunc = null;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<IExecuteArg> Args;//XX

		/// <summary>
		/// 
		/// </summary>
		public CPointer<c_int> Rets;
	}
}
