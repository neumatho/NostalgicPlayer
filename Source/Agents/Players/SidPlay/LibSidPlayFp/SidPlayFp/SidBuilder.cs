/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Collections.Generic;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.SidPlayFp
{
	/// <summary>
	/// Base class for SID builders
	/// </summary>
	internal abstract class SidBuilder
	{
		private readonly string name;

		protected string errorBuffer;

		protected readonly HashSet<SidEmu> sidObjs = new HashSet<SidEmu>();

		protected bool status;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected SidBuilder(string name)
		{
			this.name = name;
			errorBuffer = Resources.IDS_SID_NA;
			status = true;
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Available devices. 0 means endless
		/// </summary>
		/********************************************************************/
		public abstract uint AvailDevices();



		/********************************************************************/
		/// <summary>
		/// Create the SID emulator
		/// </summary>
		/********************************************************************/
		public abstract uint Create(uint sids);



		/********************************************************************/
		/// <summary>
		/// Toggle SID filter emulation
		/// </summary>
		/********************************************************************/
		public abstract void Filter(bool enable);
		#endregion

		/********************************************************************/
		/// <summary>
		/// Find a free SID of the required specs
		/// </summary>
		/********************************************************************/
		public SidEmu Lock(EventScheduler scheduler, SidConfig.sid_model_t model, bool digiBoost)
		{
			status = true;

			foreach (SidEmu sid in sidObjs)
			{
				if (sid.Lock(scheduler))
				{
					sid.Model(model, digiBoost);
					return sid;
				}
			}

			// Unable to locate a free SID
			status = false;
			errorBuffer = string.Format(Resources.IDS_SID_ERR_NO_SIDS, Name());

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Release this SID
		/// </summary>
		/********************************************************************/
		public void Unlock(SidEmu device)
		{
			if (sidObjs.TryGetValue(device, out SidEmu so))
				so.Unlock();
		}



		/********************************************************************/
		/// <summary>
		/// Error message
		/// </summary>
		/********************************************************************/
		public string Error()
		{
			return errorBuffer;
		}



		/********************************************************************/
		/// <summary>
		/// Determine current state of object
		/// </summary>
		/********************************************************************/
		public bool GetStatus()
		{
			return status;
		}



		/********************************************************************/
		/// <summary>
		/// Get the builder's name
		/// </summary>
		/********************************************************************/
		protected string Name()
		{
			return name;
		}
	}
}
