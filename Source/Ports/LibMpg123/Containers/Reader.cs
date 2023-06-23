/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibMpg123.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class Reader
	{
		public delegate c_int Init_Delegate(Mpg123_Handle fr);
		public delegate void Close_Delegate(Mpg123_Handle fr);
		public delegate ssize_t FullRead_Delegate(Mpg123_Handle fr, Memory<c_uchar> buf, ssize_t count);
		public delegate c_int Head_Read_Delegate(Mpg123_Handle fr, out c_ulong newHead);
		public delegate c_int Head_Shift_Delegate(Mpg123_Handle fr, ref c_ulong head);
		public delegate off_t Skip_Bytes_Delegate(Mpg123_Handle fr, off_t len);
		public delegate c_int Read_Frame_Body_Delegate(Mpg123_Handle fr, Memory<c_uchar> buf, c_int size);
		public delegate c_int Back_Bytes_Delegate(Mpg123_Handle fr, off_t bytes);
		public delegate c_int Seek_Frame_Delegate(Mpg123_Handle fr, off_t newFrame);
		public delegate off_t Tell_Delegate(Mpg123_Handle fr);
		public delegate void Rewind_Delegate(Mpg123_Handle fr);
		public delegate void Forget_Delegate(Mpg123_Handle fr);

		public Init_Delegate Init;
		public Close_Delegate Close;
		public FullRead_Delegate FullRead;
		public Head_Read_Delegate Head_Read;
		public Head_Shift_Delegate Head_Shift;				// Succ: TRUE, else <= 0 (FALSE or READER_MORE)
		public Skip_Bytes_Delegate Skip_Bytes;				// Succ: TRUE, else <= 0 (FALSE or READER_MORE)
		public Read_Frame_Body_Delegate Read_Frame_Body;	// Succ: >= 0, else error or READER_MORE
		public Back_Bytes_Delegate Back_Bytes;
		public Seek_Frame_Delegate Seek_Frame;
		public Tell_Delegate Tell;
		public Rewind_Delegate Rewind;
		public Forget_Delegate Forget;
	}
}
