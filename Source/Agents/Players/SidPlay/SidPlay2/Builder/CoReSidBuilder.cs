/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSid;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Environment;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Imp;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Builder
{
	/// <summary>
	/// Builder interface to the ReSID emulator
	/// </summary>
	internal class CoReSidBuilder : CoBuilder, IReSidBuilder
	{
		private readonly List<ReSid> sidObjs;

		private string errorMessage;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private CoReSidBuilder(string name) : base(name)
		{
			sidObjs = new List<ReSid>();
		}



		/********************************************************************/
		/// <summary>
		/// Create the builder instance
		/// </summary>
		/********************************************************************/
		public static ISidUnknown Create(string name)
		{
			CoReSidBuilder builder = new CoReSidBuilder(name);

			if (builder.status)
				return builder.IUnknown();

			return null;
		}

		#region ISidUnknown implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override bool IQuery(IId iid, out object implementation)
		{
			if (iid == IReSidBuilder.IId())
				implementation = this.StaticCast<IReSidBuilder>();
			else if (iid == ISidBuilder.IId())
				implementation = this.StaticCast<IReSidBuilder>();
			else if (iid == ISidUnknown.IId())
				implementation = this.StaticCast<IReSidBuilder>();
			else
			{
				implementation = null;
				return false;
			}

			return true;
		}
		#endregion

		#region ISidBuilder implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override string Error => errorMessage;



		/********************************************************************/
		/// <summary>
		/// Find a free SID of the required specs
		/// </summary>
		/********************************************************************/
		public override ISidUnknown Lock(IC64Env env, Sid2Model model)
		{
			int size = sidObjs.Count;
			status = true;

			for (int i = 0; i < size; i++)
			{
				ReSid sid = sidObjs[i];
				if (sid.Lock(env))
				{
					sid.Model(model);
					return sid.IUnknown();
				}
			}

			// Unable to locate free SID
			status = false;
			errorMessage = Resources.IDS_SID_ERR_NO_SIDS;

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Allow something to use this SID
		/// </summary>
		/********************************************************************/
		public override void Unlock(ISidUnknown device)
		{
			ISidUnknown emulation = device.IUnknown();

			// Make sure this is our SID
			int size = sidObjs.Count;
			for (int i = 0; i < size; i++)
			{
				if (sidObjs[i].IUnknown() == emulation)
				{
					// Unlock it
					ReSid sid = sidObjs[i];
					sid.Lock(null);
					break;
				}
			}
		}
		#endregion

		#region IReSidBuilder implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint Create(uint sids)
		{
			// Check available devices
			uint count = Devices(false);
			if (!status)
				return count;

			if ((count != 0) && (count < sids))
				sids = count;

			for (count = 0; count < sids; count++)
			{
				ReSid sid = new ReSid(this);

				if (!sid.IsOk)
				{
					status = false;
					errorMessage = sid.Error;

					return count;
				}

				sidObjs.Add(sid);
			}

			return count;
		}



		/********************************************************************/
		/// <summary>
		/// True will give you the number of used devices.
		///   Return values: 0 none, positive is used SIDs
		/// 
		/// False will give you all available SIDs.
		///   Return values: 0 endless, positive is available SIDs
		/// </summary>
		/********************************************************************/
		public uint Devices(bool used)
		{
			status = true;

			if (used)
				return (uint)sidObjs.Count;

			// Available devices
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Enable filter
		/// </summary>
		/********************************************************************/
		public void Filter(bool enable)
		{
			int size = sidObjs.Count;
			status = true;

			for (int i = 0; i < size; i++)
			{
				ReSid sid = sidObjs[i];
				sid.Filter(enable);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set new filter definition
		/// </summary>
		/********************************************************************/
		public void Filter(Spline.FCPoint[] filter)
		{
			int size = sidObjs.Count;
			status = true;

			for (int i = 0; i < size; i++)
			{
				ReSid sid = sidObjs[i];
				if (!sid.Filter(filter))
				{
					status = false;
					errorMessage = Resources.IDS_SID_ERR_FILTER_DEFINITION;

					return;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Sampling(uint freq)
		{
			int size = sidObjs.Count;
			status = true;

			for (int i = 0; i < size; i++)
			{
				ReSid sid = sidObjs[i];
				sid.Sampling(freq);
			}
		}
		#endregion
	}
}
