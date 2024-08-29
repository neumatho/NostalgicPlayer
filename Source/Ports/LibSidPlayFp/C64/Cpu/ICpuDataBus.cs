/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp.C64.Cpu
{
	internal interface ICpuDataBus
	{
		/// <summary>
		/// 
		/// </summary>
		public uint8_t CpuRead(uint_least16_t addr);

		/// <summary>
		/// 
		/// </summary>
		public void CpuWrite(uint_least16_t addr, uint8_t data);
	}
}
