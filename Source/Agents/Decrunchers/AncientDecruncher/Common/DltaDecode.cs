/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Exceptions;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Common
{
	/// <summary>
	/// Helper class for delta decoding
	/// </summary>
	internal static class DltaDecode
	{
		/********************************************************************/
		/// <summary>
		/// Delta decoder
		/// </summary>
		/********************************************************************/
		public static void Decode(string agentName, byte[] bufferDest, byte[] bufferSrc, uint offset, uint size)
		{
			if (OverflowCheck.Sum(offset, size) > bufferSrc.Length)
				throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

			if (OverflowCheck.Sum(offset, size) > bufferDest.Length)
				throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

			byte ctr = 0;
			for (uint i = 0; i < size; i++)
			{
				ctr += bufferSrc[offset + i];
				bufferDest[offset + i] = ctr;
			}
		}
	}
}
