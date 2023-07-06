/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.ReSidFp.Exceptions
{
	/// <summary>
	/// 
	/// </summary>
	public class SidErrorException : Exception
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SidErrorException(string msg) : base(msg)
		{
		}
	}
}
