using System.Drawing;
using OpenCvSharp;
using ZXing;
using ZXing.Common;
using System.Diagnostics;

namespace DetectQRCode.OCR.Utils
{
    public static class LabelDetectorZXing
    {
        /// <summary>
        /// Phát hiện QR code trong ảnh sử dụng thư viện ZXing
        /// </summary>
        /// <param name="roi">Vùng ảnh cần tìm QR code</param>
        /// <returns>Tọa độ 4 điểm của QR code (Point2f[]) nếu tìm thấy, null nếu không tìm thấy</returns>
        public static (Point2f[]? qrPoints, string qrText) DetectQRCodeZXing(Bitmap roi)
        {
            if (roi == null)
                return (null, null);

            try
            {
                // Khởi tạo ZXing reader cho Bitmap
                var reader = new ZXing.Windows.Compatibility.BarcodeReader
                {
                    AutoRotate = true,
                    TryInverted = true,
                    Options = new DecodingOptions
                    {
                        PossibleFormats = new[] { BarcodeFormat.QR_CODE },
                        TryHarder = true
                    }
                };

                // Decode QR code trực tiếp từ Bitmap
                var result = reader.Decode(roi);

                if (result != null && !string.IsNullOrEmpty(result.Text))
                {
                    // ZXing trả về các ResultPoint, ta cần convert sang Point2f[]
                    if (result.ResultPoints != null && result.ResultPoints.Length >= 3)
                    {
                        // ZXing thường trả về 3 hoặc 4 điểm (finder patterns)
                        // Nếu có 3 điểm, ta cần tính điểm thứ 4
                        Point2f[] qrPoints;

                        if (result.ResultPoints.Length == 4)
                        {
                            // Đã có đủ 4 điểm
                            qrPoints = new Point2f[4];
                            for (int i = 0; i < 4; i++)
                            {
                                qrPoints[i] = new Point2f(result.ResultPoints[i].X, result.ResultPoints[i].Y);
                            }
                        }
                        else if (result.ResultPoints.Length == 3)
                        {
                            // ZXing trả về 3 finder patterns (top-left, top-right, bottom-left)
                            // Ta tính điểm thứ 4 (bottom-right)
                            var p0 = new Point2f(result.ResultPoints[0].X, result.ResultPoints[0].Y); // Top-left
                            var p1 = new Point2f(result.ResultPoints[1].X, result.ResultPoints[1].Y); // Top-right
                            var p2 = new Point2f(result.ResultPoints[2].X, result.ResultPoints[2].Y); // Bottom-left
                            
                            // Tính điểm thứ 4: bottom-right = (top-right - top-left) + bottom-left
                            var p3 = new Point2f(
                                p1.X - p0.X + p2.X,
                                p1.Y - p0.Y + p2.Y
                            );

                            qrPoints = new Point2f[] { p0, p1, p3, p2 };
                        }
                        else
                        {
                            return (null, null);
                        }

                        return (qrPoints, result.Text);
                    }
                }

                return (null, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DetectQRCodeZXing ERROR] {ex.Message}");
                return (null, null);
            }
        }
    }
}
