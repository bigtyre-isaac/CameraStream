using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CameraStream.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger, CameraServer cameraServer)
        {
            _logger = logger;
            CameraServer = cameraServer;
        }

        private CameraServer CameraServer { get; }
        public IReadOnlyList<CameraViewModel> Cameras { get; private set; }

        public void OnGet()
        {
            var cameras = CameraServer.Cameras.Select((c, i) => new CameraViewModel()
            {
                StreamUri = new Uri($"api/cameras/{i+1}/stream", UriKind.Relative),
                SnapshotUri = new Uri($"api/cameras/{i+1}/snapshot", UriKind.Relative),
                Name = c.Name
            }).ToList();

            Cameras = cameras.AsReadOnly();
        }
    }

    public class CameraViewModel
    {
        public Uri StreamUri { get; set; }
        public Uri SnapshotUri { get; set; }
        public string Name { get; set; }
    }
}