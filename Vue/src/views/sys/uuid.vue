<script setup>
import { customAlphabet } from "nanoid";
function renderKey() {
  const nanoid = customAlphabet("1234567890abcdefghijklmnopqrstuvwxyz".toUpperCase(), 13);
  let csv = "";
  let arr = [];
  for (let i = 1; i <= 100000; i++) {
    let key = "WW3-2022-";
    key += nanoid();
    arr.push(key);
    csv += key + "\n";
  }
  console.log([...new Set(arr)].length);
  downloadFile(csv, "key.csv");
  arr = [];
  csv = "";
}
function downloadFile(data, fileName) {
  var csvData = data;
  var blob = new Blob([csvData], {
    type: "application/csv;charset=utf-8;",
  });

  if (window.navigator.msSaveBlob) {
    // FOR IE BROWSER
    navigator.msSaveBlob(blob, fileName);
  } else {
    // FOR OTHER BROWSERS
    var link = document.createElement("a");
    var csvUrl = URL.createObjectURL(blob);
    link.href = csvUrl;
    link.style = "visibility:hidden";
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }
}
renderKey();
</script>
