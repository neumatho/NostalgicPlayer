/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibReSidFp
{
	/// <summary>
	/// Potentiometer representation.
	///
	/// This class will probably never be implemented in any real way
	/// </summary>
	internal class Potentiometer
	{
		/********************************************************************/
		/// <summary>
		/// Read paddle value. Not modeled
		/// </summary>
		/********************************************************************/
		public byte ReadPot()
		{
			return 0xff;
		}
	}
}
