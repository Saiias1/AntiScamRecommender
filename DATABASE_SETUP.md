# PostgreSQL Database Setup voor Railway

## ✅ Wat is al gedaan:

1. ✅ PostgreSQL database aangemaakt op Railway
2. ✅ Entity Framework Core packages geïnstalleerd
3. ✅ Database context & entities gemaakt
4. ✅ DatabaseService gebouwd (vervangt CSV DataService)
5. ✅ Database seeder gemaakt (importeert CSV data)
6. ✅ Program.cs ge

updated voor PostgreSQL

## 🚀 Railway Setup Stappen:

### Stap 1: Environment Variable instellen

1. Ga naar je Railway project
2. Selecteer je API service
3. Ga naar **Variables** tab
4. Je database URL staat al klaar als `DATABASE_URL`
   - Railway zet dit automatisch als de database is linked!
   - Formaat: `postgresql://user:password@host:port/database`

**Belangrijk**: Als `DATABASE_URL` er NIET is:
1. Klik op **"New Variable"**
2. Selecteer **"Add Reference"**
3. Kies je PostgreSQL database
4. Selecteer `DATABASE_URL`
5. Klik **Add**

### Stap 2: Deploy naar Railway

```bash
# Commit alle wijzigingen
git add .
git commit -m "Add PostgreSQL database support"
git push origin main
```

Railway zal automatisch:
1. ✅ Database tabellen aanmaken (`users`, `modules`, `ratings`)
2. ✅ CSV data importeren (1001 users, 30 modules, 20k ratings)
3. ✅ API starten

### Stap 3: Verificatie

Check de Railway logs:
```
✓ Ensuring database is created...
✓ Database ready
✓ Starting database seeding...
✓ Seeding modules...
✓ Seeded 30 modules
✓ Seeding users...
✓ Seeded 1000 users
✓ Seeding ratings...
✓ Seeded 1000/20000 ratings
✓ Seeded 2000/20000 ratings
...
✓ Seeded 20000 ratings total
✓ Database seeding completed successfully!
```

Test de API:
```bash
# Health check (moet database stats tonen)
curl https://jouw-api.up.railway.app/api/health

# Register nieuwe user
curl -X POST https://jouw-api.up.railway.app/api/users/register \
  -H "Content-Type: application/json" \
  -d '{"ageGroup":"26-35","digitalLiteracy":3,"preferredTopic":"phishing"}'

# Moet userId 1002 teruggeven (1001 zijn er al)
```

## 📊 Database Schema:

### Users Table
```sql
CREATE TABLE users (
    user_id BIGINT PRIMARY KEY,
    user_cluster INTEGER,
    digital_literacy REAL,
    age_group VARCHAR(20),
    risk_profile VARCHAR(20),
    preferred_topic VARCHAR(50)
);
```

### Modules Table
```sql
CREATE TABLE modules (
    module_id BIGINT PRIMARY KEY,
    scam_type VARCHAR(50),
    difficulty REAL,
    target_literacy REAL,
    duration_min REAL
);
```

### Ratings Table
```sql
CREATE TABLE ratings (
    id SERIAL PRIMARY KEY,
    user_id BIGINT,
    module_id BIGINT,
    rating REAL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_ratings_user_module ON ratings(user_id, module_id);
```

## 🔄 Data Flow:

### Bij eerste start:
1. API start → verbindt met PostgreSQL
2. `EnsureCreatedAsync()` → maakt tabellen aan als ze niet bestaan
3. `DatabaseSeeder` → checkt of modules.Count > 0
4. Als leeg → importeert CSV files:
   - `Data/users.csv` → users table (1000 rows)
   - `Data/modules.csv` → modules table (30 rows)
   - `Data/ratings.csv` → ratings table (20000 rows)
5. Bij volgende starts → skip seeding (data bestaat al)

### Bij nieuwe user registratie:
```
Frontend → POST /api/users/register
  ↓
Controller → DatabaseService.AddUserAsync()
  ↓
PostgreSQL → INSERT INTO users (...)
  ↓
Response ← { userId: 1002, profile: {...} }
```

### Bij nieuwe rating:
```
Frontend → POST /api/ratings
  ↓
Controller → DatabaseService.AddRatingAsync()
  ↓
PostgreSQL → INSERT INTO ratings (user_id, module_id, rating, created_at)
  ↓
ML Model → hertraint bij volgende recommendation request
```

## ⚠️ Belangrijke Notes:

1. **Data is NU persistent!** 🎉
   - Nieuwe users blijven bestaan na restart
   - Ratings worden opgeslagen in database
   - Geen data verlies meer!

2. **CSV files blijven nodig:**
   - Alleen voor INITIËLE seeding
   - Na eerste import → database is de bron
   - ML.NET model laadt data uit database via DatabaseService

3. **Performance:**
   - Railway free tier: 500MB database
   - 20k ratings = ~1MB
   - Ruimte voor 10 miljoen+ ratings

4. **Backup:**
   - Railway maakt automatisch backups
   - Export via: `pg_dump` (zie Railway docs)

## 🐛 Troubleshooting:

### "Could not connect to database"
- Check of DATABASE_URL variabele bestaat
- Ga naar Railway → PostgreSQL → Connect tab
- Kopieer connection string

### "Seeding failed"
- Check of Data/ folder bestaat in deployment
- Railway moet CSV files hebben
- Check `.dockerignore` of `Data/` niet excluded is

### "Table already exists"
- Normaal! Betekent database al seeded was
- Geen probleem - data blijft behouden

## 📈 Volgende Stappen:

Na deployment:
- [ ] Oude CSV DataService verwijderen (niet meer nodig)
- [ ] Controllers updaten om DatabaseService te gebruiken
- [ ] Frontend testen met nieuwe users
- [ ] Monitor Railway logs voor errors

## 💰 Kosten:

Railway Free Tier limits:
- ✅ 500MB PostgreSQL database (ruim genoeg)
- ✅ $5 credit/maand
- ✅ Unlimited deployment builds

Verwachte kosten: **€0/maand** (binnen free tier)
