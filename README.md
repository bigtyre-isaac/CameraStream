# CameraStream

An ASP.NET Core (.NET 6) application that reads MJPEG camera feeds and serves them as browser-viewable MJPEG streams and individual JPEG snapshots. The home page displays all configured cameras.

**Status:** This project is no longer in active development.

## Getting started

1. Install the .NET 6 SDK.
2. Update the `Cameras` array in `CameraStream/appsettings.json` with your camera names and MJPEG stream URLs:

   ```json
   "Cameras": [
     {
       "Name": "Office",
       "Uri": "http://camera-address/videostream.cgi?user=USERNAME&pwd=PASSWORD"
     }
   ]
   ```

   Replace the example URL with your camera's MJPEG endpoint, including credentials if required. The server must be able to reach each camera. Add an entry for each camera.

3. From the repository root, run:

   ```sh
   dotnet run --project CameraStream/CameraStream.csproj
   ```

4. Open <http://localhost:5121> to view the cameras. API documentation is available at <http://localhost:5121/swagger>.

## Endpoints

- `/api/cameras/{cameraId}/stream` — continuous MJPEG stream (`multipart/x-mixed-replace`).
- `/api/cameras/{cameraId}/snapshot` — latest JPEG frame.

Camera IDs start at **1** and follow the order in the `Cameras` array. Frames are refreshed from the source feeds at 10 FPS; stream responses send the latest frame at approximately 30 FPS.

## License

[MIT](LICENSE)
