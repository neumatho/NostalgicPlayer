/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AvDictionaryEntry
	{
		/// <summary>
		/// 
		/// </summary>
		public CPointer<char> Key;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<char> Value;
	}
}
