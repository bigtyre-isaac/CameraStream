using CameraStream.Configuration;
using RestSharp;

namespace CameraStream
{
    public class CameraServer
    {
        private readonly List<Camera> _cameras = new();
        public IReadOnlyList<Camera> Cameras => _cameras.AsReadOnly();

        public CameraServer(IEnumerable<CameraSettings> settings)
        {
            foreach (var setting in settings)
            {
                _cameras.Add(new Camera(setting));
            }
        }

        public Camera GetCamera(int i)
        {
            return Cameras[i];
        }
    }

    public class Camera
    {
        public Camera(CameraSettings settings)
        {
            Settings = settings;
            Client = new RestClient();
        }

        public string Name => Settings.Name;
        public CameraSettings Settings { get; }
        public RestClient Client { get; }

        internal void UpdateCurrentFrame()
        {
            CurrentFrame = GetFrame();
        }

        public byte[] CurrentFrame { get; set; }

        public byte[] GetFrame()
        {

            var request = new RestRequest(Settings.Uri);

            try
            {
                var response = Client.DownloadData(request);
                
            }
            catch (Exception ex)
            {
                throw;
            }

            return Array.Empty<byte>();
        }
    }
}
