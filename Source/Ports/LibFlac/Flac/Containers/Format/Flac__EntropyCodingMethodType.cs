/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format
{
	/// <summary>
	/// An enumeration of the available entropy coding methods
	/// </summary>
	public enum Flac__EntropyCodingMethodType
	{
		/// <summary>
		/// Residual is coded by partitioning into contexts, each with it's own 4-bit rice parameter
		/// </summary>
		Partitioned_Rice = 0,

		/// <summary>
		/// Residual is coded by partitioning into contexts, each with it's own 5-bit rice parameter
		/// </summary>
		Partitioned_Rice2 = 1,
	}
}
