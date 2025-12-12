using DetectQRCode.Models.Camera;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using PaddleOCRSharp;
using Sdcb.RotationDetector;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace DetectQRCode.OCR.Utils
{
    /// <summary>
    /// Version 2: Sử dụng YOLO11 Detector để phát hiện QR và Label
    /// </summary>
    public static class DetectLabelFromImageV2
    {
        /// <summary>
        /// Detect label sử dụng YOLO11 detector
        /// </summary>
        /// <param name="frame">Mat frame từ camera</param>
        /// <param name="yoloDetector">YOLO11 detector instance (được tạo từ Form1)</param>
        /// <param name="ocr">PaddleOCR engine</param>
        /// <param name="currentThreshold">Threshold hiện tại (không dùng trong version này)</param>
        /// <param name="cameraBox">PictureBox để hiển thị camera</param>
        /// <param name="picPreprocessed">PictureBox để hiển thị preprocessed image</param>
        /// <returns>DetectInfo chứa thông tin QR và label</returns>
        public static DetectInfo DetectLabel(
            Mat frame, 
            Yolo11Seg yoloDetector,
            PaddleOCREngine ocr,
             PaddleRotationDetector rotationDetector,
            int currentThreshold,  
            OverlayPictureBox cameraBox, 
            PictureBox picPreprocessed)
        {
            try
            {
                if (frame == null || frame.Empty())
                {
                    Debug.WriteLine("[⚠] Frame is null or empty");
                    return null;
                }

                if (yoloDetector == null)
                {
                    Debug.WriteLine("[⚠] YOLO Detector is null, falling back to original method");
                    return DetectLabelFromImage.DetectLabel(frame, ocr, currentThreshold, cameraBox, picPreprocessed);
                }

                var result = new DetectInfo();

                // ============================================
                // 1. YOLO DETECTION - Detect trực tiếp trên frame (Mat)
                // ============================================
                var detections = yoloDetector.Detect(frame);

                if (detections == null || detections.Count == 0)
                {
                    Debug.WriteLine("[ℹ] No objects detected by YOLO");
                    
                    // Hiển thị frame gốc
                    var bmpFull = MatToBitmap(frame);
                    cameraBox.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            var old = cameraBox.Image;
                            cameraBox.Image = bmpFull;
                            cameraBox.IsObjectDetected = false;
                            old?.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[⚠ DISPLAY FRAME ERROR] {ex.Message}");
                        }
                    }));
                    
                    return result;
                }

                // ============================================
                // 2. VẼ BOUNDING BOXES LÊN FRAME
                // ============================================
                using var displayFrame = frame.Clone();

                DetectionResult labelDetection = null;

                foreach (var detection in detections)
                {
                    var bbox = detection.BoundingBox;
                    
                    // Vẽ bounding box
                    Cv2.Rectangle(displayFrame, bbox, Scalar.Yellow, 2);
                    
                    // Vẽ label text
                    string label = $"{detection.ClassName}: {detection.Confidence:P0}";
                    Cv2.PutText(displayFrame, label, 
                        new OpenCvSharp.Point(bbox.X, bbox.Y - 5),
                        HersheyFonts.HersheySimplex, 0.6, Scalar.Yellow, 2);

                    
                    if (detection.ClassName.ToLower().Contains("label"))
                    {
                        labelDetection = detection;
                    }
                }

                if (labelDetection != null)
                {
                    /* var bbox = labelDetection.BoundingBox;

                     using var croppedMat = new Mat(frame, bbox);

                     // Convert Mat to Bitmap for ImageEnhancer
                     var croppedBmp = MatToBitmap(croppedMat);

                     // ============================================
                     // 🎨 IMAGE ENHANCEMENT PIPELINE
                     // ============================================
                     Debug.WriteLine($"[ENHANCEMENT] Starting pipeline for bbox: {bbox}");
                     var sw = System.Diagnostics.Stopwatch.StartNew();

                     try
                     {
                         var enhanced = croppedBmp;  // Start with original
                         Debug.WriteLine($"[ENHANCEMENT] Original size: {enhanced.Width}x{enhanced.Height}");

                         // 1️⃣ Tăng sáng (nhà xưởng thường tối)
                         var brightened = ImageEnhancer.EnhanceDark(enhanced, clipLimit: 2.5);
                         if (enhanced != croppedBmp) enhanced.Dispose();
                         enhanced = brightened;
                         Debug.WriteLine($"[ENHANCEMENT] ✓ EnhanceDark completed");

                         // 2️⃣ Làm sắc nét (cải thiện QR detection)
                         var sharpened = ImageEnhancer.SharpenBlurry(enhanced);
                         if (enhanced != croppedBmp) enhanced.Dispose();
                         enhanced = sharpened;
                         Debug.WriteLine($"[ENHANCEMENT] ✓ SharpenBlurry completed");

                         // 3️⃣ Upscale nếu ảnh quá nhỏ
                         int minDim = Math.Min(enhanced.Width, enhanced.Height);
                         if (minDim < 400)
                         {
                             var upscaled = ImageEnhancer.UpscaleSmall(enhanced, 2.0);
                             if (enhanced != croppedBmp) enhanced.Dispose();
                             enhanced = upscaled;
                             Debug.WriteLine($"[ENHANCEMENT] ✓ UpscaleSmall completed: {enhanced.Width}x{enhanced.Height}");
                         }
                         else
                         {
                             Debug.WriteLine($"[ENHANCEMENT] ⊘ UpscaleSmall skipped (size ok)");
                         }




                         // Dispose original cropped bitmap nếu đã enhance
                         if (enhanced != croppedBmp)
                         {
                             croppedBmp.Dispose();
                             croppedBmp = enhanced;  // Use enhanced version
                         }

                         sw.Stop();
                         Debug.WriteLine($"[ENHANCEMENT] ✅ Pipeline completed in {sw.ElapsedMilliseconds}ms");
                     }
                     catch (Exception ex)
                     {
                         sw.Stop();
                         Debug.WriteLine($"[⚠ IMAGE ENHANCEMENT ERROR] {ex.Message}");
                         Debug.WriteLine($"[ENHANCEMENT] ⚠️ Using original image (fallback)");
                         // Continue with original cropped bitmap
                     }

                     // ============================================
                     // 🔍 QR DETECTION (AFTER ENHANCEMENT)
                     // ============================================
                     Debug.WriteLine($"[QR DETECTION] Starting detection on enhanced image...");
                     var qrSw = System.Diagnostics.Stopwatch.StartNew();

                     var (qrPoints, qrText) = LabelDetectorZXing.DetectQRCodeZXing(croppedBmp);
                     result.QRCode = qrText;

                     qrSw.Stop();
                     Debug.WriteLine($"[QR DETECTION] Completed in {qrSw.ElapsedMilliseconds}ms - Result: {qrText ?? "NOT FOUND"}");


                     // ============================================
                     // 🖼️ DISPLAY PREPROCESSED IMAGE
                     // ============================================*/

                    /*picPreprocessed.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            var old = picPreprocessed.Image;
                            picPreprocessed.Image = croppedBmp;  // PictureBox owns bitmap now
                            old?.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[⚠ DISPLAY CROP ERROR] {ex.Message}");
                        }
                    }));*/
                    var maskContours = new List<(int x, int y)>();

                    foreach (var contour in labelDetection.Contours)
                    {
                        var polygon = contour
                            .Select(p => (p.X, p.Y))
                            .ToList();
                        foreach(var p in polygon)
                        {
                            maskContours.Add((p.X, p.Y));
                        }
                        
                    }

                    var croptImage = RotationImage.ProcessRotationImage(frame, maskContours);
                    var croppedBmp = MatToBitmap(croptImage);
                    try
                    {
                        var enhanced = croppedBmp;  // Start with original
                        Debug.WriteLine($"[ENHANCEMENT] Original size: {enhanced.Width}x{enhanced.Height}");

                        // 1️⃣ Tăng sáng (nhà xưởng thường tối)
                        var brightened = ImageEnhancer.EnhanceDark(enhanced, clipLimit: 2.5);
                        if (enhanced != croppedBmp) enhanced.Dispose();
                        enhanced = brightened;
                        Debug.WriteLine($"[ENHANCEMENT] ✓ EnhanceDark completed");

                        // 2️⃣ Làm sắc nét (cải thiện QR detection)
                        var sharpened = ImageEnhancer.SharpenBlurry(enhanced);
                        if (enhanced != croppedBmp) enhanced.Dispose();
                        enhanced = sharpened;
                        Debug.WriteLine($"[ENHANCEMENT] ✓ SharpenBlurry completed");

                        // 3️⃣ Upscale nếu ảnh quá nhỏ
                        int minDim = Math.Min(enhanced.Width, enhanced.Height);
                        if (minDim < 400)
                        {
                            var upscaled = ImageEnhancer.UpscaleSmall(enhanced, 2.0);
                            if (enhanced != croppedBmp) enhanced.Dispose();
                            enhanced = upscaled;
                            Debug.WriteLine($"[ENHANCEMENT] ✓ UpscaleSmall completed: {enhanced.Width}x{enhanced.Height}");
                        }
                        else
                        {
                            Debug.WriteLine($"[ENHANCEMENT] ⊘ UpscaleSmall skipped (size ok)");
                        }




                        // Dispose original cropped bitmap nếu đã enhance
                        if (enhanced != croppedBmp)
                        {
                            croppedBmp.Dispose();
                            croppedBmp = enhanced;  // Use enhanced version
                        }




                       
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[⚠ IMAGE ENHANCEMENT ERROR] {ex.Message}");
                        Debug.WriteLine($"[ENHANCEMENT] ⚠️ Using original image (fallback)");
                        // Continue with original cropped bitmap
                    }

                    Mat mat = BitmapConverter.ToMat(croppedBmp);

                    var rotation = RotationImage.CheckLabelRotation(mat, rotationDetector);
                    if (rotation != RotationDegree._0)
                    {
                        
                        var matRotation = RotationImage.Rotate(mat, rotation);
                        croppedBmp = MatToBitmap(matRotation);
                    }
                    var (qrPoints, qrText) = LabelDetectorZXing.DetectQRCodeZXing(croppedBmp);
                    result.QRCode = qrText;
                    
                    OpenCvSharp.Point[] qrBox = qrPoints
                        .Select(p => new OpenCvSharp.Point((int)Math.Round(p.X), (int)Math.Round(p.Y)))
                        .ToArray();

                    // Gọi hàm với kiểu dữ liệu đã đúng
                    var mergedCrop = CropComponent.CropAndMergeBottomLeftAndAboveQr(croppedBmp, qrBox);
                    if (mergedCrop != null)
                    {
                        var (ocrTexts, minScore, debugText) = ExtractTextsFromMergedCrop(ocr, mergedCrop);
                        mergedCrop.Dispose();
                       
                        result.ProductTotal = ocrTexts[0];
                        result.ProductCode = ocrTexts[1];
                        result.Size = ocrTexts[3];
                        result.Color = ocrTexts[2];
                    }

                    picPreprocessed.BeginInvoke(new Action(() =>
                   {
                       try
                       {
                           var old = picPreprocessed.Image;
                           picPreprocessed.Image = mergedCrop;  // PictureBox owns bitmap now
                           old?.Dispose();
                       }
                       catch (Exception ex)
                       {
                           Debug.WriteLine($"[⚠ DISPLAY CROP ERROR] {ex.Message}");
                       }
                   }));

                     

                }
                else
                {
                     // Nếu không tìm thấy label, clear picPreprocessed
                    picPreprocessed.BeginInvoke(new Action(() =>
                    {
                        var old = picPreprocessed.Image;
                        picPreprocessed.Image = null;
                        old?.Dispose();
                    }));
                }

                var displayBmp = MatToBitmap(displayFrame);
                cameraBox.BeginInvoke(new Action(() =>
                {
                    var old = cameraBox.Image;
                    cameraBox.Image = displayBmp;
                    cameraBox.IsObjectDetected = detections.Count > 0;
                    old?.Dispose();
                }));

                
                

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[⚠ DETECT LABEL V2 ERROR] {ex.Message}");
                Debug.WriteLine($"[⚠ Stack Trace] {ex.StackTrace}");
                return null;
            }
        }


        public static (List<string> texts, float minScore, string DebugText) ExtractTextsFromMergedCrop(PaddleOCREngine ocr, Bitmap mergedCrop)
        {
            var texts = new List<string>();
            string DebugText = "";
            float minScore = 999;

            try
            {
                if (ocr == null || mergedCrop == null)
                    return (texts, -999, "[?] Input null");

                OCRResult result;
                lock (ocr)
                {
                    result = ocr.DetectText(mergedCrop);
                }

                if (result?.TextBlocks?.Count > 0)
                {
                    texts = result.TextBlocks
                        .Where(tb => !string.IsNullOrWhiteSpace(tb.Text))
                        .Select(tb => tb.Text.Trim())
                        .ToList();

                    foreach (var tb in result.TextBlocks)
                    {
                        if (tb.Score < minScore)
                            minScore = tb.Score;
                        DebugText += $"{tb.Text?.Trim()} | Score: {tb.Score * 100:F2}%\r\n";
                    }
                }

                return (texts, minScore, DebugText);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[? OCR ONLY ERROR] {ex.Message}");
                return (texts, -999, DebugText);
            }
        }

        /// <summary>
        /// Convert Mat to Bitmap
        /// </summary>
        private static Bitmap MatToBitmap(Mat mat)
        {
            int w = mat.Width;
            int h = mat.Height;
            int channels = mat.Channels();

            Bitmap bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            var rect = new Rectangle(0, 0, w, h);
            var bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);

            int stride = bmpData.Stride;
            int rowLength = w * channels;

            byte[] buffer = new byte[rowLength];

            for (int y = 0; y < h; y++)
            {
                IntPtr src = mat.Data + y * (int)mat.Step();
                System.Runtime.InteropServices.Marshal.Copy(src, buffer, 0, rowLength);

                IntPtr dst = bmpData.Scan0 + y * stride;
                System.Runtime.InteropServices.Marshal.Copy(buffer, 0, dst, rowLength);
            }

            bmp.UnlockBits(bmpData);
            return bmp;
        }

        /// <summary>
        /// Áp dụng Unsharp Mask để làm sắc nét ảnh
        /// </summary>
        /// <param name="src">Mat nguồn</param>
        /// <param name="amount">Cường độ sharpening (1.0 = 100%, 1.5 = 150%, khuyến nghị: 1.0-2.0)</param>
        /// <param name="radius">Bán kính Gaussian blur (pixels, khuyến nghị: 1-3)</param>
        /// <param name="threshold">Ngưỡng (0-255, 0 = không ngưỡng, khuyến nghị: 0-10)</param>
        /// <returns>Mat đã được sharpened (cần dispose sau khi dùng)</returns>
        private static Mat ApplyUnsharpMask(Mat src, double amount = 1.5, int radius = 2, int threshold = 0)
        {
            // 1. Tạo bản mờ của ảnh gốc (Gaussian Blur)
            var blurred = new Mat();
            int ksize = radius * 2 + 1; // Kernel size phải là số lẻ
            Cv2.GaussianBlur(src, blurred, new OpenCvSharp.Size(ksize, ksize), 0);

            // 2. Tính "mask" = original - blurred
            var mask = new Mat();
            Cv2.Subtract(src, blurred, mask);

            // 3. Nếu có threshold, chỉ sharpen vùng có độ tương phản cao
            if (threshold > 0)
            {
                var maskAbs = new Mat();
                Cv2.ConvertScaleAbs(mask, maskAbs);
                
                var thresholdMask = new Mat();
                Cv2.Threshold(maskAbs, thresholdMask, threshold, 255, ThresholdTypes.Binary);
                
                Cv2.BitwiseAnd(mask, mask, mask, thresholdMask);
                
                maskAbs.Dispose();
                thresholdMask.Dispose();
            }

            // 4. Nhân mask với amount
            var weightedMask = new Mat();
            Cv2.ConvertScaleAbs(mask, weightedMask, amount, 0);

            // 5. Cộng vào ảnh gốc: sharpened = original + (amount * mask)
            var sharpened = new Mat();
            Cv2.Add(src, weightedMask, sharpened);

            // Cleanup
            blurred.Dispose();
            mask.Dispose();
            weightedMask.Dispose();

            return sharpened;
        }
    }
}
