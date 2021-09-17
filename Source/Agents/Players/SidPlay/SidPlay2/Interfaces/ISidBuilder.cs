/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Environment;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Interfaces
{
	internal interface ISidBuilder : ISidUnknown
	{
		private static readonly IId iid = new IId(0x1c9ea475, 0xac10, 0x4345, 0x8b88, 0x3e48, 0x04e0ea38);

		/// <summary>
		/// Return an unique ID for this implementation
		/// </summary>
		static IId IId()
		{
			return iid;
		}

		/// <summary></summary>
		bool IsOk { get; }

		/// <summary></summary>
		string Error { get; }

		/// <summary>
		/// Find a free SID of the required specs
		/// </summary>
		ISidUnknown Lock(IC64Env env, Sid2Model model);

		/// <summary>
		/// Allow something to use this SID
		/// </summary>
		void Unlock(ISidUnknown device);
	}
}
