using System.Net;
using System.Text.Json;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "حدث خطأ غير متوقع: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            // القيم الافتراضية (للأخطاء غير المتوقعة)
            var statusCode = (int)HttpStatusCode.InternalServerError;
            var message = "حدث خطأ غير متوقع في الخادم";

            // تحديد الحالة بناءً على نوع الخطأ
            switch (ex)
            {
                case KeyNotFoundException:
                    statusCode = (int)HttpStatusCode.NotFound; // 404
                    message = ex.Message;
                    break;

                case InvalidOperationException:
                    statusCode = (int)HttpStatusCode.BadRequest; // 400
                    message = ex.Message; // هنا ستظهر رسالة "لا يمكن اعتماد فاتورة بإجمالي صفر"
                    break;

                case ArgumentException:
                    statusCode = (int)HttpStatusCode.BadRequest; // 400
                    message = ex.Message;
                    break;

                case UnauthorizedAccessException:
                    statusCode = (int)HttpStatusCode.Forbidden; // 403
                    message = "ليس لديك صلاحية للقيام بهذا الإجراء";
                    break;

                default:
                    statusCode = (int)HttpStatusCode.InternalServerError; // 500
                    message = "حدث خطأ داخلي، يرجى التواصل مع الدعم الفني";
                    break;
            }

            context.Response.StatusCode = statusCode;

            // استخدام الـ Wrapper الخاص بك لتوحيد شكل الرد
            var responseResponse = ApiResponse<object>.Failed(message, statusCode);

            // إعدادات الـ JSON لضمان خروج البيانات بشكل CamelCase (اختياري)
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(responseResponse, options);

            return context.Response.WriteAsync(json);
        }
    }
}
