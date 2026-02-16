# setup_links.ps1
# clone 후 Unity Assets/Shared 를 shared/ClassLibrary456 로 연결(정션) 재생성

$ErrorActionPreference = "Stop"

$repo = Split-Path -Parent $MyInvocation.MyCommand.Path
$src  = Join-Path $repo "shared\ClassLibrary456"
$dst  = Join-Path $repo "Omok_Game_client\Assets\Shared"

Write-Host "[INFO] REPO = $repo"
Write-Host "[INFO] SRC  = $src"
Write-Host "[INFO] DST  = $dst"
Write-Host ""

if (-not (Test-Path $src)) {
  Write-Host "[ERROR] 원본 폴더가 없음: $src"
  Write-Host "shared\ClassLibrary456 경로 확인해라."
  exit 1
}

# 목적지 기존 폴더/링크 처리
if (Test-Path $dst) {
  $item = Get-Item $dst -Force

  # Junction/Symlink는 Attributes에 ReparsePoint가 찍힘
  $isLink = ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0

  if ($isLink) {
    Write-Host "[WARN] 기존 링크/정션 제거: $dst"
    Remove-Item $dst -Force
  } else {
    $stamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $backup = "${dst}_backup_$stamp"
    Write-Host "[WARN] 기존 일반폴더 발견 -> 백업으로 이동"
    Write-Host "       $dst -> $backup"
    Move-Item $dst $backup
  }
}

# 정션 생성 (관리자 권한 필요할 수 있음)
Write-Host "[INFO] Creating Junction..."
cmd /c "mklink /J `"$dst`" `"$src`""

Write-Host ""
Write-Host "[OK] 완료. Unity 열면 Assets/Shared가 shared/ClassLibrary456를 가리킨다."
