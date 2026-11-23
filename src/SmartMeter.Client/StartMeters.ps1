$meters = @(
    "METER001",
    "METER002",
    "METER003",
    "METER004",
    "METER005",
    "METER006",
    "METER007",
    "METER008",
    "METER009",
    "METER0010",
    "METER0011",
    "METER0012"
)

foreach ($meter in $meters)
{
    Start-Process powershell `
        -ArgumentList "-NoExit", "-Command", "dotnet run --project .\SmartMeter.Client.csproj -- $meter"
}