
function Get-ScriptDirectory
{
  $Invocation = (Get-Variable MyInvocation -Scope 1).Value
  Split-Path $Invocation.MyCommand.Path
}


$NextLabsSfbBinFolder = Get-ScriptDirectory
write-host $NextLabsSfbBinFolder
$NextLabsSfbFolder = Split-Path -parent $NextLabsSfbBinFolder

Remove-WebSite -Name "NLWebSite"

Remove-NetFirewallRule -DisplayName "NextLabs-Classify-Site"

Start-Sleep -s 2

invoke-command -scriptblock {iisreset}

