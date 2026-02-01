/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class UrlProtocol
	{
		/// <summary>
		/// 
		/// </summary>
		public CPointer<char> Name;

		/// <summary>
		/// 
		/// </summary>
		public FormatFunc.Url_Open_Delegate Url_Open;

		/// <summary>
		/// This callback is to be used by protocols which open further nested
		/// protocols. options are then to be passed to ffurl_open_whitelist()
		/// or ffurl_connect() for those nested protocols
		/// </summary>
		public FormatFunc.Url_Open2_Delegate Url_Open2;

		/// <summary>
		/// 
		/// </summary>
		public FormatFunc.Url_Accept_Delegate Url_Accept;

		/// <summary>
		/// 
		/// </summary>
		public FormatFunc.Url_Handshake_Delegate Url_Handshake;

		/// <summary>
		/// Read data from the protocol.
		/// If data is immediately available (even less than size), EOF is
		/// reached or an error occurs (including EINTR), return immediately.
		/// Otherwise:
		/// In non-blocking mode, return AVERROR(EAGAIN) immediately.
		/// In blocking mode, wait for data/EOF/error with a short timeout (0.1s),
		/// and return AVERROR(EAGAIN) on timeout.
		/// Checking interrupt_callback, looping on EINTR and EAGAIN and until
		/// enough data has been read is left to the calling function; see
		/// retry_transfer_wrapper in avio.c
		/// </summary>
		public FormatFunc.Url_Read_Delegate Url_Read;

		/// <summary>
		/// 
		/// </summary>
		public FormatFunc.Url_Write_Delegate Url_Write;

		/// <summary>
		/// 
		/// </summary>
		public FormatFunc.Url_Seek_Delegate Url_Seek;

		/// <summary>
		/// 
		/// </summary>
		public FormatFunc.Url_Close_Delegate Url_Close;

		/// <summary>
		/// 
		/// </summary>
		public FormatFunc.ReadPause_Delegate Url_Read_Pause;

		/// <summary>
		/// 
		/// </summary>
		public FormatFunc.ReadSeek_Delegate Url_Read_Seek;

		/// <summary>
		/// 
		/// </summary>
		public FormatFunc.Url_Get_File_Handle_Delegate Url_Get_File_Handle;

		/// <summary>
		/// 
		/// </summary>
		public FormatFunc.Url_Get_Multi_File_Handle_Delegate Url_Get_Multi_File_Handle;

		/// <summary>
		/// 
		/// </summary>
		public FormatFunc.Url_Get_Short_Seek_Delegate Url_Get_Short_Seek;

		/// <summary>
		/// 
		/// </summary>
		public FormatFunc.Url_Shutdown_Delegate Url_Shutdown;

		/// <summary>
		/// 
		/// </summary>
		public AvClass Priv_Data_Class;

		/// <summary>
		/// 
		/// </summary>
		public UtilFunc.Allocator_Delegate Priv_Data_Allocator;

		/// <summary>
		/// 
		/// </summary>
		public UrlProtocolFlag Flags;

		/// <summary>
		/// 
		/// </summary>
		public FormatFunc.Url_Check_Delegate Url_Check;

		/// <summary>
		/// 
		/// </summary>
		public FormatFunc.Url_Open_Dir_Delegate Url_Open_Dir;

		/// <summary>
		/// 
		/// </summary>
		public FormatFunc.Url_Read_Dir_Delegate Url_Read_Dir;

		/// <summary>
		/// 
		/// </summary>
		public FormatFunc.Url_Close_Dir_Delegate Url_Close_Dir;

		/// <summary>
		/// 
		/// </summary>
		public FormatFunc.Url_Delete_Delegate Url_Delete;

		/// <summary>
		/// 
		/// </summary>
		public FormatFunc.Url_Move_Delegate Url_Move;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<char> Default_Whitelist;
	}
}
