/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
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

					return Encoding.UTF8.GetString(bytes).Replace("\r", string.Empty);
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
			HistoryModel result = new HistoryModel
			{
				Version = version
			};

			int startList = historyHtml.IndexOf("<ul", index);
			if (startList != -1)
			{
				index = startList;

				result.Bullets = ReadBullets(historyHtml, ref index);
			}

			if (result.Bullets == null)
				result.Bullets = [];

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Read bullets
		/// </summary>
		/********************************************************************/
		private HistoryBulletModel[] ReadBullets(string historyHtml, ref int index)
		{
			// Adjust index, if not right there
			index = historyHtml.IndexOf("<ul", index);

			List<HistoryBulletModel> bullets = new List<HistoryBulletModel>();
			HistoryBulletModel currentBullet = null;

			StringReader sr = new StringReader(historyHtml.Substring(index));

			// Skip the ul
			string line = sr.ReadLine();
			index += line!.Length + 1;

			for (;;)
			{
				line = sr.ReadLine();
				if (line == null)
					break;

				string trimmedLine = line.TrimStart();

				if (trimmedLine.StartsWith("<ul "))
				{
					currentBullet.SubBullets = ReadBullets(historyHtml, ref index);

					sr.Dispose();
					sr = new StringReader(historyHtml.Substring(index));
				}
				else if (trimmedLine.StartsWith("<li>"))
				{
					int bulletEnd = trimmedLine.IndexOf("</li>", 4);
					if (bulletEnd == -1)
						break;

					string bullet = trimmedLine.Substring(4, bulletEnd - 4);
					bullet = bullet.Replace("&quot;", "\"").Replace("<i>", string.Empty).Replace("</i>", string.Empty);

					currentBullet = new HistoryBulletModel
					{
						BulletText = bullet
					};

					bullets.Add(currentBullet);

					index += line.Length + 1;
				}
				else
				{
					index += line.Length + 1;
					break;
				}
			}

			sr.Dispose();

			return bullets.ToArray();
		}
		#endregion
	}
}
