<script>
import { ref, inject, onMounted, computed } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode } from "primevue/api";
import Editor from "primevue/editor";
import { encr, removeAccents } from "../../../../util/function.js";
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
    report: Object,
    group: Object,
    xls: Boolean,
    callbackFun: Function,
  },
  components: {
    Editor,
  },
  setup(props, ctx) {
    //Config Database
    const filterconfigDatabases = ref({
      global: { value: null, matchMode: FilterMatchMode.CONTAINS },
      table_name: { value: null, matchMode: FilterMatchMode.STARTS_WITH },
    });
    const axios = inject("axios");
    const cryoptojs = inject("cryptojs");
    const store = inject("store");
    const toast = useToast();
    const swal = inject("$swal");
    const showLoadding = ref(false);
    const selectedTabel = ref();
    const dtTables = ref([]);
    const dtPars = ref([]);
    const dtProcs = ref([]);
    const mdTable = ref({});
    const mdProc = ref();
    const txtSQL = ref("");
    const objExcel = ref({});
    const goTableProc = (tb) => {
      mdTable.value = tb;
    };
    const goProc = async () => {
      let strSQL = {
        query: false,
        proc: "proc_get_info",
        par: [
          {
            par: "user_id",
            va: "",
          },
          {
            par: "proc_name",
            va: mdProc.value ? mdProc.value.proc_name : "",
          },
        ],
      };
      console.log(strSQL);
      swal.fire({
        width: 110,
        didOpen: () => {
          swal.showLoading();
        },
      });
      debugger
      const axResponse = await axios.post(
        baseURL + "/api/HRM_SQL/getData",
        {
          str: encr(JSON.stringify(strSQL), SecretKey, cryoptojs).toString(),
        },
        {
          headers: { Authorization: `Bearer ${store.getters.token}` },
        }
      );

      if (axResponse.status == 200) {
        if (axResponse.data.error) {
        } else {
          let dts = JSON.parse(axResponse.data.data)[0];
          dtPars.value = dts;
          debugger
          if (mdProc.value) {
            let sql = mdProc.value.proc_name + " ";
            let dfs = props.report.proc_name
              ? props.report.proc_name.split(" ")
              : [];
            if (dfs.length > 0)
              dts.forEach((dt, i) => {
                sql += dfs[i + 1];
              });
            // if (mdProc.value.proc_des) {
            //     sql = mdProc.value.proc_des;
            // }

            goSQL(sql);
          }
        }
      }
      swal.close();
    };
    const goSQL = async (sql) => {
      let strSQL = {
        query: true,
        proc: sql || txtSQL.value,
      };
      console.log(strSQL);

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

      if (axResponse.status == 200) {
        if (axResponse.data.error) {
          toast.error("Không mở được bản ghi");
        } else {
          dtTables.value = [];
          let dts = JSON.parse(axResponse.data.data);
          let check = true;
          if (dts.length > 0) {
            dts.forEach((dt, i) => {
              if (dt.length > 0) {
                dt.forEach((r) => {
                  if (r.is_data && r.is_data != "null") {
                    r.is_data = JSON.parse(r.is_data);
                    Object.keys(r.is_data[0]).forEach((k) => {
                      r[k] = r.is_data[0][k];
                    });
                  }
                  delete r.is_data;
                });
                let o = {
                  table_id: mdProc.value ? mdProc.value.proc_name : "",
                  table_name: "Bảng " + i,
                  stt: i,
                  isproc: true,
                  cols: [],
                };

                Object.keys(dt[0]).forEach((k) => {
                  o.cols.push({
                    table_id: props.report.proc_name,
                    column_id: k,
                    column_title: dt[0][k],
                    column_type: "",
                  });
                });
                dtTables.value.push(o);
              } else if (props.report.report_type == 3) {
                check = false;
                decision_config();
              }
            });
          } else if (props.report.report_type == 3) {
            console.log("sosa");
          }
          if (check) {
            mdTable.value = dtTables.value.filter(
              (x) => x.stt == (props.group.tid || props.group.key || 0)
            )[0];
            if (!mdTable.value) mdTable.value = dtTables.value[0];
          }
        }
      }
      swal.close();
    };
    const decision_config = async () => {
      let strSQL = {
        query: false,
        proc: "smartreport_decision_config",
        par: [
          {
            par: "user_id",
            va: "",
          },
        ],
      };
      console.log(strSQL);

      const axResponseCF = await axios.post(
        baseURL + "/api/HRM_SQL/getData",
        {
          str: encr(JSON.stringify(strSQL), SecretKey, cryoptojs).toString(),
        },
        {
          headers: { Authorization: `Bearer ${store.getters.token}` },
        }
      );

      if (axResponseCF.status == 200) {
        if (axResponseCF.data.error) {
        } else {
          let dt = JSON.parse(axResponseCF.data.data)[0];
          if (dt.length > 0) {
            let o = {
              table_id: mdProc.value ? mdProc.value.proc_name : "",
              table_name: "Bảng 0",
              stt: 0,
              isproc: true,
              cols: [],
            };
            dt.forEach((k) => {
              o.cols.push({
                table_id: props.report.proc_name,
                column_id: k.column_id,
                column_title: k.column_title,
                column_type: "",
              });
            });
            dtTables.value.push(o);
          }
          mdTable.value = dtTables.value.filter(
              (x) => x.stt == (props.group.tid || props.group.key || 0)
            )[0];
            if (!mdTable.value) mdTable.value = dtTables.value[0];
        }
      }
    };
    const searchCols = (s) => {
      // if (s) {
      //     filterconfigDatabases.value['global'].value = s.replace("[", "").replace("]", "");
      // }
    };
    const listProc = async (s) => {
      showLoadding.value = true;
      let strSQL = {
        query: false,
        proc: "proc_search",
        par: [
          {
            par: "user_id",
            va: "",
          },
          {
            par: "report_key",
            va: props.report.report_key,
          },
        ],
      };

      try {
        const axResponse = await axios.post(
          baseURL + "/api/HRM_SQL/getData",
          {
            str: encr(JSON.stringify(strSQL), SecretKey, cryoptojs).toString(),
          },
          {
            headers: { Authorization: `Bearer ${store.getters.token}` },
          }
        );

        if (axResponse.status == 200) {
          if (axResponse.data.error) {
            toast.error("Không tải được dữ liệu");
          } else {
            let dts = JSON.parse(axResponse.data.data);
            dtProcs.value = dts[0];
            mdProc.value = dtProcs.value[0];

            if (
              props.report.report_config &&
              props.report.report_config.trim() != ""
            ) {
              let objConfig = JSON.parse(props.report.report_config);

              if (objConfig && Object.entries(objConfig.proc).length > 0) {
                dtPars.value = objConfig.proc.parameters;
                txtSQL.value = objConfig.proc.sql;

                goProc();
              } else {
                goProc();
              }
            } else {
              goProc();
            }
          }
        }
        showLoadding.value = false;
      } catch (e) {
        console.log(e);
        showLoadding.value = false;
        toast.error("Có lỗi xảy ra, vui lòng thử lại!");
      }
    };
    const saveDatabase = () => {
      var tbchons = [];

      let arrChons = dtTables.value.filter((x) => x.chon);
      if (arrChons.length == 0) arrChons = [mdTable.value];
      arrChons.forEach((tb) => {
        let tbc = { ...tb };
        let cc = selectedTabel.value;
        if (!cc) {
          cc = {
            column_id: mdTable.value.stt,
            column_title: mdTable.value.table_name,
          };
        }
        tbc.cols = [cc];
        tbchons.push(tbc);
      });

      props.callbackFun(tbchons, {
        name: mdProc.value ? mdProc.value.proc_name : "",
        parameters: dtPars.value,
        sql: txtSQL.value,
        issql: (txtSQL.value || "").toLowerCase().includes("select "),
      });
    };
    let dochtml = {};
    const isxls = ref();
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
        const response = await fetch(baseURL + "api/SRC/" + apimethod, {
          method: "POST",
          body: formData,
        });
        var data = await response.json();
        dochtml = document.getElementById("dochtml");
        let html = data.htmls;
        dochtml.innerHTML = html;
        isxls.value = true;

        addExcelStyle();
        await initDataTempAuto(true);
      } catch (e) {
        console.log(e);
      }
      swal.close();
    };
    const isUrlReport = ref(false);
    let dtUser = {};
    const objDataTemp = ref([]);
    const readonly = ref(props.readonly ? true : false);
    const expandedRows = ref([]);
    const dtExcels = ref([]);
    const dtExcelCols = ref([]);
    const dtExcelRows = ref([]);
    let objConfig = {};

    function removeTags(str) {
      if (str == null || str == "") return "";
      else str = str.toString();

      return str.replace(/(<([^>]+)>)/gi, "").replaceAll("&nbsp;", " ");
    }
    const goProcD = async (query, name, par, f, o) => {
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

    const initDataTempAuto = async (tf) => {
      if (!isUrlReport.value) {
        let dts = await goProcD(
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
      if (isxls.value) initdbXLS();
      // if (objDataTemp.value[0].cols.length == 0) {
      //   await initTempAI();
      // }
      tempHTMLGoc = dochtml.innerHTML;
    };
    let tempHTMLGoc = "";
    const listDataImport = ref([]);
    const importJsonData = (event) => {
      let file = event.files[0];
      if (file.name.includes("xls")) {
        myDocUploader(event);
        return false;
      }
    };

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
            "rgb(255, 192, 0)" &&
          x.innerText != null &&
          x.innerText != ""
      );

      mdTable.value.cols = [];
      ths.forEach((element, i) => {
        let xn = element.getAttribute("title");
        let th = ths.find(
          (x) =>
            x.getAttribute("title").replace(/\d/g, "") ==
              xn.replace(/\d/g, "") && !x.getAttribute("colspan")
        );
        let tn = null;
        if (th) {
          tn = element.innerText;
          if (th.getAttribute("tname")) {
            tn = th.getAttribute("tname") + " (" + tn + ")";
          }
        }
        mdTable.value.cols.push({ column_id: tn, column_title: "" });
      });

      showLoadding.value = false;
    };

    onMounted(() => {
      if (
        props.report.proc_name &&
        props.report.proc_name.toLowerCase().includes("select")
      ) {
        mdTable.value = { stt: 0, tid: 0, cols: [], issql: true };
        txtSQL.value = props.report.proc_name;

        goSQL(txtSQL.value);
      } else if (dtProcs.value.length == 0) {
        listProc();
      }
    });
    ctx.expose({ saveDatabase, searchCols });
    return {
      showLoadding,
      selectedTabel,
      saveDatabase,
      goSQL,
      goTableProc,
      dtTables,
      mdTable,
      filterconfigDatabases,
      searchCols,
      mdProc,
      txtSQL,
      importJsonData,
      listDataImport,
      isUrlReport,
      readonly,
    };
  },
};
</script>
<template>
  <DataTable
    v-model:selection="selectedTabel"
    v-model:filters="filterconfigDatabases"
    :globalFilterFields="['column_id', 'column_title']"
    :loading="showLoadding"
    v-if="mdTable && mdTable.cols"
    :value="mdTable.cols"
    showGridlines
    class="p-datatable-sm h-full"
    scrollable
    scrollHeight="calc(100vh - 340px)"
    tableStyle="table-layout: fixed;"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <i class="pi pi-database"></i>
        <span class="text-xl text-900 font-bold">Bảng</span>
        <Button
          class="m-0 p-button-rounded p-button-outlined"
          size="small"
          :label="tb.stt.toString()"
          @click="goTableProc(tb)"
          :class="
            mdTable.stt == tb.stt ? 'p-button-info' : 'p-button-secondary'
          "
          v-for="tb in dtTables"
        />
        <div class="flex-1"></div>
        <span>
          <FileUpload
            chooseLabel="Import Excel"
            class="mr-1"
            mode="basic"
            :auto="true"
            :customUpload="true"
            @uploader="importJsonData"
            name="doc[]"
            accept=".xls,.xlsx"
            v-if="report.report_type == 1"
          />
        </span>
        <span class="p-input-icon-left">
          <i class="pi pi-search" />
          <InputText
            @keyup.enter="searchCols()"
            v-model="filterconfigDatabases['global'].value"
            placeholder="Enter để tìm kiếm"
          />
        </span>
      </div>
    </template>
    <Column
      selectionMode="single"
      headerStyle="max-width: 3rem"
      bodyStyle="max-width: 3rem"
    >
    </Column>
    <Column header="Mã">
      <template #body="slotProps">
        {{ slotProps.data.column_id }}
      </template>
    </Column>
    <Column header="Mô tả">
      <template #body="slotProps">
        {{ slotProps.data.column_title }}
      </template>
    </Column>
  </DataTable>

  <div
    style="display: none"
    :contenteditable="!isUrlReport && !readonly"
    spellcheck="false"
    id="dochtml"
    @contextmenu="onRightClick"
  ></div>
</template>
 