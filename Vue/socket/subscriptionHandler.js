const subscriptions = {};
const webpush = require("web-push");

const vapidKeys = {
    publicKey: 'BClGBpl2qTJtZVVSmksuLtbWUTN-JdA8SSafi1jKxqFc0aV4ZhZ50ecYzI5DK1nob6jTZHur25nX9A1Q1HuyXb4',
    privateKey: 'mdwA4YBj8l1AUQ1BIBI8WbLu47icH5Qmcrjez47-Q24'
};
webpush.setVapidDetails("mailto:example@yourdomain.org", vapidKeys.publicKey, vapidKeys.privateKey);

function handlePushNotificationSubscription(req, res) {
    subscriptions[req.body.id] = req.body.subscrition;
    res.setHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    res.status(201).json({});
}

function sendPushNotification(req, res) {
    if (req.body.uids != null) {
        var send_users = Object.entries(subscriptions).filter(([id, subscription]) => req.body.uids.includes(id));
        send_users.forEach(([id, subscription]) => {
            webpush
                .sendNotification(subscription, JSON.stringify(req.body.options))
                .catch(err => {
                    console.log(err);
                });
        });
    } else {
        Object.entries(subscriptions).forEach(([id, subscription]) => {
            webpush
                .sendNotification(subscription, JSON.stringify(req.body.options))
                .catch(err => {
                    console.log(err);
                });
        });
    }
    res.setHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    res.status(201).json({});
}

module.exports = { handlePushNotificationSubscription, sendPushNotification };