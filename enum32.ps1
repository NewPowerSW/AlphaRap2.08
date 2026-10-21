[void][System.Reflection.Assembly]::LoadWithPartialName("Cognex.VisionPro.Core")
$core = [System.Reflection.Assembly]::LoadWithPartialName("Cognex.VisionPro.Core")
$t = $core.GetType("Cognex.VisionPro.CogFrameGrabbers")
Write-Output ("Is64Bit=" + [Environment]::Is64BitProcess + "  core=" + $core.GetName().Version)
try {
  $g = [Activator]::CreateInstance($t, [Type[]]@())
  Write-Output ("Count=" + $g.Count)
  for ($i = 0; $i -lt $g.Count; $i++) {
    $fg = $g[$i]
    Write-Output ("  [" + $i + "] Name=" + $fg.Name + " SN=" + $fg.SerialNumber + " UID=" + $fg.UniqueID)
  }
  $g.Dispose()
} catch { Write-Output ("ERR: " + $_.Exception.Message) }