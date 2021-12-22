using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;

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
        public IActionResult GetFrame(int cameraId)
        {
            var camera = CameraServer.GetCamera(cameraId);
            var currentFrame = camera.CurrentFrame;

            var first = true;
            return new MjpegStreamContent(
                async cancellationToken =>
                {
                    if (first)
                        first = false; // no delay for first image
                    else
                        await Task.Delay(2000, cancellationToken); // 2 second delay between image updates

                    return camera.CurrentFrame;
                }
            );
        }
    }

    public class MjpegStreamContent : IActionResult
    {
        private static readonly string _boundary = "MotionImageStream";
        private static readonly string _contentType = "multipart/x-mixed-replace;boundary=" + _boundary;
        private static readonly byte[] _newLine = Encoding.UTF8.GetBytes("\r\n");

        private readonly Func<CancellationToken, Task<byte[]>> _onNextImage;
        private readonly Action? _onEnd;

        public MjpegStreamContent(Func<CancellationToken, Task<byte[]>> onNextImage, Action? onEnd = null)
        {
            _onNextImage = onNextImage;
            _onEnd = onEnd;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.ContentType = _contentType;

            var outputStream = context.HttpContext.Response.Body;
            var cancellationToken = context.HttpContext.RequestAborted;

            try
            {
                while (true)
                {
                    var imageBytes = await _onNextImage(cancellationToken);

                    var header = $"--{_boundary}\r\nContent-Type: image/jpeg\r\nContent-Length: {imageBytes.Length}\r\n\r\n";
                    var headerData = Encoding.UTF8.GetBytes(header);
                    await outputStream.WriteAsync(headerData, 0, headerData.Length, cancellationToken);
                    await outputStream.WriteAsync(imageBytes, 0, imageBytes.Length, cancellationToken);
                    await outputStream.WriteAsync(_newLine, 0, _newLine.Length, cancellationToken);

                    if (cancellationToken.IsCancellationRequested)
                        break;
                }
            }
            catch (TaskCanceledException)
            {
                // connection closed, no need to report this
            }
            finally
            {
                _onEnd?.Invoke();
            }
        }
    }
}
