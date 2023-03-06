<script>
import { ref, inject, onMounted, computed } from "vue";
import Editor from "primevue/editor";

const download = (filename, text) => {
  var element = document.createElement("a");
  element.setAttribute(
    "href",
    "data:text/plain;charset=utf-8," + encodeURIComponent(text)
  );
  element.setAttribute("download", filename);
  element.style.display = "none";
  document.body.appendChild(element);
  element.click();
  document.body.removeChild(element);
};

function genKey() {
  function _p8(s) {
    var p = (Math.random().toString(16) + "000000000").substring(2, 8);
    return s ? "-" + p.substring(0, 4) + "-" + p.substring(4, 4) : p;
  }
  return _p8() + _p8(true) + _p8(true) + _p8();
}

const setAttributes = (el, attrs) => {
  for (var key in attrs) {
    el.setAttribute(key, attrs[key]);
  }
};

export default {
  components: {
    Editor,
  },
  setup() {
    const cryoptojs = inject("cryptojs");
    const store = inject("store");
    const config = {
      headers: {
        Authorization: `Bearer ${store.getters.token}`,
      },
    };

    const pureColor = ref("#000000");
    const gradientColor = ref(
      "linear-gradient(0deg, rgba(0, 0, 0, 1) 0%, rgba(0, 0, 0, 1) 100%)"
    );
    let dochtml;
    const isHasHTML = ref(false);
    const isstart = ref(false);
    const isend = ref(false);
    const spans = ref([]);
    const axios = inject("axios");
    const api = "http://10.211.55.20:6868/";
    const elementSpan = ref({});
    let today = new Date();
    let datausers = [
      {
        "Tên công ty": "Công ty cổ phần phần mềm Phương Đông",
        Số: "10121988",
        "Giới tính": "Nam",
        "Sinh ngày": "10",
        "Tháng sinh": "12",
        "Năm sinh": "1988",
        "Nơi sinh": "Trạm y tế xã Đặng Xá, Gia Lâm, Hà Nội",
        Ảnh: '<img width="129" height="160" alt="" style="object-fit: cover;margin-top:-0.82pt; margin-left:14.63pt; -aw-left-pos:15.75pt; -aw-rel-hpos:column; -aw-rel-vpos:paragraph; -aw-top-pos:0.3pt; -aw-wrap-type:none; position:absolute" src="https://apiv2.soe.vn//Portals/Users/44311433_1860466920669366_6024507152440229888_21013107585291.jpg"/>',
        "Điện thoại": "0987729288",
        "Dân tộc": "Kinh",
        "Tôn giáo": "Không",
        "Địa điểm": "Hà Nội",
        "Trình độ văn hoá": "Đại học",
        "Khen thưởng": "Tấm gương người tốt việc tốt",
        "Sở trường": "Công nghệ",
        Ngày: (today.getDay() < 10 ? "0" : "") + today.getDay(),
        Tháng: (today.getMonth() < 10 ? "0" : "") + today.getMonth(),
        Năm: today.getFullYear(),
        "Họ và Tên": "Hoàng Đức Công",
        "Ngày sinh": "10/12/1988",
        CMND: "001088032053",
        "Ngày cấp": "26/03/2019",
        "Nơi cấp": "Công an thành phố Hà Nội",
        "Ngày vào đoàn": "26/03/2003",
        "Địa điểm đoàn": "Trường tiểu học Đặng Xá",
        "Ngày vào đảng": "26/03/2018",
        "Địa điểm đảng": "Đại học BK Hà Nội",
        "Hộ khẩu TT": "Số 18/16/61 , Đường Ỷ Lan, Đặng Xá, Gia Lâm, Hà Nội",
        "Chỗ ở hiện tại": "Số 18/16/61 , Đường Ỷ Lan, Đặng Xá, Gia Lâm, Hà Nội",
        "Chức vụ": "Giám đốc kỹ thuật",
        "Quyền và nghĩa vụ":
          "Chịu trách nhiệm về các sản phẩm phát triển của công ty",
        "Chữ ký giám đốc":
          "<img height=96 src='https://cdn.fast.vn/tmp/20201020155613-9.PNG'/>",
        "Xác nhận": "Thông tin kê khai đúng với hồ sơ",
        "Tên cha": "Hoàng Văn Bá",
        "Năm sinh cha": "1957",
        "Nghề nghiệp cha": "Thương binh",
        "Cơ quan cha": "Hà Nội",
        "Chỗ ở cha": "Số 18/16/61 , Đường Ỷ Lan, Đặng Xá, Gia Lâm, Hà Nội",
        "Tên mẹ": "Nguyễn Thị Phượng",
        "Năm sinh mẹ": "1957",
        "Nghề nghiệp mẹ": "Giáo viên",
        "Cơ quan mẹ": "Hà Nội",
        "Chỗ ở mẹ": "Số 18/16/61 , Đường Ỷ Lan, Đặng Xá, Gia Lâm, Hà Nội",
        "Anh chị em": [
          {
            STT: 3,
            "Họ tên anh em": "Hoàng Thị Lan Anh",
            "Năm sinh anh em": "1983",
            "Nghề nghiệp anh em": "Điều dưỡng",
            "Cơ quan anh em": "Bệnh viện Xanh Pôn Hà Nội",
          },
          {
            STT: 4,
            "Họ tên anh em": "Hoàng Thị Minh Tú",
            "Năm sinh anh em": "1986",
            "Nghề nghiệp anh em": "Nhân viên Văn Phòng",
            "Cơ quan anh em": "Hàng không Quốc gia Malaysia",
          },
        ],
        "Đào tạo": [
          {
            "Thời gian": "2006-2011",
            Trường: "Đại học Bách Khoa Hà Nội",
            "Ngành học": "Cơ tin",
            "Hình thức": "Chính Quy",
            "Văn bằng": "Đại học",
          },
          {
            "Thời gian": "2010-2012",
            Trường: "FPT Aptech",
            "Ngành học": "CNTT",
            "Hình thức": "Liên thông",
            "Văn bằng": "Kỹ sư",
          },
        ],
        "Công tác": [
          {
            "Thời gian": "2010-2011",
            "Đơn vị": "Công ty cổ phần ITNT",
            "Chức vụ": "Nhân viên",
          },
          {
            "Thời gian": "2012-2014",
            "Đơn vị": "Công ty cổ phần phần mềm Phương Đông",
            "Chức vụ": "Nhân viên",
          },
          {
            "Thời gian": "2014-2016",
            "Đơn vị": "Công ty cổ phần phần mềm Phương Đông",
            "Chức vụ": "Trưởng phòng kỹ thuật",
          },
          {
            "Thời gian": "2016-2023",
            "Đơn vị": "Công ty cổ phần phần mềm Phương Đông",
            "Chức vụ": "Giám đốc kỹ thuật",
          },
        ],
      },
      {
        "Tên công ty": "Công ty cổ phần phần mềm Phương Đông",
        Số: "02032023",
        "Địa điểm": "Hà Nội",
        Ngày: (today.getDay() < 10 ? "0" : "") + today.getDay(),
        Tháng: (today.getMonth() < 10 ? "0" : "") + today.getMonth(),
        Năm: today.getFullYear(),
        "Họ và Tên": "Nguyễn Thành Chung",
        "Giới tính": "Nam",
        "Sinh ngày": "01",
        "Tháng sinh": "09",
        "Năm sinh": "1998",
        "Nơi sinh": "Trạm y tế Cẩm Phả, Quảng Ninh",
        Ảnh: '<img width="129" height="160" alt="" style="object-fit: cover;margin-top:-0.82pt; margin-left:14.63pt; -aw-left-pos:15.75pt; -aw-rel-hpos:column; -aw-rel-vpos:paragraph; -aw-top-pos:0.3pt; -aw-wrap-type:none; position:absolute" src="https://sfile.soe.vn//Portals/CBA146D4A77C43259CC8231885703B78/NhanSu/Thumb/%E1%BA%A3nh%20s%E1%BA%AFp%20up%20av21110210285375.jpg"/>',
        "Ngày sinh": "01/09/1998",
        CMND: "00109923456",
        "Ngày cấp": "20/02/2020",
        "Nơi cấp": "Công an tỉnh Quảng Ninh",
        "Dân tộc": "Kinh",
        "Tôn giáo": "Không",
        "Hộ khẩu TT": "Cẩm Phả, Quảng Ninh",
        "Đào tạo": [
          {
            "Thời gian": "2018-2022",
            Trường: "Học viện kỹ thuật quân sự",
            "Ngành học": "CNTT",
            "Hình thức": "Chính Quy",
            "Văn bằng": "Đại học",
          },
        ],
        "Công tác": [
          {
            "Thời gian": "2019-2021",
            "Đơn vị": "Công ty cổ phần phần mềm Phương Đông",
            "Chức vụ": "Nhân viên",
          },
          {
            "Thời gian": "2021-2023",
            "Đơn vị": "Công ty cổ phần phần mềm Phương Đông",
            "Chức vụ": "Trưởng phòng kỹ thuật",
          },
        ],
        "Chỗ ở hiện tại": "Đình Thôn, Mỹ Đình, Từ Liêm, Hà Nội",
        "Chức vụ": "Trưởng phòng kỹ thuật",
        "Quyền và nghĩa vụ":
          "Chịu trách nhiệm về các sản phẩm phát triển của công ty",
        "Chữ ký giám đốc":
          "<img height=96 src='https://tindep.com/wp-content/uploads/2019/04/mau-chu-ky-ten-huy-dep-nhat-1.png'/>",
      },
    ];
    datausers.forEach((u) => {
      Object.keys(u).forEach((p) => {
        if (isNaN(p) && u[p]) {
          u[p.toUpperCase()] = u[p].toString().toUpperCase();
          u[p.toLowerCase()] = u[p].toString().toLowerCase();
        }
      });
    });
    const datakeys = ref(Object.keys(datausers[0]));
    let mapdatakeys = [];
    function removeAccents(str) {
      var AccentsMap = [
        "aàảãáạăằẳẵắặâầẩẫấậ",
        "AÀẢÃÁẠĂẰẲẴẮẶÂẦẨẪẤẬ",
        "dđ",
        "DĐ",
        "eèẻẽéẹêềểễếệ",
        "EÈẺẼÉẸÊỀỂỄẾỆ",
        "iìỉĩíị",
        "IÌỈĨÍỊ",
        "oòỏõóọôồổỗốộơờởỡớợ",
        "OÒỎÕÓỌÔỒỔỖỐỘƠỜỞỠỚỢ",
        "uùủũúụưừửữứự",
        "UÙỦŨÚỤƯỪỬỮỨỰ",
        "yỳỷỹýỵ",
        "YỲỶỸÝỴ",
      ];
      for (var i = 0; i < AccentsMap.length; i++) {
        var re = new RegExp("[" + AccentsMap[i].substr(1) + "]", "g");
        var char = AccentsMap[i][0];
        str = str.replace(re, char);
      }
      return str.toLocaleLowerCase().replaceAll(" ", "");
    }
    datakeys.value.forEach((element) => {
      mapdatakeys.push({ name: element, id: removeAccents(element) });
    });
    let users = [
      {
        id: "cong",
        name: "Công",
        fname: "Hoàng Đức Công",
        severity: "success",
        img: "https://apiv2.soe.vn//Portals/Users/44311433_1860466920669366_6024507152440229888_21013107585291.jpg",
      },
      {
        id: "chung",
        name: "Chung",
        fname: "Nguyễn Thành Chung",
        severity: "info",
        img: "https://sfile.soe.vn//Portals/CBA146D4A77C43259CC8231885703B78/NhanSu/Thumb/%E1%BA%A3nh%20s%E1%BA%AFp%20up%20av21110210285375.jpg",
      },
    ];
    const user = ref(users[0]);
    const initDocHTML = (html) => {
      html = html.replaceAll("&nbsp;", " ");
      html = html.replace(/[\.…]{2,}/gim, "{{}}");
      dochtml = document.getElementById("dochtml");
      // iframe.innerHTML= html;
      dochtml.contentWindow.document.open();
      dochtml.contentWindow.document.write(html);
      dochtml.contentWindow.document.close();
      isHasHTML.value = true;
      spans.value = [];
      eventDoc();
    };
    const uid = function () {
      return Date.now().toString(36) + Math.random().toString(36).substr(2);
    };
    const parseHTML = (html) => {
      var doc = new DOMParser().parseFromString(html, "text/html");
      return doc.querySelector("span").innerHTML;
    };
    let tableElement;
    function addClickSpan(e) {
      if (e.target.innerHTML.trim() == "&nbsp;") {
        e.target.innerHTML = "";
      }
      let el = spans.value.find((x) => x.id == e.target.id);
      if (el) elementSpan.value = el;
      e.stopPropagation();
    }
    function addBlurSpan(e) {
      let span = spans.value.find((x) => x.id == e.target.id);
      if (span) {
        let html = span.element;
        if (span.historys.length > 0) {
          html = span.historys[span.historys.length - 1].element.trim();
        }
        if (e.target.innerHTML.trim() != parseHTML(html).trim()) {
          e.target.classList.add("os-span-edit-os");
          span.version++;
          let vesion = {
            version: span.version,
            user: user.value,
            element: e.target.outerHTML,
            date: new Date(),
          };
          span.historys.push(vesion);
          reUsers.value.forEach((u) => {
            u.badge = spans.value.filter(
              (x) => x.historys.filter((h) => h.user.id == u.id) > 0
            );
          });
        }
      }
    }
    const eventDoc = () => {
      dochtml.querySelectorAll("table").forEach((element) => {
        element.addEventListener("click", function (e) {
          tableElement = element;
          displayTable.value = true;
          e.stopPropagation();
        });
      });
      dochtml.querySelectorAll("td").forEach((element) => {
        element.style.setProperty("vertical-align", "middle");
        element.style.setProperty("align-items", "center");
      });
      let groups = [];
      dochtml.querySelectorAll("p>span").forEach((element) => {
        element.setAttribute("contenteditable", true);
        if (
          element.closest("td") &&
          element.closest("td").querySelectorAll("span").length == 1
        ) {
          element.classList.add("maxw");
        }
        let hasedid = false;
        if (element.className && element.className.match(/os-span-edit-os/)) {
          hasedid = true;
          element.id = element.className.split("os-span-edit-os")[0];
          element.className = element.className.replace(
            "os-span-edit-os",
            " os-span-edit-os"
          );
        }
        const clone = element.cloneNode(true);
        let osid = uid();
        if (!clone.id) {
          clone.id = osid;
        } else if (hasedid) {
          osid = clone.id;
        }
        spans.value.push({
          element: clone.outerHTML,
          id: osid,
          version: 1,
          historys: [],
        });
        if (!element.id) {
          element.id = osid;
          if (!hasedid) element.classList.add(osid);
        }
        element.addEventListener("click", addClickSpan);
        element.addEventListener("blur", addBlurSpan);
        //Check group
        let group = element.className.split("os-span-edit-osgroup-");
        if (group.length > 1) {
          let groupdataclass = group[1].trim();
          if (groups.indexOf(groupdataclass) == -1) {
            groups.push(groupdataclass);
          }
          element.parentElement.classList.add(groupdataclass);
        }
      });
      //set Groups
      groups.forEach((gr) => {
        let wrapper = document.querySelector(".for-data" + gr);
        if (wrapper == null) {
          wrapper = document.createElement("div");
          wrapper.classList.add("for-data" + gr);
          dochtml.querySelectorAll("p." + gr).forEach((p) => {
            wrap(p, wrapper);
          });
        }
        wrapper.addEventListener("click", function (e) {
          tableElement = wrapper;
          displayTable.value = true;
          e.stopPropagation();
        });
      });
    };
    let spanstart;
    function wrap(el, wrapper) {
      el.parentNode.insertBefore(wrapper, el);
      wrapper.appendChild(el);
    }
    const setSelect = (f) => {
      tableElement = null;
      if (f) {
        isstart.value = !isstart.value;
        let span = document.getElementById(elementSpan.value.id);
        span.classList.toggle("os-span-edit-os");
        if (isstart.value) {
          spanstart = span;
        } else {
          spanstart = null;
        }
      } else {
        isend.value = !isend.value;
        let span = document.getElementById(elementSpan.value.id);
        span.classList.toggle("os-span-edit-os");
        if (isend.value) {
          var childs = [];
          childs.push(spanstart.closest("p"));
          let nexspan = spanstart.nextElementSibling;
          if (nexspan != null) {
            nexspan.classList.add("os-span-edit-os");
            while (nexspan && nexspan.id != span.id) {
              if (childs.indexOf(nexspan.closest("p")) == -1) {
                childs.push(nexspan.closest("p"));
              }
              nexspan.classList.add("os-span-edit-os");
              if (nexspan.nextElementSibling == null) {
                nexspan = nexspan.parentElement.nextElementSibling.children[0];
              } else {
                nexspan = nexspan.nextElementSibling;
              }
            }
          }
          if (childs.indexOf(span.closest("p")) == -1) {
            childs.push(span.closest("p"));
          }
          //
          var wrapper = document.createElement("div");
          wrapper.classList.add("for-span-table");
          childs.forEach((chi) => {
            wrap(chi, wrapper);
          });
          displayTable.value = true;
        }
      }
    };
    const displayTable = ref(false);
    const mdData = ref("");
    const saveConfigTable = () => {
      if (tableElement != null) {
        tableElement.className = "for-data" + mdData.value;
      } else {
        let gclass = "for-data" + mdData.value;
        let div = document.querySelector(".for-span-table");
        div.className = gclass;
        div.querySelectorAll("p>span").forEach((p) => {
          p.classList.add("group-" + mdData.value);
        });
      }
      isstart.value = false;
      isend.value = false;
      displayTable.value = false;
    };
    const itemusers = ref([]);
    const showLoadding = ref(false);
    const myJsonUploader = (event) => {
      showLoadding.value = true;
      let file = event.files[0];
      var reader = new FileReader();
      reader.onload = function () {
        var text = reader.result;
        spans.value = JSON.parse(text);
        reUsers.value.forEach((u) => {
          u.badge = spans.value.filter(
            (x) => x.historys.filter((h) => h.user.id == u.id).length > 0
          ).length;
        });
        showLoadding.value = false;
      };
      reader.readAsText(file);
    };
    const myDocUploader = async (event) => {
      showLoadding.value = true;
      let formData = new FormData();
      formData.append("doc", event.files[0]);
      try {
        // const response = await fetch(api + "api/tool/genderHtml", {
        //   method: "POST",
        //   body: formData,
        // });

        const response = await axios
          .post(api + "api/tool/genderHtml", formData, config)
          .then((response) => {
            var data = response.data;
            tempHTML = "";
            initDocHTML(data.html);
            showLoadding.value = false;
          });
      } catch (e) {
        console.log(e);
      }
    };
    let tempHTML = "";
    const downloadFile = async (f) => {
      if (tempHTML != "") {
        dochtml.innerHTML = tempHTML;
      } else {
        tempHTML = dochtml.innerHTML;
      }
      let data = datausers.find((x) => x["Họ và Tên"] == chonuser.fname);
      //Table
      document.querySelectorAll('[class^="for-data"]').forEach((div) => {
        let dtclass = div.className.replace("for-data", "");
        div.className = dtclass;
        if (dtclass) {
          if (div.tagName.toLocaleLowerCase() == "table") {
            div = div.querySelectorAll("tbody>tr")[1];
          }
          let ok = mapdatakeys.find((x) => x.id == dtclass);
          (data[dtclass] || data[ok.name] || []).forEach((r) => {
            let divclone = div.cloneNode(true);
            let dhtml = divclone.innerHTML;
            Object.keys(r).forEach((k) => {
              dhtml = dhtml.replaceAll(`{{${k}}}`, r[k.trim()] || "");
            });
            divclone.innerHTML = dhtml;
            div.parentNode.insertBefore(divclone, div);
          });
          div.remove();
        }
      });
      let html = dochtml.innerHTML;
      datakeys.value.forEach((k) => {
        html = html.replaceAll(`{{${k}}}`, data[k.trim()] || "");
      });
      html = html.replaceAll("{{}}", "");
      html = html.replaceAll("os-span-edit-os", "");
      dochtml.innerHTML = html;
      html = dochtml.innerHTML;
      eventDoc();
      if (f) {
        return false;
      }
      showLoadding.value = true;
      try {
        const axResponse = await axios.post(
          api + "api/tool/exportDoc",
          {
            str: encr(
              JSON.stringify({
                lib: "word",
                name: genKey() + ".docx",
                html: html,
                opition: {
                  orientation: "Portrait",
                  pageSize: "A4",
                  left: 37.79,
                  top: 68.03,
                  right: 37.79,
                  bottom: 68.03,
                },
              }),
              SecretKey,
              cryoptojs
            ).toString(),
          },
          config
        );

        if (axResponse.status == 200) {
          window.open(api + "api/Files/downloadFile?name=doc-test-edit.docx");
          download("doc-test-edit.json", JSON.stringify(spans.value));
        }
        showLoadding.value = false;
      } catch (e) {
        console.log(e);
      }
    };
    const downloadTemplateFile = async () => {
      let html = dochtml.innerHTML;
      tempHTML = html;
      showLoadding.value = true;
      try {
        const axResponse = await axios.post(
          api + "api/tool/exportDoc",
          {
            str: encr(
              JSON.stringify({
                lib: "word",
                name: genKey() + ".docx",
                html: html,
                opition: {
                  orientation: "Portrait",
                  pageSize: "A4",
                  left: 37.79,
                  top: 68.03,
                  right: 37.79,
                  bottom: 68.03,
                },
              }),
              SecretKey,
              cryoptojs
            ).toString(),
          },
          config
        );

        if (axResponse.status == 200) {
          window.open(api + "api/Files/downloadFile?name=doc-test-edit.docx");
          download("doc-test-edit.json", JSON.stringify(spans.value));
        }
        showLoadding.value = false;
      } catch (e) {
        console.log(e);
      }
    };
    onMounted(() => {
      document.querySelector("title").innerHTML = "SOE";
      document
        .querySelector("link[rel=icon]")
        .setAttribute("href", "https://dashboard.soe.vn/favicon.ico");
      users.forEach((u) => {
        itemusers.value.push({
          label: u.name,
          icon: "pi pi-user",
          command: () => {
            user.value = u;
          },
        });
      });
      document.addEventListener("selectionchange", () => {
        let txt = getSelectionText();
        if (txt != "") {
          selectText = txt;
        }
      });
    });
    const filterUser = ref(users[0]);
    let chonuser = users[0];
    const goUser = (u) => {
      chonuser = u;
      if (filterUser.value.id == u.id) {
        filterUser.value = {};
      } else filterUser.value = u;
    };
    const compSpans = computed(() =>
      spans.value.filter(
        (x) =>
          (!filterUser.value.id && x.historys.length > 0) ||
          x.historys.filter((u) => u.user.id == filterUser.value.id).length > 0
      )
    );
    const isComment = ref(false);
    const spanEditor = ref("");
    const editSpan = (f) => {
      isComment.value = !f;
      displayCK.value = true;
      let ht = elementSpan.value.historys;
      if (f) {
        if (ht.length > 0) {
          spanEditor.value = ht[ht.length - 1].element;
        } else {
          spanEditor.value = elementSpan.value.element;
        }
      } else {
        spanEditor.value = "";
      }
    };
    const displayCK = ref(false);
    const saveCK = () => {
      if (isComment.value) {
      } else {
        let el = document.getElementById(elementSpan.value.id);
        el.classList.add("os-span-edit-os");
        el.innerHTML = spanEditor.value.replace(/<p>(.+)<\/p>/gim, "$1");
        elementSpan.value.version++;
        let vesion = {
          version: elementSpan.value.version,
          user: user.value,
          element: el.outerHTML,
          date: new Date(),
        };
        elementSpan.value.historys.push(vesion);
      }
      displayCK.value = false;
    };
    const reUsers = ref(users);
    const menu = ref();
    const onRightClick = (event) => {
      menu.value.show(event);
    };
    function move(array, oldIndex, newIndex) {
      if (newIndex >= array.length) {
        newIndex = array.length - 1;
      }
      array.splice(newIndex, 0, array.splice(oldIndex, 1)[0]);
      return array;
    }
    const goKey = (item) => {
      if (elementSpan.value != null) {
        let el = spans.value.find((x) => x.id == elementSpan.value.id);
        let span = document.getElementById(elementSpan.value.id);
        if (span != null) {
          let html = span.innerHTML.replaceAll("..", "");
          let mts = html.match(/{{.+}}/gim);
          if (mts && mts.length == 1) {
            html = html.replace(/{{.*}}/gim, "{{}}");
          }
          html = html.replace("{{}}", "{{" + item + "}}");
          elementSpan.innerHTML = html;
          el.innerHTML = elementSpan.innerHTML;
          span.innerHTML = elementSpan.innerHTML;
          span.classList.add("os-span-edit-os");
        }
        datakeys.value = move(
          datakeys.value,
          datakeys.value.indexOf(item),
          datakeys.value.length - 1
        );
      }
    };
    let selectText = "";
    function getSelectionText() {
      var text = "";
      if (window.getSelection) {
        text = window.getSelection().toString();
      } else if (document.selection && document.selection.type != "Control") {
        text = document.selection.createRange().text;
      }
      return text;
    }
    const setStyle = (val) => {
      let text = getSelectionText();
      if (text == "") {
        text = selectText;
      }
      let el = spans.value.find((x) => x.id == elementSpan.value.id);
      let span = document.getElementById(elementSpan.value.id);
      if (span != null) {
        let html = span.innerHTML;
        let regStyle = new RegExp(
          `<span[^>]*style=(['"][^>]*['"])[^>]*>?${text}<\/span>`,
          "igm"
        );
        let style = "";
        let matchs = html.match(regStyle);
        if (matchs) {
          matchs.forEach((st) => {
            let sts = st.match(
              /(?:\G(?!^)|\bstyle=")([^:]*):\s*([^;]*)[;"](?=[^>]*>)/g
            );
            if (sts.length > 0)
              style = sts[0]
                .replace("'style=\"'", "")
                .replace("\"'", "")
                .replace('style="', "")
                .replace('"', "");
          });
        }
        if (style) {
          style += ";";
        }
        let reg = new RegExp(`(<span[^>]*>)?${text}(<\/span>)?`, "igm");
        switch (val) {
          case "N":
            style = style.replace(/(text-transform:[^;]+)/, "");
            style = style.replace(/(text-decoration:[^;]+)/, "");
            style = style.replace(/(font-style:[^;]+)/, "");
            style = style.replace(/(font-weight:[^;]+)/, "");
            break;
          case "B":
            style = style.replace(/(font-weight:[^;]+)/, "");
            style += "font-weight:bold;";
            break;
          case "I":
            style = style.replace(/(font-style:[^;]+)/, "");
            style += "font-style:italic;";
            break;
          case "U":
            style = style.replace(/(text-decoration:[^;]+)/, "");
            style += "text-decoration:underline;";
            break;
          case "AA":
            style = style.replace(/(text-transform:[^;]+)/, "");
            style += "text-transform:uppercase;";
            break;
          case "Aa":
            style = style.replace(/(text-transform:[^;]+)/, "");
            style += "text-transform:capitalize;";
            break;
          case "aa":
            style = style.replace(/(text-transform:[^;]+)/, "");
            style += "text-transform:lowercase;";
            break;
          default:
            style = style.replace(/(color:[^;]+)/, "");
            style += `color:${val};`;
            break;
        }
        style = style.replaceAll(";;", ";");
        html = html.replace(reg, `<span style="${style}">${text}</span>`);
        elementSpan.innerHTML = html;
        el.innerHTML = elementSpan.innerHTML;
        span.innerHTML = elementSpan.innerHTML;
      }
    };
    const compuDatakeys = computed(() =>
      mapdatakeys.filter((x) => Array.isArray(datausers[0][x.name]))
    );
    const refershHTML = () => {
      dochtml = document.getElementById("dochtml");
      dochtml.innerHTML = tempHTML;
      isHasHTML.value = true;
      spans.value = [];
      eventDoc();
    };
    return {
      user,
      compuDatakeys,
      setStyle,
      pureColor,
      gradientColor,
      onRightClick,
      menu,
      datakeys,
      itemusers,
      elementSpan,
      myDocUploader,
      myJsonUploader,
      downloadFile,
      editSpan,
      isHasHTML,
      compSpans,
      showLoadding,
      displayCK,
      isComment,
      spanEditor,
      saveCK,
      reUsers,
      goUser,
      goKey,
      mapdatakeys,
      filterUser,
      isstart,
      isend,
      setSelect,
      displayTable,
      saveConfigTable,
      mdData,
      downloadTemplateFile,
      refershHTML,
    };
  },
};
</script>

<template>
  <div class="flex mb-1">
    <FileUpload
      chooseLabel="Chọn File Word"
      mode="basic"
      :auto="true"
      :customUpload="true"
      @uploader="myDocUploader"
      name="doc[]"
      accept=".doc,.docx"
    />
    <FileUpload
      v-if="isHasHTML"
      chooseLabel="Chọn File Json"
      class="ml-1 mr-1"
      mode="basic"
      :auto="true"
      :customUpload="true"
      @uploader="myJsonUploader"
      name="doc[]"
      accept=".json"
    />
    <ProgressSpinner
      v-if="showLoadding"
      style="width: 50px; height: 50px"
      strokeWidth="8"
      fill="var(--surface-ground)"
      animationDuration=".5s"
    />
    <div class="tool" v-if="isHasHTML && elementSpan.id">
      <Button
        v-tooltip="'Chữ thường'"
        @click="setStyle('N')"
        label="Normal"
        class="p-button-secondary p-button-outlined ml-1"
      />
      <Button
        v-tooltip="'Chữ đậm'"
        @click="setStyle('B')"
        label="B"
        class="p-button-secondary p-button-outlined ml-1"
      />
      <Button
        v-tooltip="'Chữ nghiêng'"
        @click="setStyle('I')"
        label="I"
        class="p-button-secondary p-button-outlined ml-1"
      />
      <Button
        v-tooltip="'Chữ gạch chân'"
        @click="setStyle('U')"
        label="U"
        class="p-button-secondary p-button-outlined ml-1"
      />
      <Button
        v-tooltip="'Viết hoa'"
        @click="setStyle('AA')"
        label="AA"
        class="p-button-secondary p-button-outlined ml-1"
      />
      <Button
        v-tooltip="'Viết hoa chữ đầu'"
        @click="setStyle('Aa')"
        label="Aa"
        class="p-button-secondary p-button-outlined ml-1"
      />
      <Button
        v-tooltip="'Viết thường'"
        @click="setStyle('aa')"
        label="aa"
        class="p-button-secondary p-button-outlined ml-1"
      />
      <Button
        v-tooltip="'Chọn màu chữ'"
        class="p-button-secondary p-button-outlined ml-1"
      >
        <ColorPicker
          style="width: 24px"
          v-model:gradientColor="gradientColor"
          pickerType="chrome"
          @pureColorChange="setStyle"
          v-model:pureColor="pureColor"
        />
      </Button>
      <Dropdown
        @change="goKey($event.value.name)"
        class="ml-1"
        optionLabel="name"
        :options="mapdatakeys"
        :filter="true"
        placeholder="Data"
        :showClear="true"
      >
        <template #option="slotProps">
          <div>{{ slotProps.option.name }}</div>
        </template>
      </Dropdown>
      <Button
        @click="setSelect(true)"
        label="Dòng bắt đầu"
        :class="
          (isstart ? '' : 'p-button-secondary p-button-outlined') + ' ml-1'
        "
      />
      <Button
        v-if="isstart"
        @click="setSelect(false)"
        label="Dòng kết thúc"
        :class="(isend ? '' : 'p-button-secondary p-button-outlined') + ' ml-1'"
      />
    </div>
  </div>
  <Divider class="m-0 p-0" />
  <div class="flex" style="background-color: #eee">
    <div
      :style="{
        height: 'calc(100vh - 50px)',
        overflow: 'hidden',
        padding: '20px',
        backgroundColor: '#ccc',
      }"
    >
      <div
        class="doc-page card shadow-1"
        style="min-height: 100%; position: relative"
      >
        <!-- <div spellcheck="false" id="dochtml" @contextmenu="onRightClick"></div> -->
        <iframe
          name="dochtml"
          id="dochtml"
          :style="{ width: '100%', height: '100%', border: 'none' }"
          spellcheck="false"
          @contextmenu="onRightClick"
        ></iframe>
      </div>
    </div>
    <div class="flex-1 bg-white mr-2 ml-2" v-if="isHasHTML">
      <div style="height: calc(100vh - 150px); overflow-y: auto">
        <div class="card p-5">
          <div class="flex">
            <div class="flex-1 p-0 m-0">
              <Button
                v-tooltip="'Hiển thị tất cả'"
                v-if="elementSpan.element"
                icon="pi pi-angle-left"
                class="p-button-rounded p-button-secondary p-button-outlined p-button-sm mr-1"
                @click="elementSpan = {}"
              />
              <Button
                v-tooltip="'Chỉnh sửa nâng cao'"
                v-if="elementSpan.element"
                icon="pi pi-file-edit"
                class="p-button-rounded p-button-success p-button-sm mr-1"
                @click="editSpan(true)"
              />
              <Button
                v-tooltip="'Thêm comment'"
                v-if="elementSpan.element"
                icon="pi pi-comments"
                class="p-button-rounded p-button-info p-button-sm mr-1"
                @click="editSpan(false)"
              />
            </div>
            <AvatarGroup class="ml-1 mr-1">
              <Avatar
                :style="{
                  border: filterUser.id == u.id ? '3px solid red' : 'none',
                }"
                @click="goUser(u)"
                v-for="u in reUsers"
                :image="u.img"
                size="large"
                shape="circle"
                :v-badge.danger="'u.badge'"
              />
            </AvatarGroup>
            <SplitButton
              class="p-button-sm"
              :label="user.name"
              icon="pi pi-user-edit"
              :model="itemusers"
            >
            </SplitButton>
          </div>
          <Divider />
          <div class="history" v-if="elementSpan.element">
            <div class="goc flex m-1 p-2">
              <Badge value="1" severity="success" class="mr-1"></Badge>
              <div class="flex-1 ml-1 mr-1" v-html="elementSpan.element"></div>
              <i>{{ elementSpan.date }}</i>
            </div>
            <div
              :class="'history-row flex m-1 p-2 ' + sp.user.id"
              v-for="sp in elementSpan.historys"
            >
              <Badge :value="sp.version" class="mr-1"></Badge>
              <Avatar :image="sp.user.img" shape="circle" />
              <div class="flex-1 ml-1 mr-1" v-html="sp.element"></div>
              <Chip :label="sp.date.toLocaleString('vi-VN')" class="ml-2" />
            </div>
          </div>
          <div class="history" v-if="!elementSpan.element">
            <div
              @click="elementSpan = sp"
              :class="
                'history-row flex m-1 p-2 ' +
                (!sp.historys[sp.historys.length - 1]
                  ? ''
                  : sp.historys[sp.historys.length - 1].user.id)
              "
              v-for="sp in compSpans"
            >
              <Badge :value="sp.version" class="mr-1"></Badge>
              <Avatar
                :image="sp.historys[sp.historys.length - 1].user.img"
                shape="circle"
              />
              <div
                class="flex-1 ml-1 mr-1"
                v-html="sp.historys[sp.historys.length - 1].element"
              ></div>
              <Chip
                :label="
                  sp.historys[sp.historys.length - 1].date.toLocaleString(
                    'vi-VN'
                  )
                "
                class="ml-2"
              />
            </div>
          </div>
        </div>
      </div>
      <div class="text-center">
        <Button
          @click="refershHTML()"
          label="Template"
          icon="pi pi-refresh"
          class="p-button-secondary ml-1"
        />
        <Button
          class="ml-1 mr-1 p-button-success"
          @click="downloadFile(true)"
          label="Xem trước"
          icon="pi pi-print"
        />
        <Button @click="downloadFile()" label="Tải xuống" icon="pi pi-save" />
        <Button
          @click="downloadTemplateFile()"
          label="Tải Template"
          icon="pi pi-download"
          class="p-button-secondary ml-1"
        />
      </div>
    </div>
  </div>
  <Dialog
    :header="isComment ? 'Bình luận' : 'Chỉnh sửa nâng cao'"
    v-model:visible="displayCK"
    :breakpoints="{ '960px': '75vw', '640px': '90vw' }"
    :style="{ width: '50vw' }"
    :maximizable="true"
    :modal="true"
  >
    <Editor v-model="spanEditor" editorStyle="height: 320px" />
    <template #footer>
      <Button
        label="Không"
        icon="pi pi-times"
        @click="displayCK = false"
        class="p-button-text"
      />
      <Button label="Lưu lại" icon="pi pi-save" @click="saveCK()" autofocus />
    </template>
  </Dialog>
  <Dialog
    header="Thiết lập bảng"
    v-model:visible="displayTable"
    :style="{ width: '360px' }"
    :modal="true"
  >
    <Dropdown
      v-model="mdData"
      :editable="true"
      class="w-full"
      optionValue="id"
      optionLabel="name"
      :options="compuDatakeys"
      :filter="true"
      placeholder="Trường dữ liệu bảng"
    >
      <template #option="slotProps">
        <div>{{ slotProps.option.name }}</div>
      </template>
    </Dropdown>
    <template #footer>
      <Button
        label="Không"
        icon="pi pi-times"
        @click="displayTable = false"
        class="p-button-text"
      />
      <Button
        label="Lưu lại"
        icon="pi pi-save"
        @click="saveConfigTable()"
        autofocus
      />
    </template>
  </Dialog>
  <ContextMenu ref="menu" :model="datakeys">
    <template #item="{ item }">
      <Button @click="goKey(item)" :label="item" class="mb-1 w-full" />
    </template>
  </ContextMenu>
</template>
<style lang="scss" scoped>
:deep {
  .os-span-edit-os {
    background-color: yellow !important;
    cursor: pointer;
  }

  *[class^="for-data"] {
    background-color: yellow !important;
    position: relative;
  }

  *[class^="for-data"]:after {
    content: "\e94a";
    font-family: "primeicons";
    position: absolute;
    top: -25px;
    right: 0px;
    cursor: pointer;
  }

  .vc-color-wrap {
    width: 24px !important;
    margin: 0 !important;
    height: 20px !important;
  }

  .history .os-span-edit-os {
    background-color: unset !important;
    cursor: pointer;
  }

  .history-row.cong {
    background-color: aliceblue;
  }

  .history-row.hai {
    background-color: antiquewhite;
  }

  .history-row.duong {
    background-color: #eee;
  }

  .history-row {
    vertical-align: middle;
    justify-content: center;
    align-items: center;
  }

  span[contenteditable="true"].maxw {
    min-width: 100%;
    display: block;
  }

  td {
    vertical-align: middle !important;
    align-items: center;
  }
}

.doc-page {
  width: 27cm;
  padding: 20mm;
  display: block;
  page-break-before: always;
  background-color: #fff;
}
</style>
<style>
#app-sidebar,
#mobile-header {
  display: none !important;
}

.p-contextmenu-root-list {
  max-height: 400px;
  overflow-y: auto;
  padding: 10px !important;
}
</style>
