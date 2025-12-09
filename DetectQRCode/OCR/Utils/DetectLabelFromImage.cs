using DetectQRCode.Models.Camera;
using OpenCvSharp;
using PaddleOCRSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectQRCode.OCR.Utils
{
    public static class DetectLabelFromImage
    {
        public static DetectInfo DetectLabel(Mat frame, PaddleOCREngine ocr, int currentThreshold,  OverlayPictureBox cameraBox, PictureBox picPreprocessed)
        {
            try
            {
               
                var bmpFull = MatToBitmap(frame);
                var roiResult = GetGuideBoxRoi(bmpFull, cameraBox.GuideBox, cameraBox);
                var roi = roiResult.Image;
                var mapped = roiResult.Mapped;
                var result = new DetectInfo();

                if (roi == null)
                {
                    // Không có ROI h?p l? ? hi?n th? ?nh g?c
                    cameraBox.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            var old = cameraBox.Image;
                            // CLONE d? tránh GDI+ "Object is currently in use elsewhere"
                            cameraBox.Image = (Bitmap)bmpFull.Clone();
                            old?.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[? DISPLAY FRAME ERROR] {ex.Message}");
                        }
                    }));
                    // Gi?i phóng b?n g?c sau khi dã clone cho UI
                    bmpFull.Dispose();
                }

                try
                {
                    using var mat = frame.Clone();
                    
                    // Hiển thị ROI lên picPreprocessed
                    if (roi != null && picPreprocessed != null)
                    {
                        picPreprocessed.BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                var old = picPreprocessed.Image;
                                picPreprocessed.Image = (Bitmap)roi.Clone();
                                old?.Dispose();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"[⚠ DISPLAY ROI ERROR] {ex.Message}");
                            }
                        }));
                    }

                    var (qrPoints, qrText) = LabelDetector.DetectQRCode(roi);// LabelDetectorZXing.DetectQRCodeZXing(roi); //LabelDetector.DetectQRCode(roi);
                    //var (qrPoints1, qrText1) = LabelDetector.DetectQRCode(roi);
                    //var a = qrPoints;
                    //var b = qrPoints1;
                    result.QRCode = qrText;

                    if (qrPoints != null)
                    {
                        var (rect, rectPoints, debugBmp1, rectInGuildlBox) = LabelDetector.DetectLabelRegionWithQrCode(roi, qrPoints);

                        if (rectPoints != null && rectInGuildlBox)
                        {
                            var qrBox = qrPoints.Select(p =>
                                   new OpenCvSharp.Point(p.X + mapped.X, p.Y + mapped.Y)
                               ).ToArray();

                            var qrRectangle = rectPoints.Select(p =>
                                   new OpenCvSharp.Point(p.X + mapped.X, p.Y + mapped.Y)
                               ).ToArray();

                            Cv2.Polylines(mat, new[] { qrBox }, true, Scalar.Lime, 2);
                            Cv2.Polylines(mat, new[] { qrRectangle }, true, Scalar.Lime, 2);

                            var debugBmp = MatToBitmap(mat);


                            // Hi?n th? ?nh ra li?n
                            cameraBox.BeginInvoke(new Action(() =>
                            {
                                var old = cameraBox.Image;
                                cameraBox.Image = (Bitmap)debugBmp.Clone();
                                cameraBox.IsObjectDetected = true;
                                old?.Dispose();
                            }));

                            var (alignedLabel, qrBoxScale) = LabelDetector.CropAndAlignLabel(roi, rect.Value, rectPoints, rectPoints, qrPoints);

                            if (alignedLabel != null)
                            {
                                var mergedCrop = CropComponent.CropAndMergeBottomLeftAndAboveQr(alignedLabel, qrBoxScale);
                                if (mergedCrop != null)
                                {
                                    var (ocrTexts, minScore, debugText) = ExtractTextsFromMergedCrop(ocr, mergedCrop);
                                    mergedCrop.Dispose();
                                    alignedLabel.Dispose();
                                    roi.Dispose();
                                    bmpFull.Dispose();
                                    result.ProductTotal = ocrTexts[0];
                                    result.ProductCode = ocrTexts[1];
                                    result.Size = ocrTexts[3];
                                    result.Color = ocrTexts[2];
                                }
                                alignedLabel.Dispose();
                            }
                            roi.Dispose();
                            bmpFull.Dispose();
                        }
                        else
                        {
                            cameraBox.BeginInvoke(new Action(() =>
                            {
                                cameraBox.IsObjectDetected = false;
                                cameraBox.Invalidate();

                            }));

                            var qrBox = qrPoints.Select(p =>
                                   new OpenCvSharp.Point(p.X + mapped.X, p.Y + mapped.Y)
                               ).ToArray();

                            // chuy?n t?a d? hình ch? nh?t quanh Label -> t?a d? full ?nh
                            var qrRectangle = rectPoints.Select(p =>
                                new OpenCvSharp.Point(p.X + mapped.X, p.Y + mapped.Y)
                            ).ToArray();

                            // 3?? V? khung label và tâm trên frame full
                            Cv2.Polylines(mat, new[] { qrBox }, true, Scalar.Lime, 2);

                            // 3?? V? khung HCN lên frame full
                            Cv2.Polylines(mat, new[] { qrRectangle }, true, Scalar.Red, 2);

                            roi.Dispose();
                            bmpFull.Dispose();
                        }
                        /*var debugBmp = MatToBitmap(mat);
                        cameraBox.BeginInvoke(new Action(() =>
                        {
                            var old = cameraBox.Image;
                            cameraBox.Image = (Bitmap)debugBmp.Clone();
                            cameraBox.IsObjectDetected = true;
                            old?.Dispose();
                        }));
                        roi.Dispose();
                        bmpFull.Dispose();*/
                    }
                    else
                    {
                        cameraBox.BeginInvoke(new Action(() =>
                        {
                            cameraBox.IsObjectDetected = false;  // ?? tr? l?i khung d?
                            cameraBox.Invalidate();
                        }));
                        Debug.WriteLine("Không phát hi?n du?c QR trong Guild Box!");
                        if (roi != null)
                        {
                            roi.Dispose();
                        }
                        if (bmpFull != null)
                        {
                            bmpFull.Dispose();
                        }
                       
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    // X? lý l?i n?u c?n
                    roi?.Dispose();
                    bmpFull?.Dispose();
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[? DETECT LABEL ERROR] {ex.Message}");
                return null;
            }

        }

        private static Bitmap PreImageProcess(Bitmap imageInput)
        {
            //------------------------------------
            // 0. Convert Bitmap → Mat
            //------------------------------------
            using Mat input = BitmapToMat(imageInput);

            //------------------------------------
            // 1. Grayscale
            //------------------------------------
            /*using Mat gray = new Mat();
            Cv2.CvtColor(input, gray, ColorConversionCodes.BGR2GRAY);

            //------------------------------------
            // 2. CLAHE (tăng tương phản)
            //------------------------------------
            using var clahe = Cv2.CreateCLAHE(clipLimit: 2.0, tileGridSize: new OpenCvSharp.Size(8, 8));
            using Mat enhanced = new Mat();
            clahe.Apply(gray, enhanced);

            //------------------------------------
            // 3. Giảm noise
            //------------------------------------
            using Mat blur = new Mat();
            Cv2.GaussianBlur(enhanced, blur, new OpenCvSharp.Size(3, 3), 0);

            using Mat bilateral = new Mat();
            Cv2.BilateralFilter(blur, bilateral, 9, 75, 75);

            //------------------------------------
            // 4. Adaptive Threshold
            //------------------------------------
            using Mat thresh = new Mat();
            Cv2.AdaptiveThreshold(
                bilateral,
                thresh,
                255,
                AdaptiveThresholdTypes.GaussianC,
                ThresholdTypes.Binary,
                21,
                2
            );

            //------------------------------------
            // 5. Sharpen
            //------------------------------------

            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3));



            using Mat sharp = new Mat();
            Cv2.Filter2D(thresh, sharp, -1, kernel);*/

            //------------------------------------
            // 6. Resize (tăng kích thước QR)
            //------------------------------------
            using Mat resized = new Mat();
            Cv2.Resize(input, resized, new OpenCvSharp.Size(), 5.0, 5.0);

            //------------------------------------
            // Trả về Bitmap
            //------------------------------------
            return MatToBitmap(resized);
        }

        private static Bitmap MatToBitmap(Mat mat)
        {
            int w = mat.Width;
            int h = mat.Height;
            int channels = mat.Channels(); // 3 = BGR

            Bitmap bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            var rect = new Rectangle(0, 0, w, h);
            var bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);

            int stride = bmpData.Stride;
            int rowLength = w * channels;

            byte[] buffer = new byte[rowLength];

            for (int y = 0; y < h; y++)
            {
                // src: pointer vào d? li?u Mat
                IntPtr src = mat.Data + y * (int)mat.Step();
                // copy t? Mat vào byte[]
                System.Runtime.InteropServices.Marshal.Copy(src, buffer, 0, rowLength);

                // dst: con tr? d?n bitmap row
                IntPtr dst = bmpData.Scan0 + y * stride;
                // copy byte[] vào Bitmap
                System.Runtime.InteropServices.Marshal.Copy(buffer, 0, dst, rowLength);
            }

            bmp.UnlockBits(bmpData);
            return bmp;
        }

        private static Mat BitmapToMat(Bitmap bmp)
        {
            using var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            return Cv2.ImDecode(ms.ToArray(), ImreadModes.Color);
        }

        private static RoiResult GetGuideBoxRoi(Bitmap frame, Rectangle guideBox, OverlayPictureBox cameraBox)
        {
            RoiResult result = new RoiResult();

            if (frame == null || frame.Width == 0 || frame.Height == 0)
                return result;
            if (cameraBox == null || cameraBox.ClientSize.Width == 0 || cameraBox.ClientSize.Height == 0)
                return result;

            float imgW = frame.Width;
            float imgH = frame.Height;
            float boxW = cameraBox.ClientSize.Width;
            float boxH = cameraBox.ClientSize.Height;

            float scale = Math.Min(boxW / imgW, boxH / imgH);
            float drawW = imgW * scale;
            float drawH = imgH * scale;
            float offsetX = (boxW - drawW) / 2f;
            float offsetY = (boxH - drawH) / 2f;

            float x = (guideBox.X - offsetX) / scale;
            float y = (guideBox.Y - offsetY) / scale;
            float w = guideBox.Width / scale;
            float h = guideBox.Height / scale;

            x = Math.Max(0, x);
            y = Math.Max(0, y);
            w = Math.Min(imgW - x, w);
            h = Math.Min(imgH - y, h);

            var mapped = new Rectangle((int)x, (int)y, (int)w, (int)h);
            mapped.Intersect(new Rectangle(0, 0, (int)imgW, (int)imgH));

            if (mapped.Width > 0 && mapped.Height > 0)
                result.Image = frame.Clone(mapped, frame.PixelFormat);

            result.Mapped = mapped;
            result.Scale = scale;
            result.OffsetX = offsetX;
            result.OffsetY = offsetY;

            return result;
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
    }
}


