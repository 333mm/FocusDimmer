$manifestPath = "$PSScriptRoot\FocusDimmer.Package\Package.appxmanifest"

if (-not (Test-Path $manifestPath)) {
    Write-Error "Manifest file not found at $manifestPath"
    exit 1
}

[xml]$xml = Get-Content $manifestPath

# Get current Identity Name
$identityName = $xml.Package.Identity.Name
Write-Host "Current Identity: $identityName"

$targetName = ""

if ($identityName -eq "sanmiri.FocusDimmer") {
    $targetName = "Focus Dimmer Pro"
    Write-Host "Detected PRO version. Setting name to: $targetName"
}
elseif ($identityName -eq "sanmiri.FocusDimmerLite") {
    $targetName = "Focus Dimmer"
    Write-Host "Detected LITE version. Setting name to: $targetName"
}
else {
    Write-Warning "Unknown Identity Name: $identityName. No changes made."
    exit
}

# Define Namespaces for XmlNamespaceManager
$ns = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
$ns.AddNamespace("p", "http://schemas.microsoft.com/appx/manifest/foundation/windows10")
$ns.AddNamespace("uap", "http://schemas.microsoft.com/appx/manifest/uap/windows10")
$ns.AddNamespace("desktop", "http://schemas.microsoft.com/appx/manifest/desktop/windows10")

# 1. Update Properties > DisplayName
$node = $xml.SelectSingleNode("//p:Properties/p:DisplayName", $ns)
if ($node) { $node.InnerText = $targetName }

# 2. Update Application > VisualElements > DisplayName
$node = $xml.SelectSingleNode("//uap:VisualElements", $ns)
if ($node) { $node.SetAttribute("DisplayName", $targetName) }

# 3. Update Categories > StartupTask > DisplayName
$node = $xml.SelectSingleNode("//desktop:StartupTask", $ns)
if ($node) { $node.SetAttribute("DisplayName", $targetName) }

# Save
$xml.Save($manifestPath)
Write-Host "Manifest updated successfully."
