/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Controls.Types;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces
{
	/// <summary>
	/// Controls derive from this interface can use image bank images
	/// </summary>
	internal interface IImageBank
	{
		ImageBankArea ImageArea { get; }
	}
}
