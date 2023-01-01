/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.Sawteeth.Implementation
{
	/// <summary>
	/// Low frequency oscillation
	/// </summary>
	internal class Lfo
	{
		private float curr;
		private float step;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Lfo()
		{
			curr = 0.0f;
			step = (float)(6.28 * 0.00002267573696145124);
		}



		/********************************************************************/
		/// <summary>
		/// Set frequency
		/// </summary>
		/********************************************************************/
		public void SetFreq(float freq)
		{
			step = (float)(freq * (6.28 * 0.00002267573696145124));
		}



		/********************************************************************/
		/// <summary>
		/// Return next value
		/// </summary>
		/********************************************************************/
		public float Next()
		{
			return (float)Math.Cos(curr += step);
		}
	}
}
