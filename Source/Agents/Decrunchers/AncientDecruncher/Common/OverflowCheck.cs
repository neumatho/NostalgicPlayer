/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Common
{
	/// <summary>
	/// 
	/// </summary>
	internal static class OverflowCheck
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static uint Sum(uint a, uint b)
		{
			// TODO: Add type traits to handle signed integers
			uint ret = a + b;
			if (ret < a)
				throw new OverflowException();

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static uint Sum(uint a, uint b, params uint[] args)
		{
			uint ret = Sum(a, b);

			foreach (uint v in args)
				ret = Sum(ret, v);

			return ret;
		}
	}
}
