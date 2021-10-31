﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.MikModConverter {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Polycode.NostalgicPlayer.Agent.ModuleConverter.MikModConverter.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Converts all supported MikMod formats to NostalgicPlayer UniMod format.
        ///This format is almost like the original MikMod UniMod format except for a few changes.
        ///Written by Thomas Neumann based on MikMod 3.3.11.1 + what has been added to 3.3.12 so far.
        ///
        ///This version can convert these formats:
        ///
        ///Asylum (AMF)
        ///Composer 669 and Unis 669 (669)
        ///Digital Sound and Music Interface (AMF)
        ///Digital Sound Interface Kit (DSM)
        ///Farandole Composer (FAR)
        ///FastTracker II (XM)
        ///General DigiMusic (GDM)
        ///Imago Orpheus (IMF) [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string IDS_MIKCONV_DESCRIPTION {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_DESCRIPTION", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This loader recognizes “Composer 669” modules. The 669 format were among the first PC module formats. They do not have a wide range of effects and support 8 channels.
        ///
        ///“Composer 669” was written by Tran of Renaissance, a.k.a. Tomasz Pytel and released in 1992..
        /// </summary>
        internal static string IDS_MIKCONV_DESCRIPTION_AGENT1 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_DESCRIPTION_AGENT1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This loader recognizes “Scream Tracker” modules. “Scream Tracker” was the first PC tracker, as well as the first PC module format. Loosely inspired by the “SoundTracker” format, it does not have as many effects as Protracker, although it supports 31 instruments and 4 channels.
        ///
        ///“Scream Tracker” was written by PSI of Future Crew, a.k.a. Sami Tammilehto..
        /// </summary>
        internal static string IDS_MIKCONV_DESCRIPTION_AGENT10 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_DESCRIPTION_AGENT10", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This loader recognizes “Scream Tracker 3” modules. This version was a huge improvement over the original “Scream Tracker”. It supported 32 channels, up to 99 instruments, and a large choice of effects.
        ///
        ///“Scream Tracker 3” was written by PSI of Future Crew, a.k.a. Sami Tammilehto, and released in 1994..
        /// </summary>
        internal static string IDS_MIKCONV_DESCRIPTION_AGENT11 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_DESCRIPTION_AGENT11", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This loader recognizes “STMIK 0.2” modules. “STMIK” (the Scream Tracker Music Interface Kit) was a module playing library distributed by Future Crew to play Scream Tracker module in games and demos. It uses an intermediate format between STM and S3M and comes with a tool converting STM modules to STX.
        ///
        ///“STMIK” was written by PSI of Future Crew, a.k.a. Sami Tammilehto..
        /// </summary>
        internal static string IDS_MIKCONV_DESCRIPTION_AGENT12 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_DESCRIPTION_AGENT12", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This loader recognizes “UltraTracker” modules. They are mostly similar to Protracker modules, but support two effects per channel.
        ///
        ///“UltraTracker” was written by MAS of Prophecy, a.k.a. Marc Andre Schallehn, and released in 1993..
        /// </summary>
        internal static string IDS_MIKCONV_DESCRIPTION_AGENT13 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_DESCRIPTION_AGENT13", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This loader recognizes “UniMod” modules. This is the internal format used by MikMod. If you find any modules in this format, it probably won&apos;t be played correct, because not all effects were supported at the time those modules were used..
        /// </summary>
        internal static string IDS_MIKCONV_DESCRIPTION_AGENT14 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_DESCRIPTION_AGENT14", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This loader recognizes the modules in “umx” files from games like “Unreal”, “DeusEx”, etc. To NostalgicPlayer, UMX is just a container and the real music format may be one of “ScreamTracker 3”, “Impulse Tracker”, “FastTracker 2”, or possibly a “Protracker” compatible one..
        /// </summary>
        internal static string IDS_MIKCONV_DESCRIPTION_AGENT15 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_DESCRIPTION_AGENT15", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This loader recognizes “FastTracker 2” modules. This format was designed from scratch, instead of creating yet another ProTracker variation. It was the first format using instruments as well as samples, and envelopes for finer effects.
        ///FastTracker 2 was written by Fredrik Huss and Magnus Hogdahl, and released in 1994..
        /// </summary>
        internal static string IDS_MIKCONV_DESCRIPTION_AGENT16 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_DESCRIPTION_AGENT16", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This loader is the same as the FastTracker 2 (XM) loader, but can depack the samples used in the module. The samples are 4-bit ADPCM packed..
        /// </summary>
        internal static string IDS_MIKCONV_DESCRIPTION_AGENT17 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_DESCRIPTION_AGENT17", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This loader recognizes “Unis 669” modules. This format is the successor of the &quot;Composer 669&quot; and introduces some new effects like the super fast tempo and stereo balance. Support 8 channels.
        ///
        ///“Unis 669 Composer” was written by Jason Nunn and released in 1994..
        /// </summary>
        internal static string IDS_MIKCONV_DESCRIPTION_AGENT2 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_DESCRIPTION_AGENT2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This loader recognizes the “Advanced Module Format”, which is the internal module format of the “Digital Sound and Music Interface” (DSMI) library.
        ///
        ///This format has the same limitations as the S3M format. The most famous DSMI application was DMP, the Dual Module Player.
        ///
        ///DMP and the DSMI library were written by Otto Chrons. DSMI was first released in 1993..
        /// </summary>
        internal static string IDS_MIKCONV_DESCRIPTION_AGENT3 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_DESCRIPTION_AGENT3", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This loader recognize the “ASYLUM Music Format”, which was used in Crusader series of games by Origin. This format uses the .amf extension, but is very similar to a 8 Channel Mod file..
        /// </summary>
        internal static string IDS_MIKCONV_DESCRIPTION_AGENT4 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_DESCRIPTION_AGENT4", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This loader recognizes the DSIK format, which is the internal module format of the “Digital Sound Interface Kit” (DSIK) library, the ancester of the SEAL library. This format has the same limitations as the S3M format.
        ///
        ///The DSIK library was written by Carlos Hasan and released in 1994..
        /// </summary>
        internal static string IDS_MIKCONV_DESCRIPTION_AGENT5 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_DESCRIPTION_AGENT5", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This loader recognizes “Farandole” modules. These modules can be up to 16 channels and have Protracker comparable effects.
        ///
        ///The Farandole composer was written by Daniel Potter and released in 1994..
        /// </summary>
        internal static string IDS_MIKCONV_DESCRIPTION_AGENT6 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_DESCRIPTION_AGENT6", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This loader recognizes the “General DigiMusic” format, which is the internal format of the “Bells, Whistles and Sound Boards” library. This format has the same limitations as the S3M format.
        ///
        ///The BWSB library was written by Edward Schlunder and first released in 1993..
        /// </summary>
        internal static string IDS_MIKCONV_DESCRIPTION_AGENT7 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_DESCRIPTION_AGENT7", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This loader recognizes “Imago Orpheus” modules. This format is roughly equivalent to the XM format, but with two effects columns instead of a volume column and an effect column.
        ///
        ///Imago Orpheus was written by Lutz Roeder and released in 1994..
        /// </summary>
        internal static string IDS_MIKCONV_DESCRIPTION_AGENT8 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_DESCRIPTION_AGENT8", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This loader recognizes “Impulse Tracker” modules, currently the most powerful format. These modules support up to 64 real channels, and up to 256 virtual channels with the “New Note Action” feature. Besides, it has the widest range of effects, and supports 16 bit samples as well as surround sound.
        ///
        ///“Impulse Tracker” was written by Jeffrey Lim and released in 1996..
        /// </summary>
        internal static string IDS_MIKCONV_DESCRIPTION_AGENT9 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_DESCRIPTION_AGENT9", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Module header have some bad values.
        /// </summary>
        internal static string IDS_MIKCONV_ERR_BAD_HEADER {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_ERR_BAD_HEADER", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can&apos;t initialize converter.
        /// </summary>
        internal static string IDS_MIKCONV_ERR_INITIALIZE {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_ERR_INITIALIZE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can&apos;t read the module header.
        /// </summary>
        internal static string IDS_MIKCONV_ERR_LOADING_HEADER {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_ERR_LOADING_HEADER", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can&apos;t read the instrument information.
        /// </summary>
        internal static string IDS_MIKCONV_ERR_LOADING_INSTRUMENTS {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_ERR_LOADING_INSTRUMENTS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can&apos;t read the pattern information.
        /// </summary>
        internal static string IDS_MIKCONV_ERR_LOADING_PATTERNS {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_ERR_LOADING_PATTERNS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can&apos;t read the sample information.
        /// </summary>
        internal static string IDS_MIKCONV_ERR_LOADING_SAMPLEINFO {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_ERR_LOADING_SAMPLEINFO", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can&apos;t read the track information.
        /// </summary>
        internal static string IDS_MIKCONV_ERR_LOADING_TRACKS {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_ERR_LOADING_TRACKS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MikMod Converter.
        /// </summary>
        internal static string IDS_MIKCONV_NAME {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Composer 669.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_AGENT1 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_AGENT1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Scream Tracker 2.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_AGENT10 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_AGENT10", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Scream Tracker 3.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_AGENT11 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_AGENT11", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Scream Tracker Music Interface Kit.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_AGENT12 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_AGENT12", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to UltraTracker.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_AGENT13 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_AGENT13", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to UniMod.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_AGENT14 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_AGENT14", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unreal Music File.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_AGENT15 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_AGENT15", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to FastTracker II.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_AGENT16 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_AGENT16", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MOD Plugin Packed.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_AGENT17 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_AGENT17", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unis 669.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_AGENT2 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_AGENT2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Digital Sound and Music Interface.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_AGENT3 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_AGENT3", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Asylum.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_AGENT4 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_AGENT4", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Digital Sound Interface Kit.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_AGENT5 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_AGENT5", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Farandole Composer.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_AGENT6 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_AGENT6", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to General DigiMusic.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_AGENT7 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_AGENT7", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Imago Orpheus.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_AGENT8 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_AGENT8", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Impulse Tracker.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_AGENT9 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_AGENT9", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Digital Sound and Music Interface module format {0}.{1}.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_DSMI {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_DSMI", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Impulse Tracker {0}.{1:D2}.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_IT {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_IT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Impulse Tracker 2.14p3.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_IT_214P3 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_IT_214P3", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Compressed Impulse Tracker 2.14p3.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_IT_214P3_COMPRESSED {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_IT_214P3_COMPRESSED", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Impulse Tracker 2.14p4.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_IT_214P4 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_IT_214P4", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Compressed Impulse Tracker 2.14p4.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_IT_214P4_COMPRESSED {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_IT_214P4_COMPRESSED", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Compressed Impulse Tracker {0}.{1:D2}.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_IT_COMPRESSED {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_IT_COMPRESSED", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MOD Plugin Packed ({0}).
        /// </summary>
        internal static string IDS_MIKCONV_NAME_MODPLUGIN {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_MODPLUGIN", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Scream Tracker {0}.{1:D2}.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_S3M {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_S3M", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Imago Orpheus {0}.{1:D2} (S3M format).
        /// </summary>
        internal static string IDS_MIKCONV_NAME_S3M_IMAGO {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_S3M_IMAGO", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Impulse Tracker {0}.{1:D2} (S3M format).
        /// </summary>
        internal static string IDS_MIKCONV_NAME_S3M_IT {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_S3M_IT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Impulse Tracker 2.14p3 (S3M format).
        /// </summary>
        internal static string IDS_MIKCONV_NAME_S3M_IT_214P3 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_S3M_IT_214P3", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Impulse Tracker 2.14p4 (S3M format).
        /// </summary>
        internal static string IDS_MIKCONV_NAME_S3M_IT_214P4 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_S3M_IT_214P4", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unknown Tracker {0}.{1:D2} (S3M format).
        /// </summary>
        internal static string IDS_MIKCONV_NAME_S3M_UNKNOWN {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_S3M_UNKNOWN", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Scream Tracker 2{0}.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_STM {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_STM", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to  (Converted by MOD2STM).
        /// </summary>
        internal static string IDS_MIKCONV_NAME_STM_MOD2STM {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_STM_MOD2STM", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to  (Wuzamod).
        /// </summary>
        internal static string IDS_MIKCONV_NAME_STM_WUZAMOD {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_STM_WUZAMOD", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Scream Tracker Music Interface Kit (STM2STX 1.{0}).
        /// </summary>
        internal static string IDS_MIKCONV_NAME_STX {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_STX", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to UltraTracker 1.{0}.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_ULT {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_ULT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unreal Music File ({0} - {1}).
        /// </summary>
        internal static string IDS_MIKCONV_NAME_UMX {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_UMX", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 1998.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_UMX_1998 {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_UMX_1998", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unreal Beta.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_UMX_BETA {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_UMX_BETA", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Mobile Forces.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_UMX_MOBILE {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_UMX_MOBILE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Return to Na Pali.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_UMX_NAPALI {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_UMX_NAPALI", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tactical Ops.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_UMX_TACTICAL {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_UMX_TACTICAL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unreal Tournament.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_UMX_TOURNAMENT {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_UMX_TOURNAMENT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unreal.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_UMX_UNREAL {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_UMX_UNREAL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} (was {1}).
        /// </summary>
        internal static string IDS_MIKCONV_NAME_UNI {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_UNI", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} (XM format {1}.{2:D2}).
        /// </summary>
        internal static string IDS_MIKCONV_NAME_XM {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_XM", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unknown tracker.
        /// </summary>
        internal static string IDS_MIKCONV_NAME_XM_UNKNOWN {
            get {
                return ResourceManager.GetString("IDS_MIKCONV_NAME_XM_UNKNOWN", resourceCulture);
            }
        }
    }
}
