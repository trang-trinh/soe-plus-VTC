<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { encr } from "../../util/function";
import { useToast } from "vue-toastification";

const router = inject("router");
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const cryoptojs = inject("cryptojs");
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
//Declare
const isFirst = ref(false);
const options = ref({
  loading: false,
  statistical_id: null,
  searchprocedure: "",
  searchparameter: "",
  totalprocedure: 0,
  total: 0,
});
const procedures = ref([]);
const selectedKeyProcedure = ref([]);
watch(selectedKeyProcedure, () => {
  goProcedure(selectedKeyProcedure.value);
});
const goProcedure = (node) => {
  router.push({
    name: "statisticalchart",
    params: { id: node.statistical_id },
  });
};

//Init
const initProcedure = (rf) => {
  if (rf) {
    options.value.loading = true;
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  axios
    .post(
      baseURL + "/api/statistical/statistical_procedure_get",
      {
        str: encr(
          JSON.stringify({ search: options.value.searchprocedure }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbs = JSON.parse(data);
          if (tbs[0] != null && tbs[0].length > 0) {
            procedures.value = tbs[0].filter((x) => x["status"] === true);
          } else {
            procedures.value = [];
          }
          if (tbs.length == 2) {
            options.value.totalprocedure = tbs[1][0].total;
          }
        }
      }
      swal.close();
      if (isFirst.value) isFirst.value = false;
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      if (options.value.loading) options.value.loading = false;
      swal.close();
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    });
};
onMounted(() => {
  initProcedure(true);
  return {};
});
</script>
<template>
  <div class="surface-100 p-3">
    <div class="d-lang-table">
      <DataTable
        @sort="onSort($event)"
        :value="procedures"
        :totalRecords="options.totalprocedure"
        :lazy="true"
        :rowHover="true"
        :showGridlines="true"
        :scrollable="true"
        v-model:selection="selectedKeyProcedure"
        selectionMode="single"
        dataKey="statistical_id"
        scrollHeight="flex"
        filterDisplay="menu"
        filterMode="lenient"
        responsiveLayout="scroll"
      >
        <Column
          field="is_order"
          header="STT"
          headerStyle="text-align:center;max-width:50px;height:50px"
          bodyStyle="text-align:center;max-width:50px;"
          class="align-items-center justify-content-center text-center"
        >
        </Column>
        <Column
          field="procedure_name"
          header="Loại báo cáo"
          headerStyle="height:50px;max-width:auto;"
        >
          <template #body="slotProps">
            <div class="p-2">{{ slotProps.data.statistical_name }}</div>
          </template>
        </Column>
        <template #empty>
          <div
            class="
              align-items-center
              justify-content-center
              p-4
              text-center
              m-auto
            "
            v-if="!isFirst || options.total == 0"
            style="display: flex; height: calc(100vh - 195px)"
          >
            <div>
              <img src="../../assets/background/nodata.png" height="144" />
              <h3 class="m-1">Không có dữ liệu</h3>
            </div>
          </div>
        </template>
      </DataTable>
    </div>
  </div>
</template>
<style scoped>
.d-lang-table {
  height: calc(100vh - 75px);
  overflow-y: auto;
  background-color: #fff;
}
.inputanh {
  border: 1px solid #ccc;
  width: 96px;
  height: 96px;
  cursor: pointer;
  padding: 1px;
}
.ipnone {
  display: none;
}
.inputanh img {
  object-fit: cover;
  width: 100%;
  height: 100%;
}
.format-flex-center {
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
}
.format-grid-center {
  display: grid;
  align-items: center;
  justify-content: center;
  text-align: center;
}
.style-day {
  line-height: 1.8rem;
}
.style-day.false {
  color: #2196f3 !important;
}
.style-day.true {
  color: red !important;
}
.form-group {
  display: grid;
  margin-bottom: 1rem;
}
.form-group > label {
  margin-bottom: 0.5rem;
}
.ip36 {
  width: 100%;
}
.p-ulchip {
  margin: 0;
  margin-top: 0.5rem;
  padding: 0;
  list-style: none;
}
.p-lichip {
  float: left;
}
.p-multiselect-label {
  height: 100%;
  display: flex;
  align-items: center;
}
.type0 {
  background-color: #ff8b4e;
}
.type1 {
  background-color: #33c9dc;
}
.description {
  color: #aaa;
  font-size: 12px;
}
</style>
<style lang="scss" scoped>
::v-deep(.d-lang-table) {
  .p-datatable-thead .justify-content-center .p-column-header-content {
    justify-content: center !important;
  }

  .p-datatable-table {
    position: absolute;
  }

  .p-datatable-thead {
    position: sticky;
    top: 0;
    z-index: 1;
  }
}
::v-deep(.form-group) {
  .p-multiselect .p-multiselect-label,
  .p-dropdown .p-dropdown-label {
    height: 100%;
    display: flex;
    align-items: center;
  }
  .p-chip img {
    margin: 0;
  }
  .p-avatar-text {
    font-size: 1rem;
  }
}
</style>