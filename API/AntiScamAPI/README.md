# 🛡️ Anti-Scam Recommender API

ASP.NET Core Web API met **Hybrid ML.NET Recommendation System** (70% Collaborative Filtering + 30% Content-Based Filtering)

---

## 🚀 Quick Start

### Local Development
```bash
cd AntiScamAPI
dotnet restore
dotnet run --urls="http://localhost:5000"
```

Test: `curl http://localhost:5000/api/health`

### Railway Deployment
```bash
# 1. Commit code to Git
git add .
git commit -m "Deploy to Railway"
git push

# 2. Deploy on Railway.app
# - Connect GitHub repo
# - Railway auto-detects Dockerfile
# - Get public URL: https://YOUR-APP.up.railway.app
```

See [RAILWAY_DEPLOYMENT.md](../RAILWAY_DEPLOYMENT.md) for full guide.

---

## 📋 API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/health` | GET | Health check + data stats |
| `/api/recommendations` | POST | Get hybrid recommendations |
| `/api/users/register` | POST | Register new user |
| `/api/users/{userId}` | GET | Get user profile |
| `/api/modules` | GET | List all modules |
| `/api/modules/{moduleId}` | GET | Get module details |
| `/api/ratings` | POST | Submit user rating |

---

## 🧪 Example Requests

### Get Recommendations
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
      "scamType": "whatsapp"
    }
  ]
}
```

### Register User
```bash
curl -X POST http://localhost:5000/api/users/register \
  -H "Content-Type: application/json" \
  -d '{"AgeGroup": "26-35", "DigitalLiteracy": 3, "PreferredTopic": "phishing"}'
```

---

## 🎯 Hybrid System

**Collaborative Filtering (70%)**
- ML.NET Matrix Factorization
- Learns from user-item interactions
- Works best for users with rating history

**Content-Based Filtering (30%)**
- Feature matching (topic, literacy, difficulty)
- Solves cold-start problem
- Works for new users without ratings

**Smart Fallback:**
- New users (no ratings) → 100% content-based
- Existing users (has ratings) → 70/30 hybrid

---

## 📁 Project Structure

```
AntiScamAPI/
├── Program.cs                    # Startup + CORS + PORT config
├── Dockerfile                    # Multi-stage .NET 9.0 build
├── railway.json                  # Railway deployment config
├── Controllers/
│   ├── RecommendationsController # Hybrid recommendations
│   ├── UsersController           # User registration
│   ├── ModulesController         # Module CRUD
│   ├── RatingsController         # Rating submission
│   └── HealthController          # Health check
├── Services/
│   ├── HybridRecommendationService # Main service (singleton)
│   └── DataService               # CSV read/write
├── Models/
│   ├── HybridRecommender         # Hybrid logic
│   ├── ContentBasedRecommender   # Content matching
│   └── DTOs                      # Request/Response models
└── Data/
    ├── model.zip                 # Trained MF model (382 KB)
    ├── modules.csv               # 30 training modules
    ├── users.csv                 # 1000 users
    └── ratings.csv               # 20,000 ratings
```

---

## 🐛 Bug Fixes

### ✅ Fixed: User Registration Empty Fields
**Problem:** Profile returned empty after registration
**Solution:** Added in-memory user cache

### ✅ Fixed: Recommendations Crash for New Users
**Problem:** Infinity/NaN for users without ratings
**Solution:** Fallback to content-based only for new users

---

## 🔧 Tech Stack

- **Framework**: .NET 9.0
- **ML Library**: ML.NET 5.0 (Matrix Factorization)
- **Platform**: Railway.app (Docker deployment)
- **Architecture**: Hybrid Recommender System
- **Data**: CSV files (lightweight, portable)

---

## 📊 Performance

- **Model Size**: 382 KB
- **Memory**: ~200-300 MB runtime
- **Cold Start**: < 5 seconds
- **Recommendation Latency**: < 100ms
- **Concurrent Users**: 100+ (Railway Hobby tier)

---

## 🌐 Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `PORT` | 8080 | Server port (Railway sets this) |
| `ASPNETCORE_ENVIRONMENT` | Production | Environment mode |

---

## 📖 Documentation

- [Full Deployment Guide](../RAILWAY_DEPLOYMENT.md)
- [API Test Guide](../TEST_API.md)
- [Model Training](../../python/ModelTraining.ipynb)

---

## 📝 License

MIT License - Anti-Scam Training Recommender System

---

**Version**: 1.0.0
**Last Updated**: 24 December 2025
**Maintainer**: Thomas More AI Project
