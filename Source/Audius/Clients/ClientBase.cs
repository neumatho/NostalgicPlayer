/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;

namespace Polycode.NostalgicPlayer.Audius.Clients
{
	/// <summary>
	/// Base class for all Audius client classes
	/// </summary>
	internal abstract class ClientBase
	{
		private const int Timeout = 15000;

		#if DEBUG
		private const string ApplicationName = "NostalgicPlayerTest";
		#else
		private const string ApplicationName = "NostalgicPlayer";
		#endif

		private record DataModel<T>(T Data);

		private static readonly Lock audiusUrlLock = new Lock();
		private static string audiusUrl = null;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected ClientBase()
		{
			lock (audiusUrlLock)
			{
				// If we don't have a URL, then we need to retrieve it
				if (audiusUrl == null)
				{
					// Retrieve the URL from the Audius API
					audiusUrl = GetAudiusUrl();
				}
			}
		}

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Make a REST call to Audius and return the data
		/// </summary>
		/********************************************************************/
		protected T DoRequest<T>(RestRequest request, CancellationToken cancellationToken)
		{
			return DoRequest<T>(audiusUrl, request, cancellationToken);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Create a rest client to use
		/// </summary>
		/********************************************************************/
		private RestClient CreateRestClient(string url)
		{
			RestClientOptions options = new RestClientOptions(url)
			{
				ThrowOnAnyError = true,
				ThrowOnDeserializationError = true,
			};

			return new RestClient(options);
		}



		/********************************************************************/
		/// <summary>
		/// Make a REST call to Audius and return the data
		/// </summary>
		/********************************************************************/
		private T DoRequest<T>(string url, RestRequest request, CancellationToken cancellationToken)
		{
			try
			{
				return Task.Run(() =>
				{
					RestClient client = CreateRestClient(url);

					request.Timeout = TimeSpan.FromMilliseconds(Timeout);
					request.AddQueryParameter("app_name", ApplicationName);

					return client.GetAsync<DataModel<T>>(request, cancellationToken);
				}).Result.Data;
			}
			catch(AggregateException aggregateException)
			{
				foreach (Exception ex in aggregateException.InnerExceptions)
				{
					Exception inner = ex;

					while (inner != null)
					{
						if (inner is TimeoutException)
							throw inner;

						if (inner is TaskCanceledException)
							throw new OperationCanceledException(inner.Message);

						inner = inner.InnerException;
					}
				}

				if (aggregateException.InnerException != null)
					throw aggregateException.InnerException;

				throw;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Get the URL for the Audius API
		/// </summary>
		/********************************************************************/
		private string GetAudiusUrl()
		{
			string[] availableUrls = DoRequest<string[]>("https://api.audius.co", new RestRequest(), CancellationToken.None);
			return availableUrls[Random.Shared.Next(availableUrls.Length)];
		}
		#endregion
	}
}
