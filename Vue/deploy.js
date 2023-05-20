const FtpDeploy = require("ftp-deploy");
const ftpDeploy = new FtpDeploy();

const config = {
    user: "soe",
    password: "#@sPlus2023$#@",
    host: "103.226.251.47",
    port: 21,
    localRoot: __dirname + "/dist",
    remoteRoot: "\\SOEVTC\\vue",
    include: ['*', '**/*'],
    // exclude: [
    //     "dist/**",
    // ],
    // delete ALL existing files at destination before uploading, if true
    deleteRemote: false,
    // Passive mode is forced (EPSV command is not sent)
    forcePasv: true,
    // use sftp or ftp
    sftp: false,
};

ftpDeploy
    .deploy(config)
    .then((res) => console.log("finished:", res))
    .catch((err) => console.log(err));
