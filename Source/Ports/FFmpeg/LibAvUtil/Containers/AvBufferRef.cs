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
	/// A reference to a data buffer.
	///
	/// The size of this struct is not a part of the public ABI and it is not meant
	/// to be allocated directly
	/// </summary>
	public class AvBufferRef : IClearable, ICopyTo<AvBufferRef>
	{
		/// <summary>
		/// 
		/// </summary>
		public AvBuffer Buffer;

		/// <summary>
		/// The data buffer. It is considered writable if and only if
		/// this is the only reference to the buffer, in which case
		/// av_buffer_is_writable() returns 1
		/// </summary>
		public IDataContext Data;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			Buffer = null;
			Data = null;
		}



		/********************************************************************/
		/// <summary>
		/// Copy the current object into the given
		/// </summary>
		/********************************************************************/
		public void CopyTo(AvBufferRef destination)
		{
			destination.Buffer = Buffer;
			destination.Data = Data;
		}
	}
}
