/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Polycode.NostalgicPlayer.External.Audius;
using Polycode.NostalgicPlayer.External.Audius.Interfaces;
using Polycode.NostalgicPlayer.External.Audius.Models.Tracks;
using Polycode.NostalgicPlayer.External.Download;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Library.Loaders;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow
{
	/// <summary>
	/// Loader class that helps start an Audius streaming
	/// </summary>
	public class AudiusLoader : StreamLoader, IMetadata, IStreamSeek
	{
		private static readonly IPictureDownloader pictureDownloader;

		private readonly IAudiusClientFactory clientFactory;

		/********************************************************************/
		/// <summary>
		/// Static constructor - initialize the picture downloader
		/// </summary>
		/********************************************************************/
		static AudiusLoader()
		{
			IPictureDownloaderFactory factory = DependencyInjection.Container.GetInstance<IPictureDownloaderFactory>();
			pictureDownloader = factory.Create();
			pictureDownloader.SetMaxNumberInCache(5);

			AppDomain.CurrentDomain.ProcessExit += AudiusLoader_Dtor;
		}



		/********************************************************************/
		/// <summary>
		/// Destructor - dispose the picture downloader
		/// </summary>
		/********************************************************************/
		static void AudiusLoader_Dtor(object sender, EventArgs e)
		{
			pictureDownloader.Dispose();
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AudiusLoader()
		{
			clientFactory = DependencyInjection.Container.GetInstance<IAudiusClientFactory>();
		}

		#region LoaderBase overrides
		/********************************************************************/
		/// <summary>
		/// Will try to find a player that understand the source and then
		/// load it into memory or prepare it
		/// </summary>
		/********************************************************************/
		public override bool Load(string source, out string errorMessage)
		{
			// Get track information
			SetupTrackInformation(source);

			ITrackClient trackClient = clientFactory.GetTrackClient();
			Uri trackUrl = trackClient.GetStreamingUrl(source);

			return base.Load(trackUrl.AbsoluteUri, out errorMessage);
		}
		#endregion

		#region IMetadata implementation
		/********************************************************************/
		/// <summary>
		/// Return the title of the song
		/// </summary>
		/********************************************************************/
		public string Title
		{
			get;
			private set;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name of the author
		/// </summary>
		/********************************************************************/
		public string Author
		{
			get;
			private set;
		}



		/********************************************************************/
		/// <summary>
		/// Return some comment about the song
		/// </summary>
		/********************************************************************/
		public string[] Comment
		{
			get;
			private set;
		}



		/********************************************************************/
		/// <summary>
		/// Return a specific font to be used for the comments
		/// </summary>
		/********************************************************************/
		public Font CommentFont => null;



		/********************************************************************/
		/// <summary>
		/// Return the lyrics separated in lines
		/// </summary>
		/********************************************************************/
		public string[] Lyrics => [];



		/********************************************************************/
		/// <summary>
		/// Return a specific font to be used for the lyrics
		/// </summary>
		/********************************************************************/
		public Font LyricsFont => null;



		/********************************************************************/
		/// <summary>
		/// Holds the duration of the module this item has
		/// </summary>
		/********************************************************************/
		public TimeSpan Duration
		{
			get;
			private set;
		}



		/********************************************************************/
		/// <summary>
		/// Return all pictures available
		/// </summary>
		/********************************************************************/
		public PictureInfo[] Pictures
		{
			get;
			private set;
		}
		#endregion

		#region IStreamSeek implementation
		/********************************************************************/
		/// <summary>
		/// Tells whether the stream supports seeking or not
		/// </summary>
		/********************************************************************/
		public bool CanSeek => true;



		/********************************************************************/
		/// <summary>
		/// Set the stream to the current position. Return the new stream to
		/// use
		/// </summary>
		/********************************************************************/
		public Stream SetPosition(long newPosition)
		{
			return ReconnectToPosition(newPosition);
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Return a seeking implementation of the stream supports it
		/// </summary>
		/********************************************************************/
		protected override IStreamSeek GetSeeker()
		{
			return this;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Retrieve and setup track metadata if any
		/// </summary>
		/********************************************************************/
		private void SetupTrackInformation(string trackId)
		{
			try
			{
				ITrackClient trackClient = clientFactory.GetTrackClient();
				TrackModel track = trackClient.GetTrackInfo(trackId, CancellationToken.None);

				Title = track.Title;
				Author = track.User.Name;
				Comment = string.IsNullOrEmpty(track.Description) ? null : track.Description.Split('\n');
				Duration = track.Duration.HasValue ? TimeSpan.FromSeconds(track.Duration.Value) : TimeSpan.Zero;

				if (track.Artwork?._480x480 != null)
				{
					PictureInfo picture = Task.Run(async () =>
					{
						Bitmap bitmap = await pictureDownloader.GetPictureAsync(track.Artwork._480x480, CancellationToken.None);

						using (MemoryStream ms = new MemoryStream())
						{
							bitmap.Save(ms, ImageFormat.Png);

							return new PictureInfo(ms.ToArray(), Resources.IDS_AUDIUS_PICTURE_NAME);
						}
					}).Result;

					Pictures = [ picture ];
				}
			}
			catch (TimeoutException)
			{
				// Ignore timeout. We just don't show any information
			}
		}
		#endregion
	}
}
