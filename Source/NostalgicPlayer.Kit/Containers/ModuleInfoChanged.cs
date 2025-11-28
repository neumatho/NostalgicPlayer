/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// Holding needed information when some module information has changed
	/// </summary>
	public class ModuleInfoChanged
	{
		/// <summary>
		/// Use this if module name has changed
		/// </summary>
		public const int ModuleNameChanged = -1;

		/// <summary>
		/// Use this if author has changed
		/// </summary>
		public const int AuthorChanged = -2;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleInfoChanged(int line, string newValue)
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
