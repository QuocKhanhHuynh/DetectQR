using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectQRCode.OCR.Utils
{
    public class RectangleAroundQR
    {
        public static Bitmap DrawDebugRectangle(Bitmap inputBmp, Point2f[] qrPoints, Point2f[] rectPoints)
        {
            if (inputBmp == null)
                return null;

            using var mat = LabelDetector.BitmapToMat(inputBmp);
            Mat debugMat = mat.Clone();

            // V? hình ch? nh?t (màu xanh lá)
            if (rectPoints != null && rectPoints.Length == 4)
            {
                OpenCvSharp.Point[] rectPts = rectPoints.Select(p => new OpenCvSharp.Point((int)p.X, (int)p.Y)).ToArray();
                Cv2.Polylines(debugMat, new[] { rectPts }, true, new Scalar(0, 255, 0), 3);

                // V? các d?nh và nhãn
                for (int i = 0; i < rectPts.Length; i++)
                {
                    Cv2.Circle(debugMat, rectPts[i], 5, new Scalar(0, 255, 0), -1);
                    Cv2.PutText(debugMat, $"R{i}", new OpenCvSharp.Point(rectPts[i].X + 8, rectPts[i].Y - 8),
                        HersheyFonts.HersheySimplex, 0.5, new Scalar(0, 255, 0), 1);
                }
            }

            // V? QR code (màu d?)
            if (qrPoints != null && qrPoints.Length == 4)
            {
                OpenCvSharp.Point[] qrPts = qrPoints.Select(p => new OpenCvSharp.Point((int)p.X, (int)p.Y)).ToArray();
                Cv2.Polylines(debugMat, new[] { qrPts }, true, new Scalar(0, 0, 255), 2);

                // V? các di?m QR và nhãn
                for (int i = 0; i < qrPts.Length; i++)
                {
                    Cv2.Circle(debugMat, qrPts[i], 4, new Scalar(0, 0, 255), -1);
                    Cv2.PutText(debugMat, $"Q{i}", new OpenCvSharp.Point(qrPts[i].X + 8, qrPts[i].Y + 8),
                        HersheyFonts.HersheySimplex, 0.5, new Scalar(0, 0, 255), 1);
                }
            }

            return LabelDetector.MatToBitmap(debugMat);
        }

        /// <summary>
        /// Tính 4 góc hình ch? nh?t d?a trên QR dã xoay th?ng v?i 3 di?m chu?n.
        /// </summary>
        /// <param name="qrPoints">4 di?m QR code (Q0, Q1, Q2, Q3)</param>
        /// <param name="imageWidth">Chi?u r?ng c?a ?nh d? clamp các di?m</param>
        /// <param name="imageHeight">Chi?u cao c?a ?nh d? clamp các di?m</param>
        /// <returns>4 d?nh hình ch? nh?t (R0, R1, R2, R3) dã du?c gi?i h?n trong ph?m vi ?nh</returns>
        public static Point2f[] GetRectangleAroundQR(Point2f[] qrPoints, float offsetX, float offsetY, float widthScale, float heightScale, float imageWidth, float imageHeight)
        {
            try
            {
                var a = utils.fileConfig;
                if (qrPoints == null || qrPoints.Length != 4)
                    return null;

                // Tính vector hu?ng c?a 2 c?nh QR
                Point2f vecHorizontal = qrPoints[1] - qrPoints[0]; // Q0 -> Q1: hu?ng ngang
                Point2f vecVertical = qrPoints[3] - qrPoints[0];   // Q0 -> Q3: hu?ng d?c
                                                                   // Tính d? dài c?nh QR
                float qrSideLength = (float)Math.Sqrt(vecHorizontal.X * vecHorizontal.X + vecHorizontal.Y * vecHorizontal.Y);

                // Chu?n hóa vector thành vector don v?
                float lenH = (float)Math.Sqrt(vecHorizontal.X * vecHorizontal.X + vecHorizontal.Y * vecHorizontal.Y);
                float lenV = (float)Math.Sqrt(vecVertical.X * vecVertical.X + vecVertical.Y * vecVertical.Y);
                Point2f unitH = lenH > 0 ? new Point2f(vecHorizontal.X / lenH, vecHorizontal.Y / lenH) : new Point2f(1, 0);
                Point2f unitV = lenV > 0 ? new Point2f(vecVertical.X / lenV, vecVertical.Y / lenV) : new Point2f(0, 1);

                // R0: T? Q0, di lên (ngu?c hu?ng Q0->Q3) 1qr, r?i d?ch sang trái 2.5qr
                Point2f R0 = qrPoints[0] - unitV * (utils.fileConfig.labelRectangle.up * qrSideLength) - unitH * (utils.fileConfig.labelRectangle.left * qrSideLength);

                // R1: T? Q0, di lên (ngu?c hu?ng Q0->Q3) 1qr, r?i d?ch sang ph?i 0.5qr
                Point2f R1 = qrPoints[0] - unitV * (utils.fileConfig.labelRectangle.up * qrSideLength) + unitH * (utils.fileConfig.labelRectangle.right * qrSideLength);

                // R2: T? Q0, di xu?ng (cùng hu?ng Q0->Q3) 2qr, r?i d?ch sang ph?i 1qr
                Point2f R2 = qrPoints[0] + unitV * (utils.fileConfig.labelRectangle.down * qrSideLength) + unitH * (utils.fileConfig.labelRectangle.right * qrSideLength);

                // R3: T? R0, di xu?ng 2qr, r?i d?ch sang trái 2.5qr    
                Point2f R3 = qrPoints[0] + unitV * (utils.fileConfig.labelRectangle.down * qrSideLength) - unitH * (utils.fileConfig.labelRectangle.left * qrSideLength);

                // Tính offset c?n thi?t d? d?ch chuy?n toàn b? guidebox v? trong frame
                // Ði?u này giúp gi? nguyên hình d?ng guidebox, không b? bi?n d?ng
                Point2f[] tempPoints = new Point2f[] { R0, R1, R2, R3 };
                Point2f offset = CalculateOffsetToFitInFrame(tempPoints, imageWidth, imageHeight);

                // Áp d?ng offset cho t?t c? các di?m
                R0 += offset;
                R1 += offset;
                R2 += offset;
                R3 += offset;

                Point2f[] rectPoints = new Point2f[4];
                rectPoints[0] = R0;
                rectPoints[1] = R1;
                rectPoints[2] = R2;
                rectPoints[3] = R3;

                return rectPoints;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetRectangleAroundQR: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Tính offset c?n thi?t d? d?ch chuy?n toàn b? guidebox v? trong frame
        /// mà v?n gi? nguyên hình d?ng (không bi?n d?ng)
        /// </summary>
        private static Point2f CalculateOffsetToFitInFrame(Point2f[] points, float maxWidth, float maxHeight)
        {
            float offsetX = 0;
            float offsetY = 0;

            // Tìm t?a d? min/max c?a t?t c? các di?m
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (var point in points)
            {
                minX = Math.Min(minX, point.X);
                maxX = Math.Max(maxX, point.X);
                minY = Math.Min(minY, point.Y);
                maxY = Math.Max(maxY, point.Y);
            }

            // X? lý tr?c X: Uu tiên biên nào vi ph?m nhi?u hon
            if (minX < 0 && maxX >= maxWidth)
            {
                // Guidebox l?n hon frame, uu tiên can gi?a ho?c gi? nguyên
                // Nhung t?t nh?t là uu tiên biên trái
                offsetX = -minX;
            }
            else if (minX < 0)
            {
                // Vu?t biên trái, d?ch sang ph?i
                offsetX = -minX;
            }
            else if (maxX >= maxWidth)
            {
                // Vu?t biên ph?i, d?ch sang trái
                offsetX = maxWidth - 1 - maxX;
            }

            // X? lý tr?c Y: Uu tiên biên nào vi ph?m nhi?u hon
            if (minY < 0 && maxY >= maxHeight)
            {
                // Guidebox l?n hon frame, uu tiên can gi?a ho?c gi? nguyên
                // Nhung t?t nh?t là uu tiên biên trên
                offsetY = -minY;
            }
            else if (minY < 0)
            {
                // Vu?t biên trên, d?ch xu?ng
                offsetY = -minY;
            }
            else if (maxY >= maxHeight)
            {
                // Vu?t biên du?i, d?ch lên
                offsetY = maxHeight - 1 - maxY;
            }

            return new Point2f(offsetX, offsetY);
        }
    }
}

