<script setup>
import { onMounted, ref, inject } from "vue";
import { encr } from "../../../../util/function";
import { useToast } from "vue-toastification";
import moment from "moment";
import dialogcontract from "../../contract/component/dialogcontract.vue";

const router = inject("router");
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const base_url = baseURL;
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const toast = useToast();
const cryoptojs = inject("cryptojs");
const basedomainURL = baseURL;

//Get arguments
const props = defineProps({
  tab: Number,
});

//Declare
const options = ref({
  loading: true,
  pageNo: 1,
  pageSize: 25,
  total: 0,
  file: {},
  type_files: [],
  is_type_files: [],
});
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const selectedNodes = ref({});

//filter
const search = () => {
  initView10(true);
};
const opfilter = ref();
const toggleFilter = (event) => {
  opfilter.value.toggle(event);
};
const resetFilter = () => {
  options.value.type_files = [];
  options.value.is_type_files = [];
};
const removeFilter = (idx, array) => {
  array.splice(idx, 1);
};
const filter = (event) => {
  opfilter.value.toggle(event);
  initView10(true);
};

//Function
const componentKey = ref({});
const forceRerender = (type) => {
  if (!componentKey.value[type]) {
    componentKey.value[type] = 0;
  }
  componentKey.value[type] += 1;
};
const typeFiles = ref([
  { is_type: 0, title: "Sơ yếu lý lịch" },
  { is_type: 1, title: "Hợp đồng" },
  { is_type: 2, title: "Đào tạo" },
]);
const type_files = ref([
  { type_file: "pdf", title: "PDF" },
  { type_file: "jpg,jpeg,png,gif", title: "Ảnh" },
  { type_file: "doc,docs,xls,xlsx", title: "Word,Excel" },
  { type_file: "orther", title: "Khác" },
]);
const is_type_files = ref([
  { is_type: 0, title: "File sơ yếu lý lịch" },
  { is_type: 1, title: "File hợp đồng" },
  { is_type: 2, title: "File đào tạo" },
]);
const files = ref([]);
const formatBytes = (bytes, decimals = 2) => {
  if (bytes === 0) return "0 Bytes";
  const k = 1024;
  const dm = decimals < 0 ? 0 : decimals;
  const sizes = ["Bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + " " + sizes[i];
};
const headerDialogFile = ref();
const displayDialogFile = ref(false);
const openViewDialogFile = (str) => {
  headerDialogFile.value = str;
  displayDialogFile.value = true;
  forceRerender(0);
};
const closeDialogFile = () => {
  displayDialogFile.value = false;
  forceRerender(0);
};
const selectRow6 = (event) => {
  if (event && event.data) {
    options.value.file = event.data;
    openViewDialogFile(event.data["file_name"]);
  }
};

//init
const initView10 = (rf) => {
  if (rf) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  var type_files = null;
  if (options.value.type_files != null && options.value.type_files.length > 0) {
    type_files = options.value.type_files.map((x) => x["type_file"]).join(",");
  }
  var is_type_files = null;
  if (
    options.value.is_type_files != null &&
    options.value.is_type_files.length > 0
  ) {
    is_type_files = options.value.is_type_files
      .map((x) => x["is_type"])
      .join(",");
  }
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_myprofile_get_10",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "search", va: options.value.search },
              { par: "pageNo", va: options.value.pageNo },
              { par: "pageSize", va: options.value.pageSize },
              { par: "type_files", va: type_files },
              { par: "is_type_files", va: is_type_files },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      var data = response.data.data;
      if (data != null) {
        var tbs = JSON.parse(data);
        if (tbs[0] != null && tbs[0].length > 0) {
          tbs[0].forEach((item) => {
            if (item["file_size"] != null) {
              item["file_size"] = formatBytes(item["file_size"]);
            }
            if (item["created_date"] != null) {
              item["created_date"] = moment(
                new Date(item["created_date"])
              ).format("DD/MM/YYYY");
            }
            var idx = typeFiles.value.findIndex(
              (x) => x["is_type"] === item["is_type"]
            );
            if (idx !== -1) {
              item["is_type_name"] = typeFiles.value[idx]["title"];
            }
          });
          files.value = tbs[0];
          if (tbs[1] != null && tbs[1].length > 0) {
            options.value.total = tbs[1][0].total;
          }
        } else {
          files.value = [];
          options.value.total = 0;
        }
      }
      swal.close();
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      swal.close();
      if (options.value.loading) options.value.loading = false;
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo!",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
        return;
      } else {
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
    });
};
onMounted(() => {
    initView10(true);
});
//page
const onPage = (event) => {
  if (event.rows != options.value.pageSize) {
    options.value.pageSize = event.rows;
  }
  options.value.pageNo = event.page + 1;
  initView10(true);
};
</script>
<template>
  <div class="surface-100">
    <Toolbar class="outline-none surface-0 border-none">
      <template #start>
        <span class="p-input-icon-left">
          <i class="pi pi-search" />
          <InputText
            @keypress.enter="search()"
            v-model="options.search"
            type="text"
            spellcheck="false"
            :placeholder="'Tìm kiếm'"
          />
        </span>
        <Button
          @click="toggleFilter($event)"
          type="button"
          class="ml-2 p-button-outlined p-button-secondary"
          aria:haspopup="true"
          aria-controls="overlay_panel"
        >
          <div>
            <span class="mr-2"><i class="pi pi-filter"></i></span>
            <span class="mr-2">Lọc dữ liệu</span>
            <span><i class="pi pi-chevron-down"></i></span>
          </div>
        </Button>
        <OverlayPanel
          :showCloseIcon="false"
          ref="opfilter"
          appendTo="body"
          class="p-0 m-0"
          id="overlay_panel"
          style="width: 400px"
        >
          <div class="grid formgrid m-0">
            <div
              class="col-12 md:col-12 p-0"
              :style="{
                minHeight: 'unset',
                maxHeight: 'calc(100vh - 300px)',
                overflow: 'auto',
              }"
            >
              <div class="row">
                <div class="col-12 md:col-12">
                  <div class="row">
                    <div class="col-12 md:col-12 p-0">
                      <div class="form-group">
                        <label>Loại file</label>
                        <MultiSelect
                          :options="type_files"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          v-model="options.type_files"
                          optionLabel="title"
                          placeholder="Chọn loại file"
                          class="w-full limit-width"
                          style="min-height: 36px"
                          panelClass="d-design-dropdown"
                        >
                          <template #value="slotProps">
                            <ul
                              class="p-ulchip"
                              v-if="
                                slotProps.value && slotProps.value.length > 0
                              "
                            >
                              <li
                                class="p-lichip"
                                v-for="(value, index) in slotProps.value"
                                :key="index"
                              >
                                <Chip class="mr-2 mb-2 px-3 py-2">
                                  <div class="flex">
                                    <div>
                                      <span>{{ value.title }}</span>
                                    </div>
                                    <span
                                      tabindex="0"
                                      class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                      @click="
                                        removeFilter(index, options.type_files);
                                        $event.stopPropagation();
                                      "
                                      v-tooltip.top="'Xóa'"
                                    ></span>
                                  </div>
                                </Chip>
                              </li>
                            </ul>
                            <span v-else>
                              {{ slotProps.placeholder }}
                            </span>
                          </template>
                        </MultiSelect>
                      </div>
                    </div>
                    <div class="col-12 md:col-12 p-0">
                      <div class="form-group">
                        <label>Vị trí file</label>
                        <MultiSelect
                          :options="is_type_files"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          v-model="options.is_type_files"
                          optionLabel="title"
                          placeholder="Chọn vị trí"
                          class="w-full limit-width"
                          style="min-height: 36px"
                          panelClass="d-design-dropdown"
                        >
                          <template #value="slotProps">
                            <ul
                              class="p-ulchip"
                              v-if="
                                slotProps.value && slotProps.value.length > 0
                              "
                            >
                              <li
                                class="p-lichip"
                                v-for="(value, index) in slotProps.value"
                                :key="index"
                              >
                                <Chip class="mr-2 mb-2 px-3 py-2">
                                  <div class="flex">
                                    <div>
                                      <span>{{ value.title }}</span>
                                    </div>
                                    <span
                                      tabindex="0"
                                      class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                      @click="
                                        removeFilter(
                                          index,
                                          options.is_type_files
                                        );
                                        $event.stopPropagation();
                                      "
                                      v-tooltip.top="'Xóa'"
                                    ></span>
                                  </div>
                                </Chip>
                              </li>
                            </ul>
                            <span v-else>
                              {{ slotProps.placeholder }}
                            </span>
                          </template>
                        </MultiSelect>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div class="col-12 md:col-12 p-0">
              <Toolbar
                class="border-none surface-0 outline-none px-0 pb-0 w-full"
              >
                <template #start>
                  <Button
                    @click="resetFilter()"
                    class="p-button-outlined"
                    label="Bỏ chọn"
                  ></Button>
                </template>
                <template #end>
                  <Button @click="filter($event)" label="Lọc"></Button>
                </template>
              </Toolbar>
            </div>
          </div>
        </OverlayPanel>
      </template>
      <template #end> </template>
    </Toolbar>
    <div class="d-lang-table">
      <DataTable
        @page="onPage($event)"
        @rowSelect="selectRow6"
        :value="files"
        :paginator="true"
        :rows="options.pageSize"
        :rowsPerPageOptions="[25, 50, 100, 200]"
        :totalRecords="options.total"
        :scrollable="true"
        :lazy="true"
        :rowHover="true"
        :showGridlines="false"
        :globalFilterFields="['file_name']"
        v-model:selection="selectedNodes"
        selectionMode="single"
        dataKey="file_id"
        scrollHeight="flex"
        filterDisplay="menu"
        filterMode="lenient"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
        responsiveLayout="scroll"
        rowGroupMode="subheader"
        groupRowsBy="is_type_name"
      >
        <template #groupheader="slotProps">
          <i class="pi pi-list mr-2"></i>{{ slotProps.data.is_type_name }}
        </template>
        <Column
          field="file_name"
          header="Tên file số hóa"
          headerStyle="text-align:center;height:50px"
          bodyStyle="text-align:left;"
          class="align-items-center justify-content-left text-left"
        >
          <template #body="slotProps">
            <div class="flex align-items-center">
              <img
                class="mr-2"
                :src="
                  basedomainURL +
                  '/Portals/Image/file/' +
                  slotProps.data.file_type +
                  '.png'
                "
                style="object-fit: contain"
                width="40"
                height="40"
              />
              <span
                :style="{
                  wordBreak: 'break-word',
                }"
              >
                {{ slotProps.data.file_name }}</span
              >
            </div>
          </template>
        </Column>
        <Column
          field="file_size"
          header="Kích cỡ"
          headerStyle="text-align:center;max-width:150px;height:50px"
          bodyStyle="text-align:center;max-width:150px;"
          class="align-items-center justify-content-center text-center"
        />
        <Column
          field="created_date"
          header="Ngày tạo"
          headerStyle="text-align:center;max-width:150px;height:50px"
          bodyStyle="text-align:center;max-width:150px;"
          class="align-items-center justify-content-center text-center"
        />
        <template #empty>
          <div
            class="align-items-center justify-content-center p-4 text-center m-auto"
            style="
              display: flex;
              width: 100%;
              height: calc(100vh - 386px);
              background-color: #fff;
            "
          >
            <div v-if="!options.loading && options.total === 0">
              <img
                src="../../../../assets/background/nodata.png"
                height="144"
              />
              <h3 class="m-1">Không có dữ liệu</h3>
            </div>
          </div>
        </template>
      </DataTable>
    </div>
  </div>
  <!--Dialog-->
  <dialogfile
    :key="componentKey['0']"
    :headerDialog="headerDialogFile"
    :displayDialog="displayDialogFile"
    :file="options.file"
    :closeDialog="closeDialogFile"
  />
</template>
<style scoped>
@import url(../../contract/component/stylehrm.css);
.d-lang-table {
    height: calc(100vh - 275px);
    overflow-y: auto;
    background-color: #fff;
}
.box-info .card {
  border: none;
  border-radius: 0;
  position: relative;
  display: -webkit-box;
  display: -ms-flexbox;
  display: flex;
  -webkit-box-orient: vertical;
  -webkit-box-direction: normal;
  -ms-flex-direction: column;
  flex-direction: column;
  min-width: 0;
  word-wrap: break-word;
  background-color: #fff;
  background-clip: border-box;
}
.box-info .card-header {
  -webkit-box-flex: 1;
  -ms-flex: 1 1 auto;
  flex: 1 1 auto;
  padding: 1rem;
  overflow: hidden;
  border-bottom: solid 1px rgba(0, 0, 0, 0.1);
  font-size: 15px;
  font-weight: bold;
  color: #005a9e;
}
.box-info .card-body {
  -webkit-box-flex: 1;
  -ms-flex: 1 1 auto;
  flex: 1 1 auto;
  padding: 1rem;
  overflow: hidden;
}
</style>
<style lang="scss" scoped>
::v-deep(.border-radius) {
  img {
    border-radius: 50%;
  }
}
</style>
<style lang="scss" scoped>
::v-deep(.p-timeline) {
  background-color: #fff;
  padding: 1rem;
  .p-timeline-event .p-timeline-event-opposite {
    display: none !important;
  }
  .p-timeline-event:nth-child(even) {
    flex-direction: row;
  }
  .p-card-body {
    padding: 0;
  }
}
</style>
