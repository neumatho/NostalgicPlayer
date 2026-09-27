/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io_Read;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io
{
	/// <summary>
	/// 
	/// </summary>
	internal interface ITraits
	{
		IFileData Get_Shared(IFileData data);
		IFileData Get_Ref(IFileData data);
		IFileData Make_Data();
		IFileData Make_Data(byte_span data);
		IFileData Make_Chunk(IFileData data, size_t position, size_t size);
	}
}
