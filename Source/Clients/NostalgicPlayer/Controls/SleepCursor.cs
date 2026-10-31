/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Platform.Native;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Controls
{
	/// <summary>
	/// Smart control to make a sleep cursor when initiated via using()
	/// </summary>
	public class SleepCursor : Component
	{
		private readonly Cursor oldCursor;

		private static readonly Lock myLock = new Lock();
		private static int counter = 0;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SleepCursor()
		{
			lock (myLock)
			{
				if (counter == 0)
					oldCursor = Cursor.Current;

				Cursor.Current = Cursors.WaitCursor;
				counter++;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Dispose the object
		/// </summary>
		/********************************************************************/
		protected override void Dispose(bool disposing)
		{
			lock (myLock)
			{
				counter--;
				if (counter == 0)
				{
					FlushInputMessages();
					Cursor.Current = oldCursor;
				}
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Remove all mouse and keyboard messages that have been queued
		/// while the sleep cursor was shown, so they will not be executed
		/// afterwards
		/// </summary>
		/********************************************************************/
		private static void FlushInputMessages()
		{
			while (User32.PeekMessageW(out _, IntPtr.Zero, WM.MOUSEFIRST, WM.MOUSELAST, PM.REMOVE))
			{
			}

			while (User32.PeekMessageW(out _, IntPtr.Zero, WM.NCMOUSEFIRST, WM.NCMOUSELAST, PM.REMOVE))
			{
			}

			while (User32.PeekMessageW(out _, IntPtr.Zero, WM.KEYFIRST, WM.KEYLAST, PM.REMOVE))
			{
			}
		}
		#endregion
	}
}
