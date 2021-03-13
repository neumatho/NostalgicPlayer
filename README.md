# NostalgicPlayer
Modules are relatively small files which contains various sound samples and information on how they should be played. This way of making music goes a long way back, and even at the time of the Commodore 64 the concept was used. But it was on the Amiga computer it really started off. The original type of modules were made with a program called SoundTracker, but since then a lot of different module types have shown up, all of them offering different features - such as more sound channels, more samples, realtime echo and so on.

NostalgicPlayer is a program that can play these modules. NostalgicPlayer is a Windows version of the APlayer, which was started on the Amiga computer back in 1993, later continued on BeOS, and was a reaction to the lack of good module players for the Amiga.

Please enjoy this program, and support the development by giving any feedback (bug reports and flattering comments :-))

You can find an installation package on Microsoft Store, just search after NostalgicPlayer.

# Structure

The player is structured, so it is easy to create your own user interface around the logic. All the main logic is in the *NostalgicPlayerLibrary* and *NostalgicPlayerKit*. The later is used by the agents to communicate with the player.

Agents can be anything, e.g. players, converters, visuals etc. You only need to include the agents you want to use, if you write your own player around the library. Just remember the credits. All agents are loaded dynamic.

In the Clients folder, you can find two different clients that use the player. *NostalgicPlayerConsole* is a simple player that loads a single module and play it. It will then write some information to the console. *NostalgicPlayer* is a full blown player using Windows Forms and is the one that is released to the public.

# Formats supported

| Format | Extension | Converter | Player | Description |
| ------ | -------------- | --------- | ------ | ----------- |
| D.O.C. SoundTracker II | .mod | | ModTracker | A lot of new effects has been added since the previous Ultimate SoundTracker. Most of them uses the Amiga hardware to do modulation etc. Those effects are not supported. Besides that, other added effects have the order that is known today.<br/><br/>Modules using this player, could also have been made in The Exterminator SoundTracker 2.0. |
| D.O.C. SoundTracker VI | .mod | | ModTracker | This player has the same effects as D.O.C. SoundTracker II, but the speed effect has been added. Most of the effects uses the Amiga hardware to do modulation etc. Those effects are not supported. Besides that, other effects have the order that is known today.<br/><br/>Modules using this player, could also have been made in Defjam SoundTracker III, Alpha Flight SoundTracker IV or D.O.C. SoundTracker IV. |
| D.O.C. SoundTracker IX | .mod | | ModTracker | This tracker was the first one, which introduced the effect order used by all subsequently trackers. The previous versions have some other effects using the Amiga hardware to do modulation etc., but they didn't made it in subsequently trackers. |
| Fast/TakeTracker | .mod | | ModTracker | This tracker is from the PC, but uses the same file format as the other mod trackers. It supports up to 32 channels. TakeTracker only up to 16 channels. The modules use the same ID mark, so thats why they are under one format. |
| JamCracker | .jam | | JamCracker | It came from the Amiga, but it's not the most used format. There are some nice tunes available by Dr. Awesome (Bjørn Lynne). |
| Master SoundTracker 1.0 | .mod | | ModTracker | This tracker introduced bigger samples (up to 32 KB), but have the same effects as SoundTracker IX. |
| MultiTracker | .mtm | | ModTracker | This tracker from the PC, was the first one to have 32 channels and supports GUS soundcards. It introduced the mtm file format, which I will say is the successor to the mod format. In this format, a pattern contains indivual tracks, which can be combined as will. |
| NoiseTracker | .mod | | ModTracker | Before ProTracker, this format was the most used. Many people think, that the M.K. mark found in the module file, stands for Mahoney & Kaktus (the creators of NoiseTracker), but that is wrong. In fact, it stands for Michael Kleps, who created a lot of the previous SoundTracker editors. It is also him who introduced 31 samples and there the mark was introduced.<br/><br/>Modules using this player, could have been made in any SoundTracker from 2.3 to 2.6, NoiseTracker 1.x and NoiseTracker 2.x. |
| SoundTracker 2.2 | .mod | | ModTracker | More effects was added to this tracker (position jump and pattern break). It was also the last tracker with only 15 samples. All subsequently trackers have 31 samples. |
| StarTrekker | .mod | | ModTracker | This is a NoiseTracker clone, but improved by the possibility to have synth samples, which are stored in an external file with the extension .nt. NostalgicPlayer supports playing synth sound, if the file is found. |
| StarTrekker 8 voices | .mod | | ModTracker | This is the same as the original StarTrekker, except it uses 8 voices. |
| ProTracker | .mod | | ModTracker | This was a very popular tracker on the demo scene. When it came out, almost everybody started using it. It has a lot of extra effects than NoiseTracker and introduced the BPM tempo (almost like Ultimate SoundTracker 1.8). |
| Ultimate SoundTracker 1.0 - 1.21 | .mod | | ModTracker | This is one of the first trackers created on the Amiga. It was created by Karsten Obarski to make music for games. The file format has later been used and extended in other trackers. This format only supports 15 samples and two effects. |
| Ultimate SoundTracker 1.8 - 2.0 | .mod | | ModTracker | This is one of the first trackers created on the Amiga. It was created by Karsten Obarski to make music for games. The file format has later been used and extended in other trackers. This format only supports 15 samples and two effects. This version introduced variable tempo and multiple sample disks. |
