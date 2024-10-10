/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class Stereo_Dec_State : IDeepCloneable<Stereo_Dec_State>
	{
		public readonly opus_int16[] pred_prev_Q13 = new opus_int16[2];
		public readonly opus_int16[] sMid = new opus_int16[2];
		public readonly opus_int16[] sSide = new opus_int16[2];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			Array.Clear(pred_prev_Q13);
			Array.Clear(sMid);
			Array.Clear(sSide);
		}



		/********************************************************************/
		/// <summary>
		/// Clone the current object into a new one
		/// </summary>
		/********************************************************************/
		public Stereo_Dec_State MakeDeepClone()
		{
			Stereo_Dec_State clone = new Stereo_Dec_State();

			Array.Copy(pred_prev_Q13, clone.pred_prev_Q13, pred_prev_Q13.Length);
			Array.Copy(sMid, clone.sMid, sMid.Length);
			Array.Copy(sSide, clone.sSide, sSide.Length);

			return clone;
		}
	}
}
