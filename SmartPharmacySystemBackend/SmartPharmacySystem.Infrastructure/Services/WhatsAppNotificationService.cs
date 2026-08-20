using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.IServices;

namespace SmartPharmacySystem.Infrastructure.Services
{
    public class WhatsAppNotificationService : IWhatsAppNotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WhatsAppNotificationService> _logger;
        private readonly string _whatsappApiUrl;
        private readonly string _whatsappStatusUrl;
        private readonly string _whatsappBaseUrl;
        private readonly string _defaultCountryCode = "967";
        private readonly bool _diagnosticMode;
        private readonly bool _healthCheckBeforeSend;

        public WhatsAppNotificationService(HttpClient httpClient, IConfiguration configuration, ILogger<WhatsAppNotificationService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var configuredUrl = configuration["WhatsAppSettings:ApiUrl"];
            _whatsappApiUrl = !string.IsNullOrWhiteSpace(configuredUrl)
                ? configuredUrl
                : "http://localhost:3000/api/send-message";

            try
            {
                var sendUri = new Uri(_whatsappApiUrl);
                _whatsappBaseUrl = $"{sendUri.Scheme}://{sendUri.Host}:{sendUri.Port}";
                _whatsappStatusUrl = $"{_whatsappBaseUrl}/api/status";
            }
            catch
            {
                _whatsappBaseUrl = "http://localhost:3000";
                _whatsappStatusUrl = "http://localhost:3000/api/status";
            }

            var diagMode = configuration["WhatsAppSettings:DiagnosticMode"];
            _diagnosticMode = !string.IsNullOrWhiteSpace(diagMode) && bool.TryParse(diagMode, out var dm) && dm;

            var hcBefore = configuration["WhatsAppSettings:HealthCheckBeforeSend"];
            _healthCheckBeforeSend = !string.IsNullOrWhiteSpace(hcBefore) ? bool.TryParse(hcBefore, out var hc) && hc : true;

            _logger.LogInformation("========================================");
            _logger.LogInformation("🏥 WhatsApp Service initialized with ADVANCED diagnostics");
            _logger.LogInformation("📤 Send API URL: {Url}", _whatsappApiUrl);
            _logger.LogInformation("🩺 Status URL: {Url}", _whatsappStatusUrl);
            _logger.LogInformation("🌐 Base URL: {Url}", _whatsappBaseUrl);
            _logger.LogInformation("🌍 Default Country Code: {Code}", _defaultCountryCode);
            _logger.LogInformation("🔍 DiagnosticMode: {Diag}", _diagnosticMode);
            _logger.LogInformation("🩺 HealthCheckBeforeSend: {HC}", _healthCheckBeforeSend);
            _logger.LogInformation("========================================");

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(5000);
                    var bootCheck = await CheckHealthAsync();
                    if (_diagnosticMode || !bootCheck.IsReady)
                    {
                        _logger.LogWarning("========================================");
                        _logger.LogWarning("🏥 BOOT HEALTH CHECK RESULT:");
                        _logger.LogWarning("  IsAvailable (HTTP reachable): {A}", bootCheck.IsAvailable);
                        _logger.LogWarning("  IsReady (WhatsApp logged in): {R}", bootCheck.IsReady);
                        _logger.LogWarning("  HTTP Status: {Code}", bootCheck.HttpStatusCode);
                        _logger.LogWarning("  Response Time: {Ms}ms", bootCheck.ResponseTimeMs.ToString("F0"));
                        if (!string.IsNullOrWhiteSpace(bootCheck.ErrorMessage))
                            _logger.LogWarning("  Error: {Err}", bootCheck.ErrorMessage);
                        if (!string.IsNullOrWhiteSpace(bootCheck.LastDisconnectReason))
                            _logger.LogWarning("  Last Disconnect: {R}", bootCheck.LastDisconnectReason);
                        if (!string.IsNullOrWhiteSpace(bootCheck.QrCode))
                            _logger.LogWarning("  QR Code is AVAILABLE - open {Base} to scan it!", _whatsappBaseUrl);
                        _logger.LogWarning("  Retries: {Ret}/{Max}", bootCheck.Retries, 5);
                        _logger.LogWarning("  💡 QUICK FIX: Open {Base} in browser and scan QR!", _whatsappBaseUrl);
                        _logger.LogWarning("========================================");
                    }
                }
                catch (Exception bootEx)
                {
                    _logger.LogWarning(bootEx, "Boot health check failed (non-fatal, IGNORED to protect API stability)");
                }
            }).ContinueWith(t =>
            {
                if (t.IsFaulted && t.Exception != null)
                {
                    _logger.LogCritical(t.Exception, "FATAL GUARD: Boot health check OBSERVED UNHANDLED exception in finalizer guard. API process is SAFE.");
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public async Task<WhatsAppHealthResult> CheckHealthAsync()
        {
            var result = new WhatsAppHealthResult();
            var sw = Stopwatch.StartNew();
            try
            {
                _logger.LogDebug("🩺 CheckHealthAsync: GET {Url}", _whatsappStatusUrl);
                using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(10)))
                {
                    var response = await _httpClient.GetAsync(_whatsappStatusUrl, cts.Token);
                    sw.Stop();
                    result.ResponseTimeMs = sw.Elapsed.TotalMilliseconds;
                    result.HttpStatusCode = (int)response.StatusCode;
                    result.IsAvailable = response.IsSuccessStatusCode;

                    var body = await response.Content.ReadAsStringAsync();
                    result.RawResponse = body.Length > 2000 ? body.Substring(0, 2000) : body;

                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            using (var doc = JsonDocument.Parse(body))
                            {
                                var root = doc.RootElement;
                                if (root.TryGetProperty("isReady", out var ready))
                                    result.IsReady = ready.ValueKind == JsonValueKind.True;
                                if (root.TryGetProperty("qr", out var qr) && qr.ValueKind == JsonValueKind.String)
                                    result.QrCode = qr.GetString();
                                if (root.TryGetProperty("generatedAt", out var gen))
                                {
                                    if (DateTime.TryParse(gen.GetString(), out var dt))
                                        result.QrGeneratedAt = dt;
                                }
                                if (root.TryGetProperty("lastDisconnectReason", out var disc) && disc.ValueKind == JsonValueKind.String)
                                    result.LastDisconnectReason = disc.GetString();
                                if (root.TryGetProperty("retries", out var ret) && ret.TryGetInt32(out var rv))
                                    result.Retries = rv;
                            }
                        }
                        catch (Exception jsonEx)
                        {
                            _logger.LogWarning(jsonEx, "CheckHealth: Failed to parse status JSON");
                        }
                    }

                    _logger.LogDebug("🩺 CheckHealthAsync Result: Available={A}, Ready={R}, HTTP={Code}, {Ms}ms",
                        result.IsAvailable, result.IsReady, result.HttpStatusCode, result.ResponseTimeMs.ToString("F0"));
                }
            }
            catch (TaskCanceledException)
            {
                sw.Stop();
                result.ResponseTimeMs = sw.Elapsed.TotalMilliseconds;
                result.IsAvailable = false;
                result.ErrorMessage = "TIMEOUT: WhatsApp API server did not respond within 10 seconds - is it running on " + _whatsappBaseUrl + "?";
                _logger.LogError("❌ CheckHealth TIMEOUT after {Ms}ms - WhatsApp API server is UNREACHABLE at {Base}",
                    result.ResponseTimeMs.ToString("F0"), _whatsappBaseUrl);
            }
            catch (HttpRequestException hre)
            {
                sw.Stop();
                result.ResponseTimeMs = sw.Elapsed.TotalMilliseconds;
                result.IsAvailable = false;
                result.ErrorMessage = "CONNECTION ERROR: " + hre.Message;
                _logger.LogError(hre, "❌ CheckHealth HttpRequestException - WhatsApp API server DOWN at {Base}. Hint: run `npm start` in SmartPharmacyWhatsAppApi folder.", _whatsappBaseUrl);
            }
            catch (Exception ex)
            {
                sw.Stop();
                result.ResponseTimeMs = sw.Elapsed.TotalMilliseconds;
                result.IsAvailable = false;
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "❌ CheckHealth GENERIC EXCEPTION");
            }
            return result;
        }

        public async Task<bool> SendMessageAsync(string phoneNumber, string message)
        {
            _logger.LogInformation("========================================");
            _logger.LogInformation("WhatsApp SendMessageAsync INVOKED.");
            _logger.LogInformation("Raw phoneNumber: '{Phone}'", phoneNumber ?? "(NULL)");
            _logger.LogInformation("Message length: {Len} chars", message?.Length ?? 0);
            _logger.LogInformation("========================================");

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                _logger.LogError("WhatsApp ABORT: phoneNumber is empty/null.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                _logger.LogError("WhatsApp ABORT: message is empty/null.");
                return false;
            }

            string safeMessage = message;
            try
            {
                safeMessage = Regex.Replace(safeMessage, @"[\u0000-\u0008\u000B\u000C\u000E-\u001F\u007F]", string.Empty);
                safeMessage = Regex.Replace(safeMessage, @"[\u202A-\u202E\u200E\u200F]", " ");
                safeMessage = Regex.Replace(safeMessage, @"[\r\n]{3,}", "\n\n");
                safeMessage = safeMessage.Trim();

                const int MAX_MESSAGE_LENGTH = 6000;
                if (safeMessage.Length > MAX_MESSAGE_LENGTH)
                {
                    _logger.LogWarning("WhatsApp: Message too long ({Len} > {Max}), TRUNCATING.", safeMessage.Length, MAX_MESSAGE_LENGTH);
                    safeMessage = safeMessage.Substring(0, MAX_MESSAGE_LENGTH - 20) + "\n... (تم اقتطاع الرسالة)";
                }

                if (safeMessage.Length == 0)
                {
                    _logger.LogError("WhatsApp ABORT: message became empty after sanitization.");
                    return false;
                }
            }
            catch (Exception sanitizeEx)
            {
                _logger.LogWarning(sanitizeEx, "WhatsApp: Message sanitization failed, continuing with raw message (safe fallback).");
                safeMessage = message;
                if (safeMessage.Length > 6000)
                    safeMessage = safeMessage.Substring(0, 5980) + "...";
            }

            try
            {
                if (_healthCheckBeforeSend)
                {
                    _logger.LogInformation("🩺 Step 0 - Pre-flight health check to WhatsApp API...");
                    try
                    {
                        var hc = await CheckHealthAsync();
                        if (!hc.IsAvailable)
                        {
                            _logger.LogCritical("========================================");
                            _logger.LogCritical("❌ FATAL: WhatsApp API SERVER IS NOT RUNNING / UNREACHABLE");
                            _logger.LogCritical("  HTTP Result: IsAvailable=false, HTTP={Code}, Time={Ms}ms", hc.HttpStatusCode, hc.ResponseTimeMs.ToString("F0"));
                            if (!string.IsNullOrWhiteSpace(hc.ErrorMessage)) _logger.LogCritical("  Details: {Err}", hc.ErrorMessage);
                            _logger.LogCritical("  👉 HOW TO FIX:");
                            _logger.LogCritical("     1. Open Terminal/CMD and go to: cd d:\\MyPharmacyProject\\SmartPharmacyWhatsAppApi");
                            _logger.LogCritical("     2. Run: npm install  (first time only)");
                            _logger.LogCritical("     3. Run: npm start");
                            _logger.LogCritical("     4. Open browser: {Base}", _whatsappBaseUrl);
                            _logger.LogCritical("========================================");
                            return false;
                        }
                        if (!hc.IsReady)
                        {
                            _logger.LogWarning("========================================");
                            _logger.LogWarning("⚠️  WARNING: WhatsApp API is RUNNING but NOT LOGGED IN (isReady=false)");
                            _logger.LogWarning("  HTTP Code: {Code}, Retries: {Ret}/5, Time={Ms}ms", hc.HttpStatusCode, hc.Retries, hc.ResponseTimeMs.ToString("F0"));
                            if (!string.IsNullOrWhiteSpace(hc.LastDisconnectReason))
                                _logger.LogWarning("  Last Disconnect Reason: {R}", hc.LastDisconnectReason);
                            if (!string.IsNullOrWhiteSpace(hc.QrCode))
                                _logger.LogWarning("  📱 QR CODE IS AVAILABLE - needs scan!");
                            _logger.LogWarning("  👉 QUICK FIX: Open browser → {Base} → scan QR code with phone", _whatsappBaseUrl);
                            _logger.LogWarning("  (Proceeding with send attempt anyway...)");
                            _logger.LogWarning("========================================");
                        }
                        else
                        {
                            _logger.LogInformation("✅ Step 0 - Health check PASSED: WhatsApp is READY! ({Ms}ms)", hc.ResponseTimeMs.ToString("F0"));
                        }
                    }
                    catch (Exception hcEx)
                    {
                        _logger.LogWarning(hcEx, "⚠️  Step 0 - Health check threw exception (non-fatal, proceeding with send anyway)");
                    }
                }

                string cleanedNumber = Regex.Replace(phoneNumber, "[^0-9]", "");
                _logger.LogInformation("Step 1 - digits only: '{Cleaned}' (len={Len})", cleanedNumber, cleanedNumber.Length);

                if (cleanedNumber.Length == 0)
                {
                    _logger.LogError("WhatsApp ABORT: phone number has zero digits after cleaning. Raw: '{Raw}'", phoneNumber);
                    return false;
                }

                cleanedNumber = cleanedNumber.TrimStart('0');
                _logger.LogInformation("Step 2 - remove leading zeros: '{Cleaned}' (len={Len})", cleanedNumber, cleanedNumber.Length);

                if (cleanedNumber.StartsWith(_defaultCountryCode))
                {
                    _logger.LogInformation("Step 3 - already has country code. Keeping as-is.");
                }
                else
                {
                    if (cleanedNumber.Length >= 9)
                    {
                        string last9 = cleanedNumber.Substring(cleanedNumber.Length - 9, 9);
                        char firstDigit = last9[0];
                        if (firstDigit == '7')
                        {
                            cleanedNumber = _defaultCountryCode + last9;
                            _logger.LogInformation("Step 3A - Yemeni pattern detected (starts with 7, 9 digits). New number: '{Cleaned}'", cleanedNumber);
                        }
                        else
                        {
                            cleanedNumber = _defaultCountryCode + last9;
                            _logger.LogInformation("Step 3B - No country code, appending default (last 9 digits). New number: '{Cleaned}'", cleanedNumber);
                        }
                    }
                    else if (cleanedNumber.Length > 0)
                    {
                        cleanedNumber = _defaultCountryCode + cleanedNumber;
                        _logger.LogInformation("Step 3C - Short number (<9 digits). Prepending default. New number: '{Cleaned}'", cleanedNumber);
                    }
                }

                if (cleanedNumber.Length < 10)
                {
                    _logger.LogWarning("Step 4 - WARNING: final number length is only {Len} ('{Cleaned}') - proceeding anyway", cleanedNumber.Length, cleanedNumber);
                }
                else
                {
                    _logger.LogInformation("Step 4 - Final number to send: '{Cleaned}' (length {Len})", cleanedNumber, cleanedNumber.Length);
                }

                _logger.LogInformation("Step 5 - Safe message length: {Len} chars (first 200): {Preview}",
                    safeMessage.Length,
                    safeMessage.Length > 200 ? safeMessage.Substring(0, 200) + "..." : safeMessage);

                var requestPayload = new
                {
                    phoneNumber = cleanedNumber,
                    message = safeMessage
                };

                string jsonPayload;
                try
                {
                    jsonPayload = JsonSerializer.Serialize(requestPayload);
                }
                catch (Exception jsonEx)
                {
                    _logger.LogError(jsonEx, "WhatsApp ABORT: JSON serialization FAILED. Falling back to simple object.");
                    var fallbackPayload = new
                    {
                        phoneNumber = cleanedNumber,
                        message = "رسالة من النظام - تعذر تنسيق الرسالة. يرجى مراجعة التطبيق."
                    };
                    jsonPayload = JsonSerializer.Serialize(fallbackPayload);
                }

                _logger.LogInformation("Step 6 - JSON payload length: {Len} chars (first 300): {Json}",
                    jsonPayload.Length,
                    jsonPayload.Length > 300 ? jsonPayload.Substring(0, 300) + "..." : jsonPayload);

                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                _logger.LogInformation("Step 7 - HTTP POST to {Url}", _whatsappApiUrl);

                using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(45)))
                {
                    try
                    {
                        var response = await _httpClient.PostAsync(_whatsappApiUrl, content, cts.Token);
                        string responseContent = await response.Content.ReadAsStringAsync();

                        _logger.LogInformation("Step 8 - Response: HTTP {Status} ({Code}). Body length: {Len}. Body preview: {Body}",
                            response.StatusCode, (int)response.StatusCode,
                            responseContent?.Length ?? 0,
                            string.IsNullOrEmpty(responseContent) ? "(EMPTY)" : (responseContent.Length > 300 ? responseContent.Substring(0, 300) + "..." : responseContent));

                        if (response.IsSuccessStatusCode)
                        {
                            bool apiOk = true;
                            string apiError = null;
                            try
                            {
                                if (!string.IsNullOrWhiteSpace(responseContent))
                                {
                                    using (var doc = JsonDocument.Parse(responseContent))
                                    {
                                        if (doc.RootElement.TryGetProperty("success", out var successProp))
                                        {
                                            if (successProp.ValueKind == JsonValueKind.False)
                                            {
                                                apiOk = false;
                                            }
                                            else if (successProp.ValueKind == JsonValueKind.True && !successProp.GetBoolean())
                                            {
                                                apiOk = false;
                                            }
                                        }
                                        if (doc.RootElement.TryGetProperty("error", out var errProp) && errProp.GetString() != null)
                                        {
                                            apiError = errProp.GetString();
                                            if (!string.IsNullOrWhiteSpace(apiError)) apiOk = false;
                                        }
                                        if (doc.RootElement.TryGetProperty("status", out var statusProp))
                                        {
                                            var statusStr = statusProp.GetString();
                                            if (!string.IsNullOrWhiteSpace(statusStr) &&
                                                (statusStr.Equals("error", StringComparison.OrdinalIgnoreCase) ||
                                                 statusStr.Equals("fail", StringComparison.OrdinalIgnoreCase) ||
                                                 statusStr.Equals("failed", StringComparison.OrdinalIgnoreCase)))
                                            {
                                                apiOk = false;
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception jsonEx)
                            {
                                _logger.LogWarning(jsonEx, "Step 8B - Failed to parse response JSON (but HTTP is 2xx). Treating as success.");
                                apiOk = true;
                            }

                            if (!apiOk)
                            {
                                _logger.LogError("Step 8B - API returned success=false/error. Server error: {Err}", apiError ?? "(unknown)");
                                return false;
                            }

                            _logger.LogInformation("✅ WhatsApp SUCCESS: Message sent to {Cleaned}", cleanedNumber);
                            return true;
                        }
                        else
                        {
                            var rawBody = string.IsNullOrEmpty(responseContent) ? "(EMPTY)" : (responseContent.Length > 2000 ? responseContent.Substring(0, 2000) : responseContent);
                            string? serverError = null;
                            bool needQr = false;
                            bool notRegistered = false;
                            try
                            {
                                if (!string.IsNullOrWhiteSpace(responseContent))
                                {
                                    using var doc = JsonDocument.Parse(responseContent);
                                    if (doc.RootElement.TryGetProperty("error", out var errProp) && errProp.ValueKind == JsonValueKind.String)
                                    {
                                        serverError = errProp.GetString();
                                        if (!string.IsNullOrWhiteSpace(serverError))
                                        {
                                            if (serverError.Contains("QR") || serverError.Contains("غير متصلة") || serverError.Contains("غير جاهزة"))
                                                needQr = true;
                                            if (serverError.Contains("غير مسجل") || serverError.Contains("isRegisteredUser=false") || serverError.Contains("not registered"))
                                                notRegistered = true;
                                        }
                                    }
                                }
                            }
                            catch { }

                            _logger.LogError("========================================");
                            _logger.LogError("❌ WhatsApp FAIL — DETAILED DIAGNOSTICS");
                            _logger.LogError("  HTTP Status: {Status} ({Code})", response.StatusCode, (int)response.StatusCode);
                            _logger.LogError("  Target URL: {Url}", _whatsappApiUrl);
                            _logger.LogError("  Phone Number: {Cleaned}", cleanedNumber);
                            _logger.LogError("  Message Length: {Len} chars", safeMessage.Length);
                            _logger.LogError("  Server Error Message: {SrvErr}", serverError ?? "(none parsed)");
                            _logger.LogError("  Raw Response Body: {Body}", rawBody);
                            _logger.LogError("----------------------------------------");

                            if ((int)response.StatusCode == 503 || needQr)
                            {
                                _logger.LogError("🔴 ROOT CAUSE: WhatsApp API SERVICE IS NOT LOGGED IN (503 ServiceUnavailable)");
                                _logger.LogError("  The Node.js server at {Base} is running, but WhatsApp is NOT connected.", _whatsappBaseUrl);
                                _logger.LogError("  Server said: {Msg}", serverError ?? "(no server error message)");
                                _logger.LogError("========================================");
                                _logger.LogError("👉 STEP-BY-STEP FIX:");
                                _logger.LogError("   1. Open ANY web browser (Chrome/Edge) on this computer");
                                _logger.LogError("   2. Go to URL: {Base}", _whatsappBaseUrl);
                                _logger.LogError("   3. You will see a QR code on the page");
                                _logger.LogError("   4. On YOUR PHONE open WhatsApp → ⋮ Menu → Linked Devices → Link a Device");
                                _logger.LogError("   5. Scan the QR code from the browser");
                                _logger.LogError("   6. Wait until page shows ✅ 'تم الربط بنجاح!' (green badge)");
                                _logger.LogError("   7. After that, try sending the invoice/sale again");
                                _logger.LogError("========================================");
                                _logger.LogError("🛠️  TROUBLESHOOTING (if still not working):");
                                _logger.LogError("   • Is Node.js WhatsApp service actually running? Check it's not crashed.");
                                _logger.LogError("   • Open the Node.js terminal window to see real-time logs");
                                _logger.LogError("   • If QR code is old/expired, click 🔄 'تحديث الصفحة' in browser");
                                _logger.LogError("   • If session is corrupted, use the 🗑️ 'حذف الجلسة والبدء من الصفر' button");
                                _logger.LogError("   • Also check: {Base}/api/status (JSON status endpoint)", _whatsappBaseUrl);
                                _logger.LogError("========================================");
                            }
                            else if (notRegistered)
                            {
                                _logger.LogError("🟡 ROOT CAUSE: Phone number NOT registered on WhatsApp: {Cleaned}", cleanedNumber);
                                _logger.LogError("  The customer's phone number doesn't have an active WhatsApp account.");
                            }
                            else if ((int)response.StatusCode >= 500)
                            {
                                _logger.LogError("🔴 ROOT CAUSE: WhatsApp API SERVER INTERNAL ERROR (5xx)");
                                _logger.LogError("  Check the Node.js terminal window (SmartPharmacyWhatsAppApi) for stack trace errors");
                            }
                            else if ((int)response.StatusCode == 400)
                            {
                                _logger.LogError("🟡 ROOT CAUSE: BAD REQUEST (400) - check phone number format or message content");
                            }
                            else if ((int)response.StatusCode == 404)
                            {
                                _logger.LogError("🔴 ROOT CAUSE: ENDPOINT NOT FOUND (404) - wrong URL in appsettings.json WhatsAppSettings:ApiUrl");
                                _logger.LogError("  Current: {Url}", _whatsappApiUrl);
                                _logger.LogError("  Expected: {Base}/api/send-message", _whatsappBaseUrl);
                            }

                            _logger.LogError("========================================");
                            return false;
                        }
                    }
                    catch (TaskCanceledException tce) when (cts.IsCancellationRequested)
                    {
                        _logger.LogError(tce, "❌ WhatsApp EXCEPTION: TIMEOUT after 45s. URL={Url}, BaseAddress={Base}",
                            _whatsappApiUrl, _httpClient.BaseAddress?.ToString() ?? "(null)");
                        return false;
                    }
                }
            }
            catch (TaskCanceledException tce)
            {
                _logger.LogError(tce, "❌ WhatsApp EXCEPTION: TIMEOUT (outer catch). URL={Url}, BaseAddress={Base}",
                    _whatsappApiUrl, _httpClient.BaseAddress?.ToString() ?? "(null)");
                return false;
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, "❌ WhatsApp EXCEPTION: HttpRequestException. URL={Url}, BaseAddress={Base}, Message={Msg}",
                    _whatsappApiUrl, _httpClient.BaseAddress?.ToString() ?? "(null)", hre.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ WhatsApp EXCEPTION: Generic catch-all. Type={Type}, Message={Msg}", ex.GetType().Name, ex.Message);
                return false;
            }
        }
    }
}
