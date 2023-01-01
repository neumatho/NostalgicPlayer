/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Containers
{
	/// <summary>
	/// This class holds all static information about the player
	/// </summary>
	public class ModuleInfoStatic
	{
		private readonly ModulePlayerSupportFlag moduleSupportFlag;
		private readonly SamplePlayerSupportFlag sampleSupportFlag;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleInfoStatic()
		{
			ModuleName = string.Empty;
			Author = string.Empty;
			ModuleFormat = string.Empty;
			PlayerName = string.Empty;
			Channels = 0;
			ModuleSize = 0;

			moduleSupportFlag = ModulePlayerSupportFlag.None;
			sampleSupportFlag = SamplePlayerSupportFlag.None;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private ModuleInfoStatic(AgentInfo playerAgentInfo, string moduleName, string author, string[] comment, Font commentFont, string[] lyrics, Font lyricsFont, string moduleFormat, string playerName, int channels, int virtualChannels, long crunchedSize, long moduleSize, int maxSongNumber)
		{
			PlayerAgentInfo = playerAgentInfo;
			ModuleName = moduleName;
			Author = author;
			Comment = comment;
			CommentFont = commentFont;
			Lyrics = lyrics;
			LyricsFont = lyricsFont;
			ModuleFormat = moduleFormat;
			PlayerName = playerName;
			Channels = channels;
			VirtualChannels = virtualChannels;
			CrunchedSize = crunchedSize;
			ModuleSize = moduleSize;
			MaxSongNumber = maxSongNumber;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor (for module players)
		/// </summary>
		/********************************************************************/
		internal ModuleInfoStatic(AgentInfo playerAgentInfo, AgentInfo converterAgentInfo, string moduleName, string author, string[] comment, Font commentFont, string[] lyrics, Font lyricsFont, string moduleFormat, string playerName, int channels, int virtualChannels, long crunchedSize, long moduleSize, ModulePlayerSupportFlag supportFlag, int maxSongNumber, InstrumentInfo[] instruments, SampleInfo[] samples) : this(playerAgentInfo, moduleName, author, comment, commentFont, lyrics, lyricsFont, moduleFormat, playerName, channels, virtualChannels, crunchedSize, moduleSize, maxSongNumber)
		{
			moduleSupportFlag = supportFlag;
			sampleSupportFlag = SamplePlayerSupportFlag.None;

			ConverterAgentInfo = converterAgentInfo;

			Instruments = instruments?.Length == 0 ? null : instruments;
			Samples = samples?.Length == 0 ? null : samples;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor (for sample players)
		/// </summary>
		/********************************************************************/
		internal ModuleInfoStatic(AgentInfo playerAgentInfo, string moduleName, string author, string[] comment, Font commentFont, string[] lyrics, Font lyricsFont, string moduleFormat, string playerName, int channels, long crunchedSize, long moduleSize, SamplePlayerSupportFlag supportFlag, int frequency) : this(playerAgentInfo, moduleName, author, comment, commentFont, lyrics, lyricsFont, moduleFormat, playerName, channels, channels, crunchedSize, moduleSize, 1)
		{
			sampleSupportFlag = supportFlag;
			moduleSupportFlag = ModulePlayerSupportFlag.None;

			Frequency = frequency;
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
		/// Return the format of the module
		/// </summary>
		/********************************************************************/

		public string ModuleFormat
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
		/// Tells whether it is possible to change the position
		/// </summary>
		/********************************************************************/
		public bool CanChangePosition
		{
			get
			{
				return ((moduleSupportFlag & ModulePlayerSupportFlag.SetPosition) != 0) || ((sampleSupportFlag & SamplePlayerSupportFlag.SetPosition) != 0);
			}
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
	}
}
