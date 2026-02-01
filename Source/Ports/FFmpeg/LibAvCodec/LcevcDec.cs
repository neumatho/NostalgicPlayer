/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// 
	/// </summary>
	public static class LcevcDec
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int FF_Lcevc_Process(IClass logCtx, AvFrame frame)//XX 281
		{
			FrameDecodeData fdd = (FrameDecodeData)frame.Private_Refs;
			FFLcevcFrame frame_Ctx = (FFLcevcFrame)fdd.Post_Process_Opaque;
			FFLcevcContext lcevc = frame_Ctx.Lcevc;

			if (lcevc.Initialized == 0)
			{
				c_int ret = Lcevc_Init(lcevc, logCtx);

				if (ret < 0)
					return ret;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int FF_Lcevc_Alloc(out FFLcevcContext pLcevc)//XX 312
		{
			FFLcevcContext lcevc = null;

			pLcevc = lcevc;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void FF_Lcevc_Unref(IOpaque opaque)//XX 324
		{
			FFLcevcFrame lcevc = (FFLcevcFrame)opaque;

			RefStruct.Av_RefStruct_Unref(ref lcevc.Lcevc);
			Frame.Av_Frame_Free(ref lcevc.Frame);
			Mem.Av_Free(opaque);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Lcevc_Init(FFLcevcContext lcevc, IClass logCtx)//XX 254
		{
			lcevc.Initialized = 1;

			return 0;
		}
		#endregion
	}
}
