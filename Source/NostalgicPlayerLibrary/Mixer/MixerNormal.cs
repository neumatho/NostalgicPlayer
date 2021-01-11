/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.PlayerLibrary.Mixer.Containers;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Mixer
{
	/// <summary>
	/// Normal mixer implementation
	/// </summary>
	internal class MixerNormal : MixerBase
	{
		private const int FRACBITS = 11;

		private const int CLICK_SHIFT = 6;
		private const int CLICK_BUFFER = 1 << CLICK_SHIFT;

		private long idxSize;			// The current size of the playing sample in fixed point
		private long idxLoopPos;		// The loop start position in fixed point
		private long idxLoopEnd;		// The loop end position in fixed point
		private long idxReleaseEnd;		// The release end position in fixed point

		[DllImport("kernel32.dll")]
		private static extern IntPtr LoadLibrary(string dllToLoad);

		[DllImport("kernel32.dll")]
		private static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

		[DllImport("kernel32.dll")]
		private static extern bool FreeLibrary(IntPtr hModule);

		private static IntPtr dllPtr = IntPtr.Zero;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		static MixerNormal()
		{
			AppDomain.CurrentDomain.ProcessExit += MixerNormal_Dtor;

			string oldPath = Environment.CurrentDirectory;

			try
			{
				Assembly ass = Assembly.GetEntryAssembly();
				string path = Path.GetDirectoryName(ass.Location);

				// Load the dll file
				Environment.CurrentDirectory = path;

				if (Environment.Is64BitProcess)
					dllPtr = LoadLibrary(@"NostalgicPlayerLibrary_Native-x64.dll");
				else
					dllPtr = LoadLibrary(@"NostalgicPlayerLibrary_Native-x86.dll");
			}
			finally
			{
				Environment.CurrentDirectory = oldPath;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Destructor
		/// </summary>
		/********************************************************************/
		private static void MixerNormal_Dtor(object sender, EventArgs e)
		{
			if (dllPtr != IntPtr.Zero)
			{
				FreeLibrary(dllPtr);
				dllPtr = IntPtr.Zero;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will initialize mixer stuff
		/// </summary>
		/********************************************************************/
		protected override void InitMixer()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will cleanup mixer stuff
		/// </summary>
		/********************************************************************/
		protected override void CleanupMixer()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Returns the click constant value
		/// </summary>
		/********************************************************************/
		public override int GetClickConstant()
		{
			return CLICK_BUFFER;
		}



		/********************************************************************/
		/// <summary>
		/// This is the main mixer method
		/// </summary>
		/********************************************************************/
		public override void Mixing(int[] dest, int offset, int todo, MixerMode mode)
		{
			// Loop through all the channels and mix the samples into the buffer
			for (int t = 0; t < channelNumber; t++)
			{
				ref VoiceInfo vnf = ref voiceInfo[t];

				if (vnf.Kick)
				{
					vnf.Current = ((long)vnf.Start) << FRACBITS;
					vnf.Kick = false;
					vnf.Active = true;
				}

				if (vnf.Frequency == 0)
					vnf.Active = false;

				if (vnf.Active)
				{
					vnf.Increment = ((long)vnf.Frequency << FRACBITS) / mixerFrequency;

					if ((vnf.Flags & SampleFlags.Reverse) != 0)
						vnf.Increment = -vnf.Increment;

					int lVol, rVol, pan;

					if (vnf.Enabled)
					{
						lVol = vnf.LeftVolume * masterVolume / 256;
						rVol = vnf.RightVolume * masterVolume / 256;
					}
					else
					{
						lVol = 0;
						rVol = 0;
					}

					vnf.OldLeftVolume = vnf.LeftVolumeSelected;
					vnf.OldRightVolume = vnf.RightVolumeSelected;

					if ((mode & MixerMode.Stereo) != 0)
					{
						if ((vnf.Flags & SampleFlags.Speaker) != 0)
						{
							vnf.LeftVolumeSelected = lVol;
							vnf.RightVolumeSelected = rVol;
						}
						else
						{
							if (vnf.Panning != (int)Panning.Surround)
							{
								// Stereo, calculate the volume with panning
								pan = (((vnf.Panning - 128) * stereoSeparation) / 128) + 128;

								vnf.LeftVolumeSelected = (lVol * ((int)Panning.Right - pan)) >> 8;
								vnf.RightVolumeSelected = (lVol * pan) >> 8;
							}
							else
							{
								// Dolby Surround
								vnf.LeftVolumeSelected = vnf.RightVolumeSelected = lVol / 2;
							}
						}
					}
					else
					{
						// Well, just mono
						vnf.LeftVolumeSelected = lVol;
					}

					idxSize = vnf.Size != 0 ? ((long)vnf.Size << FRACBITS) - 1 : 0;
					idxLoopEnd = vnf.RepeatEnd != 0 ? ((long)vnf.RepeatEnd << FRACBITS) - 1 : 0;
					idxLoopPos = (long)vnf.RepeatPosition << FRACBITS;
					idxReleaseEnd = vnf.ReleaseLength != 0 ? ((long)vnf.ReleaseLength << FRACBITS) - 1 : 0;

					AddChannel(ref vnf, dest, offset, todo, mode);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Mix a channel into the buffer
		/// </summary>
		/********************************************************************/
		private void AddChannel(ref VoiceInfo vnf, int[] buf, int offset, int todo, MixerMode mode)
		{
			sbyte[] s;

			if ((s = vnf.Address) == null)
			{
				vnf.Current = 0;
				vnf.Active = false;
				return;
			}

			// Update the 'current' index so the sample loops, or
			// stops playing if it reached the end of the sample
			while (todo > 0)
			{
				if ((vnf.Flags & SampleFlags.Reverse) != 0)
				{
					// The sampling is playing in reverse
					if (((vnf.Flags & SampleFlags.Loop) != 0) && (vnf.Current < idxLoopPos))
					{
						// The sample is looping, and has reached the loop start index
						if ((vnf.Flags & SampleFlags.Bidi) != 0)
						{
							// Sample is doing bidirectional loops, so 'bounce'
							// the current index against the idxLoopPos
							vnf.Current = idxLoopPos + (idxLoopPos - vnf.Current);
							vnf.Increment = -vnf.Increment;
							vnf.Flags &= ~SampleFlags.Reverse;
						}
						else
						{
							// Normal backwards looping, so set the
							// current position to loopEnd index
							vnf.Current = idxLoopEnd - (idxLoopPos - vnf.Current);
						}
					}
					else
					{
						// The sample is not looping, so check if it reached index 0
						if (vnf.Current < 0)
						{
							// Playing index reached 0, so stop playing this sample
							vnf.Current = 0;
							vnf.Active = false;
							break;
						}
					}
				}
				else
				{
					// The sample is playing forward
					if ((vnf.Flags & SampleFlags.Loop) != 0)
					{
						if (vnf.Current >= idxLoopEnd)
						{
							// Do we have a loop address?
							if (vnf.LoopAddress == null)
							{
								vnf.Current = 0;
								vnf.Active = false;
								break;
							}

							// Copy the loop address
							s = vnf.Address = vnf.LoopAddress;

							// Should we release the sample?
							if (vnf.ReleaseLength != 0)
							{
								// Yes, so set the current position
								vnf.Current = vnf.Current - idxLoopEnd;
								vnf.Flags |= SampleFlags.Release;
								vnf.Flags &= ~SampleFlags.Loop;
							}
							else
							{
								// The sample is looping, so check if it reached the loopEnd index
								if ((vnf.Flags & SampleFlags.Bidi) != 0)
								{
									// Sample is doing bidirectional loops, so 'bounce'
									// the current index against the idxLoopEnd
									vnf.Current = idxLoopEnd - (vnf.Current - idxLoopEnd);
									vnf.Increment = -vnf.Increment;
									vnf.Flags |= SampleFlags.Reverse;
								}
								else
								{
									// Normal looping, so set the
									// current position to loopEnd index
									vnf.Current = idxLoopPos + (vnf.Current - idxLoopEnd);
								}
							}
						}
					}
					else
					{
						// Sample is not looping, so check if it reached the last position
						if ((vnf.Flags & SampleFlags.Release) != 0)
						{
							// We play the release part
							if (vnf.Current >= idxReleaseEnd)
							{
								// Stop playing this sample
								vnf.Current = 0;
								vnf.Active = false;
								break;
							}
						}
						else
						{
							if (vnf.Current >= idxSize)
							{
								// Stop playing this sample
								vnf.Current = 0;
								vnf.Active = false;
								break;
							}
						}
					}
				}

				long end = (vnf.Flags & SampleFlags.Reverse) != 0 ?
							(vnf.Flags & SampleFlags.Loop) != 0 ? idxLoopPos : 0 :
							(vnf.Flags & SampleFlags.Loop) != 0 ? idxLoopEnd :
							(vnf.Flags & SampleFlags.Release) != 0 ? idxReleaseEnd : idxSize;

				// If the sample is not blocked
				int done;

				if ((end == vnf.Current) || (vnf.Increment == 0))
					done = 0;
				else
				{
					done = Math.Min((int)((end - vnf.Current) / vnf.Increment + 1), todo);
					if (done < 0)
						done = 0;
				}

				if (done == 0)
				{
					vnf.Active = false;
					break;
				}

				long endPos = vnf.Current + done * vnf.Increment;

				if ((vnf.LeftVolume != 0) || (vnf.RightVolume != 0))
				{
					GCHandle pinnedBuf = GCHandle.Alloc(buf, GCHandleType.Pinned);

					try
					{
						IntPtr bufAddr = pinnedBuf.AddrOfPinnedObject();

						// Use 32 bit mixers as often as we can (they're much faster)
						if ((vnf.Current < 0x7fffffff) && (endPos < 0x7fffffff))
						{
							// Use 32 bit mixers
							//
							// Check to see if we need to make interpolation on the mixing
							if (false)
							{
							}
							else
							{
								// No interpolation
								if ((vnf.Flags & SampleFlags._16Bits) != 0)
								{
									// 16 bit input sample to be mixed
									if ((mode & MixerMode.Stereo) != 0)
									{
										if ((vnf.Panning == (int)Panning.Surround) && ((mode & MixerMode.Surround) != 0))
											vnf.Current = Mix16SurroundNormal(s, bufAddr, offset, (int)vnf.Current, (int)vnf.Increment, done, vnf.LeftVolumeSelected, vnf.RightVolumeSelected);
										else
											vnf.Current = Mix16StereoNormal(s, bufAddr, offset, (int)vnf.Current, (int)vnf.Increment, done, vnf.LeftVolumeSelected, vnf.RightVolumeSelected);
									}
									else
										vnf.Current = Mix16MonoNormal(s, bufAddr, offset, (int)vnf.Current, (int)vnf.Increment, done, vnf.LeftVolumeSelected);
								}
								else
								{
									// 8 bit input sample to be mixed
									if ((mode & MixerMode.Stereo) != 0)
									{
										if ((vnf.Panning == (int)Panning.Surround) && ((mode & MixerMode.Surround) != 0))
											vnf.Current = Mix8SurroundNormal(s, bufAddr, offset, (int)vnf.Current, (int)vnf.Increment, done, vnf.LeftVolumeSelected, vnf.RightVolumeSelected);
										else
											vnf.Current = Mix8StereoNormal(s, bufAddr, offset, (int)vnf.Current, (int)vnf.Increment, done, vnf.LeftVolumeSelected, vnf.RightVolumeSelected);
									}
									else
										vnf.Current = Mix8MonoNormal(s, bufAddr, offset, (int)vnf.Current, (int)vnf.Increment, done, vnf.LeftVolumeSelected);
								}
							}
						}
						else
						{
							// Use 64 bit mixers
							//
							// Check to see if we need to make interpolation on the mixing
							if (false)
							{
							}
							else
							{
								// No interpolation
								if ((vnf.Flags & SampleFlags._16Bits) != 0)
								{
									// 16 bit input sample to be mixed
									if ((mode & MixerMode.Stereo) != 0)
									{
										if ((vnf.Panning == (int)Panning.Surround) && ((mode & MixerMode.Surround) != 0))
											vnf.Current = Mix16SurroundNormal64(s, bufAddr, offset, (int)vnf.Current, (int)vnf.Increment, done, vnf.LeftVolumeSelected, vnf.RightVolumeSelected);
										else
											vnf.Current = Mix16StereoNormal64(s, bufAddr, offset, (int)vnf.Current, (int)vnf.Increment, done, vnf.LeftVolumeSelected, vnf.RightVolumeSelected);
									}
									else
										vnf.Current = Mix16MonoNormal64(s, bufAddr, offset, (int)vnf.Current, (int)vnf.Increment, done, vnf.LeftVolumeSelected);
								}
								else
								{
									// 8 bit input sample to be mixed
									if ((mode & MixerMode.Stereo) != 0)
									{
										if ((vnf.Panning == (int)Panning.Surround) && ((mode & MixerMode.Surround) != 0))
											vnf.Current = Mix8SurroundNormal64(s, bufAddr, offset, (int)vnf.Current, (int)vnf.Increment, done, vnf.LeftVolumeSelected, vnf.RightVolumeSelected);
										else
											vnf.Current = Mix8StereoNormal64(s, bufAddr, offset, (int)vnf.Current, (int)vnf.Increment, done, vnf.LeftVolumeSelected, vnf.RightVolumeSelected);
									}
									else
										vnf.Current = Mix8MonoNormal64(s, bufAddr, offset, (int)vnf.Current, (int)vnf.Increment, done, vnf.LeftVolumeSelected);
								}
							}
						}
					}
					finally
					{
						pinnedBuf.Free();
					}
				}
				else
				{
					// Update the sample position
					vnf.Current = endPos;
				}

				todo -= done;
				offset += (mode & MixerMode.Stereo) != 0 ? done << 1 : done;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Mixes a 16 bit sample into a mono output buffer
		/// </summary>
		/********************************************************************/
		private int Mix16MonoNormal(sbyte[] source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel)
		{
			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Mixes a 16 bit sample into a stereo output buffer
		/// </summary>
		/********************************************************************/
		private int Mix16StereoNormal(sbyte[] source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int rVolSel)
		{
			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Mixes a 16 bit surround sample into a stereo output buffer
		/// </summary>
		/********************************************************************/
		private int Mix16SurroundNormal(sbyte[] source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int rVolSel)
		{
			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Mixes a 8 bit sample into a mono output buffer
		/// </summary>
		/********************************************************************/
		private delegate int del_Mix8MonoNormal(IntPtr source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel);
		private int Mix8MonoNormal(sbyte[] source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel)
		{
			if (_Mix8MonoNormal == null)
			{
				IntPtr f = Environment.Is64BitProcess ? GetProcAddress(dllPtr, "Mix8MonoNormal") : GetProcAddress(dllPtr, "_Mix8MonoNormal@28");
				_Mix8MonoNormal = (del_Mix8MonoNormal)Marshal.GetDelegateForFunctionPointer(f, typeof(del_Mix8MonoNormal));
			}

			GCHandle pinnedBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

			try
			{
				return _Mix8MonoNormal(pinnedBuf.AddrOfPinnedObject(), dest, offset, index, increment, todo, lVolSel);
			}
			finally
			{
				pinnedBuf.Free();
			}
		}
		private static del_Mix8MonoNormal _Mix8MonoNormal = null;



		/********************************************************************/
		/// <summary>
		/// Mixes a 8 bit sample into a stereo output buffer
		/// </summary>
		/********************************************************************/
		private delegate int del_Mix8StereoNormal(IntPtr source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int rVolSel);
		private int Mix8StereoNormal(sbyte[] source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int rVolSel)
		{
			if (_Mix8StereoNormal == null)
			{
				IntPtr f = Environment.Is64BitProcess ? GetProcAddress(dllPtr, "Mix8StereoNormal") : GetProcAddress(dllPtr, "_Mix8StereoNormal@32");
				_Mix8StereoNormal = (del_Mix8StereoNormal)Marshal.GetDelegateForFunctionPointer(f, typeof(del_Mix8StereoNormal));
			}

			GCHandle pinnedBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

			try
			{
				return _Mix8StereoNormal(pinnedBuf.AddrOfPinnedObject(), dest, offset, index, increment, todo, lVolSel, rVolSel);
			}
			finally
			{
				pinnedBuf.Free();
			}
		}
		private static del_Mix8StereoNormal _Mix8StereoNormal = null;



		/********************************************************************/
		/// <summary>
		/// Mixes a 8 bit surround sample into a stereo output buffer
		/// </summary>
		/********************************************************************/
		private int Mix8SurroundNormal(sbyte[] source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int rVolSel)
		{
			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Mixes a 16 bit sample into a mono output buffer
		/// </summary>
		/********************************************************************/
		private int Mix16MonoNormal64(sbyte[] source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel)
		{
			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Mixes a 16 bit sample into a stereo output buffer
		/// </summary>
		/********************************************************************/
		private int Mix16StereoNormal64(sbyte[] source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int rVolSel)
		{
			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Mixes a 16 bit surround sample into a stereo output buffer
		/// </summary>
		/********************************************************************/
		private int Mix16SurroundNormal64(sbyte[] source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int rVolSel)
		{
			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Mixes a 8 bit sample into a mono output buffer
		/// </summary>
		/********************************************************************/
		private int Mix8MonoNormal64(sbyte[] source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel)
		{
			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Mixes a 8 bit sample into a stereo output buffer
		/// </summary>
		/********************************************************************/
		private int Mix8StereoNormal64(sbyte[] source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int rVolSel)
		{
			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Mixes a 8 bit surround sample into a stereo output buffer
		/// </summary>
		/********************************************************************/
		private int Mix8SurroundNormal64(sbyte[] source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int rVolSel)
		{
			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Converts the mixed data to a 16 bit sample buffer
		/// </summary>
		/********************************************************************/
		private delegate void del_ConvertTo16(IntPtr dest, int offset, IntPtr source, int count);
		protected override void ConvertTo16(byte[] dest, int offset, int[] source, int count)
		{
			if (_ConvertTo16 == null)
			{
				IntPtr f = Environment.Is64BitProcess ? GetProcAddress(dllPtr, "ConvertTo16") : GetProcAddress(dllPtr, "_ConvertTo16@16");
				_ConvertTo16 = (del_ConvertTo16)Marshal.GetDelegateForFunctionPointer(f, typeof(del_ConvertTo16));
			}

			GCHandle destBuf = GCHandle.Alloc(dest, GCHandleType.Pinned);

			try
			{
				GCHandle sourceBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

				try
				{
					_ConvertTo16(destBuf.AddrOfPinnedObject(), offset, sourceBuf.AddrOfPinnedObject(), count);
				}
				finally
				{
					sourceBuf.Free();
				}
			}
			finally
			{
				destBuf.Free();
			}
		}
		private static del_ConvertTo16 _ConvertTo16 = null;



		/********************************************************************/
		/// <summary>
		/// Converts the mixed data to a 32 bit sample buffer
		/// </summary>
		/********************************************************************/
		private delegate void del_ConvertTo32(IntPtr dest, int offset, IntPtr source, int count);
		protected override void ConvertTo32(byte[] dest, int offset, int[] source, int count)
		{
			if (_ConvertTo32 == null)
			{
				IntPtr f = Environment.Is64BitProcess ? GetProcAddress(dllPtr, "ConvertTo32") : GetProcAddress(dllPtr, "_ConvertTo32@16");
				_ConvertTo32 = (del_ConvertTo32)Marshal.GetDelegateForFunctionPointer(f, typeof(del_ConvertTo32));
			}

			GCHandle destBuf = GCHandle.Alloc(dest, GCHandleType.Pinned);

			try
			{
				GCHandle sourceBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

				try
				{
					_ConvertTo32(destBuf.AddrOfPinnedObject(), offset, sourceBuf.AddrOfPinnedObject(), count);
				}
				finally
				{
					sourceBuf.Free();
				}
			}
			finally
			{
				destBuf.Free();
			}
		}
		private static del_ConvertTo32 _ConvertTo32 = null;
	}
}
