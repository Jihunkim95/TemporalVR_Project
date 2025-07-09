unity/TemporalVR/Assets/Scripts/TemporalVR/
├── Core/
│   ├── TVR_Controller.cs        # VR 컨트롤러 입력 & 모드 관리
│   ├── TVRFeedback.cs          # 시각적 피드백 (커서, 타임라인, 효과)
│   ├── TVRSetup.cs             # 자동 셋업 헬퍼
│   └── PerformanceMonitor.cs   # FPS & 메모리 모니터링
│
├── Morphing/
│   ├── TMorphObj.cs            # 모프 기반 시간 객체 (핵심)
│   ├── TKeyframe.cs            # 키프레임 데이터 구조
│   ├── TMorphData.cs           # 모프 시퀀스 저장/로드
│   └── TMorphTest.cs           # 테스트 & 데모용
│
├── Brush/
│   ├── TBrushData.cs           # Temporal Brush 데이터
│   └── TBrushController.cs     # 브러시 입력 처리
│
├── Base/
│   ├── TObject.cs              # (Legacy - 단계적 제거 예정)
│   └── TemporalObject.cs       # 베이스 클래스
│
└── Utilities/
    └── TemporalEventSystem.cs  # 이벤트 시스템