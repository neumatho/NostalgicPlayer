/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// Tell the format to save a sample in
	/// </summary>
	public class SaveSampleFormatInfo
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SaveSampleFormatInfo(byte bits, int channels, uint frequency, long loopStart, long loopLength, string name, string author)
		{
			Bits = bits;
			Channels = channels;
			Frequency = frequency;
			LoopStart = loopStart;
			LoopLength = loopLength;
			Name = name;
			Author = author;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the number of bits each sample is
		/// </summary>
		/********************************************************************/
		public byte Bits
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the number of channels. This can either be 1 for mono or
		/// 2 for stereo
		/// </summary>
		/********************************************************************/
		public int Channels
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the sample frequency
		/// </summary>
		/********************************************************************/
		public uint Frequency
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the start offset to the loop point in samples
		/// </summary>
		/********************************************************************/
		public long LoopStart
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the length of the loop in samples
		/// </summary>
		/********************************************************************/
		public long LoopLength
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the name of the sample
		/// </summary>
		/********************************************************************/
		public string Name
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the author of the sample
		/// </summary>
		/********************************************************************/
		public string Author
		{
			get;
		}
	}
}
