/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Interfaces
{
	/// <summary>
	/// 
	/// </summary>
	internal class SidLazyIPtr<TInterface> : SidIPtr<TInterface> where TInterface : class, ISidUnknown
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SidLazyIPtr(SidLazyIPtr<TInterface> unknown) : base(unknown)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SidLazyIPtr(ISidUnknown unknown) : base(unknown)
		{
		}
	}
}
