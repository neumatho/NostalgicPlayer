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
		public c_uint Period;

		/// <summary>
		/// Sample position
		/// </summary>
		public c_uint Position;

		/// <summary>
		/// Linear bend from base note
		/// </summary>
		public c_short PitchBend;

		/// <summary>
		/// Current base note number
		/// </summary>
		public byte Note;

		/// <summary>
		/// Current instrument number
		/// </summary>
		public byte Instrument;

		/// <summary>
		/// Current sample number
		/// </summary>
		public byte Sample;

		/// <summary>
		/// Current volume
		/// </summary>
		public byte Volume;

		/// <summary>
		/// Current stereo pan
		/// </summary>
		public byte Pan;

		/// <summary>
		/// Reserved
		/// </summary>
		public byte Reserved;

		/// <summary>
		/// Current track event
		/// </summary>
		public readonly Xmp_Event Event = new Xmp_Event();
	}
}
