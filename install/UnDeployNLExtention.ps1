
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

$NLRoomJS = join-path $RoomManagerNLJSFolder "NlRoomForm.js"
write-host $NLRoomJS

$NLRoomPNG = join-path $RoomManagerNLRSFolder "room_enforce.png"
write-host $NLRoomPNG

$NLRoomNPNG = join-path $RoomManagerNLRSFolder "room_notEnforce.png"
write-host $NLRoomNPNG

$Existed = Test-Path $NLRoomJS
if ($Existed)
{
    Remove-Item $NLRoomJS
}

$Existed = Test-Path $NLRoomPNG
if ($Existed)
{
    Remove-Item $NLRoomPNG
    Remove-Item -Force -Recurse $RoomManagerNLRSFolder
}

$Existed = Test-Path $NLRoomNPNG
if ($Existed)
{
    Remove-Item $NLRoomNPNG
    Remove-Item -Force -Recurse $RoomManagerNLRSFolder
}

$WebConfig = join-path $RoomManagerWebFolder "Web.Config"
$Existed = Test-Path $WebConfig
if (!$Existed)
{
	exit
}
$WebConfigBak = join-path $RoomManagerWebFolder "Web.Config.bak"
Copy-Item $WebConfig -Destination $WebConfigBak

$xml = [xml](Get-Content $WebConfig)
$modules = $xml.SelectSingleNode("/configuration/system.webServer/modules")
if ($modules -eq $null)
{
    write-host "no modules"
	exit
}
$elemadd = $modules.SelectSingleNode("descendant::add[@name='NextlabsSFBServerEnforce']")
if ($elemadd)
{
    write-host "found NextlabsSFBServerEnforce module and remove"
	$modules.RemoveChild($elemadd)
	$xml.Save($WebConfig)
}

Start-Sleep -s 3

invoke-command -scriptblock {iisreset}
