if not exist ArchiSteamFarm\ArchiSteamFarm (git submodule update --init)
git submodule foreach "git fetch origin; git checkout $(git rev-list --tags --max-count=1);"
del .\ItemDispenser\*.zip
dotnet publish -c "Release" -f "net48" -o "out/generic-netf"
rename .\ItemDispenser\ItemDispenser.zip ItemDispenser-netf.zip 
dotnet publish -c "Release" -f "netcoreapp3.1" -o "out/generic" "/p:LinkDuringPublish=false"