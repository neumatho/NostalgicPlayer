/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
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

		private ImageColorBitmapCache playing;

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

		private ImageColorBitmapCache file;
		private ImageColorBitmapCache directory;
		private ImageColorBitmapCache az;
		private ImageColorBitmapCache za;
		private ImageColorBitmapCache shuffle;
		private ImageColorBitmapCache setSubSong;
		private ImageColorBitmapCache clearSubSong;
		private ImageColorBitmapCache load;
		private ImageColorBitmapCache save;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public MainImages(IThemeManager themeManager) : base(themeManager)
		{
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
			playing?.Dispose();
			playing = null;

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

			file?.Dispose();
			file = null;
			directory?.Dispose();
			directory = null;
			az?.Dispose();
			az = null;
			za?.Dispose();
			za = null;
			shuffle?.Dispose();
			shuffle = null;
			setSubSong?.Dispose();
			setSubSong = null;
			clearSubSong?.Dispose();
			clearSubSong = null;
			load?.Dispose();
			load = null;
			save?.Dispose();
			save = null;
		}



		/********************************************************************/
		/// <summary>
		/// Gets the playing item image
		/// </summary>
		/********************************************************************/
		public Bitmap PlayingItem(Color color)
		{
			if (playing == null)
				playing = new ImageColorBitmapCache(Category, nameof(IMainImages.PlayingItem), 10, 10);

			return playing.GetBitmap(color);
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
					information = GetSvgBitmap(Category, nameof(IMainImages.Information), CurrentColors.InformationColor, 20, 20);

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
					mute = GetSvgBitmap(Category, nameof(IMainImages.Mute), CurrentColors.MuteColor, 20, 20);

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
					add = GetSvgBitmap(Category, nameof(IMainImages.Add), CurrentColors.AddColor, 20, 20);

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
					remove = GetSvgBitmap(Category, nameof(IMainImages.Remove), CurrentColors.RemoveColor, 20, 20);

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
					swap = GetSvgBitmap(Category, nameof(IMainImages.Swap), CurrentColors.SwapColor, 20, 20);

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
					sort = GetSvgBitmap(Category, nameof(IMainImages.Sort), CurrentColors.SortColor, 20, 20);

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
					moveUp = GetSvgBitmap(Category, nameof(IMainImages.MoveUp), CurrentColors.MoveUpColor, 20, 20);

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
					moveDown = GetSvgBitmap(Category, nameof(IMainImages.MoveDown), CurrentColors.MoveDownColor, 20, 20);

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
					list = GetSvgBitmap(Category, nameof(IMainImages.List), CurrentColors.ListColor, 20, 20);

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
					disk = GetSvgBitmap(Category, nameof(IMainImages.Disk), CurrentColors.DiskColor, 20, 20);

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
					previousModule = GetSvgBitmap(Category, nameof(IMainImages.PreviousModule), CurrentColors.PreviousModuleColor, 20, 20);

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
					nextModule = GetSvgBitmap(Category, nameof(IMainImages.NextModule), CurrentColors.NextModuleColor, 20, 20);

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
					previousSong = GetSvgBitmap(Category, nameof(IMainImages.PreviousSong), CurrentColors.PreviousSongColor, 20, 20);

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
					nextSong = GetSvgBitmap(Category, nameof(IMainImages.NextSong), CurrentColors.NextSongColor, 20, 20);

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
					fastRewind = GetSvgBitmap(Category, nameof(IMainImages.FastRewind), CurrentColors.FastRewindColor, 20, 20);

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
					fastForward = GetSvgBitmap(Category, nameof(IMainImages.FastForward), CurrentColors.FastForwardColor, 20, 20);

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
					play = GetSvgBitmap(Category, nameof(IMainImages.Play), CurrentColors.PlayColor, 20, 20);

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
					eject = GetSvgBitmap(Category, nameof(IMainImages.Eject), CurrentColors.EjectColor, 20, 20);

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
					pause = GetSvgBitmap(Category, nameof(IMainImages.Pause), CurrentColors.PauseColor, 20, 20);

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
					loop = GetSvgBitmap(Category, nameof(IMainImages.Loop), CurrentColors.LoopColor, 20, 20);

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
					favorites = GetSvgBitmap(Category, nameof(IMainImages.Favorites), CurrentColors.FavoritesColor, 20, 20);

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
					equalizer = GetSvgBitmap(Category, nameof(IMainImages.Equalizer), CurrentColors.EqualizerColor, 20, 20);

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
					samples = GetSvgBitmap(Category, nameof(IMainImages.Samples), CurrentColors.SamplesColor, 20, 20);

				return samples;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the file image
		/// </summary>
		/********************************************************************/
		public Bitmap File(Color color)
		{
			if (file == null)
				file = new ImageColorBitmapCache(Category, nameof(IMainImages.File), 20, 20);

			return file.GetBitmap(color);
		}



		/********************************************************************/
		/// <summary>
		/// Gets the directory image
		/// </summary>
		/********************************************************************/
		public Bitmap Directory(Color color)
		{
			if (directory == null)
				directory = new ImageColorBitmapCache(Category, nameof(IMainImages.Directory), 20, 20);

			return directory.GetBitmap(color);
		}



		/********************************************************************/
		/// <summary>
		/// Gets the A-Z sorting image
		/// </summary>
		/********************************************************************/
		public Bitmap AZ(Color color)
		{
			if (az == null)
				az = new ImageColorBitmapCache(Category, nameof(IMainImages.AZ), 20, 20);

			return az.GetBitmap(color);
		}



		/********************************************************************/
		/// <summary>
		/// Gets the Z-A sorting image
		/// </summary>
		/********************************************************************/
		public Bitmap ZA(Color color)
		{
			if (za == null)
				za = new ImageColorBitmapCache(Category, nameof(IMainImages.ZA), 20, 20);

			return za.GetBitmap(color);
		}



		/********************************************************************/
		/// <summary>
		/// Gets the shuffle image
		/// </summary>
		/********************************************************************/
		public Bitmap Shuffle(Color color)
		{
			if (shuffle == null)
				shuffle = new ImageColorBitmapCache(Category, nameof(IMainImages.Shuffle), 20, 20);

			return shuffle.GetBitmap(color);
		}



		/********************************************************************/
		/// <summary>
		/// Gets the set subsong image
		/// </summary>
		/********************************************************************/
		public Bitmap SetSubSong(Color color)
		{
			if (setSubSong == null)
				setSubSong = new ImageColorBitmapCache(Category, nameof(IMainImages.SetSubSong), 20, 20);

			return setSubSong.GetBitmap(color);
		}



		/********************************************************************/
		/// <summary>
		/// Gets the clear subsong image
		/// </summary>
		/********************************************************************/
		public Bitmap ClearSubSong(Color color)
		{
			if (clearSubSong == null)
				clearSubSong = new ImageColorBitmapCache(Category, nameof(IMainImages.ClearSubSong), 20, 20);

			return clearSubSong.GetBitmap(color);
		}



		/********************************************************************/
		/// <summary>
		/// Gets the load image
		/// </summary>
		/********************************************************************/
		public Bitmap Load(Color color)
		{
			if (load == null)
				load = new ImageColorBitmapCache(Category, nameof(IMainImages.Load), 20, 20);

			return load.GetBitmap(color);
		}



		/********************************************************************/
		/// <summary>
		/// Gets the load image
		/// </summary>
		/********************************************************************/
		public Bitmap Save(Color color)
		{
			if (save == null)
				save = new ImageColorBitmapCache(Category, nameof(IMainImages.Save), 20, 20);

			return save.GetBitmap(color);
		}
	}
}
