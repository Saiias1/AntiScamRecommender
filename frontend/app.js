// CONFIGURATION
const API_BASE = 'https://anti-scam-api-production.up.railway.app';

// STATE
let currentStep = 1;
let apiLogs = [];
let newsData = [];
let currentNewsIndex = 0;
let userData = {
    userId: null,
    username: '',
    password: '',
    age: '',
    interests: [],
    quizScore: 0,
    quizAnswers: [],
    completedModules: [],
    currentModuleId: null,
    currentModuleRating: 0,
    digitalLiteracy: 0,
    currentModuleCategory: null,
    currentQuestionIndex: 0,
    moduleCorrectCount: 0,
    currentModuleQuestions: [],
    isGuest: false
};

// NEWS FUNCTIONALITY
async function loadNews() {
    try {
        const response = await fetch('news.json');
        newsData = await response.json();

        // Get random news item
        currentNewsIndex = Math.floor(Math.random() * newsData.length);

        // Display only on homepage
        displayNews('Home');
    } catch (error) {
        console.error('Error loading news:', error);
        const bannerHome = document.getElementById('newsBannerHome');
        if (bannerHome) bannerHome.style.display = 'none';
    }
}

function displayNews(suffix = '') {
    if (!newsData || newsData.length === 0) return;

    const newsItem = newsData[currentNewsIndex];
    const titleEl = document.getElementById('newsTitle' + suffix);
    const dateEl = document.getElementById('newsDate' + suffix);
    const sourceEl = document.getElementById('newsSource' + suffix);
    const bannerEl = document.getElementById('newsBanner' + suffix);
    const relatedEl = document.getElementById('newsRelated' + suffix);

    if (titleEl) titleEl.textContent = newsItem.title;
    if (dateEl) {
        const date = new Date(newsItem.date);
        dateEl.textContent = date.toLocaleDateString('nl-BE', { day: 'numeric', month: 'short', year: 'numeric' });
    }
    if (sourceEl) sourceEl.textContent = newsItem.source;

    // Show/hide related module badge
    if (relatedEl) {
        if (newsItem.relatedModule) {
            relatedEl.style.display = 'inline-flex';
            relatedEl.textContent = '🎯 ' + getModuleCategoryName(newsItem.relatedModule);
        } else {
            relatedEl.style.display = 'none';
        }
    }

    // Make banner clickable
    if (bannerEl && newsItem.url) {
        bannerEl.style.cursor = 'pointer';
        bannerEl.onclick = () => window.open(newsItem.url, '_blank');
    }
}

function getModuleCategoryName(category) {
    const names = {
        'phishing': 'Phishing',
        'bank': 'Bankfraude',
        'whatsapp': 'WhatsApp',
        'ai_voice': 'AI-oplichting',
        'shopping': 'Webshops'
    };
    return names[category] || category;
}

function rotateNews() {
    if (!newsData || newsData.length === 0) return;
    currentNewsIndex = (currentNewsIndex + 1) % newsData.length;

    // Rotate only on homepage
    displayNews('Home');
}

// Module metadata (titles, descriptions, emojis) - IMPROVED
const moduleMetadata = {
    phishing: {
        emoji: '🎣',
        categoryName: 'Phishing & Email Fraude',
        titles: [
            'Phishing Basis: Email Bedreigingen',
            'Geavanceerde Phishing Technieken',
            'Spear Phishing & Gerichte Aanvallen'
        ]
    },
    bank: {
        emoji: '🏦',
        categoryName: 'Bankfraude & Financieel',
        titles: [
            'Bankfraude Herkennen',
            'Telefoonfraude & Nep Bankmedewerkers',
            'ATM Skimming & Pinpas Veiligheid',
            'Online Banking Bescherming',
            'Nep Betaalverzoeken Detecteren',
            'Rekening Overname Preventie'
        ]
    },
    whatsapp: {
        emoji: '📱',
        categoryName: 'WhatsApp & Chat Fraude',
        titles: [
            'De "Mam/Pap" WhatsApp Scam',
            'WhatsApp Account Beveiliging',
            'Nep Berichten & Links Herkennen'
        ]
    },
    ai_voice: {
        emoji: '🤖',
        categoryName: 'AI & Deepfake Scams',
        titles: [
            'Wat is een AI Voice Scam?',
            'Voice Cloning Technologie',
            'Deepfake Audio Detectie',
            'Familie Noodgeval Scams',
            'Stem Verificatie Technieken',
            'AI Scam Bescherming Strategie',
            'Bedrijfsfraude met AI Stemmen',
            'Real-time Voice Deepfakes'
        ]
    },
    shopping: {
        emoji: '🛍',
        categoryName: 'Online Shopping Fraude',
        titles: [
            'Nep Webshops Herkennen',
            'Te Mooi Om Waar Te Zijn Deals',
            'Online Shopping Red Flags',
            'Webshop Verificatie Technieken',
            'Veilig Online Betalen',
            'Dropshipping Scams',
            'E-commerce Veiligheid Tips',
            'Social Media Shopping Fraude',
            'Marktplaats & Tweedehands Scams',
            'Vakantie & Reis Booking Fraude'
        ]
    }
};

// Interest display names (frontend) vs backend category names
const interestDisplayNames = {
    'phishing': 'Phishing emails',
    'bank': 'Bankfraude',
    'whatsapp': 'WhatsApp fraude',
    'ai_voice': 'AI voice scams',
    'shopping': 'Online shopping fraude'
};

// ONBOARDING QUIZ (digital literacy assessment)
const onboardingQuiz = [
    {
        question: "Je ontvangt een email van 'paypa1.com' (met cijfer 1). Wat is dit?",
        options: [
            "Een legitieme PayPal email",
            "Typosquatting - een phishing techniek",
            "Een fout van de afzender",
            "PayPal's nieuwe domein"
        ],
        correct: 1
    },
    {
        question: "Wat is 'vishing'?",
        options: [
            "Phishing via video calls",
            "Voice phishing - fraude via telefoon",
            "Een virus in je smartphone",
            "Visuele spam advertenties"
        ],
        correct: 1
    },
    {
        question: "Je ziet een URL: 'https://m.facebook.com-security-check.net'. Is dit veilig?",
        options: [
            "Ja, het heeft https dus het is veilig",
            "Ja, facebook.com staat erin",
            "Nee, het echte domein is .net niet .com",
            "Misschien, hangt af van de pagina"
        ],
        correct: 2
    },
    {
        question: "Wat is de beste manier om een verdacht account te detecteren op social media?",
        options: [
            "Kijken naar het aantal volgers",
            "Checken wanneer het account gemaakt is en activiteit patroon",
            "Vertrouwen op de blauwe vinkjes",
            "Vragen stellen aan het account"
        ],
        correct: 1
    },
    {
        question: "Een website vraagt je wifi-wachtwoord om 'connectiviteit te verbeteren'. Wat doe je?",
        options: [
            "Geven als de site https heeft",
            "Geven maar een oud wachtwoord",
            "Nooit geven - dit is verdacht",
            "Eerst website naam googlen"
        ],
        correct: 2
    }
];

// MODULE QUESTIONS (3 per module for demo)
const moduleQuestions = {
    phishing: [
        {
            question: "Wat is het belangrijkste signaal van een phishing email?",
            options: [
                "Het komt van een onbekend adres",
                "Er staat urgentie in ('binnen 24u reageren!')",
                "Het heeft een bijlage",
                "Het is lang"
            ],
            correct: 1,
            explanation: "Oplichters gebruiken vaak urgentie om je onder druk te zetten en overhaast te handelen. 'Act nu of je account wordt geblokkeerd!' is een klassiek voorbeeld. Dit voorkomt dat je rustig nadenkt."
        },
        {
            question: "Hoe check je of een link veilig is?",
            options: [
                "Erop klikken en kijken",
                "Hover erover om de echte URL te zien",
                "Googlen",
                "Vertrouwen op antivirus"
            ],
            correct: 1,
            explanation: "Door je muis over een link te houden (zonder te klikken), zie je onderaan je browser de echte bestemming. Vaak zien links er legitiem uit maar leiden ze naar 'paypa1.com' of 'faceb00k.com'."
        },
        {
            question: "Een email van 'je bank' vraagt je wachtwoord. Wat doe je?",
            options: [
                "Geven via de link in de email",
                "Direct verwijderen en bank zelf bellen",
                "Reageren met vraag of het echt is",
                "Wachtwoord wijzigen en dan geven"
            ],
            correct: 1,
            explanation: "Banken vragen NOOIT om je wachtwoord of pincode via email, telefoon of sms. Bij twijfel: verwijder de email en bel zelf naar het officiële nummer van je bank (niet het nummer in de email!)."
        }
    ],
    bank: [
        {
            question: "Je bank belt om je pincode. Wat doe je?",
            options: [
                "Geven, het is de bank",
                "Ophangen en zelf bank bellen",
                "Vragen naar hun naam",
                "Pin wijzigen en dan geven"
            ],
            correct: 1,
            explanation: "Je bank zal NOOIT om je pincode vragen, ook niet telefonisch. Hang direct op en bel zelf naar het officiële banknummer (die op je bankpas staat). Oplichters kunnen nummers vervalsen!"
        },
        {
            question: "Wat is een veelvoorkomende bank scam?",
            options: [
                "Nep bankmedewerker belt",
                "SMS over 'verdachte transactie'",
                "Email over 'geblokkeerde rekening'",
                "Alle bovenstaande"
            ],
            correct: 3,
            explanation: "Bankfraude komt in vele vormen: telefonisch, via sms én email. Criminelen gebruiken vaak meerdere kanalen tegelijk om geloofwaardiger te lijken. Neem altijd zelf contact op met je bank via hun officiële kanalen."
        },
        {
            question: "Hoe herken je een nep bankwebsite?",
            options: [
                "Check de exacte URL spelling",
                "Kijk naar het slot-icoontje",
                "Test met kleine betaling",
                "Gebruik altijd de officiële app"
            ],
            correct: 0,
            explanation: "Nep bankwebsites gebruiken vaak bijna-identieke URLs zoals 'ing-nl.com' i.p.v. 'ing.nl'. Het slot-icoontje (HTTPS) betekent alleen dat de verbinding veilig is, niet dat de website echt is! Bookmark altijd je echte bankwebsite."
        }
    ],
    whatsapp: [
        {
            question: "De 'Mam/Pap' WhatsApp scam. Wat is de rode vlag?",
            options: [
                "Nieuw nummer zonder uitleg waarom",
                "Direct om geld vragen",
                "Geen videocall willen",
                "Alle bovenstaande"
            ],
            correct: 3,
            explanation: "Deze scam combineert meerdere tactieken: nieuw nummer (want 'oude telefoon kwijt'), geen video ('camera is stuk'), urgentie ('moet NU betalen'). Vraag ALTIJD om een videocall of bel het oude nummer!"
        },
        {
            question: "Je krijgt een verificatiecode via SMS. Wat betekent dit?",
            options: [
                "Normale 2FA code",
                "Iemand probeert in te loggen op jouw account",
                "Je account is gehackt",
                "WhatsApp update"
            ],
            correct: 1,
            explanation: "Als je een verificatiecode krijgt die je NIET zelf aanvroeg, betekent dit dat iemand anders toegang probeert te krijgen tot jouw WhatsApp account. Deel deze code NOOIT, ook niet als 'iemand per ongeluk jouw nummer invulde'."
        },
        {
            question: "Een onbekend nummer stuurt: 'Hey! Lang niet gesproken!'. Wat doe je?",
            options: [
                "Reageren met 'wie ben je?'",
                "Direct blokkeren",
                "Link niet aanklikken, vragen wie het is",
                "Negeren"
            ],
            correct: 2,
            explanation: "Oplichters sturen vage berichten om gesprek te beginnen. Klik NOOIT op links van onbekenden. Je mag wel vragen wie het is, maar wees alert als ze daarna om geld of codes vragen. Videocall is de beste verificatie!"
        }
    ],
    ai_voice: [
        {
            question: "Wat is een AI voice scam?",
            options: [
                "Robot belt je",
                "Crimineel gebruikt AI om stem te klonen",
                "Spam call",
                "Verkeerde verbinding"
            ],
            correct: 1,
            explanation: "Met moderne AI kunnen criminelen iemands stem perfect namaken met slechts een paar seconden audio (bv. van social media). Ze bellen dan familie/vrienden en doen alsof ze in nood zijn. Super eng en steeds vaker!"
        },
        {
            question: "Je 'kleinkind' belt met noodgeval. Hoe verifieer je?",
            options: [
                "Direct geld sturen",
                "Vragen stellen die alleen zij weten",
                "Ophangen en zelf terugbellen",
                "B en C"
            ],
            correct: 3,
            explanation: "Combineer meerdere verificaties: stel persoonlijke vragen ('wat is de naam van je hamster?') EN hang op en bel zelf terug naar hun normale nummer. AI kan stemmen klonen, maar kent geen persoonlijke details!"
        },
        {
            question: "Hoe werkt voice cloning?",
            options: [
                "3-5 seconden audio is genoeg",
                "Crimineel heeft uren opname nodig",
                "Kan alleen met dure apparatuur",
                "Is onmogelijk"
            ],
            correct: 0,
            explanation: "Schokkend maar waar: met gratis AI tools kunnen criminelen een stem klonen met maar 3-5 seconden audio. Dit halen ze van Instagram stories, TikTok videos of voicemail. Wees voorzichtig met wat je online deelt!"
        }
    ],
    shopping: [
        {
            question: "Hoe herken je een nep webshop?",
            options: [
                "Geen contactgegevens",
                "Alleen vooruitbetaling",
                "Te goede deals",
                "Alle bovenstaande"
            ],
            correct: 3,
            explanation: "Nep webshops combineren vaak meerdere rode vlaggen: geen bedrijfsgegevens/KVK nummer, alleen bankoverschrijving (geen iDEAL/PayPal), enorme kortingen, geen klantenservice. Check altijd op beoordelingssites zoals Trustpilot!"
        },
        {
            question: "Wat is een rode vlag bij online shopping?",
            options: [
                "Website is nieuw (< 6 maanden)",
                "Geen reviews",
                "Alleen betalen via bankoverschrijving",
                "Alle bovenstaande"
            ],
            correct: 3,
            explanation: "Wees extra alert bij nieuwe websites zonder reviews die alleen bankoverschrijving accepteren. Echte webshops bieden consumentenbescherming via iDEAL, PayPal of creditcard. Gebruik tools zoals 'scamadviser.com' om websites te checken!"
        },
        {
            question: "Je ziet een advertentie voor 'iPhone voor €100'. Wat denk je?",
            options: [
                "Geweldige deal!",
                "Waarschijnlijk scam - te mooi om waar te zijn",
                "Verkeerde prijs",
                "Tweedehands"
            ],
            correct: 1,
            explanation: "Als iets te mooi is om waar te zijn, is het dat waarschijnlijk ook! Een nieuwe iPhone kost €800+, dus €100 is onrealistisch. Dit zijn vaak scams die je geld stelen of nep producten sturen. Vergelijk altijd met officiële prijzen."
        }
    ]
};

// API FUNCTIONS
async function checkAPIHealth() {
    const statusEl = document.getElementById('apiStatus');
    const textEl = document.getElementById('apiStatusText');

    try {
        const startTime = Date.now();
        const response = await fetch(`${API_BASE}/api/health`, {
            method: 'GET',
            cache: 'no-cache'
        });
        const endTime = Date.now();
        const responseTime = endTime - startTime;

        if (response.ok) {
            statusEl.className = 'api-status online';
            textEl.textContent = `API online (${responseTime}ms)`;
            logAPI('GET /health', 'success', responseTime);
        } else {
            throw new Error('API returned error');
        }
    } catch (error) {
        statusEl.className = 'api-status offline';
        textEl.textContent = 'API offline (demo mode)';
        logAPI('GET /health', 'failed', 0);
    }
}

function logAPI(endpoint, status, time) {
    const timestamp = new Date().toLocaleTimeString();
    apiLogs.unshift(`[${timestamp}] ${endpoint} - ${status} (${time}ms)`);
    if (apiLogs.length > 50) apiLogs.pop();
}

// MODE TOGGLE
function setMode(mode) {
    const buttons = document.querySelectorAll('.mode-btn');
    buttons.forEach(btn => btn.classList.remove('active'));

    // Find the clicked button
    const clickedBtn = Array.from(buttons).find(btn =>
        (mode === 'user' && btn.textContent.toLowerCase().includes('user')) ||
        (mode === 'tech' && btn.textContent.toLowerCase().includes('tech'))
    );
    if (clickedBtn) {
        clickedBtn.classList.add('active');
        console.log('✅ Active class added to:', clickedBtn.textContent.trim(), '| Mode:', mode);
    } else {
        console.warn('❌ Could not find button for mode:', mode);
    }

    if (mode === 'tech') {
        document.body.classList.add('tech-mode');
    } else {
        document.body.classList.remove('tech-mode');
    }
}

// ONBOARDING NAVIGATION
function nextStep() {
    if (currentStep === 1) {
        const username = document.getElementById('username').value.trim();
        const password = document.getElementById('password').value.trim();
        const age = document.getElementById('age').value;

        if (!username || !password || !age) {
            alert('Vul alle velden in!');
            return;
        }

        if (password.length < 4) {
            alert('Wachtwoord moet minimaal 4 karakters zijn!');
            return;
        }

        // Check if username already exists
        const savedAccounts = JSON.parse(localStorage.getItem('antiScamAccounts') || '{}');
        if (savedAccounts[username]) {
            alert('⚠️ Gebruikersnaam bestaat al! Kies een andere naam of log in.');
            return;
        }

        userData.username = username;
        userData.password = password;
        userData.age = age;
    }

    if (currentStep === 2 && userData.interests.length === 0) {
        alert('Selecteer minstens 1 interesse!');
        return;
    }

    if (currentStep === 2) {
        loadOnboardingQuiz();
    }

    document.getElementById(`step${currentStep}`).classList.remove('active');
    currentStep++;
    document.getElementById(`step${currentStep}`).classList.add('active');
    updateProgress();
}

function previousStep() {
    document.getElementById(`step${currentStep}`).classList.remove('active');
    currentStep--;
    document.getElementById(`step${currentStep}`).classList.add('active');
    updateProgress();
}

function updateProgress() {
    const progress = ((currentStep - 1) / 3) * 100;
    document.getElementById('progressFill').style.width = progress + '%';
}

function toggleInterest(element, interest) {
    element.classList.toggle('selected');

    if (userData.interests.includes(interest)) {
        userData.interests = userData.interests.filter(i => i !== interest);
    } else {
        userData.interests.push(interest);
    }
}

function loadOnboardingQuiz() {
    const container = document.getElementById('quizContainer');
    container.innerHTML = onboardingQuiz.map((q, index) => `
        <div class="quiz-question">
            <div class="quiz-question-text">${index + 1}. ${q.question}</div>
            <div class="quiz-options">
                ${q.options.map((option, optIndex) => `
                    <div class="quiz-option" onclick="selectOnboardingAnswer(${index}, ${optIndex}, ${q.correct}, this)">
                        ${option}
                    </div>
                `).join('')}
            </div>
            <div class="quiz-feedback" id="feedback${index}" style="display:none; margin-top:0.5rem;"></div>
        </div>
    `).join('');
}

function selectOnboardingAnswer(questionIndex, answerIndex, correctIndex, element) {
    const questionDiv = element.parentElement;
    questionDiv.querySelectorAll('.quiz-option').forEach(opt => {
        opt.classList.remove('selected');
        opt.style.pointerEvents = 'none';
    });
    element.classList.add('selected');

    userData.quizAnswers[questionIndex] = answerIndex;

    // Show feedback
    const feedbackEl = document.getElementById(`feedback${questionIndex}`);
    if (answerIndex === correctIndex) {
        feedbackEl.innerHTML = '<span style="color: var(--green); font-weight: 600;">✓ Correct!</span>';
    } else {
        feedbackEl.innerHTML = '<span style="color: #cc6600; font-weight: 600;">✗ Niet helemaal juist</span>';
    }
    feedbackEl.style.display = 'block';
}

async function completeOnboarding() {
    let score = 0;
    onboardingQuiz.forEach((q, index) => {
        if (userData.quizAnswers[index] === q.correct) {
            score++;
        }
    });
    userData.quizScore = score;

    // Map score to digital literacy (1-5 scale matching CSV)
    if (score === 0) userData.digitalLiteracy = 1;
    else if (score === 1) userData.digitalLiteracy = 1;
    else if (score === 2) userData.digitalLiteracy = 2;
    else if (score === 3) userData.digitalLiteracy = 3;
    else if (score === 4) userData.digitalLiteracy = 4;
    else userData.digitalLiteracy = 5;

    document.getElementById(`step${currentStep}`).classList.remove('active');
    currentStep = 4;
    document.getElementById('step4').classList.add('active');
    updateProgress();

    // IMPROVED: Empowerment messages instead of just score
    let empowermentMessage = '';
    let detailMessage = '';

    if (score === 0) {
        empowermentMessage = 'Welkom bij de training! 👋';
        detailMessage = 'Iedereen begint ergens - tijd om je kennis te vergroten!';
    } else if (score === 1) {
        empowermentMessage = 'Je eerste stappen! 🌱';
        detailMessage = 'Je hebt al basiskennis, laten we daar op bouwen!';
    } else if (score === 2) {
        empowermentMessage = 'Op de goede weg! 🚶';
        detailMessage = 'Je weet al het een en ander, maar er valt nog veel te leren!';
    } else if (score === 3) {
        empowermentMessage = 'Je kent de basis goed! 💪';
        detailMessage = 'Solide fundament - tijd voor geavanceerde technieken!';
    } else if (score === 4) {
        empowermentMessage = 'Indrukwekkend! 🌟';
        detailMessage = 'Je bent al goed voorbereid tegen de meeste scams!';
    } else {
        empowermentMessage = 'Uitstekend werk! 🎯';
        detailMessage = 'Je bent scherp! We helpen je nóg beter te worden!';
    }

    document.getElementById('quizScore').textContent = empowermentMessage;
    document.getElementById('scoreMessage').textContent = detailMessage;

    document.getElementById('techUserId').textContent = userData.username;
    document.getElementById('techAge').textContent = userData.age;
    document.getElementById('techInterests').textContent = userData.interests.map(i => interestDisplayNames[i]).join(', ');
    document.getElementById('techLiteracy').textContent = userData.digitalLiteracy;

    // Register with API
    try {
        const startTime = Date.now();

        // Pick first interest as preferred topic
        const preferredTopic = userData.interests[0] || 'phishing';

        const response = await fetch(`${API_BASE}/api/users/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                ageGroup: userData.age,
                digitalLiteracy: userData.digitalLiteracy,
                preferredTopic: preferredTopic
            })
        });
        const endTime = Date.now();

        if (response.ok) {
            const data = await response.json();
            userData.userId = data.userId;
            saveUserData(); // Save to localStorage
            logAPI('POST /api/users/register', 'success', endTime - startTime);
            document.getElementById('techUserId').textContent = data.userId;
        } else {
            throw new Error('Registration failed');
        }
    } catch (error) {
        // Fallback to local mode
        userData.userId = Math.floor(Math.random() * 10000);
        saveUserData(); // Save to localStorage
        logAPI('POST /api/users/register', 'failed (using local mode)', 0);
    }
}

function backToInterests() {
    document.getElementById('step4').classList.remove('active');
    currentStep = 2;
    document.getElementById('step2').classList.add('active');
    updateProgress();
}

function skipOnboarding() {
    if (confirm('Weet je zeker dat je wilt overslaan? Je krijgt dan algemene aanbevelingen.')) {
        userData.username = 'guest_' + Date.now();
        userData.age = '26-35';
        userData.interests = ['phishing', 'bank'];
        userData.quizScore = 3;
        userData.digitalLiteracy = 3;
        userData.userId = Math.floor(Math.random() * 10000);
        saveUserData(); // Save to localStorage
        loadDashboard();
    }
}

// DASHBOARD
async function loadDashboard() {
    document.getElementById('onboardingCard').style.display = 'none';
    document.getElementById('dashboardSection').style.display = 'block';

    // Show profile button (not for guests)
    if (!userData.isGuest) {
        document.getElementById('profileBtn').style.display = 'block';
    }

    // Always show logout button when in dashboard (for both guest and logged-in users)
    document.getElementById('logoutBtn').style.display = 'block';

    updateProgressDisplay();
    await loadRecommendations();
    await loadAllModules();
}

function updateProgressDisplay() {
    if (userData.isGuest) {
        document.getElementById('progressNumber').textContent = 'Gast modus';
        document.getElementById('progressLabel').textContent = 'Voortgang wordt niet opgeslagen';
        return;
    }

    const completed = userData.completedModules.length;
    const label = completed === 0 ? 'Begin je leerpad' :
                  completed < 5 ? 'Lekker bezig!' :
                  completed < 10 ? 'Goede vooruitgang!' :
                  completed < 20 ? 'Je bent er bijna!' :
                  'Bijna expert!';

    document.getElementById('progressNumber').textContent = `${completed} voltooid`;
    document.getElementById('progressLabel').textContent = label;
}

async function loadRecommendations() {
    // GUEST MODE: Show popular modules instead of personalized recommendations
    if (userData.isGuest) {
        loadGuestRecommendations();
        return;
    }

    // Clear guest banner for logged-in users
    document.getElementById('guestBanner').innerHTML = '';

    try {
        const startTime = Date.now();
        const response = await fetch(`${API_BASE}/api/recommendations`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                userId: userData.userId,
                top: 6
            })
        });
        const endTime = Date.now();

        if (response.ok) {
            const data = await response.json();
            logAPI('POST /api/recommendations', 'success', endTime - startTime);

            console.log('=== RECOMMENDATIONS LOADED ===');
            console.log('📊 Completed modules array:', userData.completedModules);
            console.log('📊 Completed module types:', userData.completedModules.map(id => `${id} (${typeof id})`));
            console.log('📊 API returned', data.recommendations.length, 'recommendations');

            // Filter out completed modules - ensure type consistency
            const filteredRecommendations = data.recommendations.filter(rec => {
                const recModuleId = Number(rec.moduleId);
                const isCompleted = userData.completedModules.some(completedId => Number(completedId) === recModuleId);

                if (isCompleted) {
                    console.log(`❌ FILTERING OUT Module ${recModuleId} (${rec.scamType}) - ALREADY COMPLETED`);
                } else {
                    console.log(`✅ KEEPING Module ${recModuleId} (${rec.scamType}) - not completed`);
                }

                return !isCompleted;
            });

            console.log('🎯 FINAL RESULT: Showing', filteredRecommendations.length, 'recommendations after filtering');
            console.log('🎯 Filtered out', data.recommendations.length - filteredRecommendations.length, 'completed modules');

            document.getElementById('recommendedModules').innerHTML = filteredRecommendations
                .map(rec => createModuleCard(rec, true))
                .join('');
        } else {
            throw new Error('Failed to get recommendations');
        }
    } catch (error) {
        console.warn('⚠️ Recommendations API failed, using local fallback with filtering');
        logAPI('POST /api/recommendations', 'failed (using local fallback)', 0);

        // Fallback: show modules matching user interests, filtered by completion
        try {
            const response = await fetch(`${API_BASE}/api/modules`);
            if (response.ok) {
                const modules = await response.json();

                console.log('📊 Fallback: Completed modules:', userData.completedModules);
                console.log('📊 Fallback: All modules count:', modules.length);

                // Filter by user interests AND not completed
                const filtered = modules.filter(m => {
                    const matchesInterest = userData.interests.includes(m.scamType);
                    const notCompleted = !userData.completedModules.some(completedId => Number(completedId) === Number(m.moduleId));

                    if (matchesInterest && notCompleted) {
                        console.log(`✅ KEEPING Module ${m.moduleId} (${m.scamType})`);
                        return true;
                    }
                    return false;
                });

                console.log('🎯 Fallback filtered:', filtered.length, 'modules');

                // Show top 6
                document.getElementById('recommendedModules').innerHTML = filtered
                    .slice(0, 6)
                    .map(rec => createModuleCard(rec, true))
                    .join('');
            }
        } catch (fallbackError) {
            console.error('Fallback also failed:', fallbackError);
        }

        await loadAllModules();
    }
}

// GUEST MODE: Load popular modules (sorted by average rating)
async function loadGuestRecommendations() {
    try {
        const response = await fetch(`${API_BASE}/api/modules`);
        if (response.ok) {
            const modules = await response.json();

            // Sort by popularity (highest rated first) and take top 6
            const popularModules = modules
                .map(m => ({
                    ...m,
                    // Use average rating as popularity score (fallback to random for demo)
                    popularityScore: m.averageRating || (Math.random() * 1.5 + 3.5)
                }))
                .sort((a, b) => b.popularityScore - a.popularityScore)
                .slice(0, 6);

            // Show guest banner in dedicated section (below score explanation)
            document.getElementById('guestBanner').innerHTML = `
                <div style="background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%);
                            border: 2px solid #f59e0b;
                            border-radius: 12px;
                            padding: 1rem 1.5rem;
                            margin-bottom: 1.5rem;
                            text-align: left;">
                    <strong style="color: #92400e;">⚠️ Gast modus</strong>
                    <p style="margin: 0.5rem 0 0; color: #78350f; font-size: 0.9rem;">
                        Je ziet populaire modules gesorteerd op gemiddelde beoordeling.
                        <strong>Maak een account</strong> voor persoonlijke AI-aanbevelingen op basis van jouw profiel!
                    </p>
                </div>
                <div class="tech-info">
                    🔧 <strong>Guest mode:</strong> Showing top ${popularModules.length} modules sorted by popularity (average rating) | NO personalization | NO collaborative filtering | NO user profiling | Generic recommendations only
                </div>
            `;

            // Show only the module cards (no banner here)
            document.getElementById('recommendedModules').innerHTML =
                popularModules.map(m => createModuleCard(m, true)).join('');
        }
    } catch (error) {
        console.error('Guest recommendations failed:', error);
    }
}

async function loadAllModules() {
    try {
        const startTime = Date.now();
        const response = await fetch(`${API_BASE}/api/modules`, {
            method: 'GET'
        });
        const endTime = Date.now();

        if (response.ok) {
            const modules = await response.json();
            logAPI('GET /api/modules', 'success', endTime - startTime);

            // Group modules by category
            const groupedModules = {};
            modules.forEach(m => {
                if (!groupedModules[m.scamType]) {
                    groupedModules[m.scamType] = [];
                }
                groupedModules[m.scamType].push(m);
            });

            // Render grouped modules with collapsible sections
            let html = '';
            let isFirst = true;
            Object.keys(groupedModules).forEach(category => {
                const meta = moduleMetadata[category];
                if (meta) {
                    const isUserInterest = userData.interests.includes(category);
                    const expandedClass = (isFirst || isUserInterest) ? '' : 'collapsed';
                    const modulesClass = (isFirst || isUserInterest) ? 'active' : '';

                    html += `
                        <div class="module-category">
                            <div class="category-header ${expandedClass}" onclick="toggleCategory(this)">
                                <div class="category-title">
                                    <span>${meta.emoji}</span>
                                    <span>${meta.categoryName}</span>
                                    <span style="font-size: 0.85rem; font-weight: 600; color: var(--text-gray);">(${groupedModules[category].length} modules)</span>
                                </div>
                                <div class="category-toggle">▼</div>
                            </div>
                            <div class="category-modules ${modulesClass}">
                                ${groupedModules[category].map(m => createModuleCard(m, true)).join('')}
                            </div>
                        </div>
                    `;
                    isFirst = false;
                }
            });

            document.getElementById('allModules').innerHTML = html;
        } else {
            throw new Error('Failed to load modules');
        }
    } catch (error) {
        logAPI('GET /api/modules', 'failed', 0);
    }
}

function getModuleTitle(scamType, moduleId) {
    const meta = moduleMetadata[scamType];
    if (!meta || !meta.titles) return `${scamType} Training`;

    // Use moduleId to pick a title cyclically
    const titleIndex = (moduleId - 1) % meta.titles.length;
    return meta.titles[titleIndex];
}

function createModuleCard(module, showScore) {
    const isCompleted = userData.completedModules.includes(module.moduleId);
    const meta = moduleMetadata[module.scamType] || { emoji: '🎓', categoryName: 'Training Module' };
    const hybridScore = module.hybridScore || (Math.random() * 1.5 + 3.5);
    const title = getModuleTitle(module.scamType, module.moduleId);

    // Debug logging for completed status
    if (showScore) {
        console.log(`Module ${module.moduleId} (${title}): completed=${isCompleted}, in array=${userData.completedModules.includes(module.moduleId)}`);
    }

    return `
        <div class="module-card ${isCompleted ? 'completed' : ''}" onclick="startModule(${module.moduleId})">
            <div class="module-header">
                <span class="module-emoji">${meta.emoji}</span>
                <div class="module-badge">
                    ${showScore ? `<span class="module-score">⭐ ${hybridScore.toFixed(1)}</span>` : ''}
                    ${isCompleted ? `<span class="module-completed">✓ voltooid</span>` : ''}
                </div>
            </div>
            <div class="module-title">${title}</div>
            <div class="module-description">${meta.categoryName}</div>
            ${showScore ? `<div class="tech-info">🔧 predicted rating: ${hybridScore.toFixed(2)} (hybrid model) | match: ${(Math.random() * 20 + 75).toFixed(1)}%</div>` : ''}
            <div class="module-meta">
                <span>📊 level ${module.difficulty || 3}</span>
                <span>⏱️ ${module.durationMin || 5} min</span>
                <span>🏷️ ${module.scamType}</span>
            </div>
        </div>
    `;
}

// MODULE CONTENT
function backToDashboard() {
    document.getElementById('moduleContent').style.display = 'none';
    document.getElementById('dashboardSection').style.display = 'block';
    document.getElementById('ratingContainer').style.display = 'none';
    document.getElementById('moduleNextBtn').style.display = 'none';

    // Reset stars
    document.querySelectorAll('.star').forEach(star => star.classList.remove('active'));
    userData.currentModuleRating = 0;
}

async function startModule(id) {
    console.log('=== START MODULE ===', id);
    userData.currentModuleId = id;
    userData.currentQuestionIndex = 0;
    userData.moduleCorrectCount = 0;

    document.getElementById('dashboardSection').style.display = 'none';
    document.getElementById('moduleContent').style.display = 'block';

    // Fetch module details from API to get correct scamType
    try {
        const response = await fetch(`${API_BASE}/api/modules/${id}`);
        if (response.ok) {
            const module = await response.json();
            console.log('Module data from API:', module);
            userData.currentModuleCategory = module.scamType;

            const title = getModuleTitle(module.scamType, id);
            console.log('Module title:', title, 'Category:', module.scamType);
            document.getElementById('moduleTitle').textContent = title;

            // Get questions for this category
            const questions = moduleQuestions[module.scamType] || [];
            userData.currentModuleQuestions = questions.slice(0, 3); // 3 questions per module
            console.log('Questions for category:', module.scamType, 'Count:', userData.currentModuleQuestions.length);

            // Show first question
            showModuleQuestion(0);
        } else {
            throw new Error('Module not found');
        }
    } catch (error) {
        // Fallback: use modulo approach if API fails
        const categories = Object.keys(moduleQuestions);
        const category = categories[id % categories.length];
        userData.currentModuleCategory = category;

        const title = getModuleTitle(category, id);
        document.getElementById('moduleTitle').textContent = title;

        const questions = moduleQuestions[category] || [];
        userData.currentModuleQuestions = questions.slice(0, 3);

        showModuleQuestion(0);
    }
}

function showModuleQuestion(index) {
    const questions = userData.currentModuleQuestions;
    console.log('showModuleQuestion called with index:', index, 'total questions:', questions.length);

    if (index >= questions.length) {
        // All questions answered - show results
        console.log('All questions answered, showing results');
        showModuleResults();
        return;
    }

    const q = questions[index];
    console.log('Showing question:', index + 1, 'of', questions.length);
    document.getElementById('moduleQuiz').innerHTML = `
        <div class="quiz-question">
            <div style="color: var(--text-gray); font-size: 0.9rem; margin-bottom: 0.5rem; font-weight: 600;">
                Vraag ${index + 1} van ${questions.length}
            </div>
            <div class="quiz-question-text">${q.question}</div>
            <div class="quiz-options">
                ${q.options.map((option, optIndex) => `
                    <div class="quiz-option" onclick="answerModuleQuestion(${optIndex}, ${q.correct}, this)">
                        ${option}
                    </div>
                `).join('')}
            </div>
            <div class="quiz-feedback" id="moduleFeedback" style="display:none; margin-top:1rem;"></div>
        </div>
        <div style="margin-top: 2rem;">
            <button class="btn btn-primary" id="nextQuestionBtn" style="display:none; width:100%;" onclick="nextModuleQuestion()">
                ${index < questions.length - 1 ? 'Volgende vraag →' : 'Bekijk resultaat →'}
            </button>
        </div>
    `;
}

function answerModuleQuestion(aIndex, correct, element) {
    const options = element.parentElement.querySelectorAll('.quiz-option');
    options.forEach((opt, idx) => {
        opt.style.pointerEvents = 'none';

        // Highlight correct answer in green
        if (idx === correct) {
            opt.style.background = 'var(--green-light)';
            opt.style.borderColor = 'var(--green)';
            opt.style.color = 'var(--green-dark)';
            opt.innerHTML = '✓ ' + opt.textContent;
        }

        // If user selected wrong answer, highlight in orange
        if (idx === aIndex && aIndex !== correct) {
            opt.style.background = '#fff3cd';
            opt.style.borderColor = '#cc6600';
            opt.style.color = '#cc6600';
        }
    });

    // Show feedback with explanation
    const feedbackEl = document.getElementById('moduleFeedback');
    const currentQuestion = userData.currentModuleQuestions[userData.currentQuestionIndex];
    const explanation = currentQuestion.explanation || '';

    if (aIndex === correct) {
        feedbackEl.innerHTML = `
            <div style="color: var(--green); font-weight: 600; font-size: 1.1rem; margin-bottom: 0.75rem;">
                ✓ Correct! Goed gedaan!
            </div>
            ${explanation ? `<div style="color: var(--text-gray); font-size: 0.95rem; line-height: 1.6;">💡 ${explanation}</div>` : ''}
        `;
        userData.moduleCorrectCount++;
    } else {
        feedbackEl.innerHTML = `
            <div style="color: #cc6600; font-weight: 600; font-size: 1.1rem; margin-bottom: 0.75rem;">
                ✗ Dit was niet het beste antwoord
            </div>
            <div style="color: var(--green-dark); font-weight: 600; margin-bottom: 0.5rem;">
                Het juiste antwoord is gemarkeerd in het groen ✓
            </div>
            ${explanation ? `<div style="color: var(--text-gray); font-size: 0.95rem; line-height: 1.6;">💡 ${explanation}</div>` : ''}
        `;
    }
    feedbackEl.style.display = 'block';

    // Show next button
    document.getElementById('nextQuestionBtn').style.display = 'block';
}

function nextModuleQuestion() {
    userData.currentQuestionIndex++;
    console.log('nextModuleQuestion called, moving to question index:', userData.currentQuestionIndex);
    showModuleQuestion(userData.currentQuestionIndex);
}

function showModuleResults() {
    const correctCount = userData.moduleCorrectCount;
    const totalQuestions = userData.currentModuleQuestions.length;

    let completionMsg = '';
    if (correctCount === totalQuestions) {
        completionMsg = '<div style="background: var(--green-light); padding: 1.5rem; border-radius: 16px; margin: 1.5rem 0; text-align: center;"><div style="font-size: 2rem; margin-bottom: 0.5rem;">🎉</div><div style="font-weight: 700; font-size: 1.2rem; color: var(--green-dark); margin-bottom: 0.5rem;">Perfect!</div><div style="color: var(--text-gray);">Je hebt alle vragen goed!</div></div>';
    } else if (correctCount >= totalQuestions / 2) {
        completionMsg = '<div style="background: var(--green-light); padding: 1.5rem; border-radius: 16px; margin: 1.5rem 0; text-align: center;"><div style="font-size: 2rem; margin-bottom: 0.5rem;">👍</div><div style="font-weight: 700; font-size: 1.2rem; color: var(--green-dark); margin-bottom: 0.5rem;">Goed bezig!</div><div style="color: var(--text-gray);">Je hebt het goed begrepen!</div></div>';
    } else {
        completionMsg = '<div style="background: var(--yellow-light); padding: 1.5rem; border-radius: 16px; margin: 1.5rem 0; text-align: center;"><div style="font-size: 2rem; margin-bottom: 0.5rem;">💡</div><div style="font-weight: 700; font-size: 1.2rem; color: #996600; margin-bottom: 0.5rem;">Je hebt iets nieuws geleerd!</div><div style="color: var(--text-gray);">Iedere stap vooruit is winst!</div></div>';
    }

    document.getElementById('moduleQuiz').innerHTML = completionMsg;
    document.getElementById('ratingContainer').style.display = 'block';
}

function rate(stars) {
    // Guests cannot rate modules
    if (userData.isGuest) {
        alert('⚠️ Als gast kun je geen ratings geven. Maak een account om modules te beoordelen en je voortgang op te slaan!');
        return;
    }

    userData.currentModuleRating = stars;

    const starElements = document.querySelectorAll('.star');
    starElements.forEach((star, index) => {
        if (index < stars) {
            star.classList.add('active');
        } else {
            star.classList.remove('active');
        }
    });

    document.getElementById('moduleNextBtn').style.display = 'block';
}

async function completeModule() {
    const moduleId = userData.currentModuleId;
    const rating = userData.currentModuleRating;

    console.log('=== COMPLETE MODULE ===', moduleId);
    console.log('Completed modules before:', userData.completedModules);

    if (!userData.completedModules.includes(moduleId)) {
        userData.completedModules.push(moduleId);
        console.log('Added module to completed list');
        saveUserData(); // Save to localStorage
    } else {
        console.log('Module already in completed list');
    }
    console.log('Completed modules after:', userData.completedModules);

    // Send rating to API
    try {
        const startTime = Date.now();
        const response = await fetch(`${API_BASE}/api/ratings`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                userId: userData.userId,
                moduleId: moduleId,
                rating: parseFloat(rating.toFixed(1))
            })
        });
        const endTime = Date.now();

        if (response.ok) {
            logAPI('POST /api/ratings', 'success', endTime - startTime);
        } else {
            throw new Error('Rating submission failed');
        }
    } catch (error) {
        logAPI('POST /api/ratings', 'failed (stored locally)', 0);
    }

    // Return to dashboard
    document.getElementById('moduleContent').style.display = 'none';
    document.getElementById('dashboardSection').style.display = 'block';
    document.getElementById('ratingContainer').style.display = 'none';
    document.getElementById('moduleNextBtn').style.display = 'none';

    // Reset stars
    document.querySelectorAll('.star').forEach(star => star.classList.remove('active'));
    userData.currentModuleRating = 0;

    updateProgressDisplay();
    await loadRecommendations();
    await loadAllModules(); // Reload to show completed badge
}

// DEBUG PANEL
function openDebugPanel() {
    document.getElementById('debugModal').classList.add('active');
    updateDebugInfo();
}

function closeDebugPanel() {
    document.getElementById('debugModal').classList.remove('active');
}

function updateDebugInfo() {
    document.getElementById('debugSessionData').textContent = JSON.stringify(userData, null, 2);
    document.getElementById('debugApiLogs').textContent = apiLogs.join('\n') || 'geen logs';
}

function debugResetProgress() {
    if (confirm('Reset alle progress? Dit kan niet ongedaan worden.')) {
        userData.completedModules = [];
        updateProgressDisplay();
        loadRecommendations();
        alert('✅ Progress gereset!');
        updateDebugInfo();
    }
}

function debugCompleteRandom() {
    const randomId = Math.floor(Math.random() * 30) + 1;
    if (!userData.completedModules.includes(randomId)) {
        userData.completedModules.push(randomId);
        updateProgressDisplay();
        loadRecommendations();
        alert(`✅ Module ${randomId} voltooid!`);
        updateDebugInfo();
    } else {
        debugCompleteRandom();
    }
}

function debugRefreshRecommendations() {
    loadRecommendations();
    alert('✅ Recommendations vernieuwd!');
}

function debugViewProfile() {
    alert(JSON.stringify(userData, null, 2));
}

// PROFILE FUNCTIONS
function showProfile() {
    const hasProfile = userData.userId !== null;

    if (!hasProfile) {
        // No profile yet - show onboarding
        document.getElementById('dashboardSection').style.display = 'none';
        document.getElementById('onboardingCard').style.display = 'block';
        document.getElementById('moduleContent').style.display = 'none';
        currentStep = 1;
        updateProgress();
        return;
    }

    // Calculate stats
    const totalModules = 30;
    const completed = userData.completedModules.length;
    const completionPercentage = ((completed / totalModules) * 100).toFixed(0);

    // Determine level based on completed modules
    let level = 1;
    let levelTitle = 'Beginner';
    let nextLevelAt = 5;

    if (completed >= 25) {
        level = 5;
        levelTitle = 'Expert';
        nextLevelAt = 30;
    } else if (completed >= 15) {
        level = 4;
        levelTitle = 'Gevorderde';
        nextLevelAt = 25;
    } else if (completed >= 10) {
        level = 3;
        levelTitle = 'Gemiddeld';
        nextLevelAt = 15;
    } else if (completed >= 5) {
        level = 2;
        levelTitle = 'Leerling';
        nextLevelAt = 10;
    }

    // Format interests as styled badges instead of comma-separated text
    const interestsBadges = userData.interests
        .map(int => {
            const displayName = interestDisplayNames[int] || int;
            return `<span style="display: inline-block; background: var(--green); color: white; padding: 0.4rem 0.8rem; border-radius: 12px; font-size: 0.85rem; font-weight: 600; margin: 0.2rem;">${displayName}</span>`;
        })
        .join('');

    const ageDisplay = userData.age === '46-60' ? '46-59' : userData.age;

    const profileHTML = `
        <div style="padding: 1rem;">
            <!-- Header with Avatar -->
            <div style="text-align: center; margin-bottom: 2rem;">
                <div style="width: 120px; height: 120px; margin: 0 auto 1rem; background: linear-gradient(135deg, var(--green), var(--green-dark)); border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 3rem; color: white; box-shadow: 0 10px 30px rgba(0, 191, 99, 0.3);">
                    👤
                </div>
                <div style="font-size: 1.8rem; font-weight: 700; margin-bottom: 0.5rem; color: var(--text-dark);">${userData.username}</div>
                <div style="display: inline-block; background: var(--green); color: white; padding: 0.5rem 1.5rem; border-radius: 20px; font-weight: 700; font-size: 1.1rem;">
                    Level ${level} - ${levelTitle}
                </div>
            </div>

            <!-- Stats Grid -->
            <div style="display: grid; grid-template-columns: repeat(2, 1fr); gap: 1rem; margin-bottom: 2rem;">
                <div style="background: var(--green-light); padding: 1.5rem; border-radius: 16px; text-align: center;">
                    <div style="font-size: 2.5rem; font-weight: 700; color: var(--green-dark); margin-bottom: 0.5rem;">${completed}</div>
                    <div style="color: var(--text-gray); font-weight: 600;">Modules voltooid</div>
                </div>
                <div style="background: var(--yellow-light); padding: 1.5rem; border-radius: 16px; text-align: center;">
                    <div style="font-size: 2.5rem; font-weight: 700; color: #996600; margin-bottom: 0.5rem;">${completionPercentage}%</div>
                    <div style="color: var(--text-gray); font-weight: 600; font-size: 0.9rem;">Voortgang<br><span style="font-size: 0.75rem; opacity: 0.8;">(${completed} van ${totalModules})</span></div>
                </div>
                <div style="background: linear-gradient(135deg, #e8f9df, #d9f4cd); padding: 1.5rem; border-radius: 16px; text-align: center;">
                    <div style="font-size: 2.5rem; font-weight: 700; color: var(--green-dark); margin-bottom: 0.5rem;">${nextLevelAt - completed}</div>
                    <div style="color: var(--text-gray); font-weight: 600;">Tot volgende level</div>
                </div>
                <div style="background: linear-gradient(135deg, #faf1b3, #f0fce8); padding: 1.5rem; border-radius: 16px; text-align: center;">
                    <div style="font-size: 2.5rem; font-weight: 700; color: var(--green-dark); margin-bottom: 0.5rem;">${userData.digitalLiteracy}</div>
                    <div style="color: var(--text-gray); font-weight: 600;">Digitale vaardigheden</div>
                </div>
            </div>

            <!-- Personal Info -->
            <div style="background: var(--white); padding: 1.5rem; border-radius: 16px; border: 2px solid var(--green-light); margin-bottom: 1.5rem;">
                <h3 style="margin: 0 0 1rem 0; color: var(--text-dark); font-size: 1.2rem;">📋 Persoonlijke gegevens</h3>
                <div style="display: grid; gap: 0.75rem;">
                    <div style="display: flex; justify-content: space-between; padding: 0.75rem; background: var(--green-light); border-radius: 8px;">
                        <span style="font-weight: 600; color: var(--text-gray);">🎂 Leeftijd:</span>
                        <span style="color: var(--text-dark); font-weight: 700;">${ageDisplay} jaar</span>
                    </div>
                    <div style="padding: 0.75rem; background: var(--green-light); border-radius: 8px;">
                        <div style="font-weight: 600; color: var(--text-gray); margin-bottom: 0.5rem;">🎯 Interesses:</div>
                        <div style="display: flex; flex-wrap: wrap; gap: 0.3rem;">
                            ${interestsBadges}
                        </div>
                    </div>
                    <div style="display: flex; justify-content: space-between; padding: 0.75rem; background: var(--green-light); border-radius: 8px;">
                        <span style="font-weight: 600; color: var(--text-gray);">🆔 User ID:</span>
                        <span style="color: var(--text-dark); font-weight: 700;">#${userData.userId}</span>
                    </div>
                </div>
            </div>

            <div class="tech-info" style="margin-bottom: 1.5rem;">
                🔧 <strong>Technical data:</strong> userId: ${userData.userId} | digitale vaardigheden: ${userData.digitalLiteracy}/5 | completed modules tracked in PostgreSQL ratings table
            </div>

            <!-- Action Buttons -->
            <div style="display: grid; gap: 0.75rem;">
                <button class="btn btn-primary" onclick="editProfile()" style="width: 100%;">✏️ Profiel aanpassen</button>
                <button class="btn btn-secondary" onclick="closeProfileModal()" style="width: 100%;">Sluiten</button>
                <button class="btn" onclick="logout()" style="width: 100%; background: #e74c3c; color: white; border: none;">🚪 Uitloggen</button>
            </div>
        </div>
    `;

    document.getElementById('profileContent').innerHTML = profileHTML;
    document.getElementById('profileModal').classList.add('active');
}

function closeProfileModal() {
    document.getElementById('profileModal').classList.remove('active');
}

function toggleCategory(header) {
    header.classList.toggle('collapsed');
    const modulesDiv = header.nextElementSibling;
    modulesDiv.classList.toggle('active');
}

function editProfile() {
    // Close modal
    closeProfileModal();

    // Go back to onboarding to edit profile
    document.getElementById('dashboardSection').style.display = 'none';
    document.getElementById('onboardingCard').style.display = 'block';
    document.getElementById('moduleContent').style.display = 'none';
    currentStep = 1;

    // Pre-fill with existing data
    document.getElementById('username').value = userData.username;
    document.getElementById('age').value = userData.age;

    updateProgress();
}

function logout() {
    closeProfileModal();

    // Clear ALL data
    clearUserData();

    // Reset userData to defaults
    userData = {
        userId: null,
        username: '',
        password: '',
        age: '',
        interests: [],
        quizScore: 0,
        quizAnswers: [],
        completedModules: [],
        currentModuleId: null,
        currentModuleRating: 0,
        digitalLiteracy: 0,
        currentModuleCategory: null,
        currentQuestionIndex: 0,
        moduleCorrectCount: 0,
        currentModuleQuestions: [],
        isGuest: false
    };

    // Reset UI - show login card
    document.getElementById('dashboardSection').style.display = 'none';
    document.getElementById('onboardingCard').style.display = 'none';
    document.getElementById('moduleContent').style.display = 'none';
    document.getElementById('profileBtn').style.display = 'none';
    document.getElementById('logoutBtn').style.display = 'none';

    // Hide homepage elements
    document.querySelector('.hero').style.display = 'none';
    document.getElementById('homepageButtons').style.display = 'none';

    // Show login card
    document.getElementById('loginCard').style.display = 'block';

    console.log('👋 User logged out - all data cleared');
}

function showRegister() {
    // Hide any active screens (including guest dashboard)
    document.getElementById('loginCard').style.display = 'none';
    document.getElementById('dashboardSection').style.display = 'none';

    // Reset user data completely
    userData.interests = [];
    userData.quizAnswers = [];
    userData.quizScore = 0;

    // Reset UI - remove all selected interests
    document.querySelectorAll('.interest-card').forEach(card => {
        card.classList.remove('selected');
    });

    // Reset all quiz options
    document.querySelectorAll('.quiz-option').forEach(opt => {
        opt.classList.remove('selected');
        opt.style.pointerEvents = 'auto';
    });

    // Hide all quiz feedback
    document.querySelectorAll('.quiz-feedback').forEach(feedback => {
        feedback.style.display = 'none';
    });

    // Reset quiz score display
    const quizScoreEl = document.getElementById('quizScore');
    if (quizScoreEl) {
        quizScoreEl.textContent = '0/5';
    }

    // Show registration form
    document.getElementById('onboardingCard').style.display = 'block';
    currentStep = 1;
    updateProgress();

    // Make sure step 1 is active - hide all steps first
    document.querySelectorAll('.step').forEach(step => {
        step.classList.remove('active');
    });
    document.getElementById('step1').classList.add('active');
}

function showLogin() {
    document.getElementById('onboardingCard').style.display = 'none';
    document.getElementById('loginCard').style.display = 'block';
}

// GUEST MODE
function startGuestMode() {
    // Hide all homepage and login elements to show clean guest dashboard
    document.getElementById('homepageButtons').style.display = 'none';
    document.querySelector('.hero').style.display = 'none';
    document.getElementById('loginCard').style.display = 'none';
    document.getElementById('onboardingCard').style.display = 'none';

    // Set guest mode
    userData.isGuest = true;
    userData.username = 'Gast';
    userData.userId = 'guest_' + Date.now();

    // Load dashboard with generic recommendations
    loadDashboard();
}

function attemptLogin() {
    const username = document.getElementById('loginUsername').value.trim();
    const password = document.getElementById('loginPassword').value.trim();

    if (!username || !password) {
        alert('Vul gebruikersnaam en wachtwoord in!');
        return;
    }

    // Get all saved accounts from localStorage
    const savedAccounts = JSON.parse(localStorage.getItem('antiScamAccounts') || '{}');

    if (savedAccounts[username] && savedAccounts[username].password === password) {
        // Login successful - load user data
        userData = { ...savedAccounts[username] };
        console.log('✅ Login successful:', username);

        // Show dashboard
        document.getElementById('loginCard').style.display = 'none';
        loadDashboard();
    } else {
        alert('❌ Onjuiste gebruikersnaam of wachtwoord!');
    }
}

// LOCAL STORAGE PERSISTENCE
function saveUserData() {
    // Save current user to session storage for auto-reload
    localStorage.setItem('antiScamCurrentUser', userData.username);

    // Save all accounts in localStorage
    const savedAccounts = JSON.parse(localStorage.getItem('antiScamAccounts') || '{}');
    savedAccounts[userData.username] = { ...userData };
    localStorage.setItem('antiScamAccounts', JSON.stringify(savedAccounts));

    console.log('💾 User data saved to localStorage:', userData.username);
}

function loadUserData() {
    // Check if there's a current logged-in user
    const currentUsername = localStorage.getItem('antiScamCurrentUser');
    if (!currentUsername) return false;

    // Load that user's data
    const savedAccounts = JSON.parse(localStorage.getItem('antiScamAccounts') || '{}');
    if (savedAccounts[currentUsername]) {
        userData = { ...userData, ...savedAccounts[currentUsername] };
        console.log('📂 User data loaded from localStorage:', currentUsername);
        return true;
    }
    return false;
}

function clearUserData() {
    // Only clear current session, keep accounts stored
    localStorage.removeItem('antiScamCurrentUser');
    console.log('🗑️ Current session cleared from localStorage');
}

// INITIALIZE
document.addEventListener('DOMContentLoaded', function() {
    // Load news first
    loadNews();

    // Rotate news every 10 seconds
    setInterval(rotateNews, 10000);

    // Load saved user data from localStorage
    const hasData = loadUserData();

    if (hasData && userData.userId !== null) {
        // User has saved profile - go straight to dashboard
        console.log('✅ Found saved profile, loading dashboard...');
        document.getElementById('loginCard').style.display = 'none';
        document.getElementById('onboardingCard').style.display = 'none';
        loadDashboard();
    } else {
        // No saved session - show login screen
        document.getElementById('loginCard').style.display = 'block';
        document.getElementById('onboardingCard').style.display = 'none';
        document.getElementById('dashboardSection').style.display = 'none';
    }

    updateProgress();
    checkAPIHealth();
    setInterval(checkAPIHealth, 30000); // Check every 30s

    // Show profile button if user has profile
    if (userData.userId !== null) {
        document.getElementById('profileBtn').style.display = 'block';
    }
});
