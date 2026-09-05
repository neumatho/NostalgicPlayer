/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common
{
	/// <summary>
	/// 
	/// </summary>
	internal class VersionWithRevision
	{
		public Version Version;
		public uint64 Revision;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private VersionWithRevision(Version version, uint64 revision)
		{
			Version = version;
			Revision = revision;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasRevision()
		{
			return Revision != 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsEqualTo(VersionWithRevision other)
		{
			return (Version == other.Version) && (Revision == other.Revision);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsEquivalentTo(VersionWithRevision other)
		{
			if ((Version == other.Version) && (Revision == other.Revision))
				return true;

			if (HasRevision() && other.HasRevision())
				return false;

			return Version == other.Version;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsNewerThan(VersionWithRevision other)
		{
			if (Version < other.Version)
				return false;

			if (Version > other.Version)
				return true;

			if (!HasRevision() && !other.HasRevision())
				return false;

			if (HasRevision() && other.HasRevision())
			{
				if (Revision < other.Revision)
					return false;

				if (Revision > other.Revision)
					return true;

				return false;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsOlderThan(VersionWithRevision other)
		{
			if (Version < other.Version)
				return true;

			if (Version > other.Version)
				return false;

			if (!HasRevision() && !other.HasRevision())
				return false;

			if (HasRevision() && other.HasRevision())
			{
				if (Revision < other.Revision)
					return true;

				if (Revision > other.Revision)
					return false;

				return false;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static VersionWithRevision Parse(string s)
		{
			Version version = new Version();
			uint64 revision = 0;
			string[] tokens = s.Split('-');

			if (tokens.Length >= 1)
				version = Version.Parse(tokens[0]);

			if (tokens.Length >= 2)
				uint64.TryParse(tokens[1].Substring(1), out revision);

			return new VersionWithRevision(version, revision);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override string ToString()
		{
			if (!HasRevision())
				return Version.ToString();

			if (!Version.IsTestVersion())
				return Version.ToString();

			return string.Format("{0}-r{1}", Version, Revision);
		}
	}
}
