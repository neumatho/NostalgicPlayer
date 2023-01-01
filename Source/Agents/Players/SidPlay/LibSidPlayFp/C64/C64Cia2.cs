/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Banks;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Cia;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64
{
	/// <summary>
	/// CIA 2
	///
	/// Generates NMIs
	/// </summary>
	internal class C64Cia2 : Mos652x, IBank
	{
		private readonly C64Env env;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public C64Cia2(C64Env env) : base(env.Scheduler())
		{
			this.env = env;
		}

		#region IBank implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Poke(uint_least16_t address, uint8_t value)
		{
			Write(SidEndian.Endian_16Lo8(address), value);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint8_t Peek(uint_least16_t address)
		{
			return Read(SidEndian.Endian_16Lo8(address));
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Signal interrupt
		/// </summary>
		/********************************************************************/
		public override void Interrupt(bool state)
		{
			if (state)
				env.InterruptNmi();
		}
		#endregion
	}
}
