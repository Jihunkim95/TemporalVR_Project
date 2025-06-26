import os
import json
import subprocess
from datetime import datetime, timedelta
from pathlib import Path

class ContextManager:
    def __init__(self):
        self.project_root = Path.cwd()
        self.start_date = datetime(2025, 6, 26)  # 프로젝트 시작일
        
    def get_project_day(self):
        """프로젝트 진행 일수 계산"""
        delta = datetime.now() - self.start_date
        return delta.days + 1
    
    def get_git_status(self):
        """Git 상태 정보 수집"""
        try:
            # 마지막 커밋 정보
            last_commit = subprocess.check_output(
                ["git", "log", "-1", "--pretty=format:%h - %s (%cr)"],
                text=True
            ).strip()
            
            # 수정된 파일들
            modified = subprocess.check_output(
                ["git", "diff", "--name-only"],
                text=True
            ).strip().split('\n')
            
            # 최근 수정된 파일들 (지난 24시간)
            recently_edited = subprocess.check_output(
                ["git", "log", "--since='24 hours ago'", "--name-only", "--pretty=format:"],
                text=True
            ).strip().split('\n')
            
            return {
                "last_commit": last_commit,
                "modified": [f for f in modified if f],
                "recently_edited": list(set([f for f in recently_edited if f]))[:5]
            }
        except:
            return {
                "last_commit": "No commits yet",
                "modified": [],
                "recently_edited": []
            }
    
    def calculate_progress(self):
        """전체 진행률 계산 (대략적)"""
        day = self.get_project_day()
        total_days = 330  # 11개월
        base_progress = (day / total_days) * 100
        
        # 파일 수 기반 보정
        try:
            # Blender 스크립트 파일 수
            blender_scripts_path = Path("blender/scripts")
            if blender_scripts_path.exists():
                py_files = len(list(blender_scripts_path.glob("*.py")))
            else:
                py_files = 0
                
            # Unity 스크립트 파일 수
            unity_scripts_path = Path("unity/TemporalVR/Assets/Scripts")
            if unity_scripts_path.exists():
                cs_files = len(list(unity_scripts_path.glob("*.cs")))
            else:
                cs_files = 0
                
            bonus = min((py_files + cs_files) * 0.5, 10)  # 최대 10% 보너스
            return min(base_progress + bonus, 100)
        except:
            return base_progress
    
    def load_yesterday_summary(self):
        """어제 작업 내용 로드"""
        try:
            notes_file = self.project_root / "research" / "daily_notes.md"
            if notes_file.exists():
                with open(notes_file, 'r', encoding='utf-8') as f:
                    content = f.read()
                    # 마지막 날짜 섹션 찾기
                    lines = content.split('\n')
                    for i in range(len(lines)-1, -1, -1):
                        if lines[i].startswith('## 2025-'):
                            # 해당 섹션의 주요 내용 추출
                            return self.extract_section_summary(lines, i)
            return "No previous work recorded"
        except:
            return "Could not load yesterday's summary"
    
    def extract_section_summary(self, lines, start_idx):
        """섹션에서 주요 내용 추출"""
        summary = []
        for i in range(start_idx + 1, len(lines)):
            if lines[i].startswith('## '):  # 다음 섹션
                break
            if lines[i].strip() and not lines[i].startswith('#'):
                summary.append(lines[i].strip())
                if len(summary) >= 3:  # 처음 3줄만
                    break
        return ' '.join(summary)
    
    def suggest_today_focus(self):
        """오늘의 추천 작업"""
        day = self.get_project_day()
        
        # 요일별 기본 포커스
        weekday = datetime.now().weekday()
        focus_areas = {
            0: "Core feature development",  # 월
            1: "Blender scripting",         # 화
            2: "Unity VR implementation",   # 수
            3: "Integration & testing",     # 목
            4: "Documentation & review",    # 금
            5: "Experimentation",          # 토
            6: "Planning & research"       # 일
        }
        
        return focus_areas.get(weekday, "General development")
    
    def detect_blockers(self):
        """현재 블로커 감지"""
        blockers = []
        
        # TODO 파일 체크
        todo_file = self.project_root / "TODO.md"
        if todo_file.exists():
            with open(todo_file, 'r', encoding='utf-8') as f:
                content = f.read()
                if "BLOCKER" in content or "CRITICAL" in content:
                    blockers.append("Critical issues in TODO.md")
        
        # 최근 커밋 메시지에서 문제 감지
        try:
            recent_commits = subprocess.check_output(
                ["git", "log", "-5", "--pretty=format:%s"],
                text=True
            ).lower()
            if "fix" in recent_commits or "bug" in recent_commits:
                blockers.append("Recent bug fixes needed")
        except:
            pass
        
        return blockers if blockers else ["No blockers detected"]
    
    def update_project_master(self, data):
        """project_master.md 업데이트"""
        template = f"""# TEMPORAL VR PROJECT MASTER
Last Updated: {datetime.now().strftime('%Y-%m-%d')} (Day {data['current_day']})

## 🎯 Project Overview
**Goal**: Create a VR system where users can model 3D objects by manipulating time as a 4th dimension
**Target**: SIGGRAPH Asia 2026 Paper Submission
**Duration**: 11 months (June 2025 - May 2026)

## 📊 Current Status
- Phase: Day {data['current_day']} - Development
- Days to Deadline: {330 - data['current_day']}
- Progress: {data['progress']:.1f}%

## 🛠️ Tech Stack
- **Blender 4.4**: Procedural modeling backend
- **Unity 2023.3 LTS**: VR frontend
- **OpenXR**: Cross-platform VR support
- **Cursor AI**: Development assistant

## 📈 Recent Activity
- Last Commit: {data['last_commit']}
- Modified Files: {len(data['modified_files'])}
- Active Development: {', '.join(data['modified_files'][:3]) if data['modified_files'] else 'No pending changes'}

## 🎯 Current Focus
- Primary: Temporal modeling system implementation
- Secondary: VR interaction design
- Research: Time visualization methods

## ⚠️ Active Issues
{chr(10).join(f'- {blocker}' for blocker in self.detect_blockers())}

## 🔗 Quick Links
- Research Notes: `research/daily_notes.md`
- Current Scripts: `blender/scripts/`
- Unity Project: `unity/TemporalVR/`
- Knowledge Base: `knowledge/solutions.md`

## 📁 Project Structure
```
C:\\Research\\TemporalVR_Project
├── automation/          # Automation scripts
├── blender/            # Blender development
├── unity/              # Unity VR project  
├── knowledge/          # Knowledge base
└── research/           # Research notes
```
"""
        
        # project_master.md가 있는지 확인
        project_master_path = self.project_root / 'project_master.md'
        
        with open(project_master_path, 'w', encoding='utf-8') as f:
            f.write(template)
        
        print(f"✅ Updated project_master.md (Day {data['current_day']})")
    
    def create_daily_context(self, data):
        """daily_context.md 생성"""
        template = f"""# Daily Context - {datetime.now().strftime('%Y-%m-%d')} (Day {self.get_project_day()})

## 🎯 Today's Focus
{data['today_focus']}

## 📊 Starting Point
### Yesterday's Summary
{data['yesterday']}

### Git Status
- Modified files: {len(self.get_git_status()['modified'])}
- Recent activity: {', '.join(data['hot_files'][:3]) if data['hot_files'] else 'No recent files'}

## 🔥 Hot Files (Recently Active)
{chr(10).join(f'- `{file}`' for file in data['hot_files']) if data['hot_files'] else '- No recent file activity'}

## ⚠️ Current Blockers
{chr(10).join(f'- {blocker}' for blocker in data['blockers'])}

## 📋 Today's Checklist
- [ ] Review yesterday's work
- [ ] Continue {data['today_focus'].lower()}
- [ ] Test in VR if applicable
- [ ] Update research notes
- [ ] Commit and push changes

## 💡 Quick References
- Blender API: https://docs.blender.org/api/current/
- Unity XR: https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@2.5/manual/
- Project Knowledge: `knowledge/solutions.md`

## 🎮 Cursor AI Context
Working on Temporal VR project - Day {self.get_project_day()}
Check .cursorrules for project standards
Reference knowledge/solutions.md for past solutions

## 📁 Current Project Structure
- Blender scripts: `blender/scripts/`
- Unity project: `unity/TemporalVR/`
- Research notes: `research/daily_notes.md`
- Knowledge base: `knowledge/solutions.md`

## 🎯 Project Vision Reminder
Creating a VR system where artists can sculpt time itself.
Today's work contributes to: [specific aspect]


"""
        
        # daily_context.md 경로
        daily_context_path = self.project_root / 'daily_context.md'
        
        with open(daily_context_path, 'w', encoding='utf-8') as f:
            f.write(template)
        
        print(f"✅ Created daily_context.md for Day {self.get_project_day()}")
    
    def update_all_contexts(self):
        """모든 컨텍스트 업데이트 메인 함수"""
        print(f"\n🚀 Updating Temporal VR contexts - Day {self.get_project_day()}")
        print("=" * 50)
        
        # 필요한 폴더들이 있는지 확인
        required_folders = [
            'automation',
            'blender/scripts',
            'blender/assets/models',
            'unity',
            'knowledge',
            'research'
        ]
        
        for folder in required_folders:
            folder_path = self.project_root / folder
            if not folder_path.exists():
                folder_path.mkdir(parents=True, exist_ok=True)
                print(f"📁 Created missing folder: {folder}")
        
        # Git 상태 확인
        git_status = self.get_git_status()
        
        # 어제 작업 로드
        yesterday_work = self.load_yesterday_summary()
        
        # project_master.md 업데이트
        self.update_project_master({
            "current_day": self.get_project_day(),
            "last_commit": git_status["last_commit"],
            "modified_files": git_status["modified"],
            "progress": self.calculate_progress()
        })
        
        # daily_context.md 생성
        self.create_daily_context({
            "yesterday": yesterday_work,
            "today_focus": self.suggest_today_focus(),
            "hot_files": git_status["recently_edited"],
            "blockers": self.detect_blockers()
        })
        
        print("\n✨ All contexts updated successfully!")
        print(f"📅 Today is Day {self.get_project_day()} of your Temporal VR journey")
        print(f"📊 Overall progress: {self.calculate_progress():.1f}%")
        print("\n💡 Next: Open daily_context.md to see today's focus")

if __name__ == "__main__":
    manager = ContextManager()
    manager.update_all_contexts()