<script>
import { ref, inject, onMounted, computed } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode } from "primevue/api";
import Editor from "primevue/editor";
import { encr, removeAccents } from "../../../../util/function.js";
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
          if (mdProc.value) {
            debugger
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
          dts.forEach((dt, i) => {
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
            if (dt.length > 0)
              Object.keys(dt[0]).forEach((k) => {
                o.cols.push({
                  table_id: props.report.proc_name,
                  column_id: k,
                  column_title: dt[0][k],
                  column_type: "",
                });
              });
            dtTables.value.push(o);
          });
          mdTable.value = dtTables.value.filter(
            (x) => x.stt == (props.group.tid || props.group.key || 0)
          )[0];
          if (!mdTable.value) mdTable.value = dtTables.value[0];
        }
      }
      swal.close();
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
              if (objConfig && objConfig.proc) {
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
          class="m-0"
          size="small"
          :label="tb.stt.toString()"
          @click="goTableProc(tb)"
          :severity="mdTable.stt == tb.stt ? 'info' : 'secondary'"
          rounded
          outlined
          v-for="tb in dtTables"
        />
        <div class="flex-1"></div>
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
    <Column selectionMode="single" headerStyle="width: 3rem"> </Column>
    <Column header="Mã">
      <template #body="slotProps">
        {{ slotProps.data.column_id }}
      </template>
    </Column>
    <Column header="Ví dụ">
      <template #body="slotProps">
        {{ slotProps.data.column_title }}
      </template>
    </Column>
  </DataTable>
</template>
<style lang="scss" scoped></style>