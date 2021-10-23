/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.SidPlayFp;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSid;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.Builders
{
	/// <summary>
	/// Builder for ReSid
	/// </summary>
	internal class ReSidBuilder : SidBuilder
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ReSidBuilder() : base("ReSid")
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
					sidObjs.Add(new ReSid(this));
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
			foreach (ReSid s in sidObjs)
				s.Filter(enable);
		}
		#endregion



		/********************************************************************/
		/// <summary>
		/// Set new filter definition
		/// </summary>
		/********************************************************************/
		public bool Filter(Spline.FCPoint[] filter)
		{
			Spline.FCPoint[] fc = new Spline.FCPoint[0x802];
			int f0 = 0;
			int points = 0;

			if (filter == null)
			{
				// Select default filter
				foreach (ReSid s in sidObjs)
					s.FcDefault(fc, out points);
			}
			else
			{
				// Make sure there are enough filter points and they are legal
				points = filter.Length;
				if ((points < 2) || (points > 0x800))
					return false;

				Spline.FCPoint[] fStart = { new Spline.FCPoint(-1, 0) };
				Spline.FCPoint[] fPrev = fStart;
				int fPrevOffset = 0;
				int fin = 0;
				int fOut = 0;

				// Last check, make sure they are list in numerical order
				// for both axis
				while (points-- > 0)
				{
					if (fPrev[fPrevOffset].X >= filter[fin].X)
						return false;

					fOut++;
					fc[fOut].X = filter[fin].X;
					fc[fOut].Y = filter[fin].Y;

					fPrev = filter;
					fPrevOffset = fin++;
				}

				// Updated ReSID interpolate requires we
				// repeat the end points
				fc[fOut + 1].X = fc[fOut].X;
				fc[fOut + 1].Y = fc[fOut].Y;

				fc[0].X = fc[1].X;
				fc[0].Y = fc[1].Y;

				points = filter.Length + 2;
			}

			// Function from ReSID
			points--;

			foreach (ReSid s in sidObjs)
				Spline.Interpolate(fc, f0, points, s.FcPlotter(), 1.0f);

			return true;
		}
	}
}
