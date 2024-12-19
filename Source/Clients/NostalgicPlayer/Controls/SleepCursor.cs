/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Controls
{
	/// <summary>
	/// Smart control to make a sleep cursor when initiated via using()
	/// </summary>
	public class SleepCursor : Component
	{
		private readonly Cursor oldCursor;

		private static Lock myLock = new Lock();
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
					Cursor.Current = oldCursor;
			}
		}
	}
}
