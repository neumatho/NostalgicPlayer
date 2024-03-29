﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers
{
	/// <summary>
	/// Different error numbers
	/// </summary>
	internal enum Error
	{
		None = 0,
		Data_Corrupted,
		Version_Unsupported,
		Reading_Data,
		Wrong_Chunk_Order,
		Sample_Size_Not_Supported,
	}
}
