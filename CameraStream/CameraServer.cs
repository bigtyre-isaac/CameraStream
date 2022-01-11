using CameraStream.Configuration;
using log4net;
using RestSharp;
using System.Diagnostics;
using System.Net;

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
            if (i > Cameras.Count) throw new KeyNotFoundException();
            return Cameras[i-1];
        }
    }

    public class Camera
    {
        public Camera(CameraSettings settings)
        {
            Settings = settings;
            Client = new RestClient();

            _ = StartStream();

            Decoder = new MJPEGStreamDecoder();
            Decoder.StartStream(settings.Uri);

            //var request = new RestRequest(settings.Uri);
            //var response = Client.Get(request);
        }

        async Task StartStream()
        {
            
        }

        public string Name => Settings.Name;
        public CameraSettings Settings { get; }
        public RestClient Client { get; }

        internal void UpdateCurrentFrame()
        {
            CurrentFrame = GetFrame();
        }

        public byte[] CurrentFrame { get; set; }
        public MJPEGStreamDecoder Decoder { get; }

        public byte[] GetFrame()
        {

            return Decoder.NextFrame ?? Array.Empty<byte>();
            //var request = new RestRequest(Settings.Uri);
            /*
            try
            {
                var response = Client.DownloadData(request);
                
            }
            catch (Exception ex)
            {
                throw;
            }*/

            return Array.Empty<byte>();
        }
    }

    public class MJPEGStreamDecoder
    {
        private static ILog Log { get; } = LogManager.GetLogger(typeof(MJPEGStreamDecoder));

        public bool tryOnStart = false;

        float RETRY_DELAY = 5f;
        int MAX_RETRIES = 3;
        int retryCount = 0;


        public byte[]? NextFrame { get; private set; }

        Thread worker;
        int threadID = 0;

        static Random random = new(); 
        List<BufferedStream> trackedBuffers = new List<BufferedStream>();

        private void OnDestroy()
        {
            foreach (var b in trackedBuffers)
            {
                if (b != null)
                    b.Close();
            }
        }

        public void StartStream(Uri url)
        {
            retryCount = 0;
            foreach (var b in trackedBuffers)
                b.Close();

            worker = new Thread(() => ReadMJPEGStreamWorker(threadID = random.Next(65536), url));
            worker.Start();
        }

        void ReadMJPEGStreamWorker(int id, Uri url)
        {

            var client = new HttpClient();
            var stream = client.GetStreamAsync(url).GetAwaiter().GetResult();

            //var webRequest = WebRequest.Create(url);
            //webRequest.Method = "GET";
            List<byte> frameBuffer = new List<byte>();

            int lastByte = 0x00;
            bool addToBuffer = false;

            BufferedStream buffer = null;
            try
            {
                //Stream stream = webRequest.GetResponse().GetResponseStream();
                buffer = new BufferedStream(stream);
                trackedBuffers.Add(buffer);
            }
            catch (System.Exception ex)
            {
                Log.Error(ex);
            }
            int newByte;
            while (buffer != null)
            {
                if (threadID != id) return; // We are no longer the active thread! stop doing things damnit!
                if (!buffer.CanRead)
                {
                    Log.Error("Can't read buffer!");
                    break;
                }

                newByte = -1;

                try
                {
                    newByte = buffer.ReadByte();
                }
                catch
                {
                    break; // Something happened to the stream, start a new one
                }

                if (newByte < 0) // end of stream or failure
                {
                    continue; // End of data
                }

                if (addToBuffer)
                    frameBuffer.Add((byte)newByte);

                if (lastByte == 0xFF) // It's a command!
                {
                    if (!addToBuffer) // We're not reading a frame, should we be?
                    {
                        if (IsStartOfImage(newByte))
                        {
                            addToBuffer = true;
                            frameBuffer.Add((byte)lastByte);
                            frameBuffer.Add((byte)newByte);
                        }
                    }
                    else // We're reading a frame, should we stop?
                    {
                        if (newByte == 0xD9)
                        {
                            frameBuffer.Add((byte)newByte);
                            addToBuffer = false;
                            NextFrame = frameBuffer.ToArray();
                            frameBuffer.Clear();
                        }
                    }
                }

                lastByte = newByte;
            }

            if (retryCount < MAX_RETRIES)
            {
                retryCount++;
                Log.DebugFormat("[{0}] Retrying Connection {1}...", id, retryCount);
                foreach (var b in trackedBuffers)
                    b.Dispose();
                trackedBuffers.Clear();
                worker = new Thread(() => ReadMJPEGStreamWorker(threadID = random.Next(65536), url));
                worker.Start();
            }
        }

        static bool IsStartOfImage(int command)
        {
            switch (command)
            {
                case 0x8D:
                    Log.Debug("Command SOI");
                    return true;
                case 0xC0:
                    Log.Debug("Command SOF0");
                    return true;
                case 0xC2:
                    Log.Debug("Command SOF2");
                    return true;
                case 0xC4:
                    Log.Debug("Command DHT");
                    break;
                case 0xD8:
                    //Log.Debug("Command DQT");
                    return true;
                case 0xDD:
                    Log.Debug("Command DRI");
                    break;
                case 0xDA:
                    Log.Debug("Command SOS");
                    break;
                case 0xFE:
                    Log.Debug("Command COM");
                    break;
                case 0xD9:
                    Log.Debug("Command EOI");
                    break;
            }
            return false;
        }

    }
}
