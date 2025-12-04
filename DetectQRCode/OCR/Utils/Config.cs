using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectQRCode.OCR.Utils
{
    public class Config
    {
        // vùng ch?a ch?a 3 thông tin: mã áo, size áo và màu áo
        public Component bottomLeftComponent { get; set; } = new Component
        {
            width = 0.75f,
            height = 0.35f
        };
        // vùng ch?a thông tin s? lu?ng don hàng và th? t? don hàng
        public Component aboveQrComponent { get; set; } = new Component
        {
            doiTamSangPhai = 0.2f,
            doiTamLenTren = 0.1f,
            width = 0.6f,
            height = 0.45f
        };
        // các tham s? c?a mô hình PadlleOCR
        public PaddleOCRParams modelParams { get; set; } = new PaddleOCRParams
        {
            det = true,
            cls = false,
            use_angle_cls = true,
            rec = true,
            det_db_thresh = 0.3f,
            det_db_box_thresh = 0.5f,
            cls_thresh = 0.9f,
            cpu_math_library_num_threads = 6,
            det_db_score_mode = true
        };
        public SystemArivables systemArivable { get; set; } = new SystemArivables
        {
            debugMode = false,
            showTime = true,
            saveJsonResult = false
        };
        public LabelRectangle labelRectangle { get; set; } = new LabelRectangle
        {
            up = 1.2f,
            down = 2.2f,
            left = 3.8f,
            right = 1.4f
        };
    }
        
    // mô t? m?t vùng c?t thông tin s? lu?ng don hàng - n?m phía trên qr code. Ð? l?n tính tuong d?i % so sánh v?i d? dài c?nh c?a qr code
    public class Component
    {
        public float doiTamSangPhai { get; set; }   // d?i v? trí c?t sang ph?i, tính t? góc trên bên ph?i c?a qr code
        public float doiTamLenTren { get; set; }   // d?i v? trí c?t lên trên, tính t? góc trên bên ph?i c?a qr code
        public float width { get; set; } // chi?u r?ng vùng c?t, tính t? v? trí c?t sang trái
        public float height { get; set; } // chi?u cao vùng c?t, tính t? v? trí c?t lên trên
    }

    public class PaddleOCRParams
    {
        // ?? Có nh?n di?n ch? (Detection)
        public bool det { get; set; } = true;

        // ?? Có nh?n di?n hu?ng ch? (Classification)
        public bool cls { get; set; } = false;

        // ?? S? d?ng b? phân lo?i hu?ng ch? (Angle Classifier)
        public bool use_angle_cls { get; set; }

        // ?? Có nh?n di?n n?i dung ch? (Recognition)
        public bool rec { get; set; } = true;

        // ?? Ngu?ng nh? phân hóa trong DB Detector (0.0–1.0)
        public float det_db_thresh { get; set; } = 0.3f;

        // ?? Ngu?ng confidence d? gi? l?i box (0.0–1.0)
        public float det_db_box_thresh { get; set; } = 0.5f;

        // ?? Ngu?ng confidence khi ki?m tra hu?ng ch? (classification)
        public float cls_thresh { get; set; } = 0.9f;

        // ?? B?t tang t?c tính toán b?ng Intel MKL-DNN (oneDNN)
        public bool enable_mkldnn { get; set; } = true;

        // ?? S? lu?ng CPU song song du?c dùng
        public int cpu_math_library_num_threads { get; set; } = 6;

        // tính score d?a trên da giác, chính xách hon nhung ch?m hon xíu
        public bool det_db_score_mode { get; set; } = false; 

    }
    public class SystemArivables
    {
        public bool debugMode { get; set; } = false; // luu ?nh ? t?ng model d? debug
        public bool showTime { get; set; } = true; // show th?i gian ? ch? d? debug

        public bool saveJsonResult { get; set; } = true; // luu k?t qu? d?ng json cho label trích xu?t thành công
    }

    public class LabelRectangle
    {
        public float up { get; set; }
        public float down { get; set; }
        public float left { get; set; }
        public float right { get; set; }
    }
}
