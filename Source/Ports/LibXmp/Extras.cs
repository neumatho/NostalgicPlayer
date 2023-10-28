/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Player;
using Polycode.NostalgicPlayer.Ports.LibXmp.FormatExtras;

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

		#region Module extras
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Release_Module_Extras()
		{
			Module_Data m = ctx.M;

			if (m.Extra != null)
				m.Extra.Release_Module_Extras();
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

			if (m.Extra is IModuleNewChannelExtras moduleNewChannelExtras)
			{
				if (moduleNewChannelExtras.New_Channel_Extras(xc) < 0)
					return -1;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Release_Channel_Extras(Channel_Data xc)
		{
			if (xc.Extra != null)
				xc.Extra.Release_Channel_Extras();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Reset_Channel_Extras(Channel_Data xc)
		{
			if (xc.Extra is IChannelReset channelReset)
				channelReset.Reset_Channel();
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

			if (xc.Extra is IChannelPlay channelPlay)
				channelPlay.Play(chn);

			if (xc.Ins >= m.Mod.Ins)		// SFX instruments have no extras
				return;

			if (m.Mod.Xxi[xc.Ins].Extra is IInstrumentPlay instrumentPlay)
				instrumentPlay.Play(xc, chn);
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
			else if (m.Mod.Xxi[xc.Ins].Extra is IInstrumentGetVolume instrumentGetVolume)
				vol = instrumentGetVolume.GetVolume(xc) * xc.Volume / 64;
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

			if (xc.Extra is IChannelGetPeriod channelGetPeriod)
				period = channelGetPeriod.GetPeriod();
			else
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

			if (xc.Extra is IChannelGetLinearBend channelGetLinearBend)
				linear_Bend = channelGetLinearBend.GetLinearBend();
			else
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
			if (xc.Extra is IChannelProcessFx channelProcessFx)
				channelProcessFx.Process_Fx(chn, note, fxT, fxP, fNum);
		}
		#endregion
	}
}
