# radare2gui_dotnet
Another radare2 gui for windows ( alpha 1.0 )
## Screenshots
**Dissasembly View**

![r2pipe_gui_last](https://cloud.githubusercontent.com/assets/12532269/20506528/0776869a-b055-11e6-8f99-0e0710ea07e8.png)

**Hexadecimal View**

![hexview_lemon](https://cloud.githubusercontent.com/assets/12532269/20545911/2c376e50-b112-11e6-885a-82e315de21d6.png)


**Sections**

![r2pipeguidotnet_sections](https://cloud.githubusercontent.com/assets/12532269/20545956/8533dcbe-b112-11e6-9e5d-0496e209f663.png)

**radare2 usage help ( ? )**

![r2pipeguidotnet_needhelp](https://cloud.githubusercontent.com/assets/12532269/20546023/fdb2993c-b112-11e6-8175-81cc901dfee1.png)

**Nice Themes ^^**

![r3pipe_gui_dotnet_terlinal256_theme](https://cloud.githubusercontent.com/assets/12532269/20459127/22927e56-aeb7-11e6-92a6-b3d9bb0eb3ca.png)

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
### download from builts
Precompiled radare2 binaries can be found here: http://bin.rada.re/

Download the zip with the latest windows binary files ( ex: http://bin.rada.re/radare2-w32-1.0.2.zip ) and extract them to some folder.

### compile radare2.exe from sources ( and other files )
Follow these instructions: https://radare.gitbooks.io/radare2book/content/introduction/windows_compilation.html

![r2pipe_dotnet_sf2](https://cloud.githubusercontent.com/assets/12532269/20446745/854239ba-addb-11e6-81c4-7dd25c48e37f.png)

@m_ndingo
