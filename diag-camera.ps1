param([string]$AppDir = "")

function W($s) { Write-Output $s }
function TryGet($obj, $prop) {
    try { $v = $obj.$prop; if ($null -eq $v) { return "" } return [string]$v } catch { return "<n/a>" }
}

W ("Time             : " + (Get-Date).ToString("yyyy-MM-dd HH:mm:ss"))
W ("OS               : " + [Environment]::OSVersion.VersionString)
W ("ProcessIs64Bit   : " + [Environment]::Is64BitProcess)
W ""

# ---------- 1. VisionPro core ----------
$core = $null
try { $core = [System.Reflection.Assembly]::LoadWithPartialName("Cognex.VisionPro.Core") } catch { }
if ($null -eq $core) {
    W "VisionPro Core    : NOT FOUND for this bitness"
} else {
    W ("VisionPro Core    : v" + $core.GetName().Version + "   " + $core.Location)
}
W ""

# ---------- 2. raw frame grabber enumeration ----------
W "--- CogFrameGrabbers (same API AlphaRap uses) ---"
if ($null -ne $core) {
    try {
        $t = $core.GetType("Cognex.VisionPro.CogFrameGrabbers")
        $g = [Activator]::CreateInstance($t, [Type[]]@())
        W ("Count             : " + $g.Count)
        for ($i = 0; $i -lt $g.Count; $i++) {
            $fg = $g[$i]
            W ("  [" + $i + "] Name='" + (TryGet $fg "Name") + "'  SN='" + (TryGet $fg "SerialNumber") + "'  UID='" + (TryGet $fg "UniqueID") + "'")
        }
        $g.Dispose()
    } catch {
        W ("ERROR             : " + $_.Exception.GetBaseException().Message)
    }
} else {
    W "skipped (no VisionPro for this bitness)"
}
W ""

# ---------- 3. locate the AlphaRap folder and its config ----------
if ([string]::IsNullOrEmpty($AppDir)) {
    $here = Split-Path -Parent $MyInvocation.MyCommand.Path
    foreach ($c in @((Join-Path $here "AlphaRap\bin\x64\Debug"), $here)) {
        if (Test-Path (Join-Path $c "AlphaRap.exe")) { $AppDir = $c; break }
    }
}
W ("--- AlphaRap ---")
if ([string]::IsNullOrEmpty($AppDir)) {
    W "app folder        : not found (pass it as the first argument)"
} else {
    W ("app folder        : " + $AppDir)

    $cfgPath = Join-Path $AppDir "ModuleData\SettingData\VPForm.Cameras.xml"
    if (Test-Path $cfgPath) {
        try {
            $xml = [xml](Get-Content $cfgPath -Encoding UTF8)
            $cams = @($xml.SelectNodes("//Camera"))
            W ("config cameras    : " + $cams.Count + "   (" + $cfgPath + ")")
            foreach ($c in $cams) {
                W ("  camera Name='" + $c.Name + "'  SN='" + $c.SN + "'  Exposure='" + $c.Exposure + "'")
            }
        } catch {
            W ("config read error  : " + $_.Exception.Message)
        }
    } else {
        W ("config cameras    : no config file at " + $cfgPath)
    }

    try {
        $exe = Join-Path $AppDir "AlphaRap.exe"
        $display = Join-Path $AppDir "Cognex.VisionPro.Display.Controls.dll"
        if (Test-Path $display) { [void][System.Reflection.Assembly]::LoadFrom($display) }
        $app = [System.Reflection.Assembly]::LoadFrom($exe)
        $dev = $app.GetType("AlphaRap.CogCameraDevices")
        $enum = $dev.GetMethod("Enumerate", [type[]]@([bool]))
        $list = $enum.Invoke($null, [object[]]@($true))
        W ("app enumerate     : " + $list.Count + " device(s)")
        for ($i = 0; $i -lt $list.Count; $i++) {
            W ("  [" + $i + "] " + $list[$i].ToString() + "   bindKey='" + $list[$i].BindKey + "'")
        }
        $err = $dev.GetField("LastError").GetValue($null)
        W ("app LastError     : " + $(if ([string]::IsNullOrEmpty($err)) { "(none)" } else { $err }))
    } catch {
        W ("app enumerate     : ERROR -> " + $_.Exception.GetBaseException().Message)
    }
}
W ""

# ---------- 4. network + firewall (GigE discovery needs broadcast) ----------
W "--- network ---"
try {
    Get-NetIPAddress -AddressFamily IPv4 -ErrorAction Stop |
        Where-Object { $_.IPAddress -notlike "127.*" } |
        ForEach-Object { W ("  adapter " + $_.InterfaceIndex + " : " + $_.IPAddress + "/" + $_.PrefixLength + "  " + $_.InterfaceAlias) }
} catch {
    W ("  (cannot query adapters: " + $_.Exception.Message + ")")
}

W "--- firewall rules mentioning AlphaRap ---"
try {
    $rules = netsh advfirewall firewall show rule name=all dir=in 2>$null | Select-String -SimpleMatch "AlphaRap"
    if ($rules) { foreach ($r in $rules) { W ("  " + $r.ToString().Trim()) } }
    else { W "  none found (an inbound rule is normally NOT required, but a blocked app can hide GigE cameras)" }
} catch {
    W ("  (cannot query firewall: " + $_.Exception.Message + ")")
}
W ""
W "done"