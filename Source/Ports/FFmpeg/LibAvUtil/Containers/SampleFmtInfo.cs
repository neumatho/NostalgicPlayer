/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class SampleFmtInfo
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SampleFmtInfo(string name, c_int bits, c_int planar, AvSampleFormat altForm)
		{
			Name = name.ToCharPointer();
			Bits = bits;
			Planar = planar;
			AltForm = altForm;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public CPointer<char> Name { get; }



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Bits { get; }



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Planar { get; }



		/********************************************************************/
		/// <summary>
		/// Planar ‹-› packed alternative form
		/// </summary>
		/********************************************************************/
		public AvSampleFormat AltForm { get; }
	}
}
