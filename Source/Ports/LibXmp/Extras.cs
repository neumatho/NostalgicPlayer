/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Player;

namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// 
	/// </summary>
	internal class Extras
	{
		private readonly Xmp_Context ctx;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Extras(Xmp_Context ctx)
		{
			this.ctx = ctx;
		}
//XX Alt dette skal laves med et interface som en klasse nedarver fra og bliver oprettet i loader og gemt i mod
		#region Module extras
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Release_Module_Extras()
		{
			Module_Data m = ctx.M;

/*//XX			if (Has_Med_Module_Extras(m))
				LibXmp_Med_Release_Module_Extras(m);
			else if (Has_Hmn_Module_Extras(m))
				LibXmp_Hmn_Release_Module_Extras(m);
			else if (Has_Far_Module_Extras(m))
				LibXmp_Far_Release_Module_Extras(m);*/
		}
		#endregion

		#region Channel extras
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_New_Channel_Extras(Channel_Data xc)
		{
			Module_Data m = ctx.M;

/*//XX			if (Has_Med_Module_Extras(m))
			{
				if (LibXmp_Med_New_Channel_Extras(xc) < 0)
					return -1;
			}
			else if (Has_Hmn_Module_Extras(m))
			{
				if (LibXmp_Hmn_New_Channel_Extras(xc) < 0)
					return -1;
			}
			else if (Has_Far_Module_Extras(m))
			{
				if (LibXmp_Far_New_Channel_Extras(xc) < 0)
					return -1;
			}
*/
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Release_Channel_Extras(Channel_Data xc)
		{
			Module_Data m = ctx.M;

/*//XX			if (Has_Med_Module_Extras(m))
				LibXmp_Med_Release_Channel_Extras(xc);
			else if (Has_Hmn_Module_Extras(m))
				LibXmp_Hmn_Release_Channel_Extras(xc);
			else if (Has_Far_Module_Extras(m))
				LibXmp_Far_Release_Channel_Extras(xc);*/
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Reset_Channel_Extras(Channel_Data xc)
		{
			Module_Data m = ctx.M;

/*//XX			if (Has_Med_Module_Extras(m))
				LibXmp_Med_Reset_Channel_Extras(xc);
			else if (Has_Hmn_Module_Extras(m))
				LibXmp_Hmn_Reset_Channel_Extras(xc);
			else if (Has_Far_Module_Extras(m))
				LibXmp_Far_Reset_Channel_Extras(xc);*/
		}
		#endregion

		#region Player extras
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Play_Extras(Channel_Data xc, c_int chn)
		{
			Module_Data m = ctx.M;
//			c_int vol;

//XX			if (Has_Far_Channel_Extras(xc))
//				LibXmp_Far_Play_Extras(xc, chn);

			if (xc.Ins >= m.Mod.Ins)		// SFX instruments have no extras
				return;

/*//XX			if (Has_Med_Instrument_Extras(m.Mod.Xxi[xc.Ins]))
				LibXmp_Med_Play_Extras(xc, chn);
			else if (Has_Hmn_Instrument_Extras(m.Mod.Xxi[xc.Ins]))
				LibXmp_Hmn_Play_Extras(xc, chn);*/
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Extras_Get_Volume(Channel_Data xc)
		{
			Module_Data m = ctx.M;
			c_int vol;

			if (xc.Ins >= m.Mod.Ins)
				vol = xc.Volume;
/*//XX			else if (Has_Med_Instrument_Extras(m.Mod.Xxi[xc.Ins]))
				vol = Med_Channel_Extras(xc).Volume * xc.Volume / 64;
			else if (Has_Hmn_Instrument_Extras(m.Mod.Xxi[xc.Ins]))
				vol = Hmn_Channel_Extras(xc).Volume * xc.Volume / 64;*/
			else
				vol = xc.Volume;

			return vol;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Extras_Get_Period(Channel_Data xc)
		{
			c_int period;

/*//XX			if (Has_Med_Channel_Extras(xc))
				period = LibXmp_Med_Change_Period(xc);
			else*/
				period = 0;

			return period;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Extras_Get_Linear_Bend(Channel_Data xc)
		{
			c_int linear_Bend;

/*//XX			if (Has_Med_Channel_Extras(xc))
				linear_Bend = LibXmp_Med_Linear_Bend(xc);
			else if (Has_Hmn_Channel_Extras(xc))
				linear_Bend = LibXmp_Hmn_Linear_Bend(xc);
			else*/
				linear_Bend = 0;

			return linear_Bend;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Extras_Process_Fx(Channel_Data xc, c_int chn, uint8 note, uint8 fxT, uint8 fxP, c_int fNum)
		{
/*//XX			if (Has_Med_Channel_Extras(xc))
				LibXmp_Med_Extras_Process_Ex(xc, chn, note, fxT, fxP, fNum);
			else if (Has_Hmn_Channel_Extras(xc))
				LibXmp_Hmn_Extras_Process_Ex(xc, chn, note, fxT, fxP, fNum);
			else if (Has_Far_Channel_Extras(xc))
				LibXmp_Far_Extras_Process_Ex(xc, chn, note, fxT, fxP, fNum);*/
		}
		#endregion
	}
}
