/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Banks
{
	/// <summary>
	/// Interface to PLA functions
	/// </summary>
	internal interface IPla
	{
		/// <summary>
		/// 
		/// </summary>
		void SetCpuPort(uint8_t state);

		/// <summary>
		/// 
		/// </summary>
		uint8_t GetLastReadByte();

		/// <summary>
		/// 
		/// </summary>
		event_clock_t GetPhi2Time();
	}
}
