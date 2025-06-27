# Temporal VR - 11개월 연속성 보장 루틴 (현재 구조 반영)

## 🎯 **핵심 전략: "Context + Vision Persistence System"**

Claude는 매 대화마다 리셋되므로, **프로젝트 비전 + 체계적인 컨텍스트 전달**로 연속성을 만듭니다.

## 📅 **일일 루틴 (Daily Routine)**

### **🌅 아침 시작 (5분)**

#### 1. **프로젝트 상태 업데이트**
```bash
# morning_start.bat 실행
cd C:\Research\TemporalVR_Project
git pull
python automation\temporal_vr_automation.py morning
```

#### 2. **Claude 세션 시작 템플릿**
```markdown
# 이 내용을 복사해서 Claude에게 전달:

안녕하세요! Temporal VR 프로젝트 Day [X] 입니다.

## 🎯 프로젝트 비전
"시간을 4차원 공간으로 조작하는 VR 모델링 시스템"
- SIGGRAPH Asia 2026 목표
- 핵심: Temporal Brush로 시간을 "칠하는" 인터페이스

[project_master.md 전체 내용 붙여넣기]

오늘의 작업:
[daily_context.md의 Today's Focus 섹션]

이전 세션 요약:
[research/daily_notes.md의 어제 기록 마지막 부분]

현재 집중 영역: [현재 Phase 및 RQ]
```

### **💻 작업 중 (필요시) - 컨텍스트 업데이트**

#### **중간 업데이트 (선택사항)**
```bash
# 중요한 진전이 있을 때
git add .
git commit -m "구현 내용"
python automation\temporal_vr_automation.py update
```

#### **Claude와 진행 상황 공유**
```markdown
# Claude에게 전달:

현재 진행 상황 업데이트:
- 완료: [작업 내용]
- 진행중: [현재 파일:라인]
- 발견: [새로운 인사이트]
- 문제: [막힌 부분]

[업데이트된 project_master.md 중요 부분]

다음 단계 도움 필요: [구체적 요청]
```

### **🌙 저녁 마무리 (10분)**

#### 1. **세션 종료**
```bash
python automation\temporal_vr_automation.py evening
```

#### 2. **입력할 내용들**
```
📝 Daily Session Summary
========================
오늘 작업 한 줄 요약: [예: Unity VR 핸드 트래킹 구현]

✅ What did you complete today?
- [완료 항목 1]
- [완료 항목 2]
(빈 줄로 종료)

🔧 Technical progress/discoveries:
[기술적 진전 설명]

💡 Research insights (for paper):
[논문에 쓸 만한 발견]

⚠️ Issues or blockers:
[문제점이나 막힌 부분]

🎯 Tomorrow's plan:
- [내일 할 일 1]
- [내일 할 일 2]
(빈 줄로 종료)

💭 Reflection (optional):
[오늘의 회고]
```

## 📁 **핵심 파일 관리**

### **1. 자동 생성 파일 (읽기 전용)**
```
📄 project_master.md     # 전체 프로젝트 상태 (자동 업데이트)
📄 daily_context.md      # 오늘의 작업 컨텍스트 (매일 재생성)
```

### **2. 수동 작성 파일 (직접 관리)**
```
📄 research/daily_notes.md    # 연구 일지 (evening 명령으로 자동 추가)
📄 knowledge/solutions.md     # 문제 해결책 (수동 추가)
📄 TODO.md                    # 할 일 목록 (필요시 생성)
📄 research/ideas_backlog.md  # 아이디어 저장 (수동 추가)
```

## 📆 **주간 루틴**

### **월요일: 주간 계획**
```markdown
# research/weekly_summary.md에 추가

## Week [X] Planning - [날짜]

### 지난주 성과
- 주요 완성 기능: [목록]
- 해결한 문제: [목록]
- 수집한 데이터: [있다면]

### 이번주 목표
1. [구체적 개발 목표]
2. [연구/실험 목표]
3. [문서화 목표]

### 예상 난관
- [기술적 도전]
- [시간 제약]
```

### **금요일: 주간 정리**
```bash
# 주간 통계 확인
python automation\temporal_vr_automation.py status

# Git 로그 확인
git log --oneline --since="last monday"

# 다음주 계획 수립
```

## 🔄 **월별 연구 포커스**

### **현재 설계된 진행 단계**
```python
# temporal_vr_automation.py의 _get_current_phase() 참고
Day 1-14:     Foundation & Setup
Day 15-60:    Core Development  
Day 61-120:   Feature Implementation
Day 121-180:  Testing & Optimization
Day 181-240:  User Studies
Day 241-300:  Paper Writing
Day 301-330:  Final Preparation
```

### **연구 질문 로테이션**
```python
# 3주 단위로 RQ 포커스 변경
Week 1-3:   RQ1 - Time dimension representation
Week 4-6:   RQ2 - Interaction paradigms  
Week 7-9:   RQ3 - Creative workflows
(반복)
```

## 📊 **진행 상황 추적**

### **일일 체크**
```bash
# 아침
python automation\temporal_vr_automation.py morning
→ daily_context.md 확인

# 저녁  
python automation\temporal_vr_automation.py evening
→ research/daily_notes.md 자동 업데이트
```

### **주간 체크**
- [ ] 주간 커밋 수 확인
- [ ] 진행률 변화 체크
- [ ] 블로커 해결 여부
- [ ] 다음주 포커스 결정

### **월간 체크**
- [ ] 전체 진행률 평가
- [ ] 논문 섹션 진행도
- [ ] 마일스톤 달성 여부
- [ ] 남은 기간 재평가

## 💡 **효과적인 활용 팁**

### **1. TODO.md 활용**
```markdown
# TODO.md 예시

## CRITICAL
- Unity XR 플러그인 호환성 문제 해결

## High Priority  
- [ ] Temporal brush 제스처 구현
- [ ] Blender 메모리 최적화

## Nice to Have
- [ ] 추가 시각 효과
```

### **2. knowledge/solutions.md 즉시 기록**
```markdown
## 2025-06-27: Unity XR Setup Issue
Problem: OpenXR loader 초기화 실패
Solution: Project Settings > XR Plug-in Management에서 
         Initialize XR on Startup 체크
Code: `XRGeneralSettings.Instance.Manager.InitializeLoaderSync()`
```

### **3. 실험 결과 별도 관리**
```markdown
# research/experiments/exp_001_temporal_brush.md

## Experiment: Temporal Brush Gesture Test
Date: 2025-07-15
Participants: 5

### Setup
- VR Device: Quest 3
- Unity Version: 2023.3 LTS
- Test Duration: 30 min/person

### Results
| Gesture | Success Rate | Avg Time |
|---------|--------------|----------|
| Pinch   | 85%          | 2.3s     |
| Grab    | 92%          | 1.8s     |

### Insights
- Grab gesture more intuitive
- Need haptic feedback
```

## 🚀 **빠른 시작 체크리스트**

### **매일 아침**
1. `morning_start.bat` 실행
2. `daily_context.md` 확인
3. Claude에게 컨텍스트 전달
4. 작업 시작

### **매일 저녁**
1. `python automation\temporal_vr_automation.py evening` 실행
2. 오늘의 성과 입력
3. Git push (선택)
4. 내일 계획 확인

### **문제 발생 시**
1. `TODO.md`에 BLOCKER 추가
2. `python automation\temporal_vr_automation.py update`
3. `knowledge/solutions.md`에 해결책 기록

## 🎯 **성공의 열쇠**

1. **일관성**: 매일 같은 루틴 유지
2. **즉시성**: 발견/문제를 바로 기록
3. **구체성**: 모호한 표현 피하기
4. **연속성**: 어제-오늘-내일 연결

이 루틴을 따르면 11개월 후 SIGGRAPH에 제출할 완성도 높은 논문과 시스템을 갖게 될 것입니다! 🏆