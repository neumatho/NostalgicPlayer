/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Environment
{
	/// <summary>
	/// This is the environment file which defines all the standard functions
	/// to be inherited by the ICs
	/// </summary>
	internal abstract class C64Environment : IC64Environment
	{
		private C64Environment envp;

		/********************************************************************/
		/// <summary>
		/// Initialize the environment
		/// </summary>
		/********************************************************************/
		public void SetEnvironment(C64Environment env)
		{
			envp = env;
		}

		#region Environment methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void EnvReset()
		{
			envp.EnvReset();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual byte EnvReadMemByte(ushort addr)
		{
			return envp.EnvReadMemByte(addr);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void EnvWriteMemByte(ushort addr, byte data)
		{
			envp.EnvWriteMemByte(addr, data);
		}
		#endregion

		#region SidPlay compatible methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual bool EnvCheckBankJump(ushort addr)
		{
			return envp.EnvCheckBankJump(addr);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual byte EnvReadMemDataByte(ushort addr)
		{
			return envp.EnvReadMemDataByte(addr);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void EnvSleep()
		{
			envp.EnvSleep();
		}
		#endregion
	}
}
