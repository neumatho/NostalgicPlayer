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
		public Player_Data P { get; set; } = new Player_Data();
		public Mixer_Data S { get; } = new Mixer_Data();
		public Module_Data M { get; } = new Module_Data();
		public Rng_State Rng { get; } = new Rng_State();
		public Xmp_State State { get; set; }
	}
}
