/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Theme.Purple
{
	/// <summary>
	/// Holds all colors used by the images
	/// </summary>
	internal class PurpleImageColors : IImageColors
	{
		private readonly ILabelColors labelColors = new PurpleLabelColors();

		private static readonly Color informationColor = Color.FromArgb(0, 0, 0);
		private static readonly Color muteColor = Color.FromArgb(0, 0, 0);
		private static readonly Color addColor = Color.FromArgb(63, 104, 165);
		private static readonly Color removeColor = Color.FromArgb(63, 104, 165);
		private static readonly Color swapColor = Color.FromArgb(63, 104, 165);
		private static readonly Color sortColor = Color.FromArgb(63, 104, 165);
		private static readonly Color moveUpColor = Color.FromArgb(63, 104, 165);
		private static readonly Color moveDownColor = Color.FromArgb(63, 104, 165);
		private static readonly Color listColor = Color.FromArgb(63, 104, 165);
		private static readonly Color diskColor = Color.FromArgb(63, 104, 165);
		private static readonly Color previousModuleColor = Color.FromArgb(148, 47, 255);
		private static readonly Color nextModuleColor = Color.FromArgb(148, 47, 255);
		private static readonly Color previousSongColor = Color.FromArgb(148, 47, 255);
		private static readonly Color nextSongColor = Color.FromArgb(148, 47, 255);
		private static readonly Color fastRewindColor = Color.FromArgb(148, 47, 255);
		private static readonly Color fastForwardColor = Color.FromArgb(148, 47, 255);
		private static readonly Color playColor = Color.FromArgb(148, 47, 255);
		private static readonly Color ejectColor = Color.FromArgb(148, 47, 255);
		private static readonly Color pauseColor = Color.FromArgb(255, 106, 180);
		private static readonly Color loopColor = Color.FromArgb(0, 0, 0);
		private static readonly Color favoritesColor = Color.FromArgb(0, 0, 0);
		private static readonly Color equalizerColor = Color.FromArgb(0, 0, 0);
		private static readonly Color samplesColor = Color.FromArgb(0, 0, 0);

		private static readonly Color sampleLoopColor = Color.FromArgb(55, 134, 64);
		private static readonly Color samplePingPongColor = Color.FromArgb(16, 43, 147);
		private static readonly Color sampleStereoColor = Color.FromArgb(255, 120, 70);
		private static readonly Color sampleMultiOctavesColor = Color.FromArgb(119, 137, 201);

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color InformationColor => informationColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color MuteColor => muteColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color AddColor => addColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color RemoveColor => removeColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color SwapColor => swapColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color SortColor => sortColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color MoveUpColor => moveUpColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color MoveDownColor => moveDownColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color ListColor => listColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color DiskColor => diskColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color PreviousModuleColor => previousModuleColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color NextModuleColor => nextModuleColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color PreviousSongColor => previousSongColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color NextSongColor => nextSongColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color FastRewindColor => fastRewindColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color FastForwardColor => fastForwardColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color PlayColor => playColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color EjectColor => ejectColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color PauseColor => pauseColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color LoopColor => loopColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color FavoritesColor => favoritesColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color EqualizerColor => equalizerColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color SamplesColor => samplesColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color PreviousPictureColor => labelColors.TextColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color NextPictureColor => labelColors.TextColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color SampleLoopColor => sampleLoopColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color SamplePingPongColor => samplePingPongColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color SampleStereoColor => sampleStereoColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color SampleMultiOctavesColor => sampleMultiOctavesColor;
	}
}
