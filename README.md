# Apple Game (사과게임) - Unity WebGL

숫자의 합이 10이 되도록 사과를 드래그하여 선택하는 퍼즐 게임입니다.
2007년 일본의 원작 사과게임을 Unity WebGL로 재구현한 버전입니다.

## 게임 규칙

1. 10x17 그리드에 1~9 숫자가 적힌 사과 170개가 배치됩니다
2. 마우스/터치로 드래그하여 사각형 영역을 선택합니다
3. 선택 영역 내 사과 숫자의 합이 **정확히 10**이면 사과가 제거됩니다
4. 제거된 사과 위의 사과는 중력에 의해 아래로 떨어집니다
5. 2분 안에 최대한 많은 사과를 제거하세요!

## 차별화 기능 (원작 대비)

- **실시간 합계 표시**: 드래그 중 선택된 사과의 합을 실시간으로 표시
- **힌트 시스템**: 유효한 조합을 하이라이트 (게임당 3회)
- **셔플 기능**: 남은 사과의 위치를 섞기 (게임당 2회)
- **콤보 시스템**: 연속 성공 시 점수 배율 증가

## 씬 설정 방법

1. Unity에서 `Assets/Scenes/SampleScene.unity`를 엽니다
2. Hierarchy에서 기존 오브젝트를 모두 삭제합니다 (카메라 포함)
3. 빈 GameObject를 생성합니다 (`Create Empty`)
4. 이름을 `GameSetup`으로 변경합니다
5. Inspector에서 `Add Component` → `AppleGameSetup`을 추가합니다
6. **Play** 버튼을 누르면 모든 것이 자동으로 생성됩니다

> AppleGameSetup이 카메라, 매니저, UI, 배경 등을 모두 자동 생성합니다.

## 프로젝트 구조

```
Assets/
├── Scripts/
│   ├── AppleGameSetup.cs   # 씬 부트스트래퍼 (이것만 오브젝트에 추가하면 됨)
│   ├── GameManager.cs       # 게임 상태, 타이머, 흐름 제어
│   ├── GridManager.cs       # 사과 그리드 생성/관리, 중력, 힌트, 셔플
│   ├── Apple.cs             # 개별 사과 (숫자, 색상, 애니메이션)
│   ├── DragSelector.cs      # 드래그 입력 처리, 선택 영역, 실시간 합계
│   ├── ScoreManager.cs      # 점수, 콤보, 최고기록
│   ├── UIManager.cs         # UI 패널 (메뉴, HUD, 카운트다운, 결과)
│   └── AudioManager.cs      # 사운드 관리 (클립은 Inspector에서 할당)
├── Scenes/
│   └── SampleScene.unity
└── Settings/
    └── (URP 설정 파일들)
```

## 아키텍처

```
AppleGameSetup (부트스트래퍼)
├── Camera (자동 생성, 직교 투영)
├── GameManager (게임 상태 머신)
│   ├── 타이머 (2분 카운트다운)
│   ├── 힌트/셔플 관리
│   └── 이벤트 시스템 (상태 변경 알림)
├── GridManager (그리드 관리)
│   ├── 사과 생성/배치
│   ├── 중력 시스템
│   ├── 힌트 탐색 알고리즘
│   └── 셔플 (Fisher-Yates)
├── DragSelector (입력 처리)
│   ├── Pointer (마우스+터치 통합)
│   ├── 선택 영역 시각 피드백
│   └── 실시간 합계 표시
├── ScoreManager (점수 시스템)
│   ├── 콤보 (3초 타임아웃)
│   ├── 최고기록 (PlayerPrefs)
│   └── 점수 팝업 이벤트
├── UIManager (UI 자동 생성)
│   ├── 메인 메뉴
│   ├── 게임 HUD
│   ├── 카운트다운 오버레이
│   └── 결과 화면
├── AudioManager (오디오)
│   ├── BGM
│   └── SFX (Inspector에서 클립 할당)
└── FirebaseManager (랭킹 시스템)
    ├── 익명 인증
    ├── 점수 제출
    └── 리더보드 불러오기
```

## WebGL 빌드 설정

### 필수 설정 (File → Build Settings → Player Settings)

1. **Platform**: WebGL로 전환
2. **Player Settings**:
   - API Compatibility Level: `.NET Standard 2.1`
   - IL2CPP Code Generation: `Faster (Smaller) Builds`
   - Managed Stripping Level: `High`
3. **Publishing Settings**:
   - Compression Format: `Brotli` (가장 작은 크기)
   - Data Caching: `Enabled`
   - Debug Symbols: `Off`

### 빌드 방법

1. `File` → `Build Settings`
2. `WebGL` 플랫폼 선택 → `Switch Platform`
3. `Player Settings`에서 위 설정 적용
4. `Build` 클릭하여 출력 폴더 선택
5. 빌드 완료 후 `index.html`을 웹 서버에 배포

### 호스팅

- **itch.io**: 빌드 폴더를 zip으로 압축하여 업로드
- **GitHub Pages**: 빌드 결과물을 `docs/` 폴더에 커밋
- **직접 서버**: Brotli/gzip 서빙을 위한 MIME 타입 설정 필요

## 오디오 추가 방법

AudioManager에 AudioClip을 할당하면 사운드가 재생됩니다:

1. 오디오 파일들을 `Assets/Audio/` 폴더에 추가
2. Hierarchy에서 `AudioManager` 오브젝트 선택
3. Inspector에서 각 슬롯에 AudioClip 드래그 앤 드롭:
   - `BGM Clip`: 배경 음악
   - `Success Clip`: 사과 제거 성공
   - `Fail Clip`: 실패 피드백
   - `Combo Clip`: 콤보 달성
   - `Countdown Clip`: 카운트다운
   - `Game Over Clip`: 게임 종료

## 랭킹 시스템 (Firebase)

### 현재 상태
- ✅ UI 구현 완료 (메인 메뉴 RANKING 버튼, 리더보드 패널)
- ✅ 닉네임 시스템 완료 (LocalStorage 저장)
- ✅ 자동 점수 제출 (게임 오버 시)
- ⚠️ Firebase SDK 미설치 (플레이스홀더 데이터로 작동)

### Firebase 연동 방법

#### 1. Firebase 프로젝트 생성
1. [Firebase Console](https://console.firebase.google.com/) 접속
2. 새 프로젝트 생성
3. Firestore Database 활성화
4. Authentication → 로그인 방법 → 익명 활성화

#### 2. Unity Firebase SDK 설치
1. [Firebase Unity SDK](https://firebase.google.com/download/unity) 다운로드
2. Unity에서 다음 패키지 임포트:
   - `FirebaseAuth.unitypackage`
   - `FirebaseFirestore.unitypackage`
3. `google-services.json` (Android) 또는 `GoogleService-Info.plist` (iOS) 다운로드
4. `Assets/` 폴더에 추가

#### 3. 코드 활성화
`FirebaseManager.cs` 파일에서 주석 해제:
```csharp
// 1. 상단 using 구문 주석 해제
// 2. InitializeFirebase() 내부 코드 주석 해제
// 3. SignInAnonymously() 주석 해제
// 4. SubmitScore() 내부 코드 주석 해제
// 5. LoadLeaderboard() 내부 코드 주석 해제
```

#### 4. Firestore 보안 규칙 설정
```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    match /leaderboard/{document} {
      // 인증된 사용자만 읽기/쓰기 가능
      allow read: if request.auth != null;
      allow create: if request.auth != null;
    }
  }
}
```

#### 5. 빌드 설정 (WebGL)
- `Project Settings` → `Player` → `Publishing Settings`
- Compression Format: `Disabled` (Firebase SDK와 호환성 문제 방지)
- 또는 `Gzip` 사용

### 작동 방식
1. 게임 시작 시 자동으로 익명 로그인
2. 게임 오버 시 닉네임+점수+콤보 자동 제출
3. RANKING 버튼 클릭 → 상위 10명 불러오기
4. 닉네임은 결과 화면에서 변경 가능 (LocalStorage 저장)

### Firebase 없이 테스트
- Firebase SDK 미설치 시 플레이스홀더 데이터로 작동
- 모든 UI와 기능은 정상 작동
- 실제 데이터 저장/불러오기만 불가능

## 기술 스택

- Unity 6 (6000.0.58f2)
- Universal Render Pipeline (2D)
- Input System (마우스 + 터치 통합)
- TextMeshPro (UI 텍스트)
- Firebase (Firestore + Auth) - 선택사항

## 향후 개발 계획

### v1.5
- 특수 사과 (폭탄, 황금, 무지개)
- 추가 게임 모드 (Zen, 챌린지, 스테이지)

### v2.0
- ✅ 글로벌 리더보드 (UI 완료, Firebase 연동 준비 완료)
- 리플레이 공유
- 커스텀 테마/스킨
- 친구 도전 시스템
