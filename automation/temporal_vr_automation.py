"""
Temporal VR Project - Unified Automation System
통합된 자동화 시스템 - 모든 기능을 하나로
"""

import os
import json
import subprocess
import shutil
from datetime import datetime, timedelta
from pathlib import Path
from typing import Dict, List, Optional
import sys

class TemporalVRAutomation:
    def __init__(self):
        self.project_root = Path.cwd()
        self.start_date = datetime(2025, 6, 26)
        self.config = self.load_or_create_config()
        
    def load_or_create_config(self) -> Dict:
        """설정 파일 로드 또는 생성"""
        config_file = self.project_root / "automation" / "config.json"
        
        default_config = {
            "blender_path": "C:\\Program Files\\Blender Foundation\\Blender 4.4\\blender.exe",
            "unity_path": "C:\\Program Files\\Unity\\Hub\\Editor\\2023.3.55f1\\Editor\\Unity.exe",
            "cursor_path": "cursor",
            "git_remote": "origin",
            "auto_push": False,
            "archive_daily_contexts": True,
            "unity_project_path": "unity\\TemporalVR"
        }
        
        if config_file.exists():
            try:
                with open(config_file, 'r', encoding='utf-8') as f:
                    loaded_config = json.load(f)
                    # 기본값으로 누락된 키 채우기
                    for key, value in default_config.items():
                        if key not in loaded_config:
                            loaded_config[key] = value
                    return loaded_config
            except:
                print("⚠️ Config file corrupted, using defaults")
                
        # 설정 파일 생성
        config_file.parent.mkdir(exist_ok=True)
        with open(config_file, 'w', encoding='utf-8') as f:
            json.dump(default_config, f, indent=2, ensure_ascii=False)
        return default_config
    
    def get_project_day(self) -> int:
        """프로젝트 진행 일수"""
        delta = datetime.now() - self.start_date
        return delta.days + 1
    
    def archive_daily_context(self):
        """기존 daily_context.md를 아카이브"""
        if not self.config.get("archive_daily_contexts", True):
            return
            
        daily_context_path = self.project_root / 'daily_context.md'
        
        if daily_context_path.exists():
            # 파일의 수정 날짜 확인
            mod_time = datetime.fromtimestamp(daily_context_path.stat().st_mtime)
            archive_date = mod_time.strftime('%Y-%m-%d')
            
            # 아카이브 폴더들
            archive_folders = [
                self.project_root / '.cursor' / 'context' / 'archive',
                self.project_root / 'research' / 'daily_contexts'
            ]
            
            for folder in archive_folders:
                folder.mkdir(parents=True, exist_ok=True)
                archive_filename = f"daily_context_{archive_date}.md"
                archive_path = folder / archive_filename
                
                if not archive_path.exists():
                    shutil.copy2(daily_context_path, archive_path)
                    print(f"📁 Archived to: {folder.name}/{archive_filename}")
    
    def get_git_status(self) -> Dict:
        """Git 상태 정보 수집"""
        try:
            # Git 저장소 확인
            if not (self.project_root / '.git').exists():
                return {
                    "last_commit": "No git repository",
                    "current_branch": "none",
                    "modified": [],
                    "recently_edited": []
                }
            
            # 마지막 커밋 정보
            last_commit = subprocess.check_output(
                ["git", "log", "-1", "--pretty=format:%h - %s (%cr)"],
                text=True, encoding='utf-8', stderr=subprocess.DEVNULL
            ).strip()
            
            # 수정된 파일들
            modified = subprocess.check_output(
                ["git", "diff", "--name-only"],
                text=True, encoding='utf-8'
            ).strip().split('\n')
            
            # 최근 수정된 파일들 (지난 24시간)
            recently_edited = subprocess.check_output(
                ["git", "log", "--since='24 hours ago'", "--name-only", "--pretty=format:"],
                text=True, encoding='utf-8'
            ).strip().split('\n')
            
            # 현재 브랜치
            current_branch = subprocess.check_output(
                ["git", "branch", "--show-current"],
                text=True, encoding='utf-8'
            ).strip()
            
            return {
                "last_commit": last_commit or "No commits yet",
                "current_branch": current_branch or "main",
                "modified": [f for f in modified if f],
                "recently_edited": list(set([f for f in recently_edited if f]))[:5]
            }
        except Exception as e:
            print(f"⚠️ Git status error: {e}")
            return {
                "last_commit": "Git error",
                "current_branch": "unknown",
                "modified": [],
                "recently_edited": []
            }
    
    def calculate_progress(self) -> float:
        """전체 진행률 계산"""
        day = self.get_project_day()
        total_days = 330  # 11개월
        base_progress = (day / total_days) * 100
        
        try:
            # Blender 스크립트 파일 수
            blender_scripts = Path("blender/scripts")
            py_files = len(list(blender_scripts.glob("*.py"))) if blender_scripts.exists() else 0
            
            # Unity 스크립트 파일 수 (재귀적으로 검색)
            unity_path = Path(self.config["unity_project_path"]) / "Assets"
            cs_files = len(list(unity_path.rglob("*.cs"))) if unity_path.exists() else 0
            
            # 연구 노트 수
            research_files = len(list(Path("research").glob("*.md"))) if Path("research").exists() else 0
            
            # 실험 파일 수
            experiment_files = len(list(Path("research/experiments").glob("*.md"))) if Path("research/experiments").exists() else 0
            
            bonus = min((py_files + cs_files + research_files + experiment_files) * 0.3, 15)
            return min(base_progress + bonus, 100)
        except Exception as e:
            print(f"⚠️ Progress calculation warning: {e}")
            return base_progress
    
    def load_yesterday_summary(self) -> str:
        """어제 작업 내용 로드"""
        try:
            notes_file = self.project_root / "research" / "daily_notes.md"
            if notes_file.exists():
                with open(notes_file, 'r', encoding='utf-8') as f:
                    content = f.read()
                    lines = content.split('\n')
                    
                    # 뒤에서부터 검색하여 가장 최근 날짜 섹션 찾기
                    for i in range(len(lines)-1, -1, -1):
                        if lines[i].startswith('## 2025-') or lines[i].startswith('## 2026-'):
                            summary_lines = []
                            for j in range(i+1, min(i+10, len(lines))):
                                if lines[j].startswith('## '):
                                    break
                                if lines[j].strip() and not lines[j].startswith('#'):
                                    summary_lines.append(lines[j].strip())
                            return ' '.join(summary_lines[:3]) if summary_lines else "No summary found"
            return "No previous work recorded"
        except Exception as e:
            return f"Could not load yesterday's summary: {e}"
    
    def suggest_today_focus(self) -> str:
        """오늘의 추천 작업"""
        day = self.get_project_day()
        weekday = datetime.now().weekday()
        
        # 초기 2주 특별 포커스
        if day <= 14:
            early_focus = {
                1: "Project setup and environment configuration",
                2: "Unity VR project initialization and XR setup",
                3: "Blender-Unity pipeline establishment",
                4: "Basic temporal data structure design",
                5: "First VR controller interaction",
                6: "Temporal visualization prototype",
                7: "Week 1 review and documentation",
                8: "Temporal brush concept implementation",
                9: "VR hand tracking refinement",
                10: "Time dimension visualization tests",
                11: "Performance baseline measurement",
                12: "User interaction patterns design",
                13: "Integration testing",
                14: "Week 2 review and planning"
            }
            return early_focus.get(day, "Foundation development")
        
        # 요일별 기본 포커스
        focus_areas = {
            0: "Core feature development",      # 월
            1: "Blender scripting & algorithms", # 화
            2: "Unity VR implementation",        # 수
            3: "Integration & testing",          # 목
            4: "Documentation & review",         # 금
            5: "Experimentation & research",     # 토
            6: "Planning & paper writing"        # 일
        }
        
        return focus_areas.get(weekday, "General development")
    
    def detect_blockers(self) -> List[str]:
        """현재 블로커 감지"""
        blockers = []
        
        # TODO 파일 체크
        todo_file = self.project_root / "TODO.md"
        if todo_file.exists():
            try:
                with open(todo_file, 'r', encoding='utf-8') as f:
                    content = f.read().upper()
                    if "BLOCKER" in content:
                        blockers.append("❗ Critical blockers in TODO.md")
                    if "CRITICAL" in content:
                        blockers.append("⚠️ Critical issues in TODO.md")
            except:
                pass
        
        # 최근 커밋에서 문제 감지
        git_status = self.get_git_status()
        if "fix" in git_status.get("last_commit", "").lower():
            blockers.append("🔧 Recent bug fixes in progress")
        
        # 큰 파일 경고
        try:
            for file_path in Path(".").rglob("*"):
                if file_path.is_file() and file_path.stat().st_size > 50 * 1024 * 1024:  # 50MB
                    if not any(skip in str(file_path) for skip in ['.git', 'node_modules', 'Library']):
                        blockers.append(f"📦 Large file warning: {file_path.name}")
                        break
        except:
            pass
        
        return blockers if blockers else ["✅ No blockers detected"]
    
    def update_project_master(self):
        """project_master.md 업데이트"""
        git_status = self.get_git_status()
        progress = self.calculate_progress()
        blockers = self.detect_blockers()
        day = self.get_project_day()
        
        template = f"""# TEMPORAL VR PROJECT MASTER
Last Updated: {datetime.now().strftime('%Y-%m-%d %H:%M')} (Day {day})

## 🎯 Project Overview
**Goal**: Create a VR system where users can model 3D objects by manipulating time as a 4th dimension
**Target**: SIGGRAPH Asia 2026 Paper Submission
**Duration**: 11 months (June 2025 - May 2026)

## 📊 Current Status
- **Phase**: Day {day} - {self._get_current_phase(day)}
- **Days to Deadline**: {330 - day} days
- **Progress**: {progress:.1f}%
- **Week**: {(day - 1) // 7 + 1} of 47

## 🛠️ Tech Stack
- **Blender 4.4**: Procedural modeling backend
- **Unity 2023.3 LTS**: VR frontend
- **OpenXR**: Cross-platform VR support
- **Cursor AI**: Development assistant

## 📈 Recent Activity
- **Last Commit**: {git_status['last_commit']}
- **Current Branch**: {git_status['current_branch']}
- **Modified Files**: {len(git_status['modified'])}
- **Active Files**: {', '.join(git_status['modified'][:3]) or 'No pending changes'}

## 🎯 Current Focus
- **Primary**: {self.suggest_today_focus()}
- **Research Question**: {self._get_current_research_focus(day)}
- **Paper Section**: {self._get_current_paper_focus(progress)}

## ⚠️ Active Issues
{chr(10).join(f'- {blocker}' for blocker in blockers)}

## 📁 Quick Links
- Research Notes: `research/daily_notes.md`
- Blender Scripts: `blender/scripts/`
- Unity Project: `{self.config["unity_project_path"]}/`
- Knowledge Base: `knowledge/solutions.md`
- Experiments: `research/experiments/`

## 🎯 Today's Priorities
1. {self.suggest_today_focus()}
2. Update research notes with findings
3. Commit progress with meaningful message
"""
        
        master_file = self.project_root / "project_master.md"
        with open(master_file, 'w', encoding='utf-8') as f:
            f.write(template)
        
        print(f"✅ Project master updated (Day {day})")
    
    def create_daily_context(self):
        """daily_context.md 생성"""
        # 기존 파일 아카이브
        self.archive_daily_context()
        
        git_status = self.get_git_status()
        blockers = self.detect_blockers()
        day = self.get_project_day()
        
        template = f"""# Daily Context - {datetime.now().strftime('%Y-%m-%d')} (Day {day})

## 🎯 Today's Focus
**{self.suggest_today_focus()}**

## 📊 Project Status
- **Progress**: {self.calculate_progress():.1f}% complete
- **Phase**: {self._get_current_phase(day)}
- **Week**: {(day - 1) // 7 + 1} of 47

## 📝 Yesterday's Summary
{self.load_yesterday_summary()}

## 💻 Git Status
- **Modified files**: {len(git_status['modified'])}
- **Current branch**: {git_status['current_branch']}
- **Last commit**: {git_status['last_commit']}

## 🔥 Hot Files (Recently Active)
{chr(10).join(f'- `{file}`' for file in git_status['recently_edited']) or '- No recent activity'}

## ⚠️ Current Blockers
{chr(10).join(f'- {blocker}' for blocker in blockers)}

## 📋 Today's Checklist
- [ ] Review yesterday's work
- [ ] {self.suggest_today_focus()}
- [ ] Test changes in VR if applicable  
- [ ] Update `research/daily_notes.md`
- [ ] Commit with descriptive message
- [ ] Run `python automation/temporal_vr_automation.py evening`

## 🎯 Research Focus
**Current RQ**: {self._get_current_research_focus(day)}
**Expected Output**: {self._get_expected_output(day)}

## 💡 Quick Commands
```bash
# Test Blender script
blender -b -P blender/scripts/temporal_base.py

# Open Unity project  
"{self.config['unity_path']}" -projectPath "{self.config['unity_project_path']}"

# Evening routine
python automation/temporal_vr_automation.py evening
```

## 📚 References
- Blender 4.4 API: https://docs.blender.org/api/4.4/
- Unity 2023.3 Docs: https://docs.unity3d.com/2023.3/Documentation/Manual/
- OpenXR Spec: https://www.khronos.org/openxr/

---
*Remember: Every line of code brings us closer to sculpting time itself!* 🚀
"""
        
        context_file = self.project_root / "daily_context.md"
        with open(context_file, 'w', encoding='utf-8') as f:
            f.write(template)
        
        print(f"✅ Daily context created for Day {day}")
    
    def _get_current_phase(self, day: int) -> str:
        """현재 프로젝트 단계"""
        if day <= 14:
            return "Foundation & Setup"
        elif day <= 60:
            return "Core Development"
        elif day <= 120:
            return "Feature Implementation"
        elif day <= 180:
            return "Testing & Optimization"
        elif day <= 240:
            return "User Studies"
        elif day <= 300:
            return "Paper Writing"
        else:
            return "Final Preparation"
    
    def _get_current_research_focus(self, day: int) -> str:
        """현재 연구 질문 포커스"""
        week = (day - 1) // 7
        rq_cycle = week % 3
        
        rq_map = {
            0: "RQ1: Intuitive time dimension representation in VR",
            1: "RQ2: Efficient temporal manipulation paradigms",
            2: "RQ3: Creative workflow enhancements"
        }
        return rq_map.get(rq_cycle, "General research")
    
    def _get_current_paper_focus(self, progress: float) -> str:
        """현재 논문 작성 포커스"""
        if progress < 20:
            return "Literature review collection"
        elif progress < 40:
            return "Method section outline"
        elif progress < 60:
            return "Implementation details"
        elif progress < 80:
            return "Evaluation design"
        else:
            return "Results and discussion"
    
    def _get_expected_output(self, day: int) -> str:
        """오늘의 예상 산출물"""
        weekday = datetime.now().weekday()
        outputs = {
            0: "New feature implementation or algorithm",
            1: "Blender script with temporal functionality", 
            2: "Unity VR interaction improvement",
            3: "Integration test results",
            4: "Documentation or research notes",
            5: "Experimental prototype or demo",
            6: "Weekly summary and next week plan"
        }
        return outputs.get(weekday, "Development progress")
    
    def collect_session_summary(self) -> Dict:
        """세션 요약 대화형 수집"""
        print("\n📝 Daily Session Summary")
        print("=" * 50)
        
        summary = {
            "date": datetime.now().strftime('%Y-%m-%d'),
            "day": self.get_project_day()
        }
        
        # 제목
        print(f"\n📅 Day {summary['day']} Summary")
        summary["title"] = input("오늘 작업 한 줄 요약: ").strip()
        
        # 완료 작업
        print("\n✅ What did you complete today?")
        completed = []
        print("(Enter each item and press Enter. Empty line to finish)")
        while True:
            item = input("- ").strip()
            if not item:
                break
            completed.append(item)
        summary["completed"] = completed
        
        # 기술적 발견
        print("\n🔧 Technical progress/discoveries:")
        summary["technical"] = input("> ").strip()
        
        # 연구 인사이트
        print("\n💡 Research insights (for paper):")
        summary["insights"] = input("> ").strip()
        
        # 문제점
        print("\n⚠️ Issues or blockers:")
        summary["issues"] = input("> ").strip()
        
        # 내일 계획
        print("\n🎯 Tomorrow's plan:")
        next_steps = []
        print("(Enter each item and press Enter. Empty line to finish)")
        while True:
            item = input("- ").strip()
            if not item:
                break
            next_steps.append(item)
        summary["next_steps"] = next_steps
        
        # 회고 (선택)
        print("\n💭 Reflection (optional):")
        reflection = input("> ").strip()
        if reflection:
            summary["reflection"] = reflection
        
        return summary
    
    def update_research_notes(self, summary: Dict):
        """research/daily_notes.md 업데이트"""
        notes_file = self.project_root / "research" / "daily_notes.md"
        
        # 파일이 없으면 생성
        if not notes_file.exists():
            notes_file.parent.mkdir(exist_ok=True)
            with open(notes_file, 'w', encoding='utf-8') as f:
                f.write("# Temporal VR Research Notebook\n\n")
        
        # 노트 추가
        note_content = f"""
## {summary['date']} ({datetime.now().strftime('%a')}) - Day {summary['day']}: {summary['title']}

### 🎯 Completed Tasks
{self._format_list(summary.get('completed', []))}

### 🔧 Technical Progress
{summary.get('technical', 'No technical updates')}

### 💡 Research Insights
{summary.get('insights', 'No new insights today')}

### ⚠️ Issues & Blockers
{summary.get('issues', 'No issues encountered')}

### 📊 Statistics
- Git commits today: {self._count_today_commits()}
- Files modified: {len(self.get_git_status()['modified'])}
- Progress: {self.calculate_progress():.1f}%

### 🎯 Next Steps
{self._format_list(summary.get('next_steps', []))}

{'### 💭 Reflection' + chr(10) + summary['reflection'] if summary.get('reflection') else ''}

---
"""
        
        with open(notes_file, 'a', encoding='utf-8') as f:
            f.write(note_content)
        
        print("✅ Research notes updated")
    
    def _format_list(self, items: List[str]) -> str:
        """리스트를 마크다운 형식으로"""
        if not items:
            return "- None"
        return '\n'.join(f"- {item}" for item in items)
    
    def _count_today_commits(self) -> int:
        """오늘의 커밋 수"""
        try:
            output = subprocess.check_output(
                ["git", "log", "--since='today 00:00'", "--oneline"],
                text=True, encoding='utf-8'
            )
            return len([line for line in output.strip().split('\n') if line])
        except:
            return 0
    
    def add_to_knowledge_base(self, insights: str):
        """지식 베이스에 인사이트 추가"""
        if not insights:
            return
            
        kb_file = self.project_root / "knowledge" / "solutions.md"
        kb_file.parent.mkdir(exist_ok=True)
        
        if not kb_file.exists():
            with open(kb_file, 'w', encoding='utf-8') as f:
                f.write("# Temporal VR - Knowledge Base\n\n")
        
        entry = f"""
## {datetime.now().strftime('%Y-%m-%d')} - Day {self.get_project_day()}

### Insight
{insights}

### Context
- Phase: {self._get_current_phase(self.get_project_day())}
- Research Focus: {self._get_current_research_focus(self.get_project_day())}

### Tags
#insight #day{self.get_project_day()}

---
"""
        
        with open(kb_file, 'a', encoding='utf-8') as f:
            f.write(entry)
        
        print("✅ Knowledge base updated")
    
    def create_git_commit(self, summary: Dict):
        """Git 커밋 생성"""
        try:
            # 변경사항 확인
            git_status = self.get_git_status()
            if not git_status['modified'] and not subprocess.check_output(
                ["git", "ls-files", "--others", "--exclude-standard"], 
                text=True
            ).strip():
                print("ℹ️ No changes to commit")
                return
            
            # 스테이징
            subprocess.run(["git", "add", "-A"], check=True)
            
            # 커밋 메시지
            commit_msg = f"Day {summary['day']}: {summary['title']}"
            
            # 상세 메시지 추가
            if summary.get('completed'):
                details = "\n\nCompleted:\n" + self._format_list(summary['completed'])
                commit_msg += details
            
            # 커밋
            subprocess.run(["git", "commit", "-m", commit_msg], check=True)
            print(f"✅ Committed: {summary['title']}")
            
            # Push 옵션
            if self.config.get("auto_push", False):
                subprocess.run(["git", "push"], check=True)
                print("✅ Pushed to remote")
            else:
                push = input("\n🔄 Push to remote? (y/n): ").lower()
                if push == 'y':
                    subprocess.run(["git", "push"], check=True)
                    print("✅ Pushed to remote")
                    
        except subprocess.CalledProcessError as e:
            print(f"⚠️ Git error: {e}")
        except Exception as e:
            print(f"⚠️ Unexpected error: {e}")
    
    def run_morning_routine(self):
        """아침 루틴"""
        print("\n🌅 Temporal VR - Morning Routine")
        print("=" * 50)
        print(f"📅 {datetime.now().strftime('%Y-%m-%d %A')}")
        print(f"📊 Day {self.get_project_day()} of 330")
        print("=" * 50)
        
        # Git pull
        print("\n[1/3] Pulling latest changes...")
        try:
            result = subprocess.run(["git", "pull"], capture_output=True, text=True)
            if "Already up to date" in result.stdout:
                print("✅ Already up to date")
            else:
                print("✅ Updated from remote")
        except:
            print("⚠️ Git pull failed - check connection")
        
        # 컨텍스트 업데이트
        print("\n[2/3] Updating contexts...")
        self.update_project_master()
        self.create_daily_context()
        
        # 오늘의 포커스 표시
        print("\n[3/3] Today's Focus")
        print("=" * 50)
        print(f"🎯 {self.suggest_today_focus()}")
        print(f"🔬 {self._get_current_research_focus(self.get_project_day())}")
        print(f"📝 Expected output: {self._get_expected_output(self.get_project_day())}")
        
        print("\n✅ Morning setup complete!")
        print("💡 Check daily_context.md for full details")
    
    def run_evening_routine(self):
        """저녁 루틴"""
        print("\n🌙 Temporal VR - Evening Routine")
        print("=" * 50)
        print(f"📅 Completing Day {self.get_project_day()}")
        print("=" * 50)
        
        # 세션 요약 수집
        summary = self.collect_session_summary()
        
        print("\n🔄 Processing your summary...")
        
        # 연구 노트 업데이트
        self.update_research_notes(summary)
        
        # 지식 베이스 업데이트
        if summary.get('insights'):
            self.add_to_knowledge_base(summary['insights'])
        
        # Git 커밋
        self.create_git_commit(summary)
        
        # 통계 표시
        print("\n📊 Today's Statistics")
        print("=" * 50)
        print(f"- Day completed: {self.get_project_day()}/330")
        print(f"- Overall progress: {self.calculate_progress():.1f}%")
        print(f"- Days remaining: {330 - self.get_project_day()}")
        
        print("\n✨ Great work today!")
        print("💤 See you tomorrow for Day {}!".format(self.get_project_day() + 1))

def main():
    """메인 실행 함수"""
    automation = TemporalVRAutomation()
    
    # 명령줄 인자 처리
    if len(sys.argv) > 1:
        command = sys.argv[1].lower()
        
        if command == "morning":
            automation.run_morning_routine()
        elif command == "evening":
            automation.run_evening_routine()
        elif command == "update":
            print("📝 Updating contexts only...")
            automation.update_project_master()
            automation.create_daily_context()
            print("✅ Contexts updated!")
        elif command == "status":
            print(f"📊 Project Status")
            print(f"- Day: {automation.get_project_day()}/330")
            print(f"- Progress: {automation.calculate_progress():.1f}%")
            print(f"- Phase: {automation._get_current_phase(automation.get_project_day())}")
        else:
            print("❓ Unknown command")
            print("Usage: python temporal_vr_automation.py [morning|evening|update|status]")
    else:
        print("🤖 Temporal VR Automation System")
        print("=" * 50)
        print("Commands:")
        print("  morning  - Start your day (update contexts)")
        print("  evening  - End your day (save progress)")
        print("  update   - Update contexts only")
        print("  status   - Show project status")
        print("\nUsage: python temporal_vr_automation.py [command]")

if __name__ == "__main__":
    main()