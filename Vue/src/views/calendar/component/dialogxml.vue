<script setup>
import { ref, inject, onMounted } from "vue";
import { encr } from "../../../util/function";
import { useToast } from "vue-toastification";
const store = inject("store");
const axios = inject("axios");
const cryoptojs = inject("cryptojs");
const swal = inject("$swal");
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const toast = useToast();

//Get arguments
const props = defineProps({
  headerDialog: String,
  displayDialog: Boolean,
  closeDialog: Function,
});

//Declare
const options = ref({
  view: 1,
  week_start_date: null,
  week_end_date: null,
  today: new Date(),
  month: new Date().getMonth() + 1,
  year: new Date().getFullYear(),
});
const groups = ref([
  { view: 1, icon: "", title: "Xuất theo tuần" },
  { view: 2, icon: "", title: "Xuất theo tháng" },
  { view: 3, icon: "", title: "Xuất theo năm" },
]);
const currentweek = ref({});
const weeks = ref([]);
const months = ref([
  { month: 1 },
  { month: 2 },
  { month: 3 },
  { month: 4 },
  { month: 5 },
  { month: 6 },
  { month: 7 },
  { month: 8 },
  { month: 9 },
  { month: 10 },
  { month: 11 },
  { month: 12 },
]);
const years = ref([]);

//Function
const changeView = (view) => {
  options.value.view = view;
  if (options.value.view != null) {
    switch (options.value.view) {
      case 1:
        if (weeks.value != null && weeks.value.length > 0) {
          currentweek.value = weeks.value.find(
            (x) => x["week_no"] === options.value["week"]
          );
          options.value["week_start_date"] = new Date(
            currentweek.value["week_start_date"]
          );
          options.value["week_end_date"] = new Date(
            currentweek.value["week_end_date"]
          );
        }
        break;
      case 2:
        options.value["week_start_date"] = new Date(
          options.value.year,
          options.value.month - 1,
          1
        );
        options.value["week_end_date"] = new Date(
          options.value.year,
          options.value.month,
          0
        );
        break;
      case 3:
        options.value["week_start_date"] = new Date(options.value.year, 0, 1);
        options.value["week_end_date"] = new Date(options.value.year, 12, 0);
        break;
      default:
        break;
    }
  }
};
const saveXML = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/calendar_week/export_xml",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_duty_list_xml",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "search", va: "" },
              { par: "week_start_date", va: options.value["week_start_date"] },
              { par: "week_end_date", va: options.value["week_end_date"] },
              { par: "is_type", va: 3 },
              { par: "filterDate", va: false },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err === "1") {
        swal.fire({
          title: "Thông báo!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
      toast.success("Lưu vào hệ thống thàng công!");
      props.closeDialog();
    })
    .catch((error) => {
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
// const downloadXML = (item) => {
//   swal.fire({
//     width: 110,
//     didOpen: () => {
//       swal.showLoading();
//     },
//   });
//   axios
//     .post(
//       baseURL + "/api/calendar_week/download_xml",
//       {
//         str: encr(
//           JSON.stringify({
//             proc: "calendar_duty_get_xml",
//             par: [
//               { par: "user_id", va: store.getters.user.user_id },
//               { par: "file_id", va: "9639DCAB1456443583ACC26C36BA6CD5" },
//             ],
//           }),
//           SecretKey,
//           cryoptojs
//         ).toString(),
//       },
//       config
//     )
//     .then((response) => {
//       swal.close();
//       var link = document.createElement("a"); //set up anchor
//       link.setAttribute("target", "_blank");
//       if (Blob !== undefined) {
//         var blob = new Blob([response.data], { type: "application/xml" });
//         link.setAttribute("href", URL.createObjectURL(blob));
//       } else {
//         link.setAttribute(
//           "href",
//           "data:text/html," + encodeURIComponent(response.data)
//         );
//       }
//       link.setAttribute("download", item.file_name || "filename.xml");
//       document.body.appendChild(link);
//       link.click();
//       document.body.removeChild(link);
//     })
//     .catch((error) => {
//       swal.close();
//       swal.fire({
//         title: "Thông báo!",
//         text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
//         icon: "error",
//         confirmButtonText: "OK",
//       });
//     });
// };

//Init
const initDictionary = () => {
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_get_dictionary_duty",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "year", va: options.value.year },
            ],
          }),
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
            weeks.value = tbs[0];
          } else {
            weeks.value = [];
          }
          var exist =
            weeks.value.findIndex((x) => x["is_current_week"] === true) != -1;
          if (exist) {
            currentweek.value = weeks.value.find(
              (x) => x["is_current_week"] === true
            );
          } else {
            currentweek.value = weeks.value.find(
              (x) => x["week_no"] === options.value.week || 0
            );
          }
          if (currentweek.value != null) {
            options.value["week"] = currentweek.value["week_no"];
            options.value["week_start_date"] = new Date(
              currentweek.value["week_start_date"]
            );
            options.value["week_end_date"] = new Date(
              currentweek.value["week_end_date"]
            );
          }
          if (months.value != null && months.value.length > 0) {
            months.value.forEach((m, i) => {
              m["is_current_month"] = false;
              if (m["month"] === options.value.month) {
                m["is_current_month"] = true;
              }
            });
          }
          var startYear = 1970;
          const endYear = new Date().getFullYear() + 10;
          years.value = [];
          for (var i = startYear; i <= endYear; i++) {
            years.value.push({
              year: i,
              IsCurrentYear: i === options.value.today.getFullYear(),
              IsPass: i < options.value.today.getFullYear()
            });
            startYear++;
          }
        }
      }
    })
    .catch((error) => {
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
onMounted(() => {
  initDictionary();
});
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible="props.displayDialog"
    :style="{ width: '60vw' }"
    :maximizable="true"
    :closable="false"
    style="z-index: 1000"
  >
    <form @submit.prevent="">
      <div class="row m-2">
        <div class="col-12 md:col-12">
          <Toolbar class="outline-none surface-0 border-none p-0">
            <template #start>
              <div style="height: 36px; display: flex; align-items: center">
                <SelectButton
                  v-model="options.view"
                  :options="groups"
                  @change="changeView(options.view)"
                  optionValue="view"
                  optionLabel="view"
                  dataKey="view"
                  aria-labelledby="custom"
                >
                  <template #option="slotProps">
                    <span>{{ slotProps.option.title }}</span>
                  </template>
                </SelectButton>
              </div>
            </template>
            <template #end>
              <div class="form-group m-0 mr-2" v-if="options.view !== 0">
                <Dropdown
                  :options="years"
                  :filter="true"
                  :showClear="false"
                  v-model="options.year"
                  @change="goYear(options.year)"
                  optionLabel="year"
                  optionValue="year"
                  placeholder="Chọn năm"
                  class="ip36"
                  style="min-width: 170px"
                >
                  <template #value="slotProps">
                    <div
                      class="country-item country-item-value flex"
                      v-if="slotProps.value"
                    >
                      <i class="pi pi-calendar mr-2 format-flex-center"></i>
                      <div>Năm {{ slotProps.value }}</div>
                    </div>
                    <span v-else>
                      {{ slotProps.placeholder }}
                    </span>
                  </template>
                  <template #option="slotProps">
                    <div
                      class="country-item country-item-value py-2"
                      v-if="slotProps.option"
                    >
                      <div>Năm {{ slotProps.option.year }}</div>
                    </div>
                    <span v-else> Chưa có dữ liệu năm </span>
                  </template>
                </Dropdown>
              </div>
              <div class="form-group m-0" v-if="options.view === 2">
                <Dropdown
                  :options="months"
                  :filter="true"
                  :showClear="false"
                  v-model="options.month"
                  @change="goMonth(options.month)"
                  optionLabel="month"
                  optionValue="month"
                  placeholder="Chọn tháng"
                  class="ip36"
                  style="min-width: 170px"
                >
                  <template #value="slotProps">
                    <div
                      class="country-item country-item-value flex"
                      v-if="slotProps.value"
                    >
                      <i class="pi pi-calendar mr-2 format-flex-center"></i>
                      <div>Tháng {{ slotProps.value }}</div>
                    </div>
                    <span v-else>
                      {{ slotProps.placeholder }}
                    </span>
                  </template>
                  <template #option="slotProps">
                    <div
                      class="country-item country-item-value py-2"
                      v-if="slotProps.option"
                    >
                      <div>Tháng {{ slotProps.option.month }}</div>
                    </div>
                    <span v-else> Chưa có dữ liệu tháng </span>
                  </template>
                </Dropdown>
              </div>
              <div class="form-group m-0" v-if="options.view === 1">
                <Dropdown
                  :options="weeks"
                  :filter="true"
                  :showClear="false"
                  v-model="options.week"
                  @change="goWeek(options.week)"
                  optionLabel="week_no"
                  optionValue="week_no"
                  placeholder="Chọn tuần"
                  class="ip36"
                  style="min-width: 170px"
                >
                  <template #value="slotProps">
                    <div
                      class="country-item country-item-value flex"
                      v-if="slotProps.value"
                    >
                      <i class="pi pi-calendar mr-2 format-flex-center"></i>
                      <div>Tuần {{ slotProps.value }}</div>
                    </div>
                    <span v-else>
                      {{ slotProps.placeholder }}
                    </span>
                  </template>
                  <template #option="slotProps">
                    <div
                      class="country-item country-item-value py-2"
                      v-if="slotProps.option"
                    >
                      <div>Tuần {{ slotProps.option.week_no }}</div>
                    </div>
                    <span v-else> Chưa có dữ liệu tuần </span>
                  </template>
                </Dropdown>
              </div>
            </template>
          </Toolbar>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="props.closeDialog()"
        class="p-button-text"
      />
      <Button label="Lưu" icon="pi pi-check" @click="saveXML()" />
    </template>
  </Dialog>
</template>
<style scoped>
@import url(./stylecalendar.css);
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
::v-deep(.avatar-item) {
  .p-avatar.p-avatar-lg {
    width: 3rem;
    height: 3rem;
  }
}
::v-deep(.is-close) {
  .p-panel-header {
    color: red;
  }
}
</style>
