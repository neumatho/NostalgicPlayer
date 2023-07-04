/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format
{
	/// <summary>
	/// Header for the entropy coding method
	/// </summary>
	public class Flac__EntropyCodingMethod
	{
		/// <summary></summary>
		public Flac__EntropyCodingMethodType Type;
		/// <summary></summary>
		public IEntropyCodingMethod Data;
	}
}
