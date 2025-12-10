using AForge.Video;
using AForge.Video.DirectShow;
using DetectQRCode.Models.Camera;
using DetectQRCode.OCR.Utils;
using OpenCvSharp;
using PaddleOCRSharp;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DetectQRCode
{
    public partial class Form1 : Form
    {
        // Camera components
        private FilterInfoCollection? _videoDevices;
        private VideoCaptureDevice? _videoSource;
        private bool _isProcessingFrame = false;
        private DateTime _lastOcrProcessTime = DateTime.MinValue;
        private const int MIN_OCR_INTERVAL_MS = 500; // OCR tối đa 2 lần/giây
        private Bitmap? _latestFrame;
        private readonly object _frameLock = new object();

        // OCR components
        private PaddleOCREngine? _ocrEngine;
        
        // YOLO11 detector
        private Yolo11Seg? _yoloDetector;
        
        // Mode: Camera or Import
        private bool _isCameraMode = true;

        // FPS tracking
        private int _frameCount = 0;
        private int _processedFrameCount = 0;
        private DateTime _fpsStartTime = DateTime.Now;
        private System.Windows.Forms.Timer? _fpsTimer;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
            this.FormClosing += Form1_FormClosing;
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            try
            {
                // Set default mode to Camera
                rbCamera.Checked = true;
                
                // Load camera devices
                _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                cmbCameraDevices.Items.Clear();

                if (_videoDevices.Count == 0)
                {
                    MessageBox.Show("No camera devices found!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    foreach (FilterInfo device in _videoDevices)
                    {
                        cmbCameraDevices.Items.Add(device.Name);
                    }

                    if (cmbCameraDevices.Items.Count > 0)
                    {
                        cmbCameraDevices.SelectedIndex = 0;
                    }
                }

                // Initialize FPS timer (update every 1 second)
                _fpsTimer = new System.Windows.Forms.Timer();
                _fpsTimer.Interval = 1000; // 1 second
                _fpsTimer.Tick += FpsTimer_Tick;
                _fpsTimer.Start();
                _fpsStartTime = DateTime.Now;

                UpdateStatus("Ready. Select mode: Camera or Import Image.", Color.Green);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading cameras: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnStartCamera_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbCameraDevices.SelectedIndex < 0)
                {
                    MessageBox.Show("Please select a camera device!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                StopCamera();

                int deviceIndex = cmbCameraDevices.SelectedIndex;
                _videoSource = new VideoCaptureDevice(_videoDevices![deviceIndex].MonikerString);
                _videoSource.NewFrame += VideoSource_NewFrame;
                _videoSource.Start();

                btnStartCamera.Enabled = false;
                btnStopCamera.Enabled = true;
                cmbCameraDevices.Enabled = false;

                UpdateStatus("Camera started. Processing frames...", Color.Blue);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting camera: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnStopCamera_Click(object sender, EventArgs e)
        {
            StopCamera();
            UpdateStatus("Camera stopped.", Color.Orange);
        }

        private void StopCamera()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
                _videoSource.WaitForStop();
                _videoSource.NewFrame -= VideoSource_NewFrame;
                _videoSource = null;
            }

            btnStartCamera.Enabled = true;
            btnStopCamera.Enabled = false;
            cmbCameraDevices.Enabled = true;
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // Tăng frame counter
            System.Threading.Interlocked.Increment(ref _frameCount);

            // Lấy frame mới từ camera
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();

            try
            {
                // 🔥 FIX: Hiển thị frame ngay lập tức (không chờ OCR)
                // Clone để tránh dispose conflict
                Bitmap displayFrame = (Bitmap)bitmap.Clone();
                picCamera.BeginInvoke(new Action(() =>
                {
                    var old = picCamera.Image;
                    picCamera.Image = displayFrame;
                    old?.Dispose();
                }));

                // 🔥 FIX: Lưu frame mới nhất và xử lý OCR bất đồng bộ (không block event)
                lock (_frameLock)
                {
                    _latestFrame?.Dispose();
                    _latestFrame = (Bitmap)bitmap.Clone();
                }

                // 🔥 FIX: Xử lý OCR bất đồng bộ, có throttling và skip nếu đang xử lý
                _ = Task.Run(async () =>
                {
                    // Skip nếu đang xử lý frame khác
                    if (_isProcessingFrame)
                        return;

                    // Throttling: Chỉ xử lý OCR tối đa 2 lần/giây
                    var timeSinceLastOcr = (DateTime.Now - _lastOcrProcessTime).TotalMilliseconds;
                    if (timeSinceLastOcr < MIN_OCR_INTERVAL_MS)
                        return;

                    _isProcessingFrame = true;
                    _lastOcrProcessTime = DateTime.Now;
                    System.Threading.Interlocked.Increment(ref _processedFrameCount);

                    try
                    {
                        // Lazy initialization OCR engine
                        if (_ocrEngine == null)
                        {
                            _ocrEngine = InitializeOCREngine();
                        }

                        if (_ocrEngine != null)
                        {
                            Bitmap? frameToProcess = null;
                            lock (_frameLock)
                            {
                                if (_latestFrame != null)
                                {
                                    frameToProcess = (Bitmap)_latestFrame.Clone();
                                }
                            }

                            if (frameToProcess != null)
                            {
                                using var mat = BitmapToMat(frameToProcess);
                                if (mat != null)
                                {
                                    // Initialize YOLO detector (lazy loading)
                                    /*if (_yoloDetector == null)
                                    {
                                        _yoloDetector = InitializeYoloDetector();
                                    }

                                    // Gọi DetectLabel V2 với YOLO detector
                                    var result = DetectLabelFromImageV2.DetectLabel(mat, _yoloDetector, _ocrEngine, 180, picCamera, picPreprocessed);*/
                                    var result = DetectLabelFromImage.DetectLabel(mat, _ocrEngine, 180, picCamera, picPreprocessed);


                                    if (result != null && result.QRCode != null)
                                    {
                                        this.BeginInvoke(new Action(() =>
                                        {
                                            lblQRCode.Text = $"QR: {result.QRCode}";
                                            lblSize.Text = $"Size: {result.Size ?? "N/A"}";
                                            lblColor.Text = $"Color: {result.Color ?? "N/A"}";
                                            lblProductCode.Text = $"Product: {result.ProductCode ?? "N/A"}";
                                            lblProductTotal.Text = $"Total: {result.ProductTotal ?? "N/A"}";
                                            
                                            UpdateStatus($"Detected: {result.QRCode}", Color.Green);
                                        }));
                                    }
                                }
                                frameToProcess.Dispose();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error in DetectLabel processing: {ex.Message}");
                    }
                    finally
                    {
                        _isProcessingFrame = false;
                    }
                });

                // Dispose bitmap gốc sau khi đã clone
                bitmap.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in VideoSource_NewFrame: {ex.Message}");
                bitmap?.Dispose();
            }
        }

        private PaddleOCREngine? InitializeOCREngine()
        {
            try
            {
                // Sử dụng default constructor - PaddleOCRSharp sẽ tự tìm models
                return new PaddleOCREngine();
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new Action(() =>
                {
                    UpdateStatus($"OCR Engine initialization failed: {ex.Message}", Color.Red);
                }));
                return null;
            }
        }

        private Yolo11Seg? InitializeYoloDetector()
        {
            try
            {
                // Cấu hình đường dẫn model và class names
                string modelPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "models", "yolo11n.onnx");
                string[] classNames = new[] { "label" };

                if (!System.IO.File.Exists(modelPath))
                {
                    Debug.WriteLine($"[⚠] YOLO model not found: {modelPath}");
                    Debug.WriteLine("[ℹ] Using fallback detection method (original DetectLabelFromImage)");
                    return null;
                }

                var detector = new Yolo11Seg(
                    modelPath,
                    classNames,
                    confThreshold: 0.5f,
                    iouThreshold: 0.45f
                );

                Debug.WriteLine("[✓] YOLO11 Detector initialized successfully!");
                this.BeginInvoke(new Action(() =>
                {
                    UpdateStatus("YOLO11 Detector loaded!", Color.Green);
                }));

                return detector;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[⚠] Failed to initialize YOLO: {ex.Message}");
                return null;
            }
        }

        private Mat? BitmapToMat(Bitmap bmp)
        {
            try
            {
                using var ms = new System.IO.MemoryStream();
                bmp.Save(ms, ImageFormat.Png);
                return Cv2.ImDecode(ms.ToArray(), ImreadModes.Color);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to convert Bitmap to Mat: {ex.Message}");
                return null;
            }
        }

        private void btnSaveFrame_Click(object sender, EventArgs e)
        {
            SaveFrameToFolder(""); // Root SavedFrames folder
        }

        private void btnSaveNghieng_Click(object sender, EventArgs e)
        {
            SaveFrameToFolder("Nghiêng");
        }

        private void btnSaveMo_Click(object sender, EventArgs e)
        {
            SaveFrameToFolder("Mờ");
        }

        private void btnSaveCong_Click(object sender, EventArgs e)
        {
            SaveFrameToFolder("Cong");
        }

        private void btnSaveNho_Click(object sender, EventArgs e)
        {
            SaveFrameToFolder("Nhỏ");
        }

        private void SaveFrameToFolder(string subFolder)
        {
            try
            {
                Bitmap? frameToSave = null;
                lock (_frameLock)
                {
                    if (_latestFrame != null)
                    {
                        frameToSave = (Bitmap)_latestFrame.Clone();
                    }
                }

                if (frameToSave == null)
                {
                    UpdateStatus("No frame available to save!", Color.Orange);
                    return;
                }

                // Create directory: OCR/SavedFrames/[SubFolder]/
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var savedFramesDir = System.IO.Path.Combine(baseDir, "OCR", "SavedFrames");
                
                if (!string.IsNullOrEmpty(subFolder))
                {
                    savedFramesDir = System.IO.Path.Combine(savedFramesDir, subFolder);
                }
                
                System.IO.Directory.CreateDirectory(savedFramesDir);

                // Create filename: Frame_Timestamp.png
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                var fileName = $"Frame_{timestamp}.png";
                var filePath = System.IO.Path.Combine(savedFramesDir, fileName);

                // Save as PNG (lossless, maximum quality)
                frameToSave.Save(filePath, ImageFormat.Png);
                frameToSave.Dispose();

                var folderInfo = string.IsNullOrEmpty(subFolder) ? "" : $" to {subFolder}";
                UpdateStatus($"Frame saved{folderInfo}: {fileName}", Color.Green);
                Debug.WriteLine($"Frame saved: {filePath}");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error saving frame: {ex.Message}", Color.Red);
                Debug.WriteLine($"Failed to save frame: {ex.Message}");
            }
        }

        private void rbCamera_CheckedChanged(object sender, EventArgs e)
        {
            _isCameraMode = rbCamera.Checked;
            UpdateModeUI();
        }

        private void rbImportImage_CheckedChanged(object sender, EventArgs e)
        {
            if (rbImportImage.Checked)
            {
                _isCameraMode = false;
                StopCamera();
                UpdateModeUI();
            }
        }

        private void UpdateModeUI()
        {
            if (_isCameraMode)
            {
                // Camera mode
                cmbCameraDevices.Enabled = true;
                btnStartCamera.Enabled = true;
                btnStopCamera.Enabled = false;
                btnBrowseImage.Enabled = false;
                UpdateStatus("Camera mode selected.", Color.Blue);
            }
            else
            {
                // Import mode
                cmbCameraDevices.Enabled = false;
                btnStartCamera.Enabled = false;
                btnStopCamera.Enabled = false;
                btnBrowseImage.Enabled = true;
                UpdateStatus("Import mode selected. Click 'Browse Image' to load a file.", Color.Blue);
            }
        }

        private void btnBrowseImage_Click(object sender, EventArgs e)
        {
            try
            {
                using var openFileDialog = new OpenFileDialog
                {
                    Title = "Select Image File",
                    Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*",
                    FilterIndex = 1
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ProcessImportedImage(openFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ProcessImportedImage(string filePath)
        {
            try
            {
                // Reset guide box to red and clear previous detection data
                picCamera.BeginInvoke(new Action(() =>
                {
                    picCamera.IsObjectDetected = false;
                    picCamera.Invalidate();
                }));
                
                this.BeginInvoke(new Action(() =>
                {
                    lblQRCode.Text = "QR: N/A";
                    lblSize.Text = "Size: N/A";
                    lblColor.Text = "Color: N/A";
                    lblProductCode.Text = "Product: N/A";
                    lblProductTotal.Text = "Total: N/A";
                }));
                
                // Load image
                var bitmap = new Bitmap(filePath);
                
                // Clone BEFORE BeginInvoke to avoid threading issues
                var displayBitmap = (Bitmap)bitmap.Clone();
                var frameBitmap = (Bitmap)bitmap.Clone();
                var ocrBitmap = (Bitmap)bitmap.Clone();  // Clone for OCR task
                
                // Display image
                picCamera.BeginInvoke(new Action(() =>
                {
                    var old = picCamera.Image;
                    picCamera.Image = displayBitmap;
                    old?.Dispose();
                }));

                // Save to latest frame
                lock (_frameLock)
                {
                    _latestFrame?.Dispose();
                    _latestFrame = frameBitmap;
                }

                UpdateStatus($"Image loaded: {System.IO.Path.GetFileName(filePath)}", Color.Green);

                // Dispose original bitmap BEFORE async task
                bitmap.Dispose();

                // Process OCR with cloned bitmap
                _ = Task.Run(() =>
                {
                    try
                    {
                        if (_ocrEngine == null)
                        {
                            _ocrEngine = InitializeOCREngine();
                        }

                        if (_ocrEngine != null)
                        {
                            using var mat = BitmapToMat(ocrBitmap);
                            if (mat != null)
                            {
                                // Initialize YOLO detector (lazy loading)
                                //if (_yoloDetector == null)
                                //{
                                //    _yoloDetector = InitializeYoloDetector();
                                //}

                                // Gọi DetectLabel V2 với YOLO detector
                                //var result = DetectLabelFromImageV2.DetectLabel(mat, _yoloDetector, _ocrEngine, 180, picCamera, picPreprocessed);
                                var result = DetectLabelFromImage.DetectLabel(mat, _ocrEngine, 180, picCamera, picPreprocessed);

                                if (result != null && result.QRCode != null)
                                {
                                    this.BeginInvoke(new Action(() =>
                                    {
                                        lblQRCode.Text = $"QR: {result.QRCode}";
                                        lblSize.Text = $"Size: {result.Size ?? "N/A"}";
                                        lblColor.Text = $"Color: {result.Color ?? "N/A"}";
                                        lblProductCode.Text = $"Product: {result.ProductCode ?? "N/A"}";
                                        lblProductTotal.Text = $"Total: {result.ProductTotal ?? "N/A"}";
                                        
                                        UpdateStatus($"Detected: {result.QRCode}", Color.Green);
                                    }));
                                }
                                else
                                {
                                    this.BeginInvoke(new Action(() =>
                                    {
                                        UpdateStatus("No QR code detected in image.", Color.Orange);
                                    }));
                                }
                            }
                            else
                            {
                                this.BeginInvoke(new Action(() =>
                                {
                                    UpdateStatus("Failed to convert image to Mat.", Color.Red);
                                }));
                            }
                        }
                        
                        // Dispose OCR bitmap after processing
                        ocrBitmap.Dispose();
                    }
                    catch (Exception ex)
                    {
                        ocrBitmap?.Dispose();
                        this.BeginInvoke(new Action(() =>
                        {
                            UpdateStatus($"OCR Error: {ex.Message}", Color.Red);
                        }));
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateStatus(string message, Color color)
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.BeginInvoke(new Action(() => UpdateStatus(message, color)));
                return;
            }

            lblStatus.Text = message;
            lblStatus.ForeColor = color;
        }

        private void FpsTimer_Tick(object? sender, EventArgs e)
        {
            var elapsed = (DateTime.Now - _fpsStartTime).TotalSeconds;
            if (elapsed <= 0) return;

            double fps = _frameCount / elapsed;
            double processedFps = _processedFrameCount / elapsed;

            // Update FPS label
            if (lblFPS != null)
            {
                lblFPS.Text = $"FPS: {fps:F1} | Processed: {processedFps:F1}";
            }

            // Reset counters
            _frameCount = 0;
            _processedFrameCount = 0;
            _fpsStartTime = DateTime.Now;
        }



        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _fpsTimer?.Stop();
            _fpsTimer?.Dispose();

            StopCamera();
            
            lock (_frameLock)
            {
                _latestFrame?.Dispose();
            }

            _ocrEngine?.Dispose();
            _yoloDetector?.Dispose();
        }
    }
}
