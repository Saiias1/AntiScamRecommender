# 🚂 Railway Deployment Guide - Anti-Scam API

Complete guide voor deploying de ASP.NET Core Hybrid ML.NET API naar Railway.app

---

## 📋 Pre-Deployment Checklist

✅ Alle files aanwezig:
- `Dockerfile` - Multi-stage build voor .NET 9.0
- `.dockerignore` - Exclude build artifacts
- `railway.json` - Railway configuratie
- `Program.cs` - Updated met Railway PORT support
- `Data/` folder met:
  - `model.zip` (382 KB)
  - `modules.csv`
  - `users.csv`
  - `ratings.csv`

---

## 🚀 Deployment Stappen

### Stap 1: Railway Account Setup

1. **Ga naar [Railway.app](https://railway.app)**
2. **Sign up** met GitHub account (gratis tier: $5/maand credit)
3. **Verify** je email

### Stap 2: Nieuw Project Aanmaken

1. Click **"New Project"**
2. Kies **"Deploy from GitHub repo"**
3. **Authorize Railway** om toegang tot je GitHub repos
4. Als je repo nog niet op GitHub staat:

```bash
# In de API folder
cd "c:\Users\Admin\OneDrive - Thomas More\AI project\Succes\Model\AntiScamRecommender\API\AntiScamAPI"

# Initialize git (als nog niet gedaan)
git init
git add .
git commit -m "Initial commit: Hybrid ML.NET API"

# Create GitHub repo en push
# (Doe dit via GitHub.com → New Repository)
git remote add origin https://github.com/YOUR_USERNAME/anti-scam-api.git
git branch -M main
git push -u origin main
```

### Stap 3: Railway Configuratie

1. **Select je repository** in Railway
2. Railway detecteert automatisch de `Dockerfile`
3. **Root Directory**: Laat leeg (of stel in op `/API/AntiScamAPI` als je hele project pushed)
4. Click **"Deploy"**

### Stap 4: Environment Variables (Optioneel)

Railway gebruikt automatisch:
- `PORT` - Dynamisch toegewezen (Railway's default)
- `ASPNETCORE_ENVIRONMENT` - Automatisch ingesteld op `Production`

**Geen extra variables nodig!** De API werkt out-of-the-box.

### Stap 5: Deployment Monitoren

1. In Railway dashboard → **"Deployments"** tab
2. Volg de build logs:
   ```
   Building Dockerfile...
   ✓ Restore dependencies
   ✓ Build project
   ✓ Publish to /app
   ✓ Copy data files
   ✓ Container started on port 8080
   ```

3. Wacht tot status = **"Active"** (groen)

### Stap 6: Public Domain Genereren

1. In Railway project → **"Settings"** tab
2. Scroll naar **"Domains"**
3. Click **"Generate Domain"**
4. Railway geeft je een URL: `https://YOUR-APP.up.railway.app`

---

## ✅ Test Je Deployment

### Health Check
```bash
curl https://YOUR-APP.up.railway.app/api/health
```

**Verwacht:**
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

### Test Hybrid Recommendations
```bash
curl -X POST https://YOUR-APP.up.railway.app/api/recommendations \
  -H "Content-Type: application/json" \
  -d '{"UserId": 42, "Top": 3}'
```

### Test User Registration
```bash
curl -X POST https://YOUR-APP.up.railway.app/api/users/register \
  -H "Content-Type: application/json" \
  -d '{"AgeGroup": "26-35", "DigitalLiteracy": 3, "PreferredTopic": "phishing"}'
```

---

## 📊 Railway Dashboard Features

### Metrics (gratis tier)
- **CPU usage**
- **Memory usage**
- **Network traffic**
- **Request count**

### Logs
- Real-time logging
- Filter by level (Info/Warning/Error)
- Download logs

### Auto-Deploy
- Elke `git push` naar main branch → automatische redeploy
- Zero-downtime deployments

---

## 💰 Kosten (Railway Pricing)

**Hobby Plan (gratis tier):**
- $5/maand in credits (gratis)
- 500 execution hours/maand
- 512MB RAM per service
- 1GB disk

**Voor deze API:**
- Geschatte cost: **$2-3/maand** (goed binnen gratis tier!)
- Model: 382 KB (klein)
- CSVs: < 1 MB totaal
- Memory: ~200-300 MB runtime

---

## 🔧 Troubleshooting

### Build Failed: "model.zip not found"
**Fix:** Zorg dat `Data/` folder in de repo staat:
```bash
git add Data/model.zip Data/*.csv -f
git commit -m "Add data files"
git push
```

### Port Binding Error
**Fix:** Program.cs gebruikt nu Railway's PORT env var automatisch.
Check logs: `Railway Dashboard → Logs`

### 502 Bad Gateway
**Fix:** App crashed tijdens startup. Check logs voor errors:
```bash
# In Railway logs zoek naar:
"Loading hybrid recommendation service..."
"Model loaded successfully!"
```

### OOM (Out of Memory)
**Fix:** Upgrade naar Pro plan ($20/maand) voor meer RAM.

---

## 🔄 Update Deployment

Elke keer dat je code wijzigt:

```bash
# Make changes in code
git add .
git commit -m "Update: [beschrijf wijziging]"
git push

# Railway deploy automatisch!
```

Monitor deployment in Railway dashboard.

---

## 🌐 Custom Domain (Optioneel)

1. Railway → **Settings** → **Domains**
2. Click **"Custom Domain"**
3. Enter jouw domein: `api.yourdomain.com`
4. Add CNAME record in je DNS:
   ```
   CNAME: api.yourdomain.com → YOUR-APP.up.railway.app
   ```
5. SSL certificaat = automatisch (Let's Encrypt)

---

## 📝 API Endpoints (Production)

Vervang `https://YOUR-APP.up.railway.app` met jouw Railway URL:

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/health` | GET | Health check |
| `/api/recommendations` | POST | Hybrid recommendations |
| `/api/users/register` | POST | Register user |
| `/api/users/{id}` | GET | Get user profile |
| `/api/modules` | GET | Get all modules |
| `/api/modules/{id}` | GET | Get module details |
| `/api/ratings` | POST | Submit rating |

---

## 🎯 Production Checklist

✅ **CORS configuratie**: AllowAll (OK voor testing, change voor productie!)
✅ **HTTPS**: Automatisch via Railway
✅ **Environment**: Production mode
✅ **PORT binding**: Railway's dynamic PORT
✅ **Data files**: Included in Docker image
✅ **Model loading**: Singleton (laadt 1x bij startup)
✅ **Error handling**: Proper try-catch in controllers
✅ **Logging**: ASP.NET Core logging → Railway logs

---

## 🚨 Security Considerations (Voor Productie)

**CORS - Update voor productie:**
```csharp
// In Program.cs - verander van AllowAll naar specific origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://yourdomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

**Rate Limiting** (optioneel):
```bash
# Install package
dotnet add package AspNetCoreRateLimit

# Add in Program.cs
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options => {
    options.GeneralRules = new List<RateLimitRule> {
        new RateLimitRule {
            Endpoint = "*",
            Limit = 100,
            Period = "1m"
        }
    };
});
```

---

## 📞 Support & Resources

- **Railway Docs**: https://docs.railway.app
- **Railway Discord**: https://discord.gg/railway
- **API Issues**: Check Railway logs en GitHub Issues

---

## ✅ Next Steps

1. ☐ Deploy naar Railway
2. ☐ Test alle endpoints met production URL
3. ☐ Monitor performance in Railway dashboard
4. ☐ (Optioneel) Setup custom domain
5. ☐ (Optioneel) Add API documentation (Swagger)
6. ☐ Share API URL met frontend team!

---

**Deployment Date**: 24 December 2025
**Framework**: .NET 9.0
**Platform**: Railway.app
**Region**: Auto (nearest to user)

🎉 **Je API is production-ready!**
