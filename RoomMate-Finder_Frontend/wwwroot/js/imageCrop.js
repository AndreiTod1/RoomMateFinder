// Crop image with zoom and offset support
window.cropImageToCircle = async (imageDataUrl, cropX, cropY, cropRadius, scale, imageOffsetX, imageOffsetY, containerWidth, containerHeight) => {
    return new Promise((resolve, reject) => {
        const img = new Image();
        img.crossOrigin = 'anonymous';
        
        img.onload = () => {
            try {
                const canvas = document.createElement('canvas');
                const ctx = canvas.getContext('2d');
                
                const outputSize = 400;
                canvas.width = outputSize;
                canvas.height = outputSize;
                
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
                
                // Source rectangle in natural image coordinates
                const sx = cropOnImageX - cropRadiusNatural;
                const sy = cropOnImageY - cropRadiusNatural;
                const sWidth = cropRadiusNatural * 2;
                const sHeight = cropRadiusNatural * 2;
                
                console.log('CROP DEBUG:', {
                    input: { cropX, cropY, cropRadius, scale, imageOffsetX, imageOffsetY, containerWidth, containerHeight },
                    natural: { naturalWidth, naturalHeight },
                    display: { displayWidth, displayHeight },
                    pixelRatio,
                    cropOnImage: { x: cropOnImageX, y: cropOnImageY },
                    source: { sx, sy, sWidth, sHeight }
                });
                
                ctx.drawImage(img, sx, sy, sWidth, sHeight, 0, 0, outputSize, outputSize);
                
                canvas.toBlob((blob) => {
                    if (blob) {
                        const reader = new FileReader();
                        reader.onloadend = () => resolve(reader.result);
                        reader.onerror = () => reject(new Error('Failed to read blob'));
                        reader.readAsDataURL(blob);
                    } else {
                        reject(new Error('Failed to create blob'));
                    }
                }, 'image/jpeg', 0.95);
                
            } catch (error) {
                reject(error);
            }
        };
        
        img.onerror = () => reject(new Error('Failed to load image'));
        img.src = imageDataUrl;
    });
};

// Simple center crop (kept for compatibility)
window.cropImageFromCenter = async (imageDataUrl, outputSize) => {
    return new Promise((resolve, reject) => {
        const img = new Image();
        img.crossOrigin = 'anonymous';
        
        img.onload = () => {
            try {
                const canvas = document.createElement('canvas');
                const ctx = canvas.getContext('2d');
                
                canvas.width = outputSize;
                canvas.height = outputSize;
                
                const size = Math.min(img.naturalWidth, img.naturalHeight);
                const sx = (img.naturalWidth - size) / 2;
                const sy = (img.naturalHeight - size) / 2;
                
                ctx.drawImage(img, sx, sy, size, size, 0, 0, outputSize, outputSize);
                
                canvas.toBlob((blob) => {
                    if (blob) {
                        const reader = new FileReader();
                        reader.onloadend = () => resolve(reader.result);
                        reader.onerror = () => reject(new Error('Failed to read blob'));
                        reader.readAsDataURL(blob);
                    } else {
                        reject(new Error('Failed to create blob'));
                    }
                }, 'image/jpeg', 0.95);
                
            } catch (error) {
                reject(error);
            }
        };
        
        img.onerror = () => reject(new Error('Failed to load image'));
        img.src = imageDataUrl;
    });
};

