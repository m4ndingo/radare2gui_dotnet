# radare2gui_dotnet
Another radare2 gui for windows
## Screenshots
![r2pipe_dotnet_classic](https://cloud.githubusercontent.com/assets/12532269/20447430/0a9937e6-addf-11e6-966f-f62c5616f3f6.png)
![r2pipe_dotnet_sf2](https://cloud.githubusercontent.com/assets/12532269/20446745/854239ba-addb-11e6-81c4-7dd25c48e37f.png)
# Compilation
## Packages required

Nuget console:
```
PM> Install-Package Newtonsoft.Json
PM> Install-Package r2pipe
```
## Framework 4.5.1
Since Nuget installs r2pipe compiled with the .net framework 4.5.1, you have to configure the project for this version; also you need the framework (downloadable from Microsoft)

## Compile r2pipe (from sources) for other .net frameworks
You can download r2pipe from https://github.com/radare/radare2-r2pipe/ and compile against other framework versions. Open the solution (.sln) and compile the dll, then add to this project if needed :) 

## radare2 binaries for Windows
By the moment, follow these instructions: https://radare.gitbooks.io/radare2book/content/introduction/windows_compilation.html

@m_ndingo
