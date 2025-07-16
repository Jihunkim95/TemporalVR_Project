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
- Unity스크립트구조.md
- 현재 구조(TemporalObject → TMorphObj → TMorphTest)가 복잡하여 향후 리팩토링 필요.
- Growing Tree 데모 씬 생성
- VR 컨트롤러로 성장 속도 조절 구현

### 💭 Reflection
오늘은 큰 버그들을 해결하고 Temporal Brush의 기초를 구현한 생산적인 날이었다. 특히 Vertex 단위 시간 제어를 위한 TMorphObj_V2 설계는 프로젝트의 핵심 혁신이 될 것 같다. 내일은 Week 2를 마무리하고 실제로 "시간을 칠하는" 데모구현

---

## 2025-07-09 (Wed) - Day 14: TMorphObj_V2와 Temporal Brush 연동 완성 및 vertex color 시각화 구현

### 🎯 Completed Tasks
- TMorphObj_V2와 Temporal Brush 완전한 연동 구현
- TVR_Controller의 Paint Mode에서 Ray-based hit detection 추가
- TemporalEventSystem.cs 전체 구현 (누락되었던 파일)
- Vertex Color Material 변환 시스템 구축
- TemporalEventSystem.cs 전체 구현 (누락되었던 파일)

### 🔧 Technical Progress
Standard Shader는 vertex color를 지원하지 않아 Sprites/Default나 Unlit/VertexColor로 변경 필요. TMorphObj_V2의 TKeyframe.color는 사용되지 않고, Time Gradient만 vertex color 결정에 사용됨. Ray-based 브러시 위치 계산으로 정확한 표면 타겟팅 가능.

### 💡 Research Insights
Vertex별 개별 시간 제어를 통한 부분적 temporal manipulation 성공. 시간을 공간에 "칠하는" 인터페이스의 직관성 확인. VR에서 4차원(시간) 조작의 실용적 구현 가능성 입증.

### ⚠️ Issues & Blockers
초기에 브러시 이펙트가 컨트롤러 위치에 생성되는 문제 → hit.point 사용으로 해결. Material이 vertex color를 표시하지 않는 문제 → Shader 변경으로 해결.

### 📊 Statistics
- Git commits today: 2
- Files modified: 4
- Progress: 19.2%

### 🎯 Next Steps
- 더 복잡한 mesh (Sphere, subdivided plane)에서 temporal painting 테스트
- Brush 모드별 차별화 구현 (Absolute, Relative, Smooth, Ripple)
- 성능 최적화 및 LOD 시스템 검토
- Phase 2 준비: 프로시저럴 성장 알고리즘 설계

### 💭 Reflection
오늘 드디어 "시간을 칠하는" 핵심 기능이 실제로 작동하는 것을 확인했습니다. Vertex color로 시간의 흐름이 시각화되고, mesh가 실제로 변형되는 것을 보니 SIGGRAPH 논문의 핵심 아이디어가 구현 가능함을 확신하게 되었습니다. Phase 1이 성공적으로 마무리되고 있습니다!

---

## 2025-07-10 (Thu) - Day 15: TMorphObj 계열 분석 완료 및 Vertex 단위 시간 제어로 "시간을 칠하는" Paint Mode 구현 성공

### 🎯 Completed Tasks
- TMorphObj, TMorphObj_V2, TMorphTest 스크립트 완전 분석 및 차이점 파악
- ProBuilder Vertex Color 표시 문제 해결 (Shader 변경)
- Plane과 Cube에서 Paint Mode 테스트 성공
- Mesh Scale 문제 해결 (Blender import scale 조정)
- Wave Test 키프레임 데이터 생성 및 적용
- Vertex별 개별 시간 제어 구현 확인

### 🔧 Technical Progress
TMorphObj_V2가 프로젝트 비전에 가장 적합한 구현체임을 확인. Vertex별로 다른 시간을 가질 수 있어 한 객체 안에 과거와 미래가 공존 가능. ProBuilder 메시는 UnityVertexColor 셰이더 필요

### 💡 Research Insights
"시간을 4차원 공간으로 조작"하는 핵심 개념 실현. Scrub Mode(균일한 시간 변화)와 Paint Mode(부분적 시간 변화)의 차이가 창의적 가능성을 극대화. 나무의 각 부분이 다른 성장 단계를 가질 수 있음.

### ⚠️ Issues & Blockers
roBuilder Vertex Color 문제와 Mesh Scale 문제 해결.painting시Shpere Effect때문에, 프레인 드랍현상이 일어나는것 같음. Plane Painting시에 Painting된 vertex영역이 떨림.

### 📊 Statistics
- Git commits today: 2
- Files modified: 1
- Progress: 19.5%

### 🎯 Next Steps
- Curve Editor 구현 시작 (시간 보간 곡선 편집)
- Brush 속도가 시간 변화 속도에 영향 주도록 개선
- 다중 객체 시스템 설계 시작
- 성능 최적화 (많은 Vertex에서도 90 FPS 유지)

### 💭 Reflection
vertex의 painting 시간제어 효과가 visual적으로 보여져서 만족스럽다.

---

## 2025-07-11 (Fri) - Day 16: Brush Effect 최적화

### 🎯 Completed Tasks
- Brush Effect 최적화

### 🔧 Technical Progress


### 💡 Research Insights


### ⚠️ Issues & Blockers


### 📊 Statistics
- Git commits today: 2
- Files modified: 2
- Progress: 19.8%

### 🎯 Next Steps
- None



---

## 2025-07-16 (Wed) - Day 21: Temporal VR 비전 재정립 및 TMorphObj_V2 구현

### 🎯 Completed Tasks
- Temporal VR 프로젝트의 정확한 비전 재정립 (Space-Time Hypercube 개념)
- TMorphObj_V2 완전 재구현 - 시간 마킹과 재생 분리
- TemporalEventSystem에 OnPlaybackToggled 이벤트 추가
- TVR_Controller의 브러시 메서드 호출 수정 (ApplyTemporalBrush → MarkTemporalData)
- Serialization 충돌 해결 (currentTime → playbackPosition)
- 브러시 시간 누적 방식 구현 (오래 칠수록 빨간색으로 변화)

### 🔧 Technical Progress
프로젝트의 핵심 비전을 재정립: "시간을 4차원 공간으로 조작"하는 Space-Time Hypercube (STH) 개념. 기존의 실시간 변형 방식에서 벗어나, 브러시로 시간 정보를 마킹하고 별도의 타임라인 재생으로 변형을 확인하는 방식으로 전환. 이는 사용자가 시간축을 따라 객체의 변화를 스케치하는 본래 비전에 더 가깝다.

### 💡 Research Insights
Temporal Brush는 즉각적인 변형 도구가 아니라 "시간 정보를 칠하는" 마킹 도구여야 함. 이는 SIGGRAPH 논문의 Space-Time Hypercube 시각화에서 4D 데이터를 3D로 투영하는 개념과 일치. 브러시 누적 방식은 시간의 강도를 시각적으로 표현하는 효과적인 방법.

### ⚠️ Issues & Blockers
VR 내 재생 컨트롤러 UI가 아직 구현되지 않음. 현재는 백엔드 로직만 완성된 상태로, 실제 VR에서 재생/정지, 타임라인 스크러빙 등의 인터랙션 구현 필요.

### 📊 Statistics
- Git commits today: 3
- Files modified: 1
- Progress: 21.4%

### 🎯 Next Steps
- VR 재생 컨트롤 UI 구현 (왼쪽 컨트롤러 활용)
- 타임라인 시각화 위젯 추가
- 재생 속도 조절 기능
- 시간 마킹 프리뷰 모드

### 💭 Reflection
Git commits today: 6

---

## 2025-07-16 (Wed) - Day 21: Temporal VR 비전 재정립 및 TMorphObj_V2 구현

### 🎯 Completed Tasks
- Temporal VR 프로젝트의 정확한 비전 재정립 (Space-Time Hypercube 개념)
- TMorphObj_V2 완전 재구현 - 시간 마킹과 재생 분리
- TemporalEventSystem에 OnPlaybackToggled 이벤트 추가
- TVR_Controller의 브러시 메서드 호출 수정 (ApplyTemporalBrush → MarkTemporalData)
- Serialization 충돌 해결 (currentTime → playbackPosition)
- 브러시 시간 누적 방식 구현 (오래 칠수록 빨간색으로 변화)

### 🔧 Technical Progress
프로젝트의 핵심 비전을 재정립: "시간을 4차원 공간으로 조작"하는 Space-Time Hypercube (STH) 개념. 기존의 실시간 변형 방식에서 벗어나, 브러시로 시간 정보를 마킹하고 별도의 타임라인 재생으로 변형을 확인하는 방식으로 전환. 이는 사용자가 시간축을 따라 객체의 변화를 스케치하는 본래 비전에 더 가깝다.

### 💡 Research Insights
Temporal Brush는 즉각적인 변형 도구가 아니라 "시간 정보를 칠하는" 마킹 도구여야 함. 이는 SIGGRAPH 논문의 Space-Time Hypercube 시각화에서 4D 데이터를 3D로 투영하는 개념과 일치. 브러시 누적 방식은 시간의 강도를 시각적으로 표현하는 효과적인 방법.

### ⚠️ Issues & Blockers
VR 내 재생 컨트롤러 UI가 아직 구현되지 않음. 현재는 백엔드 로직만 완성된 상태로, 실제 VR에서 재생/정지, 타임라인 스크러빙 등의 인터랙션 구현 필요.

### 📊 Statistics
- Git commits today: 3
- Files modified: 3
- Progress: 21.4%

### 🎯 Next Steps
- VR 재생 컨트롤 UI 구현 (왼쪽 컨트롤러 활용)
- 타임라인 시각화 위젯 추가
- 재생 속도 조절 기능
- 시간 마킹 프리뷰 모드

### 💭 Reflection
Git commits today: 6

---

## 2025-07-16 (Wed) - Day 21: Temporal VR 비전 재정립 및 TMorphObj_V2 구현

### 🎯 Completed Tasks
- Temporal VR 프로젝트의 정확한 비전 재정립 (Space-Time Hypercube 개념)
- TMorphObj_V2 완전 재구현 - 시간 마킹과 재생 분리
- TemporalEventSystem에 OnPlaybackToggled 이벤트 추가
- TVR_Controller의 브러시 메서드 호출 수정 (ApplyTemporalBrush → MarkTemporalData)
- Serialization 충돌 해결 (currentTime → playbackPosition)
- 브러시 시간 누적 방식 구현 (오래 칠수록 빨간색으로 변화)

### 🔧 Technical Progress
프로젝트의 핵심 비전을 재정립: "시간을 4차원 공간으로 조작"하는 Space-Time Hypercube (STH) 개념. 기존의 실시간 변형 방식에서 벗어나, 브러시로 시간 정보를 마킹하고 별도의 타임라인 재생으로 변형을 확인하는 방식으로 전환. 이는 사용자가 시간축을 따라 객체의 변화를 스케치하는 본래 비전에 더 가깝다.

### 💡 Research Insights
Temporal Brush는 즉각적인 변형 도구가 아니라 "시간 정보를 칠하는" 마킹 도구여야 함. 이는 SIGGRAPH 논문의 Space-Time Hypercube 시각화에서 4D 데이터를 3D로 투영하는 개념과 일치. 브러시 누적 방식은 시간의 강도를 시각적으로 표현하는 효과적인 방법.

### ⚠️ Issues & Blockers
VR 내 재생 컨트롤러 UI가 아직 구현되지 않음. 현재는 백엔드 로직만 완성된 상태로, 실제 VR에서 재생/정지, 타임라인 스크러빙 등의 인터랙션 구현 필요.

### 📊 Statistics
- Git commits today: 3
- Files modified: 3
- Progress: 21.4%

### 🎯 Next Steps
- VR 재생 컨트롤 UI 구현 (왼쪽 컨트롤러 활용)
- 타임라인 시각화 위젯 추가
- 재생 속도 조절 기능
- 시간 마킹 프리뷰 모드



---

## 2025-07-16 (Wed) - Day 21: Temporal VR 비전 재정립 및 TMorphObj_V2 구현

### 🎯 Completed Tasks
- Temporal VR 프로젝트의 정확한 비전 재정립 (Space-Time Hypercube 개념)
- TMorphObj_V2 완전 재구현 - 시간 마킹과 재생 분리
- TemporalEventSystem에 OnPlaybackToggled 이벤트 추가
- TVR_Controller의 브러시 메서드 호출 수정 (ApplyTemporalBrush → MarkTemporalData)
- Serialization 충돌 해결 (currentTime → playbackPosition)
- 브러시 시간 누적 방식 구현 (오래 칠수록 빨간색으로 변화)

### 🔧 Technical Progress
프로젝트의 핵심 비전을 재정립: "시간을 4차원 공간으로 조작"하는 Space-Time Hypercube (STH) 개념. 기존의 실시간 변형 방식에서 벗어나, 브러시로 시간 정보를 마킹하고 별도의 타임라인 재생으로 변형을 확인하는 방식으로 전환. 이는 사용자가 시간축을 따라 객체의 변화를 스케치하는 본래 비전에 더 가깝다.

### 💡 Research Insights
Temporal Brush는 즉각적인 변형 도구가 아니라 "시간 정보를 칠하는" 마킹 도구여야 함. 이는 SIGGRAPH 논문의 Space-Time Hypercube 시각화에서 4D 데이터를 3D로 투영하는 개념과 일치. 브러시 누적 방식은 시간의 강도를 시각적으로 표현하는 효과적인 방법.

### ⚠️ Issues & Blockers
VR 내 재생 컨트롤러 UI가 아직 구현되지 않음. 현재는 백엔드 로직만 완성된 상태로, 실제 VR에서 재생/정지, 타임라인 스크러빙 등의 인터랙션 구현 필요.

### 📊 Statistics
- Git commits today: 3
- Files modified: 3
- Progress: 21.4%

### 🎯 Next Steps
- VR 재생 컨트롤 UI 구현 (왼쪽 컨트롤러 활용)
- 타임라인 시각화 위젯 추가
- 재생 속도 조절 기능
- 시간 마킹 프리뷰 모드



---
