# Temporal VR - 11개월 연속성 보장 루틴 (개선판)

## 🎯 **핵심 전략: "Context + Vision Persistence System"**

Claude는 매 대화마다 리셋되므로, **프로젝트 비전 + 체계적인 컨텍스트 전달**로 연속성을 만듭니다.

## 📅 **일일 루틴 (Daily Routine)**

### **🌅 아침 시작 (5분)**

#### 1. **프로젝트 상태 업데이트**
```bash
# morning_start.bat 실행
cd C:\Research\TemporalVR_Project
git pull
python automation\update_contexts.py
```

#### 2. **Claude 세션 시작 - 강화된 템플릿**
```markdown
# 이 내용을 복사해서 Claude에게 전달:

안녕하세요! Temporal VR 프로젝트 Day [X] 입니다.

## 🎯 프로젝트 비전
"시간을 4차원 공간으로 조작하는 VR 모델링 시스템"
- SIGGRAPH Asia 2026 목표
- 핵심: Temporal Brush로 시간을 "칠하는" 인터페이스

[project_master.md 전체 내용 붙여넣기]

[research/project_vision.md의 핵심 개념 요약]

오늘의 작업:
[daily_context.md의 Today's Focus 섹션]

이전 세션 요약:
[research/daily_notes.md의 어제 기록 마지막 부분]

현재 집중 영역: [RQ1/RQ2/RQ3 중 해당하는 것]
```

### **💻 작업 중 (매 시간) - 연구 중심**

#### **연구 진행 체크포인트**
```markdown
# 1시간마다 Claude에게 전달:

현재 진행 상황 업데이트:
- 완료: [작업 내용]
- 진행중: [현재 파일:라인]
- 실험 결과: [있다면 데이터]
- 발견: [새로운 인사이트 - 논문에 쓸 수 있는 것]
- 문제: [막힌 부분]

연구 질문 관련:
- 이 작업이 RQ[X]에 어떻게 기여하는가?
- 예상되는 논문 섹션: [Introduction/Related Work/Method/Results]

다음 단계 도움 필요: [구체적 요청]
```

### **🌙 저녁 마무리 (15분) - 강화된 기록**

#### 1. **연구 중심 세션 종료**
```python
# automation/end_session.py - 개선된 버전
def save_session_summary():
    summary = {
        "date": str(datetime.date.today()),
        "research_focus": input("오늘의 연구 초점 (RQ1/RQ2/RQ3): "),
        "completed": input("완료한 작업들: ").split(","),
        "technical_progress": input("기술적 진전: "),
        "insights": input("논문에 쓸 중요 발견: "),
        "experiments": input("실험 결과 (있다면): "),
        "issues": input("미해결 이슈: "),
        "next_steps": input("내일 할 일: ").split(","),
        "paper_notes": input("논문 아이디어 (선택): ")
    }
    
    # 1. daily_notes.md 업데이트
    update_research_notes(summary)
    
    # 2. experiments/ 폴더에 실험 결과 저장
    if summary["experiments"]:
        save_experiment_data(summary)
    
    # 3. paper_drafts/ 폴더에 논문 노트 저장
    if summary["paper_notes"]:
        update_paper_drafts(summary)
```

## 📆 **주간 루틴 (Weekly Routine) - 연구 마일스톤**

### **월요일: 주간 연구 계획**
```markdown
# research/weekly_summary.md 작성

## Week [X] Research Planning (날짜)

### 연구 진행 상황
- RQ1 진행도: [X]%
- RQ2 진행도: [X]%  
- RQ3 진행도: [X]%

### 이번주 연구 목표
1. [어떤 RQ에 집중?]
2. [어떤 실험 진행?]
3. [어떤 프로토타입 개발?]

### 논문 섹션 진행
- [ ] Introduction: [상태]
- [ ] Related Work: [상태]
- [ ] Method: [상태]
- [ ] Implementation: [상태]
- [ ] Results: [상태]
```

### **금요일: 주간 연구 성과 정리**
```markdown
## Week [X] 성과 요약

### 📊 정량적 성과
- 코드 라인: [X]
- 실험 횟수: [X]
- 수집 데이터: [X]

### 📝 정성적 성과
- 주요 발견: [논문에 쓸 내용]
- 해결한 문제: [기술적 기여]
- 새로운 아이디어: [향후 연구]

### 📸 시각적 성과
- 스크린샷/비디오: [파일명]
- 다이어그램: [Figure X 후보]
```

## 🔄 **강화된 연속성 시스템**

### **1. 연구 비전 중심 파일 체계**

```
📄 project_master.md          # 전체 상태
📄 daily_context.md           # 일일 작업
📄 research/project_vision.md # 🆕 연구 비전 (고정)
📄 research/daily_notes.md    # 연구 일지
📄 research/paper_drafts/     # 🆕 논문 초안
📄 knowledge/solutions.md     # 기술 해결책
📄 knowledge/core_concepts.md # 🆕 핵심 개념
```

### **2. 월별 포커스 시스템**

```python
# automation/monthly_focus.py
MONTHLY_FOCUS = {
    1: "Foundation & Prototype",
    2: "Core System Development", 
    3: "Advanced Features",
    4: "Performance Optimization",
    5: "User Study Design",
    6: "User Study Execution",
    7: "Data Analysis",
    8: "Paper Writing - Method",
    9: "Paper Writing - Results", 
    10: "Paper Revision & Video",
    11: "Final Submission"
}

def get_current_focus():
    month = get_project_month()
    return MONTHLY_FOCUS[month]
```

### **3. 논문 진행 추적**

```markdown
# research/paper_progress.md

## Paper Sections Status

### Abstract (0/150 words)
- [ ] Problem statement
- [ ] Method summary
- [ ] Key results
- [ ] Contributions

### Introduction (0/2 pages)
- [ ] Motivation
- [ ] Problem definition
- [ ] Contributions list
- [ ] Paper overview

### Related Work (0/2 pages)
- [ ] VR modeling systems
- [ ] Temporal interfaces
- [ ] 4D visualization
- [ ] Procedural modeling

[...]
```

## 📊 **개선된 체크리스트**

### **매일 체크**
- [ ] project_vision.md 다시 읽기 (비전 상기)
- [ ] 오늘 작업이 어떤 RQ에 기여하는지 명확히
- [ ] 실험 결과 기록
- [ ] 논문에 쓸 만한 발견 메모
- [ ] Git 커밋 (의미 있는 메시지로)

### **매주 체크**  
- [ ] 주간 연구 성과 정리
- [ ] 논문 진행도 업데이트
- [ ] 다음 주 실험 계획
- [ ] 지도교수/동료 피드백 (있다면)

### **매월 체크**
- [ ] 월별 마일스톤 달성 여부
- [ ] 논문 섹션 초안 작성
- [ ] 전체 진행률 재평가
- [ ] SIGGRAPH 마감일까지 남은 시간 체크

## 🚀 **성공을 위한 추가 팁**

### **1. 매일 "Why" 질문하기**
```markdown
- 이 기능이 왜 필요한가?
- 이것이 논문의 novelty에 어떻게 기여하는가?
- 리뷰어가 이것을 보고 뭐라고 할까?
```

### **2. 실패도 기록하기**
```markdown
# knowledge/failed_attempts.md
실패한 시도들도 논문의 "Design Space Exploration"에 
중요한 내용이 될 수 있습니다.
```

### **3. 경쟁 논문 추적**
```markdown
# research/related_papers.md
최신 VR/Graphics 논문들을 계속 추적하여
우리 연구의 차별점을 명확히 합니다.
```

이렇게 수정하면 11개월간의 SIGGRAPH 여정을 체계적으로 관리할 수 있습니다! 🎯