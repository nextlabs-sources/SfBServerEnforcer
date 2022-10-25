
function Get-ScriptDirectory
{
  $Invocation = (Get-Variable MyInvocation -Scope 1).Value
  Split-Path $Invocation.MyCommand.Path
}

$NextLabsSfbBinFolder = Get-ScriptDirectory
write-host $NextLabsSfbBinFolder

$NLRoomJS = join-path $NextLabsSfbBinFolder "NlRoomForm.js"
$NLRoomPNG = join-path $NextLabsSfbBinFolder "room_enforce.png"
$NLRoomNPNG = join-path $NextLabsSfbBinFolder "room_notEnforce.png"
write-host $NLRoomJS
write-host $NLRoomPNG
write-host $NLRoomNPNG

$Existed = Test-Path $NLRoomJS
if (!$Existed)
{
	exit
}

$Existed = Test-Path $NLRoomPNG
if (!$Existed)
{
	exit
}

$Existed = Test-Path $NLRoomNPNG
if (!$Existed)
{
	exit
}

$RoomManagerWebFolder = Get-WebFilePath 'IIS:\Sites\Skype for Business Server Internal Web Site\PersistentChat'
$RoomManagerNLJSFolder = join-path $RoomManagerWebFolder "RM\NLJScripts"
$RoomManagerNLRSFolder = join-path $RoomManagerWebFolder "RM\NLResources"
write-host $RoomManagerWebFolder
write-host $RoomManagerNLJSFolder
write-host $RoomManagerNLRSFolder

$Existed = Test-Path $RoomManagerWebFolder
if (!$Existed)
{
	exit
}

$Existed = Test-Path $RoomManagerNLJSFolder
if (!$Existed)
{
    New-Item $RoomManagerNLJSFolder -type directory -force
}

$Existed = Test-Path $RoomManagerNLRSFolder
if (!$Existed)
{
    New-Item $RoomManagerNLRSFolder -type directory -force
}

Copy-Item $NLRoomJS -Destination $RoomManagerNLJSFolder
Copy-Item $NLRoomPNG -Destination $RoomManagerNLRSFolder
Copy-Item $NLRoomNPNG -Destination $RoomManagerNLRSFolder


$WebConfig = join-path $RoomManagerWebFolder "Web.Config"
$Existed = Test-Path $WebConfig
if (!$Existed)
{
	exit
}
$WebConfigBak = join-path $RoomManagerWebFolder "Web.Config.bak"
Copy-Item $WebConfig -Destination $WebConfigBak
write-host $WebConfigBak

$xml = [xml](Get-Content $WebConfig)
$sysWebServer = $xml.SelectSingleNode("/configuration/system.webServer")

$hcdll = join-path $NextLabsSfbBinFolder "HTTPComponent.dll"
$hcversion = (Get-Command $hcdll).FileVersionInfo.FileVersion
write-host $hcversion
$hcattribute = 'Nextlabs.SFBServerEnforcer.HTTPComponent.HTTPModuleMain, HTTPComponent, Version=' + $hcversion + ', Culture=neutral, PublicKeyToken=560279cf2b780177'
write-host $hcattribute

if ($sysWebServer.SelectSingleNode("modules") -eq $null)
{
	$modules = $xml.CreateNode([System.Xml.XmlNodeType]::Element,'modules',$null)
    $sysWebServer.AppendChild($modules)

    $elem = $xml.CreateNode([System.Xml.XmlNodeType]::Element,'add',$null)
    $elem.SetAttribute('name','NextlabsSFBServerEnforce')
    $elem.SetAttribute('type',$hcattribute)
    
    $modules.AppendChild($elem)
}
else
{
    $modules = $sysWebServer.SelectSingleNode("modules")

	$elemadd = $modules.SelectSingleNode("descendant::add[@name='NextlabsSFBServerEnforce']")
	if ($elemadd)
	{
		$modules.RemoveChild($elemadd)	
	}

	$elem = $xml.CreateNode([System.Xml.XmlNodeType]::Element,'add',$null)
	$elem.SetAttribute('name','NextlabsSFBServerEnforce')
	$elem.SetAttribute('type',$hcattribute)
		
	$modules.AppendChild($elem)
}

$xml.Save($WebConfig)

Start-Sleep -s 2

invoke-command -scriptblock {iisreset}
