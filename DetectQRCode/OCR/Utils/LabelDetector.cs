using OpenCvSharp;
using PaddleOCRSharp;
using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DetectQRCode.OCR.Utils
{
    public class LabelDetector
    {

        /// <summary>
        /// Xác d?nh t?a d? label, coi label có n?m trong Guild Box không,
        /// Input: Bitmap (BGR), t?a d? QR code
        /// Output: (rotatedRect, boxPoints, qrText, qrPoints) or (null, null, null, null) n?u label không n?m trong Guild Box
        /// </summary>
        //public static (RotatedRect? rect, OpenCvSharp.Point[]? box, string? qrText, Point2f[]? qrPoints180, Point2f[]? qrPoints1)
        public static (RotatedRect? rect, Point2f[] rectPoints, Bitmap DebugBitMap,bool rectInGuildlBox) DetectLabelRegionWithQrCode(Bitmap inputBmp, Point2f[] qrPoints)
        {
            //if (inputBmp == null)
            //    return (null, null);

            // Ð? dài c?nh QR code
            float qrSideLength = (float)Point2f.Distance(qrPoints[1], qrPoints[0]);


            // Convert Bitmap -> Mat (BGR)
            Mat src;
            using (var ms = new MemoryStream())
            {
                inputBmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                src = Cv2.ImDecode(ms.ToArray(), ImreadModes.Color);
            }

            if (qrPoints != null)
            {
                // Tính hình ch? nh?t bao quanh
                Point2f[] rectPoints = RectangleAroundQR.GetRectangleAroundQR(qrPoints, offsetX: 0.0f, offsetY: 0.00f, widthScale: 4f, heightScale: 2f, imageWidth: inputBmp.Width, imageHeight: inputBmp.Height);
                
                // V? hình ch? nh?t lên Guild Box d? debug
                Bitmap debugBmp = RectangleAroundQR.DrawDebugRectangle(inputBmp, qrPoints, rectPoints);
                // Ki?m tra null tru?c khi g?i
                //if (form1Reference != null)
                //{
                //    formform1Reference.ShowBitmapCoDung(debugBmp);
                //}
                // Hi?n th? ho?c luu file
                // pictureBox.Image = debugBmp;
                //Debug.WriteLine("V? hình ch? nh?t bao quanh QR code");
                //debugBmp.Save("D:\Project\WinForm\demo_ocr_label\debug_imgs\veHCN.jpg");

                // Ki?m tra xem 4 d?nh có n?m trong ROI box không
                // ROI box là inputBmp (?nh dã crop), nên ki?m tra trong bounds (0, 0, width, height)
                bool rectInGuildlBox = true;
                float roiWidth = inputBmp.Width;
                float roiHeight = inputBmp.Height;
                foreach (var point in rectPoints)
                {
                    if (point.X < 0 || point.X >= roiWidth || point.Y < 0 || point.Y >= roiHeight)
                    {
                        rectInGuildlBox = false;
                        break;
                    }
                }

                // tính react
                RotatedRect rect = Cv2.MinAreaRect(rectPoints);
                

                return (rect, rectPoints, debugBmp, rectInGuildlBox);
            }

            return (null, null, null, false);

            //try
            //{
            //    // 1?? To grayscale
            //    using var gray = new Mat();
            //    Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

            //    // 2?? Làm mu?t ?nh — lo?i b? noise cao t?n
            //    // GaussianBlur giúp làm m?m biên, tránh nhi?u tr?ng den l?
            //    Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(5, 5), 0);

            //    // 3?? Làm n?i b?t c?nh (tùy ch?n, tang tuong ph?n n?u label sáng không d?u)
            //    // Uncomment n?u c?n tang d? nét vùng sáng
            //    // Cv2.Laplacian(gray, gray, MatType.CV_8U, 3);

            //    // 3 Binary threshold (label tr?ng nên threshold cao)
            //    using var binary = new Mat();
            //    //Debug.WriteLine("Ngu?ng sáng nh?n di?n label: " + thresholdValue);
            //    Cv2.Threshold(gray, binary, thresholdValue, 255, ThresholdTypes.Binary);

            //    // 4 Morphological operations d? lo?i b? nhi?u & làm nét vùng label
            //    using var morph = new Mat();
            //    Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3));

            //    // M? (open): xóa di?m nhi?u nh?
            //    Cv2.MorphologyEx(binary, morph, MorphTypes.Open, kernel, iterations: 1);
            //    // Ðóng (close): làm vùng label kín, li?n m?ch
            //    Cv2.MorphologyEx(morph, morph, MorphTypes.Close, kernel, iterations: 2);


            //    // 3?? Find contours (external)
            //    Cv2.FindContours(binary, out OpenCvSharp.Point[][] contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            //    if (contours == null || contours.Length == 0)
            //        return (null, null, null, null, null);

            //    // 4?? Ch?n contour l?n nh?t
            //    OpenCvSharp.Point[] biggest = null!;
            //    double maxArea = 0;
            //    foreach (var c in contours)
            //    {
            //        double area = Cv2.ContourArea(c);
            //        if (area > maxArea)
            //        {
            //            maxArea = area;
            //            biggest = c;
            //        }
            //    }

            //    //if (biggest == null || maxArea < 1000)
            //     if (biggest == null)
            //         return (null, null, null, null, null);

            //    // 5?? L?y MinAreaRect và box
            //    var rect = Cv2.MinAreaRect(biggest);
            //    var ptsF = rect.Points();
            //    var box = ptsF.Select(p => new OpenCvSharp.Point((int)Math.Round(p.X), (int)Math.Round(p.Y))).ToArray();

            //    // 6?? Crop vùng label theo bounding box (d? ki?m tra QR)
            //    var bound = Cv2.BoundingRect(biggest);
            //    bound.X = Math.Max(0, bound.X);
            //    bound.Y = Math.Max(0, bound.Y);
            //    bound.Width = Math.Min(src.Width - bound.X, bound.Width);
            //    bound.Height = Math.Min(src.Height - bound.Y, bound.Height);

            //    using var labelRoi = new Mat(src, bound);

            //    // 7?? Dò QR code trong vùng label
            //    string qrText = "";
            //    Point2f[] qrPoints180 = null!; // Ð?i tên d? rõ nghia
            //    Point2f[] qrPoints = null!;
            //    try
            //    {
            //        using var qr = new QRCodeDetector();
            //        using var straight = new Mat();

            //        qrText = qr.DetectAndDecode(labelRoi, out qrPoints180, straight);

            //        if (qrPoints180 != null)
            //        {
            //            qrPoints = qrPoints180;
            //            for (int i = 0; i < qrPoints.Length; i++)
            //            {
            //                qrPoints[i].X += bound.X;
            //                qrPoints[i].Y += bound.Y;
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        System.Diagnostics.Debug.WriteLine($"[QR ERROR] {ex.Message}");
            //    }

            //    // 8?? Ch? tr? v? n?u có QR th?t
            //    if (!string.IsNullOrEmpty(qrText))
            //        //Debug.WriteLine($"? k phát hi?n qrtexxt");
            //        return (rect, box, qrText, qrPoints180, qrPoints);

            //    return (null, null, null, null, null);
            //}
            //finally
            //{
            //    src.Dispose();
            //}
        }





        /// <summary>
        /// Xoay và c?t label theo t?a d? rect trong ROI.
        /// Nh?n vào: ROI bitmap, rect, box, qrPoints ? tr? v? ?nh label dã xoay th?ng.
        /// </summary>
        public static (Bitmap BitMapCropped, OpenCvSharp.Point[] qrBox) CropAndAlignLabel(Bitmap roi, RotatedRect rect, OpenCvSharp.Point2f[] box,
                                Point2f[] qrPoints180, Point2f[] qrPoints)
        {
            try
            {
                if (roi == null)
                    throw new ArgumentNullException(nameof(roi));

                using var src = BitmapToMat(roi);

                // ?? 1) Kích thu?c label
                int labelWidth = (int)rect.Size.Width;
                int labelHeight = (int)rect.Size.Height;
                if (labelWidth <= 0 || labelHeight <= 0)
                    return (null, null);

                // ?? 2) Chu?n hóa góc xoay
                float angle = rect.Angle;

                if (rect.Size.Width < rect.Size.Height)
                    angle += 90;

                if (angle >= 135 && angle <= 180)
                {
                    angle -= 180;
                    labelWidth = (int)rect.Size.Height;
                    labelHeight = (int)rect.Size.Width;
                }
                if (angle > 90 && angle <= 135)
                {
                    labelWidth = (int)rect.Size.Height;
                    labelHeight = (int)rect.Size.Width;
                }

                float labelAngle = angle;

                // ?? 3) Tính góc QR Code t? qrPoints180 (dùng d? xác d?nh ngu?c)
                Point2f vec_QR_Top = qrPoints180[1] - qrPoints180[0];
                float qrAngle = (float)(Math.Atan2(vec_QR_Top.Y, vec_QR_Top.X) * (180.0 / Math.PI));

                // ?? 4) So sánh góc
                float deltaAngle = labelAngle - qrAngle;
                while (deltaAngle <= -180) deltaAngle += 360;
                while (deltaAngle > 180) deltaAngle -= 360;
                bool needs180Flip = Math.Abs(deltaAngle) > 90;

                //Debug.WriteLine($"?? Label={labelAngle:F1}°, QR={qrAngle:F1}°, ?={deltaAngle:F1}° ? Flip180={needs180Flip}");

                // ?? 5) Ma tr?n xoay quanh tâm label
                Mat rotationMatrix = Cv2.GetRotationMatrix2D(rect.Center, labelAngle, 1.0);

                // ?? 6) Xoay ROI
                Mat rotated = new Mat();
                Cv2.WarpAffine(src, rotated, rotationMatrix, src.Size(), InterpolationFlags.Linear, BorderTypes.Replicate);

                // ?? 7) C?t vùng label
                OpenCvSharp.Point2f center = rect.Center;
                int x = (int)(center.X - labelWidth / 2.0f);
                int y = (int)(center.Y - labelHeight / 2.0f);

                x = Math.Max(0, Math.Min(x, rotated.Width - 1));
                y = Math.Max(0, Math.Min(y, rotated.Height - 1));
                labelWidth = Math.Min(labelWidth, rotated.Width - x);
                labelHeight = Math.Min(labelHeight, rotated.Height - y);

                OpenCvSharp.Rect cropRect = new(x, y, labelWidth, labelHeight);
                Mat cropped = new Mat(rotated, cropRect);

                // ?? 8) Tính l?i t?a d? QR trong ?nh dã xoay & c?t (d?a trên qrPoints)
                Point2f[] rotatedQRPoints = new Point2f[qrPoints.Length];

                // --- affine chu?n ---
                Mat affine33 = Mat.Eye(3, 3, MatType.CV_64F);
                rotationMatrix.CopyTo(affine33[new OpenCvSharp.Rect(0, 0, 3, 2)]);

                Mat cropTranslate = Mat.Eye(3, 3, MatType.CV_64F);
                cropTranslate.At<double>(0, 2) = -x;
                cropTranslate.At<double>(1, 2) = -y;

                Mat finalTransform = cropTranslate * affine33;

                for (int i = 0; i < qrPoints.Length; i++)
                {
                    double px = qrPoints[i].X;
                    double py = qrPoints[i].Y;

                    double X = finalTransform.At<double>(0, 0) * px +
                               finalTransform.At<double>(0, 1) * py +
                               finalTransform.At<double>(0, 2);
                    double Y = finalTransform.At<double>(1, 0) * px +
                               finalTransform.At<double>(1, 1) * py +
                               finalTransform.At<double>(1, 2);

                    rotatedQRPoints[i] = new Point2f((float)X, (float)Y);
                }

                // ?? 9) L?t 180° n?u c?n
                if (needs180Flip)
                {
                    Cv2.Rotate(cropped, cropped, RotateFlags.Rotate180);
                    for (int i = 0; i < rotatedQRPoints.Length; i++)
                    {
                        rotatedQRPoints[i].X = labelWidth - rotatedQRPoints[i].X;
                        rotatedQRPoints[i].Y = labelHeight - rotatedQRPoints[i].Y;
                    }
                    //Debug.WriteLine("?? Ðã xoay l?i 180° (d?a trên QR geometry).");
                }

                // ?? 10) ?nh ph?i có 3 kênh
                if (cropped.Channels() == 1)
                    Cv2.CvtColor(cropped, cropped, ColorConversionCodes.GRAY2BGR);

                // ?? 11) Debug log
                //Debug.WriteLine($"Cropped size: {cropped.Width}x{cropped.Height}");
                //for (int i = 0; i < rotatedQRPoints.Length; i++)
                //    Debug.WriteLine($"   ? QR[{i}] after transform: ({rotatedQRPoints[i].X:F1}, {rotatedQRPoints[i].Y:F1})");

                // ?? 12) V? QR box
                OpenCvSharp.Point[] qrBox = rotatedQRPoints
                    .Select(p => new OpenCvSharp.Point((int)Math.Round(p.X), (int)Math.Round(p.Y)))
                    .ToArray();


                ////DEBUG: V? box QR lên ?nh cropped
                //for (int i = 0; i < qrBox.Length; i++)
                //{
                //   qrBox[i].X = Math.Max(0, Math.Min(qrBox[i].X, cropped.Width - 1));
                //   qrBox[i].Y = Math.Max(0, Math.Min(qrBox[i].Y, cropped.Height - 1));
                //}

                //Cv2.Polylines(cropped, new[] { qrBox }, true, new Scalar(0, 0, 255), 2);
                //for (int i = 0; i < qrBox.Length; i++)
                //   Cv2.Circle(cropped, qrBox[i], 4, new Scalar(0, 255, 0), -1);

                Bitmap BitMapCropped = MatToBitmap(cropped);

                 // ?? 13) Tr? k?t qu?
                 return (BitMapCropped, qrBox);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CropAndAlignLabel ERROR] {ex.Message}");
                return (null, null);
            }
        }

        /// <summary>
        /// Tìm QR code trong vùng ROI.
        /// </summary>
        /// <param name="roi">Vùng ?nh c?n tìm QR code</param>
        /// <returns>T?a d? 4 di?m c?a QR code (Point2f[]) n?u tìm th?y, null n?u không tìm th?y</returns>
        public static (Point2f[]? qrPoints, string qrText) DetectQRCode(Bitmap roi)
        {
            if (roi == null)
                return (null, null);

            try
            {
                using var mat = BitmapToMat(roi);
                using var qr = new QRCodeDetector();
                using var straight = new Mat();

                string qrText = qr.DetectAndDecode(mat, out Point2f[] qrPoints, straight);

                if (!string.IsNullOrEmpty(qrText) && qrPoints != null && qrPoints.Length == 4)
                    return (qrPoints, qrText);

                return (null, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DetectQRCode ERROR] {ex.Message}");
                return (null, null);
            }
        }

        // === Helper ===
        public static Mat BitmapToMat(Bitmap bmp)
        {
            using var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            return Cv2.ImDecode(ms.ToArray(), ImreadModes.Color);
        }

        public static Bitmap MatToBitmap(Mat mat)
        {
            Cv2.ImEncode(".png", mat, out var buf);
            using var ms = new MemoryStream(buf);
            using var tmp = new Bitmap(ms);
            return new Bitmap(tmp);
        }


    }
}

