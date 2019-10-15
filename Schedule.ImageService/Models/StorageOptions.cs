namespace Schedule.ImageService.Models
{
    /// <summary> Object storage settings(S3) </summary>
    public class StorageOptions
    {
        public string Host { get; set; }
        
        public string Bucket { get; set; }
        
        public string AccessKey { get; set; }
        
        public string SecretKey { get; set; }
    }
}