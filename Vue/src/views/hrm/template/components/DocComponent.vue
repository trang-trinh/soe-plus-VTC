<script>
import { ref, inject, onMounted, computed, readonly } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode } from "primevue/api";
import Editor from "primevue/editor";
import ConfigDatabaseComponent from "./ConfigDatabaseComponent.vue";
import UserComponent from "./UserComponent.vue";
import InputComponent from "./InputComponent.vue";
import { ColorPicker } from "vue3-colorpicker";
import { Diff } from "vue-diff";
import { CodeDiff } from "v-code-diff";
import "vue3-colorpicker/style.css";
import {
  uid,
  encr,
  getKeyByValue,
  decodeHTMLEntities,
  download,
  isObject,
  removeAccents,
  change_unsigned,
  getTagSelection,
  capitalizeFirstLetter,
  getRandomInt,
  getRandomArray,
  utilformatDate,
} from "../../../../util/function.js";
import { getDataTemp } from "../../../../util/data.js";
import {
  draggableEle,
  wrap,
  parseHTML,
  parseHTMLText,
  colName,
  colNumber,
} from "../../../../util/htmleditor.js";
export default {
  props: {
    db: String,
    report: Object,
    pars: Object,
    onedata: Boolean,
    isedit: Boolean,
    readonly: Boolean,
    callbackFun: Function,
    header: String,
    reload: Function,
  },
  components: {
    Editor,
    ConfigDatabaseComponent,
    UserComponent,
    InputComponent,
    colorpickers: ColorPicker,
    Diff,
    CodeDiff,
  },
  setup(props) {
    const bgColor = [
      "#F8E69A",
      "#AFDFCF",
      "#F4B2A3",
      "#9A97EC",
      "#CAE2B0",
      "#8BCFFB",
      "#CCADD7",
    ];
    //Khởi tạo biến
    //refChild
    const isedit = props.isedit || false;
    const onedata = props.onedata;
    const header = props.header || null;
    const isUrlReport = ref(props.pars ? true : false);
    const readonly = ref(props.readonly ? true : false);
    const isReportListView = ref(
      props.pars && Object.keys(props.pars).length == 0
    );
    const isViewReport = ref(readonly.value || isUrlReport.value);
    const configDBChild = ref(null);
    const report = props.report || null;
    const cryoptojs = inject("cryptojs");
    const selectedStamps = ref({});
    const toast = useToast();
    const swal = inject("$swal");
    const ipsearch = ref("");
    const pureColor = ref("#000000");
    const gradientColor = ref(
      "linear-gradient(0deg, rgba(0, 0, 0, 1) 0%, rgba(0, 0, 0, 1) 100%)"
    );
    let dochtml;
    const isdataSidebar = ref(false);
    const isfullSidebar = ref(false);
    const isfull = ref(false);
    const divZoom = ref(isUrlReport ? 0.9 : 1.0);
    const isdoc = ref(true);
    const isxls = ref(false);
    const isHasHTML = ref(false);
    const isstart = ref(false);
    const isend = ref(false);
    const isDesingData = ref(false);
    const spans = ref([]);
    const axios = inject("axios");
    const objDataTemp = ref([]);
    const objDataTempSave = ref([]);
    let delElements = [];
    const rfUserComp = ref(null);
    const store = inject("store");
    const config = {
      headers: { Authorization: `Bearer ${store.getters.token}` },
    };
    const itemButExports =
      isUrlReport.value || readonly.value
        ? [
            {
              label: "In",
              icon: "pi pi-print",
              command: () => {
                printDiv();
              },
            },
            {
              label: "Xuất file",
              icon: "pi pi-download",
              command: () => {
                downloadFile();
              },
            },
          ]
        : [
            {
              label: "Xem Data",
              icon: "pi pi-print",
              command: () => {
                downloadFile(true);
              },
            },
            {
              label: "In",
              icon: "pi pi-print",
              command: () => {
                printDiv();
              },
            },
            {
              label: "Xuất file",
              icon: "pi pi-download",
              command: () => {
                downloadFile();
              },
            },
            {
              label: "Load lại Template",
              icon: "pi pi-refresh",
              command: () => {
                refershHTML();
              },
            },
            {
              label: "Lưu Template",
              icon: "pi pi-save",
              command: () => {
                downloadTemplateFile(true);
              },
            },
          ];
    const actionMenus = ref([
      {
        label: "Chỉnh sửa",
        icon: "pi pi-fw pi-pencil",
        command: () => {
          const sel = iframeWindow.getSelection();
          const range = sel.getRangeAt(0);
          let td = range.startContainer.parentElement.closest("td");
          if (!td) td = range.startContainer.parentElement.closest("span");
          clickEditData(td);
        },
      },
      {
        label: "Cấu hình",
        icon: "pi pi-fw pi-cog",
        command: () => {
          isViewReport.value = false;
          const sel = iframeWindow.getSelection();
          const range = sel.getRangeAt(0);
          if (isxls.value) {
            let td = range.startContainer.parentElement.closest("td");
            if (!td) td = range.startContainer.parentElement.closest("span");
            if (td.tagName == "TD" && td.className.includes("tablecell")) {
              td = td.parentElement;
            }
            addRowXls(td);
          }
        },
      },
      {
        label: "Mặc định",
        icon: "pi pi-fw pi-list",
        items: [
          {
            label: "Copy",
            icon: "pi pi-fw pi-copy",
            command: () => {
              document.execCommand("copy", false);
            },
          },
          {
            label: "Cut",
            icon: "pi pi-fw pi-clone",
            command: () => {
              document.execCommand("cut", false);
            },
          },
          {
            label: "Delete",
            icon: "pi pi-fw pi-trash",
            command: () => {
              document.execCommand("delete", false);
            },
          },
          {
            label: "Undo dòng",
            icon: "pi pi-fw pi-undo",
            command: () => {
              //document.execCommand("redo", false);
              let i = delElements.length - 1;
              let ids = delElements[i];
              ids.forEach((id) => {
                let delE = document.getElementById(id);
                if (delE) {
                  delE.classList.remove("os-delete-os");
                }
              });
              delElements.splice(i, 1);
            },
          },
          {
            label: "Chèn",
            icon: "pi pi-fw pi-file",
            items: [
              {
                label: "Thêm dòng trên",
                icon: "pi pi-fw pi-angle-up",
                command: () => {
                  const sel = iframeWindow.getSelection();
                  const range = sel.getRangeAt(0);
                  let tagn = getTagSelection(iframeWindow);
                  if (tagn == "tr") {
                    let trs = range.startContainer.parentElement.closest(tagn);
                    let tclone = trs.cloneNode(true);
                    tclone.querySelectorAll("span").forEach((el) => {
                      el.innerHTML = "&nbsp;";
                      el.id = uid();
                      el.className = el.id;
                      spans.value.push({
                        element: "",
                        id: el.id,
                        version: 1,
                        historys: [],
                      });
                      el.addEventListener("click", addClickSpan);
                    });
                    trs.parentNode.insertBefore(tclone, trs);
                  }
                },
              },
              {
                label: "Thêm dòng dưới",
                icon: "pi pi-fw pi-angle-down",
              },

              {
                separator: true,
              },
              {
                label: "Thêm cột trái",
                icon: "pi pi-fw pi-angle-left",
              },
              {
                label: "Thêm cột phải",
                icon: "pi pi-fw pi-angle-right",
              },
              {
                separator: true,
              },
              {
                label: "Xoá dòng",
                icon: "pi pi-fw pi-trash",
                command: () => {
                  const sel = iframeWindow.getSelection();
                  const range = sel.getRangeAt(0);
                  let istable =
                    range.startContainer.parentElement.closest("td").children
                      .length == 1;
                  let tagn = istable ? "tr" : "p";
                  let trs = range.startContainer.parentElement.closest(tagn);
                  let tre = range.endContainer.parentElement.closest(tagn);
                  let delids = [];
                  if (trs != tre) {
                    while (trs != tre) {
                      trs.id = uid();
                      trs.classList.add("os-delete-os");
                      delids.push(trs.id);
                      trs = trs.nextElementSibling;
                    }
                    tre.id = uid();
                    tre.classList.add("os-delete-os");
                    delids.push(tre.id);
                  } else {
                    tre.id = uid();
                    tre.classList.add("os-delete-os");
                    delids.push(tre.id);
                  }
                  delElements.push(delids);
                },
              },
            ],
          },
          {
            label: "Config Data",
            icon: "pi pi-fw pi-cog",
            command: () => {
              const sel = iframeWindow.getSelection();
              const range = sel.getRangeAt(0);
              let td = range.startContainer.parentElement.closest("td");
              clickConfigData(td);
            },
          },
          {
            label: "Chỉnh ảnh",
            icon: "pi pi-fw pi-images",
            command: () => {
              const sel = iframeWindow.getSelection();
              const range = sel.getRangeAt(0);
              draggableEle(
                range.startContainer.querySelector("img"),
                iframeDoc
              );
            },
          },
        ],
      },
      {
        separator: true,
      },
      {
        label: "Thoát",
        icon: "pi pi-fw pi-power-off",
      },
    ]);
    const elementSpan = ref({});
    const hversion = ref(1);
    const displayTable = ref(false);
    const displayEditData = ref(false);
    const mdData = ref("");
    const mdDataHeader = ref("");
    const textEdit = ref("");
    const textEditExcel = ref("");
    const itemusers = ref([]);
    const showLoadding = ref(false);
    const displayElement = ref(false);
    const htmlElement = ref("");
    const objExcel = ref({});
    const currentText = ref("");
    const drcurrentText = ref({});
    const txtIPXLS = ref("");
    const datamaps = ref([]);
    const isComment = ref(false);
    const spanEditor = ref("");
    const displayCK = ref(false);
    const menu = ref();
    //init Datausers
    let iframeDoc;
    let iframeWindow;
    let tempHTML = "";
    let filename = "";
    let tableElement;
    let spanstart;
    let selectText = "";
    let datausers = getDataTemp();
    const datakeys = ref(Object.keys(datausers[0]));
    let mapdatakeys = [];
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
    const reUsers = ref(users);
    const user = ref(users[0]);
    const filterUser = ref(users[0]);
    let chonuser = users[0];

    function addClickSpan(e) {
      // if (e.target.innerHTML.trim() == "&nbsp;") {
      //     e.target.innerHTML = "";
      // }
      let el = spans.value.find((x) => x.id == e.target.id);
      if (el) {
        elementSpan.value = el;
      }
      if (!elementSpan.value.goc) {
        elementSpan.value.goc = e.target.outerHTML;
      }
      e.stopPropagation();
    }
    function addBlurSpan(e) {
      let el = e.target || e;
      let span = spans.value.find((x) => x.id == el.id);
      if (span) {
        let html = span.element;
        if (span.historys.length > 0) {
          html = span.historys[span.historys.length - 1].element.trim();
        }
        if (!html || el.innerHTML.trim() != parseHTML(html)) {
          el.classList.add("os-span-edit-os");
          if (!span.goc) {
            span.goc = elementSpan.value.element;
          }
          let vesion = {
            version: hversion.value,
            user: user.value,
            element: el.outerHTML,
            date: new Date(),
          };
          //span.historys.push(vesion);
          if (span.historys.length == 0) {
            //Add gốc
            span.historys[0] = {
              version: hversion.value - 1,
              user: user.value,
              element: elementSpan.value.element,
              date: new Date(),
            };
          }
          span.historys[hversion.value] = vesion;
          reUsers.value.forEach((u) => {
            u.badge = spans.value.filter(
              (x) => x.historys.filter((h) => h.user.id == u.id) > 0
            );
          });
        }
      }
    }
    const eventDoc = () => {
      if (dochtml)
        isdoc.value =
          dochtml.querySelector("title") != null &&
          dochtml.querySelector("title").innerHTML != "landscape";
      if (!isUrlReport.value && !readonly.value)
        dochtml.querySelectorAll("table").forEach((element) => {
          element.addEventListener("click", function (e) {
            if (element.innerHTML.match(/{{[^}]*}}/gim)) {
              tableElement = element;
              let fdatas = e.target.className
                .replaceAll("for-data", "")
                .split("-");
              mdData.value = fdatas[0];
              if (fdatas.length > 1) {
                mdDataHeader.value = fdatas[1];
              }
              displayTable.value = true;
              e.stopPropagation();
            }
          });
        });
      //Nếu là excel
      if (isxls.value) {
        //let stt = 0;
        if (dochtml)
          dochtml.querySelectorAll("tr").forEach((element) => {
            if (element.innerText.trim() == "" && element.children.length > 2) {
              var css = window.getComputedStyle(element.children[2]);
              if (
                css.getPropertyValue("background-color") != "rgb(255, 255, 0)"
              ) {
                //stt++;
                element.remove();
              }
            }
          });
      }
      if (!isxls.value) {
        if (dochtml)
          dochtml.querySelectorAll("td").forEach((element) => {
            element.style.setProperty("vertical-align", "middle");
            element.style.setProperty("align-items", "center");
          });
      } else
        dochtml.querySelectorAll("td").forEach((element) => {
          element.style.setProperty("padding", "5px");
        });
      let groups = [];
      if (dochtml)
        dochtml
          .querySelectorAll(isxls.value ? "td" : "p>span")
          .forEach((element) => {
            let hasedid = false;
            if (isxls.value) {
              if (element.innerText.includes("data-")) {
                let fdatas = element.innerText
                  .split("|")[1]
                  .replaceAll("]", "")
                  .replaceAll("[", "");
                element.innerText = element.innerText.split("|")[0];
                let xtr = element;
                if (element.tagName == "TD") xtr = element.parentElement;
                xtr.className = "for-data" + fdatas.replace("data-", "");
              } else if (element.innerText.startsWith("(Sum")) {
                let isum = element.innerText.substring(4, 5);
                element.innerText = element.innerText.replace(/\(Sum\d\)/g, "");
                element.parentElement.className = "sum-data-" + isum;
              } else if (element.innerText.includes("|[")) {
                let cname = "[" + element.innerText.split("|[")[1];
                element.innerText = element.innerText.split("|[")[0];
                let type = cname.includes("0;")
                  ? 0
                  : cname.includes("1;")
                  ? 1
                  : 2;
                let etr = element;
                if (type == 0) {
                  etr = element.parentElement;
                }
                etr.setAttribute("config", cname);
              }
              // if (element.innerText.match(/{{.+}}/)) {
              //     let tdtext = element.innerText;
              //     element.innerText = tdtext.split("{{")[0];
              //     let cls = tdtext.split("{{")[1].replaceAll("}}", "").trim();
              //     element.id = cls.split(" ")[1];
              //     element.classList.add(element.id);
              //     hasedid = true;
              // }
            }
            if (
              element.className &&
              element.className.match(/os-span-edit-os/)
            ) {
              hasedid = true;
              if (!element.id)
                element.id = element.className
                  .split("os-span-edit-os")[0]
                  .trim();
              element.className = element.className.replace(
                "os-span-edit-os",
                " os-span-edit-os"
              );
            } else if (element.classList.length == 1 && !isxls.value) {
              hasedid = true;
              if (!element.id) element.id = element.className.trim();
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
            if (!isUrlReport.value && !readonly.value)
              element.addEventListener("click", addClickSpan);
            //Check group
            let group = element.className.split("os-span-edit-osgroup-");
            if (group.length > 1) {
              let groupdataclass = group[1].trim().split(" ")[0];
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
        if (!isUrlReport.value && !readonly.value)
          wrapper.addEventListener("click", function (e) {
            tableElement = wrapper;
            mdData.value = e.target.className.replace("for-data", "");
            displayTable.value = true;
            e.stopPropagation();
          });
      });
    };
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
    const changeInputExcel = (xls) => {
      let n1 = tableElement.getAttribute("title").match(/\d+/)[0];
      if (xls) {
        let n2 = textEditExcel.value.match(/\d+/)[0];
        n1 =
          objExcel.value[textEdit.value.match(/{{[^{]+}}/g)[0]].match(/\d+/)[0];
        textEdit.value = textEditExcel.value
          .replaceAll(n2, n1)
          .replace(/\w+\d+/g, function (vl) {
            return getKeyByValue(objExcel.value, vl) || vl;
          });
      } else {
        let cthuc = "";
        let txt = textEdit.value;
        cthuc = txt.replace(/{{[^{]+}}/g, function (vl) {
          return objExcel.value[vl] || vl;
        });
        let n2 = cthuc.match(/\d+/)[0];
        if (n2 != n1) {
          cthuc = cthuc.replaceAll(n2, n1);
        }
        textEditExcel.value = cthuc;
      }
    };
    const clickEditData = (td) => {
      textEditExcel.value = "";
      if (td.innerText.includes("[")) {
        let cthuc = "";
        let txt = td.innerText;
        cthuc = txt.replace(/{{[^{]+}}/g, function (vl) {
          return objExcel.value[vl] || vl;
        });
        if (dochtml.innerHTML.split(txt).length > 2) {
          let n1 = td.getAttribute("title").match(/\d+/)[0];
          let n2 = cthuc.match(/\d+/)[0];
          if (n2 != n1) {
            cthuc = cthuc.replaceAll(n2, n1);
          }
        }
        textEditExcel.value = cthuc;
      }
      textEdit.value = td.innerText;
      tableElement = td;
      displayEditData.value = true;
    };
    const saveEditData = () => {
      tableElement.innerText = textEdit.value;
      displayEditData.value = false;
    };
    const clickConfigData = (td) => {
      if (td.innerHTML.match(/{{[^}]*}}/gim)) {
        tableElement = td;
        if (td.innerText.includes("|")) {
          let fdatas = td.innerText
            .split("|")[1]
            .replaceAll("]", "")
            .replaceAll("[", "");
          mdData.value = fdatas.replace("data-", "");
        } else if (td.parentElement.className.includes("for-data")) {
          let fdatas = td.parentElement.className;
          mdData.value = fdatas.replace("for-data", "");
        }
        displayTable.value = true;
      } else if (td.innerHTML.match(/\[[^}]*\]/gim)) {
        tableElement = td;
        if (td.innerText.includes("|")) {
          let fdatas = td.innerText
            .split("|")[1]
            .replaceAll("]", "")
            .replaceAll("[", "");
          mdData.value = fdatas.replace("data-", "");
        } else if (td.parentElement.className.includes("for-data")) {
          let fdatas = td.parentElement.className;
          mdData.value = fdatas.replace("for-data", "");
        }
        displayTable.value = true;
      }
    };
    const saveConfigTable = () => {
      if (tableElement != null) {
        if (tableElement.tagName == "TD") {
          //tableElement.innerText = `${tableElement.innerText}|data-[${mdData.value}]`;
          tableElement.parentElement.className = "for-data" + mdData.value;
        } else {
          tableElement.className = "for-data" + mdData.value;
          if (mdDataHeader.value != "") {
            //tableElement.querySelector("thead").className = 'for-data' + mdDataHeader.value;
            tableElement.className += "-for-data" + mdDataHeader.value;
          }
        }
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
    function rerenderImg() {
      dochtml.querySelectorAll(".resizable.draggable").forEach((el) => {
        let img = el.children[0];
        if (img != null) {
          img.setAttribute("style", el.getAttribute("style"));
          el.removeAttribute("style");
          el.removeAttribute("class");
        }
      });
    }
    function displayNumber(key, so) {
      if (parseInt(so) > 10000) return parseInt(so).toLocaleString();
      return so;
    }
    const addExcelStyle = () => {
      if (isxls.value) {
        dochtml.querySelectorAll(".tablecell").forEach((tr) => {
          tr.remove();
        });
        dochtml.querySelectorAll("tbody>tr").forEach((tr, i) => {
          let x = tr.insertCell(0);
          x.className = "tablecell";
          x.innerHTML = i + 1;
        });

        let tr = dochtml.querySelector("tbody").insertRow(0);
        tr.className = "tablecell";
        //Dòng trên
        let sttcol = 1;
        dochtml.querySelectorAll("colgroup>col").forEach((col, j) => {
          if (j == 0) {
            let colclone = col.cloneNode(true);
            colclone.className = "tablecell";
            colclone.removeAttribute("style");
            colclone.setAttribute("width", 50);
            col.parentNode.insertBefore(colclone, col);
          }
          let ic = col.getAttribute("span") || 1;
          for (let iii = 0; iii < ic; iii++) {
            sttcol++;
          }
        });
        for (let si = 1; si < sttcol; si++) {
          let d = tr.insertCell(si - 1);
          d.className = "thcell";
          d.innerHTML = colName(si);
        }
        let d = tr.insertCell(0);
        d.className = "thcell";
        d.innerHTML = "";
        //Chèn công thức excel
        objExcel.value = {};
        dochtml.querySelectorAll("tbody>tr").forEach((tr, i) => {
          let str = 0;
          let stl = 1;
          let col = 0;
          let sttcol = 0;
          tr.querySelectorAll("td").forEach((td, j) => {
            let xlname = `${colName(td.getAttribute("i") || str)}${i}`;
            td.setAttribute("title", xlname);
            if (td.innerText.includes("{{")) {
              let txt = td.innerText;
              if (txt.includes("[")) {
                txt = txt.substring(txt.indexOf("[") + 1, txt.indexOf("="));
              } else {
                if (!txt.startsWith("{{")) txt = "";
              }
              if (txt != "" && txt.startsWith("{{")) {
                objExcel.value[txt] = xlname;
              }
            }
            str += parseInt(td.getAttribute("colspan") || 1);
            if (td.getAttribute("colspan") > 1 && tr.nextElementSibling) {
              var css = window.getComputedStyle(td);
              if (
                css.getPropertyValue("background-color") == "rgb(255, 192, 0)"
              ) {
                for (let k = 0; k < td.getAttribute("colspan"); k++) {
                  if (tr.nextElementSibling.children[stl]) {
                    tr.nextElementSibling.children[stl].setAttribute(
                      "i",
                      col + j + k - sttcol
                    );
                    tr.nextElementSibling.children[stl].setAttribute(
                      "tname",
                      td.innerText
                    );
                  }
                  stl += 1;
                }
                col += parseInt(td.getAttribute("colspan"));
                sttcol++;
              }
            }
          });
        });
      }
    };
    const clearSelect = () => {
      dochtml.querySelectorAll(".os-span-edit-os").forEach((el) => {
        el.classList.toggle("off");
      });
    };
    const goUser = (u) => {
      chonuser = u;
      if (filterUser.value.id == u.id) {
        filterUser.value = {};
      } else filterUser.value = u;
    };
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
    const onRightClick = (event) => {
      if (menu.value) menu.value.show(event);
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
        let span = iframeDoc.getElementById(elementSpan.value.id);
        if (span != null) {
          let html = span.innerHTML;
          let mts = html.match(/{{.+}}/gim);
          if (mts && mts.length == 1) {
            html = html.replace(/{{.*}}/gim, "{{}}");
          }
          if (html.includes("{{}}")) {
            html = html.replace("{{}}", "{{" + item + "}}");
          } else if (selectText != "") {
            html = html.replace(selectText, "{{" + item + "}}");
          }
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
    function getSelectionText() {
      var text = "";
      if (iframeWindow.getSelection) {
        text = iframeWindow.getSelection().toString();
      } else if (iframeDoc.selection && iframeDoc.selection.type != "Control") {
        text = iframeDoc.selection.createRange().text;
      }
      return text;
    }
    function highlight(cssText) {
      const sel = iframeWindow.getSelection();
      const range = sel.getRangeAt(0);
      const {
        commonAncestorContainer,
        startContainer,
        endContainer,
        startOffset,
        endOffset,
      } = range;
      const nodes = [];
      // console.group("range");
      // console.log("range", range);
      // console.log("commonAncestorContainer", commonAncestorContainer);
      // console.log("startContainer", startContainer);
      // console.log("endContainer", endContainer);
      // console.log("startOffset", startOffset);
      // console.log("endOffset", endOffset);
      // console.log(startContainer.parentNode);
      // console.groupEnd();

      if (startContainer === endContainer) {
        let span;
        if (startContainer.tagName) {
          span = [...startContainer.childNodes].find(
            (x) => x.innerText == sel.toString()
          );
          if (span != null) {
            span.style.cssText = cssText;
            return false;
          }
        }
        if (!span) {
          span = document.createElement("span");
        }
        span.style.cssText = cssText;
        range.surroundContents(span);
        return;
      }

      // get all posibles selected nodes
      function getNodes(childList) {
        console.group("***** getNode: ", childList);
        childList.forEach((node) => {
          console.log("node:", node, "nodoType", node.nodeType);

          const nodeSel = sel.containsNode(node, true);
          console.log("nodeSel", nodeSel);

          // if is not selected
          if (!nodeSel) return;

          const tempStr = node.nodeValue;
          console.log("nodeValue:", tempStr);

          if (
            node.nodeType === 3 &&
            tempStr.replace(/^\s+|\s+$/gm, "") !== ""
          ) {
            console.log("nodo agregado");
            nodes.push(node);
          }

          if (node.nodeType === 1) {
            if (node.childNodes) getNodes(node.childNodes);
          }
        });
        console.groupEnd();
      }

      getNodes(commonAncestorContainer.childNodes);

      console.log(nodes);

      nodes.forEach((node, index, listObj) => {
        const { nodeValue } = node;
        let text, prevText, nextText;

        if (index === 0) {
          prevText = nodeValue.substring(0, startOffset);
          text = nodeValue.substring(startOffset);
        } else if (index === listObj.length - 1) {
          text = nodeValue.substring(0, endOffset);
          nextText = nodeValue.substring(endOffset);
        } else {
          text = nodeValue;
        }

        const span = document.createElement("span");
        span.style.cssText = cssText;
        span.append(document.createTextNode(text));
        const { parentNode } = node;

        parentNode.replaceChild(span, node);

        if (prevText) {
          const prevDOM = document.createTextNode(prevText);
          parentNode.insertBefore(prevDOM, span);
        }

        if (nextText) {
          const nextDOM = document.createTextNode(nextText);
          parentNode.insertBefore(nextDOM, span.nextSibling);
        }
      });

      sel.removeRange(range);
    }
    function tags(tag, cssText) {
      var range = iframeWindow.getSelection().getRangeAt(0),
        span = iframeDoc.createElement(tag);
      span.style.cssText = cssText;
      span.appendChild(range.extractContents());
      range.insertNode(span);
    }
    const execStyle = (val) => {
      var command = "";
      var arg;
      var style = "";
      switch (val) {
        case "N":
          command = "removeFormat";
          break;
        case "B":
          command = "bold";
          break;
        case "I":
          command = "italic";
          break;
        case "U":
          command = "underline";
          break;
        case "AA":
          style = "text-transform:uppercase";
          //tags('span', style);
          highlight(style);
          break;
        case "Aa":
          style = "text-transform:capitalize";
          highlight(style);
          //tags('span', style);
          break;
        case "aa":
          style = "text-transform:lowercase";
          highlight(style);
          //tags('span', style);
          break;
        default:
          command = "foreColor";
          arg = val;
          break;
      }
      iframeDoc.execCommand(command, false, arg);
      iframeDoc.getElementById("dochtml").focus();
    };
    const refershHTML = () => {
      dochtml = iframeDoc.getElementById("dochtml");
      dochtml.innerHTML = tempHTML;
      isHasHTML.value = true;
      spans.value = [];
      eventDoc();
    };
    const openHTML = (html) => {
      html = (html || elementSpan.value.element).replaceAll(
        "os-span-edit-os",
        ""
      );
      htmlElement.value = html;
      currentText.value = elementSpan.value.goc;
      displayElement.value = true;
    };
    const setCurrentText = (vs) => {
      if (vs != null) currentText.value = vs.element;
      else {
        currentText.value = elementSpan.value.goc;
        drcurrentText.value = {};
      }
    };
    function removeTags(str) {
      if (str == null || str == "") return "";
      else str = str.toString();

      return str.replace(/(<([^>]+)>)/gi, "").replaceAll("&nbsp;", " ");
    }
    function PrintDiv(divid, title, style) {
      var contents = iframeDoc.getElementById(divid).innerHTML;
      contents = contents.replace(
        /<tr class="tablecell"[^</]*>(.*)<\/tr>/gim,
        ""
      );
      contents = contents.replace(
        /<td class="tablecell"[^</]*>(\d+)<\/td>/gim,
        ""
      );
      contents = contents.replace(/<col class="tablecell"[^</]*>/gim, "");
      var frame1 = iframeDoc.createElement("iframe");
      frame1.name = "frame1";
      frame1.style.position = "absolute";
      frame1.style.top = "-1000000px";
      document.body.appendChild(frame1);
      var frameDoc = frame1.contentWindow
        ? frame1.contentWindow
        : frame1.contentDocument.document
        ? frame1.contentDocument.document
        : frame1.contentDocument;
      frameDoc.document.open();
      frameDoc.document.write(
        `<html><head><title>${title}</title><style type="text/css">${style}</style>`
      );
      frameDoc.document.write("</head><body>");
      frameDoc.document.write(contents);
      frameDoc.document.write("</body></html>");
      frameDoc.document.close();
      setTimeout(function () {
        window.frames["frame1"].focus();
        window.frames["frame1"].print();
        document.body.removeChild(frame1);
      }, 500);
      return false;
    }
    const printDiv = () => {
      let ifr = true;
      let style = `
            @page{
                    margin:1.27cm;
                    mso-paper-source:0;
                    size: ${isdoc.value ? "portrait" : "landscape"};
                }
                table {page-break-inside: auto;}
                tr {page-break-inside: avoid;page-break-after: auto;}
                thead {display: table-header-group;}
                tfoot {
                    display: table-footer-group;
                }
            `;
      if (!ifr) {
        var divContents = dochtml.innerHTML;
        divContents = divContents.replace(
          /<tr class="tablecell"[^</]*>(.*)<\/tr>/gim,
          ""
        );
        divContents = divContents.replace(
          /<td class="tablecell"[^</]*>(\d+)<\/td>/gim,
          ""
        );
        divContents = divContents.replace(
          /<col class="tablecell"[^</]*>/gim,
          ""
        );
        var a = window.open("", "", "height=500, width=500");
        a.document.write(
          '<html><head><title>Lịch</title><style type="text/css">'
        );
        a.document.write(style);
        a.document.write("</style></head><body>");
        a.document.write(divContents);
        a.document.write("</body></html>");
        a.document.close();
        a.print();
        a.close();
      } else PrintDiv("dochtml", filename, style);
    };
    const rmHTML = (html) => {
      return html.replaceAll("<[^>]*>", "");
    };
    const refersMapData = () => {
      if (readonly.value) {
        saveDatamap(true);
        editForm(selectdata);
        return false;
      }
      datausers[0]["Bộ phận quản lý"] = randomData();
      datausers[0]["Bộ phận bán hàng"] = randomData();
      initMapData();
    };
    const showDataSidebar = () => {
      isdataSidebar.value = true;
      checkHide=false;
      if (datamaps.value.length == 0) {
        initMapData();
      }
    };
    let infoU = {
      "[Ông/bà]": "is_sex",
      "[Tên nhân sự]": "full_name",
      "[Chức danh NS]": "position_name",
      "[Phòng ban NS]": "department_name",
    };
    const getCompUsers = (ku, strUsers) => {
      let dt = objDataTemp.value[ku];
      if (!dt.rows) dt.rows = [];
      strUsers.forEach((u, i) => {
        let obj = { stt: i + 1, profile_id: u.profile_id };
        if (dt.rows.findIndex((x) => x.profile_id == u.profile_id) == -1) {
          dt.cols.forEach((co) => {
            obj[co.value.trim()] = u[co.value.trim()];
            if (!obj[co.value.trim()]) {
              let k = Object.keys(infoU).find((x) => x == co.value.trim());
              if (k) {
                obj[co.value.trim()] = u[infoU[k]];
              }
            }
          });
          dt.rows.push(obj);
        }
      });
    };
    const saveDatamap = (f) => {
       
      if (readonly.value) {
        //Save file quyết định, lương...
        let users = [];
        if (!oneRow.value) {
          let arr = [];
          
          dtDataReports.value.forEach((dr) => {
            let objt = {};
            Object.keys(dr)
              .filter((x) => x.includes("_"))
              .forEach((el) => {
                objt[el] = dr[el];
              });

            if (dr.datatemp) {
              dr.ok = true;

              objt.is_data = [dr.datatemp];
            } else {
              dr.ok = false;
              objt.is_data = null;
            }
            arr.push(objt);
          });
          
          props.callbackFun(arr);
          isdataSidebar.value = false;

          return false;
        }
        let idx = objDataTemp.value[0].cols.findIndex(
          (x) => x.value == "[List nhân sự]"
        );
        if (idx != -1) {
          objDataTemp.value[0].cols[idx].key = objDataTemp.value[1].rows
            .map((x) => x["[Ông/bà]"] + " " + x["[Tên nhân sự]"])
            .join(", ");
        }
        objDataTemp.value.forEach((r) => {
          let o = {};
          r.cols.forEach((c) => {
            if (
              c.inputtype == "Date" &&
              c.key instanceof Date &&
              !isNaN(c.key)
            ) {
              o[c.value] = utilformatDate(c.key, "dd/MM/yyyy");
            } else o[c.value] = c.key;
          });
          if (r.rows) {
            r.rows.sort((a, b) => a.stt - b.stt);
            o.rows = r.rows;
          }
          users.push(o);
        });

        if (f) {
          delete objForm.value.is_data;
        } else objForm.value.is_data = users;
        cForm.value.ok = !f;
        props.callbackFun(objForm.value);
        isdataSidebar.value = false;
        return false;
      }
      datausers = [...datamaps.value];
      datakeys.value = Object.keys(datausers[0]);
      mapdatakeys.value = [];
      datakeys.value.forEach((element) => {
        mapdatakeys.push({ name: element, id: removeAccents(element) });
      });
      datausers.forEach((u) => {
        Object.keys(u).forEach((p) => {
          if (isNaN(p) && u[p] && !Array.isArray(u[p]) && !isObject(u[p])) {
            u[p.toUpperCase()] = u[p].toString().toUpperCase();
            u[p.toLowerCase()] = u[p].toString().toLowerCase();
            u[capitalizeFirstLetter(p)] = capitalizeFirstLetter(
              u[p].toString(),
              true
            );
          }
        });
      });
      isdataSidebar.value = false;
      downloadFile(true);
    };
    const addRow = (r, r1) => {
      r[r1].push({ ...r[r1][r[r1].length - 1] });
    };
    const delRow = (r, r1, i) => {
      if (r[r1].length > 0) r[r1].splice(i, 1);
      if (r1 == "rows") {
        r.users = r.rows.map((x) => x.profile_id).join(",");
        rfUserComp.value[0].initSelectU(r.users);
      }
    };
    const isFormat = (r1, r) => {
      if (r1 == "nam" || r1 == "Năm" || r1 == "Số" || r1 == "CMND") {
        return false;
      }
      return true;
    };
    const isTypeInput = (vl, k) => {
      if (
        !isNaN(vl) &&
        vl != "" &&
        !vl.toString().startsWith("0") &&
        k != "năm"
      ) {
        return 1; //Số
      } else if (
        vl &&
        vl.toString().includes("/") &&
        vl.toString().split("/").length == 3
      ) {
        return 2; //Date
      }
      return 0; //Text
    };
    const arrayKeys = (r, r1) => {
      let keys = Object.keys(r[r1][0]);
      let html = dochtml.innerHTML;
      keys = keys.filter(
        (x) => !html.includes(`{{${x}}}=`) && !html.includes(`{{${x}}})=`)
      );
      return keys;
    };
    const toogledivZoom = (f) => {
      if (f) {
        divZoom.value = (divZoom.value * 10 + 1) / 10;
      } else {
        divZoom.value = (divZoom.value * 10 - 1) / 10;
      }
    };
    const toolCellLine = () => {
      let divcell = dochtml.parentElement.parentElement;
      divcell.classList.toggle("line");
    };
    //Json
    const loadJsonTemplate = (text) => {
      showLoadding.value = true;
      let sps = JSON.parse(text);
      spans.value.forEach((sp) => {
        let sap = sps.find((x) => x.id == sp.id);
        if (sap != null) {
          sp.id = sap.id;
          sp.version = sap.version;
          hversion.value = sp.version + 1;
          sp.goc = sap.goc;
          sp.historys = sap.historys;
          sp.historys.forEach((h) => {
            h.date = new Date(h.date.toString());
          });
          iframeDoc.querySelectorAll("." + sp.id).forEach((el) => {
            if (el.innerHTML.trim() != "&nbsp;") {
              el.classList.add("os-span-edit-os");
            }
          });
        }
      });
      reUsers.value.forEach((u) => {
        u.badge = spans.value.filter(
          (x) => x.historys.filter((h) => h.user.id == u.id).length > 0
        ).length;
      });
      showLoadding.value = false;
    };
    const myDocUploader = async (event) => {
      swal.fire({
        width: 110,
        didOpen: () => {
          swal.showLoading();
        },
      });
      showLoadding.value = true;
      let formData = new FormData();
      formData.append("doc", event.files[0]);
      let xls = event.files[0].name.includes(".xls");
      let apimethod = xls ? "PostFileXLS" : "PostFile";
      // apimethod = "PostFile";
      try {
        const response = await fetch(
          baseURL +
            //+ "api/Files/"
            "api/SRC/" +
            apimethod,
          {
            method: "POST",
            body: formData,
          }
        );
        var data = await response.json();

        let html = data.htmls;
        dochtml.innerHTML = html;
        isxls.value = true;
        addExcelStyle();

        await initDataTempAuto(true);

        let rowmnumber = dtExcels.value[0].rowmnumber || 1;
        dochtml.querySelectorAll('[style*="display:none"]').forEach((tr) => {
          tr.remove();
        });
        let trs = dochtml.querySelectorAll("tr");
        let rows = [];
        let cols = objDataTemp.value[0].cols;
        for (let i = rowmnumber; i < trs.length; i++) {
          if (trs[i].children.length >= cols.length) {
            let obj = {};
            cols.forEach((c, j) => {
              if (c) {
                c.value = c.value.replaceAll("\n", "");
             
              }
              if (trs[i].children[j + 1]) {
                //console.log(c.value+" - "+trs[i].children[j + 1].innerText);
                if (trs[i].children[j + 1].innerText.trim().includes("%")) {
                  obj[c.value] = trs[i].children[j + 1].innerText.trim();
                } else
                  obj[c.value] =
                    trs[i].children[j + 1].getAttribute("x:num") ||
                    trs[i].children[j + 1].innerText.trim();
              }
              if (obj[c.value] == "#REF!") {
                obj[c.value] = null;
              }
            });

            rows.push(obj);
          }
        }

        let arrF = [];
        dtDataReports.value.forEach((r) => {
          let rr = rows.find(
            (x) =>
              x["HỌ VÀ TÊN"].trim().toUpperCase() ==
                r["Họ tên"].split("<br/>")[0].trim().toUpperCase() ||
              (r["profile_code"] &&
                (x["Mã NS"] == r["profile_code"] ||
                  x["Mã Nhân sự"] == r["profile_code"]))
          );

          if (rr) {
            r.datatemp = rr;
            arrF.push(r);
          } else {
            delete r.datatemp;
          }
        });

        dtDataReports.value = arrF;
        await saveDatamap(false);

        props.callbackFun(
          { is_config: JSON.stringify(objDataTemp.value) }
        );
        // props.reload();
        showLoadding.value = false;
        isdataSidebar.value = false;
        initTemplate(true);
      } catch (e) {
        console.log(e);
      }
      swal.close();
    };
    const importJsonData = (event) => {
      let file = event.files[0];
      if (file.name.includes("xls")) {
        checkHide=true;
        myDocUploader(event);
        return false;
      }
      var reader = new FileReader();
      reader.onload = function () {
        var text = reader.result;
        datausers = JSON.parse(text);
        initMapData();
      };
      reader.readAsText(file);
    };
    //Init
    const refershConfig = () => {
      //dochtml.innerHTML = tempHTMLGoc;
      objDataTemp.value = [];
      dtExcels.value = [];
      initDataTempAuto();
    };
    let dtUser = {};
    const initDataTempAuto = async (tf) => {
       
      if (!isUrlReport.value) {
        let dts = await goProc(
          false,
          `soe_user_info`,
          [{ par: "user_id", va: store.getters.user.user_id }],
          false,
          true
        );
        dtUser = dts[0];
      }
      let stt = 0;
      let obj = { stt: 0, key: 0, value: "data-" + stt, cols: [] };
      let tagps = [];
      objDataTemp.value = [];
      if (dochtml)
        dochtml
          .querySelectorAll('[style*="background-color:#ffff00"]')
          .forEach((el, i) => {
            if (
              !el.closest("td") ||
              el.closest("td").style.backgroundColor != "rgb(255, 255, 0)"
            ) {
              if (
                !(
                  el.closest("p") &&
                  /\[.?\s*\[/g.test(el.closest("p").innerText)
                )
              ) {
                if (
                  obj.cols.findIndex(
                    (x) => x.value == removeTags(el.innerText)
                  ) == -1
                ) {
                  obj.cols.push({
                    tid: stt,
                    stt: i + 1,
                    key: readonly.value ? "" : removeTags(el.innerText),
                    value: removeTags(el.innerText),
                  });
                }
              } else {
                if (tagps.findIndex((x) => x == el.closest("p")) == -1)
                  tagps.push(el.closest("p"));
              }
            }
          });
      objDataTemp.value.push(obj);
      //For trong Word
      tagps.forEach((tp, i) => {
        stt++;
        let obj = {
          stt: i + 1,
          key: stt,
          value: "data-" + stt,
          table: true,
          cols: [],
        };
        tp.querySelectorAll('[style*="background-color:#ffff00"]').forEach(
          (el, j) => {
            if (
              !el.closest("td") ||
              el.closest("td").style.backgroundColor != "rgb(255, 255, 0)"
            ) {
              if (
                /\[.+\]/g.test(el.innerText) &&
                obj.cols.findIndex(
                  (x) => x.value == removeTags(el.innerText)
                ) == -1
              ) {
                let k = removeTags(el.innerText);
                k = k
                  .replace(/\[.*\[/, "[")
                  .replace(/\].*\]/, "]")
                  .trim();
                obj.cols.push({
                  tid: stt,
                  stt: j + 1,
                  key: readonly.value ? "" : k,
                  value: k,
                });
              } else {
                //el.style.backgroundColor="";
              }
            }
          }
        );
        tp.id = "data-" + stt;
        objDataTemp.value.push(obj);
      });
      //
      if (dochtml)
        dochtml.querySelectorAll("table").forEach((etb, i) => {
          if (
            etb.querySelector("td").style.backgroundColor == "rgb(255, 255, 0)"
          ) {
            stt++;
            let obj = { stt: i + 1, key: stt, value: "data-" + stt, cols: [] };
            etb.classList.add("data-" + stt);
            etb.querySelectorAll("tr>td").forEach((td, j) => {
              if (/\[.+\]/g.test(td.innerText))
                obj.cols.push({
                  tid: stt,
                  stt: j + 1,
                  key: readonly.value ? "" : removeTags(td.innerText),
                  value: removeTags(td.innerText),
                });
            });
            objDataTemp.value.push(obj);
          }
        });
      expandedRows.value = objDataTemp.value.filter((x) => x.cols);
      if (
        tf != true &&
        props.report.report_config &&
        props.report.report_config.trim() != ""
      ) {
        try {
          objConfig = JSON.parse(props.report.report_config.trim());

          if (isUrlReport.value && Object.keys(props.pars).length > 0) {
            await initURLReport();
          } else if (Object.keys(objConfig.proc).length > 0) {
            await initReportData(objConfig);
          }
          if (objConfig.data) {
            if (isxls.value) {
              dtExcels.value = objConfig.data;
            } else {
              //objDataTemp.value = objConfig.data;
              objDataTemp.value.forEach((ot) => {
                let td = objConfig.data.find((x) => x.value == ot.value);
                if (td) {
                  ot.key = td.key;
                  ot.cols.forEach((co) => {
                    let cot = td.cols.find((x) => x.value == co.value);
                    if (cot) {
                      Object.keys(cot).forEach((k) => {
                        co[k] = cot[k];
                      });
                    }
                  });
                }
              });
            }
          }
        } catch (e) {
          console.log(e);
        }
      }

      objDataTempSave.value=[...objDataTemp.value]
      if (isxls.value) initdbXLS();
      if (objDataTemp.value[0].cols.length == 0) {
        await initTempAI();
      }
      tempHTMLGoc = dochtml.innerHTML;
    };
    const initTempAI = async () => {
      let stt = 0;
      spans.value
        .filter(
          (x) =>
            /\.*…+\.*…*\.*…*/gim.test(x.element) || /\.{3,}/gim.test(x.element)
        )
        .forEach((el) => {
          let ehtml = el.element.replace(/\.*…+\.*…*\.*…*/g, function (su) {
            stt++;
            objDataTemp.value[0].cols.push({ value: `[${stt}]` });
            return `<fo style="background-color:yellow">[${stt}]</fo>`;
          });
          iframeDoc.getElementById(el.id).innerHTML = ehtml;
        });
      spans.value = [];
      eventDoc();
      spans.value
        .filter((x) => /\.{3,}/g.test(x.element))
        .forEach((el) => {
          let ehtml = el.element.replace(/\.{3,}/g, function (su) {
            stt++;
            objDataTemp.value[0].cols.push({ value: `[${stt}]` });
            return `<fo style="background-color:yellow">[${stt}]</fo>`;
          });
          iframeDoc.getElementById(el.id).innerHTML = ehtml;
        });
    };
    let tempHTMLGoc = "";
    const initTemplate = (fi) => {
      if (fi != true) {
        //Nếu dùng Iframe
        let iframe = document.getElementById("docIframe");
        let divleftiframe = document.getElementById("divleft-frame");
        iframeWindow = iframe.contentWindow;
        iframeDoc = iframeWindow.document;
        iframeDoc.head.insertAdjacentHTML(
          "beforeend",
          `<style>
                body{margin: 0;}.resizable{resize: both;overflow: auto;}
        .highlighted,.os-span-edit-os,.x,[class^=for-data],span.modified{background-color:#ff0!important}.history .os-span-edit-os,.os-span-edit-os.off{background-color:unset!important}tr,/*tr td{height:fit-content!important*/}.p-panel-content{overflow-x:auto}.text1line{overflow:hidden}.ul-form{margin:0;padding:0}.ul-form li{display:inline-block}.ul-form li .cols-2{margin-bottom:5px}[class^=for-data]{position:relative}[class^=for-data]:after{content:"\e94a";font-family:primeicons;position:absolute;top:-25px;right:0;cursor:pointer}.vc-color-wrap{width:24px!important;margin:0!important;height:20px!important}.history .os-span-edit-os{cursor:pointer}.history-row.cong{background-color:#f0f8ff}.history-row.hai{background-color:#faebd7}.history-row.duong{background-color:#eee}.history-row{vertical-align:middle;justify-content:center;align-items:center}.text1line{text-overflow:ellipsis;display:-webkit-box;-webkit-line-clamp:1;line-clamp:1;-webkit-box-orient:vertical}td{vertical-align:middle!important;align-items:center}.vue-diff-viewer .vue-diff-row .hljs{word-break:break-word!important;padding:10px;font-family:arial}.code-diff-view .diff-table .blob-code .blob-code-inner{font-size:15px!important;font-family:arial!important}.code-diff-view .diff-table .blob-code{padding:10px}.doc-page{margin: auto;width:29.7cm;padding:10mm;display:block;page-break-before:always;background-color:#fff;position:relative}.p-dialog-content tr:hover td{background-color:#ff0}.div-excel{padding:0!important}.div-excel tr:hover td{background-color:#ff0!important;color:#000!important}.div-excel tr:hover td:hover{background-color:#f0f8ff!important;color:#000!important}.div-excel.line td{border:1px solid #999}.tablecell{text-align:center;background-color:#eee;border-bottom:1px solid #999}td.thcell{padding:5px;border-right:1px solid #999}.thcell,td.tablecell{background-color:#eee;position:sticky;z-index:999;top:0;left:0}#app-sidebar,#gtx-trans,#mobile-header{display:none!important}.true .p-contextmenu-root-list{max-height:400px;overflow-y:auto;padding:10px!important}[contenteditable]{outline:transparent solid 0}.os-delete-os{display:none}
                </style>
                `
        );
        iframeDoc.querySelector("body").appendChild(divleftiframe);

        filename = change_unsigned(
          props.report.report_name
            .replace(/\t/g, "_")
            .replace(/[/\\?%*:|"<>]/g, "-"),
          "_"
        );
      }
      showLoadding.value = true;

      if (props.report.report_template)
        isxls.value = props.report.report_template.startsWith("<!doctype");
      tempHTML = "";

      if (props.report.report_template)
        initDocHTML(props.report.report_template);
      showLoadding.value = false;

      if (isxls.value) {
        //divZoom.value = "0.75";
        addExcelStyle();
      }
      initDataTempAuto();
      if (dochtml) {
        tempHTMLGoc = dochtml.innerHTML;
        isdoc.value =
          dochtml.querySelector("table").offsetWidth <= dochtml.offsetWidth;
        if (!isdoc.value) {
          dochtml.querySelector("table").style.marginLeft = "auto";
          dochtml.querySelector("table").style.marginRight = "auto";
        }
      }
    };
    const initmutationObserver = () => {
      var mutationObserver = new MutationObserver(function (mutations) {
        if (dochtml == iframeDoc.activeElement) {
          mutations.forEach(function (mutation) {
            if (mutation.type == "characterData") {
              addBlurSpan(mutation.target.parentElement);
              console.log(mutation);
            }
          });
        }
      });
      mutationObserver.observe(dochtml, {
        attributes: true,
        characterData: true,
        childList: true,
        subtree: true,
        attributeOldValue: true,
        characterDataOldValue: true,
      });
    };
    const initMapData = () => {
      let obj = {};
      datamaps.value = [];
      let html = dochtml.innerHTML;
      iframeDoc.querySelectorAll('[class^="for-data"]').forEach((div) => {
        let dtclass = div.className.replaceAll("for-data", "");
        let istable = dtclass.split("-").length == 2;
        dtclass.split("-").forEach((dtc, i) => {
          let ok = mapdatakeys.find((x) => x.id == dtc);
          obj[dtc] = [];
          let dts = datausers[0][dtc] || (ok ? datausers[0][ok.name] : []);
          if (dts) {
            dts = [].concat(dts);
            if (dts.length > 0) {
              dts.forEach((tr) => {
                let otr = {};
                let dhtml = div.innerHTML;
                if (istable && i == 1) {
                  dhtml = div.querySelector("thead").innerHTML;
                } else if (istable && i == 0) {
                  dhtml = div.querySelector("tbody").innerHTML;
                }
                dhtml.match(/{{[^}]+}}/gim).forEach((el) => {
                  let ek = rmHTML(el.replace("{{", "").replace("}}", ""));
                  otr[ek] = tr[ek];
                });
                obj[dtc].push(otr);
              });
            } else {
              let otr = {};
              let dhtml = div.innerHTML;
              dhtml.match(/{{[^}]+}}/gim).forEach((el) => {
                let ek = rmHTML(el.replace("{{", "").replace("}}", ""));
                otr[ek] = "";
              });
              obj[dtc].push(otr);
            }
          }
        });
        html = html.replace(div.innerHTML, "");
      });
      let mts = html.match(/{{[^}]+}}/gim);
      if (mts)
        mts.forEach((el, i) => {
          let ek = rmHTML(el.replace("{{", "").replace("}}", ""));
          obj[ek] = datausers[0][ek];
        });
      datamaps.value.push(obj);
    };
    const initDocHTML = (html) => {
      html = html.replaceAll("&nbsp;", " ");
      //html = html.replace(/[\.…]{2,}/igm, '{{}}');
      dochtml = iframeDoc.getElementById("dochtml");
      dochtml.innerHTML = html;
      isHasHTML.value = true;
      spans.value = [];
      eventDoc();
    };
    //Config Database
    let proc = {};
    const isDisplayDatabase = ref(false);
    const IsOne = ref(false);
    const dbrow = ref({});
    const openCogDatabase = (row, f, o) => {
      if (f) {
        if (row.isfor) {
          row.value = row.colname;
          objDataTemp.value = row.cols;
        } else {
          let rr = dtExcels.value.find(
            (x) => x.rowmnumber == row.rowmnumber && x.isfor
          );
          if (rr) {
            objDataTemp.value = rr.cols;
          }
        }
      }
      if (!row.value) row.value = row.name;
      dbrow.value = row;
      if (o != null) IsOne.value = o;

      isDisplayDatabase.value = true;
    };
    const saveDatabase = () => {
      //isDisplayDatabase.value = false;
      configDBChild.value.saveDatabase();
    };
    const saveConfig = () => {
      props.callbackFun({
        report_config: JSON.stringify({
          data: isxls.value ? dtExcels.value : objDataTemp.value,
          proc: proc,
          sum_key: report.sum_key,
        }),
      });
      toast.success("Đã lưu cấu hình thành công!");
    };
    const nextDBrow = (f) => {
      let idx = dtTempCols.value.findIndex(
        (x) => x["value"] == dbrow.value["value"]
      );
      dbrow.value.isconfig = true;
      if (idx != dtTempCols.value.length - 1) {
        f ? idx++ : idx--;
        dbrow.value = dtTempCols.value[idx];
      } else {
        if (objDataTemp.value[dbrow.value.tid + 1])
          dbrow.value = objDataTemp.value[dbrow.value.tid + 1];
      }
    };
    //Download
    const downloadTemplateFile = async (f) => {
      rerenderImg();
      dochtml.querySelectorAll("os-delete-os").forEach((element) => {
        element.remove();
      });
      if (dochtml.querySelector("title"))
        dochtml.querySelector("title").innerHTML = isdoc.value
          ? "portrait"
          : "landscape";
      let html = dochtml.innerHTML;
      html = html.replaceAll("os-delete-os", "");
      tempHTML = html;
      if (f) {
        props.callbackFun({ report_template: html });
        toast.success("Đã lưu Template");
        return false;
      }
      showLoadding.value = true;
      let dataHtml = { html: html, filename: filename || "doc" };
      try {
        const axResponse = await axios.post(
          baseURL +
            //"api/Files/" +
            "api/SRC/" +
            (isxls.value ? "ConvertFileXLS" : "ConvertFile"),
          dataHtml,
          config
        );

        if (axResponse.status == 200) {
          // window.open(
          //   baseURL +
          //     "api/Files/downloadFile" +
          //     (isxls.value ? "XLS" : "") +
          //     "?name=" +
          //     filename +
          //     (isxls.value ? ".xlsx" : ".docx")
          // );
          if (axResponse.data.err == "0") {
            if (isxls.value) {
              downloadFileExport(
                "GetDownloadXLS",
                dataHtml.filename,
                axResponse.data.fileName + ".html",
                ".xlsx"
              );
            } else {
              downloadFileExport(
                "GetDownload",
                dataHtml.filename,
                axResponse.data.fileName + ".html",
                ".docx"
              );
            }
          }
          let sps = [];
          spans.value
            .filter((x) => x.historys.length > 0)
            .forEach((sp) => {
              sps.push({
                id: sp.id,
                version: sp.version,
                historys: sp.historys,
              });
            });
          if (sps.length > 0) download(filename + ".json", JSON.stringify(sps));
          download(filename + ".json", JSON.stringify(sps));
        }
        showLoadding.value = false;
      } catch (e) {
        console.log(e);
      }
    };
    const downloadFileExport = (
      name_func,
      file_name_download,
      file_name,
      file_type
    ) => {
      let nameF = (file_name || "file_download") + file_type;
      let nameDownload = (file_name_download || "file_download") + file_type;
      const a = document.createElement("a");
      a.href = baseURL + "/api/SRC/" + name_func + "?name=" + nameF;
      a.download = nameDownload;
      a.target = "_blank";
      a.click();
      a.remove();
    };
    const downloadFile = async (f) => {
      if (tempHTML != "") {
        dochtml.innerHTML = tempHTML;
      } else {
        tempHTML = dochtml.innerHTML;
      }
      let data =
        datausers.find((x) => x["Họ và Tên"] == chonuser.fname) || datausers[0];
      //Table
      rerenderImg();
      dochtml.querySelectorAll('[class^="for-data"]').forEach((div) => {
        let dtclass = div.className.replaceAll("for-data", "");
        let tbdiv = div;
        div.className = dtclass;
        let thead = div.className.split("-");
        if (dtclass) {
          if (div.tagName.toLocaleLowerCase() == "table") {
            dtclass = thead[0];
            if (div.querySelectorAll("tbody>tr")[1]) {
              div = div.querySelectorAll("tbody>tr")[1];
            } else {
              div = div.querySelectorAll("tbody>tr")[0];
            }
          }
          let ok = mapdatakeys.find((x) => x.id == dtclass);
          if (dtclass) {
            (data[dtclass] || data[ok.name] || []).forEach((r, i) => {
              let divclone = div.cloneNode(true);
              divclone.querySelectorAll("[id]").forEach((el) => {
                el.id = el.id + i.toString();
                el.className = el.className + i.toString();
              });
              let dhtml = divclone.innerHTML;
              Object.keys(r).forEach((k) => {
                let vl = r[k.trim()] || "";
                if (!isNaN(vl)) vl = displayNumber(k, vl);
                if (
                  !dhtml.includes(`{{${k.trim()}}}=`) &&
                  !dhtml.includes(`{{${k.trim()}}})=`)
                )
                  dhtml = dhtml.replaceAll(`{{${k}}}`, vl);
              });
              divclone.innerHTML = dhtml;
              div.parentNode.insertBefore(divclone, div);
            });
            div.remove();
          }
          if (thead.length > 1) {
            ok = mapdatakeys.find((x) => x.id == thead[1]);
            let obj = data[thead[1]] || data[ok.name] || [];
            tbdiv.querySelectorAll("thead td").forEach((td) => {
              let dhtml = td.innerHTML;
              Object.keys(obj).forEach((k) => {
                let vl = obj[k.trim()] || "";
                if (!isNaN(vl)) vl = displayNumber(k, vl);
                dhtml = dhtml.replaceAll(`{{${k}}}`, vl);
              });
              td.innerHTML = dhtml;
            });
          }
        }
      });
      let childiv = dochtml.querySelector("div");
      if (childiv) {
        let strStyle = `
            @page Section1{
                    margin:1.27cm;
                    mso-paper-source:0;
                    size: ${isdoc.value ? "portrait" : "landscape"};
                }
                div.Section1{
                    page:Section1;
                    position: relative;
                }
                table {page-break-inside: auto;}
                tr {page-break-inside: avoid;page-break-after: auto;}
                thead {display: table-header-group;}
                tfoot {
                    display: table-footer-group;
                }
            `;
        childiv.classList.add("Section1");
        childiv.classList.add(isdoc.value ? "portrait" : "landscape");
        let stylee = iframeDoc.createElement("style");
        stylee.innerHTML = strStyle;
        try {
          dochtml.insertBefore(stylee, childiv);
        } catch (e) {}
      }
      //Xử lý với excel
      if (isxls.value) {
        dochtml.querySelectorAll("td.os-span-edit-os").forEach((element) => {
          element.innerText =
            element.innerText + "{{" + element.className + "}}";
        });
      }
      dochtml.querySelectorAll("os-delete-os").forEach((element) => {
        element.remove();
      });
      let html = dochtml.innerHTML;
      html = html.replace(/"\[(\(*({{+)[^\]]+)\]"/g, '"cgXLS[ $1]"');
      html = html.replace(/\[(\(*({{+)[^\]]+)\]/g, "funXLS[$1]");
      datakeys.value.forEach((k) => {
        let vl = data[k.trim()] || "";
        if (vl != "") {
          if (!isNaN(vl)) vl = displayNumber(k, vl);
          html = html.replaceAll(`{{${k}}}`, vl);
        }
      });
      //Tính công thức Excel
      let objFun = {};
      html = html.replace(/(funXLS\[[^\[]+\])/g, function (su) {
        let fu = su
          .replace("funXLS", "")
          .replaceAll(".", "")
          .replaceAll("[", "")
          .replaceAll("]", "");
        fu = decodeHTMLEntities(fu)
          .replace(/(<([^>]+)>)/gi, "")
          .replaceAll("”", '"');
        try {
          let vl;
          if (fu.includes("=")) {
            let ofu = fu
              .split("=")[0]
              .replaceAll("{{", "")
              .replaceAll("}}", "");
            fu = fu.substring(fu.indexOf("=") + 1);
            //console.log(fu);
            try {
              vl = eval(fu);
              if (vl && !isNaN(vl)) vl = Math.ceil(vl);
            } catch (e1) {}
            if (!vl) {
              fu = fu.replaceAll("++", "+").replaceAll("--", "-");
              fu = fu.replaceAll(",", "");
              //console.log(fu);
              if (fu.includes("{{")) {
                fu.match(/{{[^}]*}}/g).forEach((kk) => {
                  fu = fu.replaceAll(
                    kk,
                    objFun[kk.replaceAll("{{", "").replaceAll("}}", "")] || 0
                  );
                });
                //console.log(fu);
                vl = eval(fu);
                if (vl && !isNaN(vl)) vl = Math.ceil(vl);
              } else {
                try {
                  vl = Math.ceil(eval(fu));
                } catch (e2) {
                  if (fu)
                    vl = Math.ceil(fu.replaceAll("+", "").replaceAll("-", ""));
                }
              }
            }
            objFun[ofu] = vl;
            if (!objFun["arr" + ofu]) objFun["arr" + ofu] = [];
            objFun["arr" + ofu].push(vl);
          } else {
            fu = fu.replaceAll(",", "");
            vl = Math.ceil(eval(fu));
          }
          return vl ? vl.toLocaleString() : "";
        } catch (e) {
          console.log(e);
        }
      });
      //Với Excel Style
      html = html.replaceAll("cgXLS[ ", "funXLS[");
      let ii = 0;
      html = html.replace(/(funXLS\[[^\[]+\])/g, function (su) {
        let fu = su
          .replace("funXLS", "")
          .replaceAll(".", "")
          .replaceAll("[", "")
          .replaceAll("]", "");
        //fu = decodeHTMLEntities(fu).replace(/(<([^>]+)>)/gi, "").replaceAll("”", "\"");
        try {
          let vl;
          fu.match(/{{[^}]*}}/g).forEach((kk) => {
            let kke = kk.replaceAll("{{", "").replaceAll("}}", "");
            fu = fu.replaceAll(kk, objFun["arr" + kke][ii] || 0);
          });
          ii = ii + 1;
          //console.log(fu);
          vl = eval(fu);
          return vl;
        } catch (e) {
          console.log(e);
        }
      });
      //console.log(objFun);
      //html = html.replace(/height="\d+"/igm, "");
      html = html.replace(/<div style="float:right;?">(.+)<\/div>/gim, "$1");
      html = html.replaceAll("{{}}", "");
      html = html.replaceAll("os-span-edit-os", "");
      html = html.replaceAll("os-delete-os", "");
      html = html.replaceAll('style="-aw-import:ignore"', "");
      if (isxls.value) {
        html = html.replace(
          "<style>",
          `<style>tr, td {height: auto!important;}`
        );
      }
      dochtml.innerHTML = html;
      //Sum-data
      dochtml.querySelectorAll('[class^="sum-data-"]').forEach((div) => {
        let isum = div.className.replace("sum-data-", "").toString();
        if (isum == "0") {
          div.querySelectorAll("td").forEach((td, i) => {
            if (td.innerText.trim() == "0") {
              let tr = div.nextElementSibling;
              let sumvl = 0;
              while (tr && !tr.className.includes("sum-data-")) {
                let vl = tr.children[i].innerText
                  .replaceAll(".", "")
                  .replaceAll(",", "");
                if (vl && !isNaN(vl)) sumvl += parseInt(vl);
                tr = tr.nextElementSibling;
              }
              td.innerText = sumvl.toLocaleString();
              td.removeAttribute("x:fmla");
            }
          });
        } else if (isum == "2") {
          div.querySelectorAll("td").forEach((td, i) => {
            if (td.innerText.trim() == "0") {
              let tr = div.previousElementSibling;
              let sumvl = 0;
              while (tr) {
                if (tr.className.includes("sum-data-")) {
                  let vl = tr.children[i].innerText
                    .replaceAll(".", "")
                    .replaceAll(",", "");
                  if (vl && !isNaN(vl)) sumvl += parseInt(vl);
                }
                tr = tr.previousElementSibling;
              }
              td.innerText = sumvl.toLocaleString();
            }
            td.removeAttribute("x:fmla");
          });
        }
      });
      //Style Excel
      dochtml.querySelectorAll("[config]").forEach((div) => {
        let cg = div.getAttribute("config");
        if (cg) {
          let arrStyles = cg.split(";");
          let atype = arrStyles[0];
          div.style.backgroundColor = arrStyles[1];
          if (atype == 0) {
            div.querySelectorAll("td").forEach((td) => {
              td.style.color = arrStyles[2];
              td.style.backgroundColor = arrStyles[1];
            });
          } else {
            div.style.color = arrStyles[2];
          }
        }
      });
      html = dochtml.innerHTML;
      //eventDoc();
      addExcelStyle();
      if (f) {
        showLoadding.value = false;
        return false;
      }
      //
      html = html.replace(/title="[^"]+"/gim, "");
      html = html.replace(/<tr class="tablecell"[^</]*>(.*)<\/tr>/gim, "");
      html = html.replace(/<td class="tablecell"[^</]*>(\d+)<\/td>/gim, "");
      html = html.replace(/<col class="tablecell"[^</]*>/gim, "");
      showLoadding.value = true;
      let dataHtml = { html: html, filename: filename || "doc" };
      try {
        const axResponse = await axios.post(
          baseURL +
            //"api/Files/" +
            "/api/SRC/" +
            (isxls.value ? "ConvertFileXLSX" : "ConvertFile"),
          dataHtml,
          config
        );
        if (axResponse.status == 200) {
          // window.open(
          //   baseURL +
          //     "api/Files/downloadFile" +
          //     (isxls.value ? "XLS" : "") +
          //     "?name=" +
          //     filename +
          //     (isxls.value ? ".xlsx" : ".docx")
          // );
          if (axResponse.data.err == "0") {
            if (isxls.value) {
              downloadFileExport(
                "GetDownloadXLS",
                dataHtml.filename,
                axResponse.data.fileName + ".html",
                ".xlsx"
              );
            } else {
              downloadFileExport(
                "GetDownload",
                dataHtml.filename,
                axResponse.data.fileName + ".html",
                ".docx"
              );
            }
          }
          let sps = [];
          spans.value
            .filter((x) => x.historys.length > 0)
            .forEach((sp) => {
              sps.push({
                id: sp.id,
                version: sp.version,
                historys: sp.historys,
              });
            });
          if (sps.length > 0) download(filename + ".json", JSON.stringify(sps));
          //Tải HTML
          //download(filename + ".html", html);
        }
        showLoadding.value = false;
      } catch (e) {
        console.log(e);
      }
    };
    const downloadJsonData = () => {
      download(filename + "_data.json", JSON.stringify(datausers));
    };
    function randomData() {
      let chucvus = ["Giám đốc", "Trưởng phòng", "Bán hàng", "Nhân viên"];
      let hos = ["Nguyễn", "Trần", "Lê", "Hoàng", "Dương", "Lưu", "Trương"];
      let dems = ["Thị", "Văn", "Đức", "Hồng"];
      let tens = [
        "Trang",
        "Thu",
        "Dung",
        "Hương",
        "Hạnh",
        "Thuỷ",
        "Chung",
        "Long",
        "Thái",
        "Thắng",
        "Tùng",
        "Nguyệt",
      ];
      let arrs = [];
      let stt = getRandomInt(25, 50);
      for (let i = 0; i <= stt; i++) {
        let obj = {
          STT: i + 1,
          "Họ tên":
            getRandomArray(hos) +
            " " +
            getRandomArray(dems) +
            " " +
            getRandomArray(tens),
          "Chức vụ": getRandomArray(chucvus),
          "Lương chính": getRandomInt(4, 50, 1000000),
          "Trách nhiệm": getRandomInt(1, 5, 1000000),
          "Ăn trưa": getRandomInt(1, 2, 1000000),
          "Điện thoại": getRandomInt(1, 20, 100000),
          "Xăng xe": getRandomInt(3, 30, 100000),
          "Tổng thu nhập": 0,
          "Ngày công": getRandomInt(20, 26),
          "Lương đóng bảo hiểm": 0,
          "DN-KPCD": 0,
          "DN-BHXH": 0,
          "DN-BHYT": 0,
          "DN-BHTN": 0,
          "DN-Tong": 0,
          "NV-BHXH": 0,
          "NV-BHYT": 0,
          "NV-BHTN": 0,
          "NV-Tong": 0,
          "Thuế TNCN": 0,
          "Tạm ứng": 0,
          "Thực lĩnh": 0,
        };
        let luong =
          (obj["Lương chính"] +
            obj["Trách nhiệm"] +
            obj["Ăn trưa"] +
            obj["Xăng xe"]) *
          (obj["Ngày công"] / 26);
        obj["Thuế TNCN"] =
          luong > 30000000
            ? (luong * 20) / 100
            : luong > 15000000
            ? (luong * 10) / 100
            : 0;
        luong =
          luong -
          obj["Thuế TNCN"] -
          Math.ceil((obj["Lương đóng bảo hiểm"] * 10.5) / 100);
        obj["Tạm ứng"] = (getRandomInt(1, 100) * luong) / 100;
        if (obj["Tạm ứng"] > luong) obj["Tạm ứng"] = luong;
        arrs.push(obj);
      }
      return arrs;
    }
    //callbackFun
    const dataDB = ref([]);
    const rowStyle = (data) => {
      if (data.isconfig) {
        return { backgroundColor: "yellow" };
      }
    };
    let procchild = {};
    const dtTempCols = ref([]);
    const callbackFunChild = (dt, pr) => {
      let arr = [];

      dt.forEach((tr) => {
        tr.cols.forEach((td) => {
          let obj = {};
          obj.id = td.column_id;
          obj.name = td.column_title || td.column_id;
          obj.table_id = tr.table_id;
          obj.table_name = tr.table_name;
          arr.push(obj);
        });
      });
      dataDB.value = arr;
      dbrow.value.key = arr[0].id;
      if (/\[\d+\]/g.test(dbrow.value.value)) {
        dbrow.value.value = `[${arr[0].id}]`;
      }
      dtTempCols.value = dbrow.value.cols;
      if (!dtTempCols.value) {
        dtTempCols.value = objDataTemp.value[dbrow.value.tid || 0].cols;
      }
      if (dtTempCols.value) {
        let idx = dtTempCols.value.findIndex(
          (x) => x["value"] == dbrow.value["value"]
        );
        dbrow.value.isconfig = true;
        if (idx != dtTempCols.value.length - 1) {
          idx++;
          dbrow.value = dtTempCols.value[idx];
        } else {
          if (objDataTemp.value[dbrow.value.tid + 1])
            dbrow.value = objDataTemp.value[dbrow.value.tid + 1];
        }
        configDBChild.value.searchCols(dbrow.value["value"]);
      }
      if (pr) {
        proc = pr;
        procchild = { ...pr };
      }
      if (isxls.value) {
        let ir = dtExcels.value.findIndex(
          (x) => x.rowmnumber == dbrow.value.rowmnumber
        );
        if (ir != -1) {
          dtExcels.value[ir].cols = objDataTemp.value;
        }
      }
    };
    onMounted(() => {
      if (isUrlReport.value) {
        if (document.getElementById("app-header"))
          document.getElementById("app-header").classList.add("none");
        if (document.getElementById("app-body"))
          document.getElementById("app-body").classList.remove("p-2");
      }
      if (props.report) {
        initTemplate();
      }

      if (!dochtml)
        if (iframeDoc) dochtml = iframeDoc.getElementById("dochtml");
      users.forEach((u) => {
        itemusers.value.push({
          label: u.name,
          icon: "pi pi-user",
          command: () => {
            user.value = u;
          },
        });
      });
      if (!isUrlReport.value && !readonly.value) {
        if (iframeDoc)
          iframeDoc.addEventListener("selectionchange", () => {
            let txt = getSelectionText();
            if (txt != "") {
              selectText = txt;
            }
          });

        //
        initmutationObserver();
        //mutationObserver.disconnect();
      }
    });
    //Data cho Report
    const rpfilters = ref({
      global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    });
    const dtColumns = ref([]);
    const dtDataReports = ref([]);
    let objConfig = {};
    const initReportData = async (cg) => {
      if (cg) objConfig = cg;
      if (!objConfig.proc) {
        objConfig.proc = procchild;
      }
      if (!objConfig.proc.sql) {
        objConfig.proc.sql = props.report.proc_get;
      }
      if (!objConfig.proc.name) {
        objConfig.proc.name = props.report.proc_name;
      }

      dtDataReports.value = await goProc(
        objConfig.proc.issql,
        objConfig.proc.sql,
        []
      );

      if (dtDataReports.value.length > 0) {
        let obj = dtDataReports.value[0];
        dtColumns.value = Object.keys(obj).filter((x) => !x.includes("_"));
        expandedRowGroups.value = [obj.group];
        if (onedata) {
          onRowSelectReport(obj);
        }
      }
    };
    const rfReportData = async () => {
      let tselect = document.querySelector("tr.selected");
      if (tselect) tselect.classList.remove("selected");
      dochtml.innerHTML = tempHTMLGoc;
      await initReportData();
    };

    const goProc = async (query, name, par, f, o) => {
      let strSQL = {
        query: query,
        proc: name,
        par: par,
      };
      swal.fire({
        width: 110,
        didOpen: () => {
          swal.showLoading();
        },
      });

      const axResponse = await axios.post(
        baseURL + "/api/HRM_SQL/PostProc",
        {
          str: encr(JSON.stringify(strSQL), SecretKey, cryoptojs).toString(),
        },
        {
          headers: { Authorization: `Bearer ${store.getters.token}` },
        }
      );

      let dts = [];

      if (axResponse.status == 200) {
        if (axResponse.data.error) {
          //toast.error("Không mở được bản ghi");
        } else {
          dts = JSON.parse(axResponse.data.data);
          if (!f) {
            dts = dts[0];
          }
        }
      }

      swal.close();
      return dts;
    };
    const viewReport = async () => {
      if (isxls.value && dtExcels.value.length > 0) {
        let pas = [];
        objConfig.proc.parameters.forEach((pa) => {
          pas.push({
            par: pa.Parameter_name,
            va: "",
          });
        });
        datausers = [{ dts: dtDataReports.value }];
        dtExcels.value
          .filter((x) => x.isfor)
          .forEach((dtr) => {
            let tr = dochtml.querySelector(
              "tbody>tr:nth-child(" + (dtr.rowmnumber + 1) + ")"
            );
            if (tr) {
              datausers[0].dts.forEach((r, i) => {
                let trclone = tr.cloneNode(true);
                trclone.querySelectorAll("td").forEach((td) => {
                  if (td.className.includes("tablecell")) {
                    td.innerText = dtr.rowmnumber + i;
                    td.setAttribute("title", td.innerText);
                  } else {
                    if (td.innerText == 1) {
                      td.innerText = i + 1;
                    } else if (td.innerText) {
                      let ok = dtr.cols.find(
                        (x) => x.colname == td.getAttribute("title")
                      );
                      if (ok) {
                        td.innerText = r[ok.key] || "";
                      }
                    }
                    td.setAttribute(
                      "title",
                      td
                        .getAttribute("title")
                        .replace(dtr.rowmnumber, dtr.rowmnumber + i)
                    );
                  }
                });
                tr.parentNode.appendChild(trclone);
              });
              tr.remove();
            }
          });
        //dochtml.innerHTML = html;
      } else {
        if (readonly.value) {
          downloadFile();
        } else downloadFile(!isUrlReport.value);
      }
    };
    let cacheobjDataTemp = [];
    async function renderTableWord(objpar) {
       
      let pas = [];
      
      if (cacheobjDataTemp.length == 0) {
        cacheobjDataTemp = JSON.parse(JSON.stringify(objDataTemp.value));
      } else {
        objDataTemp.value = JSON.parse(JSON.stringify(cacheobjDataTemp));
      }
      
      if (objConfig.proc.parameters)
        objConfig.proc.parameters.forEach((pa) => {
          pas.push({
            par: pa.Parameter_name,
            va: objpar[pa.Parameter_name.replace("@", "")],
          });
        });

      let dts = await goProc(false, objConfig.proc.name, pas, true);
       
      //init với kiểu lưu
      let tbs = [];
      if (dts[0][0].is_data) {
         
        dts[0][0].is_data = dts[0][0].is_data.replaceAll("\\n", "");

        try {
          tbs = JSON.parse(dts[0][0].is_data);
           
          objDataTemp.value.forEach((ot, i) => {
            let tb = tbs[i];
            ot.cols.forEach((co) => {
              let vl = tb[co.value] || tb[co.key];
              if (vl) {
                if (co.inputtype == "Currency") {
                  vl = parseFloat(vl).toLocaleString();
                }
                if (co.value == co.value.toString().toLowerCase()) {
                  co.key = vl.toLocaleLowerCase();
                } else if (co.value == co.value.toString().toUpperCase()) {
                  co.key = vl.toUpperCase();
                } else {
                  co.key = vl;
                }
                co.key = co.key ? "=" + co.key : co.value;
              }
            });
          });
           
        } catch (e) {}
      }

        
      //
      datausers = dts;
      dochtml.innerHTML = tempHTMLGoc;
 
      dochtml
        .querySelectorAll('[style*="background-color:#ffff00"]')
        .forEach((el) => {
          if (
            !(
              el.closest("p") &&
              el.closest("p").id &&
              el.closest("p").id.includes("data")
            )
          ) {
            let pa = el.parentElement;
            el.outerHTML = el.outerHTML.replace(
              /<[^<]+background-color:#ffff00[^>]+>([^<]+)<\/[^>]+>/gim,
              function (s, ke) {
                let obj = objDataTemp.value[0].cols.find((x) => x.value == ke);
                let k = obj ? obj.key : ke;
                 
                // if (k == "Số người phụ thuộc") {
                //     console.log(dts[0][0][k]);
                // }
                try {
                  if (k.toString().startsWith("=")) {
                    k = k.toString().substring(1);
                  } else
                    k =
                      dts[0][0][k] ||
                      dts[0][0][k.replace("[", "").replace("]", "")];
                  if (k == null) k = "";
                } catch (e) {
                  console.log(e);
                }
                let vl = k.toString().replaceAll("\n", "<br/>");
                if (ke == ke.toLowerCase()) {
                  vl = vl.toLowerCase();
                } else if (ke == ke.toUpperCase()) {
                  vl = vl.toUpperCase();
                } else if (
                  ke.substring(0, 1) == ke.substring(0, 1).toUpperCase()
                ) {
                  vl = capitalizeFirstLetter(vl);
                }
                return s
                  .replaceAll(ke, vl)
                  .replaceAll("background-color:#ffff00", "");
              }
            );
            let txt = pa ? pa.innerText.trim() : "";
            //let rms = ["ngày", "người", "đồng"]
            let rms = ["đồng"];
            if (rms.findIndex((x) => x == txt || "0 " + x == txt) != -1) {
              pa.innerText = "";
            }
          }
        });
      dochtml.querySelectorAll("table[class*=data-]").forEach((tb) => {
        tb.querySelectorAll("td").forEach((td) => {
          td.style.backgroundColor = "";
        });
        let doc = /\[.+\]/g.test(
          tb.querySelector("tr").querySelector("td:last-child").innerText
        );
        let otb = objDataTemp.value.find(
          (x) => x.value == tb.className.replace("table ", "")
        );
        if (otb) {
          let data = dts[otb.key];
          if (!doc) {
            let tr = tb.querySelector("tr:last-child");
            data.forEach((r) => {
              let trclone = tr.cloneNode(true);
              trclone.querySelectorAll("td").forEach((td) => {
                let ok = otb.cols.find((x) => x.value == td.innerText.trim());
                if (ok) {
                  td.innerText = r[ok.key] || "";
                  td.style.backgroundColor = "";
                }
              });
              tr.parentElement.appendChild(trclone);
            });
            tr.remove();
          } else {
            //tb.style.width = "100%";
            tb.querySelectorAll("tr").forEach((tr) => {
              let td = tr.querySelector("td:last-child");
              data.forEach((r) => {
                let tdclone = td.cloneNode(true);
                tdclone.style.width = "auto";
                let ok = otb.cols.find(
                  (x) => x.value == tdclone.innerText.trim()
                );
                if (ok) {
                  td.style.backgroundColor = "";
                  tdclone.innerHTML = td.innerHTML.replace(
                    tdclone.innerText.trim(),
                    r[ok.key] || ""
                  );
                }
                tr.appendChild(tdclone);
              });
              td.remove();
            });
          }
        }
      });
      dochtml.querySelectorAll("p[id*=data-]").forEach((tb) => {
        let data = tbs[tb.id.replace("data-", "")];
        data.rows.forEach((r) => {
          let p = tb.cloneNode(true);
          let phtml = tb.innerHTML.replace(/\[[^\[\]]+\]/gim, function (su) {
            return r[su.trim()] || "";
          });
          phtml = phtml
            .replaceAll("[", "")
            .replaceAll("]", "")
            .replaceAll("background-color:#ffff00", "");
          p.innerHTML = phtml;
          tb.parentElement.insertBefore(p, tb);
        });
        tb.remove();
      });
    }
    const initURLReport = async () => {
      objConfig = JSON.parse(props.report.report_config);

      //Xử lý for Word
      renderTableWord(props.pars);
    };
    let selectdata = {};
    const onRowSelectReport = async (event) => {
       
      let tselect = document.querySelector("tr.selected");
      if (tselect) tselect.classList.remove("selected");
      if (event.originalEvent)
        event.originalEvent.target.closest("tr").classList.add("selected");
      await renderTableWord(event.data || event);
      await initProfile(event.data || event);
      if (selectdata) {
        if (selectdata.profile_id == (event.data || event).profile_id) {
          isCopy.value = !isCopy.value;
        } else selectdata = event.data || event;
      }
      if (isxls.value) {
        renderDataExcel();
      }
    };
    const dequyfun = (fun) => {
      if (fun.startsWith("=")) fun = fun.substring(1);
      fun = fun.replace(/([a-zA-Z]+\d+)/g, function (su) {
        let col = dtExcels.value.find((x) => x.value == su);
        if (col) {
          return col.key;
        }
        let cl = dochtml.querySelector(`td[title=${su}]`);
        let fun1 = cl.getAttribute("x:fmla");
        if (fun1) {
          su = dequyfun(fun1);
        }
        return su;
      });
      return fun;
    };
    const bindValueExcel = (tdtemp, td, fun, isfun) => {
      td.style.backgroundColor = "unset";
      let vl = fun.replace(/(\[[^\]]+\])/gim, function (su) {
        let tb = tdtemp.find((x) => x.value == su);
        if (tb) {
          let k = tb.key;
          if (k && k.startsWith("=")) {
            k = k.substring(1);
          }
          return k;
          //return k.replaceAll(".", "").replaceAll(",", "");
        } else return su;
      });
      if (isfun) {
        if (vl.startsWith("IF")) {
          vl = vl.substring(2).split(",");
          vl = `(${vl[0]})?${vl[1]}:${vl[2]}`;
        }
        try {
          vl = eval(vl);
          if (!isNaN(vl)) {
            vl = parseFloat(vl).toLocaleString();
          }
          td.innerText = vl;
        } catch (e) {
          td.innerText = "";
        }
      } else {
        if (!isNaN(vl)) {
          vl = parseFloat(vl).toLocaleString();
        }
        td.innerText = vl;
      }
    };
    const trimTagValue = (tv) => {
      return tv.replaceAll("[", "").replaceAll("]", "");
    };
    const renderDataExcel = () => {
      let tdtemp = objDataTemp.value[0].cols;
      dochtml.querySelectorAll("td").forEach((td) => {
        if (/\[(.+)\]/gim.test(td.innerText)) {
          var css = window.getComputedStyle(td);
          if (css.getPropertyValue("background-color") == "rgb(255, 255, 0)") {
            bindValueExcel(tdtemp, td, td.innerText, false);
          } else if (
            css.getPropertyValue("background-color") == "rgb(255, 192, 0)"
          ) {
            td.innerText =
              dtUser[td.innerText] ||
              (dtProfile && dtProfile[trimTagValue(td.innerText)]) ||
              td.innerText;
            if (td.innerText.includes("http")) {
              let hh = td.parentElement.getAttribute("height");
              let ww = td.getAttribute("width");
              td.innerHTML = `<img width="${ww}" height="${hh}" style="border-radius:50%;object-fit: contain;" src="${td.innerText}"/>`;
            }
            td.style.backgroundColor = "unset";
          }
        }
        if (td.getAttribute("x:fmla")) {
          //Nếu có công thức
          let fun = td.getAttribute("x:fmla");
          fun = dequyfun(fun);
          bindValueExcel(tdtemp, td, fun, true);
        }
      });
    };
    //Cấu hình data cho Template
    const onCellEditComplete = (event) => {
      try {
        let { data, newValue, field } = event;
        switch (field) {
          default:
            if (newValue && newValue.trim().length > 0) data[field] = newValue;
            else event.preventDefault();
            break;
        }
      } catch (e) {
        event.preventDefault();
      }
    };
    //Cấu hình database cho excel

    

    const expandedRows = ref([]);
    const dtExcels = ref([]);
    const dtExcelCols = ref([]);
    const dtExcelRows = ref([]);
    const initdbXLS = () => {
      dochtml.querySelectorAll("tr.tablecell>td.thcell").forEach((th, i) => {
        if (th.innerText.trim() != "") {
          dtExcelCols.value.push({
            id: i,
            name: th.innerText.trim(),
          });
        }
      });
      dochtml.querySelectorAll("tbody>tr").forEach((tr, i) => {
        if (tr.innerText.trim() != "") {
          dtExcelRows.value.push(i + 1);
        }
      });
      //dtExcels.value objDataTemp
      var ths = [...dochtml.querySelectorAll("td")].filter(
        (x) =>
          window.getComputedStyle(x).getPropertyValue("background-color") ==
          "rgb(255, 192, 0)"
      );
      dochtml.querySelectorAll("td").forEach((td, i) => {
        if (/\[(.+)\]/gim.test(td.innerText)) {
          var css = window.getComputedStyle(td);
          if (
            css.getPropertyValue("background-color") == "rgb(255, 255, 0)" ||
            css.getPropertyValue("background-color") == "rgb(255, 192, 0)"
          ) {
            let mts = td.innerText.match(/\[[^\]]+\]/gim);
            let iauto =
              css.getPropertyValue("background-color") == "rgb(255, 192, 0)";
            if (iauto) {
              td.innerText = dtUser[td.innerText] || td.innerText;
              if (td.innerText.includes("http")) {
                let hh = td.parentElement.getAttribute("height");
                let ww = td.getAttribute("width");
                td.innerHTML = `<img width="${ww}" height="${hh}" style="border-radius:50%;object-fit: contain;" src="${td.innerText}"/>`;
              }
            }
            if (mts) {
              mts.forEach((el) => {
                addRowXls(td, el != td.innerText ? el : null, iauto);
              });
            }
          }
        } else {
          var css = window.getComputedStyle(td);
          if (css.getPropertyValue("background-color") == "rgb(255, 255, 0)") {
            let xn = td.getAttribute("title").replace(/\d/g, "");
            let th = ths.find(
              (x) =>
                x.getAttribute("title").replace(/\d/g, "") == xn &&
                !x.getAttribute("colspan")
            );
            if (th) {
              let tn = th.innerText;
              if (th.getAttribute("tname")) {
                tn = th.getAttribute("tname") + " (" + tn + ")";
              }

              addRowXls(td, tn, false, true);
            }
          }
        }
      });
      objDataTemp.value = [];
      let obj = {
        key: 0,
        cols: [],
      };
      dtExcels.value
        .filter((x) => !x.isfor && !x.auto)
        .forEach((ex) => {
          obj.cols.push({
            key: "",
            value: ex.key,
            inputtype: ex.inputtype,
          });
        });
      objDataTemp.value.push(obj);
      dtExcels.value
        .filter((x) => x.isfor && !x.auto)
        .forEach((ex, i) => {
          obj = {
            key: i + 1,
            cols: [],
            inputtype: ex.inputtype,
          };
          if (ex.cols)
            ex.cols.forEach((co) => {
              obj.cols.push({
                key: "",
                value: co.key,
                inputtype: ex.inputtype,
              });
            });
          objDataTemp.value.push(obj);
        });
    };
    const addRowXls = (td, el, iauto, f) => {
      let obj = { isfor: td.tagName == "TR" };
      if (obj.isfor) {
        obj.rowmnumber = parseInt(td.children[0].innerText);
        obj.colname = `${td.children[1].getAttribute("title")}${td.children[
          td.children.length - 1
        ].getAttribute("title")}`;
        obj.cols = [];
        for (let i = 1; i <= td.children.length - 1; i++) {
          let ctd = td.children[i];
          let otd = {};
          otd.rowmnumber = obj.rowmnumber;
          otd.colname = ctd.getAttribute("title");
          otd.value = otd.colname;
          otd.key = el || ctd.innerText;
          otd.auto = iauto;
          otd.colnumber = colNumber(otd.colname);
          obj.cols.push(otd);
        }
        let i = dtExcels.value.findIndex((x) => x.colname == obj.colname);
        if (i == -1) dtExcels.value.push(obj);
      } else {
        if (f) {
          obj.isfortd = true;
        }
        obj.rowmnumber = parseInt(td.parentElement.children[0].innerText);
        obj.colname = td.getAttribute("title");
        obj.value = obj.colname;
        obj.auto = iauto;
        obj.key = el || td.innerText;
        obj.colnumber = colNumber(obj.colname);
        //let ir = dtExcels.value.findIndex(x => x.rowmnumber == obj.rowmnumber);
        let i = dtExcels.value.findIndex(
          (x) => x.colname == obj.colname && x.key == obj.key
        );
        // if (ir != -1) {
        //     if (!dtExcels.value[ir].cols) dtExcels.value[ir].cols = [];
        //     i = dtExcels.value[ir].cols.findIndex(x => x.colname == obj.colname);
        //     if (i == -1) {
        //         dtExcels.value[ir].cols.push(obj);
        //     }
        // } else {
        if (i == -1) {
          dtExcels.value.push(obj);
        }
        //}
      }
    };
    const delRowXLS = (r, c) => {
      if (isxls.value) {
        let i = dtExcels.value.indexOf(r);
        if (i != -1) {
          dtExcels.value.splice(i, 1);
        }
      } else {
        if (c) {
          let i = r.data.cols.indexOf(c);
          if (i != -1) {
            r.data.cols.splice(i, 1);
          }
        } else {
          let i = objDataTemp.value.indexOf(r);
          if (i != -1) {
            objDataTemp.value.splice(i, 1);
          }
        }
      }
    };
    //Compute
    const compuDatakeys = computed(() =>
      mapdatakeys.filter(
        (x) =>
          Array.isArray(datausers[0][x.name]) || isObject(datausers[0][x.name])
      )
    );
    const compSpans = computed(() =>
      spans.value.filter(
        (x) =>
          (!filterUser.value.id && x.historys.length > 0) ||
          x.historys.filter((u) => u.user.id == filterUser.value.id).length > 0
      )
    );
    const compXLS = computed(() => {
      let ars = Object.keys(objExcel.value).filter(
        (x) =>
          objExcel.value[x].includes(txtIPXLS.value.toUpperCase()) ||
          objExcel.value[x].includes(txtIPXLS.value) ||
          x.includes(txtIPXLS.value) ||
          x.toLowerCase().includes(txtIPXLS.value)
      );
      ars = ars.sort((a, b) => {
        return (
          objExcel.value[a].substring(0, 1) - objExcel.value[b].substring(0, 1)
        );
      });
      return ars;
    });
    const expandedRowGroups = ref();
    const rowClass = (dt) => {
      return dt.cols ? "has-child" : "no-child";
    };
    const goBack = () => {
      if (header != null) {
        props.callbackFun();
      } else history.back();
    };
    //Cấu hình nguồn nhập dữ liệu
    const dtTables = ref([]);
    const addTable = () => {
      dtTables.value.push({
        index: dtTables.value.length,
        name: "Data " + dtTables.value.length,
      });
    };
    const removeTable = (tb) => {
      let i = dtTables.value.findIndex((x) => x.index == tb.index);
      if (i != -1) {
        dtTables.value.splice(i, 1);
      }
      dtTables.value.forEach((tb, i) => {
        tb.index = i;
      });
    };
    const objForm = ref({});
    const cForm = ref({});

    let dtProfile = {};
    const initProfile = async (r) => {
      let dts = await goProc(
        false,
        `profile_info`,
        [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "profile_id", va: r.profile_id },
        ],
        false,
        true
      );
      if (dts.length > 0) {
        dtProfile = dts[0];
        
      }
    };
    const oneRow = ref(true);
    const editDataAll = async (objvalue) => {
      if (props.report.is_config) {
        objDataTemp.value = props.report.is_config;
      }

      let dts = await goProc(true, props.report.proc_all, [], true);
      dtDataReports.value.forEach((dt) => {
        let tb = dts[0].find((x) => x.profile_id == dt.profile_id);
        dt.is_data = tb.is_data;
        let tbs = [];
        if (dt.is_data) {
          try {
            tbs = JSON.parse(dt.is_data);
          } catch (e) {}
        }
        dt.datatemp = {};
        if (tbs) {
          objDataTemp.value.forEach((ot, i) => {
            let t = tbs[i];
            ot.cols.forEach((co, i) => {
              dt.datatemp[co.value] = t
                ? t[co.value]
                : objvalue[co.value.replace(/\[(.+)\]/g, "$1")] ||
                  tb[co.value.replace(/\[(.+)\]/g, "$1")] ||
                  null;
            });
          });
        }
      });
    };
    function isNumeric(n) {
      return !isNaN(parseFloat(n)) && isFinite(n);
    }
    const addRowWord = (tb) => {
      if (!tb.rows) tb.rows = [];
      let r = {
        stt: tb.rows.length + 1,
      };
      tb.cols.forEach((co, i) => {
        r[co.value.trim()] = "";
      });
      tb.rows.push(r);
    };
    const renderTypeInput = (co) => {
      let vl = co.key;
      if (
        co.value.toLowerCase().includes("tiền") ||
        co.value.toLowerCase().includes("ngày công") ||
        co.value.toLowerCase().includes("thu hộ") ||
        co.value.toLowerCase().includes("thực lĩnh") ||
        co.value.toLowerCase().includes("thực nhận") ||
        co.value.toLowerCase().includes("thu nhập") ||
        co.value.toLowerCase().includes("thuế") ||
        co.value.toLowerCase().includes("vnd")
      ) {
        return "Currency";
      }
      if (vl && !isNaN(vl)) {
        if (!co.value.toString().toLowerCase().includes("năm")) {
          return "Currency";
        } else {
          return "Number";
        }
      }
      return "Text";
    };
    const editForm = async (r, rcopy) => {
     
      if (props.report.is_config) {
        objDataTemp.value = props.report.is_config;
      }
      
      objDataTemp.value.forEach((ot) => {
        ot.cols.forEach((co) => {
          if (co.inputtype == "Number" || co.inputtype == "Currency") {
            if (!isNumeric(co.key)) {
              co.key = null;
            }
          }
        });
         
        if (ot.rows) {
          ot.rows.forEach((r) => {
            if (r.cols)
              r.cols.forEach((co) => {
                if (co.inputtype == "Number" || co.inputtype == "Currency") {
                  if (!isNumeric(co.key)) {
                    co.key = 0;
                  }
                }
              });
          });
          ot.users = ot.rows.map((x) => x.profile_id).join(",");
        }
      });
      oneRow.value = r ? true : false;
       
      isfullSidebar.value = !oneRow.value;
      if (!r) {
        r = cForm.value;
      }
      let pas = [];
      objConfig.proc.parameters.forEach((pa) => {
        pas.push({
          par: pa.Parameter_name,
          va: r[pa.Parameter_name.replace("@", "")],
        });
      });

      let dts = await goProc(false, objConfig.proc.name, pas, true);
      objForm.value = dts[0][0];
      cForm.value = r;
      if (rcopy) {
        let pas = [];
        objConfig.proc.parameters.forEach((pa) => {
          pas.push({
            par: pa.Parameter_name,
            va: rcopy[pa.Parameter_name.replace("@", "")],
          });
        });

        let dts = await goProc(false, objConfig.proc.name, pas, true);
        if (dts.length > 0) {
          objForm.value.is_data = dts[0][0].is_data;
        }
      }
      itemtypeInputs.value = [];
      //itemtypeInputs.value = itemtypeInputs.value.filter(x => !x.items);
      opTypeDB.value = optionTypeDBs.value[2];
      isdataSidebar.value = true;

      dts = await goProc(
        false,
        `profile_info`,
        [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "profile_id", va: r.profile_id },
        ],
        true,
        true
      );
      if (dts.length > 0) {
        dtProfile = dts[0][0];
      }
      let objvalue = {};
      props.report.datadic.forEach((dic) => {
        let obj = {
          label: dic.title,
          icon: "pi pi-database",
          items: [],
        };
        Object.keys(dic.data).forEach((k) => {
          objvalue[k] = dic.data[k];
          if (!k.includes("_")) {
            let obj1 = {
              label: k,
              key: dic.data[k],
            };
            obj.items.push(obj1);
          }
        });
        itemtypeInputs.value.push(obj);
      });
      dts.forEach((dic, i) => {
        dic = dic[0];
        let obj = {
          label: i == 0 ? "Nhân sự" : "Người dùng",
          icon: "pi pi-user",
          items: [],
        };
        if (dic) {
          Object.keys(dic).forEach((k) => {
            objvalue[k] = dic[k];
            if (!k.includes("_")) {
              let obj1 = {
                label: k,
                key: dic[k],
              };
              obj.items.push(obj1);
            }
          });
        }
        itemtypeInputs.value.push(obj);
      });
      let objSystem = {
        "Ngày hiện tại": utilformatDate(new Date(), "dd"),
        "Tháng hiện tại": utilformatDate(new Date(), "MM"),
        "Năm hiện tại": utilformatDate(new Date(), "yyyy"),
        "Giờ hiện tại": utilformatDate(new Date(), "HH"),
        "Phút hiện tại": utilformatDate(new Date(), "mm"),
        "Ngày tháng năm hiện tại": utilformatDate(new Date(), "dd/MM/yyyy"),
        "Ngày giờ hiện tại": utilformatDate(new Date(), "dd/MM/yyyy HH:mm:ss"),
      };
      let obj = {
        label: "Hệ thống",
        icon: "pi pi-cog",
        items: [],
      };
      Object.keys(objSystem).forEach((k) => {
        objvalue[k] = objSystem[k];
        if (!k.includes("_")) {
          let obj1 = {
            label: k,
            key: objSystem[k],
          };
          obj.items.push(obj1);
        }
      });
      //initData
      let tbs = [];
      if (objForm.value && objForm.value.is_data) {
        try {
          tbs = JSON.parse(objForm.value.is_data);
        } catch (e) {
          tbs = [];
        }
      }
      objDataTemp.value.forEach((ot, i) => {
        let tb = tbs.length > i ? tbs[i] : null;
        ot.cols.forEach((co) => {
          co.key = tb
            ? tb[co.value]
            : objvalue[co.value.replace(/\[(.+)\]/g, "$1")];
          if (!co.inputtype) {
            co.inputtype = renderTypeInput(co);
          }
          if (co.inputtype == "Number" || co.inputtype == "Currency") {
            if (!isNumeric(co.key)) {
              co.key = null;
            } else if (co.key) {
              co.key = parseFloat(co.key.toString());
            }
          }
        });
        if (tb && tb.rows) {
          ot.rows = tb.rows;
          ot.table = true;
        }
      });
      itemtypeInputs.value.push(obj);
      if (!oneRow.value) {
        editDataAll(objvalue);
      }
    };
    let checkHide=false;
    const onHideSidebarM=()=>{
      if(!checkHide)
      objDataTemp.value=[...objDataTempSave.value];

    }
    const optionTypeDBs = ref([
      { name: "Lấy dữ liệu động từ hệ thống", value: 1 },
      { name: "Tải dữ liệu dưới máy", value: 2 },
      { name: "Nhập dữ liệu mới", value: 3 },
    ]);
    const opTypeDB = ref(optionTypeDBs.value[0]);
    //const menuInput = ref();
    const rowTemp = ref();
    const isTypeInputs = [
      { icon: "pi pi-pencil", lable: "Text", value: "Text" },
      { icon: "pi pi-sort-numeric-down", lable: "Số", value: "Number" },
      { icon: "pi pi-calendar-minus", lable: "Ngày", value: "Date" },
      { icon: "pi pi-calendar-plus", lable: "Ngày giờ", value: "DateTime" },
      { icon: "pi pi-clock", lable: "Giờ", value: "Hour" },
      { icon: "pi pi-dollar", lable: "Tiền", value: "Currency" },
      { icon: "pi pi-users", lable: "Chọn nhân sự", value: "user" },
    ];
    const itemtypeInputs = ref([]);
    const isCopy = ref(false);
    let rowcopy = {};
    const copyRow = async () => {
      if (Object.keys(selectdata).length == 0) {
        toast.error("Vui lòng chọn dòng cần Copy");
        return false;
      }
      isCopy.value = !isCopy.value;
      if (isCopy.value) {
        rowcopy = selectdata;
      } else {
        await editForm(selectdata, rowcopy);
        rowcopy = {};
      }
    };
    const changeDiv = (ev, r, k) => {
      r.datatemp[k] = ev.target.textContent;
    };
    const changeInputType = (r) => {
      r.icon = isTypeInputs.find((x) => x.value == r.inputtype).icon;
    };
    const changeDrForm = (ev, r) => {
      if (ev.value.key) r.key = ev.value.key;
    };
    const onDateInput = (event) => {
      // Remove non-numeric characters from the input
      const cleanedInput = event.target.value.replace(/\D/g, "");
      // Format the input as a date (DD/MM/YYYY)
      if (cleanedInput.length <= 2) {
        event.target.value = cleanedInput;
      } else if (cleanedInput.length <= 4) {
        event.target.value =
          cleanedInput.slice(0, 2) + "/" + cleanedInput.slice(2);
      } else {
        event.target.value =
          cleanedInput.slice(0, 2) +
          "/" +
          cleanedInput.slice(2, 4) +
          "/" +
          cleanedInput.slice(4, 8);
      }
    };
    return {
      isedit,
      onedata,
      onDateInput,
      changeDrForm,
      changeInputType,
      isTypeInputs,
      changeDiv,
      isCopy,
      copyRow,
      itemtypeInputs,
      //toggleMenuInput,
      //menuInput,
      onHideSidebarM,
      addRowWord,
      editForm,
      objForm,
      opTypeDB,
      optionTypeDBs,
      addTable,
      removeTable,
      dtTables,
      cForm,
      //
      goBack,
      expandedRowGroups,
      rowClass,
      dtTempCols,
      //dtExcel
      bgColor,
      expandedRows,
      dtExcelRows,
      delRowXLS,
      dtExcels,
      addRowXls,
      isUrlReport,
      isViewReport,
      isReportListView,
      viewReport,
      dtDataReports,
      dtColumns,
      onRowSelectReport,
      rpfilters,
      rfReportData,
      //refChild
      configDBChild,
      report,
      htmlElement,
      ipsearch,
      isdoc,
      isxls,
      currentText,
      drcurrentText,
      displayElement,
      openHTML,
      removeTags,
      user,
      compuDatakeys,
      parseHTMLText,
      execStyle,
      pureColor,
      gradientColor,
      onRightClick,
      menu,
      datakeys,
      itemusers,
      elementSpan,
      downloadFile,
      editSpan,
      isHasHTML,
      compSpans,
      showLoadding,
      displayCK,
      isComment,
      spanEditor,
      setCurrentText,
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
      mdDataHeader,
      downloadTemplateFile,
      refershHTML,
      clearSelect,
      hversion,
      printDiv,
      isDesingData,
      actionMenus,
      isdataSidebar,
      isfullSidebar,
      showDataSidebar,
      datamaps,
      saveDatamap,
      downloadJsonData,
      addRow,
      delRow,
      refersMapData,
      importJsonData,
      isFormat,
      isTypeInput,
      arrayKeys,
      isfull,
      divZoom,
      toogledivZoom,
      toolCellLine,
      displayEditData,
      saveEditData,
      clickConfigData,
      textEdit,
      textEditExcel,
      changeInputExcel,
      objExcel,
      txtIPXLS,
      compXLS,
      itemButExports,
      objDataTemp,
      //ConfigDatabase
      callbackFunChild,
      openCogDatabase,
      isDisplayDatabase,
      saveDatabase,
      dbrow,
      nextDBrow,
      saveConfig,
      refershConfig,

      //
      onCellEditComplete,
      dataDB,
      IsOne,
      rowStyle,
      readonly,
      oneRow,
      getCompUsers,
      rfUserComp,
      selectedStamps
    };
  },
};
</script>
<template>
  <div class="d-design-doc">
    <div :class="'   flex mb-' + (isUrlReport ? 0 : 1)">
      <div
        class="tool flex-1 flex"
        v-if="isHasHTML && !isUrlReport && !readonly"
      >
        <Button
          v-tooltip="'Chữ thường'"
          @click="execStyle('N')"
          label="Normal"
          class="p-button-secondary p-button-outlined ml-1"
        />
        <Button
          v-tooltip="'Chữ đậm'"
          @click="execStyle('B')"
          label="B"
          class="p-button-secondary p-button-outlined ml-1"
        />
        <Button
          v-tooltip="'Chữ nghiêng'"
          @click="execStyle('I')"
          label="I"
          class="p-button-secondary p-button-outlined ml-1"
        />
        <Button
          v-tooltip="'Chữ gạch chân'"
          @click="execStyle('U')"
          label="U"
          class="p-button-secondary p-button-outlined ml-1"
        />
        <Button
          v-tooltip="'Viết hoa'"
          @click="execStyle('AA')"
          label="AA"
          class="p-button-secondary p-button-outlined ml-1"
        />
        <Button
          v-tooltip="'Viết hoa chữ đầu'"
          @click="execStyle('Aa')"
          label="Aa"
          class="p-button-secondary p-button-outlined ml-1"
        />
        <Button
          v-tooltip="'Viết thường'"
          @click="execStyle('aa')"
          label="aa"
          class="p-button-secondary p-button-outlined ml-1"
        />
        <Button
          v-tooltip="'Chọn màu chữ'"
          class="p-button-secondary p-button-outlined ml-1"
        >
          <colorpickers
            style="width: 24px"
            v-model:gradientColor="gradientColor"
            pickerType="chrome"
            @pureColorChange="execStyle"
            v-model:pureColor="pureColor"
          />
        </Button>
        <Button
          v-if="!isViewReport"
          @click="clearSelect"
          icon="pi pi-refresh"
          class="p-button-outlined p-button-secondary ml-1"
        />
        <div class="flex-1"></div>
        <!-- <Dropdown v-if="!isViewReport" :filterFields="['id', 'name']" @change="goKey($event.value.name)" class="ml-1"
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    optionLabel="name" :options="mapdatakeys" :filter="true" placeholder="Data" :showClear="true">
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    <template #option="slotProps">
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        <div>{{ slotProps.option.name }}</div>
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    </template>
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                </Dropdown> -->
        <!-- <Button v-if="!isViewReport" @click="setSelect(true)" label="Dòng bắt đầu"
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    :class="(isstart ? '' : 'p-button-secondary p-button-outlined') + ' ml-1'" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    :class="(isend ? '' : 'p-button-secondary p-button-outlined') + ' ml-1'" /> -->
        <ToggleButton
          v-if="!isViewReport && isedit"
          v-model="isdoc"
          onLabel=""
          offLabel=""
          onIcon="pi pi-arrows-v"
          offIcon="pi pi-arrows-h"
          class="ml-1"
        />
        <!-- <ToggleButton v-if="!isViewReport" v-model="isDesingData" onLabel="Data" offLabel="Design" onIcon="pi pi-pencil"
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    offIcon="pi pi-eye" class="ml-1" /> -->
        <Button
          v-if="!isViewReport"
          v-tooltip="'Dữ liệu'"
          @click="showDataSidebar"
          icon="pi pi-cog"
          class="p-button-secondary p-button-outlined ml-1"
        />
        <Button
          @click="toogledivZoom(false)"
          icon="pi pi-search-minus"
          class="p-button-secondary p-button-outlined ml-1"
        />
        <Button
          @click="divZoom = 1"
          :label="parseInt(divZoom * 100) + '%'"
          class="p-button-secondary p-button-outlined ml-1"
        />
        <Button
          @click="toogledivZoom(true)"
          icon="pi pi-search-plus"
          class="p-button-secondary p-button-outlined ml-1"
        />
        <Button
          v-tooltip="'Full'"
          @click="isfull = !isfull"
          :icon="'pi pi-window-' + (isfull ? 'minimize' : 'maximize')"
          class="p-button-secondary p-button-outlined ml-1"
        />
        <Button
          v-if="isxls"
          v-tooltip="'Hiển thị line Excel'"
          @click="toolCellLine"
          icon="pi pi-check-square"
          class="p-button-secondary p-button-outlined ml-1"
        />
        <ToggleButton
          v-model="isViewReport"
          onIcon="pi pi-pencil"
          offLabel=""
          onLabel=""
          offIcon="pi pi-eye"
          class="ml-1 mr-2"
        />
      </div>
      <div class="tool flex w-full p-2" v-if="isUrlReport || readonly">
        <Button
          v-if="!readonly"
          v-tooltip="'Quay lại trang trước'"
          @click="goBack()"
          icon="pi pi-angle-left"
          class="p-button-secondary p-button-outlined mr-1"
        />
        <div class="flex-1">
          <h3 v-if="!readonly" class="p-2 m-0">
            <i class="mr-1 pi pi-print"></i>
            <span v-if="header"> {{ header }} ({{ report.report_name }}) </span>
            <span v-else>
              {{ report.report_name }}
            </span>
          </h3>
        </div>
        <ToggleButton
          v-if="isedit"
          v-model="isdoc"
          onLabel=""
          offLabel=""
          onIcon="pi pi-arrows-v"
          offIcon="pi pi-arrows-h"
          class="ml-1"
        />
        <Button
          @click="toogledivZoom(false)"
          icon="pi pi-search-minus"
          class="p-button-secondary p-button-outlined ml-1"
        />
        <Button
          @click="divZoom = 1"
          :label="parseInt(divZoom * 100) + '%'"
          class="p-button-secondary p-button-outlined ml-1"
        />
        <Button
          @click="toogledivZoom(true)"
          icon="pi pi-search-plus"
          class="p-button-secondary p-button-outlined ml-1"
        />
        <Button
          v-if="isxls"
          v-tooltip="'Full'"
          @click="isfull = !isfull"
          :icon="'pi pi-window-' + (isfull ? 'minimize' : 'maximize')"
          class="p-button-secondary p-button-outlined ml-1"
        />
        <Button
          v-if="isxls"
          v-tooltip="'Hiển thị line Excel'"
          @click="toolCellLine"
          icon="pi pi-check-square"
          class="p-button-secondary p-button-outlined ml-1"
        />
        <SplitButton
          class="ml-1"
          @click="viewReport()"
          icon="pi pi-download"
          label=""
          :model="itemButExports"
        />
      </div>
      <SplitButton
        v-if="!isUrlReport && !readonly"
        @click="viewReport()"
        icon="pi pi-eye"
        label="Xem"
        :model="itemButExports"
      />
      <ProgressSpinner
        v-if="showLoadding"
        style="width: 50px; height: 50px"
        strokeWidth="8"
        fill="var(--surface-ground)"
        animationDuration=".5s"
      />
      <!-- <Tag v-if="elementSpan.element && !isUrlReport && !readonly" class="ml-1" severity="warning"
                                                                                                                                                                                                                                                                                                                                                                                                    :value="'Version ' + hversion">
                                                                                                                                                                                                                                                                                                                                                                                                </Tag> -->
    </div>
    <Divider class="m-0 p-0" />
    <div class="flex" style="background-color: #eee">
      <iframe
        id="docIframe"
        src="about:blank"
        frameborder="0"
        :style="
          'background:#ccc;width: 100%;height:calc(100vh - ' +
          (isUrlReport ? 60 : 130) +
          'px)'
        "
      ></iframe>
      <div
        id="divleft-frame"
        :class="isxls ? 'div-excel' : ''"
        style="
          overflow-y: auto;

          padding: 20px;
          background-color: #ccc;
        "
        :style="
          header == null
            ? ' max-height: calc(100% - 40px);'
            : ' max-height: calc(100% - 200px);'
        "
      >
        <div
          class="doc-page card shadow-1"
          :style="
            'zoom:' +
            divZoom +
            ';padding:' +
            (isxls ? '0' : '') +
            ';width:' +
            (isxls ? 'fit-content;min-width:21cm' : isdoc ? '21cm' : '29.7cm')
          "
        >
          <div
            :contenteditable="!isUrlReport && !readonly"
            spellcheck="false"
            id="dochtml"
            @contextmenu="onRightClick"
          ></div>
        </div>
      </div>
      <!--|| !readonly || dtDataReports.length > 0-->
      <div
        class="flex-1 bg-white"
        v-if="
          (!isUrlReport || isReportListView) && isHasHTML && !isfull && isedit
        "
        :style="
          'min-width:' + (readonly || isReportListView ? 500 : 640) + 'px;'
        "
      >
        <div v-if="isViewReport">
          <DataTable
            v-model:filters="rpfilters"
            @rowSelect="onRowSelectReport"
            selectionMode="single"
            :metaKeySelection="false"
            scrollable
            :scrollHeight="'calc(100vh - ' + (isUrlReport ? 149 : 200) + 'px)'"
            class="p-datatable-sm"
            v-model:expandedRowGroups="expandedRowGroups"
            expandableRowGroups
            rowGroupMode="subheader"
            groupRowsBy="group"
            showGridlines
            v-model:selection="selectedStamps"
            :value="dtDataReports"
          >
            <template #groupheader="slotProps">
              <b>{{ slotProps.data.group || "Dữ liệu" }}</b>
            </template>
            <template #header v-if="!onedata">
              <div class="flex justify-content-end">
                <span class="p-input-icon-left">
                  <i class="pi pi-search" />
                  <InputText
                    v-model="rpfilters['global'].value"
                    placeholder="Tìm kiếm"
                  />
                </span>
                <Button
                  class="ml-1 p-button-outlined"
                  :loading="showLoadding"
                  @click="rfReportData()"
                  icon="pi pi-refresh"
                  severity="secondary"
                />
              </div>
            </template>
            <Column
              :bodyStyle="
                'max-width:' +
                (col.includes('Ảnh') || col == 'ok'
                  ? '50pt;text-align:center'
                  : col.includes('Mã nhân sự') ||
                    col.includes('Số hợp đồng') ||
                    col.includes('Ngày tạo') ||
                    col.includes('Ngày ký')
                  ? '80pt;text-align:center'
                  : 'auto') +
                ';height:60px'
              "
              :headerStyle="
                'max-width:' +
                (col.includes('Ảnh') || col == 'ok'
                  ? '50pt;text-align:center'
                  : col.includes('Mã nhân sự') ||
                    col.includes('Số hợp đồng') ||
                    col.includes('Ngày tạo') ||
                    col.includes('Ngày ký')
                  ? '80pt;text-align:center'
                  : 'auto') +
                ';height:60px'
              "
              :bodyClass="
                col.includes('Ảnh') ||
                col.includes('Số hợp đồng') ||
                col.includes('Mã nhân sự') ||
                col.includes('Ngày tạo') ||
                col.includes('Ngày ký') ||
                col == 'ok'
                  ? 'format-center'
                  : ''
              "
              :headerClass="
                col.includes('Ảnh') ||
                col.includes('Số hợp đồng') ||
                col.includes('Mã nhân sự') ||
                col.includes('Ngày tạo') ||
                col.includes('Ngày ký') ||
                col == 'ok'
                  ? 'align-items-center justify-content-center text-center'
                  : ''
              "
              v-for="col of dtColumns"
              :key="col"
              :field="col"
              :header="col != 'ok' && !col.includes('_') ? col : ''"
            >
              <template #body="dt">
                <div v-if="col.includes('Ảnh')">
                  <Avatar
                    v-if="(dt.data[col] || '').includes('.')"
                    :image="dt.data[col]"
                    size="large"
                    shape="circle"
                  />
                  <Avatar
                    v-else
                    :label="dt.data[col]"
                    size="large"
                    :style="{
                      background: bgColor[dt.index % 7],
                      color: '#ffffff',
                      fontSize: '1.5rem !important',
                      borderRadius: '5px',
                    }"
                  />
                </div>
                <div v-if="!col.includes('_') && !col.includes('Ảnh')">
                  <div
                    v-if="col == 'ok'"
                    style="text-align: center; font-size: 200%"
                  >
                    <i
                      v-if="dt.data[col]"
                      class="pi pi-check text-green-500"
                    ></i>
                  </div>
                  <div v-else v-html="dt.data[col]"></div>
                </div>
              </template>
            </Column>
            <Column
              class="text-center"
              v-if="readonly && isedit"
              bodyClass="
           format-center
            "
              headerClass="
                 align-items-center justify-content-center text-center
            "
              bodyStyle=" max-width:80px "
              headerStyle="  max-width:80px "
            >
              <template #header>
                <Button
                  v-if="cForm && !onedata"
                  size="small"
                  @click="copyRow()"
                  :icon="'pi pi-' + (isCopy ? 'check' : 'copy')"
                  v-tooltip.left="isCopy ? 'Dán' : 'Copy'"
                  text
                  class="p-button-outlined p-button-text"
                />
                <Button
                  v-if="!onedata"
                  size="small"
                  @click="editForm()"
                  icon="pi pi-pencil"
                  severity="secondary"
                  class="mr-1 p-button-outlined p-button-text p-button-rounded"
                />
              </template>
              <template #body="slotProps">
                <Button
                  size="small"
                  @click="editForm(slotProps.data)"
                  icon="pi pi-pencil"
                  severity="secondary"
                  class="mr-1 p-button-outlined p-button-rounded"
              v-if="selectedStamps.profile_id== slotProps.data.profile_id"
                />
              </template>
            </Column>
          </DataTable>
     
        </div>
        <div v-else style="height: calc(100vh - 130px); overflow-y: auto">
          <div class="p-0">
            <!--<div class="flex" v-if="elementSpan.element">
                                                                                                                                                                                                                                                                                                                                                                                                                                                                               </div> -->
            <div class="flex m-2">
              <div class="flex-1 mr-2">
                <span class="p-input-icon-left w-full">
                  <i class="pi pi-search" />
                  <InputText
                    class="w-full"
                    v-model="rpfilters['global'].value"
                    placeholder="Tìm kiếm"
                  />
                </span>
              </div>
              <Button
                @click="saveConfig()"
                icon="pi pi-save"
                class="mr-1 p-button-outlined"
              />
              <!-- <Button @click="addCogDatabase()" outlined class="mr-1" icon="pi pi-cog" severity="secondary" /> -->
              <Button
                @click="refershConfig()"
                icon="pi pi-refresh"
                severity="secondary"
                class="p-button-outlined"
              />
            </div>
            <DataTable
              v-model:expandedRows="expandedRows"
              :rowClass="rowClass"
              v-if="!isxls && objDataTemp.length > 0"
              scrollHeight="calc(100vh - 200px)"
              class="p-datatable-sm"
              v-model:filters="rpfilters"
              :value="objDataTemp"
              filterDisplay="menu"
              filterMode="lenient"
              :scrollable="true"
              columnResizeMode="fit"
              :lazy="true"
              responsiveLayout="scroll"
              :row-hover="true"
              :showGridlines="true"
            >
              <Column
                expander
                headerStyle="text-align:center;max-width:50px;height:50px"
                bodyStyle="text-align:center;max-width:50px"
              />
              <Column
                headerStyle="text-align:center;max-width:50px;height:50px"
                bodyStyle="text-align:center;max-width:50px"
                header="STT"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  {{ slotProps.index + 1 }}
                </template>
              </Column>
              <Column
                field="value"
                header="Template"
                headerClass="align-items-center justify-content-center text-center"
                headerStyle="text-align:center ;height:50px"
                bodyStyle="text-align:left "
              ></Column>
              <Column
                field="key"
                header="Data"
                headerClass="align-items-center justify-content-center text-center"
                headerStyle="text-align:center ;height:50px"
                bodyStyle="text-align:left "
              >
                <template #body="slotProps">
                  <b
                    :style="
                      'background-color:' +
                      (slotProps.data.isconfig && !slotProps.data.cols
                        ? '#ffff00'
                        : '')
                    "
                    >{{ slotProps.data.key }}</b
                  >
                </template>
              </Column>
              <Column
                headerStyle="text-align:center;max-width:150px;height:50px"
                bodyStyle="text-align:center;max-width:150px"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <Button
                    @click="openCogDatabase(slotProps.data)"
                    icon="pi pi-cog"
                    severity="secondary"
                    class="mr-1 p-button-outlined"
                    style="width: 36px; height: 36px"
                  />
                  <Button
                    @click="delRowXLS(slotProps.data)"
                    icon="pi pi-trash"
                    severity="danger"
                    class="p-button-outlined"
                    style="width: 36px; height: 36px"
                  />
                </template>
              </Column>
              <template #expansion="row">
                <div class="py-3 w-full" v-if="row.data.cols">
                  <Toolbar class="w-full custoolbar">
                    <template #start>
                      <div><h3 class="p-0 m-0 mb-2" v-if="report.report_type == 1">Tổng lương</h3>
                        <h3 class="p-0 m-0 mb-2" v-else>Cột</h3></div>
                    </template>
                    <template #end>
                      <div v-if="report.report_type == 1">
                        <Dropdown
                          v-model="report.sum_key"
                          :options="row.data.cols"
                          optionLabel="key"
                          optionValue="key"
                          placeholder="Chọn tổng lương"
                          class="w-full"
                          style="min-width: 200px"
                        />
                      </div>
                    </template>
                  </Toolbar>

                  <DataTable
                    class="p-datatable-sm d-sidebar-full w-full"
                    :value="row.data.cols"
                    filterDisplay="menu"
                    filterMode="lenient"
                    :scrollable="true"
                    scrollHeight="flex"
                    columnResizeMode="fit"
                    :lazy="true"
                    responsiveLayout="scroll"
                    :row-hover="true"
                  >
                    <Column
                      field="value"
                      header="Template"
                      headerStyle="text-align:center;height:50px"
                      bodyStyle="text-align:left; "
                      headerClass="align-items-center justify-content-center text-center"
                    >
                    </Column>
                    <Column
                      field="key"
                      header="Data"
                      headerStyle="text-align:center;height:50px"
                      bodyStyle="text-align:left "
                      headerClass="align-items-center justify-content-center text-center"
                    ></Column>
                    <Column
                      headerStyle="text-align:center;height:50px;max-width:50px"
                      bodyStyle="text-align:left;max-width:50px "
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <Checkbox
                          v-tooltip="'Đã cấu hình'"
                          v-model="slotProps.data.isconfig"
                          :binary="true"
                          :disabled="true"
                        />
                      </template>
                    </Column>
                    <Column
                      headerStyle="text-align:center;height:50px;max-width:50px"
                      bodyStyle="text-align:left;max-width:50px "
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <Checkbox
                          v-tooltip="'Lấy tự động'"
                          v-model="slotProps.data.auto"
                          :binary="true"
                        />
                      </template>
                    </Column>
                    <Column
                      field="inputtype"
                      header="Nhập"
                      headerStyle="text-align:center;height:50px"
                      headerClass="align-items-center justify-content-center text-center"
                      bodyStyle="text-align:left; "
                    >
                      <template #body="slotProps">
                        <Dropdown
                          @change="changeInputType(slotProps.data)"
                          v-if="!slotProps.data.auto"
                          v-model="slotProps.data.inputtype"
                          optionLabel="lable"
                          optionValue="value"
                          dropdownIcon=""
                          :options="isTypeInputs"
                          class="w-full no-icon"
                        >
                          <template #option="slotProps">
                            <div>
                              <i :class="slotProps.option.icon"></i>
                              {{ slotProps.option.lable }}
                            </div>
                          </template>
                        </Dropdown>
                      </template>
                    </Column>
                    <Column
                      style="width: 100px; text-align: center"
                      class="align-items-center justify-content-center text-center"
                      headerStyle="text-align:center;height:50px"
                      bodyStyle="text-align:left; "
                    >
                      <template #body="slotProps">
                        <Button
                          @click="openCogDatabase(slotProps.data, true, true)"
                          icon="pi pi-cog"
                          severity="secondary"
                          class="p-button-outlined"
                          style="width: 36px; height: 36px"
                        />
                        <Button
                          @click="delRowXLS(row, slotProps.data)"
                          icon="pi pi-trash"
                          severity="danger"
                          class="p-button-outlined"
                          style="width: 36px; height: 36px; margin-left: 5px"
                        />
                      </template>
                    </Column>
                  </DataTable>
                </div>
              </template>
            </DataTable>
            <DataTable
              :rowClass="rowClass"
              v-model:expandedRows="expandedRows"
              v-if="isxls && dtExcels.length > 0"
              scrollable
              scrollHeight="calc(100vh - 200px)"
              class="p-datatable-sm"
              showGridlines
              :value="dtExcels"
            >
              <Column expander />
              <Column
                columnKey=""
                class="text-center"
                field="colname"
                header="Dòng/Cột"
                headerStyle="text-align:center;height:50px;max-width:100px"
                bodyStyle="text-align:left; max-width:100px "
              >
              </Column>
              <Column
                field="key"
                header="Trường"
                headerStyle="text-align:center;height:50px"
                bodyStyle="text-align:left "
                headerClass="align-items-center justify-content-center text-center"
              ></Column>
              <Column
                headerStyle="text-align:center;height:50px"
                bodyStyle="text-align:left "
                headerClass="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <Checkbox
                    v-tooltip="'Lấy tự động'"
                    v-model="slotProps.data.auto"
                    :binary="true"
                  />
                </template>
              </Column>
              <Column
                field="inputtype"
                header="Nhập"
                headerStyle="text-align:center;height:50px"
                bodyStyle="text-align:left "
                headerClass="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <Dropdown
                    @change="changeInputType(slotProps.data)"
                    v-if="!slotProps.data.auto"
                    v-model="slotProps.data.inputtype"
                    optionLabel="lable"
                    optionValue="value"
                    dropdownIcon=""
                    :options="isTypeInputs"
                    class="w-full no-icon"
                  >
                    <template #option="slotProps">
                      <div>
                        <i :class="slotProps.option.icon"></i>
                        {{ slotProps.option.lable }}
                      </div>
                    </template>
                  </Dropdown>
                </template>
              </Column>
              <Column
                class="text-center"
                headerStyle="text-align:center;height:50px;max-width:140px"
                bodyStyle="text-align:left;max-width:140px "
                headerClass="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <Button
                    @click="openCogDatabase(slotProps.data, true, true)"
                    icon="pi pi-cog"
                    severity="secondary"
                    class="p-button-outlined"
                  />
                  <Button
                    @click="delRowXLS(slotProps.data)"
                    icon="pi pi-trash"
                    severity="danger"
                    class="ml-1 p-button-outlined"
                  />
                </template>
              </Column>
              <template #expansion="slotProps">
                <div class="p-3" v-if="slotProps.data.cols.length > 0">
                  <h3 class="p-0 m-0 mb-2">Chi tiết cột</h3>
                  <DataTable
                    scrollable
                    scrollHeight="300px"
                    class="p-datatable-sm"
                    showGridlines
                    :value="slotProps.data.cols"
                  >
                    <Column
                      field="colname"
                      header="Cột"
                      headerStyle="text-align:center;height:50px;max-width:50px"
                      bodyStyle="text-align:left;max-width:50px "
                      headerClass="align-items-center justify-content-center text-center"
                    >
                    </Column>
                    <Column
                      field="key"
                      header="Trường dữ liệu"
                      headerStyle="text-align:center;height:50px "
                      bodyStyle="text-align:left "
                      headerClass="align-items-center justify-content-center text-center"
                    ></Column>
                    <Column>
                      <template #body="slotProps">
                        <Checkbox
                          v-tooltip="'Lấy tự động'"
                          v-model="slotProps.data.auto"
                          :binary="true"
                        />
                      </template>
                    </Column>
                    <Column
                      field="inputtype"
                      header="Nhập"
                      headerStyle="text-align:center;height:50px "
                      bodyStyle="text-align:left  "
                      headerClass="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <Dropdown
                          @change="changeInputType(slotProps.data)"
                          v-if="!slotProps.data.auto"
                          v-model="slotProps.data.inputtype"
                          optionLabel="lable"
                          optionValue="value"
                          dropdownIcon=""
                          :options="isTypeInputs"
                          class="w-full no-icon"
                        >
                          <template #option="slotProps">
                            <div>
                              <i :class="slotProps.option.icon"></i>
                              {{ slotProps.option.lable }}
                            </div>
                          </template>
                        </Dropdown>
                      </template>
                    </Column>
                    <Column
                      headerStyle="text-align:center;height:50px;max-width:140px"
                      bodyStyle="text-align:left;max-width:140px "
                      headerClass="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <Button
                          @click="openCogDatabase(slotProps.data, true, true)"
                          icon="pi pi-cog"
                          severity="secondary"
                          class="p-button-outlined"
                        />
                        <Button
                          @click="delRowXLS(slotProps.data)"
                          icon="pi pi-trash"
                          severity="danger"
                          class="ml-1 p-button-outlined"
                        />
                      </template>
                    </Column>
                  </DataTable>
                </div>
              </template>
            </DataTable>
          </div>
        </div>
      </div>
    </div>
    <Dialog
      v-if="!readonly"
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
      v-if="!readonly"
      header="Thông tin chỉnh sửa"
      v-model:visible="displayElement"
      :breakpoints="{ '960px': '75vw', '640px': '90vw' }"
      :style="{ width: '50vw' }"
      :maximizable="true"
      :modal="true"
    >
      <div class="flex">
        <Dropdown
          v-model="drcurrentText"
          class="w-full flex-1"
          optionLabel="version"
          :options="elementSpan.historys"
          :filter="true"
          placeholder="So sánh với gốc"
        >
          <template #value="slotProps">
            <div v-if="slotProps.value.element">
              <Tag
                class="mr-2"
                :value="'Version ' + slotProps.value.version"
              ></Tag>
              <Tag
                class="mr-2"
                severity="success"
                :value="slotProps.value.user.fname"
              ></Tag>
            </div>
            <span v-else>
              {{ slotProps.placeholder }}
            </span>
          </template>
          <template #option="slotProps">
            <div @click="setCurrentText(slotProps.option)">
              <p>
                <Tag
                  class="mr-2"
                  :value="'Version ' + slotProps.option.version"
                ></Tag>
                <Tag
                  class="mr-2"
                  severity="success"
                  :value="slotProps.option.user.fname"
                ></Tag>
              </p>
              <p class="mt-2">
                {{ parseHTMLText(slotProps.option.element, 100) }}
              </p>
            </div>
          </template>
        </Dropdown>
        <Button
          v-if="drcurrentText.element"
          class="ml-2"
          icon="pi pi-refresh"
          @click="setCurrentText()"
          label="So sánh gốc"
        />
      </div>
      <TabView>
        <TabPanel header="Xem chỉnh sửa 1">
          <div class="p-2" style="background-color: #ccc">
            <Card>
              <template #content>
                <CodeDiff
                  :old-string="removeTags(currentText)"
                  :new-string="removeTags(htmlElement)"
                  maxHeight="calc(100vh - 250px)"
                  outputFormat="side-by-side"
                />
              </template>
            </Card>
          </div>
        </TabPanel>
        <TabPanel header="Xem chỉnh sửa 2">
          <div class="p-2" style="background-color: #ccc">
            <Card>
              <template #content>
                <Diff
                  mode="split"
                  theme="light"
                  language="plaintext"
                  :prev="removeTags(currentText)"
                  :current="removeTags(htmlElement)"
                  :virtual-scroll="{ height: 300 }"
                />
              </template>
            </Card>
          </div>
        </TabPanel>
        <TabPanel header="Xem thay đổi sytle">
          <div class="p-2" style="background-color: #ccc">
            <Card>
              <template #content>
                <div class="flex">
                  <div class="flex-1" v-html="currentText"></div>
                  <Divider layout="vertical" />
                  <div class="flex-1" v-html="htmlElement"></div>
                </div>
              </template>
            </Card>
          </div>
        </TabPanel>
      </TabView>
      <template #footer>
        <Button
          label="Không"
          icon="pi pi-times"
          @click="displayElement = false"
          class="p-button-text"
        />
        <Button
          label="Lưu lại"
          icon="pi pi-save"
          @click="displayElement = false"
          autofocus
        />
      </template>
    </Dialog>
    <Dialog
      v-if="!readonly"
      header="Thiết lập bảng"
      v-model:visible="displayTable"
      :style="{ width: '360px' }"
      :modal="true"
    >
      <p class="mb-2 mt-2"><b>Tiêu đề bảng</b></p>
      <Dropdown
        v-model="mdDataHeader"
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
      <p class="mb-2 mt-2"><b>Dữ liệu bảng</b></p>
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
    <Dialog
      v-if="!readonly"
      header="Chỉnh sửa nội dung"
      v-model:visible="displayEditData"
      :style="{ width: '640px' }"
      :modal="true"
    >
      <p v-if="textEditExcel != ''"><b>Excel</b></p>
      <p v-if="textEditExcel != ''">
        <span class="p-input-icon-left w-full">
          <i
            class="pi pi-fx"
            style="color: green; font-weight: bold; top: 18px; font-size: 150%"
            >fx</i
          >
          <Textarea
            @blur="changeInputExcel(true)"
            style="color: red; font-size: 120%"
            class="w-full"
            v-model="textEditExcel"
            id="textEdit"
            autoResize
            rows="1"
          />
        </span>
      </p>
      <Textarea
        style="font-size: 120%"
        @blur="changeInputExcel(false)"
        class="w-full"
        v-model="textEdit"
        id="textEdit"
        autoResize
        rows="1"
      />
      <div
        class="flex"
        style="
          margin-top: 10px;
          padding: 5px;
          align-items: center;
          background-color: #eee;
          color: #fff;
        "
      >
        <span
          class="p-input-icon-right p-inputtext-sm flex-1"
          style="font-size: 120%"
        >
          <i class="pi pi-search" />
          <InputText
            class="w-full"
            v-model="txtIPXLS"
            placeholder="Tìm kiếm giá trị cột"
          />
        </span>
      </div>
      <div style="max-height: 320px; overflow-y: auto">
        <div class="grid m-2">
          <div
            style="border: 0.5px solid #ccc"
            class="col-6"
            v-for="k in compXLS"
          >
            <b
              ><span
                style="
                  padding: 5px;
                  font-weight: bold;
                  color: blue;
                  padding: 5px;
                "
                >{{ objExcel[k] }}</span
              ></b
            >
            <b>{{ k }}</b>
          </div>
        </div>
      </div>
      <template #footer>
        <Divider />
        <Button
          label="Không"
          icon="pi pi-times"
          @click="displayEditData = false"
          class="p-button-text"
        />
        <Button
          label="Lưu lại"
          icon="pi pi-save"
          @click="saveEditData()"
          autofocus
        />
      </template>
    </Dialog>
    <Dialog
      v-if="!readonly"
      :header="'Thông tin cấu hình nguồn dữ liệu ' + dbrow.value"
      v-model:visible="isDisplayDatabase"
      :breakpoints="{ '960px': '75vw', '640px': '90vw' }"
      :style="{ width: '1024px' }"
      :maximizable="true"
      :modal="true"
    >
      <ConfigDatabaseComponent
        :group="dbrow"
        ref="configDBChild"
        :xls="isxls"
        :one="IsOne"
        :callbackFun="callbackFunChild"
        :report="report"
      />
      <template #footer>
        <div class="flex">
          <Button
            v-if="(dtTempCols || objDataTemp).length > 0"
            icon="pi pi-angle-left"
            class="mr-1"
            @click="nextDBrow(false)"
            severity="secondary"
            text
          />
          <Dropdown
            v-if="(dtTempCols || objDataTemp).length > 0"
            v-model="dbrow"
            optionLabel="value"
            :options="dtTempCols || objDataTemp"
            :filter="true"
            placeholder="Chọn key"
          >
          </Dropdown>
          <Button
            v-if="(dtTempCols || objDataTemp).length > 0"
            icon="pi pi-angle-right"
            class="ml-1"
            @click="nextDBrow(true)"
            severity="secondary"
            text
          />
          <div class="flex-1"></div>
          <Button
            label="Không"
            icon="pi pi-times"
            @click="isDisplayDatabase = false"
            class="p-button-text"
          />
          <Button
            :disabled="showLoadding"
            :loading="showLoadding"
            label="Lưu lại"
            icon="pi pi-save"
            @click="saveDatabase()"
          />
        </div>
      </template>
    </Dialog>
    <ContextMenu
      :class="isDesingData"
      v-if="isDesingData && !readonly"
      ref="menu"
      :model="datakeys"
    >
      <template #item="{ item }">
        <Button @click="goKey(item)" :label="item" class="mb-1 w-full" />
      </template>
    </ContextMenu>
    <ContextMenu
      v-if="!isDesingData && !isUrlReport && !readonly"
      ref="menu"
      :model="actionMenus"
    ></ContextMenu>
    <Sidebar
      v-model:visible="isdataSidebar"
      position="right"
      :class="
        'w-full d-sidebar-full' + (isfullSidebar ? '  ' : ' md:w-8 lg:w-8')
      "
      @hide="onHideSidebarM"
    >
      <template #header>
        <div class="flex w-full">
          <Button
            @click="isfullSidebar = !isfullSidebar"
            :icon="'pi pi-window-' + (!isfullSidebar ? 'maximize' : 'minimize')"
            class="p-button-outlined p-button-secondary p-button-text"
          />
          <div class="text-center flex-1">
            <SelectButton
              v-if="!readonly"
              v-model="opTypeDB"
              :options="optionTypeDBs"
              optionLabel="name"
            />
            <h2 class="p-0 m-0" v-else>
              Cập nhật thông tin
              <span v-if="oneRow" v-html="cForm['Họ tên']"></span>
              <span v-else>{{ report.report_name }}</span>
            </h2>
          </div>
        </div>
      </template>
      <div class="flex w-full p-2 text-end">
        <div class="flex-1"></div>
        <Button
          @click="refersMapData"
          icon="pi pi-refresh"
          class="p-button-outlined p-button-secondary mr-1"
        />
        <Button
          v-if="opTypeDB.value == 1"
          @click="addTable()"
          icon="pi pi-plus"
          class="p-button-outlined p-button-secondary mr-1"
        />
        <FileUpload
          chooseLabel="Import Excel"
          class="mr-1"
          mode="basic"
          :auto="true"
          :customUpload="true"
          @uploader="importJsonData"
          name="doc[]"
          accept=".xls,.xlsx"
        />
        <Button
          v-if="opTypeDB.value == 2"
          severity="secondary"
          class="mr-1 p-button-outlined"
          type="button"
          label="Tải"
          icon="pi pi-download"
          @click="downloadJsonData"
        />
        <Button
          type="button"
          label="Lưu"
          icon="pi pi-save"
          @click="saveDatamap(false)"
        />
      </div>
      <div class="p-1">
        <Accordion
          v-if="opTypeDB.value == 2"
          :multiple="false"
          :activeIndex="0"
        >
          <AccordionTab v-for="(r, index) in datamaps">
            <template #header>
              <span>Dữ liệu</span>
            </template>
            <ul class="ul-form">
              <li
                :class="'m-1 grids' + (Array.isArray(r[r1]) ? ' w-full' : '')"
                v-for="r1 in Object.keys(r)"
                :style="
                  'width:' +
                  (r[r1] && r[r1].toString().includes('<img') ? '100%' : 'auto')
                "
              >
                <Panel
                  toggleable
                  v-if="Array.isArray(r[r1])"
                  v-for="r in datamaps"
                >
                  <template #header>
                    <b>{{ r1 }}({{ r[r1].length }})</b>
                  </template>
                  <template #icons>
                    <button
                      @click="addRow(r, r1)"
                      class="p-panel-header-icon p-link mr-2"
                    >
                      <span class="pi pi-plus-circle"></span>
                    </button>
                  </template>
                  <table style="width: max-content !important">
                    <thead>
                      <tr>
                        <th v-for="th in arrayKeys(r, r1)">
                          <b>{{ th }}</b>
                        </th>
                        <th></th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="(td, idx) in r[r1]">
                        <td v-for="th in arrayKeys(r, r1)">
                          <InputNumber
                            spellcheck="false"
                            :useGrouping="isFormat(th, td[th])"
                            v-if="isTypeInput(td[th], th) == 1"
                            v-model="td[th]"
                            class="w-full"
                          />
                          <InputMask
                            spellcheck="false"
                            mask="99/99/9999"
                            v-if="isTypeInput(td[th], th) == 2"
                            v-model="td[th]"
                            class="w-full"
                          />
                          <Textarea
                            v-if="isTypeInput(td[th], th) == 0"
                            spellcheck="false"
                            autoResize
                            v-model="td[th]"
                            rows="1"
                            class="w-full"
                          ></Textarea>
                        </td>
                        <td class="text-center">
                          <button
                            @click="delRow(r, r1, idx)"
                            class="p-panel-header-icon p-link"
                          >
                            <span class="pi pi-trash"></span>
                          </button>
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </Panel>
                <div v-else>
                  <div class="cols-2">
                    <b>{{ r1 }}</b>
                  </div>
                  <div class="cols-10">
                    <InputNumber
                      spellcheck="false"
                      :useGrouping="isFormat(r1, r[r1])"
                      v-if="isTypeInput(r[r1], r1) == 1"
                      v-model="r[r1]"
                      class="w-full"
                    />
                    <InputMask
                      spellcheck="false"
                      mask="99/99/9999"
                      v-if="isTypeInput(r[r1], r1) == 2"
                      v-model="r[r1]"
                      class="w-full"
                    />
                    <Textarea
                      v-if="isTypeInput(r[r1], r1) == 0"
                      spellcheck="false"
                      autoResize
                      v-model="r[r1]"
                      rows="1"
                      class="w-full"
                    ></Textarea>
                  </div>
                </div>
              </li>
            </ul>
          </AccordionTab>
        </Accordion>
        <div v-if="opTypeDB.value == 1">
          <DataTable class="p-datatable-sm" showGridlines :value="dtTables">
            <Column
              class="text-center"
              :bodyStyle="'max-width: 60px  '"
              :headerStyle="'max-width: 60px ;height:60px'"
              field="index"
              header="STT"
            >
            </Column>
            <Column field="name" header="Tiêu đề"></Column>
            <Column
              :bodyStyle="'max-width: 140px  '"
              :headerStyle="'max-width: 140px ;height:60px'"
              class="text-center"
            >
              <template #body="slotProps">
                <Button
                  @click="openCogDatabase(slotProps.data, true, false)"
                  icon="pi pi-cog"
                  severity="secondary"
                  class="p-button-outlined"
                />
                <Button
                  class="ml-1 p-button-outlined"
                  @click="removeTable(slotProps.data)"
                  icon="pi pi-trash"
                  severity="danger"
                />
              </template>
            </Column>
          </DataTable>
        </div>
        <div
          v-else
          class="p-2"
          style="height: calc(100vh - 170px); overflow-y: auto"
        >
          <Panel
            v-if="oneRow"
            toggleable
            :collapsed="false"
            v-for="dt in objDataTemp"
          >
            <template #header>
              <h3 class="p-0 m-0">
                <i class="pi pi-user"></i> Thông tin lấy dữ liệu
              </h3>
            </template>

            <DataTable
              class="p-datatable-sm"
              showGridlines
              :value="dt.rows"
              v-if="dt.table"
            >
              <template #groupheader="slotProps">
                <b>{{ slotProps.data.group || "Dữ liệu" }}</b>
              </template>
              <template #header>
                <div class="flex justify-content-end">
                  <UserComponent
                    ref="rfUserComp"
                    :removes="dt.users"
                    :hrm="true"
                    :full="true"
                    :keyuser="dt.key.toString()"
                    :callbackFun="getCompUsers"
                    :one="false"
                    :basic="true"
                  >
                  </UserComponent>
                  <Button
                    class="ml-1 p-button-outlined"
                    @click="addRowWord(dt)"
                    icon="pi pi-plus-circle"
                    severity="secondary"
                  />
                </div>
              </template>
              <Column
                field="stt"
                header="STT"
                :bodyStyle="'max-width: 60px  '"
                :headerStyle="'max-width: 60px ;height:60px'"
                headerClass="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <InputNumber
                    spellcheck="false"
                    v-model="slotProps.data.stt"
                    class="w-full"
                  />
                </template>
              </Column>
              <Column
                v-for="c in dt.cols"
                style="white-space: nowrap"
                :bodyStyle="'max-width: 150px  '"
                :headerStyle="'max-width: 150px ;height:60px'"
              >
                <template #header>
                  {{ c.value.replace(/\[(.+)\]/g, "$1") }}
                </template>
                <template #body="slotProps">
                  <Textarea
                    style="height: auto; padding: 5px"
                    spellcheck="false"
                    autoResize
                    v-model="slotProps.data[c.value]"
                    class="w-full"
                  ></Textarea>
                </template>
              </Column>
              <Column header="">
                <template #body="slotProps">
                  <button
                    @click="delRow(dt, 'rows', slotProps.index)"
                    class="p-panel-header-icon p-link"
                  >
                    <span class="pi pi-trash"></span>
                  </button>
                </template>
              </Column>
            </DataTable>
            <div class="grid" v-else>
              <div
                :class="'col-4' + (r.auto ? ' disabled' : '')"
                v-for="r in dt.cols"
              >
                <div class="mb-2">
                  <label
                    ><b>{{ r.value.replace(/\[(.+)\]/g, "$1") }}</b></label
                  >
                </div>
                <div class="h-full" style="max-height: 40px">
                  <InputComponent :okey="'key'" :col="r" :ipmodel="r" />
                </div>
              </div>
            </div>
          </Panel>
          <div v-else>
            <DataTable
              v-model:filters="rpfilters"
              scrollable
              :scrollHeight="'calc(100vh - 250px)'"
              class="p-datatable-sm fix-head"
              v-model:expandedRowGroups="expandedRowGroups"
              expandableRowGroups
              rowGroupMode="subheader"
              groupRowsBy="group"
              :lazy="true"
              :rowHover="true"
              :showGridlines="true"
              scrollDirection="both"
              :value="dtDataReports"
            >
              <template #groupheader="slotProps">
                <b>{{ slotProps.data.group || "Dữ liệu" }}</b>
              </template>
              <template #header>
                <div class="flex justify-content-end">
                  <span class="p-input-icon-left">
                    <i class="pi pi-search" />
                    <InputText
                      v-model="rpfilters['global'].value"
                      placeholder="Tìm kiếm"
                    />
                  </span>
                  <Button
                    class="ml-1 p-button-outlined"
                    :loading="showLoadding"
                    @click="rfReportData()"
                    icon="pi pi-refresh"
                    severity="secondary"
                  />
                </div>
              </template>
              <Column
                frozen
                :headerClass="
                  col.includes('Ảnh') || col == 'ok'
                    ? 'format-center '
                    : col.includes('Mã nhân sự') ||
                      col.includes('Số hợp đồng') ||
                      col.includes('Ngày tạo') ||
                      col.includes('Ngày ký')
                    ? 'format-center'
                    : ''
                "
                :bodyClass="
                  col.includes('Ảnh') || col == 'ok'
                    ? 'format-center '
                    : col.includes('Mã nhân sự') ||
                      col.includes('Số hợp đồng') ||
                      col.includes('Ngày tạo') ||
                      col.includes('Ngày ký')
                    ? 'format-center'
                    : ''
                "
                :headerStyle="
                  col.includes('Ảnh') || col == 'ok'
                    ? 'text-align:center;width:70px;height:50px'
                    : col.includes('Mã nhân sự') ||
                      col.includes('Số hợp đồng') ||
                      col.includes('Ngày tạo') ||
                      col.includes('Ngày ký')
                    ? 'text-align:center;width:200px;height:50px'
                    : ' width:250px;height:50px'
                "
                :bodyStyle="
                  col.includes('Ảnh') || col == 'ok'
                    ? 'text-align:center;width:70px;height:50px'
                    : col.includes('Mã nhân sự') ||
                      col.includes('Số hợp đồng') ||
                      col.includes('Ngày tạo') ||
                      col.includes('Ngày ký')
                    ? 'text-align:center;width:200px;height:50px'
                    : ' width:250px;height:50px'
                "
                v-for="col of dtColumns"
                :key="col"
                :field="col"
                :header="col != 'ok' && !col.includes('_') ? col : ''"
              >
                <template #body="dt">
                  <div v-if="col.includes('Ảnh')">
                    <Avatar
                      v-if="(dt.data[col] || '').includes('.')"
                      :image="dt.data[col]"
                      size="large"
                      shape="circle"
                    />
                    <Avatar
                      v-else
                      :label="dt.data[col]"
                      size="large"
                      :style="{
                        background: bgColor[dt.index % 7],
                        color: '#ffffff',
                        fontSize: '1.5rem !important',
                        borderRadius: '5px',
                      }"
                    />
                  </div>
                  <div v-if="!col.includes('_') && !col.includes('Ảnh')">
                    <div
                      v-if="col == 'ok'"
                      style="text-align: center; font-size: 200%"
                    >
                      <i
                        v-if="dt.data[col]"
                        class="pi pi-check text-green-500"
                      ></i>
                    </div>
                    <div v-else v-html="dt.data[col]"></div>
                  </div>
                </template>
              </Column>
              <Column
                v-for="c in objDataTemp[0].cols"
                style="white-space: nowrap"
                :headerStyle="' width:150px;height:50px'"
                :bodyStyle="' width:150px;height:50px'"
              >
                <template #header>
                  {{ c.value.toString().replace(/\[(.+)\]/g, "$1") }}
                </template>
                <template #body="slotProps">
                  <InputComponent
                    :div="true"
                    :okey="c.value"
                    v-if="slotProps.data.datatemp"
                    :col="c"
                    :ipmodel="slotProps.data.datatemp"
                  />
                </template>
              </Column>
            </DataTable>
          </div>
        </div>
      </div>
    </Sidebar>
  </div>
  <!-- <Menu style="max-height: 450px;overflow-y: auto;" ref="menuInput" :model="itemtypeInputs" popup id="overlay_tmenu" /> -->
</template>
<style lang="scss" scoped>
:deep {
  .no-child > td:first-child > button {
    display: none;
  }

  .has-child > td {
    background-color: #3b82f6;
    color: #fff;
  }

  .has-child > td * {
    color: #fff !important;
  }

  td {
    overflow: hidden;
  }

  .p-panel-content {
    overflow-x: auto;
  }

  .ul-form {
    margin: 0;
    padding: 0;

    li {
      display: inline-block;

      .cols-2 {
        margin-bottom: 5px;
      }
    }
  }

  .os-span-edit-os {
    background-color: yellow !important;
  }

  .os-span-edit-os.off {
    background-color: unset !important;
  }

  .highlighted {
    background-color: yellow !important;
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

  .text1line {
    overflow: hidden;
    text-overflow: ellipsis;
    display: -webkit-box;
    -webkit-line-clamp: 1;
    /* number of lines to show */
    line-clamp: 1;
    -webkit-box-orient: vertical;
  }

  td {
    vertical-align: middle !important;
    align-items: center;
  }

  .vue-diff-viewer .vue-diff-row .hljs {
    word-break: break-word !important;
  }

  span.modified,
  .x {
    background-color: yellow !important;
  }

  .code-diff-view .diff-table .blob-code .blob-code-inner {
    font-size: 15px !important;
    font-family: arial !important;
  }

  .code-diff-view .diff-table .blob-code {
    padding: 10px;
  }

  .vue-diff-viewer .vue-diff-row .hljs {
    padding: 10px;
    font-family: arial;
  }
}

.doc-page {
  width: 29.7cm;
  padding: 10mm;
  display: block;
  page-break-before: always;
  background-color: #fff;
  position: relative;
  margin: auto;
}
</style>
<style  >
.d-design-doc .no-icon {
  text-align: center;
  justify-content: center;
}

.d-design-doc .no-icon .p-dropdown-trigger {
  display: none;
}

.d-design-doc .none {
  display: none !important;
}

.d-design-doc .p-dialog-content tr:hover td {
  background-color: yellow;
}

.d-design-doc .div-excel {
  padding: 0 !important;
}

.d-design-doc .div-excel tr:hover td {
  background-color: yellow !important;
  color: #000 !important;
}

.d-design-doc .div-excel tr:hover td:hover {
  background-color: aliceblue !important;
  color: #000 !important;
}

.d-design-doc .div-excel.line td {
  border: 1px solid #999;
}

.d-design-doc .tablecell {
  text-align: center;
  font-weight: bold;
  background-color: #eee;
  font-size: 13pt;
  border-bottom: 1px solid #999;
}

.d-design-doc td.thcell {
  font-weight: bold;
  padding: 5px;
  font-size: 13pt;
  border-right: 1px solid #999;
}

.d-design-doc td.tablecell,
.thcell {
  background-color: #eee;
  position: sticky;
  z-index: 999;
  top: 0;
  left: 0;
}

.d-design-doc #app-sidebar,
#mobile-header {
  display: none !important;
}

.d-design-doc .true .p-contextmenu-root-list {
  max-height: 400px;
  overflow-y: auto;
  padding: 10px !important;
}

.d-design-doc #gtx-trans {
  display: none !important;
}

.d-design-doc [contenteditable] {
  outline: 0px solid transparent;
}

.d-design-doc .os-delete-os {
  display: none;
}

.d-design-doc tr.selected {
  background-color: yellow !important;
}

.d-design-doc td.thcell,
td.tablecell {
  border-right: 1px solid #999;
  position: sticky;
  z-index: 999;
  top: 0;
  background-color: #eee;
}

.d-design-doc .p-dialog .table-data {
  max-width: calc(100% - 290px);
}

.d-design-doc textarea[disabled] {
  background-color: #e9ecef;
}

.d-design-doc .disabled {
  display: none;
}

.d-design-doc .disabled input,
.disabled textarea,
.disabled .p-dropdown {
  background-color: #eee;
}
</style>
