/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// 
	/// </summary>
	internal class SMix
	{
		private readonly Xmp_Context ctx;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SMix(Xmp_Context ctx)
		{
			this.ctx = ctx;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Xmp_Instrument LibXmp_Get_Instrument(c_int ins)
		{
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;
			Xmp_Instrument xxi;

			if (ins < 0)
				xxi = null;
			else if (ins < mod.Ins)
				xxi = mod.Xxi[ins];
			else
				xxi = null;

			return xxi;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Xmp_Sample LibXmp_Get_Sample(c_int smp)
		{
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;
			Xmp_Sample xxs;

			if (smp < 0)
				xxs = null;
			else if (smp < mod.Smp)
				xxs = mod.Xxs[smp];
			else
				xxs = null;

			return xxs;
		}
	}
}
