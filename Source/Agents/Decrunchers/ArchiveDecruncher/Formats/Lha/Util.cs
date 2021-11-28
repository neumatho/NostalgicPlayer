/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Lha.Containers;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Lha
{
	internal partial class LhaCore
	{
		/********************************************************************/
		/// <summary>
		/// Convert path delimit
		/// </summary>
		/********************************************************************/
		private void ConvDelim(byte[] path, byte delim)
		{
			for (int p = 0; path[p] != 0x00; p++)
			{
				byte c = path[p];

				if ((c == (byte)'\\') || (c == Constants.Delim) || (c == Constants.Delim2))
					path[p] = delim;
			}
		}
	}
}
