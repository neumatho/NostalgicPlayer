/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io_Read;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common
{
	/// <summary>
	/// 
	/// </summary>
	internal class FileReader : Detail.FileReader<c_byte>
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FileReader() : base(new FileCursorTraitsFileData(), new FileCursorFileNameTraits<PathString>())
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FileReader(FileCursor other) : base(other.Traits_Type, other.FileName_Traits_Type)
		{
			other.CopyTo(this);
		}
	}
}
