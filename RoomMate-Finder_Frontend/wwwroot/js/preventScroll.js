// Prevent page scroll on crop element and handle zoom via Blazor
window.preventScrollOnElement = function(elementId, dotNetRef) {
    const element = document.getElementById(elementId);
    if (!element) return;
    
    if (element._wheelHandler) {
        element.removeEventListener('wheel', element._wheelHandler);
    }

    const wheelHandler = function(e) {
        e.preventDefault();
        e.stopPropagation();
        
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync('HandleWheelZoom', e.deltaY < 0);
        }
        
        return false;
    };
    
    element.addEventListener('wheel', wheelHandler, { passive: false });
    element._wheelHandler = wheelHandler;
};

window.allowScrollOnElement = function(elementId) {
    const element = document.getElementById(elementId);
    if (!element?._wheelHandler) return;
    
    element.removeEventListener('wheel', element._wheelHandler);
    delete element._wheelHandler;
};

