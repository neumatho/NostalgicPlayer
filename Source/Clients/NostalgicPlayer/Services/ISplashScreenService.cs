/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.MainWindow;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Services
{
	/// <summary>
	/// Service that handles the creation of the main form with splash screen
	/// </summary>
	public interface ISplashScreenService
	{
		/// <summary>
		/// Create the main form with splash screen showing progress
		/// </summary>
		Form CreateMainForm();
	}
}
