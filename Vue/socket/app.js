const express = require('express');
const http = require('http');
const log = require('log-to-file');
const compression = require("compression");
const bodyParser = require("body-parser");
const cors = require("cors");
const swaggerUi = require('swagger-ui-express');
const subscriptionHandler = require('./subscriptionHandler');
const swaggerDocument = require('./swagger.json');
var fs = require('fs');

//Start Server
//Declare
const app = express();
const server = http.createServer(app);
const port = 3333;
const host = process.env.HOSTNAME || "0.0.0.0";
//Function
//End server

//Start Socket
//Declare
const { Server } = require("socket.io");
const io = new Server(server, {
    perMessageDeflate: false,
    cors: {
        origin: `*`,
        credentials: true,
        allowEIO3: true,
    },
    transports: ["polling", "websocket"],
});
//Method
app.get('/', (req, res) => {
    res.setHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    res.setHeader("Content-Security-Policy", "script-src 'self'");
    res.send('socket connect successful!');
});
app.get('/restart', function(req, res, next) {
    process.exit(1);
    //res.setHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    //res.send('socket connect successful!');
});
app.get('/logFile/:date', (req, res) => {
    try {
        let isFalse = false;
        if (req.params.date != null) {
            let dateFormat = req.params.date.replace(/^-/g, '');
            var arrDate = dateFormat.match(/^(\d{1,2})-(\d{1,2})-(\d{4})$/);
            if (arrDate != null) {
                //let listDate = dateFormat.split('-');
                var d = parseInt(arrDate[1]),
                    m = parseInt(arrDate[2], 10),
                    y = parseInt(arrDate[3], 10);
                let monthType1 = [1, 3, 5, 7, 8, 10, 12];
                let monthType2 = [4, 6, 9, 11];
                if (m >= 1 && m <= 12 && d >= 1) {
                    if ((monthType1.includes(m) && d <= 31) || (monthType2.includes(m) && d <= 30) || (m == 2 && ((y % 4 == 0 && (y % 400 == 0 || y % 100 != 0)) ? d <= 29 : d <= 28))) {
                        isFalse = true;
                        let filenamelog = "log-" + d.toString() + "-" + m.toString() + "-" + y.toString() + ".txt";
                        let logFile = "log/" + filenamelog;
                        var strhtml = fs.readFileSync(logFile, 'utf8');
                        res.setHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
                        res.send("Data from " + filenamelog);
                    }
                }
            }
        }
        if (!isFalse) {
            res.setHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            res.send("");
        }
    } catch (e) {
        res.setHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        res.send("");
    }
});
app.get('/log', (req, res) => {
    try {
        let logFile = "log/log-" + (new Date().toLocaleDateString('vi-VN')).replaceAll("/", "-") + ".txt";
        var strhtml = fs.readFileSync(logFile, 'utf8')
        var style = `<style>body{padding:20px;font-family:'arial'}table{border-collapse: collapse;width:100%}table,th,td,tr{border:1px solid #ccc}th{
        position: sticky;
        z-index: 1;
        background-color: #054498;
        color: #fff;
        font-weight: bold;
        top: 0;
    }th,td{padding:10px}tr.in{background:darkseagreen}tr.out{background:pink}tr.rin{background:#6fded9}</style>
    `;
        var html = "<table>";
        html += "<thead><tr>";
        html += "<th align='center' width=40>STT</th>";
        html += "<th align='center' width=120>Thời gian</th>";
        html += "<th align='center' width=200>Tài khoản</th>";
        html += "<th align='left'>Nội dung</th>";
        html += "</tr></thead>";
        html += "<tbody>";
        var logs = strhtml.split("\r\n");
        let stt = 1;
        logs.forEach(function(tr) {
            var arrline = tr.split("->");
            if (arrline.length > 1) {
                var name = "";
                var nd = arrline[1];
                if (arrline[1].includes("joined")) {
                    name = arrline[1].split("joined")[0];
                    html += "<tr class='in'>";
                } else if (arrline[1].includes("disconnecting")) {
                    name = arrline[1].split("disconnecting")[0];
                    html += "<tr class='out'>";
                } else if (arrline[1].includes("reconnect")) {
                    name = arrline[1].split("reconnect")[0];
                    html += "<tr class='rin'>";
                } else {
                    html += "<tr>";
                }
                html += "<td align='center'>" + stt + "</td>";
                html += "<td align='center'>" + arrline[0] + "</td>";
                html += "<td align='center'>" + name + "</td>";
                html += "<td align='left'>" + arrline[1] + "</td>";
                html += "</tr>";
                stt++;
            }
        })
        html += "</tbody>";
        html += "</table>";
        res.setHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        res.header('Content-Type', 'text/html').send("<html><head><meta charset=\"UTF-8\">" + style + "</head><body>" + html + "</body></html>");
    } catch (e) {
        res.setHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        res.send("");
    }
});
//Function
var users = [];
io.use((socket, next) => {
    const user = socket.handshake.auth;
    if (!user || !user.user_id || (user.user_id !== "chatbot" && (!user.islogin || !user.token_id))) {
        return next(new Error("User Error!"));
    }
    socket.user = user;
    let obj = {
        socket_id: socket.id,
        token_id: socket.user.token_id,
        user_id: socket.user.user_id,
        user_key: socket.user.user_key,
        full_name: socket.user.full_name,
        avatar: socket.user.avatar,
        time: new Date(),
        type: socket.user.type
    }
    users.push(obj);
    let logFile = "log/log-" + (new Date().toLocaleDateString('vi-VN')).replaceAll("/", "-") + ".txt";
    log(socket.user.full_name + ' (' + socket.user.user_id + ' reconnect ' + socket.id + ') [' + users.length + '] ' + (socket.user.type || ''), logFile, '\r\n');
    next();
});
io.on('connection', (socket) => {
    users = [];
    for (let [id, socket] of io.of("/").sockets) {
        users.push({
            socket_id: id,
            token_id: socket.user.token_id,
            user_id: socket.user.user_id,
            user_key: socket.user.user_key,
            full_name: socket.user.full_name,
            avatar: socket.user.avatar,
            time: new Date(),
            type: socket.user.type
        })
    }

    socket.emit("getusers", users);

    socket.broadcast.emit('userconnected', {
        socket_id: socket.id,
        token_id: socket.user.token_id,
        user_id: socket.user.user_id,
        user_key: socket.user.user_key,
        full_name: socket.user.full_name,
        avatar: socket.user.avatar,
        time: new Date(),
        type: socket.user.type
    });

    socket.on('disconnect', () => {
        users = users.filter(x => x.socket_id != socket.id);
        socket.broadcast.emit("userdisconnected", socket.id);
        let logFile = "log/log-" + (new Date().toLocaleDateString('vi-VN')).replaceAll("/", "-") + ".txt";
        log(socket.user.full_name + ' (' + socket.user.user_id + ' disconnect ' + socket.id + ') ', logFile, '\r\n');
    });

    socket.on('sendData', (data) => {
        var event = data.event || 'getData';
        if (data.uids != null) {
            var send_users = users.filter((x) => data.uids.includes(x.user_id));
            send_users.forEach((user) => {
                socket.to(user.socket_id).emit(event, data);

                let logFile = "log/log-" + (new Date().toLocaleDateString('vi-VN')).replaceAll("/", "-") + ".txt";
                log(socket.user.full_name + ' (user_id: \"' + socket.user.user_id + '\" - socket_id: \"' + socket.id + '\") send message to ' + user.full_name + ' (user_id: \"' + user.user_id + '\" - socket_id: \"' + user.socket_id + '\") ', logFile, '\r\n');
            });
        } else {
            if (data.event == "reloadDataTivi" || data.event == "updateAPK" || data.event == "controlTivi") {
                socket.broadcast.emit(event, data);
            } else if (data.event == "goScreenshot") {
                socket.to(data.socket_nhan_id).emit(event, data);
            } else {
                socket.emit(event, data);
            }
        }

        //push html
        if(event === "sendNotify"){
            fetch((`http://${host}:${port}/`).concat(`sendnotification`), {
                credentials: "omit",
                headers: {
                    "content-type": "application/json;charset=UTF-8",
                    "sec-fetch-mode": "cors",
                },
                body: JSON.stringify({
                    uids: data.uids,
                    options: {
                      title: (data.title || null),
                      text: (data.contents || null),
                      image: (data.image || null),
                      tag: (data.tag || null),
                      url: (data.url || null),
                    },
                  }),
                method: "POST",
                mode: "cors"
            }).then((result) => {
                let logFile = "log/log-" + (new Date().toLocaleDateString('vi-VN')).replaceAll("/", "-") + ".txt";
                log('push html: result('+ result + ')', logFile, '\r\n');
            });
        }
    });

    socket.on('listTivi', (data) => {
        var dataUserTV = users.filter(x => x.type == "Tivi");
        socket.emit("listTiviSocket", dataUserTV);
        let logFile = "log/log-" + (new Date().toLocaleDateString('vi-VN')).replaceAll("/", "-") + ".txt";
        log(socket.user.full_name + ' (dataUserTV: ' + dataUserTV.length + ', dataTV: ' + data.length + ')', logFile, '\r\n');
    });
});
//End Socket

//Start Notification
//Function
app.use(cors());
app.use('/api-docs', swaggerUi.serve, swaggerUi.setup(swaggerDocument));
app.use(compression());
app.use(bodyParser.urlencoded({ extended: true }));
app.use(bodyParser.json());

//Method
app.post("/subscription", subscriptionHandler.handlePushNotificationSubscription);
app.post("/sendnotification", subscriptionHandler.sendPushNotification);

module.exports = app;
//End Notification

//Listen
server.listen(port, () => {
    console.log(`Node.js API server is listening on http://${host}:${port}/`);
});