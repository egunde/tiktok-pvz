# tiktok-pvz
An app that spawns zombies in Plants vs Zombies (PvZ) during a TikTok Live. Simply start the project, enter your TikTok Live username, and wait for gifts.

To use:
1. Clone the project to Visual Studio.
2. Modify the StartupPath
    - Navigate to the file PVZProcess.cs under the project IntelOrca.PvZTools
    - The value for StartupPath should be the root folder of your game which contains the .exe
    ```cs
    internal PvZProcess()
    {
        (string? path, bool goty) = GetInstallLocation(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall", wow6432node: true);
	      if (path is null)
	      {
		      (path, goty) = GetInstallLocation(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", wow6432node: false);
		      //if (path is null) throw new NullReferenceException("Could not find a Plants vs. Zombies installation.");
	      }

	      GOTY = goty;
	      StartupPath = "D:\\Games\\Plants vs. Zombies 1.0.0.1051 EN";
    }
    ```
3. Launch the project and PvZ (any order)
4. Enter a username in the appropriate console window
5. Wait for gifts!

The purpose of two console windows is to allow for either or both window(s) to be displayed on your stream.
The console that says "Interact or Gift to Spawn Zombies" will display who interacted and what zombie is spawning.
The console that asks for username will display the "yoChat", which will display who typed "/yo" and how many times they've said it. 
The yoChat is designed to encourage comments, which will boost interaction and promote your stream.

To create this, I used:
- [TikTokLiveSharp](https://github.com/frankvHoof93/TikTokLiveSharp) by [@frankvHoof93](https://github.com/frankvHoof93)
- [PVZTools](https://github.com/IntelOrca/PVZTools) by [@IntelOrca](https://github.com/IntelOrca)

The portion I wrote can be found in MainForm.cs of IntelOrca.PvZTools and Program.cs of TiktokConnector.
I know it isn't the best software practices, but it does the trick.
Improvements can be made by using IntelOrca's Memory Patch for PvZ to create a new Windows Forms Application which implements the TikTokLiveSharp library.
