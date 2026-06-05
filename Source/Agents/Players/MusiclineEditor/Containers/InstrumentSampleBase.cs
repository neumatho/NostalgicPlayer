/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers
{
	/// <summary>
	/// Common information for both instruments and samples
	/// </summary>
	internal abstract class InstrumentSampleBase
	{
		/// <summary>
		/// 
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public SampleType SampleType { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public SamplePointer? SamplePointer { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort SampleLength { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public SamplePointer? SampleRepeatPointer { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort SampleRepeatLength { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public short FineTune { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public short SemiTone { get; set; }
	}
}
