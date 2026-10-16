$meters = @(
    "METER001",
    "METER002",
    "METER003"
)

foreach ($meter in $meters)
{
    Start-Process powershell `
        -ArgumentList "-NoExit", "-Command", "dotnet run --project .\SmartMeter.Client.csproj -- $meter"
}