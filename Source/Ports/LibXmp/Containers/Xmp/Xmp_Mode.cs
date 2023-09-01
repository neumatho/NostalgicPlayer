/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp
{
	/// <summary>
	/// Player modes
	/// </summary>
	public enum Xmp_Mode
	{
		/// <summary>
		/// Autodetect mode (default)
		/// </summary>
		Auto,

		/// <summary>
		/// Play as a generic MOD player
		/// </summary>
		Mod,

		/// <summary>
		/// Play using NoiseTracker quirks
		/// </summary>
		NoiseTracker,

		/// <summary>
		/// Play using ProTracker quirks
		/// </summary>
		ProTracker,

		/// <summary>
		/// Play as a generic S3M player
		/// </summary>
		S3M,

		/// <summary>
		/// Play using ST3 bug emulation
		/// </summary>
		St3,

		/// <summary>
		/// Play using ST3+GUS quirks
		/// </summary>
		St3Gus,

		/// <summary>
		/// Play as a generic XM player
		/// </summary>
		Xm,

		/// <summary>
		/// Play using FT2 bug emulation
		/// </summary>
		Ft2,

		/// <summary>
		/// Play using IT quirks
		/// </summary>
		It,

		/// <summary>
		/// Play using IT sample mode quirks
		/// </summary>
		ItSmp
	}
}
