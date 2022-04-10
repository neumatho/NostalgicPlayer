/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Containers
{
	/// <summary>
	/// Holds all the possible expansion tags
	/// </summary>
	internal static class ExpansionTags
	{
		/// <summary>
		/// End of tags
		/// </summary>
		public const uint TagEnd = 0;

		/// <summary>
		/// Data needs relocation
		/// </summary>
		public const uint TagPtr = 0x80000000;

		/// <summary>
		/// Loader must fail if this isn't recognized
		/// </summary>
		public const uint TagMustKnow = 0x40000000;

		/// <summary>
		/// Loader must warn if this isn't recognized
		/// </summary>
		public const uint TagMustWarn = 0x20000000;

		public enum Tags : uint
		{
			/// <summary>
			/// End of list
			/// </summary>
			End = TagEnd,

			/// <summary>
			/// Number of effect groups, including the global group (will
			/// override settings in MMDSong struct), default = 1
			/// </summary>
			NumberOfEffectGroups = 1
		}

		public enum EffectTags : uint
		{
			/// <summary>
			/// End of list
			/// </summary>
			End = TagEnd,

			/// <summary></summary>
			EchoType = 1,

			/// <summary></summary>
			EchoLen = 2,

			/// <summary></summary>
			EchoDepth = 3,

			/// <summary></summary>
			StereoSeparation = 4,

			/// <summary>
			/// The global effects group shouldn't have name saved!!
			/// </summary>
			GroupName = 5 | TagPtr,

			/// <summary>
			/// Includes zero terminator
			/// </summary>
			GroupNameLen = 6
		}

		public enum TrackTags : uint
		{
			/// <summary>
			/// End of list
			/// </summary>
			End = TagEnd,

			/// <summary></summary>
			Name = 1 | TagPtr,

			/// <summary>
			/// Includes zero terminator
			/// </summary>
			NameLen = 2,

			/// <summary>
			/// Which group the track belongs
			/// </summary>
			FxGroup = 3
		}
	}
}
