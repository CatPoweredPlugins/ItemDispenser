del .\ItemDispenser\*.zip
dotnet publish -c "Release" -f "net48" -o "out/generic-netf"
rename .\ItemDispenser\ItemDispenser.zip ItemDispenser-netf.zip 
dotnet publish -c "Release" -f "netcoreapp3.0" -o "out/generic" "/p:LinkDuringPublish=false"