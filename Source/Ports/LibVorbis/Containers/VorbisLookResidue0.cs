/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class VorbisLookResidue0 : IVorbisLookResidue, IClearable
	{
		public VorbisInfoResidue0 info;

		public c_int parts;
		public c_int stages;
		public Codebook[] fullbooks;
		public Codebook phrasebook;
		public Codebook[][] partbooks;

		public c_int partvals;
		public c_int[][] decodemap;

		public c_long postbits;
		public c_long phrasebits;
		public c_long frames;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			info = null;
			parts = 0;
			stages = 0;
			fullbooks = null;
			phrasebook = null;
			partbooks = null;
			partvals = 0;
			decodemap = null;
			postbits = 0;
			phrasebits = 0;
			frames = 0;
		}
	}
}
