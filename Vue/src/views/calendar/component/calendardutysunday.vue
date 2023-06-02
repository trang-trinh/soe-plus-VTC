<script setup>
import { onMounted, inject, ref } from "vue";
import { encr } from "../../../util/function";
import { useToast } from "vue-toastification";

const cryoptojs = inject("cryptojs");
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const emitter = inject("emitter");
const isDynamicSQL = ref(false);
const basedomainURL = baseURL;
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
var newDate = new Date();
const options = ref({
  year: newDate.getFullYear(),
});
const weeks = ref([]);
const dictionarys = ref([]);
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);

//Function
const submitted = ref(false);
const model = ref({});
const headerDialogWeek = ref();
const displayDialogWeek = ref(false);
const openDialogWeek = (week) => {
  options.value.loading = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_duty_sunday_getweek",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "year", va: options.value.year },
              { par: "week", va: week },
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
          var tbs = JSON.parse(data);
          if (tbs[0] != null && tbs[0].length > 0) {
            model.value = tbs[0][0];
            if (model.value["user_id"] != null) {
              model.value["user"] = {
                user_id: model.value["user_id"],
                full_name: model.value["full_name"],
                last_name: model.value["last_name"],
                avatar: model.value["avatar"],
              };
            }
          } else {
            model.value = {
              user: null,
              year: options.value.year,
              week: week,
            };
          }
        }
      }
      swal.close();
      if (options.value.loading) options.value.loading = false;

      headerDialogWeek.value = "Trực chủ nhật tuần " + week;
      displayDialogWeek.value = true;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      if (options.value.loading) options.value.loading = false;
      addLog({
        title: "Lỗi Console editItem",
        controller: "calendarweek.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
        return;
      }
    });
};
const closeDialogWeek = () => {
  displayDialogWeek.value = false;
};

const saveModel = () => {
  submitted.value = true;
  if (!model.value.user || !model.value.user.user_id) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng điền đầy đủ thông tin trường bôi đỏ!",
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
  var obj = JSON.parse(JSON.stringify(model.value));
  if (obj.user != null) {
    obj.user_id = obj.user["user_id"];
  }
  let formData = new FormData();
  formData.append("model", JSON.stringify(obj));
  axios
    .put(
      baseURL + "/api/calendar_week/update_calendar_duty_sunday",
      formData,
      config
    )
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
      toast.success("Cập nhật thành công");
      closeDialogWeek();
      initData(true);
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
};

//init
const initData = (rf) => {
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
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_duty_sunday_get",
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
          dictionarys.value = tbs;
        }
      }
      swal.close();
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
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
  initData(true);
});
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible="props.displayDialog"
    :style="{ width: '60vw' }"
    :maximizable="true"
    :closable="false"
    style="z-index: 9000"
  >
    <form @submit.prevent="" name="submitform">
      <div class="grid formgrid m-2">
        <div class="col-12 md:col-12">
          <div class="ul-weeks row">
            <div
              class="li-week col-2 md:col-2"
              v-for="(week, index) in weeks"
              @click="openDialogWeek(week.week_no)"
              :key="index"
              :class="{
                isPass: week.pass,
                isCurrentWeek: week.is_current_week,
              }"
            >
              <div class="p-2">
                <div class="mb-1"><b>Tuần {{ week.week_no }}</b> </div>
                <div :style="{ fontSize: '11px' }">Từ ({{ week.week_start_date_short }} - {{ week.week_end_date_short }})</div>
              </div>
              <div v-if="week.user_id" class="p-2 format-center">
                <div>
                  <div class="mb-2">
                    <Avatar
                      v-bind:label="
                        week.avatar
                          ? ''
                          : (week.last_name ?? '').substring(0, 1)
                      "
                      v-bind:image="
                        week.avatar
                          ? basedomainURL + week.avatar
                          : basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                      :style="{
                        background: bgColor[index % 7],
                        color: '#ffffff',
                        width: '3rem',
                        height: '3rem',
                        fontSize: '1rem !important',
                      }"
                      class="mr-2 text-avatar"
                      size="xlarge"
                      shape="circle"
                    />
                  </div>
                  <div>{{ week.full_name }}</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Đóng"
        icon="pi pi-times"
        @click="props.closeDialog()"
        class="p-button-text"
      />
    </template>
  </Dialog>
  <Dialog
    :header="headerDialogWeek"
    v-model:visible="displayDialogWeek"
    :style="{ width: '35vw' }"
    :maximizable="false"
    :closable="false"
    style="z-index: 9001"
  >
    <form @submit.prevent="" name="submitform">
      <div class="grid formgrid m-2">
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Người trực <span class="redsao">(*)</span></label>
            <Dropdown
              :options="dictionarys[1]"
              :filter="true"
              :showClear="true"
              :editable="false"
              :class="{
                'p-invalid': !model.user && submitted,
              }"
              optionLabel="full_name"
              placeholder="Chọn người trực"
              v-model="model.user"
              class="ip36"
              style="height: auto; min-height: 36px"
            >
              <template #value="slotProps">
                <div class="mt-2" v-if="slotProps.value">
                  <Chip
                    :image="slotProps.value.avatar"
                    :label="slotProps.value.full_name"
                    class="mr-2 mb-2 pl-0"
                  >
                    <div class="flex">
                      <div class="format-flex-center">
                        <Avatar
                          v-bind:label="
                            slotProps.value.avatar
                              ? ''
                              : (slotProps.value.last_name ?? '').substring(
                                  0,
                                  1
                                )
                          "
                          v-bind:image="
                            slotProps.value.avatar
                              ? basedomainURL + slotProps.value.avatar
                              : basedomainURL + '/Portals/Image/noimg.jpg'
                          "
                          :style="{
                            background: bgColor[1 % 7],
                            color: '#ffffff',
                            width: '2rem',
                            height: '2rem',
                          }"
                          class="mr-2 text-avatar"
                          size="xlarge"
                          shape="circle"
                        />
                      </div>
                      <div class="format-flex-center">
                        <span>{{ slotProps.value.full_name }}</span>
                      </div>
                    </div>
                  </Chip>
                </div>
                <span v-else> {{ slotProps.placeholder }} </span>
              </template>
              <template #option="slotProps">
                <div v-if="slotProps.option" class="flex">
                  <div class="format-center">
                    <Avatar
                      v-bind:label="
                        slotProps.option.avatar
                          ? ''
                          : slotProps.option.last_name.substring(0, 1)
                      "
                      v-bind:image="
                        slotProps.option.avatar
                          ? basedomainURL + slotProps.option.avatar
                          : basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                      style=""
                      :style="{
                        background: bgColor[slotProps.index % 7],
                        color: '#ffffff',
                        width: '3rem',
                        height: '3rem',
                        fontSize: '1.4rem !important',
                      }"
                      class="text-avatar"
                      size="xlarge"
                      shape="circle"
                    />
                  </div>
                  <div class="ml-3">
                    <div class="mb-1">{{ slotProps.option.full_name }}</div>
                    <div class="description">
                      <div>{{ slotProps.option.position_name }}</div>
                      <div>{{ slotProps.option.department_name }}</div>
                    </div>
                  </div>
                </div>
                <span v-else> Chưa có dữ liệu </span>
              </template>
            </Dropdown>
            <div v-if="!model.user && submitted">
              <small class="p-error">
                <span class="col-12 p-0"
                  >Nội dung cuộc họp không được để trống</span
                >
              </small>
            </div>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Ghi chú</label>
            <Textarea
              v-model="model.note"
              :autoResize="true"
              rows="5"
              cols="30"
            />
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialogWeek()"
        class="p-button-text"
      />
      <Button label="Lưu" icon="pi pi-check" @click="saveModel()" />
    </template>
  </Dialog>
</template>
<style scoped>
@import url(./stylecalendar.css);
.ul-weeks {
  padding: 0;
  list-style: none;
}
.li-week {
  padding: 0;
  display: inline-block;
  height: 120px;
  box-shadow: 0px 0px 0.2px 0.2px #888888;
}
.li-week:hover {
  cursor: pointer;
  background-color: aliceblue;
  color: #000;
}
.isPass {
  background-color: #eee;
}
.isCurrentWeek {
  background-color: #6fd234;
  color: #fff;
}
</style>
<style lang="scss" scoped>
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
