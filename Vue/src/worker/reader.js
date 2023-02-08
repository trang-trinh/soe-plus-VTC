self.onmessage = function (om) {
    switch (om.data.type) {
        case "file":
            readerFile(om);
            break;
        case "translate":
            translate(om.data);
            break;
        default:
            break;
    }

}
function translate(data) {
    switch (data.tran) {
        case "HV":
            translateHV(data);
            break;
        case "VP":
            translateVP(data);
            break;
        default:
            break;
    }
};
function translateHV(data) {
    let dist = data.disthv;
    let tt = data.tt;
    let wordTS = [];
    let words = [];
    for (let i = 0; i < tt.length; i++) {
        let w = tt[i];
        let pi = 0;
        if (i > 0) {
            pi = wordTS[i - 1].start + wordTS[i - 1].end;
        }
        let obj = {};
        if (dist[w]) {
            let hw = w.replaceAll(w, dist[w]).replaceAll("\r", " ");
            obj = { starttt: i, endtt: 1, wordtt: w, start: i + pi, end: hw.length, word: hw };
            words.push(hw);
        } else {
            words.push(w);
            obj = { starttt: i, endtt: 1, wordtt: w, start: i + pi, end: w.length, word: w };
        }
        wordTS.push(obj);
    }
    console.log(wordTS);
    self.postMessage({ tran: data.tran, type: "translate", data: words.join("").replaceAll("\n", "<br>") });
}
function translateRow(distname, disthv, distvp, wordTS, words, ch) {
    if (distname[ch]) {
        words.push(distname[ch]);
    } else if (distvp[ch]) {
        words.push(distvp[ch]);
    } else {
        let end = ch.length > 5 ? 5 : ch.length;
        let w = ch.substring(0, end);
        if (isChinese(w)) {
            console.log(w);
            while (!distname[w] && !distvp[w] && end > 1) {
                end -= 1;
            }
            w = ch.substring(0, end);
            if (end == 1) {
                w = disthv[w]||w;
            } else if (distname[w]) {
                w = distname[w];
            } else {
                w = distvp[w];
            }
            if(w)
            words.push(w);
            ch = ch.substring(end);
            if (ch)
                translateRow(distname, disthv, distvp, wordTS, words, ch);
        } else {
            words.push(w);
        }
    }
}
function translateVP(data) {
    let distname = data.distname;
    let disthv = data.disthv;
    let distvp = data.distvp;
    let tt = data.tt;
    let wordTS = [];
    let words = [];
    tt.split("\n").forEach(r => {
        let chs = r.split(/。|\s|，|、|；|：|？|！|“|”|‘|’|（|）|……|——|—|《|》|〈|〉|·|﹒/);
        for (let i = 0; i < chs.length; i++) {
            translateRow(distname, disthv, distvp, wordTS, words, chs[i]);
        }
        words.push("\n");
    });
    self.postMessage({ tran: data.tran, type: "translate", data: words.join("").replaceAll("\n", "<br>") });
}
function isChinese(str) {
    const REGEX_CHINESE = /[\u3040-\u30ff\u3400-\u4dbf\u4e00-\u9fff\uf900-\ufaff\uff66-\uff9f]/;
    return str.match(REGEX_CHINESE);
}
function readerFile(om) {
    const reader = new FileReader();
    reader.onload = e => {
        let data = e.target.result.split("\n");
        let obj = {};
        data.forEach(r => {
            let disc = r.split("=");
            if (disc.length > 1) {
                obj[disc[0]] = disc[1].split("/")[0];
            }
        });
        self.postMessage({ data: obj, name: om.data.file.name, type: "file" })
    };
    reader.readAsText(om.data.file);
}