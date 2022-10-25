
function Get-ScriptDirectory
{
  $Invocation = (Get-Variable MyInvocation -Scope 1).Value
  Split-Path $Invocation.MyCommand.Path
}

$NextLabsSfbBinFolder = Get-ScriptDirectory
write-host $NextLabsSfbBinFolder

$NextLabsSfbFolder = Split-Path -parent $NextLabsSfbBinFolder

$NLWebServiceRoot = join-path $NextLabsSfbFolder "WebResource\WSAssistant"
$NLWebSiteRoot = join-path $NextLabsSfbFolder "WebResource\SiteRoot"
write-host $NLWebServiceRoot

$Existed = Test-Path $NLWebServiceRoot
if (!$Existed)
{
	exit
}

New-WebSite -Name "NLWebSite" -Port 5858 -PhysicalPath $NLWebSiteRoot -Force

New-WebApplication -Name "NLAssistant" -Site 'NLWebSite' -PhysicalPath $NLWebServiceRoot -ApplicationPool "DefaultAppPool"

$NET_FW_PROFILE2_DOMAIN = 1
$NET_FW_PROFILE2_PRIVATE = 2
$NET_FW_PROFILE2_PUBLIC = 4
$NET_FW_PROFILE2_ALL = 2147483647

$NET_FW_IP_PROTOCOL_TCP = 6
$NET_FW_IP_PROTOCOL_UDP = 17
$NET_FW_IP_PROTOCOL_ICMPv4 = 1
$NET_FW_IP_PROTOCOL_ICMPv6 = 58

$NET_FW_RULE_DIR_IN = 1
$NET_FW_RULE_DIR_OUT = 2

$NET_FW_ACTION_BLOCK = 0
$NET_FW_ACTION_ALLOW = 1

$fwPolicy = New-Object -ComObject HNetCfg.FwPolicy2

$rule = New-Object -ComObject HNetCfg.FWRule
$rule.Name = 'NextLabs-Classify-Site'
$rule.Profiles = $NET_FW_PROFILE2_ALL
$rule.Enabled = $true
$rule.Action = $NET_FW_ACTION_ALLOW
$rule.Direction = $NET_FW_RULE_DIR_IN
$rule.Protocol = $NET_FW_IP_PROTOCOL_TCP
$rule.LocalPorts = 5858

$fwPolicy.Rules.Add($rule)

Start-Sleep -s 2

invoke-command -scriptblock {iisreset}

