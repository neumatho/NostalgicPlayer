/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class MdctLookup : IVorbisLookTransform
	{
		public c_int n;
		public c_int log2n;

		public Pointer<Data_Type> trig;
		public c_int[] bitrev;

		public Data_Type scale;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			n = 0;
			log2n = 0;
			trig.SetToNull();
			bitrev = null;
			scale = 0;
		}
	}
}
