using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Newtonsoft.Json;
using Schedule.ImageService.Models;
using SelectPdf;

namespace Schedule.ImageService.Services.Impl
{
    public class ConverterService : IConverterService
    {
        private readonly IHtmlParser _htmlParser;

        private readonly HtmlToImage _converterHtmlToImage;

        private StorageOptions _options;

        private MinioClient _minio;

        private IHttpClientFactory _httpClientFactory;

        private ILogger<ConverterService> _logger;

        private const string ApiUrl = "https://shelly.kpfu.ru/e-ksu";

        private const string KpfuUrl = "https://kpfu.ru";

        public ConverterService(IHtmlParser htmlParser, IOptions<StorageOptions> options,
            IHttpClientFactory httpClientFactory, ILogger<ConverterService> logger)
        {
            _htmlParser = htmlParser;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _converterHtmlToImage = new HtmlToImage();
            _options = options.Value;
            _minio = new MinioClient(_options.Host, _options.AccessKey, _options.SecretKey);
        }

        public async Task UpdateStorage()
        {
            _logger.LogInformation("Начало обновления базы");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var groups = await GetGroups();

            try
            {
                Parallel.ForEach(groups, group =>
                {
                    UploadImage(group).GetAwaiter().GetResult();
                });
                
            }
            catch (Exception e)
            {
                _logger.LogError(JsonConvert.SerializeObject(e));
            }

            stopwatch.Stop();
            _logger.LogInformation($"База изображений обновлена. Затрачено {stopwatch.Elapsed}");
        }

        private async Task<List<Group>> GetGroups()
        {
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var response = await httpClient.GetAsync(
                    $"{ApiUrl}/portal_pg_mobile.get_group_list");
                var json = await response.Content.ReadAsStringAsync();
                var groupsRoot = JsonConvert.DeserializeObject<KpfuGroupRoot>(json);

                return groupsRoot.Groups.ToList();
            }
        }

        private async Task UploadImage(Group group)
        {
            try
            {
                using (var httpClient = _httpClientFactory.CreateClient())
                {
                    var bytes = await httpClient.GetByteArrayAsync(
                        $"{KpfuUrl}/week_sheadule_print?p_group_name={@group.GroupName}");
                    var encoding = CodePagesEncodingProvider.Instance.GetEncoding(1251);
                    var htmlPage = encoding.GetString(bytes, 0, bytes.Length);

                    if (htmlPage.Contains("Расписание не найдено"))
                    {
                        _logger.LogWarning($"Расписание не найдено, группа {group.GroupName}");
                        return;
                    }
                    
                    for (var i = 1; i <= 6; i++)
                    {
                        try
                        {
                            var htmlDocument = await _htmlParser.ParseDay(htmlPage, i);
                            _converterHtmlToImage.WebPageWidth = 600;
                            var image = _converterHtmlToImage.ConvertHtmlString(htmlDocument);
                            using (var ms = new MemoryStream())
                            {
                                image.Save(ms, ImageFormat.Png);
                                ms.Seek(0, SeekOrigin.Begin);
                                await _minio.PutObjectAsync("kpfu",
                                    $"{group.GroupName}_day{i}.png", ms, ms.Length,
                                    "image/png");
                            }

                            image.Dispose();

                        }
                        catch (Exception e)
                        {
                            _logger.LogError($"Ошибка обработки группы {group.GroupName}, день {i}");
                            _logger.LogError(JsonConvert.SerializeObject(e));
                        }
                        
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Ошибка обработки группы {group.GroupName}");
                _logger.LogError(JsonConvert.SerializeObject(e));
            }
        }
    }
}