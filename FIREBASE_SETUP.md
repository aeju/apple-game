# Firebase 설정 가이드 🔥

이 가이드는 PAW 10 게임에 Firebase 온라인 랭킹 시스템을 설정하는 방법을 안내합니다.

## 📋 목차

1. [Firebase 프로젝트 생성](#1-firebase-프로젝트-생성)
2. [Unity Firebase SDK 설치](#2-unity-firebase-sdk-설치)
3. [Firestore 보안 규칙 설정](#3-firestore-보안-규칙-설정)
4. [WebGL 빌드 설정](#4-webgl-빌드-설정)
5. [테스트](#5-테스트)
6. [문제 해결](#6-문제-해결)

---

## 1. Firebase 프로젝트 생성

### 1-1. Firebase Console 접속
- https://console.firebase.google.com/
- Google 계정으로 로그인

### 1-2. 새 프로젝트 만들기
1. **"프로젝트 추가"** 클릭
2. **프로젝트 이름**: `apple-game` (또는 원하는 이름)
3. **Google Analytics**: 비활성화 권장 (게임에 불필요)
4. **프로젝트 만들기** 클릭

### 1-3. Firestore Database 설정
1. 좌측 메뉴 → **"Firestore Database"** → **"데이터베이스 만들기"**
2. **위치 선택**: `asia-northeast3` (서울) 권장
3. **보안 규칙**: "프로덕션 모드로 시작" 선택
4. **사용 설정** 클릭

### 1-4. Authentication 설정
1. 좌측 메뉴 → **"Authentication"** → **"시작하기"**
2. **"로그인 방법"** 탭 클릭
3. **"익명"** 선택 → **"사용 설정"** 토글 ON
4. **저장**

---

## 2. Unity Firebase SDK 설치

### 2-1. Firebase Unity SDK 다운로드
1. https://firebase.google.com/download/unity 접속
2. **"Firebase Unity SDK 다운로드"** 클릭
3. `firebase_unity_sdk.zip` 다운로드 및 압축 해제

### 2-2. Unity에 패키지 임포트
1. Unity 열기 → **Assets** → **Import Package** → **Custom Package**
2. 다운로드한 SDK 폴더에서 다음 패키지만 선택:
   - ✅ `FirebaseAuth.unitypackage`
   - ✅ `FirebaseFirestore.unitypackage`
   - ✅ `FirebaseAnalytics.unitypackage` (의존성)
3. **Import** 클릭
4. 모든 파일 선택 → **Import** 클릭

### 2-3. google-services.json 추가
1. Firebase Console → 프로젝트 설정 (⚙️ 아이콘)
2. **"내 앱"** 섹션 → **"앱 추가"** 클릭
3. **Unity** 아이콘 선택
4. 패키지 이름 입력: `com.yourcompany.applegame` (또는 원하는 이름)
5. **`google-services.json`** 다운로드
6. Unity 프로젝트 **`Assets/`** 폴더 루트에 복사

### 2-4. 빌드 설정 확인
Unity → **File** → **Build Settings** → **Player Settings**:
- **Scripting Backend**: IL2CPP
- **API Compatibility Level**: .NET Standard 2.1

---

## 3. Firestore 보안 규칙 설정

### 3-1. 규칙 편집
1. Firebase Console → **Firestore Database** → **규칙** 탭
2. 다음 규칙으로 교체:

```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    // 리더보드: 인증된 사용자만 읽기/쓰기
    match /leaderboard/{entry} {
      // 누구나 읽기 가능
      allow read: if request.auth != null;
      
      // 인증된 사용자만 새 점수 등록 가능
      allow create: if request.auth != null 
                    && request.resource.data.score is int
                    && request.resource.data.nickname is string
                    && request.resource.data.combo is int
                    && request.resource.data.timestamp is int
                    && request.resource.data.score >= 0
                    && request.resource.data.score <= 100000;  // 최대 점수 제한
      
      // 수정/삭제 불가 (치팅 방지)
      allow update, delete: if false;
    }
  }
}
```

3. **게시** 버튼 클릭

### 3-2. 색인 추가 (선택사항, 성능 향상)
1. **색인** 탭 클릭
2. **복합 색인 추가**:
   - 컬렉션 ID: `leaderboard`
   - 필드: `score` (내림차순)
   - 필드: `timestamp` (내림차순)
3. **만들기** 클릭

---

## 4. WebGL 빌드 설정

### 4-1. WebGL 플랫폼 전환
1. **File** → **Build Settings**
2. **WebGL** 선택 → **Switch Platform**

### 4-2. Player Settings 조정
**Publishing Settings**:
- **Compression Format**: `Disabled` 또는 `Gzip`
  - ⚠️ `Brotli`는 Firebase SDK와 호환성 문제 가능
- **Data Caching**: Enabled
- **Enable Exceptions**: `Explicitly Thrown Exceptions Only`

### 4-3. 빌드
1. **Build** 클릭
2. 출력 폴더 선택
3. 빌드 완료까지 대기 (5-10분)

---

## 5. 테스트

### 5-1. Unity Editor에서 테스트
1. Unity Play 버튼 클릭
2. Console 확인:
   ```
   [FirebaseManager] Firebase initialized successfully!
   [FirebaseManager] User signed in successfully: [userId]
   ```
3. 게임 플레이 → 게임 오버
4. Console 확인:
   ```
   [FirebaseManager] Score submitted: [nickname] - [score]
   ```
5. RANKING 버튼 클릭
6. Firebase에서 불러온 랭킹 확인

### 5-2. WebGL 빌드 테스트
1. 빌드 폴더의 `index.html` 실행
2. 브라우저 Console 열기 (F12)
3. 게임 플레이 및 점수 제출 확인
4. Firebase Console → Firestore Database → 데이터 탭에서 실제 저장 확인

---

## 6. 문제 해결

### ❌ "Firebase not initialized" 에러
**원인**: `google-services.json` 파일이 없거나 잘못된 위치
**해결**:
1. `google-services.json`이 `Assets/` 폴더 루트에 있는지 확인
2. Unity 재시작
3. Firebase Console에서 파일 다시 다운로드

### ❌ "DependencyStatus.UnavailableOther" 에러
**원인**: Firebase SDK가 제대로 임포트되지 않음
**해결**:
1. `Assets/Firebase/` 폴더 확인
2. Firebase SDK 재설치
3. Unity 재시작

### ❌ WebGL 빌드 후 "Failed to download file" 에러
**원인**: CORS 또는 압축 설정 문제
**해결**:
1. Player Settings → Publishing Settings
2. Compression Format: `Disabled` 또는 `Gzip` 사용
3. 로컬 웹 서버로 테스트 (예: Python HTTP Server)

### ❌ "Permission denied" 에러
**원인**: Firestore 보안 규칙 문제
**해결**:
1. Firebase Console → Firestore Database → 규칙 확인
2. 위의 [3-1. 규칙 편집](#3-1-규칙-편집) 섹션대로 다시 설정
3. "게시" 클릭

### ❌ 랭킹이 표시되지 않음
**원인**: 아직 점수가 제출되지 않음
**해결**:
1. 게임을 플레이하여 점수 획득
2. Console에서 "Score submitted" 로그 확인
3. Firebase Console → Firestore Database → 데이터 탭에서 실제 저장 확인
4. RANKING 버튼 다시 클릭

---

## 📊 Firebase 사용량 모니터링

### 무료 할당량 (Spark 플랜)
- **Firestore 읽기**: 50,000회/일
- **Firestore 쓰기**: 20,000회/일
- **Authentication**: 무제한

### 예상 사용량 (하루 100명 플레이 시)
- 게임 오버 시 점수 쓰기: 100회
- 랭킹 조회 (1인당 3회): 300회
- **총**: 400회/일 (무료 한도의 1% 미만)

### 사용량 확인
Firebase Console → 사용량 탭에서 실시간 모니터링 가능

---

## 🎉 완료!

이제 Firebase 온라인 랭킹 시스템이 설정되었습니다!

**다음 단계**:
1. ✅ 게임 플레이 및 점수 제출 테스트
2. ✅ 친구들에게 링크 공유하여 랭킹 경쟁
3. ✅ Firebase Console에서 실시간 데이터 모니터링

**문제가 있나요?**
- Unity Console 로그 확인
- 브라우저 Console 로그 확인 (F12)
- Firebase Console 규칙 및 데이터 확인
