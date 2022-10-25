
function Get-ScriptDirectory
{
  $Invocation = (Get-Variable MyInvocation -Scope 1).Value
  Split-Path $Invocation.MyCommand.Path
}

$NextLabsSfbBinFolder = Get-ScriptDirectory
write-host $NextLabsSfbBinFolder

$SfbEAM = join-path $NextLabsSfbBinFolder "..\config\SfbServerEnforcer.am"
write-host $SfbEAM

$Existed = Test-Path $SfbEAM
if (!$Existed)
{
	exit
}

[xml]$xml = (gc $SfbEAM)
if ($xml -eq $null)
{
    write-host "SfbServerEnforcer.am XML file is empty"
	exit
}

$ComputerName = (Get-CIMInstance CIM_ComputerSystem).Name + "." + (Get-CIMInstance CIM_ComputerSystem).Domain
write-host $ComputerName

try{
    $PoolFqdn = Get-CsComputer -Identity $ComputerName | Select-Object Pool
}
catch [Exception] {
    write-host "Can't call skype server cmdlet. Quit!"
    exit
}

$ns = New-Object Xml.XmlNamespaceManager $xml.NameTable
$ns.AddNamespace( "r", "http://schemas.microsoft.com/lcs/2006/05" )
$app = $xml.SelectSingleNode("/r:applicationManifest", $ns)
if ($app -eq $null)
{
    write-host "Can't find node r:applicationManifest"
	exit
}

$appURI = $app.Attributes.GetNamedItem("r:appUri").Value
$appFullIdentity = $PoolFqdn.Pool + "/SFBServerEnforcer"
write-host $appURI
write-host $appFullIdentity

if($appFullIdentity -eq $null)
{
    write-host "Can't find attribute appIdentity"
	exit
}

$myCMD = "Remove-CsServerApplication -Identity " + """service:" + $appFullIdentity + """"
write-host $myCMD

Stop-Service "NLSIPProxy" -EA SilentlyContinue

try {
	powershell $myCMD
}
catch [Exception] {
	write-host "New-CsServerApplication does not work. Continue to process!"
}

Start-Sleep -s 3

Stop-Service "RtcSrv" -EA SilentlyContinue

Start-Sleep -s 3

Start-Service "RtcSrv" -EA SilentlyContinue
