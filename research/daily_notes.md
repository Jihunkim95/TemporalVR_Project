 # 📝 매일 누적되는 연구 일지

## 2025-06-26 (목) - Day 1: Project Inception

### 🎯 오늘의 목표
- [x] 프로젝트 구조 설정
- [x] Cursor AI 환경 구성
- [x] 첫 temporal 스크립트 작성
- [x] Git 저장소 초기화

### 💡 핵심 아이디어
"시간을 4차원으로 조작하는 VR 모델링 시스템"
## 2025-06-27 (Fri) - Day 2: TemporalVRController.cs 작성 및 스크립트 테스트. 앞으로도 잘 되어야될텐데 화이팅이다.

### 🎯 Completed Tasks
- None

### 🔧 Technical Progress


### 💡 Research Insights


### ⚠️ Issues & Blockers


### 📊 Statistics
- Git commits today: 3
- Files modified: 3
- Progress: 6.9%

### 🎯 Next Steps
- VR컨트롤러 입력 시스템 설계계: TemporalVRController.cs 작성 스크립트 테스트



---

## 2025-06-30 (Mon) - Day 5: VR 컨트롤러 Interaction구현

### 🎯 Completed Tasks
- Visual Feedback 시스템 구축
- 테스트 오브젝트 구현: TObject
- Visual Feedback 자동생성: TVR_Setup

### 🔧 Technical Progress


### 💡 Research Insights


### ⚠️ Issues & Blockers


### 📊 Statistics
- Git commits today: 2
- Files modified: 0
- Progress: 8.7%

### 🎯 Next Steps
- VR Controller와 원 Object가 상호작용하는지 테스트



---

## 2025-07-01 (Tue) - Day 6: TemporalVR 4가지 모드 구현 (Scrub, Paint, Sculpt, Preview)

### 🎯 Completed Tasks
- 주요 성과

### 🔧 Technical Progress
VR 컨트롤러 모드 전환 시스템 완성

### 💡 Research Insights


### ⚠️ Issues & Blockers
4가지 모드 구현 (Scrub, Paint, Sculpt, Preview)

### 📊 Statistics
- Git commits today: 1
- Files modified: 3
- Progress: 9.0%

### 🎯 Next Steps
- G키로 모드 순환
- 모드별 색상 피드백 (파란색/초록색/빨간색/노란색)



---

## 2025-07-01 (Tue) - Day 6: 4가지 모드 구현 (Scrub, Paint, Sculpt, Preview)

### 🎯 Completed Tasks
- VR 컨트롤러 모드 전환 시스템 완성
- 4가지 모드 구현 (Scrub, Paint, Sculpt, Preview)
- G키로 모드 순환
- 모드별 색상 피드백 (파란색/초록색/빨간색/노란색)

### 🔧 Technical Progress


### 💡 Research Insights


### ⚠️ Issues & Blockers


### 📊 Statistics
- Git commits today: 2
- Files modified: 0
- Progress: 9.0%

### 🎯 Next Steps
- None



---

## 2025-07-02 (Wed) - Day 7: 6일차 복습

### 🎯 Completed Tasks
- None

### 🔧 Technical Progress


### 💡 Research Insights


### ⚠️ Issues & Blockers


### 📊 Statistics
- Git commits today: 1
- Files modified: 0
- Progress: 9.3%

### 🎯 Next Steps
- None



---

## 2025-07-03 (Thu) - Day 8: TMorphObj,TKeyframe,TMorphData, TMorphTest 만들었지만, 테스트중 오류가 떠서 내일 다시해야함

### 🎯 Completed Tasks
- None

### 🔧 Technical Progress


### 💡 Research Insights


### ⚠️ Issues & Blockers


### 📊 Statistics
- Git commits today: 2
- Files modified: 0
- Progress: 17.4%

### 🎯 Next Steps
- None



---

## 2025-07-04 (Fri) - Day 9: successfully morphed time! Next: Make it work in VR

### 🎯 Completed Tasks
- None

### 🔧 Technical Progress


### 💡 Research Insights


### ⚠️ Issues & Blockers


### 📊 Statistics
- Git commits today: 1
- Files modified: 2
- Progress: 17.7%

### 🎯 Next Steps
- None



---

## 2025-07-07 (Mon) - Day 12: TMorphTest로 4가지 시간 변형(Wave/Sphere/Star/Flower) 구현 성공했으나, PerformanceMonitor와의 충돌로 Morph 동작이 중단되는 치명적 버그 발견

### 🎯 Completed Tasks
- None

### 🔧 Technical Progress


### 💡 Research Insights


### ⚠️ Issues & Blockers


### 📊 Statistics
- Git commits today: 1
- Files modified: 0
- Progress: 18.6%

### 🎯 Next Steps
- None



---

## 2025-07-08 (Tue) - Day 13: PerformanceMonitor 버그 수정 및 Temporal Brush 시스템 구현

### 🎯 Completed Tasks
- PerformanceMonitor와 TMorphObj 간의 충돌 버그 해결
- TMorphTest 색상 애니메이션 문제 수정 (material.color 직접 설정)
- TemporalBrushData.cs 구현 완료
- TMorphObj에 Temporal Brush 지원 메서드 추가
- TVRFeedback의 ShowBrushImpact 애니메이션 개선

### 🔧 Technical Progress
Sprites/Default 셰이더가 MaterialPropertyBlock을 지원하지 않아서 색상이 적용되지 않는 문제를 발견. meshRenderer.material.color = timeColor 직접 설정으로 해결. PerformanceMonitor의 통계 업데이트 주기를 5초로 늘려 성능 향상.

### 💡 Research Insights
Temporal Brush의 핵심은 "시간을 칠하는" 인터페이스. 현재는 전체 객체의 시간만 변경 가능하지만, Vertex 단위 시간 제어를 위한 TemporalMeshData 구조 설계 완료. 이는 SIGGRAPH 논문의 주요 contribution이 될 예정.

### ⚠️ Issues & Blockers
Paint Mode에서 불필요한 Sphere 생성 문제 (Particle 제거로 해결). 현재 구조(TemporalObject → TMorphObj → TMorphTest)가 복잡하여 향후 리팩토링 필요.

### 📊 Statistics
- Git commits today: 4
- Files modified: 3
- Progress: 18.9%

### 🎯 Next Steps
- 현재 구조(TemporalObject → TMorphObj → TMorphTest)가 복잡하여 향후 리팩토링 필요.
- Growing Tree 데모 씬 생성
- VR 컨트롤러로 성장 속도 조절 구현

### 💭 Reflection
오늘은 큰 버그들을 해결하고 Temporal Brush의 기초를 구현한 생산적인 날이었다. 특히 Vertex 단위 시간 제어를 위한 TMorphObj_V2 설계는 프로젝트의 핵심 혁신이 될 것 같다. 내일은 Week 2를 마무리하고 실제로 "시간을 칠하는" 데모구현

---
