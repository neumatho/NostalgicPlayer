/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// Rational number (pair of numerator and denominator)
	/// </summary>
	public struct AvRational : IClearable
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AvRational()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AvRational(c_int num, c_int den)
		{
			Num = num;
			Den = den;
		}



		/********************************************************************/
		/// <summary>
		/// Numerator
		/// </summary>
		/********************************************************************/
		public c_int Num;



		/********************************************************************/
		/// <summary>
		/// Denominator
		/// </summary>
		/********************************************************************/
		public c_int Den;



		/********************************************************************/
		/// <summary>
		/// Clear all fields
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			Num = 0;
			Den = 0;
		}
	}
}
