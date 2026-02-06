/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Polycode.NostalgicPlayer.External.Homepage.Interfaces;
using Polycode.NostalgicPlayer.External.Homepage.Models.VersionHistory;

namespace Polycode.NostalgicPlayer.External.Homepage.Clients
{
	/// <summary>
	/// 
	/// </summary>
	internal class VersionHistoryClient : IVersionHistoryClient
	{
		/********************************************************************/
		/// <summary>
		/// Return all histories between the two arguments
		/// </summary>
		/********************************************************************/
		public HistoriesModel GetHistories(string fromVersion, string toVersion)
		{
			string historyHtml = RetrieveAllVersions();

			List<HistoryModel> histories = new List<HistoryModel>();

			// Find where the history starts in the HTML
			int index = historyHtml.IndexOf("<!-- History start -->");
			if (index != -1)
			{
				// First find the list for the version that has been upgraded to
				string currentVersion;

				do
				{
					currentVersion = SearchAfterVersion(historyHtml, ref index);
					if (string.IsNullOrEmpty(currentVersion))
						break;
				}
				while (currentVersion != toVersion);

				if (!string.IsNullOrEmpty(currentVersion))
				{
					histories.Add(BuildHistoryModel(historyHtml, currentVersion, ref index));

					for (;;)
					{
						currentVersion = SearchAfterVersion(historyHtml, ref index);
						if (string.IsNullOrEmpty(currentVersion) || (currentVersion == fromVersion))
							break;

						histories.Add(BuildHistoryModel(historyHtml, currentVersion, ref index));
					}
				}
			}

			return new HistoriesModel
			{
				Histories = histories.Count != 0 ? histories.ToArray() : null
			};
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Retrieve all versions from the internet
		/// </summary>
		/********************************************************************/
		private string RetrieveAllVersions()
		{
			try
			{
				using (HttpClient httpClient = new HttpClient())
				{
					byte[] bytes = httpClient.GetByteArrayAsync("https://nostalgicplayer.dk/history").Result;

					return Encoding.UTF8.GetString(bytes);
				}
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Search after the next version and return it
		/// </summary>
		/********************************************************************/
		private string SearchAfterVersion(string historyHtml, ref int index)
		{
			int startIndex = historyHtml.IndexOf("<h1>", index);
			if (startIndex == -1)
				return null;

			int endIndex = historyHtml.IndexOf("</h1>", startIndex + 4);
			if (endIndex == -1)
				return null;

			index = endIndex + 5;
			return historyHtml.Substring(startIndex + 4, endIndex - startIndex - 4);
		}



		/********************************************************************/
		/// <summary>
		/// Add all changes for the given version
		/// </summary>
		/********************************************************************/
		private HistoryModel BuildHistoryModel(string historyHtml, string version, ref int index)
		{
			List<string> bullets = new List<string>();

			int startList = historyHtml.IndexOf("<ul", index);
			if (startList != -1)
			{
				int endList = historyHtml.IndexOf("</ul>", startList);
				if (endList != -1)
				{
					index = startList;

					for (;;)
					{
						int bulletStart = historyHtml.IndexOf("<li>", index);
						if ((bulletStart == -1) || (bulletStart > endList))
							break;

						int bulletEnd = historyHtml.IndexOf("</li>", bulletStart + 4);
						if (bulletEnd == -1)
							break;

						string bullet = historyHtml.Substring(bulletStart + 4, bulletEnd - bulletStart - 4);
						bullet = bullet.Replace("&quot;", "\"").Replace("<i>", string.Empty).Replace("</i>", string.Empty);

						bullets.Add(bullet);

						index = bulletEnd + 5;
					}
				}
			}

			return new HistoryModel
			{
				Version = version,
				Bullets = bullets.ToArray()
			};
		}
		#endregion
	}
}
