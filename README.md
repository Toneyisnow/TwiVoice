# TwiVoice

## Prerequisite

	Install Vistual Studio 2019 Community
	Install dotNet Core 3.1 SDK
	Install ASP.Net Core 3.1 SDK
	
	
## Build

	Open VS 2019, Build.

	
## Validate

	Usage of Commandline:
	
	TwiVoice.Core.Exe --usttowav <ust_file> <output_wav> <resampler_file> <voice_folder>
    TwiVoice.Core.Exe --usttojson <ust_file> <output_json> <resampler_file> <voice_folder>
    TwiVoice.Core.Exe --jsontowav <json_file> <output_wav>
    TwiVoice.Core.Exe --jsontotxt <json_file> <output_txt>


## Deploy

	1. On dev machine, build solution, and right click on "TwiVoiceWebService" project, click "Publish...", click Publish.
	2. On Production machine:
		1) Install ASP.NET Core Runtime 3.1
		2) Create a Site in IIS
		3) Copy the resampler file and voice banks into a static folder, and grant "ReadWrite" permission on that folder to IIS_USER
		4) Copy the folder from dev machine bin/Release/netcoreapp3.1/publish to the IIS site content folder
		5) Update the twi_config.json file to make sure the file paths are correct
	

