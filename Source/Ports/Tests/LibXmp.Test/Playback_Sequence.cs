/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test
{
	/// <summary>
	/// 
	/// </summary>
	internal class Playback_Sequence
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Playback_Sequence(Playback_Action action, c_int value, c_int result)
		{
			Action = action;
			Value = value;
			Result = result;
		}

		public Playback_Action Action;
		public c_int Value;
		public c_int Result;
	}
}
