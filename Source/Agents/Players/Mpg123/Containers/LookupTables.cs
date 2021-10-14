/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.Containers
{
	/// <summary>
	/// Holds different lookup tables
	/// </summary>
	internal static class LookupTables
	{
		/// <summary>
		/// Bit rates for [MPEG 1/2][layer]
		/// </summary>
		public static readonly int[,,] TabSel123 = new int[2, 3, 16]
		{
			{
				{ 0, 32, 64, 96, 128, 160, 192, 224, 256, 288, 320, 352, 384, 416, 448, 0 },
				{ 0, 32, 48, 56,  64,  80,  96, 112, 128, 160, 192, 224, 256, 320, 384, 0 },
				{ 0, 32, 40, 48,  56,  64,  80,  96, 112, 128, 160, 192, 224, 256, 320, 0 }
			},

			{
				{ 0, 32, 48, 56, 64, 80, 96, 112, 128, 144, 160, 176, 192, 224, 256, 0 },
				{ 0,  8, 16, 24, 32, 40, 48,  56,  64,  80,  96, 112, 128, 144, 160, 0 },
				{ 0,  8, 16, 24, 32, 40, 48,  56,  64,  80,  96, 112, 128, 144, 160, 0 }
			}
		};

		/// <summary>
		/// Frequency table
		/// </summary>
		public static readonly int[] Freqs = new int[9]
		{
			44100, 48000, 32000, 22050, 24000, 16000, 11025, 12000, 8000
		};

		/// <summary>
		/// Genres from MP3 tags
		/// </summary>
		public static readonly string[] Genre = new string[192]
		{
			"Blues",				"Classic Rock",			"Country",
			"Dance",				"Disco",				"Funk",
			"Grunge",				"Hip-Hop",				"Jazz",
			"Metal",				"New Age",				"Oldies",
			"Other",				"Pop",					"Rhythm And Blues",
			"Rap",					"Reggae",				"Rock",
			"Techno",				"Industrial",			"Alternative",
			"Ska",					"Death Metal",			"Pranks",
			"Soundtrack",			"Euro-Techno",			"Ambient",
			"Trip-Hop",				"Vocal",				"Jazz And Funk",
			"Fusion",				"Trance",				"Classical",
			"Instrumental",			"Acid",					"House",
			"Game",					"Sound Clip",			"Gospel",
			"Noise",				"Alternative Rock",		"Bass",
			"Soul",					"Punk",					"Space",
			"Meditative",			"Instrumental Pop",		"Instrumental Rock",
			"Ethnic",				"Gothic",				"Darkwave",
			"Techno-Industrial",	"Electronic",			"Pop-Folk",
			"Eurodance",			"Dream",				"Southern Rock",
			"Comedy",				"Cult",					"Gangsta",
			"Top 40",				"Christian Rap",		"Pop/Funk",
			"Jungle Music",			"Native US",			"Cabaret",
			"New Wave",				"Psychedelic",			"Rave",
			"Showtunes",			"Trailer",				"Lo-Fi",
			"Tribal",				"Acid Punk",			"Acid Jazz",
			"Polka",				"Retro",				"Musical",
			"Rock'n'Roll",			"Hard Rock",			"Folk",
			"Folk-Rock",			"National Folk",		"Swing",
			"Fast Fusion",			"Bebob",				"Latin",
			"Revival",				"Celtic",				"Bluegrass",
			"Avantgarde",			"Gothic Rock",			"Progessive Rock",
			"Psychedelic Rock",		"Symphonic Rock",		"Slow Rock",
			"Big Band",				"Chorus",				"Easy Listening",
			"Acoustic",				"Humour",				"Speech",
			"Chanson",				"Opera",				"Chamber Music",
			"Sonata",				"Symphony",				"Booty Bass",
			"Primus",				"Porn Groove",			"Satire",
			"Slow Jam",				"Club",					"Tango",
			"Samba",				"Folklore",				"Ballad",
			"Power Ballad",			"Rhythmic Soul",		"Freestyle",
			"Duet",					"Punk Rock",			"Drum Solo",
			"A Cappella",			"Euro-House",			"Dance Hall",
			"Goa Music",			"Drum & Bass",			"Club-House",
			"Hardcore Techno",		"Terror",				"Indie",
			"BritPop",				"Negerpunk",			"Polsk Punk",
			"Beat",					"Christian Gangsta Rap","Heavy Metal",
			"Black Metal",			"Crossover",			"Contemporary Christian",
			"Christian Rock",		"Merengue",				"Salsa",
			"Thrash Metal",			"Anime",				"JPop",
			"Synthpop",				"Abstract", 			"Art Rock",
			"Baroque",				"Bhangra",				"Big Beat",
			"Breakbeat",			"Chillout",				"Downtempo",
			"Dub",					"EBM",					"Eclectic",
			"Electro",				"Electroclash",			"Emo",
			"Experimental",			"Garage",				"Global",
			"IDM",					"Illbient",				"Industro-Goth",
			"Jam Band",				"Krautrock",			"Leftfield",
			"Lounge",				"Math Rock",			"New Romantic",
			"Nu-Breakz",			"Post-Punk",			"Post-Rock",
			"Psytrance",			"Shoegaze",				"Space Rock",
			"Trop Rock",			"World Music",			"Neoclassical",
			"Audiobook",			"Audio Theatre",		"Neue Deutsche Welle",
			"Podcast",				"Indie-Rock",			"G-Funk",
			"Dubstep",				"Garage Rock",			"Psybient"
		};
	}
}
