/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibMpg123;
using Polycode.NostalgicPlayer.Ports.LibMpg123.Containers;

namespace Polycode.NostalgicPlayer.Agent.Streamer.Mpeg
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class MpegWorker : StreamerWithDurationAgentBase
	{
		private int oldBitRate;

		private Mpg123_FrameInfo frameInfo;

		private LibMpg123 mpg123Handle;

		private int[] decodeBuffer;

		private const int InfoBitRateLine = 1;

		#region Identify
		/********************************************************************/
		/// <summary>
		/// Return an array of mime types that this agent can handle
		/// </summary>
		/********************************************************************/
		public override string[] PlayableMimeTypes => [ "audio/mpeg", "audio/mpeg3" ];
		#endregion

		#region Initialization and cleanup
		/********************************************************************/
		/// <summary>
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public override bool InitPlayer(StreamingStream streamingStream, IMetadata metadata, out string errorMessage)
		{
			if (!base.InitPlayer(streamingStream, metadata, out errorMessage))
				return false;

			// Get a Mpg123 handle, which is used on all other calls
			mpg123Handle = LibMpg123.Mpg123_New(null, out Mpg123_Errors error);
			if (error != Mpg123_Errors.Ok)
			{
				errorMessage = GetErrorString(error);
				return false;
			}

			// Make sure, that the output is always in 32-bit for every sample rate
			Mpg123_Errors result = mpg123Handle.Mpg123_Format_None();
			if (result != Mpg123_Errors.Ok)
			{
				errorMessage = GetErrorString(result);
				return false;
			}

			mpg123Handle.Mpg123_Rates(out int[] supportedRates, out ulong number);
			if (number > 0)
			{
				// Set the output format to 32-bit on every rate
				foreach (int rate in supportedRates)
				{
					result = mpg123Handle.Mpg123_Format(rate, Mpg123_ChannelCount.Mono | Mpg123_ChannelCount.Stereo, Mpg123_Enc_Enum.Enc_Signed_32);
					if (result != Mpg123_Errors.Ok)
					{
						errorMessage = GetErrorString(result);
						return false;
					}
				}
			}

			result = OpenFile(streamingStream);
			if (result != Mpg123_Errors.Ok)
			{
				errorMessage = GetErrorString(result);
				return false;
			}

			result = mpg123Handle.Mpg123_Info(out frameInfo);
			if (result != Mpg123_Errors.Ok)
			{
				errorMessage = GetErrorString(result);
				return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the player
		/// </summary>
		/********************************************************************/
		public override void CleanupPlayer()
		{
			decodeBuffer = null;
			frameInfo = null;

			if (mpg123Handle != null)
			{
				mpg123Handle.Mpg123_Close();

				mpg123Handle.Mpg123_Delete();
				mpg123Handle = null;
			}

			base.CleanupPlayer();
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the player to start the sample from start
		/// </summary>
		/********************************************************************/
		public override bool InitSound(out string errorMessage)
		{
			if (!base.InitSound(out errorMessage))
				return false;

			// Initialize some variables
			oldBitRate = 0;

			return true;
		}
		#endregion

		#region Playing
		/********************************************************************/
		/// <summary>
		/// Will load and decode a data block and store it in the buffer
		/// given
		/// </summary>
		/********************************************************************/
		public override int LoadDataBlock(int[][] outputBuffer, int countInFrames)
		{
			// Allocate/reallocate decode buffer if needed
			int channels = ChannelCount;

			if ((decodeBuffer == null) || ((countInFrames * channels) > decodeBuffer.Length))
				decodeBuffer = new int[countInFrames * channels];

			// Load the next block of data
			int filledInSamples = LoadData(decodeBuffer, countInFrames * channels);
			int filledInFrames = filledInSamples / channels;

			if (filledInSamples > 0)
				SoundHelper.SplitBuffer(channels, decodeBuffer, outputBuffer, filledInFrames);
			else
			{
				OnEndReached();

				// Loop the sample
				CleanupSound();
				InitSound(out _);
			}

			// Has the bit rate changed (mostly on VBR streams)
			if (mpg123Handle.Mpg123_Info(out Mpg123_FrameInfo newFrameInfo) == Mpg123_Errors.Ok)
			{
				if (newFrameInfo.BitRate != oldBitRate)
				{
					oldBitRate = newFrameInfo.BitRate;

					OnModuleInfoChanged(InfoBitRateLine, newFrameInfo.BitRate.ToString());
				}
			}

			return filledInFrames;
		}
		#endregion

		#region Information
		/********************************************************************/
		/// <summary>
		/// Return the number of channels the sample uses
		/// </summary>
		/********************************************************************/
		public override int ChannelCount => frameInfo.Mode == Mpg123_Mode.Mono ? 1 : 2;



		/********************************************************************/
		/// <summary>
		/// Return the frequency the sample is stored with
		/// </summary>
		/********************************************************************/
		public override int Frequency => frameInfo.Rate;



		/********************************************************************/
		/// <summary>
		/// Returns the description and value on the line given. If the line
		/// is out of range, false is returned
		/// </summary>
		/********************************************************************/
		public override bool GetInformationString(int line, out string description, out string value)
		{
			// Find out which line to take
			switch (line)
			{
				// Layer
				case 0:
				{
					description = Resources.IDS_MPG_INFODESCLINE0;
					value = frameInfo.Layer.ToString();
					break;
				}

				// Bit rate
				case 1:
				{
					description = Resources.IDS_MPG_INFODESCLINE1;
					value = frameInfo.BitRate.ToString();
					break;
				}

				// Frequency
				case 2:
				{
					description = Resources.IDS_MPG_INFODESCLINE2;
					value = frameInfo.Rate.ToString();
					break;
				}

				// Channel mode
				case 3:
				{
					description = Resources.IDS_MPG_INFODESCLINE3;

					switch (frameInfo.Mode)
					{
						default:
						case Mpg123_Mode.Mono:
						{
							value = Resources.IDS_MPG_INFO_CHANNEL_MONO;
							break;
						}

						case Mpg123_Mode.Stereo:
						{
							value = Resources.IDS_MPG_INFO_CHANNEL_STEREO;
							break;
						}

						case Mpg123_Mode.Joint:
						{
							value = Resources.IDS_MPG_INFO_CHANNEL_JOINT;
							break;
						}

						case Mpg123_Mode.Dual:
						{
							value = Resources.IDS_MPG_INFO_CHANNEL_DUAL;
							break;
						}
					}
					break;
				}

				// Private
				case 4:
				{
					description = Resources.IDS_MPG_INFODESCLINE4;
					value = (frameInfo.Flags & Mpg123_Flags.Private) != 0 ? Resources.IDS_MPG_INFO_YES : Resources.IDS_MPG_INFO_NO;
					break;
				}

				// CRCs
				case 5:
				{
					description = Resources.IDS_MPG_INFODESCLINE5;
					value = (frameInfo.Flags & Mpg123_Flags.Crc) != 0 ? Resources.IDS_MPG_INFO_YES : Resources.IDS_MPG_INFO_NO;
					break;
				}

				// Copyrighted
				case 6:
				{
					description = Resources.IDS_MPG_INFODESCLINE6;
					value = (frameInfo.Flags & Mpg123_Flags.Copyright) != 0 ? Resources.IDS_MPG_INFO_YES : Resources.IDS_MPG_INFO_NO;
					break;
				}

				// Original
				case 7:
				{
					description = Resources.IDS_MPG_INFODESCLINE7;
					value = (frameInfo.Flags & Mpg123_Flags.Original) != 0 ? Resources.IDS_MPG_INFO_YES : Resources.IDS_MPG_INFO_NO;
					break;
				}

				// Emphasis
				case 8:
				{
					description = Resources.IDS_MPG_INFODESCLINE8;

					switch (frameInfo.Emphasis)
					{
						default:
						case 0:
						{
							value = Resources.IDS_MPG_INFO_EMPHASIS0;
							break;
						}

						case 1:
						{
							value = Resources.IDS_MPG_INFO_EMPHASIS1;
							break;
						}

						case 3:
						{
							value = Resources.IDS_MPG_INFO_EMPHASIS3;
							break;
						}
					}
					break;
				}

				default:
				{
					description = null;
					value = null;

					return false;
				}
			}

			return true;
		}
		#endregion

		#region Duration calculation
		/********************************************************************/
		/// <summary>
		/// Set the position in the playing sample to the time given
		/// </summary>
		/********************************************************************/
		protected override void SetPosition(TimeSpan time)
		{
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Helper method to get the string of an error
		/// </summary>
		/********************************************************************/
		private string GetErrorString(Mpg123_Errors error)
		{
			if (error == Mpg123_Errors.Err)
				return mpg123Handle.Mpg123_StrError();

			return mpg123Handle.Mpg123_Plain_StrError(error);
		}



		/********************************************************************/
		/// <summary>
		/// Open the file
		/// </summary>
		/********************************************************************/
		private Mpg123_Errors OpenFile(StreamingStream streamingStream)
		{
			Mpg123_Errors result = mpg123Handle.Mpg123_Reader64(Read, null, null);
			if (result != Mpg123_Errors.Ok)
				return result;

			return mpg123Handle.Mpg123_Open_Handle(streamingStream);
		}



		/********************************************************************/
		/// <summary>
		/// Read from the file
		/// </summary>
		/********************************************************************/
		private int Read(object handle, Memory<byte> buf, ulong count, out ulong readCount)
		{
			StreamingStream stream = (StreamingStream)handle;

			readCount = (ulong)stream.Read(buf.Span.Slice(0, (int)count));

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Read next block of data
		/// </summary>
		/********************************************************************/
		private int LoadData(int[] outputBuffer, int count)
		{
			Span<byte> outBuf = MemoryMarshal.Cast<int, byte>(outputBuffer);
			int total = 0;
			int todo = count;

			while (todo > 0)
			{
				Mpg123_Errors result = mpg123Handle.Mpg123_Read(outBuf, (ulong)todo * 4, out ulong done);
				if ((result == Mpg123_Errors.Ok) || (result == Mpg123_Errors.Done))
				{
					if (done == 0)
					{
						// Done with the stream
						break;
					}

					outBuf = outBuf.Slice((int)done);
					done /= 4;
					todo -= (int)done;
					total += (int)done;
				}
				else
				{
					if (result == Mpg123_Errors.New_Format)
					{
						// Just ignore this one, since we don't need the new info
					}
					else
					{
						// Some error occurred, so stop
						break;
					}
				}
			}

			return total;
		}
		#endregion
	}
}
