param(
    [string]$BaseUrl = 'http://localhost:44349',
    [int]$IntervalSeconds = 2
)

$logPath = Join-Path $PSScriptRoot 'workshop-realtime-demo.log'
$client = [System.Net.Http.HttpClient]::new()
$client.Timeout = [TimeSpan]::FromSeconds(5)

$devices = @(
    [pscustomobject]@{
        Code = 'HD-FIVA-001'
        Phase = 0.0
        Pressure = 24.0
        Flow = 112.0
        Temperature = 46.0
        Torque = 305.0
        Vibration = 2.0
    },
    [pscustomobject]@{
        Code = 'NB-FIVA-001'
        Phase = 1.5
        Pressure = 19.0
        Flow = 94.0
        Temperature = 42.0
        Torque = 268.0
        Vibration = 1.7
    },
    [pscustomobject]@{
        Code = 'SZ-FIVA-001'
        Phase = 3.0
        Pressure = 29.0
        Flow = 126.0
        Temperature = 51.0
        Torque = 342.0
        Vibration = 2.6
    }
)

$round = 0
while ($true) {
    $round++
    $now = Get-Date

    try {
        $heartbeatTasks = @(
            foreach ($device in $devices) {
                $encodedCode = [uri]::EscapeDataString($device.Code)
                $client.GetAsync("$BaseUrl/api/workshop/telemetry/online?deviceCode=$encodedCode")
            }
        )
        [System.Threading.Tasks.Task]::WaitAll([System.Threading.Tasks.Task[]]$heartbeatTasks)
        foreach ($task in $heartbeatTasks) {
            $task.Result.EnsureSuccessStatusCode() | Out-Null
        }

        $contents = [System.Collections.Generic.List[System.Net.Http.HttpContent]]::new()
        $dataTasks = @(
            foreach ($device in $devices) {
            $wave = [Math]::Sin(($round + $device.Phase) / 3.0)
            $payload = @{
                DeviceCode = $device.Code
                Timestamp = $now.ToString('o')
                Values = @{
                    Pressure = [Math]::Round($device.Pressure + $wave * 2.0, 2)
                    Flow = [Math]::Round($device.Flow + $wave * 6.0, 2)
                    Temperature = [Math]::Round($device.Temperature + $wave * 1.5, 2)
                    Torque = [Math]::Round($device.Torque + $wave * 12.0, 2)
                    Vibration = [Math]::Round($device.Vibration + $wave * 0.25, 2)
                }
                EmergencyStop = $false
            } | ConvertTo-Json -Depth 5

                $content = [System.Net.Http.StringContent]::new(
                    $payload,
                    [System.Text.Encoding]::UTF8,
                    'application/json'
                )
                $contents.Add($content)
                $client.PostAsync("$BaseUrl/api/workshop/realtime-data", $content)
            }
        )
        [System.Threading.Tasks.Task]::WaitAll([System.Threading.Tasks.Task[]]$dataTasks)
        foreach ($task in $dataTasks) {
            $task.Result.EnsureSuccessStatusCode() | Out-Null
        }
        foreach ($content in $contents) {
            $content.Dispose()
        }

        Add-Content -LiteralPath $logPath -Value (
            "{0:yyyy-MM-dd HH:mm:ss} round={1} devices={2}" -f (Get-Date), $round, $devices.Count
        )
    }
    catch {
        Add-Content -LiteralPath $logPath -Value (
            "{0:yyyy-MM-dd HH:mm:ss} round={1} ERROR {2}" -f (Get-Date), $round, $_.Exception.Message
        )
    }

    Start-Sleep -Seconds $IntervalSeconds
}
