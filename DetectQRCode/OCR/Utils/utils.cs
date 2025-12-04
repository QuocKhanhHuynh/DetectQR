using System;
using System.Diagnostics;

namespace DetectQRCode.OCR.Utils
{
    public static class utils
    {
        // Config m?c d?nh - không còn d?c t? file JSON n?a
        public static Config fileConfig = new Config();
        
        // Static constructor - kh?i t?o config m?c d?nh khi class du?c load l?n d?u
        static utils()
        {
            Debug.WriteLine("Kh?i t?o OCR config m?c d?nh (không dùng file JSON)");
        }
        
        // Gi? l?i method này d? tuong thích v?i code cu, nhung không làm gì
        [Obsolete("Không còn dùng file JSON n?a, config m?c d?nh du?c kh?i t?o t? d?ng")]
        public static void LoadConfigFile(string configFileName)
        {
            // Không làm gì - config dã du?c kh?i t?o m?c d?nh
            Debug.WriteLine("LoadConfigFile du?c g?i nhung không còn dùng file JSON n?a");
        }
    }
}

