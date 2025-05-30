/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.PlayerLibrary.Loaders;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Containers
{
	/// <summary>
	/// This class holds all static information about the player
	/// </summary>
	public class ModuleInfoStatic
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleInfoStatic()
		{
			ModuleName = string.Empty;
			Author = string.Empty;
			Format = string.Empty;
			PlayerName = string.Empty;
			Channels = 0;
			VirtualChannels = 0;
			DecruncherAlgorithms = null;
			CrunchedSize = 0;
			ModuleSize = 0;
			MaxSongNumber = 0;
			CanChangePosition = false;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private ModuleInfoStatic(LoaderInfoBase loaderInfo)
		{
			PlayerAgentInfo = loaderInfo.PlayerAgentInfo;
			Format = loaderInfo.Format;
			FormatDescription = loaderInfo.FormatDescription;
			PlayerName = loaderInfo.PlayerName;
			PlayerDescription = loaderInfo.PlayerDescription;
			DecruncherAlgorithms = loaderInfo.DecruncherAlgorithms;
			CrunchedSize = loaderInfo.CrunchedSize;
			ModuleSize = loaderInfo.ModuleSize;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private ModuleInfoStatic(LoaderInfoBase loaderInfo, IPlayerAgent playerAgent) : this(loaderInfo)
		{
			ModuleName = playerAgent.ModuleName?.Trim();
			Comment = playerAgent.Comment;
			CommentFont = playerAgent.CommentFont;
			Lyrics = playerAgent.Lyrics;
			LyricsFont = playerAgent.LyricsFont;
			Pictures = playerAgent.Pictures;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		internal ModuleInfoStatic(Loader loader, IModulePlayerAgent modulePlayerAgent) : this(loader, (IPlayerAgent)modulePlayerAgent)
		{
			Channels = modulePlayerAgent.ModuleChannelCount;
			VirtualChannels = modulePlayerAgent.VirtualChannelCount;
			MaxSongNumber = modulePlayerAgent.SubSongs.Number;
			CanChangePosition = (modulePlayerAgent is IDurationPlayer) && ((modulePlayerAgent.SupportFlags & ModulePlayerSupportFlag.SetPosition) != 0);
			ConverterAgentInfo = loader.ConverterAgentInfo;

			Instruments = modulePlayerAgent.Instruments?.ToArray();
			Samples = modulePlayerAgent.Samples?.ToArray();

			if (Instruments?.Length == 0)
				Instruments = null;

			if (Samples?.Length == 0)
				Samples = null;

			Author = FindAuthor(modulePlayerAgent);
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		internal ModuleInfoStatic(Loader loader, ISamplePlayerAgent samplePlayerAgent) : this(loader, (IPlayerAgent)samplePlayerAgent)
		{
			Channels = samplePlayerAgent.ChannelCount;
			VirtualChannels = samplePlayerAgent.ChannelCount;
			MaxSongNumber = 1;
			CanChangePosition = (samplePlayerAgent is IDurationPlayer) && ((samplePlayerAgent.SupportFlags & SamplePlayerSupportFlag.SetPosition) != 0);
			Frequency = samplePlayerAgent.Frequency;

			Author = FindAuthor(samplePlayerAgent);
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		internal ModuleInfoStatic(StreamLoader loader, IStreamerAgent streamerAgent) : this(loader)
		{
			ModuleName = string.Empty;
			Comment = null;
			CommentFont = null;
			Lyrics = null;
			LyricsFont = null;
			Pictures = null;

			Channels = streamerAgent.ChannelCount;
			VirtualChannels = streamerAgent.ChannelCount;
			MaxSongNumber = 1;
			CanChangePosition = false;
			Frequency = streamerAgent.Frequency;

			Author = string.Empty;
		}

		#region Common properties
		/********************************************************************/
		/// <summary>
		/// Returns agent information about the player in use
		/// </summary>
		/********************************************************************/
		public AgentInfo PlayerAgentInfo
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the name of the module
		/// </summary>
		/********************************************************************/
		public string ModuleName
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the name of the author
		/// </summary>
		/********************************************************************/
		public string Author
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the comment separated into lines
		/// </summary>
		/********************************************************************/
		public string[] Comment
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the font to be used for comments or null
		/// </summary>
		/********************************************************************/
		public Font CommentFont
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the lyrics separated into lines
		/// </summary>
		/********************************************************************/
		public string[] Lyrics
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the font to be used for lyrics or null
		/// </summary>
		/********************************************************************/
		public Font LyricsFont
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return all pictures available
		/// </summary>
		/********************************************************************/
		public PictureInfo[] Pictures
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return the format loaded
		/// </summary>
		/********************************************************************/

		public string Format
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return the format description
		/// </summary>
		/********************************************************************/

		public string FormatDescription
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name of the player
		/// </summary>
		/********************************************************************/
		public string PlayerName
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return the description of the player
		/// </summary>
		/********************************************************************/
		public string PlayerDescription
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		/********************************************************************/
		public int Channels
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module has reserved
		/// </summary>
		/********************************************************************/
		public int VirtualChannels
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return which speakers are used to play the module
		/// </summary>
		/********************************************************************/
		public SpeakerFlag PlayBackSpeakers
		{
			get;
			internal set;   // Cannot set this information in constructor and therefore will be set later on when information is available
		}



		/********************************************************************/
		/// <summary>
		/// Return a list of all the algorithms used to decrunch the module.
		/// If null, no decruncher has been used
		/// </summary>
		/********************************************************************/
		public string[] DecruncherAlgorithms
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Return the crunched size of the module. Is zero, if not crunched.
		/// If -1, it means the crunched length is unknown
		/// </summary>
		/********************************************************************/
		public long CrunchedSize
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return the size of the module
		/// </summary>
		/********************************************************************/
		public long ModuleSize
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return the maximum number of songs in the current module
		/// </summary>
		/********************************************************************/
		public int MaxSongNumber
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Tells whether it is possible to change the position
		/// </summary>
		/********************************************************************/
		public bool CanChangePosition
		{
			get;
		}
		#endregion

		#region Module specific properties
		/********************************************************************/
		/// <summary>
		/// Returns agent information about the converted that has been used
		/// </summary>
		/********************************************************************/
		public AgentInfo ConverterAgentInfo
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return all the instruments in the module
		/// </summary>
		/********************************************************************/
		public InstrumentInfo[] Instruments
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return all the samples in the module
		/// </summary>
		/********************************************************************/
		public SampleInfo[] Samples
		{
			get;
		}
		#endregion

		#region Sample specific properties
		/********************************************************************/
		/// <summary>
		/// Return the frequency the sample is stored as
		/// </summary>
		/********************************************************************/
		public int Frequency
		{
			get;
		}
		#endregion

		#region Find author algorithm
		/********************************************************************/
		/// <summary>
		/// Return the author of the module
		/// </summary>
		/********************************************************************/
		private string FindAuthor(IPlayerAgent playerAgent)
		{
			string name = playerAgent.Author;

			if (string.IsNullOrWhiteSpace(name))
			{
				// We didn't get any author, now scan the instruments/samples
				// after an author
				List<string> nameList = Instruments?.Select(instInfo => instInfo.Name).ToList();
				if ((nameList != null) && (nameList.Count > 0))
					name = FindAuthorInList(nameList);

				if (string.IsNullOrEmpty(name))
				{
					// No author found in the instrument names, now try the samples
					nameList = Samples?.Select(sampInfo => sampInfo.Name).ToList();
					if ((nameList != null) && (nameList.Count > 0))
						name = FindAuthorInList(nameList);
				}
			}

			// Trim and return the name
			return name?.Trim();
		}



		/********************************************************************/
		/// <summary>
		/// Tries to find the author in a list of names
		/// </summary>
		/********************************************************************/
		private string FindAuthorInList(List<string> list)
		{
			int i, pos = -1, startPos = -1;
			string itemStr = string.Empty;
			string name = string.Empty;

			// First get the number of items in the list
			int count = list.Count;

			// Traverse all the names after the hash mark
			for (i = 0; i < count; i++)
			{
				// Get the string to search in
				itemStr = list[i];

				pos = itemStr.IndexOf('#');
				if (pos != -1)
				{
					if ((itemStr.Length >= (pos + 5)) && (itemStr.Substring(pos + 1, 4).ToLower() == "from"))
						startPos = pos + 5;
					else
					{
						startPos = pos + 1;

						// See if there is a "by" word after the mark
						pos = FindBy(itemStr.Substring(startPos));
						if (pos != -1)
							startPos = pos;
					}

					// Try to find the beginning of the author in the current string
					for (pos = startPos; pos < itemStr.Length; pos++)
					{
						if (char.IsLetterOrDigit(itemStr[pos]))
						{
							startPos = pos;
							break;
						}
					}

					// Find the author
					name = ClipOutAuthor(itemStr, startPos);

					// If the found name starts with a digit, ignore it.
					// Also ignore other common names, we know is not the author
					if (string.IsNullOrEmpty(name) || (char.IsDigit(name[0]) && !name.StartsWith("4-mat")) || name.ToLower().StartsWith("trax"))
					{
						startPos = -1;
						name = string.Empty;
					}
					break;
				}
			}

			if (startPos == -1)
			{
				// Traverse all the names
				for (i = 0; i < count; i++)
				{
					// Get the string to search in
					itemStr = list[i];

					// If the string is empty, we don't need to do a search :-)
					if (string.IsNullOrWhiteSpace(itemStr))
						continue;

					// Try to find a "by" word
					pos = FindBy(itemStr);
					if (pos != -1)
						break;
				}

				if (pos != -1)
				{
					// Now try to find the author, search through the current
					// string to the end of the list
					for (;;)
					{
						// Scan each character in the rest of the string
						for (; pos < itemStr.Length; pos++)
						{
							if (char.IsLetterOrDigit(itemStr[pos]))
							{
								startPos = pos;
								break;
							}
						}

						// Get a start position, break the loop
						if (startPos != -1)
							break;

						// Get next line
						i++;
						if (i == count)
							break;

						itemStr = list[i];
						pos = 0;
					}
				}
				else
				{
					// We didn't find a "by" word, try to find other author marks
					for (i = 0; i < count; i++)
					{
						// Get the string to search in
						itemStr = list[i];

						// If the string is empty, we don't need to do a search :-)
						if (string.IsNullOrWhiteSpace(itemStr))
							continue;

						// Is there a ">>>" mark?
						if (itemStr.StartsWith(">>>"))
						{
							startPos = 3;
							break;
						}

						// What about the ">>" mark?
						if (itemStr.StartsWith(">>"))
						{
							startPos = 2;
							break;
						}

						// Is there a "?>>>" mark?
						if ((itemStr.Length >= 4) && (itemStr.Substring(1, 3) == ">>>"))
						{
							startPos = 4;
							break;
						}

						// What about the "?>>" mark?
						if ((itemStr.Length >= 4) && (itemStr.Substring(1, 2) == ">>"))
						{
							startPos = 3;
							break;
						}
					}

					if (startPos != -1)
					{
						// See if there is a "by" word after the mark
						pos = FindBy(itemStr.Substring(startPos));
						if (pos != -1)
							startPos = pos;
					}
				}

				if (startPos != -1)
					name = ClipOutAuthor(itemStr, startPos);
			}

			return name;
		}



		/********************************************************************/
		/// <summary>
		/// Find the end of the given string and return what is found
		/// </summary>
		/********************************************************************/
		private string ClipOutAuthor(string itemStr, int startPos)
		{
			// Got the start position of the author, now find the end position
			string name = itemStr.Substring(startPos).Trim();

			int pos;
			for (pos = 0; pos < name.Length; pos++)
			{
				// Get the current character
				char chr = name[pos];

				// Check for legal characters
				if (((chr == ' ') || (chr == '!') || (chr == '\'') || (chr == '-') || (chr == '/') || char.IsDigit(chr)))
				{
					// It's legal, go to the next character
					continue;
				}

				// Check to see if the & character is the last one and if
				// not, it's legal
				if ((chr == '&') && ((pos + 1) < name.Length))
					continue;

				if (chr == '.')
				{
					// The point is the last character
					if ((pos + 1) == name.Length)
						break;

					// Are there a space or another point after the first one?
					if ((name[pos + 1] == ' ') || (name[pos + 1] == '.'))
						break;

					continue;
				}

				// Is the character a letter?
				if (!char.IsLetter(chr))
				{
					// No, stop the loop
					break;
				}

				// Stop if .... of
				if ((pos + 1) < name.Length)
				{
					if ((chr == 'o') && (name[pos + 1] == 'f') && ((pos + 2) == name.Length))
					{
						if ((pos > 0) && (name[pos - 1] == ' '))
							break;
					}
				}

				// Stop if .... from
				if ((pos + 3) < name.Length)
				{
					if ((chr == 'f') && (name[pos + 1] == 'r') && (name[pos + 2] == 'o') && (name[pos + 3] == 'm') && ((pos + 4) == name.Length))
					{
						if ((pos > 0) && (name[pos - 1] == ' '))
							break;
					}
				}

				// Stop if .... in
				if ((pos + 1) < name.Length)
				{
					if ((chr == 'i') && (name[pos + 1] == 'n') && ((pos + 2) == name.Length))
					{
						if ((pos > 0) && (name[pos - 1] == ' '))
							break;
					}
				}

				// Stop if .... and
				if ((pos + 2) < name.Length)
				{
					if ((chr == 'a') && (name[pos + 1] == 'n') && (name[pos + 2] == 'd') && ((pos + 3) == name.Length))
					{
						if ((pos > 0) && (name[pos - 1] == ' '))
							break;
					}
				}
			}

			// Clip out the author
			name = name.Substring(0, pos).TrimEnd();

			// Check for some special characters that needs to be removed
			if (!string.IsNullOrEmpty(name))
			{
				for (;;)
				{
					char chr = name[name.Length - 1];
					if (chr != '-')
						break;

					if (name[0] == chr)
						break;

					name = name.Substring(0, name.Length - 1);
				}
			}

			return name.TrimEnd('/').TrimEnd();
		}



		/********************************************************************/
		/// <summary>
		/// Will look in the string given after the "by" word and return the
		/// index where it found it
		/// </summary>
		/********************************************************************/
		private int FindBy(string str)
		{
			int index = 0;
			bool found = false;

			while (index < (str.Length - 1))
			{
				if (((str[index] == 'b') || (str[index] == 'B')) && ((str[index + 1] == 'y') || (str[index + 1] == 'Y')))
				{
					// Check to see if the character before "by" is a letter
					if ((index > 0) && char.IsLetter(str[index - 1]))
					{
						index++;
						continue;
					}

					// Check to see if the character after "by" is a letter
					if (((index + 2) < str.Length) && char.IsLetter(str[index + 2]))
					{
						index++;
						continue;
					}

					if ((index + 2) == str.Length)
					{
						// The last word in the string is "by", so we found it
						return index + 2;
					}

					index += 2;
					found = true;
					break;
				}

				// Go to the next character
				index++;
			}

			// Did we found the "by" word?
			if (found)
			{
				// Yep, check if it's "by" some known phrases we need to ignore
				if (((index + 5) <= str.Length) && (str.Substring(index + 1, 4) == "KIWI"))
					return -1;

				if (((index + 11) <= str.Length) && (str.Substring(index + 1, 10) == "the welder"))
					return -1;

				if (((index + 7) <= str.Length) && (str.Substring(index + 1, 6) == "e-mail"))
					return -1;

				if (((index + 7) <= str.Length) && (str.Substring(index + 1, 6) == "Gryzor"))
					return -1;

				if (((index + 6) <= str.Length) && (str.Substring(index + 1, 5) == ">Asle"))
					return -1;

				if (((index + 5) <= str.Length) && (str.Substring(index + 1, 4) == "Asle"))
					return -1;

				if (((index + 8) <= str.Length) && (str.Substring(index + 1, 7) == "Trilogy"))
					return -1;
			}
			else
			{
				// Okay, now try to find "(c)"
				index = 0;
				while (index < (str.Length - 2))
				{
					if ((str[index] == '(') && ((str[index + 1] == 'c') || (str[index + 1] == 'C')) && (str[index + 2] == ')'))
					{
						index += 3;
						found = true;
						break;
					}

					// Go to the next character
					index++;
				}
			}

			if (found)
			{
				// Find the first letter in author
				for (; index < str.Length; index++)
				{
					if (str[index] < '0')
						continue;

					if ((str[index] <= '9') || (str[index] >= 'A'))
						break;
				}

				return index;
			}

			return -1;
		}
		#endregion
	}
}
