/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Drawing;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Images
{
	/// <summary>
	/// Holds all the images needed by the Main window
	/// </summary>
	internal class MainImages : ThemedImageBase, IMainImages
	{
		private const string Category = "Main";

		private readonly Dictionary<Color, Bitmap> playingItems;

		private Bitmap information;

		private Bitmap mute;

		private Bitmap add;
		private Bitmap remove;
		private Bitmap swap;
		private Bitmap sort;
		private Bitmap moveUp;
		private Bitmap moveDown;
		private Bitmap list;
		private Bitmap disk;

		private Bitmap previousModule;
		private Bitmap nextModule;
		private Bitmap previousSong;
		private Bitmap nextSong;
		private Bitmap fastRewind;
		private Bitmap fastForward;
		private Bitmap play;
		private Bitmap eject;
		private Bitmap pause;

		private Bitmap loop;
		private Bitmap favorites;
		private Bitmap equalizer;
		private Bitmap samples;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public MainImages(IThemeManager themeManager) : base(themeManager)
		{
			playingItems = new Dictionary<Color, Bitmap>();
		}



		/********************************************************************/
		/// <summary>
		/// Dispose all the images
		/// </summary>
		/********************************************************************/
		public override void Dispose()
		{
			base.Dispose();

			FlushImages();
		}



		/********************************************************************/
		/// <summary>
		/// Flush images
		/// </summary>
		/********************************************************************/
		protected override void FlushImages()
		{
			FlushPlayingItems();

			information?.Dispose();
			information = null;

			mute?.Dispose();
			mute = null;

			add?.Dispose();
			add = null;
			remove?.Dispose();
			remove = null;
			swap?.Dispose();
			swap = null;
			sort?.Dispose();
			sort = null;
			moveUp?.Dispose();
			moveUp = null;
			moveDown?.Dispose();
			moveDown = null;
			list?.Dispose();
			list = null;
			disk?.Dispose();
			disk = null;

			previousModule?.Dispose();
			previousModule = null;
			nextModule?.Dispose();
			nextModule = null;
			previousSong?.Dispose();
			previousSong = null;
			nextSong?.Dispose();
			nextSong = null;
			fastRewind?.Dispose();
			fastRewind = null;
			fastForward?.Dispose();
			fastForward = null;
			play?.Dispose();
			play = null;
			eject?.Dispose();
			eject = null;
			pause?.Dispose();
			pause = null;

			loop?.Dispose();
			loop = null;
			favorites?.Dispose();
			favorites = null;
			equalizer?.Dispose();
			equalizer = null;
			samples?.Dispose();
			samples = null;
		}



		/********************************************************************/
		/// <summary>
		/// Gets the playing item image
		/// </summary>
		/********************************************************************/
		public Bitmap GetPlayingItem(Color color)
		{
			if (!playingItems.TryGetValue(color, out Bitmap bitmap))
			{
				if (playingItems.Count == 2)
					FlushPlayingItems();

				bitmap = GetSvgBitmap(Category, "PlayingItem", color, 10, 10);
				playingItems.Add(color, bitmap);
			}

			return bitmap;
		}



		/********************************************************************/
		/// <summary>
		/// Gets the information image
		/// </summary>
		/********************************************************************/
		public Bitmap Information
		{
			get
			{
				if (information == null)
					information = GetSvgBitmap(Category, "Information", CurrentColors.InformationColor, 20, 20);

				return information;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the mute image
		/// </summary>
		/********************************************************************/
		public Bitmap Mute
		{
			get
			{
				if (mute == null)
					mute = GetSvgBitmap(Category, "Mute", CurrentColors.MuteColor, 20, 20);

				return mute;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the add image
		/// </summary>
		/********************************************************************/
		public Bitmap Add
		{
			get
			{
				if (add == null)
					add = GetSvgBitmap(Category, "Add", CurrentColors.AddColor, 20, 20);

				return add;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the remove image
		/// </summary>
		/********************************************************************/
		public Bitmap Remove
		{
			get
			{
				if (remove == null)
					remove = GetSvgBitmap(Category, "Remove", CurrentColors.RemoveColor, 20, 20);

				return remove;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the swap image
		/// </summary>
		/********************************************************************/
		public Bitmap Swap
		{
			get
			{
				if (swap == null)
					swap = GetSvgBitmap(Category, "Swap", CurrentColors.SwapColor, 20, 20);

				return swap;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the sort image
		/// </summary>
		/********************************************************************/
		public Bitmap Sort
		{
			get
			{
				if (sort == null)
					sort = GetSvgBitmap(Category, "Sort", CurrentColors.SortColor, 20, 20);

				return sort;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the move up image
		/// </summary>
		/********************************************************************/
		public Bitmap MoveUp
		{
			get
			{
				if (moveUp == null)
					moveUp = GetSvgBitmap(Category, "MoveUp", CurrentColors.MoveUpColor, 20, 20);

				return moveUp;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the move down image
		/// </summary>
		/********************************************************************/
		public Bitmap MoveDown
		{
			get
			{
				if (moveDown == null)
					moveDown = GetSvgBitmap(Category, "MoveDown", CurrentColors.MoveDownColor, 20, 20);

				return moveDown;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the list image
		/// </summary>
		/********************************************************************/
		public Bitmap List
		{
			get
			{
				if (list == null)
					list = GetSvgBitmap(Category, "List", CurrentColors.ListColor, 20, 20);

				return list;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the disk image
		/// </summary>
		/********************************************************************/
		public Bitmap Disk
		{
			get
			{
				if (disk == null)
					disk = GetSvgBitmap(Category, "Disk", CurrentColors.DiskColor, 20, 20);

				return disk;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the previous module image
		/// </summary>
		/********************************************************************/
		public Bitmap PreviousModule
		{
			get
			{
				if (previousModule == null)
					previousModule = GetSvgBitmap(Category, "PreviousModule", CurrentColors.PreviousModuleColor, 20, 20);

				return previousModule;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the next module image
		/// </summary>
		/********************************************************************/
		public Bitmap NextModule
		{
			get
			{
				if (nextModule == null)
					nextModule = GetSvgBitmap(Category, "NextModule", CurrentColors.NextModuleColor, 20, 20);

				return nextModule;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the previous song image
		/// </summary>
		/********************************************************************/
		public Bitmap PreviousSong
		{
			get
			{
				if (previousSong == null)
					previousSong = GetSvgBitmap(Category, "PreviousSong", CurrentColors.PreviousSongColor, 20, 20);

				return previousSong;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the next song image
		/// </summary>
		/********************************************************************/
		public Bitmap NextSong
		{
			get
			{
				if (nextSong == null)
					nextSong = GetSvgBitmap(Category, "NextSong", CurrentColors.NextSongColor, 20, 20);

				return nextSong;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the fast rewind image
		/// </summary>
		/********************************************************************/
		public Bitmap FastRewind
		{
			get
			{
				if (fastRewind == null)
					fastRewind = GetSvgBitmap(Category, "FastRewind", CurrentColors.FastRewindColor, 20, 20);

				return fastRewind;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the fast forward image
		/// </summary>
		/********************************************************************/
		public Bitmap FastForward
		{
			get
			{
				if (fastForward == null)
					fastForward = GetSvgBitmap(Category, "FastForward", CurrentColors.FastForwardColor, 20, 20);

				return fastForward;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the play image
		/// </summary>
		/********************************************************************/
		public Bitmap Play
		{
			get
			{
				if (play == null)
					play = GetSvgBitmap(Category, "Play", CurrentColors.PlayColor, 20, 20);

				return play;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the eject image
		/// </summary>
		/********************************************************************/
		public Bitmap Eject
		{
			get
			{
				if (eject == null)
					eject = GetSvgBitmap(Category, "Eject", CurrentColors.EjectColor, 20, 20);

				return eject;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the pause image
		/// </summary>
		/********************************************************************/
		public Bitmap Pause
		{
			get
			{
				if (pause == null)
					pause = GetSvgBitmap(Category, "Pause", CurrentColors.PauseColor, 20, 20);

				return pause;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the loop image
		/// </summary>
		/********************************************************************/
		public Bitmap Loop
		{
			get
			{
				if (loop == null)
					loop = GetSvgBitmap(Category, "Loop", CurrentColors.LoopColor, 20, 20);

				return loop;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the favorites image
		/// </summary>
		/********************************************************************/
		public Bitmap Favorites
		{
			get
			{
				if (favorites == null)
					favorites = GetSvgBitmap(Category, "Favorites", CurrentColors.FavoritesColor, 20, 20);

				return favorites;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the equalizer image
		/// </summary>
		/********************************************************************/
		public Bitmap Equalizer
		{
			get
			{
				if (equalizer == null)
					equalizer = GetSvgBitmap(Category, "Equalizer", CurrentColors.EqualizerColor, 20, 20);

				return equalizer;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the samples image
		/// </summary>
		/********************************************************************/
		public Bitmap Samples
		{
			get
			{
				if (samples == null)
					samples = GetSvgBitmap(Category, "Samples", CurrentColors.SamplesColor, 20, 20);

				return samples;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Flush all playing item bitmaps
		/// </summary>
		/********************************************************************/
		private void FlushPlayingItems()
		{
			foreach (Bitmap bitmap in playingItems.Values)
				bitmap.Dispose();

			playingItems.Clear();
		}
	}
}
