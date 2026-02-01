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
	internal class FFBsfContext : AvBsfContext
	{
		/// <summary>
		/// 
		/// </summary>
		public AvBsfContext Pub => this;

		/// <summary>
		/// 
		/// </summary>
		public AvPacket Buffer_Pkt;

		/// <summary>
		/// 
		/// </summary>
		public c_int Eof;
	}
}
