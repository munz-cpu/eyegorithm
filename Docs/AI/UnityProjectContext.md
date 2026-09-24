# Unity 프로젝트 컨텍스트

마지막 확인: 2026-09-24

## 프로젝트 기준

- Unity: 6000.6.0f1
- 기본 씬: `Assets/Scenes/SampleScene.unity`
- 입력: Input System Package(New) 전용
- 렌더링: Universal Render Pipeline 17.6.0 패키지와 2D Renderer 관련 자산을 사용한다. 현재 `GraphicsSettings`의 전역 Render Pipeline Asset은 비어 있으므로 플랫폼별 품질 설정도 함께 확인한다.
- UI: uGUI와 TextMesh Pro
- 테스트: Unity Test Framework 1.8.0은 설치되어 있지만 프로젝트 자체 테스트 어셈블리는 아직 없다.
- 어셈블리: 게임 스크립트용 별도 asmdef는 없으며 기본 `Assembly-CSharp`에 컴파일된다.

## 주요 구조

- `Assets/Scripts/설정관리자.cs`: 알고리즘, 속도, 정원, 좌석 수, 볼륨, 잔여 시간 표시 설정
- `Assets/Scripts/급식실/급식실관리자.cs`: 줄, 입장, 좌석, 식사, 퇴장 흐름
- `Assets/Scripts/급식실/급식실대기열.cs`: FCFS, RR, SJF, 우선순위 선택 정책
- `Assets/Scripts/학생/학생소환기.cs`: 계급별 소환, 이름/식사 시간 무작위 배정, 인원 제한
- `Assets/Scripts/학생/학생정보.cs`: 학생 데이터와 외형/텍스트 표시
- `Assets/Scripts/학생/싸돌아댕기기.cs`: 평상시 배회와 목표 지점 이동
- `Assets/Scripts/UI`: 소환, 열기/닫기, 설정, 경고 UI 연결
- `Assets/Editor/급식실프로젝트구성기.cs`: 씬과 프리팹의 편집 가능한 기본 구성을 생성하거나 복구하는 에디터 메뉴

## 씬과 프리팹

- `SampleScene` 안의 `급식실 시뮬레이션` 루트에 시스템, 이동 지점, 좌석, UI가 직렬화되어 있다.
- `Assets/Prefabs/학생.prefab`은 `학생정보`, 이동 컴포넌트, Rigidbody2D, Collider2D를 가진다.
- 이미지, 폰트, 오디오, 위치, 버튼 모양은 Inspector와 씬에서 교체하도록 설계했다.
- 학생 이름 원본은 `Assets/studentName.json`이다.

## 알고리즘 규칙

- 일반/FCFS: 먼저 줄에 선 학생부터 끝까지 식사한다.
- 라운드 로빈: 설정한 퀀텀만큼 식사한 뒤 남은 시간이 있으면 줄 뒤로 돌아간다.
- 최소 실행시간 우선: 남은 식사 시간이 가장 짧은 학생을 고른다.
- 우선순위: 교장선생님, 학생회장, 3학년, 2학년, 1학년 순이다. 같은 계급은 먼저 온 학생이 우선이다.
- 모든 방식은 비선점형이다. 단, 라운드 로빈만 퀀텀 종료 시 자리를 양보한다.

## 확인된 제약

- 학생회장과 교장선생님은 각각 동시에 한 명만 존재할 수 있다.
- 최대 인원 설정의 절대 상한은 15명이다.
- 좌석 수는 배치된 다섯 자리 중 1~5개를 사용한다.
- Unity AI Assistant 패키지는 설치되어 있지만 이 작업 환경에서는 Unity Editor 자동화 연결을 직접 사용할 수 없었다.

