/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Loaders
{
	/// <summary>
	/// All format loaders derives from this interface
	/// </summary>
	internal interface IFormatLoader
	{
		c_int Test(Hio f, out string t, c_int start);
		c_int Loader(Module_Data m, Hio f, c_int start);
	}
}
