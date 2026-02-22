var FirebaseBridgePlugin = {

    FirebaseBridge_Init: function () {
        if (window._fbReady) return;

        var cfg = {
            apiKey: "AIzaSyAugXJT67-Da6GLSDZaF0V9UHyoZCkK-dE",
            authDomain: "apple-game-239ce.firebaseapp.com",
            projectId: "apple-game-239ce",
            storageBucket: "apple-game-239ce.firebasestorage.app",
            messagingSenderId: "993796862425",
            appId: "1:993796862425:web:0e5cbc4e869686e1aea9c8"
        };

        if (!firebase.apps.length) {
            firebase.initializeApp(cfg);
        }

        window._fbDb = firebase.firestore();
        window._fbAuth = firebase.auth();
        window._fbReady = false;

        window._fbAuth.signInAnonymously().then(function (cred) {
            window._fbUserId = cred.user.uid;
            window._fbReady = true;
            console.log("[FirebaseBridge] Signed in: " + window._fbUserId);
            SendMessage("FirebaseManager", "OnJsInitialized", "ok");
        }).catch(function (err) {
            console.error("[FirebaseBridge] Auth error: " + err.message);
            SendMessage("FirebaseManager", "OnJsInitialized", "error:" + err.message);
        });
    },

    FirebaseBridge_SubmitScore: function (nicknamePtr, score, combo) {
        var nickname = UTF8ToString(nicknamePtr);

        if (!window._fbReady || !window._fbDb) {
            console.warn("[FirebaseBridge] Not ready, score not submitted");
            return;
        }

        window._fbDb.collection("leaderboard").add({
            nickname: nickname,
            score: score,
            combo: combo,
            timestamp: Date.now(),
            userId: window._fbUserId || "anonymous"
        }).then(function () {
            console.log("[FirebaseBridge] Score submitted: " + nickname + " - " + score);
        }).catch(function (err) {
            console.error("[FirebaseBridge] Submit error: " + err.message);
        });
    },

    FirebaseBridge_LoadLeaderboard: function (limit) {
        if (!window._fbReady || !window._fbDb) {
            console.warn("[FirebaseBridge] Not ready, returning empty");
            SendMessage("FirebaseManager", "OnJsLeaderboardLoaded", "[]");
            return;
        }

        window._fbDb.collection("leaderboard")
            .orderBy("score", "desc")
            .limit(limit)
            .get()
            .then(function (snapshot) {
                var entries = [];
                snapshot.forEach(function (doc) {
                    var d = doc.data();
                    entries.push({
                        nickname: d.nickname || "Unknown",
                        score: d.score || 0,
                        combo: d.combo || 0,
                        timestamp: d.timestamp || 0
                    });
                });
                var json = JSON.stringify(entries);
                console.log("[FirebaseBridge] Loaded " + entries.length + " entries");
                SendMessage("FirebaseManager", "OnJsLeaderboardLoaded", json);
            })
            .catch(function (err) {
                console.error("[FirebaseBridge] Load error: " + err.message);
                SendMessage("FirebaseManager", "OnJsLeaderboardLoaded", "[]");
            });
    }
};

mergeInto(LibraryManager.library, FirebaseBridgePlugin);
