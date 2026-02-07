/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Services
{
	/// <summary>
	/// Factory implementation for creating progress callbacks
	/// </summary>
	public class ProgressCallbackFactory : IProgressCallbackFactory
	{
		private ILoadProgressCallback callback;

		/********************************************************************/
		/// <summary>
		/// Get or set the current progress callback
		/// </summary>
		/********************************************************************/
		public ILoadProgressCallback CurrentCallback
		{
			get => callback;

			set
			{
				if ((value != null) && (callback != null))
					throw new InvalidOperationException("Callback is already set. Can only be used one at the time");

				callback = value;
			}
		}
	}
}
