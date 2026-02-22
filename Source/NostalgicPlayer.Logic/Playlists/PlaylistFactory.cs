/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Polycode.NostalgicPlayer.Logic.Playlists
{
	/// <summary>
	/// This class can create multi file loaders which can handle module lists
	/// </summary>
	internal class PlaylistFactory : IPlaylistFactory
	{
		private readonly byte[] npmlSignature;
		private readonly byte[] m3uExtSignature;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PlaylistFactory()
		{
			npmlSignature = Encoding.UTF8.GetBytes("@*NpML*@");
			m3uExtSignature = Encoding.UTF8.GetBytes("#EXTM3U");
		}



		/********************************************************************/
		/// <summary>
		/// Return all available extensions for list files
		/// </summary>
		/********************************************************************/
		public string[] GetExtensions()
		{
			return
				new NpmlList().FileExtensions
				.Concat(new M3UList().FileExtensions)
				.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Try to figure out which kind of list this is and return a loader
		/// if anyone could be found
		/// </summary>
		/********************************************************************/
		public IPlaylist Create(Stream stream, string fileExtension)
		{
			byte[] buffer = new byte[16];

			// Make sure the file position is at the beginning of the file
			stream.Seek(0, SeekOrigin.Begin);

			// Read the first line
			int bytesRead = stream.Read(buffer, 0, 16);

			if (bytesRead >= 7)
			{
				if (buffer.Take(7).SequenceEqual(m3uExtSignature))
					return new M3UList();

				if (new M3UList().FileExtensions.Contains(fileExtension))
					return new M3UList();
			}

			if (bytesRead >= 11)
			{
				if (buffer.Skip(3).Take(8).SequenceEqual(npmlSignature))	// Skip BOM
					return new NpmlList();
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Create a playlist instance based on type
		/// </summary>
		/********************************************************************/
		public IPlaylist Create(PlaylistType type)
		{
			switch (type)
			{
				case PlaylistType.Npml:
					return new NpmlList();

				case PlaylistType.M3U:
					return new M3UList();

				default:
					throw new NotImplementedException();
			}
		}
	}
}
