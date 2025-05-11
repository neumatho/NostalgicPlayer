/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp.C64.Banks
{
	/// <summary>
	/// SID
	///
	/// Located at $D400-$D7FF, mirrored each 32 bytes
	/// </summary>
	internal sealed class SidBank : IBank
	{
		/// <summary>
		/// SID chip
		/// </summary>
		private C64Sid sid;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SidBank()
		{
			sid = NullSid.GetInstance();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			sid.Reset();
		}



		/********************************************************************/
		/// <summary>
		/// Set SID emulation
		/// </summary>
		/********************************************************************/
		public void SetSid(C64Sid s)
		{
			sid = s != null ? s : NullSid.GetInstance();
		}

		#region IBank implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint8_t Peek(uint_least16_t addr)
		{
			return sid.Peek(addr);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Poke(uint_least16_t addr, uint8_t data)
		{
			sid.Poke(addr, data);
		}
		#endregion
	}
}
