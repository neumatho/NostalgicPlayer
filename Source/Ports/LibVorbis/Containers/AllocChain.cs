﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AllocChain
	{
		/// <summary></summary>
		public Pointer<byte> ptr;
		/// <summary></summary>
		public AllocChain next;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			ptr.SetToNull();
			next = null;
		}
	}
}
