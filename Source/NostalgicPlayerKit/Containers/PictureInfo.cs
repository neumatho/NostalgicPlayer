/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// This class holds information about a single picture stored in a module format
	/// </summary>
	public class PictureInfo
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PictureInfo(byte[] pictureData, string description)
		{
			PictureData = pictureData;
			Description = description;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the picture itself
		/// </summary>
		/********************************************************************/
		public byte[] PictureData
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds a description of the picture
		/// </summary>
		/********************************************************************/
		public string Description
		{
			get;
		}
	}
}
