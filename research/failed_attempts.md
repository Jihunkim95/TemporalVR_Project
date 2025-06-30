2025/06/30
### 1. **XR Device Simulator 컨트롤러 인식 문제 해결**
- **문제**: Unity 2022.3.55f1에서 컨트롤러가 False로 인식
- **해결**: 
  - TVR_Controller 스크립트 XRController 타입을 Transform으로 변경
  - ActionBasedController 사용으로 전환
  - 초기화 지연 추가 (Coroutine)

  ### 2: Shader 오류
- **원인**: 렌더 파이프라인별 Shader 이름 불일치
- **해결**: URP Shader다운로드