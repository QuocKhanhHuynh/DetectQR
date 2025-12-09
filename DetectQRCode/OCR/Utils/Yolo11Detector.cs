using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DetectQRCode.OCR.Utils
{
    /// <summary>
    /// Result class for YOLO11 detection
    /// </summary>
    public class DetectionResult
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public float Confidence { get; set; }
        public Rect BoundingBox { get; set; }
        public Mat? Mask { get; set; }  // For segmentation
    }

    /// <summary>
    /// YOLO11 Segmentation Detector using ONNX Runtime
    /// </summary>
    public class Yolo11Detector : IDisposable
    {
        private readonly InferenceSession _session;
        private readonly SessionOptions _options;
        private readonly int _inputWidth = 640;
        private readonly int _inputHeight = 640;
        private readonly float _confThreshold = 0.25f;
        private readonly float _iouThreshold = 0.45f;
        private readonly string[] _classNames;

        public Yolo11Detector(string modelPath, string[] classNames, float confThreshold = 0.25f, float iouThreshold = 0.45f)
        {
            if (!File.Exists(modelPath))
            {
                throw new FileNotFoundException($"ONNX model not found at: {modelPath}");
            }

            _classNames = classNames;
            _confThreshold = confThreshold;
            _iouThreshold = iouThreshold;

            // Configure session options
            _options = new SessionOptions();
            _options.EnableCpuMemArena = true;
            _options.EnableMemoryPattern = true;
            _options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;

            // Create inference session
            _session = new InferenceSession(modelPath, _options);

            // Get model metadata
            var inputMeta = _session.InputMetadata.First();
            var shape = inputMeta.Value.Dimensions;
            if (shape.Length >= 3)
            {
                _inputHeight = shape[2];
                _inputWidth = shape[3];
            }

            Console.WriteLine($"[YOLO11] Model loaded: {Path.GetFileName(modelPath)}");
            Console.WriteLine($"[YOLO11] Input shape: [{string.Join(", ", shape)}]");
            Console.WriteLine($"[YOLO11] Confidence threshold: {_confThreshold}");
            Console.WriteLine($"[YOLO11] IoU threshold: {_iouThreshold}");
        }

        /// <summary>
        /// Detect objects in an image frame
        /// </summary>
        /// <param name="frame">Input image (BGR format from OpenCV)</param>
        /// <returns>List of detection results with bounding boxes</returns>
        public List<DetectionResult> Detect(Mat frame)
        {
            if (frame == null || frame.Empty())
            {
                return new List<DetectionResult>();
            }

            try
            {
                // Preprocessing
                var (inputTensor, scale, padW, padH) = PreprocessImage(frame);

                // Create input container
                var inputName = _session.InputMetadata.Keys.First();
                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor(inputName, inputTensor)
                };

                // Run inference
                using var results = _session.Run(inputs);
                
                // Get output
                var outputTensor = results.First().AsTensor<float>();
                
                // Postprocessing
                var detections = PostprocessOutput(outputTensor, frame.Width, frame.Height, scale, padW, padH);

                return detections;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[YOLO11] Detection error: {ex.Message}");
                return new List<DetectionResult>();
            }
        }

        /// <summary>
        /// Preprocess image: resize, normalize, and convert to tensor
        /// </summary>
        private (DenseTensor<float>, float, int, int) PreprocessImage(Mat frame)
        {
            // Calculate resize ratio (letterbox)
            float scale = Math.Min((float)_inputWidth / frame.Width, (float)_inputHeight / frame.Height);
            int newWidth = (int)(frame.Width * scale);
            int newHeight = (int)(frame.Height * scale);

            // Resize image
            var resized = new Mat();
            Cv2.Resize(frame, resized, new Size(newWidth, newHeight), 0, 0, InterpolationFlags.Linear);

            // Create padded image (letterbox)
            int padW = (_inputWidth - newWidth) / 2;
            int padH = (_inputHeight - newHeight) / 2;
            var padded = new Mat();
            Cv2.CopyMakeBorder(resized, padded, padH, _inputHeight - newHeight - padH, 
                              padW, _inputWidth - newWidth - padW, 
                              BorderTypes.Constant, new Scalar(114, 114, 114));

            // Convert BGR to RGB
            var rgb = new Mat();
            Cv2.CvtColor(padded, rgb, ColorConversionCodes.BGR2RGB);

            // Normalize to [0, 1] and convert to CHW format
            var tensor = new DenseTensor<float>(new[] { 1, 3, _inputHeight, _inputWidth });
            
            unsafe
            {
                byte* ptr = (byte*)rgb.Data;
                int channels = 3;
                
                for (int y = 0; y < _inputHeight; y++)
                {
                    for (int x = 0; x < _inputWidth; x++)
                    {
                        int pixelIndex = (y * _inputWidth + x) * channels;
                        
                        // RGB order, normalize to [0, 1]
                        tensor[0, 0, y, x] = ptr[pixelIndex + 0] / 255f;  // R
                        tensor[0, 1, y, x] = ptr[pixelIndex + 1] / 255f;  // G
                        tensor[0, 2, y, x] = ptr[pixelIndex + 2] / 255f;  // B
                    }
                }
            }

            resized.Dispose();
            padded.Dispose();
            rgb.Dispose();

            return (tensor, scale, padW, padH);
        }

        /// <summary>
        /// Postprocess YOLO output: NMS and coordinate conversion
        /// </summary>
        private List<DetectionResult> PostprocessOutput(Tensor<float> output, int originalWidth, int originalHeight, 
                                                         float scale, int padW, int padH)
        {
            var results = new List<DetectionResult>();
            var dimensions = output.Dimensions.ToArray();

            // YOLO11 output format: [batch, 84+num_masks, num_boxes] or [batch, num_boxes, 84+num_masks]
            // First 4 values: x, y, w, h (center format)
            // Next 80 values: class probabilities
            // Remaining: mask coefficients (for segmentation)

            int numBoxes, numFeatures;
            
            if (dimensions.Length == 3 && dimensions[1] > dimensions[2])
            {
                // Format: [1, features, boxes] - need to transpose
                numFeatures = (int)dimensions[1];
                numBoxes = (int)dimensions[2];
            }
            else
            {
                // Format: [1, boxes, features]
                numBoxes = (int)dimensions[1];
                numFeatures = (int)dimensions[2];
            }

            var boxes = new List<(Rect box, float conf, int classId)>();

            for (int i = 0; i < numBoxes; i++)
            {
                // Get box coordinates and class scores
                float centerX, centerY, width, height;
                float maxProb = 0f;
                int maxClassId = 0;

                if (dimensions[1] > dimensions[2])
                {
                    // Transposed format [features, boxes]
                    centerX = output[0, 0, i];
                    centerY = output[0, 1, i];
                    width = output[0, 2, i];
                    height = output[0, 3, i];

                    // Find max class probability
                    for (int c = 0; c < Math.Min(80, numFeatures - 4); c++)
                    {
                        float prob = output[0, 4 + c, i];
                        if (prob > maxProb)
                        {
                            maxProb = prob;
                            maxClassId = c;
                        }
                    }
                }
                else
                {
                    // Normal format [boxes, features]
                    centerX = output[0, i, 0];
                    centerY = output[0, i, 1];
                    width = output[0, i, 2];
                    height = output[0, i, 3];

                    // Find max class probability
                    for (int c = 0; c < Math.Min(80, numFeatures - 4); c++)
                    {
                        float prob = output[0, i, 4 + c];
                        if (prob > maxProb)
                        {
                            maxProb = prob;
                            maxClassId = c;
                        }
                    }
                }

                // Filter by confidence threshold
                if (maxProb < _confThreshold)
                    continue;

                // Convert from center format to corner format
                float x1 = centerX - width / 2;
                float y1 = centerY - height / 2;
                float x2 = centerX + width / 2;
                float y2 = centerY + height / 2;

                // Scale back to original image coordinates (remove padding and scale)
                x1 = (x1 - padW) / scale;
                y1 = (y1 - padH) / scale;
                x2 = (x2 - padW) / scale;
                y2 = (y2 - padH) / scale;

                // Clip to image bounds
                x1 = Math.Max(0, Math.Min(x1, originalWidth));
                y1 = Math.Max(0, Math.Min(y1, originalHeight));
                x2 = Math.Max(0, Math.Min(x2, originalWidth));
                y2 = Math.Max(0, Math.Min(y2, originalHeight));

                var rect = new Rect((int)x1, (int)y1, (int)(x2 - x1), (int)(y2 - y1));
                boxes.Add((rect, maxProb, maxClassId));
            }

            // Apply Non-Maximum Suppression (NMS)
            var indices = ApplyNMS(boxes, _iouThreshold);

            foreach (var idx in indices)
            {
                var (box, conf, classId) = boxes[idx];
                results.Add(new DetectionResult
                {
                    ClassId = classId,
                    ClassName = classId < _classNames.Length ? _classNames[classId] : $"Class_{classId}",
                    Confidence = conf,
                    BoundingBox = box
                });
            }

            return results;
        }

        /// <summary>
        /// Apply Non-Maximum Suppression
        /// </summary>
        private List<int> ApplyNMS(List<(Rect box, float conf, int classId)> boxes, float iouThreshold)
        {
            var result = new List<int>();
            var sorted = boxes
                .Select((box, index) => (box, index))
                .OrderByDescending(x => x.box.conf)
                .ToList();

            while (sorted.Any())
            {
                var best = sorted.First();
                result.Add(best.index);
                sorted.RemoveAt(0);

                sorted = sorted.Where(x =>
                {
                    float iou = CalculateIoU(best.box.box, x.box.box);
                    return iou <= iouThreshold || best.box.classId != x.box.classId;
                }).ToList();
            }

            return result;
        }

        /// <summary>
        /// Calculate Intersection over Union (IoU)
        /// </summary>
        private float CalculateIoU(Rect box1, Rect box2)
        {
            int x1 = Math.Max(box1.X, box2.X);
            int y1 = Math.Max(box1.Y, box2.Y);
            int x2 = Math.Min(box1.X + box1.Width, box2.X + box2.Width);
            int y2 = Math.Min(box1.Y + box1.Height, box2.Y + box2.Height);

            int intersection = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1);
            int union = box1.Width * box1.Height + box2.Width * box2.Height - intersection;

            return union == 0 ? 0 : (float)intersection / union;
        }

        public void Dispose()
        {
            _session?.Dispose();
            _options?.Dispose();
        }
    }
}
