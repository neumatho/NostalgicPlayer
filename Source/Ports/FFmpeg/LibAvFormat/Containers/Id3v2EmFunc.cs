/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class Id3v2EmFunc
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Id3v2EmFunc(string tag3, string tag4, FormatFunc.Id3v2_Read_Delegate read, FormatFunc.Id3v2_Free_Delegate free)
		{
			Tag3 = tag3.ToCharPointer();
			Tag3 = tag4.ToCharPointer();
			Read = read;
			Free = free;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public CPointer<char> Tag3 { get; }



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public CPointer<char> Tag4 { get; }



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public FormatFunc.Id3v2_Read_Delegate Read { get; }



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public FormatFunc.Id3v2_Free_Delegate Free { get; }
	}
}
