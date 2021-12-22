namespace CameraStream
{
    public class CameraUpdateService : BackgroundService
    {
        public CameraUpdateService(CameraServer server)
        {
            Server = server;
        }

        int FramesPerSecond { get; } = 10;

        public CameraServer Server { get; set; }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    UpdateCameras();
                }
                catch (Exception ex)
                {

                }
                await Task.Delay(1000 / FramesPerSecond, stoppingToken).ConfigureAwait(false);
            }
        }

        private void UpdateCameras()
        {
            var cameras = Server.Cameras;
            foreach (var camera in cameras)
            {
                try
                {
                    camera.UpdateCurrentFrame();
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
