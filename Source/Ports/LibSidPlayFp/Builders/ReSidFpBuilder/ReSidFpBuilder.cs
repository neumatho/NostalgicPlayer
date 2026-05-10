/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.SidPlayFp;

namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp.Builders.ReSidFpBuilder
{
	/// <summary>
	/// ReSidFp builder class
	/// </summary>
	public class ReSidFpBuilder : SidBuilder
	{
		private class Config
		{
			public Property<c_double> Filter8580Curve;
			public Property<c_double> Filter6581Curve;
			public Property<c_double> Filter6581Range;
			public Property<SidConfig.sid_cw_t> Cws;
			public Property<bool> Old6581Caps;
		}

		private readonly Config config;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ReSidFpBuilder(string name) : base(name)
		{
			config = new Config();
		}



		/********************************************************************/
		/// <summary>
		/// Set 6581 filter curve
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Filter6581Curve(double filterCurve)
		{
			config.Filter6581Curve = filterCurve;

			foreach (SidEmu e in sidObjs)
				((ReSidFpEmu)e).Filter6581Curve(filterCurve);
		}



		/********************************************************************/
		/// <summary>
		/// Set 6581 filter curve
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Filter6581Range(double filterRange)
		{
			config.Filter6581Range = filterRange;

			foreach (SidEmu e in sidObjs)
				((ReSidFpEmu)e).Filter6581Range(filterRange);
		}



		/********************************************************************/
		/// <summary>
		/// Set 8580 filter curve
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Filter8580Curve(double filterCurve)
		{
			config.Filter8580Curve = filterCurve;

			foreach (SidEmu e in sidObjs)
				((ReSidFpEmu)e).Filter8580Curve(filterCurve);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void CombinedWaveformsStrength(SidConfig.sid_cw_t cws)
		{
			config.Cws = cws;

			foreach (SidEmu e in sidObjs)
				((ReSidFpEmu)e).CombinedWaveforms(cws);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EnableOld6581Caps(bool enable)
		{
			config.Old6581Caps = enable;

			foreach (SidEmu e in sidObjs)
				((ReSidFpEmu)e).EnableOld6581Caps(enable);
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Create a new SID emulation
		/// </summary>
		/********************************************************************/
		internal override SidEmu Create()
		{
			try
			{
				ReSidFpEmu sid = new ReSidFpEmu(this);

				if (config.Filter6581Curve.Has_Value())
					sid.Filter6581Curve(config.Filter6581Curve.Value());

				if (config.Filter8580Curve.Has_Value())
					sid.Filter8580Curve(config.Filter8580Curve.Value());

				if (config.Filter6581Range.Has_Value())
					sid.Filter6581Range(config.Filter6581Range.Value());

				if (config.Cws.Has_Value())
					sid.CombinedWaveforms(config.Cws.Value());

				if (config.Old6581Caps.Has_Value())
					sid.EnableOld6581Caps(config.Old6581Caps.Value());

				return sid;
			}
			catch (Exception)
			{
				errorBuffer = string.Format(Resources.IDS_SID_ERR_CREATE_OBJECT, Name());
				return null;
			}
		}
		#endregion
	}
}
