/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Imp
{
	/// <summary>
	/// Unknown companion class
	/// </summary>
	internal abstract class CoUnknown : ICoUnknown
	{
		private readonly string name;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected CoUnknown(string name)
		{
			this.name = name;
		}

		#region ISidUnknown implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ISidUnknown IUnknown()
		{
			return this;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public abstract bool IQuery(IId iid, out object implementation);
		#endregion
	}
}
