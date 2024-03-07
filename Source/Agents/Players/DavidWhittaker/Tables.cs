/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DavidWhittaker
{
	/// <summary>
	/// Different tables needed
	/// </summary>
	internal static class Tables
	{
		/********************************************************************/
		/// <summary>
		/// Periods used by a real old player which is only used by QBall
		/// </summary>
		/********************************************************************/
		public static readonly ushort[] Periods1 =
		{
			                                                       256,  242,  228,
			 215,  203,  192,  181,  171,  161,  152,  144,  136
		};



		/********************************************************************/
		/// <summary>
		/// Periods used by first version of player
		/// </summary>
		/********************************************************************/
		public static readonly ushort[] Periods2 =
		{
			                                                      4096, 3864, 3648,
			3444, 3252, 3068, 2896, 2732, 2580, 2436, 2300, 2168, 2048, 1932, 1824,
			1722, 1626, 1534, 1448, 1366, 1290, 1218, 1150, 1084, 1024,  966,  912,
			 861,  813,  767,  724,  683,  645,  609,  575,  542,  512,  483,  456,
			 430,  406,  383,  362,  341,  322,  304,  287,  271,

			// These are not in the original player, but sometimes either arpeggio or transpose
			// go out of range and to made the module sound correctly, I have added these
			// extra periods
			                                                       256,  241,  228,
		};



		/********************************************************************/
		/// <summary>
		/// Periods used by newer version of player
		/// </summary>
		/********************************************************************/
		public static readonly ushort[] Periods3 =
		{
			                                                      8192, 7728, 7296,
			6888, 6504, 6136, 5792, 5464, 5160, 4872, 4600, 4336, 4096, 3864, 3648,
			3444, 3252, 3068, 2896, 2732, 2580, 2436, 2300, 2168, 2048, 1932, 1824,
			1722, 1626, 1534, 1448, 1366, 1290, 1218, 1150, 1084, 1024,  966,  912,
			 861,  813,  767,  724,  683,  645,  609,  575,  542,  512,  483,  456,
			 430,  406,  383,  362,  341,  322,  304,  287,  271,  256,  241,  228,
			 215,  203,  191,  181,  170,  161,  152,  143,  135
		};



		/********************************************************************/
		/// <summary>
		/// Holds an empty track. Used as fallback in some situations
		/// </summary>
		/********************************************************************/
		public static readonly byte[] EmptyTrack =
		{
			0x80
		};
	}
}
