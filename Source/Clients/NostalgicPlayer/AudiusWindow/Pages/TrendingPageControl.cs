/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Krypton.Toolkit;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.Pages
{
	/// <summary>
	/// Holds all the controls for the Trending tab
	/// </summary>
	public partial class TrendingPageControl : UserControl
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public TrendingPageControl()
		{
			InitializeComponent();

			genreComboBox.Items.AddRange(
			[
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ALL) { Tag = "All Genres" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC) { Tag = "Electronic" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ROCK) { Tag = "Rock" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_METAL) { Tag = "Metal" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ALTERNATIVE) { Tag = "Alternative" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_HIP_HOP_RAP) { Tag = "Hip-Hop/Rap" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_EXPERIMENTAL) { Tag = "Experimental" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_PUNK) { Tag = "Punk" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_FOLK) { Tag = "Folk" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_POP) { Tag = "Pop" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_AMBIENT) { Tag = "Ambient" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_SOUNDTRACK) { Tag = "Soundtrack" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_WORLD) { Tag = "World" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_JAZZ) { Tag = "Jazz" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ACOUSTIC) { Tag = "Acoustic" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_FUNK) { Tag = "Funk" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_R_AND_B_SOUL) { Tag = "R&B/Soul" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_DEVOTIONAL) { Tag = "Devotional" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_CLASSICAL) { Tag = "Classical" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_REGGAE) { Tag = "Reggae" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_PODCASTS) { Tag = "Podcasts" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_COUNTRY) { Tag = "Country" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_SPOKEN_WORK) { Tag = "Spoken Word" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_COMEDY) { Tag = "Comedy" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_BLUES) { Tag = "Blues" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_KIDS) { Tag = "Kids" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_AUDIOBOOKS) { Tag = "Audiobooks" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_LATIN) { Tag = "Latin" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_LOFI) { Tag = "Lo-Fi" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_HYPERPOP) { Tag = "Hyperpop" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_DANCEHALL) { Tag = "Dancehall" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_TECHNO) { Tag = "Techno" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_TRAP) { Tag = "Trap" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_HOUSE) { Tag = "House" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_TECH_HOUSE) { Tag = "Tech House" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_DEEP_HOUSE) { Tag = "Deep House" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_DISCO) { Tag = "Disco" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_ELECTRO) { Tag = "Electro" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_JUNGLE) { Tag = "Jungle" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_PROGRESSIVE_HOUSE) { Tag = "Progressive House" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_HARDSTYLE) { Tag = "Hardstyle" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_GLITCH_HOP) { Tag = "Glitch Hop" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_TRANCE) { Tag = "Trance" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_FUTURE_BASS) { Tag = "Future Bass" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_FUTURE_HOUSE) { Tag = "Future House" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_TROPICAL_HOUSE) { Tag = "Tropical House" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_DOWNTEMPO) { Tag = "Downtempo" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_DRUM_AND_BASS) { Tag = "Drum & Bass" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_DUBSTEP) { Tag = "Dubstep" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_JERSEY_CLUB) { Tag = "Jersey Club" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_VAPORWAVE) { Tag = "Vaporwave" },
				new KryptonListItem(Resources.IDS_AUDIUS_TAB_TRENDING_GENRE_ELECTRONIC_MOOMBAHTON) { Tag = "Moombahton" }
			]);
			genreComboBox.SelectedIndex = 0;

			List<AudiusListItem> items = new List<AudiusListItem>();
			for (int i = 0; i < 100; i++)
				items.Add(new AudiusListItem(
					i + 1,
					"Hej",
					"affdgdjklasjgklasjgklasjgijeiosrjtioerjtierujtioeuotijgdfiljsdklfgjlsdkjgklsdjgkljsdglkdjgkljsdlgjlsdgj",
					TimeSpan.FromSeconds(Random.Shared.Next(900)),
					Random.Shared.Next(2000),
					Random.Shared.Next(2000),
					Random.Shared.Next(2000)
					));

			audiusListControl.SetItems(items);
		}

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TypeTracks_CheckedChanged(object sender, EventArgs e)
		{
			genreComboBox.Enabled = typeTracksRadioButton.Checked;
		}
		#endregion
	}
}
