using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CameraStream
{
    [Route("api/[controller]")]
    [ApiController]
    public class CamerasController : ControllerBase
    {
        private CameraServer CameraServer { get; }

        public CamerasController(CameraServer cameraServer)
        {
            CameraServer = cameraServer;
        }

        [HttpGet("{cameraId}/stream")]
        public IActionResult GetStreamAsMjpeg(int cameraId)
        {
            var camera = CameraServer.GetCamera(cameraId);
            var currentFrame = camera.CurrentFrame;

            var frameRate = 30;
            var timePerFrame = TimeSpan.FromMilliseconds(Math.Floor(1000f / frameRate));

            var first = true;
            return new MjpegStreamContent(
                async cancellationToken =>
                {
                    if (first)
                        first = false; // no delay for first image
                    else
                        await Task.Delay(timePerFrame, cancellationToken); // 2 second delay between image updates

                    return camera.CurrentFrame;
                }
            );
        }

        [HttpGet("{cameraId}/snapshot")]
        public IActionResult GetCurrentFrame(int cameraId)
        {
            var camera = CameraServer.GetCamera(cameraId);
            var currentFrame = camera.CurrentFrame;

            return File(currentFrame, "image/jpeg");
        }
    }
}
