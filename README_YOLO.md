# YOLO11 Integration - Hướng dẫn sử dụng

## 📋 Tổng quan

Ứng dụng hiện đã được tích hợp **YOLO11 Detector** để phát hiện QR code và label thay vì phương pháp cũ.

### Điểm khác biệt:

| Phương pháp | File | Cách hoạt động |
|-------------|------|----------------|
| **V1 (Cũ)** | `DetectLabelFromImage.cs` | Phát hiện QR bằng OpenCV, sau đó detect label region |
| **V2 (Mới - YOLO)** | `DetectLabelFromImageV2.cs` | Dùng YOLO11 để detect cả QR và label cùng lúc |

---

## 🚀 Cách sử dụng

### 1. **Chuẩn bị Model YOLO11**

#### Option A: Tải model có sẵn (nếu đã train)
```bash
# Đặt file model vào thư mục:
<ProjectRoot>/models/yolo11n.onnx
```

#### Option B: Train model mới
1. Sử dụng Ultralytics YOLO11 để train model detect 2 classes:
   - `qr_code`: QR code trên label
   - `label`: Toàn bộ label/nhãn

2. Export sang ONNX:
```python
from ultralytics import YOLO

# Load trained model
model = YOLO('best.pt')

# Export to ONNX
model.export(format='onnx', simplify=True)
```

3. Đổi tên file thành `yolo11n.onnx` và đặt vào `models/` folder

### 2. **Cấu hình Model Path (Tùy chọn)**

Nếu muốn thay đổi đường dẫn model, sửa trong `Form1.cs`:

```csharp
private Yolo11Detector? InitializeYoloDetector()
{
    // THAY ĐỔI ĐƯỜNG DẪN Ở ĐÂY
    string modelPath = System.IO.Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, 
        "models", 
        "yolo11n.onnx"  // ← Đổi tên file model
    );
    
    // THAY ĐỔI CLASS NAMES
    string[] classNames = new[] { "qr_code", "label" }; // ← Đổi tên classes
    
    // ...
}
```

### 3. **Chạy ứng dụng**

```bash
cd DetectQR
dotnet build
dotnet run
```

---

## ⚙️ Cơ chế hoạt động

### Fallback Logic
Ứng dụng tự động fallback về phương pháp cũ nếu không tìm thấy model:

```
1. App starts
2. Camera frame arrives
3. Try initialize YOLO detector
   ├─ ✅ Model found → Use DetectLabelFromImageV2 (YOLO)
   └─ ❌ Model NOT found → Use DetectLabelFromImage (original)
```

### Lazy Loading
YOLO detector chỉ được load 1 lần khi frame đầu tiên được xử lý:

```csharp
if (_yoloDetector == null)
{
    _yoloDetector = InitializeYoloDetector();
}
```

---

## 📊 So sánh hiệu năng

| Tiêu chí | V1 (Original) | V2 (YOLO) |
|----------|---------------|-----------|
| **Độ chính xác** | Phụ thuộc vào điều kiện sáng | Cao, ổn định hơn |
| **Tốc độ** | ~50-100ms | ~30-50ms (tùy model) |
| **Xử lý nghiêng** | Cần preprocessing | YOLO tự handle |
| **Input** | Bitmap → Mat conversion | Trực tiếp Mat ✅ |

---

## 🔧 Troubleshooting

### ❌ Lỗi: "YOLO model not found"
```
[⚠] YOLO model not found: D:\...\models\yolo11n.onnx
[ℹ] Using fallback detection method (original DetectLabelFromImage)
```

**Giải pháp:**
- Kiểm tra file `models/yolo11n.onnx` có tồn tại không
- Kiểm tra đường dẫn trong `InitializeYoloDetector()`

### ❌ Lỗi: "Failed to initialize YOLO"
```
[⚠] Failed to initialize YOLO: <error message>
```

**Giải pháp:**
- Đảm bảo model ONNX compatible với ONNX Runtime version hiện tại
- Kiểm tra model có đúng format YOLO11 không
- Thử re-export model từ `.pt` sang `.onnx`

### 🐌 Detection chậm
- Sử dụng model nhẹ hơn: `yolo11n.onnx` (nano) thay vì `yolo11x.onnx` (extra large)
- Giảm resolution input image (hiện tại: 640x640)
- Tăng `MIN_OCR_INTERVAL_MS` trong `Form1.cs`

---

## 📝 Files đã thay đổi

| File | Mô tả |
|------|-------|
| `Form1.cs` | ✏️ Thêm YOLO detector initialization và sử dụng V2 |
| `DetectLabelFromImageV2.cs` | ✨ **MỚI** - YOLO-based detection |
| `Yolo11Detector.cs` | ✅ Đã có sẵn - YOLO detector wrapper |

---

## 🎯 Tiếp theo

### Nâng cao hiệu năng:
1. **Optimize model**: Sử dụng TensorRT hoặc quantization
2. **GPU acceleration**: Enable CUDA trong ONNX Runtime
3. **Batch processing**: Xử lý nhiều frames cùng lúc

### Cải thiện độ chính xác:
1. **Train thêm data**: Thêm ảnh với điều kiện sáng khác nhau
2. **Augmentation**: Sử dụng rotate, blur, brightness augmentation khi train
3. **Ensemble**: Kết hợp YOLO + traditional CV methods

---

## 📞 Hỗ trợ

Nếu gặp vấn đề, kiểm tra Debug Output:
```
[✓] YOLO11 Detector initialized successfully!
[YOLO] Detected: qr_code (95%) at [100, 150, 200, 200]
[YOLO] Detected: label (89%) at [50, 100, 400, 300]
[QR] Text: ABC123456
[OCR] ProductTotal: 100
```
