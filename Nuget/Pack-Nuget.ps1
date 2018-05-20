foreach($file in (Get-Item -Path .\ -Filter "*.nuspec")){
    .\nuget.exe pack $file.FullName
}