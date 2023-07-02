/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.Ancient.Common
{
	/// <summary>
	/// </summary>
	internal class HuffmanCode<T>
	{
		public uint32_t Length;
		public uint32_t Code;

		public T Value;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public HuffmanCode(uint32_t length, uint32_t code, T value)
		{
			Length = length;
			Code = code;
			Value = value;
		}
	}
}
