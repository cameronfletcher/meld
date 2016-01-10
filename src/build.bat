@..\packages\NuGet.CommandLine.3.3.0\tools\NuGet.exe restore packages.config -PackagesDirectory "..\packages"
@msbuild Meld.msbuild /t:Build;Test;Package