/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibMpg123.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class Reader_Data
	{
		public int64_t FileLen;			// Total file length or total buffer size
		public int64_t FilePos;			// Position in file or position in buffer chain

		// Custom opaque I/O handle from the client
		public object IOHandle;
		public ReaderFlags Flags;

		// The one and only lowlevel reader wrapper, wrapping over all others.
		// This is either libmpg123's wrapper or directly the user-supplied functions
		public LibMpg123.R_Read_Delegate R_Read64;
		public LibMpg123.R_LSeek_Delegate R_LSeek64;
		public LibMpg123.Cleanup_Handle_Delegate Cleanup_Handle;

		public readonly BufferChain Buffer = new BufferChain();
	}
}
