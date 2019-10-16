using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Schedule.ImageService.Services;

namespace Schedule.ImageService.Quartz
{
    public class UpdateJob : IJob
    {
        private readonly ILogger<UpdateJob> _logger;
        private IConverterService _converterService;

        public UpdateJob(ILogger<UpdateJob> logger, IConverterService converterService)
        {
            _logger = logger;
            _converterService = converterService;
        }
        
        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation($"Запуск сервиса обработки отчета по расписанию");
            await _converterService.UpdateStorage();
        }
    }
}