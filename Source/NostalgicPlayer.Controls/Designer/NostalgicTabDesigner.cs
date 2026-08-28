/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Windows.Forms.Design;

namespace Polycode.NostalgicPlayer.Controls.Designer
{
	/// <summary>
	/// Designer for NostalgicTab.
	///
	/// The .NET out-of-process WinForms designer only instantiates designers
	/// built against the Microsoft.WinForms.Designer.SDK, so this designer is
	/// never created and has no behavior of its own. It is still referenced
	/// by NostalgicTab's [Designer] attribute on purpose: that attribute
	/// shadows the framework's TabControlDesigner, which would otherwise add
	/// "Add Tab"/"Remove Tab" verbs that create plain TabPage objects instead
	/// of NostalgicTabPage. Pages are added through the Pages collection
	/// editor, and tabs are switched in the designer with the SelectedPage
	/// property instead
	/// </summary>
	internal class NostalgicTabDesigner : ParentControlDesigner
	{
	}
}
