/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.C.Std.Iterators;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// 
	/// </summary>
	internal class MidiMacroConfig : MidiMacroConfigData, IEquatable<MidiMacroConfig>
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public MidiMacroConfig()
		{
			Reset();
		}



		/********************************************************************/
		/// <summary>
		/// Reset MIDI macro config to default values
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			Algorithm.fill(Global.data(), Global.data() + Global.size(), new Macro());
			Algorithm.fill(SFx.data(), SFx.data() + SFx.size(), new Macro());
			Algorithm.fill(Zxx.data(), Zxx.data() + Zxx.size(), new Macro());

			Global[GlobalMacro.MidiOut_Start] = "FF";
			Global[GlobalMacro.MidiOut_Stop] = "FC";
			Global[GlobalMacro.MidiOut_NoteOn] = "9c n v";
			Global[GlobalMacro.MidiOut_NoteOff] = "9c n 0";
			Global[GlobalMacro.MidiOut_Program] = "Cc p";

			// SF0: Z00-Z7F controls cutoff
			CreateParameteredMacro(0, ParameteredMacro.SFxCutOff);

			// Z80-Z8F controls resonance
			CreateFixedMacro(FixedMacro.ZxxReso4Bit);
		}



		/********************************************************************/
		/// <summary>
		/// Fix old-format (not conforming to IT's MIDI macro definitions)
		/// MIDI config strings
		/// </summary>
		/********************************************************************/
		public void UpgradeMacros()
		{
			foreach (Macro macro in SFx)
				macro.UpgradeLegacyMacro();

			foreach (Macro macro in Zxx)
				macro.UpgradeLegacyMacro();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CreateParameteredMacro(uint32 macroIndex, ParameteredMacro macroType, c_int subType = 0)
		{
			if (macroIndex < Iterator.size(SFx))
				CreateParameteredMacro(out SFx[macroIndex], macroType, subType);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void CreateParameteredMacro(out Macro parameteredMacro, ParameteredMacro macroType, c_int subType)
		{
			switch (macroType)
			{
				case ParameteredMacro.SFxUnused:
				{
					parameteredMacro = string.Empty;
					break;
				}

				case ParameteredMacro.SFxCutOff:
				{
					parameteredMacro = "F0F000z";
					break;
				}

				case ParameteredMacro.SFxReso:
				{
					parameteredMacro = "F0F001z";
					break;
				}

				case ParameteredMacro.SFxFltMode:
				{
					parameteredMacro = "F0F002z";
					break;
				}

				case ParameteredMacro.SFxDryWet:
				{
					parameteredMacro = "F0F003z";
					break;
				}

				case ParameteredMacro.SFxCC:
				{
					parameteredMacro = string.Format("Bc{0:X2}z", subType & 0x7f);
					break;
				}

				case ParameteredMacro.SFxPlugParam:
				{
					parameteredMacro = string.Format("F0F{0:X3}z", Math.Min(subType, 0x17f) + 0x80);
					break;
				}

				case ParameteredMacro.SFxChannelAt:
				{
					parameteredMacro = "Dcz";
					break;
				}

				case ParameteredMacro.SFxPolyAt:
				{
					parameteredMacro = "Acnz";
					break;
				}

				case ParameteredMacro.SFxPitch:
				{
					parameteredMacro = "Ec00z";
					break;
				}

				case ParameteredMacro.SFxProgChange:
				{
					parameteredMacro = "Ccz";
					break;
				}

				case ParameteredMacro.SFxCustom:
				default:
				{
					parameteredMacro = string.Empty;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CreateFixedMacro(FixedMacro macroType)
		{
			CreateFixedMacro(Zxx, macroType);
		}



		/********************************************************************/
		/// <summary>
		/// Create Zxx (Z80 - ZFF) from preset
		/// </summary>
		/********************************************************************/
		protected void CreateFixedMacro(array<Macro> fixedMacros, FixedMacro macroType)
		{
			for (uint32 i = 0; i < MidiMacros.ZxxMacros; i++)
			{
				uint32 param = i;

				switch (macroType)
				{
					case FixedMacro.ZxxUnused:
					{
						fixedMacros[i] = string.Empty;
						break;
					}

					case FixedMacro.ZxxReso4Bit:
					{
						param = i * 8;

						if (i < 16)
							fixedMacros[i] = string.Format("F0F001{0:X2}", param);
						else
							fixedMacros[i] = string.Empty;

						break;
					}

					case FixedMacro.ZxxReso7Bit:
					{
						fixedMacros[i] = string.Format("F0F001{0:X2}", param);
						break;
					}

					case FixedMacro.ZxxCutOff:
					{
						fixedMacros[i] = string.Format("F0F000{0:X2}", param);
						break;
					}

					case FixedMacro.ZxxFltMode:
					{
						fixedMacros[i] = string.Format("F0F002{0:X2}", param);
						break;
					}

					case FixedMacro.ZxxResoFltMode:
					{
						param = (i & 0x0f) * 8;

						if (i < 16)
							fixedMacros[i] = string.Format("F0F001{0:X2}", param);
						else if (i < 32)
							fixedMacros[i] = string.Format("F0F002{0:X2}", param);
						else
							fixedMacros[i] = string.Empty;

						break;
					}

					case FixedMacro.ZxxChannelAt:
					{
						fixedMacros[i] = string.Format("Dc{0:X2}", param);
						break;
					}

					case FixedMacro.ZxxPolyAt:
					{
						fixedMacros[i] = string.Format("Acn{0:X2}", param);
						break;
					}

					case FixedMacro.ZxxPitch:
					{
						fixedMacros[i] = string.Format("Ec00{0:X2}", param);
						break;
					}

					case FixedMacro.ZxxProgChange:
					{
						fixedMacros[i] = string.Format("Cc{0:X2}", param);
						break;
					}

					case FixedMacro.ZxxCustom:
					default:
						break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool operator == (MidiMacroConfig me, MidiMacroConfig other)
		{
			if (ReferenceEquals(me, other))
				return true;

			if ((me is null) || (other is null))
				return false;

			if (!Algorithm.equal<forward_iterator<Macro>, forward_iterator<Macro>, Macro>(me.Global.begin(), me.Global.end(), other.Global.begin()))
				return false;

			if (!Algorithm.equal<forward_iterator<Macro>, forward_iterator<Macro>, Macro>(me.SFx.begin(), me.SFx.end(), other.SFx.begin()))
				return false;

			if (!Algorithm.equal<forward_iterator<Macro>, forward_iterator<Macro>, Macro>(me.Zxx.begin(), me.Zxx.end(), other.Zxx.begin()))
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator != (MidiMacroConfig me, MidiMacroConfig other)
		{
			return !(me == other);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public override bool Equals(object obj)
		{
			return (obj is MidiMacroConfig other) && (this == other);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(MidiMacroConfig other)
		{
			return this == other;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public override int GetHashCode()
		{
			HashCode hash = new HashCode();

			hash.Add(Global);
			hash.Add(SFx);
			hash.Add(Zxx);

			return hash.ToHashCode();
		}
	}
}
