/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public enum AvFieldOrder
	{
		/// <summary>
		/// 
		/// </summary>
		Unknown,

		/// <summary>
		/// 
		/// </summary>
		Progressive,

		/// <summary>
		/// Top coded_first, top displayed first
		/// </summary>
		Tt,

		/// <summary>
		/// Bottom coded first, bottom displayed first
		/// </summary>
		Bb,

		/// <summary>
		/// Top coded first, bottom displayed first
		/// </summary>
		Tb,

		/// <summary>
		/// Bottom coded first, top displayed first
		/// </summary>
		Bt
	}
}
