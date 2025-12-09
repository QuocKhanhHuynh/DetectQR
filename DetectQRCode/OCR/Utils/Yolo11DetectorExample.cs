using OpenCvSharp;
using System;
using System.Collections.Generic;

namespace DetectQRCode.OCR.Utils
{
    /// <summary>
    /// Example usage of Yolo11Detector
    /// </summary>
    public class Yolo11DetectorExample
    {
        public static void ExampleUsage()
        {
            // Define class names (adjust according to your model)
            string[] classNames = new string[] { "label", "qr_code", "barcode" }; // Example classes

            // Initialize detector
            string modelPath = "OCR/Utils/yolo11n_seg_best.onnx";
            using var detector = new Yolo11Detector(
                modelPath: modelPath,
                classNames: classNames,
                confThreshold: 0.25f,
                iouThreshold: 0.45f
            );

            // Load test image
            string imagePath = "test_image.jpg";
            if (!File.Exists(imagePath))
            {
                Console.WriteLine($"Test image not found: {imagePath}");
                return;
            }

            using var frame = Cv2.ImRead(imagePath);
            if (frame.Empty())
            {
                Console.WriteLine("Failed to load image");
                return;
            }

            // Run detection
            Console.WriteLine("Running YOLO11 detection...");
            var detections = detector.Detect(frame);

            // Display results
            Console.WriteLine($"Found {detections.Count} objects:");
            foreach (var det in detections)
            {
                Console.WriteLine($"  - {det.ClassName}: {det.Confidence:F2} at [{det.BoundingBox.X}, {det.BoundingBox.Y}, {det.BoundingBox.Width}, {det.BoundingBox.Height}]");
                
                // Draw bounding box
                Cv2.Rectangle(frame, det.BoundingBox, Scalar.Green, 2);
                
                // Draw label
                string label = $"{det.ClassName} {det.Confidence:F2}";
                int baseline;
                var labelSize = Cv2.GetTextSize(label, HersheyFonts.HersheySimplex, 0.5, 1, out baseline);
                
                Cv2.Rectangle(frame, 
                    new Point(det.BoundingBox.X, det.BoundingBox.Y - labelSize.Height - 10),
                    new Point(det.BoundingBox.X + labelSize.Width, det.BoundingBox.Y),
                    Scalar.Green, -1);
                
                Cv2.PutText(frame, label, 
                    new Point(det.BoundingBox.X, det.BoundingBox.Y - 5),
                    HersheyFonts.HersheySimplex, 0.5, Scalar.Black, 1);
            }

            // Save result
            string outputPath = "detection_result.jpg";
            Cv2.ImWrite(outputPath, frame);
            Console.WriteLine($"Result saved to: {outputPath}");

            // Show result (optional)
            Cv2.ImShow("YOLO11 Detection", frame);
            Cv2.WaitKey(0);
            Cv2.DestroyAllWindows();
        }

        /// <summary>
        /// Example for video stream / camera integration
        /// </summary>
        public static void VideoStreamExample()
        {
            string[] classNames = new string[] { "label", "qr_code", "barcode" };
            string modelPath = "OCR/Utils/yolo11n_seg_best.onnx";
            
            using var detector = new Yolo11Detector(modelPath, classNames);
            using var capture = new VideoCapture(0); // Use default camera
            
            if (!capture.IsOpened())
            {
                Console.WriteLine("Failed to open camera");
                return;
            }

            var frame = new Mat();
            Console.WriteLine("Press ESC to exit...");

            while (true)
            {
                capture.Read(frame);
                if (frame.Empty())
                    break;

                // Detect objects
                var detections = detector.Detect(frame);

                // Draw detections
                foreach (var det in detections)
                {
                    Cv2.Rectangle(frame, det.BoundingBox, Scalar.Green, 2);
                    string label = $"{det.ClassName}: {det.Confidence:F2}";
                    Cv2.PutText(frame, label, 
                        new Point(det.BoundingBox.X, det.BoundingBox.Y - 5),
                        HersheyFonts.HersheySimplex, 0.5, Scalar.Green, 2);
                }

                // Show FPS
                Cv2.PutText(frame, $"Detections: {detections.Count}", 
                    new Point(10, 30), HersheyFonts.HersheySimplex, 1, Scalar.Red, 2);

                Cv2.ImShow("YOLO11 Live Detection", frame);
                
                if (Cv2.WaitKey(1) == 27) // ESC key
                    break;
            }

            Cv2.DestroyAllWindows();
        }

        /// <summary>
        /// Integration example with existing DetectQRCode application
        /// </summary>
        public static List<DetectionResult> DetectInFrame(Mat frame, Yolo11Detector detector)
        {
            if (frame == null || frame.Empty())
                return new List<DetectionResult>();

            try
            {
                var results = detector.Detect(frame);
                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Detection failed: {ex.Message}");
                return new List<DetectionResult>();
            }
        }

        /// <summary>
        /// Get region of interest (ROI) from detection
        /// </summary>
        public static Mat? ExtractROI(Mat frame, DetectionResult detection, int padding = 10)
        {
            if (frame == null || frame.Empty())
                return null;

            try
            {
                var box = detection.BoundingBox;
                
                // Add padding
                int x = Math.Max(0, box.X - padding);
                int y = Math.Max(0, box.Y - padding);
                int width = Math.Min(frame.Width - x, box.Width + 2 * padding);
                int height = Math.Min(frame.Height - y, box.Height + 2 * padding);

                var roi = new Rect(x, y, width, height);
                return new Mat(frame, roi).Clone();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ROI extraction failed: {ex.Message}");
                return null;
            }
        }
    }
}
