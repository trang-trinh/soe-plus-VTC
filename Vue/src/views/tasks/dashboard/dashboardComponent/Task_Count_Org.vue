<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../../util/function.js";
import moment from "moment";
import DetailedWork from "../../../../components/task_origin/DetailedWork.vue";
const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");
//khai báo
const axios = inject("axios");
const store = inject("store");
const toast = useToast();
const swal = inject("$swal");
// eslint-disable-next-line no-undef
const basedomainURL = baseURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const width1 = window.screen.width;
const addLog = (log) => {
  // eslint-disable-next-line no-undef
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const options = ref({
  PageSize: 20,
  PageNo: 0,
  loading: true,
  totalRecords: 0,
  SearchText: "",
});
const props = defineProps({
  type: Boolean,
});
let user = store.state.user;
const first = ref(0);
const selectedKeys = ref();
const datalists = ref([]);
const renderTree = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  let vl = data.filter((x) => x.parent_id == null);
  data.forEach((x) => {
    x.progress = x.finished / x.total;
  });
  if (vl.length > 0) {
    data
      .filter((x) => x.parent_id == null)
      .forEach((m, i) => {
        let om = { key: m[id], data: m };
        const rechildren = (mm, pid) => {
          let dts = data.filter((x) => x.parent_id == pid);
          if (dts.length > 0) {
            if (!mm.children) mm.children = [];
            dts.forEach((em) => {
              let om1 = { key: em[id], data: em };
              rechildren(om1, em[id]);
              mm.children.push(om1);
            });
          }
        };
        rechildren(om, m[id]);
        arrChils.push(om);
        //
      });
  } else {
    data.forEach((m, i) => {
      let om = { key: m[id], data: m };
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em) => {
            let om1 = { key: em[id], data: em };
            rechildren(om1, em[id]);
            mm.children.push(om1);
          });
        }
      };
      rechildren(om, m[id]);
      arrChils.push(om);
      //
    });
  }

  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
const loadData = () => {
  options.value.loading = true;
  axios
    .post(
      // eslint-disable-next-line no-undef
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_dashboar_count_by_org",
            par: [
              { par: "user_id", va: user.user_id },
              { par: "type", va: props.type },
              { par: "search", va: options.value.SearchText },
            ],
          }),
          // eslint-disable-next-line no-undef
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      datalists.value = [];

      let data = JSON.parse(response.data.data)[0];
      let count = JSON.parse(response.data.data)[1];
      let obj = renderTree(data, "organization_id", "", "");
      datalists.value = obj.arrChils;
      options.value.totalRecords = count[0].totalRecords;
      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!" + error);
      addLog({
        title: "Lỗi Console loadData",
        controller: "TaskReview(Dashboard).vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const expandedKeys = ref();
const refresh = () => {
  first.value = 0;
  options.value.SearchText = "";
  loadData();
};
onMounted(() => {
  loadData();
});
</script>

<template>
  <div class="div-main">
    <TreeTable
      ref="dt"
      :rowHovers="true"
      :showGridlines="true"
      responsiveLayout="scroll"
      @page="onPage($event)"
      v-model:first="first"
      :expandedKeys="expandedKeys"
      :value="datalists"
      :paginator="true"
      :rows="options.PageSize"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      :scrollable="true"
      scrollHeight="flex"
      :totalRecords="options.totalRecords"
      dataKey="organization_id"
      v-model:selectionKeys="selectedKeys"
    >
      <template #header>
        <h3>
          {{
            props.type == 1
              ? "Thống kê số lượng công việc theo phòng ban"
              : "Thống kê số lượng công việc theo đơn vị"
          }}
        </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                v-model="options.SearchText"
                @keyup.enter="loadData"
                type="text"
                spellcheck="false"
                placeholder="Tìm kiếm"
              />
            </span>
          </template>

          <template #end>
            <Button
              v-if="checkDelList"
              @click="deleteList()"
              label="Xóa"
              icon="pi pi-trash"
              class="mr-2 p-button-danger"
            />

            <Button
              @click="refresh()"
              class="mr-2 p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              v-tooltip="'Tải lại'"
            />
          </template>
        </Toolbar>
      </template>
      <Column
        :header="props.type == 1 ? 'Tên phòng ban' : 'Tên đơn vị'"
        field="organization_name"
        headerClass="align-items-center justify-content-center text-center "
        :expander="true"
      ></Column>
      <Column
        class="align-items-center justify-content-center text-center max-w-8rem"
        header="Tất cả"
        field="total"
      >
        <template #body="data">
          <Button
            class="font-bold p-button-text border-1 border-round-xl border-gray-200 w-full justify-content-center"
            style="background-color: #bbbbbb; color: white"
          >
            <span>
              <span class="font-bold"> {{ data.node.data.total }}</span>
              <br />
              <span class="font-bold">Công việc</span>
            </span>
          </Button>
        </template>
      </Column>
      <Column
        class="align-items-center justify-content-center text-center max-w-8rem"
        header="Đang làm"
        field="doing"
      >
        <template #body="data">
          <Button
            class="font-bold p-button-text border-1 border-round-xl border-gray-200 w-full justify-content-center"
            style="background-color: #2196f3; color: white"
          >
            <span>
              <span class="font-bold"> {{ data.node.data.doing }}</span>
              <br />
              <span class="font-bold">Công việc</span>
            </span>
          </Button>
        </template>
      </Column>
      <Column
        header="Hoàn thành"
        class="align-items-center justify-content-center text-center max-w-8rem"
        field="finished"
      >
        <template #body="data">
          <Button
            class="font-bold p-button-text border-1 border-round-xl border-gray-200 w-full justify-content-center"
            style="background-color: #6fbf73; color: white"
          >
            <span>
              <span class="font-bold"> {{ data.node.data.finished }}</span>
              <br />
              <span class="font-bold">Công việc</span>
            </span>
          </Button>
        </template>
      </Column>

      <Column
        header="Quá hạn"
        field="expired"
        class="align-items-center justify-content-center text-center max-w-8rem"
      >
        <template #body="data">
          <Button
            class="font-bold p-button-text border-1 border-round-xl border-gray-200 w-full justify-content-center"
            style="background-color: #f00000; color: white"
          >
            <span>
              <span class="font-bold"> {{ data.node.data.expired }}</span>
              <br />
              <span class="font-bold">Công việc</span>
            </span>
          </Button>
        </template>
      </Column>
    </TreeTable>
  </div>
</template>

<style lang="scss" scoped>
.div-main {
  height: calc(100vh - 7.5rem);
}
</style>
