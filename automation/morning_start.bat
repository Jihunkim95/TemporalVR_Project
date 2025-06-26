@echo off
echo ========================================
echo  Temporal VR - Morning Startup
echo ========================================
echo.

REM Git 업데이트
echo [1/4] Updating from Git...
git pull

REM Python 스크립트 실행
echo.
echo [2/4] Updating contexts...
python automation\update_contexts.py

REM Cursor 실행
echo.
echo [3/4] Starting Cursor...
start cursor .

REM Blender 실행 (선택사항)
echo.
echo [4/4] Would you like to start Blender? (Y/N)
set /p start_blender=
if /i "%start_blender%"=="Y" (
    start "" "C:\Research\TemporalVR_Project\blender\assets\models\temporal_demo.blend"
)

echo.
echo ✅ Morning setup complete!
echo 📋 Check daily_context.md for today's focus
echo.
pause