# ✅ Railway Deployment Checklist

Volg deze stappen om je API live te krijgen op Railway.app

---

## 📦 Pre-Deployment (Lokaal)

- [ ] **Dockerfile exists** in `/API/AntiScamAPI/`
- [ ] **.dockerignore exists** (excludes bin/obj)
- [ ] **railway.json exists** (Railway config)
- [ ] **Program.cs updated** met Railway PORT support
- [ ] **Data files present**:
  - [ ] `Data/model.zip` (382 KB)
  - [ ] `Data/modules.csv`
  - [ ] `Data/users.csv`
  - [ ] `Data/ratings.csv`
- [ ] **Local test passed**: `dotnet run` werkt zonder errors
- [ ] **Health endpoint werkt**: `curl http://localhost:5000/api/health`

---

## 🐙 Git Repository Setup

```bash
# Navigate to API folder
cd "c:\Users\Admin\OneDrive - Thomas More\AI project\Succes\Model\AntiScamRecommender\API\AntiScamAPI"

# Initialize git (if not done)
git init

# Add all files
git add .

# Commit
git commit -m "Initial commit: Anti-Scam API ready for deployment"
```

### Create GitHub Repository

1. Go to **https://github.com/new**
2. Repository name: `anti-scam-api` (or any name)
3. Visibility: **Public** of **Private**
4. **Do NOT** initialize with README (je hebt al code)
5. Click **"Create repository"**

### Push to GitHub

```bash
# Add remote (replace YOUR_USERNAME)
git remote add origin https://github.com/YOUR_USERNAME/anti-scam-api.git

# Push to main branch
git branch -M main
git push -u origin main
```

**Checklist:**
- [ ] GitHub repo created
- [ ] Code pushed to main branch
- [ ] Data files included in repo (check on GitHub)

---

## 🚂 Railway Deployment

### Step 1: Create Railway Account

1. Go to **https://railway.app**
2. Click **"Start a New Project"**
3. Sign up met **GitHub** (recommended)
4. Authorize Railway om je GitHub repos te zien

**Checklist:**
- [ ] Railway account created
- [ ] Email verified
- [ ] GitHub connected

### Step 2: Create New Project

1. Click **"+ New Project"**
2. Select **"Deploy from GitHub repo"**
3. Find en select `anti-scam-api` (of jouw repo naam)
4. Railway detecteert automatisch de Dockerfile

**Checklist:**
- [ ] Project created in Railway
- [ ] Repository connected

### Step 3: Configure Build

**Railway should auto-detect:**
- ✅ Builder: **DOCKERFILE**
- ✅ Root Directory: **/** (or `/API/AntiScamAPI` if you pushed whole project)

**If not auto-detected:**
1. Click **Settings** tab
2. Under **Build**, set:
   - Builder: **Dockerfile**
   - Dockerfile Path: `Dockerfile`
   - Root Directory: Leave empty (of `/API/AntiScamAPI`)

**Checklist:**
- [ ] Dockerfile detected
- [ ] Build settings correct

### Step 4: Deploy!

1. Click **"Deploy"** button (or it starts automatically)
2. Watch build logs in **"Deployments"** tab
3. Wait for deployment status: **"Success"** ✅

**Checklist:**
- [ ] Build started
- [ ] Build logs show:
  ```
  ✓ Restore dependencies
  ✓ Publish project
  ✓ Copy data files
  ✓ Container started
  ```
- [ ] Deployment status = **Active** (green)

### Step 5: Generate Public URL

1. Click **"Settings"** tab
2. Scroll to **"Domains"** section
3. Click **"Generate Domain"**
4. Railway gives you: `https://your-app-name.up.railway.app`

**Checklist:**
- [ ] Public domain generated
- [ ] Copy URL (je hebt het nodig voor testing!)

---

## 🧪 Post-Deployment Testing

Replace `YOUR_URL` with je Railway URL.

### Test 1: Health Check
```bash
curl https://YOUR_URL/api/health
```

**Expected Response:**
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

**Checklist:**
- [ ] Health check returns 200 OK
- [ ] Model loaded = true
- [ ] Data files loaded = true

### Test 2: Get Recommendations
```bash
curl -X POST https://YOUR_URL/api/recommendations \
  -H "Content-Type: application/json" \
  -d '{"UserId": 42, "Top": 3}'
```

**Expected:** JSON with 3 recommendations

**Checklist:**
- [ ] Recommendations endpoint werkt
- [ ] Response toont hybridScore, collaborativeScore, contentScore
- [ ] No errors in Railway logs

### Test 3: Register User
```bash
curl -X POST https://YOUR_URL/api/users/register \
  -H "Content-Type: application/json" \
  -d '{"AgeGroup": "26-35", "DigitalLiteracy": 3, "PreferredTopic": "phishing"}'
```

**Expected:** `{"userId": 1001, "profile": {...}}`

**Checklist:**
- [ ] User registration werkt
- [ ] Profile fields are filled (not empty!)

### Test 4: New User Recommendations
```bash
curl -X POST https://YOUR_URL/api/recommendations \
  -H "Content-Type: application/json" \
  -d '{"UserId": 1001, "Top": 3}'
```

**Expected:** Content-based recommendations (collaborativeScore = 0)

**Checklist:**
- [ ] New user gets recommendations (no crash!)
- [ ] collaborativeScore = 0
- [ ] contentScore > 0
- [ ] hybridScore = contentScore

---

## 📊 Monitoring (Railway Dashboard)

### Metrics Tab
- [ ] CPU usage < 50%
- [ ] Memory usage < 400 MB
- [ ] No crashes in logs

### Logs Tab
Check for startup messages:
```
info: Program[0]
      Loading hybrid recommendation service...
info: Program[0]
      Hybrid recommendation service loaded successfully!
info: Program[0]
      Configuration: 70% Collaborative + 30% Content-Based
```

**Checklist:**
- [ ] Startup logs show successful model loading
- [ ] No errors in logs
- [ ] Application running smoothly

---

## 🎯 Final Checklist

- [ ] ✅ API deployed to Railway
- [ ] ✅ Public URL generated
- [ ] ✅ All endpoints tested (health, recommendations, users, ratings)
- [ ] ✅ Hybrid system working (collaborative + content-based)
- [ ] ✅ Cold start problem solved (new users get content-based recs)
- [ ] ✅ No crashes or errors
- [ ] ✅ Railway logs clean
- [ ] ✅ URL shared met team/stakeholders

---

## 📝 Your Deployment Info

Fill this in after deployment:

**Railway Project Name:** `_______________________`

**Public URL:** `https://_______________________.up.railway.app`

**Deployment Date:** `_______________________`

**Region:** `_______________________` (check Railway dashboard)

**Status:** [ ] Active / [ ] Paused / [ ] Failed

---

## 🆘 Troubleshooting

### Build Failed: "Dockerfile not found"
**Fix:**
```bash
# Make sure Dockerfile is in root of your repo
git add Dockerfile
git commit -m "Add Dockerfile"
git push
```

### Build Failed: "model.zip not found"
**Fix:**
```bash
# Force add Data files (might be in .gitignore)
git add -f Data/model.zip Data/*.csv
git commit -m "Add data files"
git push
```

### Runtime Error: "Port binding failed"
**Fix:** Check Program.cs has Railway PORT config (should be already done!)

### 502 Bad Gateway
**Fix:**
1. Check Railway logs for crash
2. Look for: "Loading hybrid recommendation service..."
3. If model load failed, check Data files exist in container

---

## 🔄 Update Deployment

Elke keer dat je code wijzigt:

```bash
git add .
git commit -m "Update: [beschrijf wijziging]"
git push
```

Railway deploy automatisch! Monitor in dashboard.

---

## ✨ Next Steps

After successful deployment:

1. [ ] Share API URL met frontend developers
2. [ ] Add API documentation (optional: Swagger)
3. [ ] Setup custom domain (optional)
4. [ ] Monitor usage in Railway dashboard
5. [ ] Consider upgrading to Pro if needed (more resources)

---

**Congratulations! 🎉**

Je Anti-Scam Hybrid ML.NET API is nu live op Railway!

URL: `https://your-app.up.railway.app`

Test alle endpoints en share de URL! 🚀
