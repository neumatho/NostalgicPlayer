/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp
{
	/// <summary>
	/// 
	/// </summary>
	public class Xmp_Channel_Info
	{
		/// <summary>
		/// Sample period (* 4096)
		/// </summary>
		public c_uint Period { get; internal set; }

		/// <summary>
		/// Sample position
		/// </summary>
		public c_uint Position { get; internal set; }

		/// <summary>
		/// Linear bend from base note
		/// </summary>
		public c_short PitchBend { get; internal set; }

		/// <summary>
		/// Current base note number
		/// </summary>
		public byte Note { get; internal set; }

		/// <summary>
		/// Current instrument number
		/// </summary>
		public byte Instrument { get; internal set; }

		/// <summary>
		/// Current sample number
		/// </summary>
		public byte Sample { get; internal set; }

		/// <summary>
		/// Current volume
		/// </summary>
		public byte Volume { get; internal set; }

		/// <summary>
		/// Current stereo pan
		/// </summary>
		public byte Pan { get; internal set; }

		/// <summary>
		/// Reserved
		/// </summary>
		public byte Reserved { get; internal set; }

		/// <summary>
		/// Current track event
		/// </summary>
		public Xmp_Event Event { get; } = new Xmp_Event();
	}
}
