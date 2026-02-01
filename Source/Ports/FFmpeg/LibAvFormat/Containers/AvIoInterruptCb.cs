/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// Callback for checking whether to abort blocking functions.
	/// AVERROR_EXIT is returned in this case by the interrupted
	/// function. During blocking operations, callback is called with
	/// opaque as parameter. If the callback returns 1, the
	/// blocking operation will be aborted.
	///
	/// No members can be added to this struct without a major bump, if
	/// new elements have been added after this struct in AVFormatContext
	/// or AVIOContext
	/// </summary>
	public class AvIoInterruptCb : IDeepCloneable<AvIoInterruptCb>
	{
		/// <summary>
		/// 
		/// </summary>
		public FormatFunc.Callback_Delegate Callback;

		/// <summary>
		/// 
		/// </summary>
		public IOpaque Opaque;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public AvIoInterruptCb MakeDeepClone()
		{
			return (AvIoInterruptCb)MemberwiseClone();
		}
	}
}
