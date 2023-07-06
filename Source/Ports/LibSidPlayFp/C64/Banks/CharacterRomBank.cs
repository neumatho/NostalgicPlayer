/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp.C64.Banks
{
	/// <summary>
	/// Character ROM
	///
	/// Located at $D000-$DFFF
	/// </summary>
	internal sealed class CharacterRomBank : RomBank
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public CharacterRomBank() : base(0x1000)
		{
		}
	}
}
