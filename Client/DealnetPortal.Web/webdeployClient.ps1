param 
( 
[Parameter(Mandatory=$false)] [String]$checkoutDir,
[Parameter(Mandatory=$false)] [String]$packageFile
)

$webdeploy = "C:\Program Files (x86)\IIS\Microsoft Web Deploy V3\msdeploy.exe"
$webApp = "Stage-Client/Dev-DealerPortal"
$computerName = "https://72.142.133.45:8172/msdeploy.axd"
$username = "FS-S2012-DEV01\stage_TeamCity_User"
$password = "sfZUk3a13lk3xiwNLnyN"
$paramFile = $checkoutDir + "\Client\DealnetPortal.Web\DeployParameters.Web.deaslnet01vm.xml"

function Deploy() 
{
    #& $webdeploy -verb:delete -dest:contentPath=$webApp/Content,ComputerName=$computerName,UserName=$username,Password=$password,IncludeAcls="False",AuthType="Basic" -retryAttempts:5 -allowUntrusted -enablerule:AppOffline
    try 
    {
        & $webdeploy -verb:sync -source:package=$packageFile -dest:auto,computerName=$computerName,userName=$username,password=$password,AuthType="Basic",IncludeAcls="False" '-setParam:"IIS Web Application Name"="Stage-Client/Dev-DealerPortal"' -setParamFile:$paramFile -verbose -enableRule:DoNotDeleteRule -allowUntrusted -enablerule:AppOffline -disableLink:AppPoolExtension -disableLink:CertificateExtension
     } 
     catch 
     {
        Write-Error -Exception $_.Exception
        Exit 1
      }
}

function LogFailed($text)
{
	Write-Host $text -ForegroundColor Red
}

function LogSuccess($text)
{
	Write-Host $text -ForegroundColor Green
}

$netAssembly = [Reflection.Assembly]::GetAssembly([System.Net.Configuration.SettingsSection])

if($netAssembly)
{
    $bindingFlags = [Reflection.BindingFlags] "Static,GetProperty,NonPublic"
    $settingsType = $netAssembly.GetType("System.Net.Configuration.SettingsSectionInternal")

    $instance = $settingsType.InvokeMember("Section", $bindingFlags, $null, $null, @())

    if($instance)
    {
        $bindingFlags = "NonPublic","Instance"
        $useUnsafeHeaderParsingField = $settingsType.GetField("useUnsafeHeaderParsing", $bindingFlags)

        if($useUnsafeHeaderParsingField)
        {
          $useUnsafeHeaderParsingField.SetValue($instance, $true)
        }
    }
}

Deploy