/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Cpu;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64
{
	/// <summary>
	/// CPU emulator
	/// </summary>
	internal sealed class C64Cpu : Mos6510
	{
		public delegate void TestHookHandler(uint_least16_t addr, uint8_t data);

		private readonly C64Env env;

		private TestHookHandler testHook;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public C64Cpu(C64Env env) : base(env.Scheduler())
		{
			this.env = env;
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

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override uint8_t CpuRead(uint_least16_t addr)
		{
			return env.CpuRead(addr);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void CpuWrite(uint_least16_t addr, uint8_t data)
		{
			if (testHook != null)
				testHook(addr, data);

			env.CpuWrite(addr, data);
		}
		#endregion
	}
}
