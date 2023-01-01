/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.SidPlayFp;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.Builders.ReSidFpBuilder
{
	/// <summary>
	/// ReSidFp builder class
	/// </summary>
	internal class ReSidFpBuilder : SidBuilder
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ReSidFpBuilder() : base("ReSidFp")
		{
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Available devices. 0 means endless
		/// </summary>
		/********************************************************************/
		public override uint AvailDevices()
		{
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Create a new SID emulation
		/// </summary>
		/********************************************************************/
		public override uint Create(uint sids)
		{
			status = true;

			// Check available devices
			uint count = AvailDevices();

			if ((count != 0) && (count < sids))
				sids = count;

			for (count = 0; count < sids; count++)
			{
				try
				{
					sidObjs.Add(new ReSidFp(this));
				}
				catch (Exception)
				{
					errorBuffer = string.Format(Resources.IDS_SID_ERR_CREATE_OBJECT, Name());
					status = false;
					break;
				}
			}

			return count;
		}



		/********************************************************************/
		/// <summary>
		/// Toggle SID filter emulation
		/// </summary>
		/********************************************************************/
		public override void Filter(bool enable)
		{
			foreach (ReSidFp s in sidObjs)
				s.Filter(enable);
		}
		#endregion
	}
}
