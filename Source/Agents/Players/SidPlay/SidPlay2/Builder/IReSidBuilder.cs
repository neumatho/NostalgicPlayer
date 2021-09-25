/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/

using Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSid;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Builder
{
	/// <summary>
	/// 
	/// </summary>
	internal interface IReSidBuilder : ISidBuilder
	{
		private static readonly IId iid = new IId(0x90a0aa02, 0xf272, 0x435d, 0x8f6b, 0x71b4, 0x5ac2f99f);

		/// <summary>
		/// Return an unique ID for this implementation
		/// </summary>
		new static IId IId()
		{
			return iid;
		}

		/// <summary></summary>
		uint Create(uint sids);

		/// <summary>
		/// True will give you the number of used devices.
		///   Return values: 0 none, positive is used SIDs
		/// 
		/// False will give you all available SIDs.
		///   Return values: 0 endless, positive is available SIDs
		/// </summary>
		uint Devices(bool used);

		/// <summary>
		/// Enable filter
		/// </summary>
		void Filter(bool enable);

		/// <summary>
		/// Set new filter definition
		/// </summary>
		void Filter(Spline.FCPoint[] filter);

		/// <summary></summary>
		void Sampling(uint freq);
	}
}
