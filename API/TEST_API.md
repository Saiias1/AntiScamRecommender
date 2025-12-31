# Anti-Scam API - Test Guide

De API draait op: **http://localhost:5000**

## ✅ Getest & Werkend

Alle endpoints zijn succesvol getest!

## 🐛 Fixed Bugs

### Bug Fix 1: User Registration
**Problem:** POST /api/users/register returned empty profile fields
**Solution:** Added in-memory user cache to HybridRecommendationService

### Bug Fix 2: Recommendations Crash for New Users
**Problem:** New users without ratings caused JSON serialization error (Infinity/NaN)
**Solution:**
- Check if user has ratings in database
- If NO ratings → use **content-based recommendations only** (collaborative score = 0)
- If HAS ratings → use **hybrid recommendations** (70% collaborative + 30% content-based)

---

## 📋 Test Endpoints

### 1. Health Check
```bash
curl http://localhost:5000/api/health
```

**Response:**
```json
{
  "status": "Healthy",
  "modelLoaded": true,
  "dataFilesLoaded": true,
  "totalUsers": 1000,
  "totalModules": 30,
  "totalRatings": 20000
}
```

---

### 2. Get Hybrid Recommendations ⭐ (BELANGRIJKSTE!)
```bash
curl -X POST http://localhost:5000/api/recommendations \
  -H "Content-Type: application/json" \
  -d '{"UserId": 42, "Top": 5}'
```

**Response:**
```json
{
  "userId": 42,
  "userProfile": {
    "literacy": 5,
    "preferredTopic": "whatsapp",
    "ageGroup": "36-45",
    "riskProfile": "medium"
  },
  "recommendations": [
    {
      "moduleId": 5,
      "title": "whatsapp Training Module",
      "hybridScore": 3.68,
      "collaborativeScore": 3.58,
      "contentScore": 3.92,
      "difficulty": 5,
      "scamType": "whatsapp",
      "targetLiteracy": 1,
      "durationMin": 11
    }
  ]
}
```

**LET OP:**
- `hybridScore` = 70% * `collaborativeScore` + 30% * `contentScore`
- Content score is hoog voor "whatsapp" omdat dat de preferred_topic van user 42 is!

---

### 3. Get Module Details
```bash
curl http://localhost:5000/api/modules/15
```

**Response:**
```json
{
  "moduleId": 15,
  "scamType": "ai_voice",
  "difficulty": 5,
  "targetLiteracy": 4,
  "durationMin": 5
}
```

---

### 4. Register New User ⭐ (FIXED!)
**Note:** Profile is now correct! No longer empty fields.
```bash
curl -X POST http://localhost:5000/api/users/register \
  -H "Content-Type: application/json" \
  -d '{
    "AgeGroup": "26-35",
    "DigitalLiteracy": 3,
    "PreferredTopic": "phishing"
  }'
```

**Response:**
```json
{
  "userId": 1001,
  "profile": {
    "literacy": 3,
    "preferredTopic": "phishing",
    "ageGroup": "26-35",
    "riskProfile": "medium"
  }
}
```

**Note:** Risk profile wordt automatisch bepaald op basis van literacy:
- Literacy ≤ 2: high risk
- Literacy = 3: medium risk
- Literacy ≥ 4: low risk

---

### 5. Submit Rating
```bash
curl -X POST http://localhost:5000/api/ratings \
  -H "Content-Type: application/json" \
  -d '{
    "UserId": 42,
    "ModuleId": 15,
    "Rating": 4.5
  }'
```

**Response:**
```json
{
  "success": true,
  "message": "Rating submitted successfully for user 42 on module 15"
}
```

**Note:** Rating wordt toegevoegd aan `Data/ratings.csv`

---

### 6. Get User Profile
```bash
curl http://localhost:5000/api/users/42
```

**Response:**
```json
{
  "literacy": 5,
  "preferredTopic": "whatsapp",
  "ageGroup": "36-45",
  "riskProfile": "medium"
}
```

---

### 7. Get All Modules
```bash
curl http://localhost:5000/api/modules
```

**Response:** Array van 30 modules
```json
[
  {
    "moduleId": 1,
    "scamType": "shopping",
    "difficulty": 5,
    "targetLiteracy": 3,
    "durationMin": 8
  },
  ...
]
```

---

## 🔧 API Starten

```bash
cd "c:\Users\Admin\OneDrive - Thomas More\AI project\Succes\Model\AntiScamRecommender\API\AntiScamAPI"
dotnet run --urls="http://localhost:5000"
```

De API laadt bij startup:
- `Data/model.zip` (collaborative filtering model)
- `Data/modules.csv` (30 modules voor content-based)
- `Data/users.csv` (1000 users voor content-based)
- `Data/ratings.csv` (20,000 ratings)

---

## 🎯 Hybride Systeem

Het systeem combineert 2 recommender approaches:

### 1. Collaborative Filtering (70% weight)
- ML.NET Matrix Factorization
- Gebruikt user-item rating matrix
- Leert patterns van gelijkgezinde users

### 2. Content-Based Filtering (30% weight)
- Feature matching tussen user & module
- **Topic match (40%)**: preferred_topic vs scam_type
- **Literacy match (30%)**: digital_literacy vs target_literacy
- **Difficulty (20%)**: module difficulty vs user capability
- **Duration (10%)**: module length preference

### Final Score
```
hybridScore = 0.7 * collaborativeScore + 0.3 * contentScore
```

---

## 📁 Project Structuur

```
API/AntiScamAPI/
├── Program.cs               # Startup + CORS + Singleton config
├── Controllers/
│   ├── RecommendationsController.cs  # Hybrid recommendations
│   ├── UsersController.cs            # User registration + profile
│   ├── ModulesController.cs          # Module details
│   ├── RatingsController.cs          # Submit ratings
│   └── HealthController.cs           # Health check
├── Services/
│   ├── HybridRecommendationService.cs  # Main service
│   └── DataService.cs                  # CSV read/write
├── Models/
│   ├── HybridRecommender.cs          # Hybrid logic (70/30)
│   ├── ContentBasedRecommender.cs    # Content matching
│   ├── ModuleInput.cs
│   ├── UserInput.cs
│   ├── RatingInput.cs
│   └── DTOs.cs                       # Request/Response models
└── Data/
    ├── model.zip                     # Trained MF model
    ├── modules.csv
    ├── users.csv
    └── ratings.csv
```

---

## 🧪 Test: New User Flow (Cold Start)

Deze test demonstreert hoe het systeem omgaat met nieuwe users zonder ratings:

```bash
# Step 1: Register nieuwe user
curl -X POST http://localhost:5000/api/users/register \
  -H "Content-Type: application/json" \
  -d '{"AgeGroup": "26-35", "DigitalLiteracy": 3, "PreferredTopic": "phishing"}'

# Response: {"userId":1003,"profile":{"literacy":3,"preferredTopic":"phishing","ageGroup":"26-35","riskProfile":"medium"}}

# Step 2: Get recommendations (content-based only, geen crash!)
curl -X POST http://localhost:5000/api/recommendations \
  -H "Content-Type: application/json" \
  -d '{"UserId": 1003, "Top": 3}'

# Response toont:
# - collaborativeScore: 0 (user heeft geen ratings)
# - contentScore: hoog voor "phishing" modules (user's preferred topic!)
# - hybridScore = contentScore (100% content-based voor nieuwe users)
```

**Voorbeeld Response:**
```json
{
  "userId": 1003,
  "userProfile": {
    "literacy": 3,
    "preferredTopic": "phishing",
    "ageGroup": "26-35",
    "riskProfile": "medium"
  },
  "recommendations": [
    {
      "moduleId": 6,
      "title": "phishing Training Module",
      "hybridScore": 4.78,
      "collaborativeScore": 0,
      "contentScore": 4.78,
      "difficulty": 1,
      "scamType": "phishing",
      "targetLiteracy": 3,
      "durationMin": 11
    }
  ]
}
```

**Let op:**
- Alle phishing modules (preferred topic match!)
- collaborativeScore = 0 (user heeft geen ratings in systeem)
- hybridScore = contentScore (fallback naar content-based only)
- Geen crash, geen Infinity/NaN errors!

---

## 🚀 Voorbeeld Workflow

1. **Check health:**
   ```bash
   curl http://localhost:5000/api/health
   ```

2. **Register nieuwe user:**
   ```bash
   curl -X POST http://localhost:5000/api/users/register \
     -H "Content-Type: application/json" \
     -d '{"AgeGroup": "18-25", "DigitalLiteracy": 2, "PreferredTopic": "ai_voice"}'
   ```

3. **Get recommendations voor deze user:**
   ```bash
   curl -X POST http://localhost:5000/api/recommendations \
     -H "Content-Type: application/json" \
     -d '{"UserId": 1001, "Top": 5}'
   ```

4. **User voltooit module en geeft rating:**
   ```bash
   curl -X POST http://localhost:5000/api/ratings \
     -H "Content-Type: application/json" \
     -d '{"UserId": 1001, "ModuleId": 15, "Rating": 5.0}'
   ```

5. **Get nieuwe recommendations (collaborative filtering leert!):**
   ```bash
   curl -X POST http://localhost:5000/api/recommendations \
     -H "Content-Type: application/json" \
     -d '{"UserId": 1001, "Top": 5}'
   ```

---

## ✅ Getest op:
- Windows 11
- .NET 9.0
- ML.NET 5.0.0
- Datum: 24/12/2025

Alle endpoints werken perfect! 🎉
