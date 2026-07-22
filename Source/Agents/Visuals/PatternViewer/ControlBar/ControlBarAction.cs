//---------------------------------------------------------------------------------------
// <copyright file="ControlBarAction.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.ControlBar
{
	/// <summary>
	/// Action to perform from control bar click
	/// </summary>
	internal readonly struct ControlBarAction
	{
		public ControlBarActionType Type
		{
			get;
		}

		public int Position
		{
			get;
		}

		private ControlBarAction(ControlBarActionType type, int position = 0)
		{
			Type = type;
			Position = position;
		}

		public static ControlBarAction None => new(ControlBarActionType.None);
		public static ControlBarAction PrevModule => new(ControlBarActionType.PrevModule);
		public static ControlBarAction PrevSubSong => new(ControlBarActionType.PrevSubSong);
		public static ControlBarAction PrevSnapshot => new(ControlBarActionType.PrevSnapshot);
		public static ControlBarAction Restart => new(ControlBarActionType.Restart);
		public static ControlBarAction PlayPause => new(ControlBarActionType.PlayPause);
		public static ControlBarAction Stop => new(ControlBarActionType.Stop);
		public static ControlBarAction NextSnapshot => new(ControlBarActionType.NextSnapshot);
		public static ControlBarAction NextSubSong => new(ControlBarActionType.NextSubSong);
		public static ControlBarAction NextModule => new(ControlBarActionType.NextModule);

		public static ControlBarAction SetPosition(int position)
		{
			return new ControlBarAction(ControlBarActionType.SetPosition, position);
		}
	}

	/// <summary>
	/// Type of control bar action
	/// </summary>
	internal enum ControlBarActionType
	{
		None,
		PrevModule,
		PrevSubSong,
		PrevSnapshot,
		Restart,
		PlayPause,
		Stop,
		NextSnapshot,
		NextSubSong,
		NextModule,
		SetPosition
	}
}
