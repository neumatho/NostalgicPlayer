/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibSwResample.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class Filter_T
	{
		public enum Filter_Type
		{
			Fir,
			Lir
		}

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Filter_T(c_int rate, Filter_Type type, size_t len, c_int gain_cB, c_double[] coefs, SwrDitherType name)
		{
			Rate = rate;
			Type = type;
			Len = len;
			Gain_cB = gain_cB;
			Coefs = coefs;
			Name = name;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Rate { get; }



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Filter_Type Type { get; }



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public size_t Len { get; }



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Gain_cB { get; }



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public CPointer<c_double> Coefs { get; }



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public SwrDitherType Name { get; }
	}
}
