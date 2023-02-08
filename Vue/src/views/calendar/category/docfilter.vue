<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../util/function";
const cryoptojs = inject("cryptojs");
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const isDynamicSQL = ref(false);
const basedomainURL = baseURL;
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const toast = useToast();

//Declare
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
  loading: false,
  typeCount: 0,
  pageno: 0,
  pagesize: 50,
  totalL: 0,
  sort: "fl.send_date desc, do.doc_master_id",
  search: "",
});
const filterSQL = ref([]);
const isFirst = ref(false);
const showFilter = ref(false);
const datas = ref([]);
const counts = ref([]);
const compendiums = ref([]);
const doc_codes = ref([]);
const send_bys = ref([]);
const doc_status = ref([]);
const doc_groups = ref([]);
const issue_places = ref([]);
const field_names = ref([]);
const dispatch_books = ref([]);
const departments = ref([]);

//Filter
const filter_compendiums = ref([]);
const filter_doc_codes = ref([]);
const filter_send_bys = ref([]);
const start_created_date = ref();
const end_created_date = ref();
const start_doc_date = ref();
const end_doc_date = ref();
const filter_doc_status = ref([]);
const filter_doc_groups = ref([]);
const filter_issue_places = ref([]);
const filter_field_names = ref([]);
const filter_dispatch_books = ref([]);
const filter_departments = ref([]);

const filters = () => {
  filterSQL.value = [];
  let array = [];

  //Trích yếu nội dung
  let object1 = {};
  if (filter_compendiums.value && filter_compendiums.value.length > 0) {
    object1.key = "do.doc_master_id";
    object1.filteroperator = "in";
    array = [];
    filter_compendiums.value.forEach((el) => {
      array.push({ value: el["doc_master_id"] });
    });
    object1.filterconstraints = array;
    filterSQL.value.push(object1);
  }

  //Số ký hiệu
  let object2 = {};
  if (filter_doc_codes.value && filter_doc_codes.value.length > 0) {
    object2.key = "do.doc_code";
    object2.filteroperator = "in";
    array = [];
    filter_doc_codes.value.forEach((el) => {
      array.push({ value: el["doc_code"] });
    });
    object2.filterconstraints = array;
    filterSQL.value.push(object2);
  }

  //Người gửi
  let object3 = {};
  if (filter_send_bys.value && filter_send_bys.value.length > 0) {
    object3.key = "fl.send_by";
    object3.filteroperator = "in";
    array = [];
    filter_send_bys.value.forEach((el) => {
      array.push({ value: el["user_key"] });
    });
    object3.filterconstraints = array;
    filterSQL.value.push(object3);
  }

  //Ngày nhận văn bản
  let object4 = {};
  if (start_created_date.value != null) {
    if (typeof start_created_date.value == "string") {
      var eDay = start_created_date.value.split("/");
      start_created_date.value = new Date(
        eDay[2] + "/" + eDay[1] + "/" + eDay[0]
      );
    }
    object4.key = "created_date";
    object4.filteroperator = "and";
    array = [];
    array.push({
      matchMode: "dateAfter",
      value: start_created_date.value,
    });
    object4.filterconstraints = array;
    filterSQL.value.push(object4);
  }
  let object5 = {};
  if (end_created_date.value != null) {
    if (typeof end_created_date.value == "string") {
      var eDay = end_created_date.value.split("/");
      end_created_date.value = new Date(
        eDay[2] + "/" + eDay[1] + "/" + eDay[0]
      );
    }
    object5.key = "created_date";
    object5.filteroperator = "and";
    array = [];
    array.push({
      matchMode: "dateBefore",
      value: end_created_date.value,
    });
    object5.filterconstraints = array;
    filterSQL.value.push(object5);
  }

  //Ngày văn bản
  let object6 = {};
  if (start_doc_date.value != null) {
    if (typeof start_doc_date.value == "string") {
      var eDay = start_doc_date.value.split("/");
      start_doc_date.value = new Date(eDay[2] + "/" + eDay[1] + "/" + eDay[0]);
    }
    object6.key = "doc_date";
    object6.filteroperator = "and";
    array = [];
    array.push({
      matchMode: "dateAfter",
      value: start_doc_date.value,
    });
    object6.filterconstraints = array;
    filterSQL.value.push(object6);
  }
  let object7 = {};
  if (end_doc_date.value != null) {
    if (typeof end_doc_date.value == "string") {
      var eDay = end_doc_date.value.split("/");
      end_doc_date.value = new Date(eDay[2] + "/" + eDay[1] + "/" + eDay[0]);
    }
    object7.key = "doc_date";
    object7.filteroperator = "and";
    array = [];
    array.push({
      matchMode: "dateBefore",
      value: end_doc_date.value,
    });
    object7.filterconstraints = array;
    filterSQL.value.push(object7);
  }

  //trạng thái văn bản
  let object8 = {};
  if (filter_doc_status.value && filter_doc_status.value.length > 0) {
    object8.key = "do.doc_status_id";
    object8.filteroperator = "in";
    array = [];
    filter_doc_status.value.forEach((el) => {
      array.push({ value: el["status_id"] });
    });
    object8.filterconstraints = array;
    filterSQL.value.push(object8);
  }

  //Nhóm văn bản
  let object9 = {};
  if (filter_doc_groups.value && filter_doc_groups.value.length > 0) {
    object9.key = "do.doc_group";
    object9.filteroperator = "in";
    array = [];
    filter_doc_groups.value.forEach((el) => {
      array.push({ value: el["role_group_id"] });
    });
    object9.filterconstraints = array;
    filterSQL.value.push(object9);
  }

  //Nơi bản hành
  let object10 = {};
  if (filter_issue_places.value && filter_issue_places.value.length > 0) {
    object10.key = "do.issue_place";
    object10.filteroperator = "in";
    array = [];
    filter_issue_places.value.forEach((el) => {
      array.push({ value: el["issue_place_id"] });
    });
    object10.filterconstraints = array;
    filterSQL.value.push(object10);
  }

  //Lĩnh vực
  let object11 = {};
  if (filter_field_names.value && filter_field_names.value.length > 0) {
    object11.key = "do.field_name";
    object11.filteroperator = "in";
    array = [];
    filter_field_names.value.forEach((el) => {
      array.push({ value: el["field_name"] });
    });
    object11.filterconstraints = array;
    object11.type_of = 1;
    filterSQL.value.push(object11);
  }

  //Số công văn
  let object12 = {};
  if (filter_dispatch_books.value && filter_dispatch_books.value.length > 0) {
    object12.key = "do.dispatch_book_id";
    object12.filteroperator = "in";
    array = [];
    filter_dispatch_books.value.forEach((el) => {
      array.push({ value: el["dispatch_book_id"] });
    });
    object12.filterconstraints = array;
    filterSQL.value.push(object12);
  }

  //LPhong ban người soạn thảo
  let object13 = {};
  if (filter_departments.value && filter_departments.value.length > 0) {
    object13.key = "do.department_id";
    object13.filteroperator = "in";
    array = [];
    filter_departments.value.forEach((el) => {
      array.push({ value: el["organization_id"] });
    });
    object13.filterconstraints = array;
    filterSQL.value.push(object13);
  }

  options.value.pageno = 0;
  options.value.total = 0;
  isDynamicSQL.value = true;
  initDataSQL();
  showFilter.value = false;
};
const reFilter = () => {
  filter_compendiums.value = [];
  filter_doc_codes.value = [];
  filter_send_bys.value = [];
  start_created_date.value = null;
  end_created_date.value = null;
  start_doc_date.value = null;
  end_doc_date.value = null;
  filter_doc_status.value = [];
  filter_doc_groups.value = [];
  filter_issue_places.value = [];
  filter_field_names.value = [];
  filter_dispatch_books.value = [];
  filter_departments.value = [];
};

//Function
const op = ref();
const toggle = (event) => {
  op.value.toggle(event);
};
const change_created_date = () => {
  if (start_created_date.value > end_created_date.value) {
    end_created_date.value = null;
    swal.fire({
      title: "Error!",
      text: "Ngày bắt đầu không được nhỏ hợn ngày kết thúc!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
};
const change_doc_date = () => {
  if (start_doc_date.value > end_doc_date.value) {
    end_doc_date.value = null;
    swal.fire({
      title: "Error!",
      text: "Ngày bắt đầu không được nhỏ hợn ngày kết thúc!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
};

//Log
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};

//Init
const initDictionary = () => {
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "doc_get_dictionary",
            par: [
              {
                par: "organization_id",
                va: store.getters.user.organization_id,
              },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "user_key", va: store.getters.user.user_key },
              { par: "typeCount", va: options.value.typeCount },
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
        let tbn = JSON.parse(data);
        compendiums.value = tbn[0];
        doc_codes.value = tbn[1];
        send_bys.value = tbn[2];
        doc_status.value = tbn[3];
        doc_groups.value = tbn[4];
        issue_places.value = tbn[5];
        field_names.value = tbn[6];
        dispatch_books.value = tbn[7];
        departments.value = tbn[8];
      }
    })
    .catch((error) => {
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const initDataSQL = () => {
  let par = {
    organization_id: store.getters.user.organization_id,
    user_key: store.getters.user.user_key,
    typeCount: options.value.typeCount,
    pageno: options.value.pageno,
    pagesize: options.value.pagesize,
    search: options.value.search,
    fields: filterSQL.value,
    order_by: options.value.sort,
  };
  axios
    .post(
      baseURL + "/api/calendar_week/filter_doc_master_list_receive",
      par,
      config
    )
    .then((response) => {
      var data = response.data;
      if (data != null) {
        if (data["err"] != "0") {
          swal.fire({
            title: "Error!",
            text:
              data["err"] == "2"
                ? response.data.ms
                : "Lọc dữ liệu không thành công.",
            icon: "error",
            confirmButtonText: "OK",
          });
          return;
        }

        let tbs = JSON.parse(data.data);
        if (tbs[0] != null && tbs[0].length > 0) {
          datas.value = tbs[0];
        } else {
          datas.value = [];
        }
        swal.close();
        if (isFirst.value) isFirst.value = false;
        if (options.value.loading) options.value.loading = false;
      }
    })
    .catch((error) => {
      debugger;
      options.value.loading = false;
      toast.error("Tải dữ liệu không thành công!");
      addLog({
        title: "Lỗi Console initDataSQL",
        controller: "boardroom.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const initCountSQL = () => {
  let par = {
    organization_id: store.getters.user.organization_id,
    user_key: store.getters.user.user_key,
    search: options.value.search,
    fields: filterSQL.value,
  };
  axios
    .post(
      baseURL + "/api/calendar_week/filter_doc_master_count_receive",
      par,
      config
    )
    .then((response) => {
      var data = response.data;
      if (data != null) {
        if (data["err"] != "0") {
          swal.fire({
            title: "Error!",
            text:
              data["err"] == "2"
                ? response.data.ms
                : "Lọc dữ liệu không thành công.",
            icon: "error",
            confirmButtonText: "OK",
          });
          return;
        }

        let tbs = JSON.parse(data.data);
        if (tbs[0] != null && tbs[0].length > 0) {
          counts.value = tbs[0];
        } else {
          counts.value = [];
        }
        swal.close();
        if (isFirst.value) isFirst.value = false;
        if (options.value.loading) options.value.loading = false;
      }
    })
    .catch((error) => {
      options.value.loading = false;
      toast.error("Tải dữ liệu không thành công!");
      addLog({
        title: "Lỗi Console initDataSQL",
        controller: "boardroom.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
onMounted(() => {
  initDictionary();
  initDataSQL();
  initCountSQL();
  return {};
});
</script>
<template>
  <div class="surface-100 p-3">
    <Toolbar class="outline-none surface-0 border-none">
      <template #start>
        <span class="p-input-icon-left">
          <i class="pi pi-search" />
          <InputText
            @keypress.enter="searchData"
            v-model="options.search"
            type="text"
            spellcheck="false"
            placeholder=" Tìm kiếm"
          />
        </span>
        <Button
          @click="toggle"
          type="button"
          class="ml-2 p-button-outlined p-button-secondary"
          icon="pi pi-filter"
          aria:haspopup="true"
          aria-controls="overlay_panel"
          v-tooltip="'Bộ lọc'"
        />
        <OverlayPanel
          :showCloseIcon="false"
          ref="op"
          appendTo="body"
          class="p-0 m-0"
          id="overlay_panel"
          style="width: 450px"
        >
          <div class="grid formgrid m-0">
            <div class="col-12 md:col-12 p-0">
              <div
                class="scroll-outer"
                style="width: 100%; height: 400px; overflow-y: auto"
              >
                <div class="scroll-inner">
                  <div class="grid formgrid m-0">
                    <div class="col-12 md:col-12 p-0">
                      <div class="form-group">
                        <label>Trích yếu nội dung</label>
                        <MultiSelect
                          :options="compendiums"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          optionLabel="compendium"
                          placeholder="Chọn trích yếu nội dung"
                          v-model="filter_compendiums"
                          class="ip36"
                          style="height: auto; min-height: 36px"
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
                                <Chip
                                  :label="value.compendium"
                                  class="mr-2 mb-2"
                                />
                              </li>
                            </ul>
                            <span v-else> {{ slotProps.placeholder }} </span>
                          </template>
                          <template #option="slotProps">
                            <div v-if="slotProps.option" class="flex">
                              <div style="word-break: break-word">
                                {{ slotProps.option.compendium }}
                              </div>
                            </div>
                            <span v-else> Chưa có dữ liệu </span>
                          </template>
                        </MultiSelect>
                      </div>
                    </div>
                    <div class="col-12 md:col-12 p-0">
                      <div class="form-group">
                        <label>Số ký hiệu</label>
                        <MultiSelect
                          :options="doc_codes"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          optionLabel="doc_code"
                          placeholder="Chọn số ký hiệu"
                          v-model="filter_doc_codes"
                          class="ip36"
                          style="height: auto; min-height: 36px"
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
                                <Chip
                                  :label="value.doc_code"
                                  class="mr-2 mb-2"
                                />
                              </li>
                            </ul>
                            <span v-else> {{ slotProps.placeholder }} </span>
                          </template>
                          <template #option="slotProps">
                            <div v-if="slotProps.option" class="flex">
                              <div>{{ slotProps.option.doc_code }}</div>
                            </div>
                            <span v-else> Chưa có dữ liệu </span>
                          </template>
                        </MultiSelect>
                      </div>
                    </div>
                    <div class="col-12 md:col-12 p-0">
                      <div class="form-group">
                        <label>Người gửi</label>
                        <MultiSelect
                          :options="send_bys"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          optionLabel="full_name"
                          placeholder="Chọn người gửi"
                          v-model="filter_send_bys"
                          class="ip36"
                          style="height: auto; min-height: 36px"
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
                                <Chip
                                  :image="value.avatar"
                                  :label="value.full_name"
                                  class="mr-2 mb-2 pl-0"
                                >
                                  <div class="flex">
                                    <div class="format-flex-center">
                                      <Avatar
                                        v-bind:label="
                                          value.avatar
                                            ? ''
                                            : (value.last_name ?? '').substring(
                                                0,
                                                1
                                              )
                                        "
                                        v-bind:image="
                                          basedomainURL + value.avatar
                                        "
                                        style="
                                          background-color: #2196f3;
                                          color: #ffffff;
                                          width: 2rem;
                                          height: 2rem;
                                        "
                                        :style="{
                                          background: bgColor[index % 7],
                                        }"
                                        class="mr-2 text-avatar"
                                        size="xlarge"
                                        shape="circle"
                                      />
                                    </div>
                                    <div class="format-flex-center">
                                      <span>{{ value.full_name }}</span>
                                    </div>
                                  </div>
                                </Chip>
                              </li>
                            </ul>
                            <span v-else> {{ slotProps.placeholder }} </span>
                          </template>
                          <template #option="slotProps">
                            <div v-if="slotProps.option" class="flex">
                              <div class="format-center">
                                <Avatar
                                  v-bind:label="
                                    slotProps.option.avatar
                                      ? ''
                                      : slotProps.option.last_name.substring(
                                          0,
                                          1
                                        )
                                  "
                                  v-bind:image="
                                    basedomainURL + slotProps.option.avatar
                                  "
                                  style="
                                    background-color: #2196f3;
                                    color: #ffffff;
                                    width: 3rem;
                                    height: 3rem;
                                    font-size: 14px !important;
                                  "
                                  :style="{
                                    background: bgColor[slotProps.index % 7],
                                  }"
                                  class="text-avatar"
                                  size="xlarge"
                                  shape="circle"
                                />
                              </div>
                              <div class="ml-3">
                                <div class="mb-1">
                                  {{ slotProps.option.full_name }}
                                </div>
                                <div class="description">
                                  <div>
                                    {{ slotProps.option.position_name }}
                                  </div>
                                  <div>
                                    {{ slotProps.option.department_name }}
                                  </div>
                                </div>
                              </div>
                            </div>
                            <span v-else> Chưa có dữ liệu tuần </span>
                          </template>
                        </MultiSelect>
                      </div>
                    </div>
                    <div class="col-12 md:col-12 p-0">
                      <div class="form-group">
                        <label>Ngày nhận văn bản</label>
                        <div class="grid formgrid m-0">
                          <div class="col-6 md:col-6 pl-0">
                            <Calendar
                              :showIcon="true"
                              class="ip36"
                              autocomplete="on"
                              inputId="time24"
                              v-model="start_created_date"
                              @date-select="change_created_date()"
                              @input="change_created_date()"
                              style="width: 100%"
                              placeholder="Từ ngày"
                            />
                          </div>
                          <div class="col-6 md:col-6 pr-0">
                            <Calendar
                              :showIcon="true"
                              class="ip36"
                              autocomplete="on"
                              inputId="time24"
                              v-model="end_created_date"
                              @date-select="change_created_date()"
                              @input="change_created_date()"
                              style="width: 100%"
                              placeholder="Đến ngày"
                            />
                          </div>
                        </div>
                      </div>
                    </div>
                    <div class="col-12 md:col-12 p-0">
                      <div class="form-group">
                        <label>Ngày văn bản</label>
                        <div class="grid formgrid m-0">
                          <div class="col-6 md:col-6 pl-0">
                            <Calendar
                              :showIcon="true"
                              class="ip36"
                              autocomplete="on"
                              inputId="time24"
                              v-model="start_doc_date"
                              @date-select="change_doc_date()"
                              @input="change_doc_date()"
                              style="width: 100%"
                              placeholder="Từ ngày"
                            />
                          </div>
                          <div class="col-6 md:col-6 pr-0">
                            <Calendar
                              :showIcon="true"
                              class="ip36"
                              autocomplete="on"
                              inputId="time24"
                              v-model="end_doc_date"
                              @date-select="change_doc_date()"
                              @input="change_doc_date()"
                              style="width: 100%"
                              placeholder="Đến ngày"
                            />
                          </div>
                        </div>
                      </div>
                    </div>
                    <div class="col-12 md:col-12 p-0">
                      <div class="form-group">
                        <label>Trạng thái văn bản</label>
                        <MultiSelect
                          :options="doc_status"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          optionLabel="status_name"
                          placeholder="Chọn trạng thái văn bản"
                          v-model="filter_doc_status"
                          class="ip36"
                          style="height: auto; min-height: 36px"
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
                                <Chip
                                  :label="value.status_name"
                                  class="mr-2 mb-2"
                                />
                              </li>
                            </ul>
                            <span v-else> {{ slotProps.placeholder }} </span>
                          </template>
                          <template #option="slotProps">
                            <div v-if="slotProps.option" class="flex">
                              <div>{{ slotProps.option.status_name }}</div>
                            </div>
                            <span v-else> Chưa có dữ liệu </span>
                          </template>
                        </MultiSelect>
                      </div>
                    </div>
                    <div class="col-12 md:col-12 p-0">
                      <div class="form-group">
                        <label>Nhóm văn bản</label>
                        <MultiSelect
                          :options="doc_groups"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          optionLabel="role_group_name"
                          placeholder="Chọn nhóm văn bản"
                          v-model="filter_doc_groups"
                          class="ip36"
                          style="height: auto; min-height: 36px"
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
                                <Chip
                                  :label="value.role_group_name"
                                  class="mr-2 mb-2"
                                />
                              </li>
                            </ul>
                            <span v-else> {{ slotProps.placeholder }} </span>
                          </template>
                          <template #option="slotProps">
                            <div v-if="slotProps.option" class="flex">
                              <div>{{ slotProps.option.role_group_name }}</div>
                            </div>
                            <span v-else> Chưa có dữ liệu </span>
                          </template>
                        </MultiSelect>
                      </div>
                    </div>
                    <div class="col-12 md:col-12 p-0">
                      <div class="form-group">
                        <label>Nơi ban hành</label>
                        <MultiSelect
                          :options="issue_places"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          optionLabel="issue_place_name"
                          placeholder="Chọn nơi ban hành"
                          v-model="filter_issue_places"
                          class="ip36"
                          style="height: auto; min-height: 36px"
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
                                <Chip
                                  :label="value.issue_place_name"
                                  class="mr-2 mb-2"
                                />
                              </li>
                            </ul>
                            <span v-else> {{ slotProps.placeholder }} </span>
                          </template>
                          <template #option="slotProps">
                            <div v-if="slotProps.option" class="flex">
                              <div>{{ slotProps.option.issue_place_name }}</div>
                            </div>
                            <span v-else> Chưa có dữ liệu </span>
                          </template>
                        </MultiSelect>
                      </div>
                    </div>
                    <div class="col-12 md:col-12 p-0">
                      <div class="form-group">
                        <label>Lĩnh vực</label>
                        <MultiSelect
                          :options="field_names"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          optionLabel="field_name"
                          placeholder="Chọn lĩnh vực"
                          v-model="filter_field_names"
                          class="ip36"
                          style="height: auto; min-height: 36px"
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
                                <Chip
                                  :label="value.field_name"
                                  class="mr-2 mb-2"
                                />
                              </li>
                            </ul>
                            <span v-else> {{ slotProps.placeholder }} </span>
                          </template>
                          <template #option="slotProps">
                            <div v-if="slotProps.option" class="flex">
                              <div>{{ slotProps.option.field_name }}</div>
                            </div>
                            <span v-else> Chưa có dữ liệu </span>
                          </template>
                        </MultiSelect>
                      </div>
                    </div>
                    <div class="col-12 md:col-12 p-0">
                      <div class="form-group">
                        <label>Sổ công văn</label>
                        <MultiSelect
                          :options="dispatch_books"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          optionLabel="dispatch_book_name"
                          placeholder="Chọn số công văn"
                          v-model="filter_dispatch_books"
                          class="ip36"
                          style="height: auto; min-height: 36px"
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
                                <Chip
                                  :label="value.dispatch_book_name"
                                  class="mr-2 mb-2"
                                />
                              </li>
                            </ul>
                            <span v-else> {{ slotProps.placeholder }} </span>
                          </template>
                          <template #option="slotProps">
                            <div v-if="slotProps.option" class="flex">
                              <div>
                                {{ slotProps.option.dispatch_book_name }}
                              </div>
                            </div>
                            <span v-else> Chưa có dữ liệu </span>
                          </template>
                        </MultiSelect>
                      </div>
                    </div>
                    <div class="col-12 md:col-12 p-0">
                      <div class="form-group">
                        <label>Phòng ban người soạn thảo</label>
                        <MultiSelect
                          :options="departments"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          optionLabel="organization_name"
                          placeholder="Chọn phòng ban người soạn thảo"
                          v-model="filter_departments"
                          class="ip36"
                          style="height: auto; min-height: 36px"
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
                                <Chip
                                  :label="value.organization_name"
                                  class="mr-2 mb-2"
                                />
                              </li>
                            </ul>
                            <span v-else> {{ slotProps.placeholder }} </span>
                          </template>
                          <template #option="slotProps">
                            <div v-if="slotProps.option" class="flex">
                              <div>
                                {{ slotProps.option.organization_name }}
                              </div>
                            </div>
                            <span v-else> Chưa có dữ liệu </span>
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
                class="border-none surface-0 outline-none p-0 mt-3 w-full"
              >
                <template #start>
                  <Button
                    @click="reFilter()"
                    class="p-button-outlined"
                    label="Xóa bộ lọc"
                  ></Button>
                </template>
                <template #end>
                  <Button @click="filters()" label="Lọc"></Button>
                </template>
              </Toolbar>
            </div>
          </div>
        </OverlayPanel>
      </template>
    </Toolbar>
    <div style="overflow-y: auto; height: 100vh">
      {{ datas }}
    </div>
  </div>
</template>
<style scoped>
.form-group {
  display: grid;
  margin-bottom: 1rem;
  flex: 1;
}

.form-group > label {
  margin-bottom: 0.5rem;
}

.scroll-outer {
  visibility: hidden;
}

.scroll-inner,
.scroll-outer:hover,
.scroll-outer:focus {
  visibility: visible;
}

.p-ulchip {
  list-style: none;
  margin: 0;
  padding: 0;
}

.p-lichip {
  float: left;
}

.description {
  color: #aaa;
  font-size: 12px;
}

.format-flex-center {
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
  vertical-align: center;
}
</style>
<style lang="scss" scoped>
::v-deep(.form-group) {
  .p-multiselect .p-multiselect-label,
  .p-dropdown .p-dropdown-label {
    height: 100%;
    display: flex;
    align-items: center;
    white-space: normal !important;
  }

  .p-chip-text {
    word-break: break-word !important;
  }

  .p-chip img {
    margin: 0;
  }
}
</style>
