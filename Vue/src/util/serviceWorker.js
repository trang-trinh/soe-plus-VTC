function receivePushNotification(event) {
    const { image, tag, url, title, text } = event.data.json();

    const options = {
        data: url,
        body: text,
        icon: image,
        vibrate: [200, 100, 200],
        tag: tag,
        image: image,
        badge: "https://project.soe.vn/favicon.ico",
        actions: [{ action: "Detail", title: "View", icon: "https://via.placeholder.com/128/ff0000" }]
    };
    event.waitUntil(self.registration.showNotification(title, options));
}

function openPushNotification(event) {
    event.notification.close();
    event.waitUntil(clients.openWindow(event.notification.data));
}

self.addEventListener("push", receivePushNotification);
self.addEventListener("notificationclick", openPushNotification);