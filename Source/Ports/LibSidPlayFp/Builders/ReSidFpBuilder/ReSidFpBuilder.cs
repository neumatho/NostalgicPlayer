/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.SidPlayFp;

namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp.Builders.ReSidFpBuilder
{
	/// <summary>
	/// ReSidFp builder class
	/// </summary>
	public class ReSidFpBuilder : SidBuilder
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ReSidFpBuilder() : base("ReSidFp")
		{
		}



		/********************************************************************/
		/// <summary>
		/// Set 6581 filter curve
		/// </summary>
		/********************************************************************/
		public void Filter6581Curve(double filterCurve)
		{
			foreach (ReSidFp s in sidObjs)
				s.Filter6581Curve(filterCurve);
		}



		/********************************************************************/
		/// <summary>
		/// Set 6581 filter curve
		/// </summary>
		/********************************************************************/
		public void Filter6581Range(double filterRange)
		{
			foreach (ReSidFp s in sidObjs)
				s.Filter6581Range(filterRange);
		}



		/********************************************************************/
		/// <summary>
		/// Set 8580 filter curve
		/// </summary>
		/********************************************************************/
		public void Filter8580Curve(double filterCurve)
		{
			foreach (ReSidFp s in sidObjs)
				s.Filter8580Curve(filterCurve);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void CombinedWaveformsStrength(SidConfig.sid_cw_t cws)
		{
			foreach (ReSidFp s in sidObjs)
				s.CombinedWaveforms(cws);
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
		#endregion
	}
}
