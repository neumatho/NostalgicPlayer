/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
global using ChannelFactorDictionary = System.Collections.Generic.Dictionary<Polycode.NostalgicPlayer.Kit.Containers.Flags.SpeakerFlag, System.Collections.Generic.Dictionary<Polycode.NostalgicPlayer.Kit.Containers.Flags.SpeakerFlag, float>>;

using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Sound
{
	/// <summary>
	/// Holds all the down-mixing tables.
	///
	/// Layout:
	///
	/// Index in array is the number of output channels. The item in the
	/// array is the down-mixing table to use. The table is a dictionary
	/// where the key is the output speaker. The value in this dictionary
	/// is yet another dictionary holding all possible input speakers and
	/// their factor.
	///
	/// Not all speakers are supported. Speakers not in this list, will
	/// not be used
	/// </summary>
	internal static class DownMixerTable
	{
		private const float None = 0.0f;
		private const float Full = 1.0f;
		private const float Half = 0.5f;
		private const float Sqrt2 = 0.707106781f;	// 1/Sqrt(2)
		private const float _2Sqrt2 = 0.35355339f;	// 1/2*Sqrt(2)
		private const float Sqrt3 = 0.577350269f;	// 1/Sqrt(3)
		private const float Sqrt4 = 0.5f;			// 1/Sqrt(4)
		private const float Sqrt5 = 0.447213595f;	// 1/Sqrt(5)
		private const float Sqrt6 = 0.40824829f;	// 1/Sqrt(6)
		private const float Sqrt7 = 0.377964473f;	// 1/Sqrt(7)

		public static readonly ChannelFactorDictionary[] ChannelFactors =
		[
			#region 1 output channel
			new()
			{
				{
					SpeakerFlag.FrontCenter, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, Sqrt2 },
						{ SpeakerFlag.FrontRight, Sqrt2 },
						{ SpeakerFlag.FrontCenter, Full },
						{ SpeakerFlag.SideLeft, Half },
						{ SpeakerFlag.SideRight, Half },
						{ SpeakerFlag.BackLeft, Half },
						{ SpeakerFlag.BackRight, Half },
						{ SpeakerFlag.TopCenter, Full },
						{ SpeakerFlag.TopFrontLeft, Half },
						{ SpeakerFlag.TopFrontCenter, Sqrt2 },
						{ SpeakerFlag.TopFrontRight, Half },
						{ SpeakerFlag.TopBackLeft, _2Sqrt2 },
						{ SpeakerFlag.TopBackRight, _2Sqrt2 }
					}
				}
			},
			#endregion

			#region 2 output channels
			new()
			{
				{
					SpeakerFlag.FrontLeft, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, Full },
						{ SpeakerFlag.FrontRight, None },
						{ SpeakerFlag.FrontCenter, Sqrt2 },
						{ SpeakerFlag.SideLeft, Sqrt2 },
						{ SpeakerFlag.SideRight, None },
						{ SpeakerFlag.BackLeft, Sqrt2 },
						{ SpeakerFlag.BackRight, None },
						{ SpeakerFlag.TopCenter, Sqrt2 },
						{ SpeakerFlag.TopFrontLeft, Sqrt2 },
						{ SpeakerFlag.TopFrontCenter, Half },
						{ SpeakerFlag.TopFrontRight, None },
						{ SpeakerFlag.TopBackLeft, Half },
						{ SpeakerFlag.TopBackRight, None }
					}
				},
				{
					SpeakerFlag.FrontRight, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, None },
						{ SpeakerFlag.FrontRight, Full },
						{ SpeakerFlag.FrontCenter, Sqrt2 },
						{ SpeakerFlag.SideLeft, None },
						{ SpeakerFlag.SideRight, Sqrt2 },
						{ SpeakerFlag.BackLeft, None },
						{ SpeakerFlag.BackRight, Sqrt2 },
						{ SpeakerFlag.TopCenter, Sqrt2 },
						{ SpeakerFlag.TopFrontLeft, None },
						{ SpeakerFlag.TopFrontCenter, Half },
						{ SpeakerFlag.TopFrontRight, Sqrt2 },
						{ SpeakerFlag.TopBackLeft, None },
						{ SpeakerFlag.TopBackRight, Half }
					}
				}
			},
			#endregion

			#region 3 output channels
			new()
			{
				{
					SpeakerFlag.FrontLeft, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, Full },
						{ SpeakerFlag.FrontRight, None },
						{ SpeakerFlag.FrontCenter, None },
						{ SpeakerFlag.SideLeft, Sqrt2 },
						{ SpeakerFlag.SideRight, None },
						{ SpeakerFlag.BackLeft, Sqrt2 },
						{ SpeakerFlag.BackRight, None },
						{ SpeakerFlag.TopCenter, Sqrt3 },
						{ SpeakerFlag.TopFrontLeft, Sqrt2 },
						{ SpeakerFlag.TopFrontCenter, None },
						{ SpeakerFlag.TopFrontRight, None },
						{ SpeakerFlag.TopBackLeft, Half },
						{ SpeakerFlag.TopBackRight, None }
					}
				},
				{
					SpeakerFlag.FrontRight, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, None },
						{ SpeakerFlag.FrontRight, Full },
						{ SpeakerFlag.FrontCenter, None },
						{ SpeakerFlag.SideLeft, None },
						{ SpeakerFlag.SideRight, Sqrt2 },
						{ SpeakerFlag.BackLeft, None },
						{ SpeakerFlag.BackRight, Sqrt2 },
						{ SpeakerFlag.TopCenter, Sqrt3 },
						{ SpeakerFlag.TopFrontLeft, None },
						{ SpeakerFlag.TopFrontCenter, None },
						{ SpeakerFlag.TopFrontRight, Sqrt2 },
						{ SpeakerFlag.TopBackLeft, None },
						{ SpeakerFlag.TopBackRight, Half }
					}
				},
				{
					SpeakerFlag.FrontCenter, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, None },
						{ SpeakerFlag.FrontRight, None },
						{ SpeakerFlag.FrontCenter, Full },
						{ SpeakerFlag.SideLeft, None },
						{ SpeakerFlag.SideRight, None },
						{ SpeakerFlag.BackLeft, None },
						{ SpeakerFlag.BackRight, None },
						{ SpeakerFlag.TopCenter, Sqrt3 },
						{ SpeakerFlag.TopFrontLeft, None },
						{ SpeakerFlag.TopFrontCenter, Sqrt2 },
						{ SpeakerFlag.TopFrontRight, None },
						{ SpeakerFlag.TopBackLeft, None },
						{ SpeakerFlag.TopBackRight, None }
					}
				}
			},
			#endregion

			#region 4 output channels
			new()
			{
				{
					SpeakerFlag.FrontLeft, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, Full },
						{ SpeakerFlag.FrontRight, None },
						{ SpeakerFlag.FrontCenter, Sqrt2 },
						{ SpeakerFlag.SideLeft, Sqrt2 },
						{ SpeakerFlag.SideRight, None },
						{ SpeakerFlag.BackLeft, None },
						{ SpeakerFlag.BackRight, None },
						{ SpeakerFlag.TopCenter, Sqrt4 },
						{ SpeakerFlag.TopFrontLeft, Sqrt2 },
						{ SpeakerFlag.TopFrontCenter, Half },
						{ SpeakerFlag.TopFrontRight, None },
						{ SpeakerFlag.TopBackLeft, None },
						{ SpeakerFlag.TopBackRight, None }
					}
				},
				{
					SpeakerFlag.FrontRight, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, None },
						{ SpeakerFlag.FrontRight, Full },
						{ SpeakerFlag.FrontCenter, Sqrt2 },
						{ SpeakerFlag.SideLeft, None },
						{ SpeakerFlag.SideRight, Sqrt2 },
						{ SpeakerFlag.BackLeft, None },
						{ SpeakerFlag.BackRight, None },
						{ SpeakerFlag.TopCenter, Sqrt4 },
						{ SpeakerFlag.TopFrontLeft, None },
						{ SpeakerFlag.TopFrontCenter, Half },
						{ SpeakerFlag.TopFrontRight, Sqrt2 },
						{ SpeakerFlag.TopBackLeft, None },
						{ SpeakerFlag.TopBackRight, None }
					}
				},
				{
					SpeakerFlag.BackLeft, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, None },
						{ SpeakerFlag.FrontRight, None },
						{ SpeakerFlag.FrontCenter, None },
						{ SpeakerFlag.SideLeft, Sqrt2 },
						{ SpeakerFlag.SideRight, None },
						{ SpeakerFlag.BackLeft, Full },
						{ SpeakerFlag.BackRight, None },
						{ SpeakerFlag.TopCenter, Sqrt4 },
						{ SpeakerFlag.TopFrontLeft, None },
						{ SpeakerFlag.TopFrontCenter, None },
						{ SpeakerFlag.TopFrontRight, None },
						{ SpeakerFlag.TopBackLeft, Sqrt2 },
						{ SpeakerFlag.TopBackRight, None }
					}
				},
				{
					SpeakerFlag.BackRight, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, None },
						{ SpeakerFlag.FrontRight, None },
						{ SpeakerFlag.FrontCenter, None },
						{ SpeakerFlag.SideLeft, None },
						{ SpeakerFlag.SideRight, Sqrt2 },
						{ SpeakerFlag.BackLeft, None },
						{ SpeakerFlag.BackRight, Full },
						{ SpeakerFlag.TopCenter, Sqrt4 },
						{ SpeakerFlag.TopFrontLeft, None },
						{ SpeakerFlag.TopFrontCenter, None },
						{ SpeakerFlag.TopFrontRight, None },
						{ SpeakerFlag.TopBackLeft, None },
						{ SpeakerFlag.TopBackRight, Sqrt2 }
					}
				}
			},
			#endregion

			#region 5 output channels
			new()
			{
				{
					SpeakerFlag.FrontLeft, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, Full },
						{ SpeakerFlag.FrontRight, None },
						{ SpeakerFlag.FrontCenter, None },
						{ SpeakerFlag.SideLeft, Sqrt2 },
						{ SpeakerFlag.SideRight, None },
						{ SpeakerFlag.BackLeft, None },
						{ SpeakerFlag.BackRight, None },
						{ SpeakerFlag.TopCenter, Sqrt5 },
						{ SpeakerFlag.TopFrontLeft, Sqrt2 },
						{ SpeakerFlag.TopFrontCenter, None },
						{ SpeakerFlag.TopFrontRight, None },
						{ SpeakerFlag.TopBackLeft, None },
						{ SpeakerFlag.TopBackRight, None }
					}
				},
				{
					SpeakerFlag.FrontRight, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, None },
						{ SpeakerFlag.FrontRight, Full },
						{ SpeakerFlag.FrontCenter, None },
						{ SpeakerFlag.SideLeft, None },
						{ SpeakerFlag.SideRight, Sqrt2 },
						{ SpeakerFlag.BackLeft, None },
						{ SpeakerFlag.BackRight, None },
						{ SpeakerFlag.TopCenter, Sqrt5 },
						{ SpeakerFlag.TopFrontLeft, None },
						{ SpeakerFlag.TopFrontCenter, None },
						{ SpeakerFlag.TopFrontRight, Sqrt2 },
						{ SpeakerFlag.TopBackLeft, None },
						{ SpeakerFlag.TopBackRight, None }
					}
				},
				{
					SpeakerFlag.FrontCenter, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, None },
						{ SpeakerFlag.FrontRight, None },
						{ SpeakerFlag.FrontCenter, Full },
						{ SpeakerFlag.SideLeft, None },
						{ SpeakerFlag.SideRight, None },
						{ SpeakerFlag.BackLeft, None },
						{ SpeakerFlag.BackRight, None },
						{ SpeakerFlag.TopCenter, Sqrt5 },
						{ SpeakerFlag.TopFrontLeft, None },
						{ SpeakerFlag.TopFrontCenter, Sqrt2 },
						{ SpeakerFlag.TopFrontRight, None },
						{ SpeakerFlag.TopBackLeft, None },
						{ SpeakerFlag.TopBackRight, None }
					}
				},
				{
					SpeakerFlag.BackLeft, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, None },
						{ SpeakerFlag.FrontRight, None },
						{ SpeakerFlag.FrontCenter, None },
						{ SpeakerFlag.SideLeft, Sqrt2 },
						{ SpeakerFlag.SideRight, None },
						{ SpeakerFlag.BackLeft, Full },
						{ SpeakerFlag.BackRight, None },
						{ SpeakerFlag.TopCenter, Sqrt5 },
						{ SpeakerFlag.TopFrontLeft, None },
						{ SpeakerFlag.TopFrontCenter, None },
						{ SpeakerFlag.TopFrontRight, None },
						{ SpeakerFlag.TopBackLeft, Sqrt2 },
						{ SpeakerFlag.TopBackRight, None }
					}
				},
				{
					SpeakerFlag.BackRight, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, None },
						{ SpeakerFlag.FrontRight, None },
						{ SpeakerFlag.FrontCenter, None },
						{ SpeakerFlag.SideLeft, None },
						{ SpeakerFlag.SideRight, Sqrt2 },
						{ SpeakerFlag.BackLeft, None },
						{ SpeakerFlag.BackRight, Full },
						{ SpeakerFlag.TopCenter, Sqrt5 },
						{ SpeakerFlag.TopFrontLeft, None },
						{ SpeakerFlag.TopFrontCenter, None },
						{ SpeakerFlag.TopFrontRight, None },
						{ SpeakerFlag.TopBackLeft, None },
						{ SpeakerFlag.TopBackRight, Sqrt2 }
					}
				}
			},
			#endregion

			#region 6 output channels
			new()
			{
				{
					SpeakerFlag.FrontLeft, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, Full },
						{ SpeakerFlag.FrontRight, None },
						{ SpeakerFlag.FrontCenter, Sqrt2 },
						{ SpeakerFlag.SideLeft, None },
						{ SpeakerFlag.SideRight, None },
						{ SpeakerFlag.BackLeft, None },
						{ SpeakerFlag.BackRight, None },
						{ SpeakerFlag.TopCenter, Sqrt6 },
						{ SpeakerFlag.TopFrontLeft, Sqrt2 },
						{ SpeakerFlag.TopFrontCenter, Half },
						{ SpeakerFlag.TopFrontRight, None },
						{ SpeakerFlag.TopBackLeft, None },
						{ SpeakerFlag.TopBackRight, None }
					}
				},
				{
					SpeakerFlag.FrontRight, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, None },
						{ SpeakerFlag.FrontRight, Full },
						{ SpeakerFlag.FrontCenter, Sqrt2 },
						{ SpeakerFlag.SideLeft, None },
						{ SpeakerFlag.SideRight, None },
						{ SpeakerFlag.BackLeft, None },
						{ SpeakerFlag.BackRight, None },
						{ SpeakerFlag.TopCenter, Sqrt6 },
						{ SpeakerFlag.TopFrontLeft, None },
						{ SpeakerFlag.TopFrontCenter, Half },
						{ SpeakerFlag.TopFrontRight, Sqrt2 },
						{ SpeakerFlag.TopBackLeft, None },
						{ SpeakerFlag.TopBackRight, None }
					}
				},
				{
					SpeakerFlag.SideLeft, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, None },
						{ SpeakerFlag.FrontRight, None },
						{ SpeakerFlag.FrontCenter, None },
						{ SpeakerFlag.SideLeft, Full },
						{ SpeakerFlag.SideRight, None },
						{ SpeakerFlag.BackLeft, None },
						{ SpeakerFlag.BackRight, None },
						{ SpeakerFlag.TopCenter, Sqrt6 },
						{ SpeakerFlag.TopFrontLeft, None },
						{ SpeakerFlag.TopFrontCenter, None },
						{ SpeakerFlag.TopFrontRight, None },
						{ SpeakerFlag.TopBackLeft, None },
						{ SpeakerFlag.TopBackRight, None }
					}
				},
				{
					SpeakerFlag.SideRight, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, None },
						{ SpeakerFlag.FrontRight, None },
						{ SpeakerFlag.FrontCenter, None },
						{ SpeakerFlag.SideLeft, None },
						{ SpeakerFlag.SideRight, Full },
						{ SpeakerFlag.BackLeft, None },
						{ SpeakerFlag.BackRight, None },
						{ SpeakerFlag.TopCenter, Sqrt6 },
						{ SpeakerFlag.TopFrontLeft, None },
						{ SpeakerFlag.TopFrontCenter, None },
						{ SpeakerFlag.TopFrontRight, None },
						{ SpeakerFlag.TopBackLeft, None },
						{ SpeakerFlag.TopBackRight, None }
					}
				},
				{
					SpeakerFlag.BackLeft, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, None },
						{ SpeakerFlag.FrontRight, None },
						{ SpeakerFlag.FrontCenter, None },
						{ SpeakerFlag.SideLeft, None },
						{ SpeakerFlag.SideRight, None },
						{ SpeakerFlag.BackLeft, Full },
						{ SpeakerFlag.BackRight, None },
						{ SpeakerFlag.TopCenter, Sqrt6 },
						{ SpeakerFlag.TopFrontLeft, None },
						{ SpeakerFlag.TopFrontCenter, None },
						{ SpeakerFlag.TopFrontRight, None },
						{ SpeakerFlag.TopBackLeft, Sqrt2 },
						{ SpeakerFlag.TopBackRight, None }
					}
				},
				{
					SpeakerFlag.BackRight, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, None },
						{ SpeakerFlag.FrontRight, None },
						{ SpeakerFlag.FrontCenter, None },
						{ SpeakerFlag.SideLeft, None },
						{ SpeakerFlag.SideRight, None },
						{ SpeakerFlag.BackLeft, None },
						{ SpeakerFlag.BackRight, Full },
						{ SpeakerFlag.TopCenter, Sqrt6 },
						{ SpeakerFlag.TopFrontLeft, None },
						{ SpeakerFlag.TopFrontCenter, None },
						{ SpeakerFlag.TopFrontRight, None },
						{ SpeakerFlag.TopBackLeft, None },
						{ SpeakerFlag.TopBackRight, Sqrt2 }
					}
				}
			},
			#endregion

			#region 7 output channels
			new()
			{
				{
					SpeakerFlag.FrontLeft, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, Full },
						{ SpeakerFlag.FrontRight, None },
						{ SpeakerFlag.FrontCenter, None },
						{ SpeakerFlag.SideLeft, None },
						{ SpeakerFlag.SideRight, None },
						{ SpeakerFlag.BackLeft, None },
						{ SpeakerFlag.BackRight, None },
						{ SpeakerFlag.TopCenter, Sqrt7 },
						{ SpeakerFlag.TopFrontLeft, Sqrt2 },
						{ SpeakerFlag.TopFrontCenter, None },
						{ SpeakerFlag.TopFrontRight, None },
						{ SpeakerFlag.TopBackLeft, None },
						{ SpeakerFlag.TopBackRight, None }
					}
				},
				{
					SpeakerFlag.FrontRight, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, None },
						{ SpeakerFlag.FrontRight, Full },
						{ SpeakerFlag.FrontCenter, None },
						{ SpeakerFlag.SideLeft, None },
						{ SpeakerFlag.SideRight, None },
						{ SpeakerFlag.BackLeft, None },
						{ SpeakerFlag.BackRight, None },
						{ SpeakerFlag.TopCenter, Sqrt7 },
						{ SpeakerFlag.TopFrontLeft, None },
						{ SpeakerFlag.TopFrontCenter, None },
						{ SpeakerFlag.TopFrontRight, Sqrt2 },
						{ SpeakerFlag.TopBackLeft, None },
						{ SpeakerFlag.TopBackRight, None }
					}
				},
				{
					SpeakerFlag.FrontCenter, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, None },
						{ SpeakerFlag.FrontRight, None },
						{ SpeakerFlag.FrontCenter, Full },
						{ SpeakerFlag.SideLeft, None },
						{ SpeakerFlag.SideRight, None },
						{ SpeakerFlag.BackLeft, None },
						{ SpeakerFlag.BackRight, None },
						{ SpeakerFlag.TopCenter, Sqrt7 },
						{ SpeakerFlag.TopFrontLeft, None },
						{ SpeakerFlag.TopFrontCenter, Sqrt2 },
						{ SpeakerFlag.TopFrontRight, None },
						{ SpeakerFlag.TopBackLeft, None },
						{ SpeakerFlag.TopBackRight, None }
					}
				},
				{
					SpeakerFlag.SideLeft, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, None },
						{ SpeakerFlag.FrontRight, None },
						{ SpeakerFlag.FrontCenter, None },
						{ SpeakerFlag.SideLeft, Full },
						{ SpeakerFlag.SideRight, None },
						{ SpeakerFlag.BackLeft, None },
						{ SpeakerFlag.BackRight, None },
						{ SpeakerFlag.TopCenter, Sqrt7 },
						{ SpeakerFlag.TopFrontLeft, None },
						{ SpeakerFlag.TopFrontCenter, None },
						{ SpeakerFlag.TopFrontRight, None },
						{ SpeakerFlag.TopBackLeft, None },
						{ SpeakerFlag.TopBackRight, None }
					}
				},
				{
					SpeakerFlag.SideRight, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, None },
						{ SpeakerFlag.FrontRight, None },
						{ SpeakerFlag.FrontCenter, None },
						{ SpeakerFlag.SideLeft, None },
						{ SpeakerFlag.SideRight, Full },
						{ SpeakerFlag.BackLeft, None },
						{ SpeakerFlag.BackRight, None },
						{ SpeakerFlag.TopCenter, Sqrt7 },
						{ SpeakerFlag.TopFrontLeft, None },
						{ SpeakerFlag.TopFrontCenter, None },
						{ SpeakerFlag.TopFrontRight, None },
						{ SpeakerFlag.TopBackLeft, None },
						{ SpeakerFlag.TopBackRight, None }
					}
				},
				{
					SpeakerFlag.BackLeft, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, None },
						{ SpeakerFlag.FrontRight, None },
						{ SpeakerFlag.FrontCenter, None },
						{ SpeakerFlag.SideLeft, None },
						{ SpeakerFlag.SideRight, None },
						{ SpeakerFlag.BackLeft, Full },
						{ SpeakerFlag.BackRight, None },
						{ SpeakerFlag.TopCenter, Sqrt7 },
						{ SpeakerFlag.TopFrontLeft, None },
						{ SpeakerFlag.TopFrontCenter, None },
						{ SpeakerFlag.TopFrontRight, None },
						{ SpeakerFlag.TopBackLeft, Sqrt2 },
						{ SpeakerFlag.TopBackRight, None }
					}
				},
				{
					SpeakerFlag.BackRight, new Dictionary<SpeakerFlag, float>
					{
						{ SpeakerFlag.FrontLeft, None },
						{ SpeakerFlag.FrontRight, None },
						{ SpeakerFlag.FrontCenter, None },
						{ SpeakerFlag.SideLeft, None },
						{ SpeakerFlag.SideRight, None },
						{ SpeakerFlag.BackLeft, None },
						{ SpeakerFlag.BackRight, Full },
						{ SpeakerFlag.TopCenter, Sqrt7 },
						{ SpeakerFlag.TopFrontLeft, None },
						{ SpeakerFlag.TopFrontCenter, None },
						{ SpeakerFlag.TopFrontRight, None },
						{ SpeakerFlag.TopBackLeft, None },
						{ SpeakerFlag.TopBackRight, Sqrt2 }
					}
				}
			},
			#endregion
		];
	}
}
