/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp
{
	/// <summary>
	/// Simple thread-safe PRNG
	/// </summary>
	internal class SidRandom
	{
		private uint seed;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SidRandom(uint seed)
		{
			this.seed = seed * 1103515245 + 12345;
		}



		/********************************************************************/
		/// <summary>
		/// Generate new pseudo-random number
		/// </summary>
		/********************************************************************/
		public uint Next()
		{
			seed = seed * 13 + 1;
			return seed;
		}
	}
}
