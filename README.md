# OpenScanToWeb

OpenScanToWeb is an open-source Visual Basic.NET application for scanning documents using TWAIN-compatible scanners (e.g., Alaris E1000, Kodak S2000), saving them as individual PDFs, and sending them to a web application via WebSocket. It features a responsive UI, zoomable image previews, multiple scan support, and a dynamic save path, making it ideal for developers integrating desktop scanning with web applications.

## Features
- Scan documents using TWAIN scanners.
- Save each scan as a separate PDF (e.g., `scan_pdf1.pdf`, `scan_pdf2.pdf`) to a user-defined folder (default: Downloads).
- Send PDFs to a web app via WebSocket.
- Responsive UI with zoom in/out and navigation for multiple scans.
- Loading indicators for scanning and saving operations.
- Customizable save path via a user interface.

## Setup
1. Clone the repository:
   ```
   git clone https://github.com/reymarkdivino/open-scan-to-web.git
   ```
2. Open `Custom Scan App.sln` in Visual Studio.
3. Install dependencies via NuGet Package Manager:
   ```
   Install-Package NTwain
   Install-Package itextsharp
   ```
4. Place icon files (24x24 PNGs recommended, 512x512 auto-resized) in `bin\Debug\Icons` or `bin\Release\Icons`:
   - `scan.png`, `save.png`, `zoom_in.png`, `zoom_out.png`, `previous.png`, `next.png`, `folder.png`
5. Ensure your TWAIN-compatible scanner is installed with up-to-date drivers from the manufacturer (e.g., [Kodak Alaris Support](https://www.alarisworld.com/en-us/support)).

## Usage
1. Run the app and click "Scan" to capture images using a TWAIN scanner.
2. Use "Zoom In"/"Zoom Out" to adjust the preview, and "Previous"/"Next" to navigate between scanned images.
3. Set a custom save path in the TextBox (default: Downloads folder) and click "Set Path."
4. Click "Save as PDF and Send" to save each scan as a separate PDF and send to a WebSocket server.

## Contributing
Contributions are welcome! See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on how to contribute, report issues, or suggest features.

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.