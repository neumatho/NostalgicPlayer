/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class MidiMacros
	{
		/// <summary>
		/// Number of global macros
		/// </summary>
		public const c_int GlobalMacros = 9;

		/// <summary>
		/// Number of parametered macros
		/// </summary>
		public const c_int SFxMacros = 16;

		/// <summary>
		/// Number of fixed macros
		/// </summary>
		public const c_int ZxxMacros = 128;

		/// <summary>
		/// Max number of chars per macro
		/// </summary>
		public const c_int MacroLength = 32;
	}
}
