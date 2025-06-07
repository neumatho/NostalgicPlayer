/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.GuiKit.Extensions
{
	/// <summary>
	/// Extension methods to the long value
	/// </summary>
	public static class ValueExtension
	{
		/********************************************************************/
		/// <summary>
		/// Convert the number to a beautified string
		/// </summary>
		/********************************************************************/
		public static string ToBeautifiedString(this long value)
		{
			return $"{value:n0}";
		}



		/********************************************************************/
		/// <summary>
		/// Convert the number to a beautified string
		/// </summary>
		/********************************************************************/
		public static string ToBeautifiedString(this int value)
		{
			return $"{value:n0}";
		}
	}
}
