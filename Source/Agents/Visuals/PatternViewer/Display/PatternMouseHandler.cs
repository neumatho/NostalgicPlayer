//---------------------------------------------------------------------------------------
// <copyright file="PatternMouseHandler.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System.Drawing;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.ControlBar;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Tracker;
using Polycode.NostalgicPlayer.Kit.Gui.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Display
{
	/// <summary>
	/// Handles mouse interactions for the pattern control
	/// </summary>
	internal static class PatternMouseHandler
	{
		/********************************************************************/
		/// <summary>
		/// Handle mouse clicks on scroll buttons and channel headers
		/// </summary>
		/// <returns>True if a redraw is needed</returns>
		/********************************************************************/
		public static bool HandleMouseClick(PatternRenderer renderer, Point location, IClientPlayerControl clientPlayer)
		{
			// Check control bar clicks first (if visible)
			if (renderer.ShowControlBar)
			{
				// Build state for disabled button checking
				ControlBarState state = new()
				{
					ModuleIndex = renderer.ModuleIndex,
					ModuleCount = renderer.ModuleCount,
					SubSongCurrent = renderer.SubSongCurrent,
					SubSongCount = renderer.SubSongTotal,
					SnapshotPosition = renderer.SnapshotPosition,
					SnapshotCount = renderer.SnapshotCount
				};

				// Get action from the registered control bar handler
				ControlBarHandlers handlers = TrackerRegistry.GetControlBarHandlers(renderer.CurrentStyleId);
				ControlBarAction action = handlers != null
					? handlers.HandleClick(location, state)
					: ControlBarAction.None;

				if (action.Type != ControlBarActionType.None)
				{
					HandleControlBarAction(action, clientPlayer);
					return true;
				}
			}

			// Check left scroll button
			if (renderer.LeftScrollButtonRect.Contains(location) && renderer.FirstVisibleChannel > 0)
			{
				renderer.FirstVisibleChannel--;
				return true;
			}

			// Check right scroll button
			if (renderer.RightScrollButtonRect.Contains(location) &&
			    renderer.FirstVisibleChannel < renderer.ChannelCount - 1)
			{
				renderer.FirstVisibleChannel++;
				return true;
			}

			// Check channel bar for mute toggle
			for (int ch = 0; ch < renderer.ChannelCount; ch++)
			{
				if (renderer.ChannelBarRects[ch].Contains(location))
				{
					// Toggle channel mute state
					bool currentlyMuted = renderer.IsChannelMuted(ch);
					clientPlayer?.EnableChannels(currentlyMuted, ch); // Enable if muted, disable if not
					break;
				}
			}

			return false;
		}

		/********************************************************************/
		/// <summary>
		/// Handle control bar action
		/// </summary>
		/********************************************************************/
		private static void HandleControlBarAction(ControlBarAction action, IClientPlayerControl clientPlayer)
		{
			if (clientPlayer == null)
			{
				return;
			}

			try
			{
				switch (action.Type)
				{
					case ControlBarActionType.PrevModule:
						clientPlayer.PreviousModule();
						break;

					case ControlBarActionType.PrevSubSong:
						clientPlayer.PreviousSubSong();
						break;

					case ControlBarActionType.PlayPause:
						if (clientPlayer.IsPaused)
						{
							clientPlayer.ResumePlaying();
						}
						else if (clientPlayer.IsPlaying)
						{
							clientPlayer.PausePlaying();
						}
						else
						{
							clientPlayer.PlayModule();
						}

						break;

					case ControlBarActionType.Stop:
						clientPlayer.StopModule();
						break;

					case ControlBarActionType.NextSubSong:
						clientPlayer.NextSubSong();
						break;

					case ControlBarActionType.NextModule:
						clientPlayer.NextModule();
						break;

					case ControlBarActionType.PrevSnapshot:
					{
						int currentPos = clientPlayer.SnapshotPosition;
						if (currentPos > 0)
						{
							clientPlayer.SetSnapshotPosition(currentPos - 1);
						}

						break;
					}

					case ControlBarActionType.Restart:
						clientPlayer.RestartSong();
						break;

					case ControlBarActionType.NextSnapshot:
					{
						int currentPos = clientPlayer.SnapshotPosition;
						int count = clientPlayer.SnapshotCount;
						if (currentPos < count - 1)
						{
							clientPlayer.SetSnapshotPosition(currentPos + 1);
						}

						break;
					}

					case ControlBarActionType.SetPosition:
						clientPlayer.SetSnapshotPosition(action.Position);
						break;
				}
			}
			catch
			{
				// Ignore errors if API methods are not available
			}
		}

		/********************************************************************/
		/// <summary>
		/// Handle mouse down for button press visual feedback
		/// </summary>
		/// <returns>True if a redraw is needed</returns>
		/********************************************************************/
		public static bool HandleMouseDown(PatternRenderer renderer, Point location)
		{
			if (!renderer.ShowControlBar)
			{
				return false;
			}

			// Build state for disabled button checking
			ControlBarState state = new()
			{
				ModuleIndex = renderer.ModuleIndex,
				ModuleCount = renderer.ModuleCount,
				SubSongCurrent = renderer.SubSongCurrent,
				SubSongCount = renderer.SubSongTotal,
				SnapshotPosition = renderer.SnapshotPosition,
				SnapshotCount = renderer.SnapshotCount
			};

			// Get which button is at this location using registered handler
			ControlBarHandlers handlers = TrackerRegistry.GetControlBarHandlers(renderer.CurrentStyleId);
			PressedButton pressed = PressedButton.None;

			if (handlers?.GetRects != null)
			{
				ControlBarRects rects = handlers.GetRects();
				if (rects != null)
				{
					pressed = ControlBarLayout.GetButtonAtLocation(rects, location, state);
				}
			}

			if (pressed != PressedButton.None)
			{
				renderer.ControlBarPressedButton = pressed;
				return true;
			}

			return false;
		}

		/********************************************************************/
		/// <summary>
		/// Handle mouse up to release button press
		/// </summary>
		/// <returns>True if a redraw is needed</returns>
		/********************************************************************/
		public static bool HandleMouseUp(PatternRenderer renderer)
		{
			if (renderer.ControlBarPressedButton != PressedButton.None)
			{
				renderer.ControlBarPressedButton = PressedButton.None;
				return true;
			}

			return false;
		}

		/********************************************************************/
		/// <summary>
		/// Handle mouse movement for scroll button hover effects
		/// </summary>
		/// <returns>True if a redraw is needed, also outputs the new cursor</returns>
		/********************************************************************/
		public static bool HandleMouseMove(PatternRenderer renderer, Point location, out Cursor newCursor)
		{
			bool oldLeftHover = renderer.LeftScrollButtonHover;
			bool oldRightHover = renderer.RightScrollButtonHover;

			// Check if mouse is over scroll buttons (only if they have valid size)
			renderer.LeftScrollButtonHover = renderer.LeftScrollButtonRect.Width > 0 &&
			                                 renderer.LeftScrollButtonRect.Contains(location);
			renderer.RightScrollButtonHover = renderer.RightScrollButtonRect.Width > 0 &&
			                                  renderer.RightScrollButtonRect.Contains(location);

			// Determine cursor
			if (renderer.LeftScrollButtonHover || renderer.RightScrollButtonHover)
			{
				newCursor = Cursors.Hand;
			}
			else
			{
				newCursor = Cursors.Default;
			}

			// Return true if hover state changed
			return oldLeftHover != renderer.LeftScrollButtonHover || oldRightHover != renderer.RightScrollButtonHover;
		}
	}
}
