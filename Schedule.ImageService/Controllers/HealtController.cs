using System;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Schedule.ImageService.Services;

namespace Schedule.ImageService.Controllers
{
    public class HealthController : Controller
    {
        private ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        /// <summary> Если метод вернет ответ, то сервер работает </summary>
        [HttpGet("/")]
        public string Index()
        {
            _logger.LogInformation("Health check");
            return $"{DateTime.Now.ToString(CultureInfo.CurrentCulture)} Сервис обработки изображений работает в штатном режиме";
        }
    }
}