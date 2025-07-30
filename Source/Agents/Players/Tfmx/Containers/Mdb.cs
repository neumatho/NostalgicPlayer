/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.Tfmx.Containers
{
	/// <summary>
	/// </summary>
	internal class Mdb : IDeepCloneable<Mdb>
	{
		public bool PlayerEnable { get; set; }
		public bool EndFlag { get; set; }
		public ushort SpeedCnt { get; set; }
		public ushort CiaSave { get; set; }
		public bool PlayPattFlag { get; set; }
		public sbyte MasterVol { get; set; }
		public sbyte FadeDest { get; set; }
		public sbyte FadeTime { get; set; }
		public sbyte FadeReset { get; set; }
		public sbyte FadeSlope { get; set; }
		public short TrackLoop { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Mdb MakeDeepClone()
		{
			return (Mdb)MemberwiseClone();
		}
	}
}
