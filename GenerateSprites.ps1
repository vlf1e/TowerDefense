Add-Type -AssemblyName System.Drawing

$spritesDir = Join-Path $PSScriptRoot "Assets\Sprites"
if (-not (Test-Path $spritesDir)) {
    New-Item -ItemType Directory -Force -Path $spritesDir | Out-Null
}

function Create-Bitmap($w, $h) {
    $bmp = New-Object System.Drawing.Bitmap($w, $h, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    return $bmp
}

function Get-Graphics($bmp) {
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAlias
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    return $g
}

# 1. tile_path.png (Зона A: путь врагов)
$bmp = Create-Bitmap 64 64
$g = Get-Graphics $bmp
$g.Clear([System.Drawing.Color]::FromArgb(255, 65, 58, 52))
# Рисуем плитку дорожки
$penBorder = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 85, 76, 68), 2)
$penLine = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(180, 50, 44, 40), 1)
$g.DrawRectangle($penBorder, 1, 1, 62, 62)
$g.DrawLine($penLine, 0, 32, 64, 32)
$g.DrawLine($penLine, 32, 0, 32, 32)
$g.DrawLine($penLine, 16, 32, 16, 64)
$g.DrawLine($penLine, 48, 32, 48, 64)
# Стрелочка направления (влево, к базе C)
$arrowBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(120, 200, 190, 170))
[System.Drawing.Point[]]$pts = @(
    [System.Drawing.Point]::new(22, 32),
    [System.Drawing.Point]::new(38, 22),
    [System.Drawing.Point]::new(38, 27),
    [System.Drawing.Point]::new(46, 27),
    [System.Drawing.Point]::new(46, 37),
    [System.Drawing.Point]::new(38, 37),
    [System.Drawing.Point]::new(38, 42)
)
$g.FillPolygon($arrowBrush, $pts)
$bmp.Save((Join-Path $spritesDir "tile_path.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

# 2. tile_buildable.png (Зона B: площадка для башен)
$bmp = Create-Bitmap 64 64
$g = Get-Graphics $bmp
$g.Clear([System.Drawing.Color]::FromArgb(255, 38, 55, 48))
$penB = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 55, 82, 70), 2)
$g.DrawRectangle($penB, 1, 1, 62, 62)
# Точки/углы разметки для строительства
$cornerPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 90, 160, 120), 2)
$g.DrawLine($cornerPen, 6, 6, 16, 6); $g.DrawLine($cornerPen, 6, 6, 6, 16)
$g.DrawLine($cornerPen, 58, 6, 48, 6); $g.DrawLine($cornerPen, 58, 6, 58, 16)
$g.DrawLine($cornerPen, 6, 58, 16, 58); $g.DrawLine($cornerPen, 6, 58, 6, 48)
$g.DrawLine($cornerPen, 58, 58, 48, 58); $g.DrawLine($cornerPen, 58, 58, 58, 48)
# Плюс в центре
$centerBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(80, 100, 180, 140))
$g.FillEllipse($centerBrush, 29, 29, 6, 6)
$bmp.Save((Join-Path $spritesDir "tile_buildable.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

# 3. tile_deadzone.png (Зона D: фон / зона бездействия)
$bmp = Create-Bitmap 64 64
$g = Get-Graphics $bmp
$g.Clear([System.Drawing.Color]::FromArgb(255, 22, 24, 28))
$stripePen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 30, 33, 38), 3)
for ($i = -64; $i -lt 128; $i += 16) {
    $g.DrawLine($stripePen, $i, 64, ($i + 64), 0)
}
$bmp.Save((Join-Path $spritesDir "tile_deadzone.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

# 4. base_crystal.png (Зона C: База / Цитадель / Кристалл)
$bmp = Create-Bitmap 96 96
$g = Get-Graphics $bmp
$g.Clear([System.Drawing.Color]::Transparent)
# Пьедестал базы
$baseBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 45, 52, 65))
$basePen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 75, 110, 150), 3)
$g.FillEllipse($baseBrush, 12, 54, 72, 32)
$g.DrawEllipse($basePen, 12, 54, 72, 32)
# Энергетический синий кристалл
[System.Drawing.Point[]]$crysPts = @(
    [System.Drawing.Point]::new(48, 8),
    [System.Drawing.Point]::new(68, 38),
    [System.Drawing.Point]::new(58, 70),
    [System.Drawing.Point]::new(38, 70),
    [System.Drawing.Point]::new(28, 38)
)
$crysBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
    ([System.Drawing.Point]::new(48, 8)),
    ([System.Drawing.Point]::new(48, 70)),
    [System.Drawing.Color]::FromArgb(255, 120, 220, 255),
    [System.Drawing.Color]::FromArgb(255, 20, 100, 220)
)
$g.FillPolygon($crysBrush, $crysPts)
$crysPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 200, 245, 255), 2)
$g.DrawPolygon($crysPen, $crysPts)
# Блик
$glowBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(180, 255, 255, 255))
$g.FillPolygon($glowBrush, @(
    [System.Drawing.Point]::new(48, 14),
    [System.Drawing.Point]::new(42, 36),
    [System.Drawing.Point]::new(48, 48),
    [System.Drawing.Point]::new(46, 24)
))
$bmp.Save((Join-Path $spritesDir "base_crystal.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

# 5. tower_archer_base.png (База пулеметной башни)
$bmp = Create-Bitmap 64 64
$g = Get-Graphics $bmp
$g.Clear([System.Drawing.Color]::Transparent)
$bBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 50, 60, 75))
$bPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 80, 130, 170), 3)
$g.FillEllipse($bBrush, 8, 8, 48, 48)
$g.DrawEllipse($bPen, 8, 8, 48, 48)
$innerBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 35, 45, 55))
$g.FillEllipse($innerBrush, 18, 18, 28, 28)
$bmp.Save((Join-Path $spritesDir "tower_archer_base.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

# 6. tower_archer_turret.png (Турель пулеметной башни, смотрит вправо)
$bmp = Create-Bitmap 64 64
$g = Get-Graphics $bmp
$g.Clear([System.Drawing.Color]::Transparent)
# Стволы
$barrelBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 120, 140, 160))
$barrelPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 20, 25, 30), 2)
$g.FillRectangle($barrelBrush, 28, 24, 26, 6)
$g.DrawRectangle($barrelPen, 28, 24, 26, 6)
$g.FillRectangle($barrelBrush, 28, 34, 26, 6)
$g.DrawRectangle($barrelPen, 28, 34, 26, 6)
# Корпус поворотной турели
$tBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 40, 160, 220))
$tPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 200, 240, 255), 2)
$g.FillEllipse($tBrush, 18, 18, 28, 28)
$g.DrawEllipse($tPen, 18, 18, 28, 28)
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 240, 245, 255))), 28, 28, 8, 8)
$bmp.Save((Join-Path $spritesDir "tower_archer_turret.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

# 7. tower_frost_base.png
$bmp = Create-Bitmap 64 64
$g = Get-Graphics $bmp
$g.Clear([System.Drawing.Color]::Transparent)
[System.Drawing.Point[]]$hexPts = @(
    [System.Drawing.Point]::new(32, 6),
    [System.Drawing.Point]::new(54, 18),
    [System.Drawing.Point]::new(54, 46),
    [System.Drawing.Point]::new(32, 58),
    [System.Drawing.Point]::new(10, 46),
    [System.Drawing.Point]::new(10, 18)
)
$fbBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 25, 50, 75))
$fbPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 80, 190, 230), 3)
$g.FillPolygon($fbBrush, $hexPts)
$g.DrawPolygon($fbPen, $hexPts)
$bmp.Save((Join-Path $spritesDir "tower_frost_base.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

# 8. tower_frost_crystal.png
$bmp = Create-Bitmap 64 64
$g = Get-Graphics $bmp
$g.Clear([System.Drawing.Color]::Transparent)
[System.Drawing.Point[]]$crysPts = @(
    [System.Drawing.Point]::new(32, 10),
    [System.Drawing.Point]::new(48, 26),
    [System.Drawing.Point]::new(40, 52),
    [System.Drawing.Point]::new(24, 52),
    [System.Drawing.Point]::new(16, 26)
)
$crysB = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 100, 230, 255))
$crysP = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 220, 255, 255), 2)
$g.FillPolygon($crysB, $crysPts)
$g.DrawPolygon($crysP, $crysPts)
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(220, 255, 255, 255))), 27, 27, 10, 10)
$bmp.Save((Join-Path $spritesDir "tower_frost_crystal.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

# 9. tower_mortar_base.png
$bmp = Create-Bitmap 64 64
$g = Get-Graphics $bmp
$g.Clear([System.Drawing.Color]::Transparent)
$mBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 60, 45, 40))
$mPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 180, 90, 50), 3)
$g.FillRectangle($mBrush, 10, 10, 44, 44)
$g.DrawRectangle($mPen, 10, 10, 44, 44)
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 40, 30, 25))), 18, 18, 28, 28)
$bmp.Save((Join-Path $spritesDir "tower_mortar_base.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

# 10. tower_mortar_barrel.png (толстая мортира)
$bmp = Create-Bitmap 64 64
$g = Get-Graphics $bmp
$g.Clear([System.Drawing.Color]::Transparent)
$mbBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 80, 70, 65))
$mbPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 210, 120, 60), 2)
$g.FillRectangle($mbBrush, 26, 22, 28, 20)
$g.DrawRectangle($mbPen, 26, 22, 28, 20)
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 20, 15, 15))), 48, 23, 10, 18)
$baseCircle = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 120, 60, 35))
$g.FillEllipse($baseCircle, 16, 16, 32, 32)
$bmp.Save((Join-Path $spritesDir "tower_mortar_barrel.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

# 11. enemy_grunt.png (Пехотинец)
$bmp = Create-Bitmap 64 64
$g = Get-Graphics $bmp
$g.Clear([System.Drawing.Color]::Transparent)
$egBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 210, 55, 55))
$egPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 100, 20, 20), 3)
$g.FillEllipse($egBrush, 12, 12, 40, 40)
$g.DrawEllipse($egPen, 12, 12, 40, 40)
# Шлем и глаза
$helmBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 130, 140, 155))
$g.FillPie($helmBrush, 14, 14, 36, 36, 180, 180)
$eyeBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 255, 240, 80))
$g.FillEllipse($eyeBrush, 20, 32, 6, 6)
$g.FillEllipse($eyeBrush, 38, 32, 6, 6)
$bmp.Save((Join-Path $spritesDir "enemy_grunt.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

# 12. enemy_scout.png (Быстрый скаут)
$bmp = Create-Bitmap 64 64
$g = Get-Graphics $bmp
$g.Clear([System.Drawing.Color]::Transparent)
[System.Drawing.Point[]]$scoutPts = @(
    [System.Drawing.Point]::new(8, 32),
    [System.Drawing.Point]::new(52, 14),
    [System.Drawing.Point]::new(42, 32),
    [System.Drawing.Point]::new(52, 50)
)
$scoutBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
    ([System.Drawing.Point]::new(8, 32)),
    ([System.Drawing.Point]::new(52, 32)),
    [System.Drawing.Color]::FromArgb(255, 255, 230, 50),
    [System.Drawing.Color]::FromArgb(255, 255, 120, 20)
)
$scoutPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 255, 255, 200), 2)
$g.FillPolygon($scoutBrush, $scoutPts)
$g.DrawPolygon($scoutPen, $scoutPts)
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 255, 255, 255))), 24, 28, 8, 8)
$bmp.Save((Join-Path $spritesDir "enemy_scout.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

# 13. enemy_golem.png (Тяжелый голем / Танк)
$bmp = Create-Bitmap 64 64
$g = Get-Graphics $bmp
$g.Clear([System.Drawing.Color]::Transparent)
$golemBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 75, 45, 95))
$golemPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 170, 95, 215), 3)
$g.FillRectangle($golemBrush, 10, 10, 44, 44)
$g.DrawRectangle($golemPen, 10, 10, 44, 44)
# Броневые пластины
$plateBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 110, 70, 140))
$g.FillRectangle($plateBrush, 14, 14, 16, 16)
$g.FillRectangle($plateBrush, 34, 14, 16, 16)
$g.FillRectangle($plateBrush, 14, 34, 36, 16)
# Зловещие красные глаза
$gEye = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 255, 40, 80))
$g.FillRectangle($gEye, 20, 24, 8, 4)
$g.FillRectangle($gEye, 36, 24, 8, 4)
$bmp.Save((Join-Path $spritesDir "enemy_golem.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

# 14. bullet_arrow.png
$bmp = Create-Bitmap 32 32
$g = Get-Graphics $bmp
$g.Clear([System.Drawing.Color]::Transparent)
$bArrow = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 255, 210, 40))
$g.FillEllipse($bArrow, 4, 10, 24, 12)
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)), 14, 12, 12, 8)
$bmp.Save((Join-Path $spritesDir "bullet_arrow.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

# 15. bullet_frost.png
$bmp = Create-Bitmap 32 32
$g = Get-Graphics $bmp
$g.Clear([System.Drawing.Color]::Transparent)
[System.Drawing.Point[]]$frostPts = @(
    [System.Drawing.Point]::new(16, 2),
    [System.Drawing.Point]::new(28, 16),
    [System.Drawing.Point]::new(16, 30),
    [System.Drawing.Point]::new(4, 16)
)
$g.FillPolygon((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 80, 230, 255))), $frostPts)
$g.DrawPolygon((New-Object System.Drawing.Pen([System.Drawing.Color]::White, 2)), $frostPts)
$bmp.Save((Join-Path $spritesDir "bullet_frost.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

# 16. bullet_mortar.png
$bmp = Create-Bitmap 32 32
$g = Get-Graphics $bmp
$g.Clear([System.Drawing.Color]::Transparent)
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 40, 35, 35))), 6, 8, 20, 20)
$g.DrawEllipse((New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 200, 100, 40), 2)), 6, 8, 20, 20)
# Фитиль
$g.DrawLine((New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 200, 160, 80), 2)), 16, 8, 22, 3)
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 255, 80, 20))), 20, 1, 6, 6)
$bmp.Save((Join-Path $spritesDir "bullet_mortar.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

# 17. circle_range.png (индикатор радиуса стрельбы)
$bmp = Create-Bitmap 256 256
$g = Get-Graphics $bmp
$g.Clear([System.Drawing.Color]::Transparent)
$fillR = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(35, 120, 220, 255))
$penR = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(200, 150, 230, 255), 3)
$penR.DashStyle = [System.Drawing.Drawing2D.DashStyle]::Dash
$g.FillEllipse($fillR, 6, 6, 244, 244)
$g.DrawEllipse($penR, 6, 6, 244, 244)
$bmp.Save((Join-Path $spritesDir "circle_range.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

# 18. circle_splash.png (радиус взрыва мортиры)
$bmp = Create-Bitmap 128 128
$g = Get-Graphics $bmp
$g.Clear([System.Drawing.Color]::Transparent)
$fillS = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(90, 255, 100, 30))
$penS = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(240, 255, 180, 50), 3)
$g.FillEllipse($fillS, 4, 4, 120, 120)
$g.DrawEllipse($penS, 4, 4, 120, 120)
$bmp.Save((Join-Path $spritesDir "circle_splash.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

# 19. ui_slot_bg.png
$bmp = Create-Bitmap 80 80
$g = Get-Graphics $bmp
$g.Clear([System.Drawing.Color]::Transparent)
$bgSlot = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(230, 25, 30, 40))
$penSlot = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 75, 125, 175), 3)
$g.FillRectangle($bgSlot, 4, 4, 72, 72)
$g.DrawRectangle($penSlot, 4, 4, 72, 72)
$bmp.Save((Join-Path $spritesDir "ui_slot_bg.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

# 20. icon_gold.png
$bmp = Create-Bitmap 32 32
$g = Get-Graphics $bmp
$g.Clear([System.Drawing.Color]::Transparent)
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 255, 195, 20))), 2, 2, 28, 28)
$g.DrawEllipse((New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 200, 140, 10), 2)), 2, 2, 28, 28)
$g.DrawString("$", (New-Object System.Drawing.Font("Arial", 14, [System.Drawing.FontStyle]::Bold)), (New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 150, 90, 5))), 7, 4)
$bmp.Save((Join-Path $spritesDir "icon_gold.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

# 21. icon_heart.png
$bmp = Create-Bitmap 32 32
$g = Get-Graphics $bmp
$g.Clear([System.Drawing.Color]::Transparent)
$hBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 235, 45, 65))
[System.Drawing.Point[]]$hPts = @(
    [System.Drawing.Point]::new(16, 28),
    [System.Drawing.Point]::new(4, 16),
    [System.Drawing.Point]::new(4, 9),
    [System.Drawing.Point]::new(10, 4),
    [System.Drawing.Point]::new(16, 9),
    [System.Drawing.Point]::new(22, 4),
    [System.Drawing.Point]::new(28, 9),
    [System.Drawing.Point]::new(28, 16)
)
$g.FillPolygon($hBrush, $hPts)
$g.DrawPolygon((New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 160, 20, 35), 2)), $hPts)
$bmp.Save((Join-Path $spritesDir "icon_heart.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

Write-Output "Sprites generated successfully in $spritesDir"
