/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// Structure to hold side data for an AVFrame.
	///
	/// sizeof(AVFrameSideData) is not a part of the public ABI, so new fields may be added
	/// to the end with a minor bump
	/// </summary>
	public class AvFrameSideData
	{
		/// <summary>
		/// 
		/// </summary>
		public AvFrameSideDataType Type;

		/// <summary>
		/// 
		/// </summary>
		public IDataContext Data;

		/// <summary>
		/// 
		/// </summary>
		public AvDictionary Metadata;

		/// <summary>
		/// 
		/// </summary>
		public AvBufferRef Buf;
	}
}
