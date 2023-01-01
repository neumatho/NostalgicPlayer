/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Common
{
	/// <summary>
	/// </summary>
	internal class HuffmanCode<T>
	{
		public uint Length;
		public uint Code;

		public T Value;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public HuffmanCode(uint length, uint code, T value)
		{
			Length = length;
			Code = code;
			Value = value;
		}
	}
}
