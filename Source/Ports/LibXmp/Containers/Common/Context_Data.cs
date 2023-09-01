/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common
{
	/// <summary>
	/// 
	/// </summary>
	internal class Context_Data
	{
		public Player_Data P = new Player_Data();
		public Mixer_Data S = new Mixer_Data();
		public Module_Data M = new Module_Data();
		public Xmp_State State;
	}
}
