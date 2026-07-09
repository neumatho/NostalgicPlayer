//---------------------------------------------------------------------------------------
// <copyright file="PatternBitmapCache.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;
using System.Drawing;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Containers;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern
{
	/// <summary>
	/// Parameters captured when pattern is rendered
	/// </summary>
	internal class CacheParameters
	{
		/// <summary>
		/// Result from RenderViewer (rect-based approach)
		/// </summary>
		public RenderViewerResult? ViewerResult
		{
			get;
			set;
		}
	}

	/// <summary>
	/// Cache for rendered pattern bitmap. The pattern is rendered once and cached,
	/// then VU meters are drawn on top of the cached bitmap each frame.
	/// Thread-safe for access from player and UI threads.
	/// </summary>
	internal class PatternBitmapCache : IDisposable
	{
		private readonly CacheParameters parameters;
		private readonly object syncLock = new();

		private Bitmap cachedBitmap;
		private bool isValid;
		private Size lastSize;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PatternBitmapCache()
		{
			parameters = new CacheParameters();
		}

		/********************************************************************/
		/// <summary>
		/// Get the parameters object to fill during rendering
		/// </summary>
		/********************************************************************/
		public CacheParameters Parameters
		{
			get
			{
				lock (syncLock)
				{
					return parameters;
				}
			}
		}

		/********************************************************************/
		/// <summary>
		/// Check if cache is currently valid
		/// </summary>
		/********************************************************************/
		public bool IsValid
		{
			get
			{
				lock (syncLock)
				{
					return isValid;
				}
			}
		}

		/********************************************************************/
		/// <summary>
		/// Dispose resources
		/// </summary>
		/********************************************************************/
		public void Dispose()
		{
			lock (syncLock)
			{
				cachedBitmap?.Dispose();
				cachedBitmap = null;
				isValid = false;
			}
		}

		/********************************************************************/
		/// <summary>
		/// Check if cache is valid and get the cached bitmap and parameters
		/// </summary>
		/********************************************************************/
		public bool TryGetValid(out Bitmap bitmap, out CacheParameters cacheParameters)
		{
			lock (syncLock)
			{
				if (isValid && cachedBitmap != null)
				{
					bitmap = cachedBitmap;
					cacheParameters = parameters;
					return true;
				}

				bitmap = null;
				cacheParameters = null;
				return false;
			}
		}

		/********************************************************************/
		/// <summary>
		/// Get or create bitmap for rendering. Returns null if size is empty.
		/// </summary>
		/********************************************************************/
		public Bitmap GetOrCreateBitmap(Size size)
		{
			lock (syncLock)
			{
				if (size.Width <= 0 || size.Height <= 0)
				{
					return null;
				}

				// Check if size changed
				if (lastSize != size)
				{
					cachedBitmap?.Dispose();
					cachedBitmap = null;
					lastSize = size;
					isValid = false;
				}

				// Create bitmap if needed
				if (cachedBitmap == null)
				{
					cachedBitmap = new Bitmap(size.Width, size.Height);
				}

				return cachedBitmap;
			}
		}

		/********************************************************************/
		/// <summary>
		/// Mark the cache as valid after rendering is complete
		/// </summary>
		/********************************************************************/
		public void SetValid()
		{
			lock (syncLock)
			{
				isValid = true;
			}
		}

		/********************************************************************/
		/// <summary>
		/// Invalidate the cache (content changed, but size is same)
		/// </summary>
		/********************************************************************/
		public void Invalidate()
		{
			lock (syncLock)
			{
				isValid = false;
				parameters.ViewerResult = null;
			}
		}

		/********************************************************************/
		/// <summary>
		/// Invalidate if size changed (disposes old bitmap)
		/// </summary>
		/********************************************************************/
		public void InvalidateIfSizeChanged(Size newSize)
		{
			lock (syncLock)
			{
				if (lastSize != newSize)
				{
					cachedBitmap?.Dispose();
					cachedBitmap = null;
					lastSize = newSize;
					isValid = false;
					parameters.ViewerResult = null;
				}
			}
		}
	}
}
