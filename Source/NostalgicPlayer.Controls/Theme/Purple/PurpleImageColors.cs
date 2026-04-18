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

		private static readonly Color sampleLoopColor = Color.FromArgb(55, 134, 64);
		private static readonly Color samplePingPongColor = Color.FromArgb(16, 43, 147);
		private static readonly Color sampleStereoColor = Color.FromArgb(255, 120, 70);
		private static readonly Color sampleMultiOctavesColor = Color.FromArgb(119, 137, 201);

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color PlayingItemColor => labelColors.TextColor;



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
