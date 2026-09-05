/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Exceptions
{
	/// <summary>
	/// Base class used for all exceptions that are thrown by libopenmpt itself
	/// </summary>
	public class OpenMptException : Exception
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public OpenMptException(string text_) : base(text_)
		{
		}
	}
}
