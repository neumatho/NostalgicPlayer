/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Crc
{
	/// <summary>
	/// 
	/// </summary>
	internal interface ICrc
	{
		void ProcessByte(byte @byte);
		ulong Result();
		ICrc Process(c_byte c);
	}
}
