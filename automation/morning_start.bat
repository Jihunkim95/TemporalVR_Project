@echo off
echo ========================================
echo  Temporal VR - Morning Startup
echo  Day %date% 
echo ========================================
echo.

REM 프로젝트 디렉토리로 이동
cd /d C:\Research\TemporalVR_Project

REM Git 업데이트
echo [1/5] Updating from Git...
git pull
if %errorlevel% neq 0 (
    echo ⚠️  Git pull failed! Check your connection.
    pause
    exit /b 1
)

REM Python 스크립트 실행
echo.
echo [2/5] Updating contexts...
python automation\update_contexts.py
if %errorlevel% neq 0 (
    echo ⚠️  Context update failed! Check Python installation.
    pause
    exit /b 1
)

REM Cursor 실행
echo.
echo [3/5] Starting Cursor...
start cursor .
timeout /t 2 >nul

REM Blender 실행 (선택사항)
echo.
echo [4/5] Would you like to start Blender? (Y/N)
set /p start_blender=
if /i "%start_blender%"=="Y" (
    REM Blender 실행 파일 직접 실행
    if exist "C:\Program Files\Blender Foundation\Blender 4.4\blender.exe" (
        start "" "C:\Program Files\Blender Foundation\Blender 4.4\blender.exe"
        
        REM 최근 작업 파일 열기 옵션
        echo.
        echo Open recent blend file? (Y/N)
        set /p open_recent=
        if /i "%open_recent%"=="Y" (
            if exist "blender\assets\models\temporal_demo.blend" (
                start "" "C:\Program Files\Blender Foundation\Blender 4.4\blender.exe" "blender\assets\models\temporal_demo.blend"
            )
        )
    ) else (
        echo ⚠️  Blender 4.4 not found at default location!
        echo Please update the path in morning_start.bat
    )
)

REM Unity 실행 (선택사항)
echo.
echo [5/5] Would you like to start Unity? (Y/N)
set /p start_unity=
if /i "%start_unity%"=="Y" (
    REM Unity Hub를 통한 프로젝트 열기
    if exist "C:\Program Files\Unity\Hub\Editor\2023.3.55f1\Editor\Unity.exe" (
        start "" "C:\Program Files\Unity\Hub\Editor\2023.3.55f1\Editor\Unity.exe" -projectPath "%cd%\unity\TemporalVR"
    ) else (
        echo ⚠️  Unity 2023.3 LTS not found!
        echo Trying Unity Hub...
        start "" "unityhub://2023.3.55f1/%cd%\unity\TemporalVR"
    )
)

REM 오늘의 컨텍스트 표시
echo.
echo ========================================
echo ✅ Morning setup complete!
echo.
echo 📋 Today's Context Summary:
type daily_context.md | findstr /i "Today's Focus" 
type daily_context.md | findstr /i "Day"
echo.
echo 💡 Full context in: daily_context.md
echo ========================================
echo.
pause