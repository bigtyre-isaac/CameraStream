namespace CameraStream.Configuration
{
    public class AppSettings
    {
        public List<CameraSettings> Cameras { get; } = new List<CameraSettings>();
    }

    public class CameraSettings
    {
        public string Name { get; set; }
        public Uri Uri { get; set; }
    }
}
