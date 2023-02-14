<script setup>
import { onMounted, inject, ref, watch } from "vue";
import { encr } from "../../../util/function";
import { useToast } from "vue-toastification";
import moment from "moment";

const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const cryoptojs = inject("cryptojs");
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
  initData: Function,
});

//Declare
const options = ref({
  loading: true,
  search: "",
  total: 0,
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
const is_check_all = ref(false);
const submitted = ref(false);
const temps = ref([]);
const datas = ref([]);
const selectedNodes = ref([]);

///
watch(selectedNodes, () => {
  selectedNodes.value;
});

//Function
function CreateGuid() {
  function _p8(s) {
    var p = (Math.random().toString(16) + "000000000").substr(2, 8);
    return s ? "-" + p.substr(0, 4) + "-" + p.substr(4, 4) : p;
  }
  return _p8() + _p8(true) + _p8(true) + _p8();
}
Date.prototype.addDays = function (days) {
  var date = new Date(this.valueOf());
  date.setDate(date.getDate() + days);
  return date;
};
const getDayDate = (d) => {
  var date = new Date(d);
  var current_day = date.getDay();
  var day_name = "";
  if (current_day != null) {
    switch (current_day) {
      case 0:
        day_name = "Chủ Nhật";
        break;
      case 1:
        day_name = "Thứ Hai";
        break;
      case 2:
        day_name = "Thứ Ba";
        break;
      case 3:
        day_name = "Thứ Tư";
        break;
      case 4:
        day_name = "Thứ Năm";
        break;
      case 5:
        day_name = "Thứ Sáu";
        break;
      case 6:
        day_name = "Thứ Bảy";
        break;
      default:
        break;
    }
  }
  return day_name;
};
// const bindDateBetweenFirstAndLast = (
//   start_date,
//   end_date,
//   add_fn,
//   interval
// ) => {
//   add_fn = add_fn || Date.prototype.addDays;
//   interval = interval || 1;

//   var retVal = [];
//   var current = new Date(start_date);

//   while (current <= end_date) {
//     retVal.push(new Date(current));
//     current = add_fn.call(current, interval);
//   }

//   return retVal;
// };
function isValidDate(d) {
  return d instanceof Date && !isNaN(d);
}
const bindDateBetweenFirstAndLast = (
  start_date,
  end_date,
  add_fn,
  interval
) => {
  var retVal = [];
  if (isValidDate(start_date) && isValidDate(end_date)) {
    add_fn = add_fn || Date.prototype.addDays;
    interval = interval || 1;

    var current = new Date(start_date);

    var checkVR = true;
    if (current >= end_date) {
      checkVR = false;
    }
    while (checkVR) {
      if (current >= end_date) {
        checkVR = false;
      }
      retVal.push(new Date(current));
      current = add_fn.call(current, interval);
    }
  }
  return retVal;
};

const changeCheckedAll = (checkedAll) => {
  if (datas.value != null && datas.value.length > 0) {
    datas.value
      .filter((x) => x["calendar_id"] != null && x["is_private"] !== true)
      .forEach((item, i) => {
        item["is_check"] = checkedAll;
      });
    selectedNodes.value = datas.value.filter((x) => x["is_check"]);
  }
};
const changeChecked = (item) => {
  var day = moment(new Date(item["day"])).format("DD/MM/YYYY");
  var is_check = item["is_check"];
  if (datas.value != null && datas.value.length > 0) {
    var calendars = datas.value.filter(
      (x) => moment(new Date(x["day"])).format("DD/MM/YYYY") === day
    );
    if (is_check) {
      calendars.forEach((a) => {
        a["is_check"] = true;
      });
      selectedNodes.value = selectedNodes.value.concat(calendars);
    } else {
      calendars.forEach((a) => {
        a["is_check"] = false;
        var idx = selectedNodes.value.findIndex(
          (b) => b["calendar_id"] === a["calendar_id"]
        );
        if (idx != -1) {
          selectedNodes.value.splice(idx, 1);
        }
      });
      var data = [...selectedNodes.value];
      selectedNodes.value = data;
    }
  }
};
const search = () => {
  initData(true);
};
const cutRice = (rf) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn cắt cơm các ngày đã chọn không!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then((result) => {
      if (result.isConfirmed) {
        submitted.value = true;
        if (!selectedNodes.value || selectedNodes.value.length === 0) {
          swal.fire({
            title: "Thông báo!",
            text: "Vui lòng chọn ngày đăng ký cắt cơm!",
            icon: "error",
            confirmButtonText: "OK",
          });
          return;
        }
        swal.fire({
          width: 110,
          didOpen: () => {
            swal.showLoading();
          },
        });
        let formData = new FormData();
        let array = selectedNodes.value.map((x) => x.day);
        var flags = [],
          output = [];
        array.forEach((element) => {
          if (flags[element]) {
            return;
          }
          flags[element] = true;
          output.push(element);
        });
        formData.append("dates", JSON.stringify(output));
        axios
          .put(baseURL + "/api/calendar_week/cut_rice", formData, config)
          .then((response) => {
            if (response.data.err === "1") {
              swal.fire({
                title: "Thông báo!",
                text: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
              });
            }
            swal.close();
            toast.success("Cập nhật thành công!");
            props.closeDialog();
            props.initData(true);
          })
          .catch((error) => {
            swal.close();
            swal.fire({
              title: "Thông báo!",
              text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
              icon: "error",
              confirmButtonText: "OK",
            });
            return;
          });
        if (submitted.value) submitted.value = true;
      }
    });
};

//Init
const holidays = ref([]);
const initDictionary = () => {
  axios
    .get(baseURL + "/api/BookingMeal/GetConfig", {
      headers: { Authorization: `Bearer ${store.getters.token}` },
    })
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          holidays.value = data.working_days;
        }
      }
    })
    .then(() => {
      initData(true);
    })
    .catch((error) => {
      swal.close();
      if (options.value.loading) options.value.loading = false;
      console.log(error);
    });
};
const initData = (rf) => {
  options.value.loading = true;
  temps.value = [];
  datas.value = [];
  if (rf) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "dashboard_check_cutrice",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "search", va: options.value.search },
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
          let tbn = JSON.parse(data);
          let userbooks = [];
          if (tbn[1] != null && tbn[1].length > 0) {
            userbooks = tbn[1];
          }
          if (tbn[0] != null && tbn[0].length > 0) {
            tbn[0].forEach((item) => {
              if (item["members"] != null) {
                item["members"] = JSON.parse(item["members"]);

                item["chutris"] = item["members"].filter(
                  (x) => x["is_type"] === "0"
                );
                item["thamgias"] = item["members"].filter(
                  (x) => x["is_type"] === "1"
                );
              }
              var start_date_copy = new Date(item["start_date"]);
              var end_date_copy = new Date(item["end_date"]);
              var start_date_new = new Date(
                start_date_copy.getFullYear(),
                start_date_copy.getMonth(),
                start_date_copy.getDate()
              );
              var end_date_new = new Date(
                end_date_copy.getFullYear(),
                end_date_copy.getMonth(),
                end_date_copy.getDate()
              );
              if ((start_date_new = end_date_new)) {
                let obj = { ...item };
                obj.day = start_date_copy;
                obj.day_name = getDayDate(start_date_copy);
                obj.day_string = moment(start_date_copy).format("DD/MM/YYYY");
                obj.is_holiday = start_date_copy.getDay() == 0;
                datas.value.push(obj);
              } else {
                let dateinweeks = bindDateBetweenFirstAndLast(
                  new Date(item["start_date"]),
                  new Date(item["end_date"])
                );
                dateinweeks.forEach((day, i) => {
                  let obj = { ...item };
                  obj.day = day;
                  obj.day_name = getDayDate(day);
                  obj.day_string = moment(day).format("DD/MM/YYYY");
                  obj.is_holiday = day.getDay() == 0;
                  datas.value.push(obj);
                });
              }
            });
            datas.value = datas.value.sort(function (a, b) {
              return new Date(a["day"]) - new Date(b["day"]);
            });
            if (datas.value != null && datas.value.length > 0) {
              datas.value.forEach((item) => {
                item["calendar_id"] = CreateGuid();
              });
              let skip = datas.value.filter(
                (a) =>
                  holidays.value.filter(
                    (b) =>
                      moment(b).format("DD/MM/YYYY") ===
                      moment(a["day"]).format("DD/MM/YYYY")
                  ).length > 0 ||
                  userbooks.filter(
                    (b) =>
                      moment(b["booking_date"]).format("DD/MM/YYYY") ===
                      moment(a["day"]).format("DD/MM/YYYY")
                  ).length > 0
              );
              if (skip != null && skip.length > 0) {
                skip.forEach((item) => {
                  var idx = datas.value.findIndex(
                    (x) => x["calendar_id"] === item["calendar_id"]
                  );
                  if (idx != -1) {
                    datas.value.splice(idx, 1);
                  }
                });
              }
            }
            is_check_all.value = true;
            changeCheckedAll(true);
          }
        }
      }
      swal.close();
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      swal.close();
      if (options.value.loading) options.value.loading = false;
      console.log(error);
    });
};
onMounted(() => {
  initDictionary();
  //initData(true);
});
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible="props.displayDialog"
    :style="{ width: '65vw' }"
    :maximizable="true"
    :closable="false"
    style="z-index: 1000"
  >
    <form @submit.prevent="">
      <Toolbar class="outline-none surface-0 border-none">
        <template #start>
          <span class="p-input-icon-left">
            <i class="pi pi-search" />
            <InputText
              @keypress.enter="search()"
              v-model="options.search"
              type="text"
              spellcheck="false"
              placeholder="Tìm kiếm"
            />
          </span>
        </template>
      </Toolbar>
      <div class="d-lang-table" style="height: calc(100vh - 300px)">
        <DataTable
          @sort="onSort($event)"
          :value="datas"
          :lazy="true"
          :rowHover="true"
          :showGridlines="true"
          :scrollable="false"
          v-model:selection="selectedNodes"
          scrollHeight="flex"
          filterDisplay="menu"
          filterMode="lenient"
          responsiveLayout="scroll"
          rowGroupMode="rowspan"
          groupRowsBy="day_string"
        >
          <Column
            field="day_string"
            headerStyle="text-align:center;width:50px"
            bodyStyle="text-align:center;width:50px"
            class="align-items-center justify-content-center text-center"
          >
            <template #header>
              <div class="mx-2">
                <Checkbox
                  v-model="is_check_all"
                  :binary="true"
                  @change="changeCheckedAll(is_check_all)"
                />
              </div>
            </template>
            <template #body="slotProps">
              <div v-if="!slotProps.data.is_private" class="mx-2">
                <Checkbox
                  v-model="slotProps.data.is_check"
                  :binary="true"
                  @change="changeChecked(slotProps.data)"
                />
              </div>
            </template>
          </Column>
          <Column
            field="day_string"
            header="Ngày tháng"
            headerStyle="text-align:center;width:150px;height:50px;"
            bodyStyle="text-align:center;width:150px;max-height:60px"
            class="align-items-center justify-content-center text-center"
          >
            <template #body="slotProps">
              <div
                class="format-grid-center style-day"
                :class="{
                  true: slotProps.data.is_holiday,
                  false: !slotProps.data.is_holiday,
                }"
              >
                <b>{{ slotProps.data.day_name }}</b>
                <span>{{ slotProps.data.day_string }}</span>
              </div>
            </template>
          </Column>
          <Column
            field="contents"
            header="Nội dung"
            headerStyle="height:50px;max-width:auto;min-width:150px;"
            bodyStyle="max-height:60px;"
          >
            <template #body="slotProps">
              <div class="mx-2 style-day">
                <div>
                  <a v-if="slotProps.data.is_important" class="mr-2">
                    <i
                      class="pi pi-star-fill"
                      v-tooltip.top="'Lịch quan trọng'"
                      style="color: #f5b041"
                    ></i>
                  </a>
                  <a
                    v-if="
                      slotProps.data.is_coincide && slotProps.data.status !== 2
                    "
                    @click="
                      openDialogCoincide(
                        'Lịch trùng',
                        slotProps.data.calendar_id
                      )
                    "
                    style="cursor: pointer"
                    class="mr-2"
                  >
                    <i
                      class="pi pi-exclamation-triangle"
                      v-tooltip.top="'Trùng lịch'"
                      style="color: red"
                    ></i>
                  </a>
                  <a class="hover" @click="goCalendar(slotProps.data)">
                    <div v-html="slotProps.data.contents"></div>
                  </a>
                  <a
                    v-if="slotProps.data.is_type === 1"
                    class="hover mx-2"
                    @click="goMeeting()"
                    style="color: #2196f3 !important"
                    v-tooltip.top="'Meeting'"
                  >
                    <i class="pi pi-video"></i>
                  </a>
                </div>
                <div v-if="slotProps.data.day_space < 1">
                  (<span>{{
                    moment(slotProps.data.start_date).format("HH:mm")
                  }}</span>
                  <span
                    v-if="
                      slotProps.data.start_date != null &&
                      slotProps.data.end_date != null
                    "
                  >
                    -
                  </span>
                  <span>{{
                    moment(slotProps.data.end_date).format("HH:mm")
                  }}</span
                  >)
                </div>
                <div v-if="slotProps.data.day_space > 0">
                  <span>{{
                    moment(slotProps.data.start_date).format("DD/MM/YYYY")
                  }}</span>
                  <span
                    v-if="
                      slotProps.data.start_date != null &&
                      slotProps.data.end_date != null
                    "
                  >
                    - </span
                  ><span>{{
                    moment(slotProps.data.end_date).format("DD/MM/YYYY")
                  }}</span>
                </div>
                <div>
                  <span v-if="slotProps.data.is_private">(Cá nhân)</span>
                </div>
              </div>
            </template>
          </Column>
          <Column
            field="chutris"
            header="Chủ trì"
            headerStyle="text-align:center;width:100px;height:50px"
            bodyStyle="text-align:center;width:100px;max-height:60px"
            class="align-items-center justify-content-center text-center"
          >
            <template #body="slotProps">
              <div class="flex justify-content-center">
                <AvatarGroup
                  v-if="
                    slotProps.data.chutris && slotProps.data.chutris.length > 0
                  "
                >
                  <Avatar
                    v-for="(item, index) in slotProps.data.chutris.slice(0, 3)"
                    v-bind:label="
                      item.avatar ? '' : item.last_name.substring(0, 1)
                    "
                    v-bind:image="
                      item.avatar
                        ? basedomainURL + item.avatar
                        : basedomainURL + '/Portals/Image/noimg.jpg'
                    "
                    v-tooltip.top="item.full_name"
                    :key="item.user_id"
                    style="border: 2px solid orange; color: white"
                    @click="onTaskUserFilter(item)"
                    @error="basedomainURL + '/Portals/Image/noimg.jpg'"
                    size="large"
                    shape="circle"
                    class="cursor-pointer"
                    :style="{ backgroundColor: bgColor[index % 7] }"
                  />
                  <Avatar
                    v-if="
                      slotProps.data.chutris &&
                      slotProps.data.chutris.length > 3
                    "
                    v-bind:label="
                      '+' + (slotProps.data.chutris.length - 3).toString()
                    "
                    shape="circle"
                    size="large"
                    style="background-color: #2196f3; color: #ffffff"
                    class="cursor-pointer"
                  />
                </AvatarGroup>
              </div>
            </template>
          </Column>
          <Column
            field="thamgias"
            header="Thành viên tham gia"
            headerStyle="text-align:center;width:150px;height:50px"
            bodyStyle="text-align:center;width:150px;max-height:60px"
            class="align-items-center justify-content-center text-center"
          >
            <template #body="slotProps">
              <div class="flex justify-content-center">
                <AvatarGroup
                  v-if="
                    slotProps.data.thamgias &&
                    slotProps.data.thamgias.length > 0
                  "
                >
                  <Avatar
                    v-for="(item, index) in slotProps.data.thamgias.slice(0, 3)"
                    v-bind:label="
                      item.avatar ? '' : item.last_name.substring(0, 1)
                    "
                    v-bind:image="
                      item.avatar
                        ? basedomainURL + item.avatar
                        : basedomainURL + '/Portals/Image/noimg.jpg'
                    "
                    v-tooltip.top="item.full_name"
                    :key="item.user_id"
                    style="border: 2px solid white; color: white"
                    @click="onTaskUserFilter(item)"
                    @error="basedomainURL + '/Portals/Image/noimg.jpg'"
                    size="large"
                    shape="circle"
                    class="cursor-pointer"
                    :style="{ backgroundColor: bgColor[index % 7] }"
                  />
                  <Avatar
                    v-if="
                      slotProps.data.thamgias &&
                      slotProps.data.thamgias.length > 3
                    "
                    v-bind:label="
                      '+' + (slotProps.data.thamgias.length - 3).toString()
                    "
                    shape="circle"
                    size="large"
                    style="background-color: #2196f3; color: #ffffff"
                    class="cursor-pointer"
                  />
                </AvatarGroup>
              </div>
              <div class="mt-2" v-if="slotProps.data.invitee">
                Người được mời: <span v-html="slotProps.data.invitee"></span>
              </div>
              <div class="mt-2" v-if="slotProps.data.departments">
                <div>
                  Phòng ban được mời:
                  <span
                    v-for="(item, index) in slotProps.data.departments"
                    :key="index"
                  >
                    <span
                      v-if="
                        index > 0 && index < slotProps.data.departments.length
                      "
                      >,
                    </span>
                    {{ item.department_name }}
                  </span>
                </div>
              </div>
            </template>
          </Column>
          <Column
            field="car_name"
            header="Xe sử dụng"
            headerStyle="text-align:center;width:120px;height:50px"
            bodyStyle="text-align:center;width:120px;max-height:60px"
            class="align-items-center justify-content-center text-center"
          >
          </Column>
          <Column
            field="boardroom_name"
            header="Địa điểm công tác"
            headerStyle="text-align:center;width:150px;height:50px"
            bodyStyle="text-align:center;width:150px;;max-height:60px"
            class="align-items-center justify-content-center text-center"
          >
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
              v-if="!options.loading && options.total == 0"
              style="display: flex; height: calc(100vh - 365px)"
            >
              <div>
                <img src="../../../assets/background/nodata.png" height="144" />
                <h3 class="m-1">Không có dữ liệu</h3>
              </div>
            </div>
          </template>
        </DataTable>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="props.closeDialog()"
        class="p-button-text"
      />
      <Button label="Cắt cơm" icon="pi pi-check" @click="cutRice()" />
    </template>
  </Dialog>
</template>
<style scoped>
@import url(../../calendar/component/stylecalendar.css);
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