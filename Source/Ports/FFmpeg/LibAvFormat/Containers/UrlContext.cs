/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class UrlContext : AvClass, IOptionContext
	{
		/// <summary>
		/// Information for av_log(). Set by url_open()
		/// </summary>
		public AvClass Av_Class => this;

		/// <summary>
		/// 
		/// </summary>
		public UrlProtocol Prot;

		/// <summary>
		/// 
		/// </summary>
		public IOpaque Priv_Data;

		/// <summary>
		/// Specified URL
		/// </summary>
		public CPointer<char> FileName;

		/// <summary>
		/// 
		/// </summary>
		public AvIoFlag Flags;

		/// <summary>
		/// If non zero, the stream is packetized with this max packet size
		/// </summary>
		public c_int Max_Packet_Size;

		/// <summary>
		/// True if streamed (no seek possible), default = false
		/// </summary>
		public bool Is_Streamed;

		/// <summary>
		/// 
		/// </summary>
		public bool Is_Connected;

		/// <summary>
		/// 
		/// </summary>
		public AvIoInterruptCb Interrupt_Callback;

		/// <summary>
		/// Maximum time to wait for (network) read/write operation completion, in mcs
		/// </summary>
		public int64_t Rw_Timeout;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<char> Protocol_Whitelist;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<char> Protocol_Blacklist;

		/// <summary>
		/// If non zero, the stream is packetized with this min packet size
		/// </summary>
		public c_int Min_Packet_Size;
	}
}
