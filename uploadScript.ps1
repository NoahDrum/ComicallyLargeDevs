#converted for sh script to ps1 script with ChatGTP

# Function to get user input for PEM key, Public IP, file path, and destination path on EC2
function Get-Input {
    $pemKeyLocation = Read-Host "Enter the path to your PEM key (No quotes)"
    $publicIp = Read-Host "Enter your EC2 public IP"
    $filePath = Read-Host "Enter the full file path to upload (on your local machine) (No quotes)"
    $destinationPath = Read-Host "Enter the full path to the directory on the EC2 instance to upload the file"
    
    # Return the collected input as a hashtable
    return @{ pemKeyLocation = $pemKeyLocation; publicIp = $publicIp; filePath = $filePath; destinationPath = $destinationPath }
}

# Generates SSH command with given input
function Generate-SSHCommand {
    param (
        $pemKeyLocation,
        $publicIp
    )
    $sshCommand = "ssh -i `"$pemKeyLocation`" ec2-user@$publicIp"
    Write-Host "Generated SSH command to connect to your EC2 instance:"
    Write-Host $sshCommand
    return $sshCommand
}

# Generates the upload file SCP command
function Generate-SCPCommand {
    param (
        $pemKeyLocation,
        $publicIp,
        $filePath,
        $destinationPath
    )
    # Format the SCP command with proper escaping for special characters
    $scpCommand = "scp -i `"$pemKeyLocation`" -r `"$filePath`" ec2-user@${publicIp}:$destinationPath"
    Write-Host "Generated SCP command to upload the file:"
    Write-Host $scpCommand
    return $scpCommand
}

# Get user input
$input = Get-Input

# Generate SCP upload command and execute it
$scpCommand = Generate-SCPCommand -pemKeyLocation $input.pemKeyLocation -publicIp $input.publicIp -filePath $input.filePath -destinationPath $input.destinationPath
Write-Host "Executing SCP command..."
Invoke-Expression $scpCommand


# Generate SSH connect command and execute it
$sshCommand = Generate-SSHCommand -pemKeyLocation $input.pemKeyLocation -publicIp $input.publicIp
Write-Host "Executing SSH command..."
Invoke-Expression $sshCommand

