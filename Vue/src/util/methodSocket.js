import { store } from "../store/store.js";
const headers = {
    "content-type": "application/json;charset=UTF-8",
    "sec-fetch-mode": "cors",
    Authorization: `Bearer ${store.getters.token}`
}

function checkUrl(url) {
    var last = url.split("").at(-1);
    if (last !== "/") {
        return url.concat("/");
    }
    return url;
}

export const socketMethod = {
    post: function(e, n) {
        return fetch(checkUrl(socketURL).concat(e), {
            credentials: "omit",
            headers: headers,
            body: JSON.stringify(n),
            method: "POST",
            mode: "cors"
        }).then((result) => {
            return result.json();
        }).then((result) => {
            return result;
        });
    },
    get: function(e) {
        return fetch(checkUrl(socketURL).concat(e), {
            credentials: "omit",
            headers: headers,
            method: "GET",
            mode: "cors"
        }).then((result) => {
            return result;
        }).then((result) => {
            return result;
        });
    }
}