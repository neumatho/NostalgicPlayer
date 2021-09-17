/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Event;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Environment
{
	/// <summary>
	/// The C64 environment interface
	/// </summary>
	internal abstract class C64Env : IC64Env
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected C64Env(IEventContext context)
		{
			Context = context;
		}

		#region IC64Env implementation
		/********************************************************************/
		/// <summary>
		/// Return the event context
		/// </summary>
		/********************************************************************/
		public IEventContext Context
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public abstract void InterruptIrq(bool state);



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public abstract void InterruptNmi();



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public abstract void SignalAec(bool state);



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public abstract byte ReadMemRamByte(ushort addr);



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public abstract void Sid2Crc(byte data);



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public abstract void Lightpen();
		#endregion
	}
}
