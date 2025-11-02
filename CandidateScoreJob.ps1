
# Replace with your endpoint URL
$apiUrl = "https://localhost:7180/api/JobApplications/score"

try {
    $response = Invoke-WebRequest -Uri $apiUrl -Method GET
    Write-Output "$(Get-Date): Success - $($response.StatusCode)" | Out-File "C:\ApiJobLogs\apijob.txt" -Append
}
catch {
    Write-Output "$(Get-Date): Failed - $($_.Exception.Message)" | Out-File "C:\ApiJobLogs\apijob.txt" -Append
}
