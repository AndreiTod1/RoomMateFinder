// Crop image with zoom and offset support
window.cropImageToCircle = async (options) => {
    const { imageDataUrl, cropX, cropY, cropRadius, scale, imageOffsetX, imageOffsetY, containerWidth, containerHeight } = options;
    return new Promise((resolve, reject) => {
        const img = new Image();
        img.crossOrigin = 'anonymous';
        
        img.onload = () => processCropImage(img, cropX, cropY, cropRadius, scale, imageOffsetX, imageOffsetY, containerWidth, resolve, reject);
        img.onerror = () => reject(new Error('Failed to load image'));
        img.src = imageDataUrl;
    });
};

function processCropImage(img, cropX, cropY, cropRadius, scale, imageOffsetX, imageOffsetY, containerWidth, resolve, reject) {
    try {
        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');
        
        const outputSize = 400;
        canvas.width = outputSize;
        canvas.height = outputSize;
        
        const cropParams = calculateCropParameters(img, cropX, cropY, cropRadius, scale, imageOffsetX, imageOffsetY, containerWidth);
        
        console.log('CROP DEBUG:', {
            input: { cropX, cropY, cropRadius, scale, imageOffsetX, imageOffsetY, containerWidth },
            natural: { naturalWidth: img.naturalWidth, naturalHeight: img.naturalHeight },
            display: { displayWidth: cropParams.displayWidth, displayHeight: cropParams.displayHeight },
            pixelRatio: cropParams.pixelRatio,
            cropOnImage: { x: cropParams.cropOnImageX, y: cropParams.cropOnImageY },
            source: { sx: cropParams.sx, sy: cropParams.sy, sWidth: cropParams.sWidth, sHeight: cropParams.sHeight }
        });
        
        ctx.drawImage(img, cropParams.sx, cropParams.sy, cropParams.sWidth, cropParams.sHeight, 0, 0, outputSize, outputSize);
        
        convertCanvasToDataUrl(canvas, resolve, reject);
    } catch (error) {
        reject(error);
    }
}

function calculateCropParameters(img, cropX, cropY, cropRadius, scale, imageOffsetX, imageOffsetY, containerWidth) {
    const naturalWidth = img.naturalWidth;
    const naturalHeight = img.naturalHeight;
    
    // CSS: width = containerWidth * scale, height = auto (proportional)
    const displayWidth = containerWidth * scale;
    const displayHeight = displayWidth * (naturalHeight / naturalWidth);
    
    // Ratio to convert from display pixels to natural pixels
    const pixelRatio = naturalWidth / displayWidth;
    
    // The image top-left is at (imageOffsetX, imageOffsetY)
    // The crop circle center is at (cropX, cropY) in container coordinates
    const cropOnImageX = (cropX - imageOffsetX) * pixelRatio;
    const cropOnImageY = (cropY - imageOffsetY) * pixelRatio;
    const cropRadiusNatural = cropRadius * pixelRatio;
    
    return {
        displayWidth,
        displayHeight,
        pixelRatio,
        cropOnImageX,
        cropOnImageY,
        sx: cropOnImageX - cropRadiusNatural,
        sy: cropOnImageY - cropRadiusNatural,
        sWidth: cropRadiusNatural * 2,
        sHeight: cropRadiusNatural * 2
    };
}

function convertCanvasToDataUrl(canvas, resolve, reject) {
    canvas.toBlob((blob) => {
        if (!blob) {
            reject(new Error('Failed to create blob'));
            return;
        }
        const reader = new FileReader();
        reader.onloadend = () => resolve(reader.result);
        reader.onerror = () => reject(new Error('Failed to read blob'));
        reader.readAsDataURL(blob);
    }, 'image/jpeg', 0.95);
}

// Simple center crop (kept for compatibility)
window.cropImageFromCenter = async (imageDataUrl, outputSize) => {
    return new Promise((resolve, reject) => {
        const img = new Image();
        img.crossOrigin = 'anonymous';
        
        img.onload = () => processCenterCrop(img, outputSize, resolve, reject);
        img.onerror = () => reject(new Error('Failed to load image'));
        img.src = imageDataUrl;
    });
};

function processCenterCrop(img, outputSize, resolve, reject) {
    try {
        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');
        
        canvas.width = outputSize;
        canvas.height = outputSize;
        
        const size = Math.min(img.naturalWidth, img.naturalHeight);
        const sx = (img.naturalWidth - size) / 2;
        const sy = (img.naturalHeight - size) / 2;
        
        ctx.drawImage(img, sx, sy, size, size, 0, 0, outputSize, outputSize);
        
        convertCanvasToDataUrl(canvas, resolve, reject);
    } catch (error) {
        reject(error);
    }
}
