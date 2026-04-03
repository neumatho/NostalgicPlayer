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
	/// Holds all the images needed by the Sample Information window
	/// </summary>
	internal class SampleInformationImages : ThemedImageBase, ISampleInformationImages
	{
		private const string Category = "SampleInformation";

		private Bitmap sampleLoop;
		private Bitmap samplePingPong;
		private Bitmap sampleStereo;
		private Bitmap sampleMultiOctave;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SampleInformationImages(IThemeManager themeManager) : base(themeManager)
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
			sampleLoop?.Dispose();
			sampleLoop = null;

			samplePingPong?.Dispose();
			samplePingPong = null;

			sampleStereo?.Dispose();
			sampleStereo = null;

			sampleMultiOctave?.Dispose();
			sampleMultiOctave = null;
		}



		/********************************************************************/
		/// <summary>
		/// Gets the sample loop image
		/// </summary>
		/********************************************************************/
		public Bitmap SampleLoop
		{
			get
			{
				if (sampleLoop == null)
					sampleLoop = GetSvgBitmap(Category, "SampleLoop", CurrentColors.SampleLoopColor, 12, 12);

				return sampleLoop;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the sample ping-pong image
		/// </summary>
		/********************************************************************/
		public Bitmap SamplePingPong
		{
			get
			{
				if (samplePingPong == null)
					samplePingPong = GetSvgBitmap(Category, "SamplePingPong", CurrentColors.SamplePingPongColor, 12, 12);

				return samplePingPong;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the sample stereo image
		/// </summary>
		/********************************************************************/
		public Bitmap SampleStereo
		{
			get
			{
				if (sampleStereo == null)
					sampleStereo = GetSvgBitmap(Category, "SampleStereo", CurrentColors.SampleStereoColor, 12, 12);

				return sampleStereo;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the sample multi octaves image
		/// </summary>
		/********************************************************************/
		public Bitmap SampleMultiOctaves
		{
			get
			{
				if (sampleMultiOctave == null)
					sampleMultiOctave = GetSvgBitmap(Category, "SampleMultiOctaves", CurrentColors.SampleMultiOctavesColor, 12, 12);

				return sampleMultiOctave;
			}
		}
	}
}
