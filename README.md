# Albion Data Sharp

## Overview

AlbionDataSharp is a custom client for the [Albion Online Data Project](https://www.albion-online-data.com/). It's built using C# and allows interaction with both the official Albion Online Data servers and your own custom servers. Configuration is done via the `appsettings.json` file.

![Console Image Placeholder](https://github.com/augusto501/AlbionDataSharp/blob/874aac3035656813c7a55cb410a31b036b7d4047/AlbionDataSharp/Screenshots/SS1.png)

## Features

- Network Listening: Listens to network events and uploads data to the Albion Online Data servers or your own NATS servers (market offers, market requests, market histories and gold averages).
- Logging: Utilizes Serilog for logging events and errors.
- Easy-to-Understand Console.
- Server Configuration: Upload data to either the official Albion Online Data servers or your own custom servers, configurable in `appsettings.json`.
- Thread Configuration: Choose the percentage of your thread count to solve PoW for Albion Online Data upload, configurable in `appsettings.json`.
- Automatic updates.

## Inspiration
This project was heavily inspired in:
1. [albiondata-client](https://github.com/ao-data/albiondata-client)
2. [AlbionOnline-StatisticsAnalysis](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis)

## Getting Started

1. Install [NpCap](https://npcap.com/#download). This is required.
2. Download `AlbionDataSharpSetup.exe` from the latest release in the [Releases section](https://github.com/augusto501/AlbionDataSharp/releases).
3. Run the file. Igone Windows safety warnings => you can check the source code if you are suspicious.
4. The installer has no pop ups. The client will be installed and shortcuts will be added to your desktop and Start Menu.
5. If needed, configure `appsettings.json`.
6. Updates will be automatic.
7. If you need to uninstall, go to Control Panel and uninstall as any other app.
