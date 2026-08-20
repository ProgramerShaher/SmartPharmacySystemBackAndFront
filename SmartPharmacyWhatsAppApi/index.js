const express = require('express');
const cors = require('cors');
const { Client, LocalAuth, NoAuth } = require('whatsapp-web.js');
const qrcode = require('qrcode-terminal');
const fs = require('fs');
const path = require('path');
const os = require('os');

const app = express();
const PORT = process.env.PORT || 3000;

const LOG_FILE = path.join(__dirname, 'whatsapp-log.txt');
const AUTH_DIR = path.join(__dirname, '.wwebjs_auth');
let CRASH_COUNT = 0;
const MAX_CRASH_BEFORE_RESET = 4;
let isShuttingDown = false;

process.on('uncaughtException', (err) => {
    CRASH_COUNT++;
    log('💥💥💥 UNCAUGHT EXCEPTION (خطأ غير متوقع) 💥💥💥', 'FATAL_CRASH');
    log('   رقم الانهيار: ' + CRASH_COUNT + '/' + MAX_CRASH_BEFORE_RESET, 'FATAL_CRASH');
    log('   النوع: ' + err.name, 'FATAL_CRASH');
    log('   الرسالة: ' + err.message, 'FATAL_CRASH');
    if (err.stack) log('   الستاك: ' + String(err.stack).substring(0, 1500), 'FATAL_CRASH');

    const needFullReset = CRASH_COUNT >= MAX_CRASH_BEFORE_RESET;
    if (needFullReset) {
        log('🔴 تجاوز الحد الأقصى للإنهيارات المتتالية → حذف الجلسة وإعادة البدء من الصفر', 'FATAL_CRASH');
        CRASH_COUNT = 0;
    }

    try { stopKeepAlive(); } catch(e) {}
    try { if (client) { try { client.destroy(); } catch(e) {} client = null; } } catch(e) {}
    isReady = false;

    if (!isShuttingDown) {
        isShuttingDown = true;
        const waitMs = needFullReset ? 4000 : 2500;
        log(`⏳ إعادة تهيئة تلقائية خلال ${waitMs/1000} ثانية... (${needFullReset ? 'مع إعادة ضبط الجلسة' : 'بدون إعادة ضبط'})`, 'FATAL_CRASH');
        setTimeout(() => {
            isShuttingDown = false;
            try {
                if (needFullReset) {
                    try { fs.rmSync(AUTH_DIR, { recursive: true, force: true }); log('✅ حذف مجلد الجلسة بنجاح', 'FATAL_CRASH'); } catch(e) {}
                    const extraChromeDir = path.join(__dirname, '.chrome-data');
                    try { if (fs.existsSync(extraChromeDir)) { fs.rmSync(extraChromeDir, { recursive: true, force: true }); } } catch(e) {}
                }
                initRetries = 0;
                isReconnecting = false;
                createClient();
            } catch (recoverErr) {
                log('❌ فشل الاستعادة التلقائية: ' + recoverErr.message, 'FATAL_CRASH');
                setTimeout(() => process.exit(1), 3000);
            }
        }, waitMs);
    }
});

process.on('unhandledRejection', (reason, promise) => {
    const msg = reason && reason.message ? reason.message : String(reason || '');
    const isExecutionContext = msg.includes('Execution context') || msg.includes('Session closed') || msg.includes('Navigation timeout');
    const isChromeCrash = msg.includes('Target closed') || msg.includes('Protocol error') || msg.includes('WebSocket');
    const isEbusy = msg.includes('EBUSY');

    if (isEbusy) {
        log(`⚠️ تم تجاهل خطأ قفل الملف (EBUSY) أثناء تنظيف الجلسة: ${msg.substring(0, 100)}`, 'UNHANDLED_REJ');
        return;
    }

    log(`⚠️ UNHANDLED PROMISE REJECTION: ${msg.substring(0, 200)}`, 'UNHANDLED_REJ');
    if (reason && reason.stack) log('   Stack: ' + String(reason.stack).substring(0, 800), 'UNHANDLED_REJ');

    if (isExecutionContext || isChromeCrash) {
        log('🔍 تم الكشف عن خطأ Nav/Execution Context → بدء استعادة تدريجية خلال 3 ثوانٍ', 'UNHANDLED_REJ');
        setTimeout(() => {
            if (!isReconnecting && !isShuttingDown) safeDestroyAndReinit(false);
        }, 3000);
    }
});

function log(msg, level = 'INFO') {
    const ts = new Date().toISOString();
    const line = `[${ts}] [${level}] ${msg}`;
    console.log(line);
    try { fs.appendFileSync(LOG_FILE, line + '\n', 'utf8'); } catch(e) {}
}

log('\n\n' + '='.repeat(70));
log('🚀 بدء تشغيل خادم WhatsApp - إصدار محسن مع إعدادات Chrome مستقرة');
log('📁 مجلد الجلسة (LocalAuth): ' + AUTH_DIR);
log('💾 سجل النشاطات: ' + LOG_FILE);
log('='.repeat(70) + '\n');

app.use(cors());
app.use(express.json());

let currentQr = null;
let qrGeneratedAt = null;
let isReady = false;
let lastDisconnectReason = null;
let lastRequest = null;
let client = null;
let lastQrPage = null;
let initRetries = 0;
const MAX_INIT_RETRIES = 5;
let keepAliveInterval = null;
let lastSuccessfulSend = 0;
let clientStuckCount = 0;
let isReconnecting = false;

function getChromeArgs() {
    return [
        '--no-sandbox',
        '--disable-setuid-sandbox',
        '--disable-dev-shm-usage',
        '--disable-accelerated-2d-canvas',
        '--no-first-run',
        '--no-zygote',
        '--disable-gpu',
        '--disable-features=VizDisplayCompositor,IsolateOrigins,site-per-process,SitePerProcess',
        '--disable-site-isolation-trials',
        '--disable-web-security',
        '--disable-features=TranslateUI',
        '--disable-component-extensions-with-background-pages',
        '--disable-default-apps',
        '--mute-audio',
        '--no-default-browser-check',
        '--ignore-certificate-errors',
        '--allow-running-insecure-content',
        '--disable-background-networking',
        '--disable-background-timer-throttling',
        '--disable-ipc-flooding-protection',
        '--disable-renderer-backgrounding',
        '--enable-features=NetworkService,NetworkServiceInProcess',
        '--force-color-profile=srgb',
        '--hide-scrollbars',
        '--metrics-recording-only',
        '--disable-breakpad',
        '--disable-client-side-phishing-detection',
        '--disable-component-update',
        '--disable-domain-reliability',
        '--disable-extensions',
        '--disable-hang-monitor',
        '--disable-popup-blocking',
        '--disable-prompt-on-repost',
        '--disable-sync',
        '--password-store=basic',
        '--use-mock-keychain',
        '--window-size=1280,720',
        '--start-maximized'
    ];
}

function findChromeExecutable() {
    const candidates = [
        process.env.CHROME_PATH || '',
        'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe',
        'C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe',
        path.join(os.homedir(), 'AppData\\Local\\Google\\Chrome\\Application\\chrome.exe'),
        'C:\\Program Files\\Microsoft\\Edge\\Application\\msedge.exe',
    ];
    for (const c of candidates) {
        if (c && fs.existsSync(c)) {
            log('✅ تم العثور على المتصفح في: ' + c, 'CHROME');
            return c;
        }
    }
    log('⚠️ لم يتم العثور على Chrome في المسارات الشائعة، سيتم محاولة استخدام النسخة المدمجة (إذا وجدت)', 'CHROME');
    return null;
}

function getChromeOptions() {
    const exe = findChromeExecutable();
    const opts = {
        headless: false,
        args: getChromeArgs(),
        ignoreHTTPSErrors: true,
        waitForInitialPage: true,
        protocolTimeout: 180000,
        timeout: 0,
        slowMo: 50
    };
    if (exe) opts.executablePath = exe;
    log('🔧 إعدادات Puppeteer الجاهزة - protocolTimeout=180s, slowMo=50ms, headless=false' + (exe ? `, Chrome=${exe}` : ', Chromium مدمج'), 'CONFIG');
    return opts;
}

function createClient() {
    log('🔄 إنشاء عميل WhatsApp جديد... (محاولة ' + (initRetries+1) + '/' + MAX_INIT_RETRIES + ')', 'INIT');
    isReconnecting = false; // إصلاح: تصفير حالة إعادة الاتصال لمنع التعليق النهائي

    try {
        client = new Client({
            authStrategy: new LocalAuth({ clientId: 'pharmacy-bot', dataPath: AUTH_DIR }),
            takeoverOnConflict: true,
            takeoverTimeoutMs: 60000,
            qrMaxRetries: 15,
            puppeteer: getChromeOptions()
        });
    } catch (createErr) {
        log('❌ فشل إنشاء الكلاينت: ' + createErr.toString(), 'FATAL');
        return null;
    }

    client.on('loading_screen', (percent, message) => {
        log(`⏳ شاشة التحميل: ${percent}% - ${message}`, 'LOADING');
    });

    client.on('authenticated', () => {
        log('✅ ✅ ✅ المصادقة نجحت! جاري تحميل الواتساب...', 'AUTH');
        currentQr = null;
    });

    client.on('auth_failure', (msg) => {
        log('❌ فشل المصادقة: ' + JSON.stringify(msg), 'AUTH_FAIL');
        isReady = false;
    });

    client.on('qr', (qr) => {
        initRetries = 0;
        currentQr = qr;
        qrGeneratedAt = new Date();
        log('\n' + '='.repeat(60), 'QR');
        log('📱 وصل رمز QR جديد - امسحه من هاتفك خلال 30 ثانية!', 'QR');
        log('🌐 أو افتح: http://localhost:' + PORT, 'QR');
        log('='.repeat(60) + '\n', 'QR');
        try { qrcode.generate(qr, { small: true }); } catch(e) {}
    });

    client.on('ready', () => {
        initRetries = 0;
        isReady = true;
        currentQr = null;
        lastDisconnectReason = null;
        clientStuckCount = 0;
        isReconnecting = false;
        startKeepAlive();
        log('\n' + '✅'.repeat(10), 'READY');
        log('✅ 🎉 تم الربط بنجاح - خدمة الواتساب جاهزة للعمل 🎉 ✅', 'READY');
        log('✅'.repeat(10) + '\n', 'READY');
    });

    client.on('disconnected', (reason) => {
        log('⚠️  تم قطع الاتصال: ' + reason, 'DISCONNECT');
        isReady = false;
        lastDisconnectReason = String(reason);
        currentQr = null;
        stopKeepAlive();

        if (String(reason).includes('NAVIGATION') || String(reason).includes('LOGOUT') || String(reason).includes('403')) {
            log('🔄 تم قطع الاتصال (LOGOUT/NAVIGATION)، محاولة إعادة تهيئة الجلسة الحالية أولاً...', 'DISCONNECT');
            setTimeout(() => {
                log('🔁 محاولة إعادة الاتصال التلقائية (استعادة الجلسة الحالية)...', 'RECONNECT');
                safeDestroyAndReinit(false);
            }, 5000);
        } else {
            log('⏱️  سيتم محاولة إعادة الاتصال تلقائياً خلال 5 ثوانٍ...', 'DISCONNECT');
            setTimeout(() => {
                log('🔁 محاولة إعادة التهيئة التلقائية بعد قطع الاتصال...', 'RECONNECT');
                safeDestroyAndReinit();
            }, 5000);
        }
    });

    client.on('change_state', (state) => {
        log('🔄 تغيير حالة الاتصال: ' + state, 'STATE');
    });

    client.on('message', msg => {
        try {
            log(`📥 رسالة من ${msg.from}: ${String(msg.body || '').substring(0, 80)}`, 'INBOUND');
        } catch(e) {}
    });

    client.initialize()
        .then(() => log('✅ client.initialize() تم استدعاؤه بنجاح - في انتظار الأحداث...', 'INIT'))
        .catch((initErr) => {
            log('❌ فشل client.initialize(): ' + initErr.toString(), 'INIT_ERR');
            if (initErr.stack) log('STACK: ' + initErr.stack, 'INIT_ERR');
            initRetries++;
            if (initRetries < MAX_INIT_RETRIES) {
                const waitMs = 3000 * initRetries;
                log(`⏱️  إعادة المحاولة بعد ${waitMs/1000} ثوانٍ... (${initRetries}/${MAX_INIT_RETRIES})`, 'RETRY');
                setTimeout(() => safeDestroyAndReinit(), waitMs);
            } else {
                log('❌ ❌ ❌ تم استنفاذ عدد المحاولات، يرجى تشغيل السكربت مرة أخرى', 'FATAL');
                log('💡 نصيحة: حذف المجلدين .chrome-data و .wwebjs_auth وإعادة المحاولة', 'FATAL');
            }
        });

    return client;
}

async function safeDestroyAndReinit(forceClearSession = false) {
    if (isReconnecting) {
        log('⏭️ تجاهل طلب إعادة تهيئة - هناك عملية جارية بالفعل', 'CLEANUP');
        return;
    }
    isReconnecting = true;
    stopKeepAlive();
    if (client) {
        try {
            log('🧹 جاري تنظيف العميل القديم...', 'CLEANUP');
            await client.destroy();
            log('✅ تم تدمير العميل القديم', 'CLEANUP');
        } catch (e) {
            log('⚠️ تحذير أثناء تدمير العميل: ' + e.message, 'CLEANUP');
        }
        client = null;
        isReady = false;
    }

    if (forceClearSession) {
        try {
            log('🗑️ حذف مجلد الجلسة عند الطلب...', 'CLEANUP');
            fs.rmSync(AUTH_DIR, { recursive: true, force: true });
        } catch(e) {}
    }

    setTimeout(() => {
        createClient();
    }, 2000);
}

function startKeepAlive() {
    stopKeepAlive();
    log('❤️  بدء تشغيل آلية Keep-Alive (فحص كل 60 ثانية)', 'KEEPALIVE');
    keepAliveInterval = setInterval(async () => {
        try {
            if (!client || !isReady) {
                stopKeepAlive();
                return;
            }
            const t0 = Date.now();
            let stateOk = true;
            try {
                const state = await client.getState();
                if (!state || state !== 'CONNECTED') {
                    log(`⚠️ Keep-Alive: حالة العميل غير طبيعية = ${state}`, 'KEEPALIVE');
                    stateOk = false;
                } else {
                    log(`❤️ Keep-Alive: حالة الاتصال = ${state} (${Date.now()-t0}ms)`, 'KEEPALIVE');
                }
            } catch (chkErr) {
                log(`⚠️ Keep-Alive: فشل فحص الحالة: ${chkErr.message}`, 'KEEPALIVE');
                stateOk = false;
            }

            if (!stateOk) {
                clientStuckCount++;
                log(`⚠️ Keep-Alive: عدد مرات الفشل المتتالية = ${clientStuckCount}/3`, 'KEEPALIVE');
                if (clientStuckCount >= 3) {
                    log('🔴 Keep-Alive: تجاوز الحد المسموح - بدء إعادة تهيئة تلقائية للعميل...', 'KEEPALIVE');
                    clientStuckCount = 0;
                    safeDestroyAndReinit(false);
                }
            } else {
                clientStuckCount = 0;
            }
        } catch (kaErr) {
            log(`❌ Keep-Alive: خطأ عام: ${kaErr.message}`, 'KEEPALIVE');
        }
    }, 60000);
}

function stopKeepAlive() {
    if (keepAliveInterval) {
        clearInterval(keepAliveInterval);
        keepAliveInterval = null;
        log('🛑 تم إيقاف آلية Keep-Alive', 'KEEPALIVE');
    }
}

createClient();

app.get('/', (req, res) => {
    res.send(`
<!DOCTYPE html>
<html lang="ar" dir="rtl">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>ربط WhatsApp - نظام الصيدلية الذكية</title>
    <script src="https://cdn.jsdelivr.net/npm/qrcodejs@1.0.0/qrcode.min.js"></script>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; font-family: 'Segoe UI', Tahoma, sans-serif; }
        body { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); min-height: 100vh; display: flex; align-items: center; justify-content: center; padding: 20px; }
        .card { background: white; border-radius: 20px; padding: 40px; box-shadow: 0 20px 60px rgba(0,0,0,0.3); max-width: 620px; width: 100%; }
        .header h1 { color: #2d3748; font-size: 24px; margin-bottom: 6px; }
        .header p { color: #718096; font-size: 14px; }
        .status-badge { display: inline-block; padding: 8px 20px; border-radius: 50px; font-size: 14px; font-weight: bold; margin: 15px 0; }
        .status-ready { background: #c6f6d5; color: #22543d; }
        .status-waiting { background: #fefcbf; color: #744210; }
        .status-error { background: #fed7d7; color: #742a2a; }
        .qr-container { background: #f7fafc; border: 3px dashed #cbd5e0; border-radius: 15px; padding: 30px; margin: 20px 0; min-height: 320px; display: flex; align-items: center; justify-content: center; }
        #qrcode img, #qrcode canvas { margin: 0 auto; }
        .loading { display: inline-block; width: 40px; height: 40px; border: 4px solid #cbd5e0; border-top-color: #667eea; border-radius: 50%; animation: spin 1s linear infinite; }
        @keyframes spin { to { transform: rotate(360deg); } }
        .instructions { background: #ebf8ff; border-right: 4px solid #4299e1; padding: 15px 20px; border-radius: 8px; text-align: right; margin-top: 16px; }
        .instructions h3 { color: #2b6cb0; margin-bottom: 8px; font-size: 16px; }
        .instructions ol { padding-right: 20px; color: #2d3748; font-size: 14px; line-height: 2; }
        .refresh-btn { margin-top: 15px; padding: 10px 25px; background: #667eea; color: white; border: none; border-radius: 8px; cursor: pointer; font-size: 14px; font-weight: bold; }
        .refresh-btn:hover { background: #5a67d8; }
        .time-info { color: #a0aec0; font-size: 12px; margin-top: 10px; }
        .test-box { margin-top: 24px; background: #faf5ff; border-right: 4px solid #805ad5; padding: 16px; border-radius: 8px; }
        .test-box h3 { color: #553c9a; margin-bottom: 12px; font-size: 16px; }
        .test-box input, .test-box textarea { width: 100%; padding: 10px; margin: 6px 0; border: 1px solid #e2e8f0; border-radius: 8px; font-size: 14px; font-family: inherit; }
        .test-box button { width: 100%; padding: 12px; background: #805ad5; color: white; border: none; border-radius: 8px; font-weight: bold; cursor: pointer; margin-top: 8px; }
        .test-box button:hover { background: #6b46c1; }
        .test-result { margin-top: 12px; padding: 10px; border-radius: 8px; font-size: 13px; display: none; white-space: pre-wrap; }
        .last-req { margin-top: 16px; background: #fff5f5; border-right: 4px solid #f56565; padding: 12px; border-radius: 8px; font-size: 12px; color: #742a2a; display: none; }
        .trouble-box { margin-top: 18px; background: #fffaf0; border-right: 4px solid #dd6b20; padding: 14px; border-radius: 8px; }
        .trouble-box h3 { color: #9c4221; margin-bottom: 10px; font-size: 15px; }
        .trouble-box button { padding: 10px 18px; margin: 5px; border: none; border-radius: 8px; cursor: pointer; font-weight: bold; font-size: 13px; color: white; }
        .btn-red { background: #e53e3e; }
        .btn-orange { background: #dd6b20; }
        .btn-blue { background: #3182ce; }
        .trouble-box button:hover { filter: brightness(0.9); }
        .trouble-box .hint { color: #7b341e; font-size: 12px; line-height: 1.8; }
    </style>
</head>
<body>
    <div class="card">
        <div class="header">
            <h1>💊 نظام الصيدلية الذكية - WhatsApp</h1>
            <p>ربط حساب واتساب لإرسال الإشعارات</p>
        </div>
        <div id="statusBadge" class="status-badge status-waiting">جاري التحميل...</div>
        <div class="qr-container" id="qrContainer">
            <div id="qrcode"></div>
            <div id="qrPlaceholder" style="color:#718096; text-align:center;">
                <p style="font-size:16px; margin-bottom:10px;">⏳ جاري تشغيل الواتساب وإعداد المتصفح...</p>
                <div class="loading"></div>
                <p style="font-size:12px; margin-top:14px; color:#a0aec0;">قد تستغرق أول مرة 30-60 ثانية</p>
            </div>
        </div>
        <div class="time-info" id="timeInfo"></div>
        <button class="refresh-btn" onclick="location.reload()">🔄 تحديث الصفحة</button>

        <div class="trouble-box">
            <h3>🛠️ في حال مواجهة مشاكل:</h3>
            <button class="btn-orange" onclick="restartClient()">🔁 إعادة تشغيل عميل الواتساب</button>
            <button class="btn-red" onclick="resetAll()">🗑️ حذف الجلسة والبدء من الصفر</button>
            <div class="hint" style="margin-top:10px;">
                💡 تلميحات إصلاح سريعة:<br>
                1. إذا فُتحت نافذة Chrome فارغة → انتظر 60 ثانية أو اضغط إعادة تشغيل<br>
                2. إذا ظهر رمز QR في الـ Terminal ولكن ليس في المتصفح → اضغط تحديث الصفحة<br>
                3. إذا تم ربط الهاتف ولكن تكرر قطع الاتصال → استخدم خيار حذف الجلسة
            </div>
        </div>

        <div class="instructions">
            <h3>📋 خطوات الربط:</h3>
            <ol>
                <li>افتح تطبيق الواتساب على هاتفك</li>
                <li>القائمة (⋮) → الأجهزة المرتبطة → ربط جهاز</li>
                <li>امسح الرمز أعلاه</li>
                <li>انتظر حتى تظهر الشارة خضراء "✅ متصل"</li>
            </ol>
        </div>

        <div class="last-req" id="lastReq"></div>

        <div class="test-box">
            <h3>🧪 اختبار إرسال مباشر:</h3>
            <input id="testPhone" placeholder="رقم الهاتف (مثال: 779007753 أو 967779007753)" value="779007753" />
            <textarea id="testMsg" rows="3" placeholder="نص الرسالة">✅ اختبار إرسال من نظام الصيدلية الذكية - الساعة </textarea>
            <button onclick="sendTest()">📤 إرسال رسالة اختبار</button>
            <div id="testResult" class="test-result"></div>
        </div>
    </div>

    <script>
        let qrObj = null;
        function showQR(text) {
            document.getElementById('qrcode').innerHTML = '';
            if (!qrObj) qrObj = new QRCode(document.getElementById('qrcode'), { text, width: 256, height: 256, colorDark: '#1a202c', colorLight: '#fff', correctLevel: QRCode.CorrectLevel.H });
            else qrObj.makeCode(text);
            document.getElementById('qrPlaceholder').style.display = 'none';
        }
        function showLoading(msg) {
            document.getElementById('qrcode').innerHTML='';
            const ph = document.getElementById('qrPlaceholder');
            ph.style.display='block';
            if (msg) ph.querySelector('p').textContent = msg;
        }
        function showReady() {
            document.getElementById('qrcode').innerHTML='';
            document.getElementById('qrPlaceholder').style.display='none';
            document.getElementById('qrContainer').innerHTML = '<div style="text-align:center;"><div style="font-size:80px;margin-bottom:15px;">✅</div><p style="font-size:20px; font-weight:bold; color:#22543d;">تم الربط بنجاح!</p><p style="color:#48bb78; margin-top:8px;">خدمة الواتساب جاهزة لاستقبال الطلبات</p></div>';
        }
        async function checkStatus() {
            try {
                const res = await fetch('/api/status');
                const data = await res.json();
                const badge = document.getElementById('statusBadge');
                if (data.isReady) { badge.textContent='✅ متصل'; badge.className='status-badge status-ready'; showReady(); }
                else if (data.qr) { badge.textContent='⏳ بانتظار مسح QR'; badge.className='status-badge status-waiting'; showQR(data.qr); document.getElementById('timeInfo').textContent = 'توليد الرمز: ' + new Date(data.generatedAt).toLocaleString('ar-SA'); }
                else { badge.textContent='⚠️ جاري التهيئة / غير متصل'; badge.className='status-badge status-waiting'; showLoading(data.loadingMsg || 'جاري تشغيل الواتساب وإعداد المتصفح...'); if (data.lastDisconnectReason) document.getElementById('timeInfo').textContent = 'سبب قطع الاتصال: ' + data.lastDisconnectReason; }
                if (data.lastRequest) {
                    const el = document.getElementById('lastReq');
                    el.style.display='block';
                    el.innerHTML = '<b>آخر طلب وصل للخادم:</b><br>' + data.lastRequest;
                }
            } catch(e) {
                const b = document.getElementById('statusBadge'); b.textContent='❌ خطأ بالاتصال بالخادم'; b.className='status-badge status-error';
            }
        }
        async function sendTest() {
            const phone = document.getElementById('testPhone').value.trim();
            let msg = document.getElementById('testMsg').value;
            msg += new Date().toLocaleTimeString('ar-SA');
            const out = document.getElementById('testResult');
            out.style.display='block';
            out.style.background='#edf2f7';
            out.style.color='#2d3748';
            out.textContent = '⏳ جاري الإرسال...';
            try {
                const res = await fetch('/api/send-message', { method:'POST', headers:{'Content-Type':'application/json'}, body: JSON.stringify({ phoneNumber: phone, message: msg }) });
                const data = await res.json();
                out.style.background = data.success ? '#c6f6d5' : '#fed7d7';
                out.style.color = data.success ? '#22543d' : '#742a2a';
                out.textContent = JSON.stringify(data, null, 2);
            } catch(err) {
                out.style.background='#fed7d7'; out.style.color='#742a2a';
                out.textContent = '❌ خطأ: ' + err.toString();
            }
        }
        async function restartClient() {
            if (!confirm('هل تريد بالتأكيد إعادة تشغيل عميل الواتساب؟')) return;
            try {
                const res = await fetch('/api/admin/restart', { method:'POST' });
                const data = await res.json();
                alert(JSON.stringify(data));
                setTimeout(() => location.reload(), 2000);
            } catch(e) { alert('خطأ: ' + e); }
        }
        async function resetAll() {
            if (!confirm('⚠️ تحذير: سيتم حذف الجلسة بالكامل وستحتاج لمسح QR مرة أخرى. هل أنت متأكد؟')) return;
            try {
                const res = await fetch('/api/admin/reset', { method:'POST' });
                const data = await res.json();
                alert(JSON.stringify(data) + '\\nسيتم إعادة تشغيل الصفحة بعد 3 ثوانٍ');
                setTimeout(() => location.reload(), 3000);
            } catch(e) { alert('خطأ: ' + e); }
        }
        checkStatus(); setInterval(checkStatus, 3000);
    </script>
</body>
</html>
    `);
});

app.get('/api/status', (req, res) => {
    let loadingMsg = null;
    try {
        if (!currentQr && !isReady) {
            loadingMsg = initRetries > 0
                ? `جاري إعادة التهيئة... (المحاولة ${initRetries}/${MAX_INIT_RETRIES})`
                : 'جاري فتح المتصفح وتحميل صفحة الواتساب... (انتظر 30-60 ثانية)';
        }
    } catch(e) {}
    res.json({
        success: true,
        isReady: isReady,
        qr: currentQr,
        generatedAt: qrGeneratedAt,
        lastDisconnectReason: lastDisconnectReason,
        lastRequest: lastRequest,
        retries: initRetries,
        maxRetries: MAX_INIT_RETRIES,
        loadingMsg: loadingMsg
    });
});

app.post('/api/admin/restart', async (req, res) => {
    try {
        log('🔁 طلب إعادة تشغيل من واجهة الويب', 'ADMIN');
        safeDestroyAndReinit(false);
        res.json({ success: true, message: 'تم طلب إعادة التشغيل، سيتم إعادة التهيئة خلال ثوانٍ' });
    } catch(e) {
        res.status(500).json({ success: false, error: e.toString() });
    }
});

app.post('/api/admin/reset', async (req, res) => {
    try {
        log('🗑️ طلب حذف الجلسة والبدء من الصفر من واجهة الويب', 'ADMIN');
        if (client) {
            try { await client.destroy(); } catch(e) {}
            client = null;
            isReady = false;
        }
        stopKeepAlive();
        const errs = [];
        try { fs.rmSync(AUTH_DIR, { recursive: true, force: true }); log('✅ حذف مجلد الجلسة القديمة: ' + AUTH_DIR, 'ADMIN'); }
        catch(e) { errs.push('AUTH: ' + e.message); }

        const extraChromeDir = path.join(__dirname, '.chrome-data');
        try { if (fs.existsSync(extraChromeDir)) { fs.rmSync(extraChromeDir, { recursive: true, force: true }); log('✅ حذف مجلد Chrome القديم (إن وجد)', 'ADMIN'); } }
        catch(e) { errs.push('CHROME-CLEANUP: ' + e.message); }

        initRetries = 0;
        isReconnecting = false;
        setTimeout(() => createClient(), 1500);
        res.json({ success: true, message: 'تم حذف الجلسة وإعادة التهيئة', warnings: errs });
    } catch(e) {
        res.status(500).json({ success: false, error: e.toString() });
    }
});

app.post('/api/send-message', async (req, res) => {
    const startTime = Date.now();
    const { phoneNumber, message } = req.body;
    lastRequest = `الوقت: ${new Date().toLocaleString('ar-SA')}<br>الرقم المستلم: ${JSON.stringify(phoneNumber)}<br>طول الرسالة: ${(message||'').length}<br>الخادم جاهز: ${isReady}`;

    log(`========== طلب إرسال جديد ==========`, 'REQ');
    log(`phoneNumber المستلم = "${phoneNumber}" (النوع: ${typeof phoneNumber}, طول: ${phoneNumber? phoneNumber.length : 'N/A'})`, 'REQ');
    log(`isReady = ${isReady}`, 'REQ');

    if (!phoneNumber || !message) {
        log(`❌ فشل: بيانات ناقصة`, 'ERR');
        return res.status(400).json({ success: false, error: 'رقم الهاتف والنص مطلوبان.' });
    }

    let waitTries = 0;
    while ((!isReady || !client) && waitTries < 6 && !isReconnecting) {
        waitTries++;
        log(`⏳ الخدمة غير جاهزة حالياً - الانتظار 2 ثانية (المحاولة ${waitTries}/6)...`, 'WAIT');
        await new Promise(r => setTimeout(r, 2000));
    }

    if (isReconnecting && (!isReady || !client)) {
        log(`⏳ جاري إعادة تهيئة العميل حالياً - الانتظار 10 ثواني إضافية...`, 'WAIT');
        await new Promise(r => setTimeout(r, 10000));
    }

    if (!isReady || !client) {
        log(`❌ فشل: الخدمة غير جاهزة بعد الانتظار (isReady=${isReady}, client=${!!client}) - بدء إعادة تهيئة تلقائية`, 'ERR');
        safeDestroyAndReinit(false);
        return res.status(503).json({ success: false, error: 'خدمة الواتساب غير متصلة حالياً. يتم محاولة إعادة الاتصال تلقائياً، يرجى المحاولة بعد 30 ثانية.' });
    }

    let cleaned = String(phoneNumber).replace(/[^0-9]/g, '');
    if (cleaned.startsWith('0')) cleaned = cleaned.substring(1);
    if (!cleaned.startsWith('967')) {
        if (cleaned.length >= 9) {
            const last9 = cleaned.substring(cleaned.length - 9);
            cleaned = '967' + last9;
            log(`[NODE] إضافة كود الدولة -> ${cleaned}`, 'PHONE');
        } else if (cleaned.length > 0) {
            cleaned = '967' + cleaned;
        }
    }
    log(`[NODE] الرقم النهائي للبحث: "${cleaned}" (الطول: ${cleaned.length})`, 'PHONE');

    const chatId = `${cleaned}@c.us`;
    log(`[NODE] chatId = ${chatId}`, 'PHONE');

    const MAX_SEND_RETRIES = 3;
    let lastError = null;
    let lastResponse = null;

    for (let attempt = 1; attempt <= MAX_SEND_RETRIES; attempt++) {
        try {
            if (attempt > 1) {
                log(`🔄 محاولة الإرسال الثانية/الثالثة بعد فشل السابقة... (المحاولة ${attempt}/${MAX_SEND_RETRIES})`, 'RETRY');
                await new Promise(r => setTimeout(r, 2000 * attempt));
            }

            let registered = true;
            try {
                const t0 = Date.now();
                const regPromise = client.isRegisteredUser(chatId);
                const timeoutPromise = new Promise((_, rej) => setTimeout(() => rej(new Error('TIMEOUT_ISREG')), 15000));
                registered = await Promise.race([regPromise, timeoutPromise]);
                log(`📊 فحص isRegisteredUser (${Date.now()-t0}ms) = ${registered}`, 'CHECK');
            } catch (chk) {
                if (String(chk.message || '').includes('TIMEOUT_ISREG')) {
                    log(`⚠️ فحص isRegisteredUser تعطل (15 ثانية) - سيتم اعتبار الرقم صالحاً ومحاولة إعادة تهيئة العميل`, 'WARN');
                    registered = true;
                } else {
                    log(`⚠️ فشل فحص التسجيل (سيتم الإرسال على أي حال): ${chk.message}`, 'WARN');
                    registered = true;
                }
            }
            if (!registered) {
                const errMsg = `الرقم ${cleaned} غير مسجل في الواتساب (isRegisteredUser=false)`;
                log(`❌ ${errMsg}`, 'ERR');
                return res.status(400).json({ success: false, error: errMsg });
            }

            log(`📤 [المحاولة ${attempt}] client.sendMessage("${chatId}")`, 'SEND');
            const t1 = Date.now();
            const sendPromise = client.sendMessage(chatId, message);
            const sendTimeoutPromise = new Promise((_, rej) => setTimeout(() => rej(new Error('TIMEOUT_SEND')), 60000));
            const response = await Promise.race([sendPromise, sendTimeoutPromise]);
            const elapsed = Date.now() - t1;
            log(`✅ تم الإرسال! خلال ${elapsed}ms - ID: ${response && response.id ? response.id._serialized : 'N/A'}`, 'OK');

            lastSuccessfulSend = Date.now();
            clientStuckCount = 0;

            return res.status(200).json({
                success: true,
                message: 'تم الإرسال بنجاح',
                responseId: response && response.id ? response.id._serialized : null,
                debug: { receivedPhone: phoneNumber, cleanedPhone: cleaned, chatId, elapsedMs: elapsed, attempt }
            });

        } catch (error) {
            lastError = error;
            const elapsed = Date.now() - startTime;
            log(`❌ خطأ في الإرسال (المحاولة ${attempt}/${MAX_SEND_RETRIES}) بعد ${elapsed}ms: ${error}`, 'ERR');
            if (error && error.stack) log(`STACK: ${error.stack}`, 'ERR');

            if (String(error.message || '').includes('TIMEOUT_SEND') || String(error.message || '').includes('TIMEOUT_ISREG')) {
                log('🔴 تم الكشف عن تعطل في العميل أثناء الإرسال - زيادة عداد التعطل', 'ERR');
                clientStuckCount++;
                if (clientStuckCount >= 2 && attempt === MAX_SEND_RETRIES) {
                    log('🔴 تجاوز حد التعطل - بدء إعادة تهيئة تلقائية للعميل بعد إرسال الرد', 'ERR');
                    setTimeout(() => safeDestroyAndReinit(false), 3000);
                }
            }

            if (attempt < MAX_SEND_RETRIES) {
                try {
                    log(`🔍 التحقق من حالة العميل قبل إعادة المحاولة...`, 'RETRY');
                    const state = await client.getState();
                    log(`🔍 حالة العميل الحالية: ${state}`, 'RETRY');
                    if (!state || state !== 'CONNECTED') {
                        log('🔍 حالة العميل غير جيدة - محاولة إعادة تهيئة سريعة قبل إعادة المحاولة', 'RETRY');
                        break;
                    }
                } catch (stErr) {
                    log(`🔍 فشل فحص حالة العميل: ${stErr.message} - الخروج من محاولات الإرسال`, 'RETRY');
                    break;
                }
            }
        }
    }

    const totalElapsed = Date.now() - startTime;
    log(`❌ ❌ ❌ فشلت جميع محاولات الإرسال (${MAX_SEND_RETRIES}) بعد ${totalElapsed}ms - بدء إعادة تهيئة تلقائية`, 'FATAL_ERR');
    setTimeout(() => safeDestroyAndReinit(false), 2000);
    return res.status(500).json({
        success: false,
        error: lastError ? lastError.toString() : 'فشل غير معروف في الإرسال',
        stack: lastError && lastError.stack ? lastError.stack : null,
        hint: 'تم تشغيل آلية الاستعادة التلقائية - يرجى المحاولة مرة أخرى بعد 45 ثانية'
    });
});

app.listen(PORT, () => {
    log('\n' + '🚀'.repeat(10), 'START');
    log(`🚀 خادم الواتساب يعمل على: http://localhost:${PORT}`, 'START');
    log(`🌐 افتح في المتصفح الآن: http://localhost:${PORT}`, 'START');
    log('🚀'.repeat(10) + '\n', 'START');
});
