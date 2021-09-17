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
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Imp
{
	/// <summary>
	/// Interface to SID emulator builder
	/// </summary>
	internal abstract class CoBuilder : CoUnknown, ICoBuilder
	{
		/// <summary>
		/// Determine current state of object (true = okay, false = error)
		/// </summary>
		protected bool status;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected CoBuilder(string name) : base(name)
		{
			status = true;
		}

		#region ISidRebuilder implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool IsOk => status;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public abstract string Error { get; }



		/********************************************************************/
		/// <summary>
		/// Find a free SID of the required specs
		/// </summary>
		/********************************************************************/
		public abstract ISidUnknown Lock(IC64Env env, Sid2Model model);



		/********************************************************************/
		/// <summary>
		/// Allow something to use this SID
		/// </summary>
		/********************************************************************/
		public abstract void Unlock(ISidUnknown device);
		#endregion
	}
}
