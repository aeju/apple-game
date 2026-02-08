# Firebase 웹 앱 설정 가이드 (Vercel 배포용) 🌐

Unity WebGL 빌드를 Vercel에 배포하는 경우 이 가이드를 따라주세요.

## 🎯 왜 웹 앱인가요?

```
Unity 프로젝트
    ↓ WebGL 빌드
HTML + JavaScript + WASM
    ↓ Vercel 배포
브라우저에서 실행
    = 웹 앱!
```

---

## 📋 설정 단계

### 1단계: Firebase 프로젝트 생성

1. **Firebase Console 접속**
   - https://console.firebase.google.com/
   - Google 계정 로그인

2. **새 프로젝트 만들기**
   - 프로젝트 이름: `apple-game`
   - Google Analytics: 비활성화

3. **Firestore Database 생성**
   - 좌측 메뉴 → Firestore Database → 데이터베이스 만들기
   - 위치: `asia-northeast3` (서울)
   - 보안 규칙: "프로덕션 모드"

4. **Authentication 활성화**
   - 좌측 메뉴 → Authentication → 시작하기
   - 로그인 방법 → "익명" 활성화

---

### 2단계: 웹 앱 등록

1. **Firebase Console → 프로젝트 설정 (⚙️)**

2. **"내 앱" 섹션에서 웹 앱 추가**
   ```
   [</> Web] ← 이 아이콘 클릭!
   ```

3. **앱 등록**
   ```
   앱 닉네임: PAW 10
   
   ⬜ 이 앱의 Firebase 호스팅도 설정합니다
   (체크 안 함 - Vercel 사용할 거니까)
   
   [앱 등록] 클릭
   ```

4. **Firebase SDK 구성 복사**
   
   화면에 다음과 같은 코드가 나타납니다:
   
   ```javascript
   // Firebase 구성
   const firebaseConfig = {
     apiKey: "AIzaSyXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
     authDomain: "apple-game-xxxx.firebaseapp.com",
     projectId: "apple-game-xxxx",
     storageBucket: "apple-game-xxxx.appspot.com",
     messagingSenderId: "123456789012",
     appId: "1:123456789012:web:xxxxxxxxxxxxx"
   };
   ```
   
   **이 값들을 메모장에 복사해두세요!** 📋

---

### 3단계: Unity에 Firebase 설정 추가

1. **firebaseConfig.json 파일 열기**
   ```
   Assets/StreamingAssets/firebaseConfig.json
   ```

2. **Firebase Console에서 복사한 값으로 교체**
   
   **변경 전:**
   ```json
   {
     "apiKey": "YOUR_API_KEY_HERE",
     "authDomain": "YOUR_PROJECT_ID.firebaseapp.com",
     ...
   }
   ```
   
   **변경 후:** (실제 값으로 교체)
   ```json
   {
     "apiKey": "AIzaSyXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
     "authDomain": "apple-game-xxxx.firebaseapp.com",
     "projectId": "apple-game-xxxx",
     "storageBucket": "apple-game-xxxx.appspot.com",
     "messagingSenderId": "123456789012",
     "appId": "1:123456789012:web:xxxxxxxxxxxxx"
   }
   ```

3. **파일 저장** (Ctrl + S)

---

### 4단계: Firestore 보안 규칙 설정

Firebase Console → Firestore Database → 규칙:

```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    // 리더보드: 인증된 사용자만 읽기/쓰기
    match /leaderboard/{entry} {
      allow read: if request.auth != null;
      allow create: if request.auth != null 
                    && request.resource.data.score is int
                    && request.resource.data.nickname is string
                    && request.resource.data.score >= 0
                    && request.resource.data.score <= 100000;
      allow update, delete: if false;
    }
  }
}
```

**"게시"** 버튼 클릭!

---

### 5단계: WebGL 빌드

1. **File → Build Settings**
   - Platform: WebGL 선택 → Switch Platform

2. **Player Settings**
   - Publishing Settings:
     - Compression Format: **Gzip** 또는 **Brotli**
     - Data Caching: Enabled
   
3. **Build**
   - Build 클릭
   - 폴더 선택: `Build/WebGL/`
   - 빌드 완료 대기

---

### 6단계: Vercel 배포

#### 방법 1: Vercel CLI (추천)

```bash
# Vercel CLI 설치
npm install -g vercel

# 빌드 폴더로 이동
cd Build/WebGL

# Vercel 배포
vercel

# 프로덕션 배포
vercel --prod
```

#### 방법 2: Vercel 웹사이트

1. https://vercel.com/ 접속
2. "New Project" 클릭
3. GitHub 연동 또는 폴더 업로드
4. 빌드 폴더 (`Build/WebGL`) 선택
5. Deploy 클릭

---

## ✅ 테스트

### Unity Editor에서 테스트

1. **Play 버튼 클릭**

2. **Console 확인**:
   ```
   [FirebaseManager] Firebase initialized successfully!
   [FirebaseManager] User signed in successfully!
   ```

3. **게임 플레이 → 게임 오버**

4. **RANKING 버튼 클릭**
   - Firebase에서 랭킹 불러오기 확인

### Vercel 배포 후 테스트

1. **Vercel URL 접속**
   - 예: `https://apple-game-xxxx.vercel.app/`

2. **브라우저 Console 열기** (F12)

3. **게임 플레이**
   - 점수 제출 확인
   - 랭킹 조회 확인

4. **Firebase Console 확인**
   - Firestore Database → 데이터 탭
   - `leaderboard` 컬렉션에 데이터 저장 확인

---

## 🔧 문제 해결

### ❌ "Firebase config not found" 에러

**원인**: `firebaseConfig.json` 파일을 못 찾음

**해결**:
1. `Assets/StreamingAssets/firebaseConfig.json` 파일 존재 확인
2. Unity 재시작
3. WebGL 빌드 다시 실행

### ❌ "Failed to initialize Firebase" 에러

**원인**: Firebase 설정 값이 잘못됨

**해결**:
1. Firebase Console → 프로젝트 설정
2. 웹 앱 구성 다시 복사
3. `firebaseConfig.json` 값 다시 확인
4. 특히 `apiKey`, `projectId` 확인

### ❌ CORS 에러 (Vercel 배포 후)

**원인**: Firebase 인증 도메인 미등록

**해결**:
1. Firebase Console → Authentication → Settings
2. **승인된 도메인** 섹션
3. Vercel 도메인 추가:
   - `your-app.vercel.app`
4. 저장

### ❌ "Permission denied" 에러

**원인**: Firestore 보안 규칙 문제

**해결**:
1. Firebase Console → Firestore Database → 규칙
2. 위의 보안 규칙 다시 설정
3. "게시" 클릭

---

## 📊 Firebase 웹 앱 vs Unity SDK

| 기능 | 웹 앱 (Vercel) | Unity SDK |
|------|----------------|-----------|
| 플랫폼 | WebGL 빌드 | PC, 모바일 네이티브 |
| 설정 파일 | firebaseConfig.json | google-services.json |
| SDK | Firebase JS SDK | Firebase Unity SDK |
| 배포 | Vercel, Netlify 등 | Steam, App Store 등 |
| 추천 사용 | ✅ **브라우저 게임** | PC/모바일 앱 |

---

## 🎉 완료!

이제 Firebase 온라인 랭킹이 Vercel에서 작동합니다!

**다음 단계**:
1. ✅ 친구들에게 Vercel URL 공유
2. ✅ 전 세계 플레이어와 랭킹 경쟁
3. ✅ Firebase Console에서 실시간 데이터 모니터링

**Vercel URL 예시**:
```
https://apple-game-xxxx.vercel.app/
```

즐거운 게임 되세요! 🎮🏆
