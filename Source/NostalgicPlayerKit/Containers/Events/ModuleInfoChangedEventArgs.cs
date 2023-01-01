/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Containers.Events
{
	/// <summary>
	/// </summary>
	public delegate void ModuleInfoChangedEventHandler(object sender, ModuleInfoChangedEventArgs e);

	/// <summary>
	/// Event class holding needed information when sending an update event
	/// </summary>
	public class ModuleInfoChangedEventArgs : EventArgs
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleInfoChangedEventArgs(int line, string newValue)
		{
			Line = line;
			Value = newValue;
		}



		/********************************************************************/
		/// <summary>
		/// Holding the line that need to be updated
		/// </summary>
		/********************************************************************/
		public int Line
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holding the new value
		/// </summary>
		/********************************************************************/
		public string Value
		{
			get;
		}
	}
}
