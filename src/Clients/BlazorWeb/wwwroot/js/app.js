window.registerConnectivityListeners = (dotNetHelper) => {
    window.addEventListener('online', () => {
        dotNetHelper.invokeMethodAsync('UpdateStatus', true);
    });
    window.addEventListener('offline', () => {
        dotNetHelper.invokeMethodAsync('UpdateStatus', false);
    });
};

window.unregisterConnectivityListeners = () => {
    // Ideally we would keep the references to remove them, but for this PoC/MVP it's fine
};

window.sysvetDownloadFile = (fileName, contentType, base64) => {
    const link = document.createElement('a');
    link.download = fileName;
    link.href = `data:${contentType};base64,${base64}`;
    document.body.appendChild(link);
    link.click();
    link.remove();
};
