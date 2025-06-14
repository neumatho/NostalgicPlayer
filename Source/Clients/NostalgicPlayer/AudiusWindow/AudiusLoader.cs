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
using Polycode.NostalgicPlayer.Audius;
using Polycode.NostalgicPlayer.Audius.Interfaces;
using Polycode.NostalgicPlayer.Audius.Models.Tracks;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;
using Polycode.NostalgicPlayer.PlayerLibrary.Interfaces;
using Polycode.NostalgicPlayer.PlayerLibrary.Loaders;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow
{
	/// <summary>
	/// Loader class that helps start an Audius streaming
	/// </summary>
	public class AudiusLoader : StreamLoader, IStreamMetadata
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AudiusLoader(Manager agentManager) : base(agentManager)
		{
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

			AudiusApi audiusApi = new AudiusApi();

			ITrackClient trackClient = audiusApi.GetTrackClient();
			Uri trackUrl = trackClient.GetStreamingUrl(source);

			return base.Load(trackUrl.AbsoluteUri, out errorMessage);
		}
		#endregion

		#region IStreamMetaData implementation
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
				AudiusApi audiusApi = new AudiusApi();

				ITrackClient trackClient = audiusApi.GetTrackClient();
				TrackModel track = trackClient.GetTrackInfo(trackId, CancellationToken.None);

				Title = track.Title;
				Author = track.User.Name;
				Comment = string.IsNullOrEmpty(track.Description) ? null : track.Description.Split('\n');
				Duration = track.Duration.HasValue ? TimeSpan.FromSeconds(track.Duration.Value) : TimeSpan.Zero;

				if (track.Artwork?._480x480 != null)
				{
					PictureInfo picture = Task.Run(async () =>
					{
						using (PictureDownloader pictureDownloader = new PictureDownloader())
						{
							Bitmap bitmap = await pictureDownloader.GetPictureAsync(track.Artwork._480x480, CancellationToken.None);

							using (MemoryStream ms = new MemoryStream())
							{
								bitmap.Save(ms, ImageFormat.Png);

								return new PictureInfo(ms.ToArray(), Resources.IDS_AUDIUS_PICTURE_NAME);
							}
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
