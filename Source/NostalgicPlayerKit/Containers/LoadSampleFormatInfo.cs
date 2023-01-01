/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// Hold information about the sample loaded
	/// </summary>
	public class LoadSampleFormatInfo
	{
		/// <summary>
		/// The different flags that can be set for the sample
		/// </summary>
		[Flags]
		public enum SampleFlag
		{
			/// <summary>
			/// Nothing
			/// </summary>
			None = 0x00,

			/// <summary>
			/// The sample is looping
			/// </summary>
			Loop = 0x01
		}

		/********************************************************************/
		/// <summary>
		/// Holds the name of the sample
		/// </summary>
		/********************************************************************/
		public string Name
		{
			get; set;
		} = string.Empty;



		/********************************************************************/
		/// <summary>
		/// Holds the author of the sample
		/// </summary>
		/********************************************************************/
		public string Author
		{
			get; set;
		} = string.Empty;



		/********************************************************************/
		/// <summary>
		/// Holds some flags about this sample
		/// </summary>
		/********************************************************************/
		public SampleFlag Flags
		{
			get; set;
		} = SampleFlag.None;



		/********************************************************************/
		/// <summary>
		/// Holds the number of bits each sample is
		/// </summary>
		/********************************************************************/
		public int Bits
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the number of channels. This can either be 1 for mono or
		/// 2 for stereo
		/// </summary>
		/********************************************************************/
		public int Channels
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the sample frequency
		/// </summary>
		/********************************************************************/
		public int Frequency
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the start offset to the loop point in samples
		/// </summary>
		/********************************************************************/
		public long LoopStart
		{
			get; set;
		} = 0;



		/********************************************************************/
		/// <summary>
		/// Holds the length of the loop in samples
		/// </summary>
		/********************************************************************/
		public long LoopLength
		{
			get; set;
		} = 0;
	}
}
