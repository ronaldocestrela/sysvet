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
