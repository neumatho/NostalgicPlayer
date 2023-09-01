﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp
{
	/// <summary>
	/// 
	/// </summary>
	public class Xmp_Callbacks
	{
		/// <summary></summary>
		public delegate c_ulong Read_Delegate(Span<uint8> dest, c_ulong len, c_ulong nMemB, object priv);
		/// <summary></summary>
		public delegate c_int Seek_Delegate(object priv, c_long offset, SeekOrigin whence);
		/// <summary></summary>
		public delegate c_long Tell_Delegate(object priv);
		/// <summary></summary>
		public delegate c_int Close_Delegate(object priv);

		/// <summary>
		/// 
		/// </summary>
		public Read_Delegate Read_Func;

		/// <summary>
		/// 
		/// </summary>
		public Seek_Delegate Seek_Func;

		/// <summary>
		/// 
		/// </summary>
		public Tell_Delegate Tell_Func;

		/// <summary>
		/// 
		/// </summary>
		public Close_Delegate Close_Func;
	}
}
