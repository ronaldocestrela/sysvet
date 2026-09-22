window.tutorPortalPush = {
    register: async function (vapidPublicKey, dotNetRef) {
        if (!('serviceWorker' in navigator) || !('PushManager' in window)) {
            return;
        }

        try {
            const registration = await navigator.serviceWorker.ready;
            const permission = await Notification.requestPermission();
            if (permission !== 'granted') {
                return;
            }

            const key = urlBase64ToUint8Array(vapidPublicKey);
            const subscription = await registration.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: key
            });

            const json = subscription.toJSON();
            await dotNetRef.invokeMethodAsync(
                'OnPushSubscriptionAsync',
                json.endpoint,
                json.keys.p256dh,
                json.keys.auth);
        } catch (e) {
            console.debug('Push registration skipped', e);
        }
    }
};

function urlBase64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
    const rawData = window.atob(base64);
    const outputArray = new Uint8Array(rawData.length);
    for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}
