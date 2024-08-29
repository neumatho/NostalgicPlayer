/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.C64.Cpu;

namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp.C64
{
	/// <summary>
	/// CPU emulator
	/// </summary>
	internal sealed class C64CpuBus : ICpuDataBus
	{
		public delegate void TestHookHandler(uint_least16_t addr, uint8_t data);

		private readonly Mmu mmu;

		private TestHookHandler testHook;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public C64CpuBus(Mmu mmu)
		{
			this.mmu = mmu;
			testHook = null;
		}



		/********************************************************************/
		/// <summary>
		/// Set hook for VICE tests
		/// </summary>
		/********************************************************************/
		public void SetTestHook(TestHookHandler handler)
		{
			testHook = handler;
		}

		#region ICpuDataBus implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint8_t CpuRead(uint_least16_t addr)
		{
			return mmu.CpuRead(addr);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void CpuWrite(uint_least16_t addr, uint8_t data)
		{
			if (testHook != null)
				testHook(addr, data);

			mmu.CpuWrite(addr, data);
		}
		#endregion
	}
}
