/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Iff
{
	/// <summary>
	/// 
	/// </summary>
	internal class Iff_Info
	{
		public delegate c_int Loader_Delegate(Module_Data m, c_int size, Hio f, object parm);

		public byte[] Id { get; } = new byte[4];
		public Loader_Delegate Loader { get; set; }
	}
}
