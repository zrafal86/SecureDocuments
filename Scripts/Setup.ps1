# Set-ExecutionPolicy RemoteSigned

$exists = Test-Path $profile
Write-Verbose $exists
if (! $exists) {
    new-item -type file -path $profile -Force
}
invoke-item $profile

function start-project($project) {

    Push-Location "~/dev/github/$project"
    Invoke-Item .;
    Invoke-Item *.sln;
    Invoke-Expression "code ."
}

# start-project("ProgrammedMom")