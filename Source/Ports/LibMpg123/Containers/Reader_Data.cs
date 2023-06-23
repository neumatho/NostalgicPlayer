/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;

namespace Polycode.NostalgicPlayer.Ports.LibMpg123.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class Reader_Data
	{
		public delegate ssize_t FdRead_Delegate(Mpg123_Handle fr, Memory<c_uchar> buf, size_t count);
		public delegate ssize_t R_Read_Handle_Delegate(object handle, Memory<c_uchar> buf, size_t count);
		public delegate ssize_t R_LSeek_Handle_Delegate(object handle, off_t offset, SeekOrigin whence);
		public delegate void Cleanup_Handle_Delegate(object handle);
		public delegate ssize_t Read_Delegate(Stream fd, Memory<c_uchar> buf, size_t count);
		public delegate ssize_t LSeek_Delegate(Stream fd, off_t offset, SeekOrigin whence);
		public delegate ssize_t FullRead_Delegate(Mpg123_Handle fr, Memory<c_uchar> buf, ssize_t count);

		public off_t FileLen;			// Total file length or total buffer size
		public off_t FilePos;			// Position in file or position in buffer chain
		public Stream FilePt;

		// Custom opaque I/O handle from the client
		public object IOHandle;
		public ReaderFlags Flags;
//		public c_long Timeout_Sec;
		public FdRead_Delegate FdRead;

		// User can replace the read and lseek functions. The r_* are the stored replacement functions or NULL
		public Read_Delegate R_Read;
		public LSeek_Delegate R_LSeek;

		// These are custom I/O routines for opaque user handles.
		// They get picked if there's some iohandle set
		public R_Read_Handle_Delegate R_Read_Handle;
		public R_LSeek_Handle_Delegate R_LSeek_Handle;

		// An optional cleaner for the handle on closing the stream
		public Cleanup_Handle_Delegate Cleanup_Handle;

		// These two pointers are the actual workers (default map to POSIX read/lseek)
		public Read_Delegate Read;
		public LSeek_Delegate LSeek;

		// Buffered readers want that abstracted, set internally
		public FullRead_Delegate FullRead;

		public readonly BufferChain Buffer = new BufferChain();
	}
}
