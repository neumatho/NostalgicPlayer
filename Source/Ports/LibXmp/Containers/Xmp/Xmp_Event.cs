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
	public class Xmp_Event
	{
		/// <summary>
		/// Note number (0 means no note)
		/// </summary>
		public byte Note;

		/// <summary>
		/// Patch number
		/// </summary>
		public byte Ins;

		/// <summary>
		/// Volume (0 to basevol)
		/// </summary>
		public byte Vol;

		/// <summary>
		/// Effect type
		/// </summary>
		public byte FxT;

		/// <summary>
		/// Effect parameter
		/// </summary>
		public byte FxP;

		/// <summary>
		/// Secondary effect type
		/// </summary>
		public byte F2T;

		/// <summary>
		/// Secondary effect parameter
		/// </summary>
		public byte F2P;

		/// <summary>
		/// Internal (reserved) flags
		/// </summary>
		public byte _Flag;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			Note = 0;
			Ins = 0;
			Vol = 0;
			FxT = 0;
			FxP = 0;
			F2T = 0;
			F2P = 0;
			_Flag = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void CopyFrom(Xmp_Event other)
		{
			Note = other.Note;
			Ins = other.Ins;
			Vol = other.Vol;
			FxT = other.FxT;
			FxP = other.FxP;
			F2T = other.F2T;
			F2P = other.F2P;
			_Flag = other._Flag;
		}
	}
}
