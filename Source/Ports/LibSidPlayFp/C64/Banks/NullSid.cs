/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp.C64.Banks
{
	/// <summary>
	/// SID chip placeholder which does nothing and returns 0xff on reading
	/// </summary>
	internal sealed class NullSid : C64Sid
	{
		private static readonly NullSid nullSid = new NullSid();

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private NullSid()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Returns singleton instance
		/// </summary>
		/********************************************************************/
		public static NullSid GetInstance()
		{
			return nullSid;
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override void Reset(uint8_t volume)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void WriteReg(uint8_t addr, uint8_t data)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override uint8_t Read(uint8_t addr)
		{
			return 0xff;
		}
		#endregion
	}
}
