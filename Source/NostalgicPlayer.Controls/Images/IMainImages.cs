/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Images
{
	/// <summary>
	/// Holds all the images needed by the Main window
	/// </summary>
	public interface IMainImages
	{
		/// <summary>
		/// Gets the playing item image
		/// </summary>
		Bitmap GetPlayingItem(Color color);

		/// <summary>
		/// Gets the information image
		/// </summary>
		Bitmap Information { get; }

		/// <summary>
		/// Gets the mute image
		/// </summary>
		Bitmap Mute { get; }

		/// <summary>
		/// Gets the add image
		/// </summary>
		Bitmap Add { get; }

		/// <summary>
		/// Gets the remove image
		/// </summary>
		Bitmap Remove { get; }

		/// <summary>
		/// Gets the swap image
		/// </summary>
		Bitmap Swap { get; }

		/// <summary>
		/// Gets the sort image
		/// </summary>
		Bitmap Sort { get; }

		/// <summary>
		/// Gets the move up image
		/// </summary>
		Bitmap MoveUp { get; }

		/// <summary>
		/// Gets the move down image
		/// </summary>
		Bitmap MoveDown { get; }

		/// <summary>
		/// Gets the list image
		/// </summary>
		Bitmap List { get; }

		/// <summary>
		/// Gets the disk image
		/// </summary>
		Bitmap Disk { get; }

		/// <summary>
		/// Gets the previous module image
		/// </summary>
		Bitmap PreviousModule { get; }

		/// <summary>
		/// Gets the next module image
		/// </summary>
		Bitmap NextModule { get; }

		/// <summary>
		/// Gets the previous song image
		/// </summary>
		Bitmap PreviousSong { get; }

		/// <summary>
		/// Gets the next song image
		/// </summary>
		Bitmap NextSong { get; }

		/// <summary>
		/// Gets the fast rewind image
		/// </summary>
		Bitmap FastRewind { get; }

		/// <summary>
		/// Gets the fast forward image
		/// </summary>
		Bitmap FastForward { get; }

		/// <summary>
		/// Gets the play image
		/// </summary>
		Bitmap Play { get; }

		/// <summary>
		/// Gets the eject image
		/// </summary>
		Bitmap Eject { get; }

		/// <summary>
		/// Gets the pause image
		/// </summary>
		Bitmap Pause { get; }

		/// <summary>
		/// Gets the loop image
		/// </summary>
		Bitmap Loop { get; }

		/// <summary>
		/// Gets the favorites image
		/// </summary>
		Bitmap Favorites { get; }

		/// <summary>
		/// Gets the equalizer image
		/// </summary>
		Bitmap Equalizer { get; }

		/// <summary>
		/// Gets the samples image
		/// </summary>
		Bitmap Samples { get; }
	}
}
