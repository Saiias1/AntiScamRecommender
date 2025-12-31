# Anti-Scam Educatie Platform - Frontend

Frontend voor het Anti-Scam aanbevelingssysteem gebouwd met vanilla HTML/CSS/JavaScript.

## 🚀 Netlify Deployment

### Stap 1: Login bij Netlify
1. Ga naar [netlify.com](https://www.netlify.com/)
2. Login met je GitHub account (of maak een account aan)

### Stap 2: Deploy via Drag & Drop
1. Klik op "Add new site" → "Deploy manually"
2. Sleep de **hele `frontend` folder** naar de upload zone
3. Wacht tot deployment klaar is (±1 minuut)
4. Je krijgt een URL zoals: `https://random-name-12345.netlify.app`

### Stap 3: Custom Domain (optioneel)
1. Ga naar "Site settings" → "Domain management"
2. Klik "Add custom domain"
3. Voer je gewenste naam in (bijv: `anti-scam-edu.netlify.app`)

## 📋 Features

### User Mode (Standaard)
- ✅ Onboarding flow (3 stappen)
- ✅ Digital literacy quiz
- ✅ Gepersonaliseerde module aanbevelingen
- ✅ 30 trainingsmodules
- ✅ Rating systeem (1-5 sterren)
- ✅ Progress tracking
- ❌ API status **VERBORGEN**

### Tech Mode (Developer View)
- ✅ Alle user mode features
- ✅ API status **ZICHTBAAR** (online/offline indicator)
- ✅ Debug panel met:
  - Session data
  - API logs
  - Reset progress
  - Complete random module
  - Refresh recommendations
  - View full profile
- ✅ Tech info panels (ML details, API calls, etc.)

## 🔧 API Integratie

De frontend communiceert met de Railway API:
- **Base URL**: `https://anti-scam-api-production.up.railway.app`

### Endpoints gebruikt:
```
GET  /health                    - API health check
POST /api/users/register        - Register nieuwe user
POST /api/recommendations       - Haal aanbevelingen op
GET  /api/modules               - Haal alle modules op
POST /api/ratings               - Submit module rating
```

### Data Format:

#### User Registration
```json
POST /api/users/register
{
  "ageGroup": "18-25" | "26-35" | "36-45" | "60+",
  "digitalLiteracy": 1-5 (integer),
  "preferredTopic": "phishing" | "bank" | "whatsapp" | "ai_voice" | "shopping"
}

Response:
{
  "userId": 1234,
  "profile": {
    "literacy": 3.0,
    "preferredTopic": "phishing",
    "ageGroup": "26-35",
    "riskProfile": "medium"
  }
}
```

#### Get Recommendations
```json
POST /api/recommendations
{
  "userId": 1234,
  "top": 6
}

Response:
{
  "userId": 1234,
  "userProfile": { ... },
  "recommendations": [
    {
      "moduleId": 5,
      "title": "WhatsApp Training Module",
      "hybridScore": 4.23,
      "collaborativeScore": 4.1,
      "contentScore": 4.5,
      "difficulty": 3,
      "scamType": "whatsapp",
      "targetLiteracy": 2,
      "durationMin": 7
    }
  ]
}
```

#### Submit Rating
```json
POST /api/ratings
{
  "userId": 1234,
  "moduleId": 5,
  "rating": 4.0
}

Response:
{
  "success": true,
  "message": "Rating submitted successfully for user 1234 on module 5"
}
```

## 📊 Data Correcties t.o.v. Originele Versie

### ✅ Gecorrigeerd:

1. **Leeftijdscategorieën**
   - ❌ Was: `18-25, 26-35, 36-45, 46-60, 60+`
   - ✅ Nu: `18-25, 26-35, 36-45, 60+` (matching CSV)

2. **Digital Literacy**
   - ❌ Was: String `"low", "medium", "high"`
   - ✅ Nu: Float/Integer `1, 2, 3, 4, 5` (matching CSV)

3. **API Endpoints**
   - ❌ Was: `POST /api/recommendations/register`
   - ✅ Nu: `POST /api/users/register`
   - ❌ Was: `POST /api/recommendations/rate`
   - ✅ Nu: `POST /api/ratings`

4. **Module Data**
   - ❌ Was: Hardcoded titles en descriptions
   - ✅ Nu: Metadata mapping via `scamType` uit CSV

5. **Rating Format**
   - ✅ Correct: Float `1.0 - 5.0`

6. **Scam Types**
   - ✅ Matching CSV: `phishing, bank, whatsapp, ai_voice, shopping`

## 🎨 UI Modes

Toggle tussen modes via de switch rechtsboven:

- **🤓 User Mode**: Clean interface voor eindgebruikers
- **🧑‍💻 Tech Mode**: Developer view met debug tools

## 📱 Responsive Design

- Desktop: Full grid layout
- Tablet: 2-column grid
- Mobile: Single column, stacked layout

## 🔒 Fallback Behavior

Als de API niet bereikbaar is:
- ✅ Lokale demo mode wordt geactiveerd
- ✅ Modules blijven zichtbaar
- ✅ Progress tracking werkt lokaal
- ✅ User kan onboarding voltooien
- ⚠️ Aanbevelingen zijn generiek (niet gepersonaliseerd)

## 📂 Bestandsstructuur

```
frontend/
├── index.html          # Main HTML met UI
├── app.js              # JavaScript logic & API calls
└── README.md           # Deze file
```

## 🐛 Debugging

**Tech Mode Debug Panel:**
1. Schakel naar Tech Mode (🧑‍💻 button)
2. Klik op "🔧 debug panel"
3. Bekijk:
   - Session data (userId, interests, completed modules, etc.)
   - API logs (laatste 50 calls)
   - Acties: reset, complete random, refresh, view profile

**Browser Console:**
- Open Developer Tools (F12)
- Check Console tab voor errors
- Check Network tab voor API calls

## 📝 Toekomstige Verbeteringen

- [ ] LocalStorage voor persistente progress
- [ ] Account systeem (login/logout)
- [ ] Social sharing van achievements
- [ ] Leaderboard
- [ ] Module bookmarks/favorites
- [ ] Email notificaties
- [ ] Multi-language support (Engels, Frans)

## 💡 Notes

- **API status is verborgen in User Mode** - alleen zichtbaar in Tech Mode
- Alle tech info panels zijn verborgen in User Mode
- Debug button is alleen zichtbaar in Tech Mode
- Onboarding kan overgeslagen worden (krijgt default profiel)
