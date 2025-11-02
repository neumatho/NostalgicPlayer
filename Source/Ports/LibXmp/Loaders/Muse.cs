/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Loaders
{
	/// <summary>
	/// Helper class to test and extract data from Muse files
	/// </summary>
	internal class Muse
	{
		/********************************************************************/
		/// <summary>
		/// Will check to see if the file is a Muse file
		/// </summary>
		/********************************************************************/
		public int IsMuseFile(Hio f)
		{
			CPointer<uint8> @in = new CPointer<uint8>(8);

			if (f.Hio_Read(@in, 1, 8) != 8)
				return -1;

			if (CMemory.memcmp(@in, "MUSE", 4) != 0)
				return -1;

			uint32 r = DataIo.ReadMem32B(@in + 4);
			if ((r != 0xdeadbeaf) && (r != 0xdeadbabe))
				return -1;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Return 
		/// </summary>
		/********************************************************************/
		public Hio GetHioWithDecompressedData(Hio f)
		{
			return Hio.Hio_Open_File2(new MuseStream(f));
		}
	}
}
