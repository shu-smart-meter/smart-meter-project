$meters = @(
    "METER001"
)

foreach ($meter in $meters)
{
    Start-Process powershell `
        -ArgumentList "-NoExit", "-Command", "dotnet run --project .\SmartMeter.Client.csproj -- $meter"
}