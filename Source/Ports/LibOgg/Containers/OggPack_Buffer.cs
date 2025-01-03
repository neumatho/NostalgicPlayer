﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.CKit;

namespace Polycode.NostalgicPlayer.Ports.LibOgg.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class OggPack_Buffer
	{
		/// <summary></summary>
		public c_long EndByte;
		/// <summary></summary>
		public c_int EndBit;

		/// <summary></summary>
		public CPointer<byte> Buffer;
		/// <summary></summary>
		public CPointer<byte> Ptr;
		/// <summary></summary>
		public c_long Storage;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			EndByte = 0;
			EndBit = 0;

			Buffer.SetToNull();
			Ptr.SetToNull();
			Storage = 0;
		}
	}
}
