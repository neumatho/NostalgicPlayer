/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// Immersive Audio Model and Formats helper functions and defines
	/// </summary>
	public static class Iamf
	{
		/********************************************************************/
		/// <summary>
		/// Free an AVIAMFAudioElement and all its contents
		/// </summary>
		/********************************************************************/
		public static void Av_Iamf_Audio_Element_Free(ref AvIamfAudioElement pAudio_Element)//XX 336
		{
			AvIamfAudioElement audio_Element = pAudio_Element;

			if (audio_Element == null)
				return;

			for (c_int i = 0; i < audio_Element.Nb_Layers; i++)
			{
				AvIamfLayer layer = audio_Element.Layers[i];
				Opt.Av_Opt_Free(layer);
				Mem.Av_Free(layer.Demixing_Matrix);
				Mem.Av_Free(layer);
			}

			Mem.Av_Free(audio_Element.Layers);

			Mem.Av_Free(audio_Element.Demixing_Info);
			Mem.Av_Free(audio_Element.Recon_Gain_Info);
			Mem.Av_FreeP(ref pAudio_Element);
		}



		/********************************************************************/
		/// <summary>
		/// Free an AVIAMFMixPresentation and all its contents
		/// </summary>
		/********************************************************************/
		public static void Av_Iamf_Mix_Presentation_Free(ref AvIamfMixPresentation pMix_Presentation)//XX 534
		{
			AvIamfMixPresentation mix_Presentation = pMix_Presentation;

			if (mix_Presentation == null)
				return;

			for (c_int i = 0; i < mix_Presentation.Nb_Submixes; i++)
			{
				AvIamfSubmix sub_Mix = mix_Presentation.Submixes[i];

				for (c_int j = 0; j < sub_Mix.Nb_Elements; j++)
				{
					AvIamfSubmixElement submix_Element = sub_Mix.Elements[j];
					Opt.Av_Opt_Free(submix_Element);
					Mem.Av_Free(submix_Element.Element_Mix_Config);
					Mem.Av_Free(submix_Element);
				}

				Mem.Av_Free(sub_Mix.Elements);

				for (c_int j = 0; j < sub_Mix.Nb_Layouts; j++)
				{
					AvIamfSubmixLayout submix_Layout = sub_Mix.Layouts[j];
					Opt.Av_Opt_Free(submix_Layout);
					Mem.Av_Free(submix_Layout);
				}

				Mem.Av_Free(sub_Mix.Layouts);
				Mem.Av_Free(sub_Mix.Output_Mix_Config);
				Mem.Av_Free(sub_Mix);
			}

			Opt.Av_Opt_Free(mix_Presentation);
			Mem.Av_Free(mix_Presentation.Submixes);

			Mem.Av_FreeP(ref pMix_Presentation);
		}
	}
}
