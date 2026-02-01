/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AvBuffer : IClearable
	{
		/// <summary>
		/// Data described by this buffer
		/// </summary>
		internal IDataContext Data;

		/// <summary>
		/// Number of existing AVBufferRef instances referring to this buffer
		/// </summary>
		internal c_uint RefCount;

		/// <summary>
		/// A callback for freeing the data
		/// </summary>
		internal UtilFunc.Buffer_Free_Delegate Free;

		/// <summary>
		/// An opaque pointer, to be used by the freeing callback
		/// </summary>
		internal IOpaque Opaque;

		/// <summary>
		/// 
		/// </summary>
		internal AvBufferFlag Flags;

		/// <summary>
		/// 
		/// </summary>
		internal BufferFlag Flags_Internal;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			Data = null;
			RefCount = 0;
			Free = null;
			Opaque = null;
			Flags = AvBufferFlag.None;
			Flags_Internal = BufferFlag.None;
		}
	}
}
