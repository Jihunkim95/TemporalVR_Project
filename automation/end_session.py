import os
import json
import subprocess
from datetime import datetime
from pathlib import Path

class SessionManager:
    def __init__(self):
        self.project_root = Path.cwd()
        self.session_data = {}
        
    def get_git_changes(self):
        """Git 변경사항 확인"""
        try:
            # 수정된 파일 목록
            modified = subprocess.check_output(
                ["git", "diff", "--name-only"],
                text=True
            ).strip().split('\n')
            
            # 추가된 파일 목록
            added = subprocess.check_output(
                ["git", "ls-files", "--others", "--exclude-standard"],
                text=True
            ).strip().split('\n')
            
            return {
                "modified": [f for f in modified if f],
                "added": [f for f in added if f]
            }
        except:
            return {"modified": [], "added": []}
    
    def update_research_notes(self, summary):
        """research/daily_notes.md 업데이트"""
        notes_file = self.project_root / "research" / "daily_notes.md"
        
        # 기존 내용 읽기
        if notes_file.exists():
            with open(notes_file, 'r', encoding='utf-8') as f:
                existing_content = f.read()
        else:
            existing_content = "# Temporal VR Research Notebook\n\n"
        
        # 오늘의 노트 추가
        today_note = f"""
## {datetime.now().strftime('%Y-%m-%d (%a)')} - Day {self.get_project_day()}: {summary.get('title', 'Development')}

### 🎯 오늘의 목표
{self.format_list(summary.get('goals', ['No goals recorded']))}

### ✅ 완료한 작업
{self.format_list(summary['completed'])}

### 💡 중요 발견/인사이트
{summary['insights'] if summary['insights'] else '- 특별한 발견사항 없음'}

### ⚠️ 미해결 이슈
{summary['issues'] if summary['issues'] else '- 모든 이슈 해결됨'}

### 📊 기술적 진전
- 수정된 파일: {len(self.get_git_changes()['modified'])}개
- 추가된 파일: {len(self.get_git_changes()['added'])}개
- 주요 변경사항: {', '.join(self.get_git_changes()['modified'][:3])}

### 🎯 내일 계획
{self.format_list(summary['next_steps'])}

### 💭 오늘의 회고
{summary.get('reflection', '하루를 마무리하며...')}

---
"""
        
        # 파일에 추가
        with open(notes_file, 'a', encoding='utf-8') as f:
            f.write(today_note)
        
        print("✅ Research notes updated")
    
    def format_list(self, items):
        """리스트를 마크다운 형식으로 변환"""
        if isinstance(items, str):
            items = [item.strip() for item in items.split(',') if item.strip()]
        return '\n'.join(f'- {item}' for item in items if item)
    
    def get_project_day(self):
        """프로젝트 진행 일수"""
        start_date = datetime(2025, 6, 26)
        delta = datetime.now() - start_date
        return delta.days + 1
    
    def add_to_knowledge_base(self, insights):
        """knowledge/solutions.md에 인사이트 추가"""
        kb_file = self.project_root / "knowledge" / "solutions.md"
        
        if not kb_file.exists():
            # 파일이 없으면 생성
            kb_file.parent.mkdir(exist_ok=True)
            with open(kb_file, 'w', encoding='utf-8') as f:
                f.write("# Temporal VR - Knowledge Base\n\n")
        
        # 인사이트 추가
        with open(kb_file, 'a', encoding='utf-8') as f:
            f.write(f"\n## {datetime.now().strftime('%Y-%m-%d')}: {insights}\n")
            f.write(f"- Context: Day {self.get_project_day()}\n")
            f.write(f"- Details: [Add more details here]\n\n")
        
        print("✅ Knowledge base updated")
    
    def prepare_tomorrow_context(self, next_steps):
        """내일을 위한 준비"""
        tomorrow_file = self.project_root / "automation" / "tomorrow_plan.json"
        
        tomorrow_data = {
            "date": (datetime.now() + timedelta(days=1)).strftime('%Y-%m-%d'),
            "planned_tasks": next_steps,
            "carry_over_issues": self.session_data.get('issues', ''),
            "focus_files": self.get_git_changes()['modified'][:5]
        }
        
        tomorrow_file.parent.mkdir(exist_ok=True)
        with open(tomorrow_file, 'w', encoding='utf-8') as f:
            json.dump(tomorrow_data, f, indent=2)
        
        print("✅ Tomorrow's context prepared")
    
    def collect_session_summary(self):
        """사용자로부터 세션 요약 수집"""
        print("\n📝 Session End Summary")
        print("=" * 50)
        
        summary = {
            "date": datetime.now().strftime('%Y-%m-%d'),
            "title": input("\n오늘 작업 한 줄 요약: ").strip()
        }
        
        print("\n✅ 완료한 작업들 (쉼표로 구분):")
        summary["completed"] = input("> ").strip()
        
        print("\n💡 중요한 발견/인사이트:")
        summary["insights"] = input("> ").strip()
        
        print("\n⚠️  미해결 이슈:")
        summary["issues"] = input("> ").strip()
        
        print("\n🎯 내일 할 일 (쉼표로 구분):")
        summary["next_steps"] = input("> ").strip()
        
        print("\n💭 오늘의 간단한 회고 (선택사항):")
        reflection = input("> ").strip()
        if reflection:
            summary["reflection"] = reflection
        
        return summary
    
    def create_git_commit(self, summary):
        """Git 커밋 생성"""
        try:
            # 모든 변경사항 스테이징
            subprocess.run(["git", "add", "-A"])
            
            # 커밋 메시지 생성
            commit_msg = f"Day {self.get_project_day()}: {summary['title']}"
            
            # 커밋
            subprocess.run(["git", "commit", "-m", commit_msg])
            
            print(f"\n✅ Git commit created: {commit_msg}")
            
            # Push 여부 확인
            push = input("\nPush to remote? (y/n): ").lower()
            if push == 'y':
                subprocess.run(["git", "push"])
                print("✅ Pushed to remote repository")
                
        except Exception as e:
            print(f"⚠️  Git operations failed: {e}")
    
    def save_session_summary(self):
        """전체 세션 종료 프로세스"""
        print("\n🌙 Temporal VR - End of Day Session")
        print(f"📅 Day {self.get_project_day()} Complete")
        print("=" * 50)
        
        # 세션 요약 수집
        summary = self.collect_session_summary()
        
        print("\n\n🔄 Processing...")
        
        # 1. Research notes 업데이트
        self.update_research_notes(summary)
        
        # 2. Knowledge base 업데이트 (인사이트가 있으면)
        if summary['insights']:
            self.add_to_knowledge_base(summary['insights'])
        
        # 3. 내일 준비
        if summary['next_steps']:
            self.prepare_tomorrow_context(summary['next_steps'])
        
        # 4. Git 커밋
        self.create_git_commit(summary)
        
        # 5. 최종 통계
        print("\n📊 Today's Statistics")
        print("=" * 50)
        changes = self.get_git_changes()
        print(f"- Files modified: {len(changes['modified'])}")
        print(f"- Files added: {len(changes['added'])}")
        print(f"- Project progress: Day {self.get_project_day()}/330")
        
        print("\n✨ Great work today! See you tomorrow!")
        print("💡 Run 'python automation\\morning_start.bat' tomorrow morning")

def main():
    """메인 실행 함수"""
    manager = SessionManager()
    manager.save_session_summary()

if __name__ == "__main__":
    main()