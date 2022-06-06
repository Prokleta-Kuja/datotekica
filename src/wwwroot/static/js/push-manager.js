var serviceWorker = '/js/push-handler.js';
var registration;

export async function checkAndUpdateWorker() {
    // Let's check if the browser supports notifications
    if (!("Notification" in window)) {
        return;
    }

    // Check if push messaging is supported
    if (!('PushManager' in window)) {
        return;
    }

    if (!('serviceWorker' in navigator)) {
        return false;
    }

    registration = await navigator.serviceWorker.register(serviceWorker);
    registration.update();

    // Are Notifications supported in the service worker?
    if (!(registration.showNotification)) {
        return;
    }

    return Notification.permission;
}

export async function getSubscription() {
    let subscription = await registration.pushManager.getSubscription();
    if (subscription)
        return getWebPushSubscriptionObject(subscription);
    else
        return;
}

export async function unsubscribe() {
    let subscription = await registration.pushManager.getSubscription();
    if (subscription)
        await subscription.unsubscribe();
}

export async function subscribe(vapidKey) {
    let permission = await Notification.requestPermission();

    if (permission !== 'granted') {
        return;
    }

    let subscription = await registration.pushManager.getSubscription();

    if (subscription) {
        return getWebPushSubscriptionObject(subscription);
    }

    var subscribeParams = { userVisibleOnly: true };
    subscribeParams.applicationServerKey = urlB64ToUint8Array(vapidKey);

    subscription = await registration.pushManager.subscribe(subscribeParams);

    return getWebPushSubscriptionObject(subscription);
}

function getWebPushSubscriptionObject(subscription) {
    return {
        endpoint: subscription.endpoint,
        p256DH: base64Encode(subscription.getKey('p256dh')),
        auth: base64Encode(subscription.getKey('auth'))
    };
}

function urlB64ToUint8Array(base64String) {
    var padding = '='.repeat((4 - base64String.length % 4) % 4);
    var base64 = (base64String + padding)
        .replace(/\-/g, '+')
        .replace(/_/g, '/');

    var rawData = window.atob(base64);
    var outputArray = new Uint8Array(rawData.length);

    for (var i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}

function base64Encode(arrayBuffer) {
    return btoa(String.fromCharCode.apply(null, new Uint8Array(arrayBuffer)));
}

export default {checkAndUpdateWorker, getSubscription, unsubscribe, subscribe };