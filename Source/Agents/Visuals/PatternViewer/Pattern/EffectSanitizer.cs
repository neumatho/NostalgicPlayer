//---------------------------------------------------------------------------------------
// <copyright file="EffectSanitizer.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System.Collections.Generic;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern
{
	/// <summary>
	/// Sanitizes effect text by replacing characters not supported by the bitmap font
	/// </summary>
	internal class EffectSanitizer
	{
		private readonly Dictionary<string, string> cache = new();
		private char fallback;
		private bool[] validChars;

		/********************************************************************/
		/// <summary>
		/// Initialize the sanitizer with allowed characters and fallback
		/// </summary>
		/********************************************************************/
		public void Initialize(string allowedChars, char fallbackChar = '#')
		{
			fallback = fallbackChar;
			validChars = new bool[256];

			foreach (char c in allowedChars)
			{
				if (c < 256)
				{
					validChars[c] = true;

					// Also allow both cases for letters
					if (char.IsLetter(c))
					{
						validChars[char.ToUpper(c)] = true;
						validChars[char.ToLower(c)] = true;
					}
				}
			}

			cache.Clear();
		}

		/********************************************************************/
		/// <summary>
		/// Sanitize effect text, replacing unsupported characters with fallback
		/// </summary>
		/********************************************************************/
		public string Sanitize(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return text;
			}

			// If not initialized, pass through unchanged
			if (validChars == null)
			{
				return text;
			}

			// Check cache first
			if (cache.TryGetValue(text, out string cached))
			{
				return cached;
			}

			// Check if any character needs replacing
			bool needsReplace = false;
			foreach (char c in text)
			{
				if (c >= 256 || !validChars[c])
				{
					needsReplace = true;
					break;
				}
			}

			string result;
			if (!needsReplace)
			{
				result = text;
			}
			else
			{
				char[] chars = text.ToCharArray();
				for (int i = 0; i < chars.Length; i++)
				{
					char c = chars[i];
					if (c >= 256 || !validChars[c])
					{
						chars[i] = fallback;
					}
				}

				result = new string(chars);
			}

			cache[text] = result;
			return result;
		}

		/********************************************************************/
		/// <summary>
		/// Clear the cache (call on song change)
		/// </summary>
		/********************************************************************/
		public void ClearCache()
		{
			cache.Clear();
		}
	}
}
