/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.Exceptions
{
	/// <summary>
	/// Load error exception
	/// </summary>
	internal class ConfigErrorException : Exception
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ConfigErrorException(string msg) : base(msg)
		{
		}
	}
}
