<script setup>
import { ref, inject, onMounted } from "vue";
import { socketMethod } from "../../../util/methodSocket";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import moment from "moment";
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
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

const timelots = ref([
  { value: 0, title: "Sáng" },
  { value: 1, title: "Chiều" },
  { value: 2, title: "Cả ngày" },
]);

const rules = {
  // is_timelot: {
  //   required,
  //   $errors: [
  //     {
  //       $property: "is_timelot",
  //       $validator: "required",
  //       $message: "Ca trực không được để trống!",
  //     },
  //   ],
  // },
};
const v$ = useVuelidate(rules, props.model);

//Get arguments
const props = defineProps({
  temp: Boolean,
  headerDialog: String,
  displayDialog: Boolean,
  closeDialog: Function,
  isAdd: Boolean,
  submitted: Boolean,
  model: Object,
  files: Array,
  selectFile: Function,
  removeFile: Function,
  initData: Function,
  databoardrooms: Array,
  datatrucbans: Array,
  datachihuys: Array,
});

//Function
const submitted = ref(props.submitted);
function isValidDate(d) {
  return d instanceof Date && !isNaN(d);
}
const changeDate = () => {
  props.model["is_timelot"] = 0;
  props.model["date_timelot"] = new Date(
    props.model["date_timelot"].getFullYear(),
    props.model["date_timelot"].getMonth(),
    props.model["date_timelot"].getDate(),
    7,
    0,
    0
  );
  props.model["date_timelot_time"] = props.model["date_timelot"];
};
const changeTimelot = (is_timelot) => {
  if (is_timelot === 0) {
    props.model["date_timelot"] = new Date(
      props.model["date_timelot"].getFullYear(),
      props.model["date_timelot"].getMonth(),
      props.model["date_timelot"].getDate(),
      7,
      0
    );
  } else if (is_timelot === 1) {
    props.model["date_timelot"] = new Date(
      props.model["date_timelot"].getFullYear(),
      props.model["date_timelot"].getMonth(),
      props.model["date_timelot"].getDate(),
      13,
      0
    );
  }
  props.model["date_timelot_time"] = props.model["date_timelot"];
};
const changeTime = () => {
  if (isValidDate(props.model["date_timelot_time"])) {
    props.model["date_timelot"] = new Date(
      props.model["date_timelot"].getFullYear(),
      props.model["date_timelot"].getMonth(),
      props.model["date_timelot"].getDate(),
      props.model["date_timelot_time"].getHours(),
      props.model["date_timelot_time"].getMinutes(),
      0
    );
  }
};
const goFile = (file) => {
  window.open(basedomainURL + file.file_path, "_blank");
};
//Function choice user
// reload component
const selectedUser = ref([]);
const is_one = ref(false);
const is_type = ref();
const headerDialogUser = ref();
const displayDialogUser = ref(false);
const showModalUser = (one, type) => {
  if (type != null) {
    switch (type) {
      case 0:
        selectedUser.value = [...props.model.trucbans];
        headerDialogUser.value = "Chọn người trực ban";
        break;
      case 1:
        selectedUser.value = [...props.model.chihuys];
        headerDialogUser.value = "Chọn người chỉ huy";
        break;
      default:
        break;
    }
  }
  is_one.value = one;
  is_type.value = type;
  displayDialogUser.value = true;
};
const choiceUser = () => {
  if (is_type.value != null) {
    switch (is_type.value) {
      case 0:
        var notexist = selectedUser.value.filter(
          (a) =>
            props.model.trucbans.findIndex(
              (b) => b["user_id"] === a["user_id"]
            ) === -1
        );
        if (notexist.length > 0) {
          props.model.trucbans = notexist;
        }
        break;
      case 1:
        var notexist = selectedUser.value.filter(
          (a) =>
            props.model.chihuys.findIndex(
              (b) => b["user_id"] === a["user_id"]
            ) === -1
        );
        if (notexist.length > 0) {
          props.model.chihuys = notexist;
        }
        break;
      default:
        break;
    }
  }
};
const removeFile = (event) => {
  props.files = [];
};
const selectFile = (event) => {
  event.files.forEach((element) => {
    props.files.push(element);
  });
};
const saveModel = (is_continue) => {
  submitted.value = true;
  if (
    !props.model.date_timelot ||
    !props.model.trucban ||
    props.model.trucban.length === 0
  ) {
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
  var obj = { ...props.model };
  if (obj.date_timelot != null) {
    obj.date_timelot = moment(obj.date_timelot).format("YYYY-MM-DDTHH:mm:ssZZ");
  }
  var checkroom = props.databoardrooms.findIndex(
    (x) => x["boardroom_id"] === obj["boardroom_id"]
  );
  if (checkroom === -1) {
    obj["place_name"] = obj["boardroom_id"];
    obj["boardroom_id"] = null;
  }
  var trucban = null;
  if (obj.trucban != null) {
    trucban = obj.trucban["user_id"];
  }
  var chihuy = null;
  if (obj.chihuy != null) {
    chihuy = obj.chihuy["user_id"];
  }

  let formData = new FormData();
  formData.append("isAdd", props.isAdd);
  formData.append("model", JSON.stringify(obj));
  formData.append("trucban", trucban);
  formData.append("chihuy", chihuy);
  for (var i = 0; i < props.files.length; i++) {
    let file = props.files[i];
    formData.append("files", file);
  }
  axios
    .put(baseURL + "/api/calendar_duty/update_calendar_duty", formData, config)
    .then((response) => {
      if (response.data.err === "1" || response.data.err === "2") {
        swal.fire({
          title: "Thông báo!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
      swal.close();
      toast.success(
        props.isAdd
          ? "Thêm lịch trực ban thành công!"
          : "Cập nhật lịch trực ban thành công!"
      );
      if (!is_continue) {
        props.closeDialog();
      }
      props.initData(true);
      // if (response.data.data != null) {
      //   var datas = JSON.parse(response.data.data);
      //   if (datas != null && datas.length > 0) {
      //     datas.forEach((item) => {
      //       socketMethod
      //         .post("sendnotification", {
      //           uids: item["uids"],
      //           options: {
      //             title: item["title"],
      //             text: item["text"],
      //             image:
      //               baseURL +
      //               (store.getters.user.background_image ||
      //                 "../assets/background/bg.png"),
      //             tag: "project.soe.vn",
      //             url: "/calendar/detail/".concat(item["calendar_duty_id"]),
      //           },
      //         })
      //         .then((res) => {});
      //     });
      //   }
      // }
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
  if (submitted.value) submitted.value = false;
};
const saveTemp = () => {
  submitted.value = true;
  if (
    !props.model.date_timelot ||
    !props.model.trucban ||
    props.model.trucban.length === 0
  ) {
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
  if (props.files != null && props.files.length > 0) {
    props.model.files = props.files;
  }
  props.initData(props.model);
  if (submitted.value) submitted.value = false;
};
const deleteFile = (item) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá tệp đính kèm này không!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then((result) => {
      if (result.isConfirmed) {
        swal.fire({
          width: 110,
          didOpen: () => {
            swal.showLoading();
          },
        });
        var ids = [item["file_id"]];
        axios
          .delete(baseURL + "/api/calendar_duty/delete_file", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: ids,
          })
          .then((response) => {
            if (response.data.err === "1") {
              swal.fire({
                title: "Thông báo!",
                text: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
              });
              return;
            }
            if (ids.length > 0) {
              ids.forEach((element, i) => {
                var idx = props.model.files.findIndex(
                  (x) => x.file_id == element
                );
                if (idx != -1) {
                  props.model.files.splice(idx, 1);
                }
              });
            }
            swal.close();
            toast.success("Xoá tệp đính kèm thành công!");
          })
          .catch((error) => {
            swal.close();
            addLog({
              title: "Lỗi Console delItem",
              controller: "boardroom.vue",
              logcontent: error.message,
              loai: 2,
            });
            if (error.status === 401) {
              swal.fire({
                title: "Thông báo!",
                text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
              return;
            }
          });
      }
    });
};

//Init
onMounted(() => {
  return {};
});
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible="props.displayDialog"
    :style="{ width: '40vw' }"
    :maximizable="true"
    :closable="false"
    style="z-index: 9000"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Ngày trực <span class="redsao">(*)</span></label>
            <div>
              <Calendar
                :showIcon="true"
                class="ip36"
                autocomplete="on"
                inputId="time24"
                :class="{
                  'p-invalid': props.model.date_timelot == null && submitted,
                }"
                v-model="props.model.date_timelot"
                @date-select="changeDate()"
                @input="changeDate()"
              />
            </div>
            <div v-if="props.model.date_timelot == null && submitted">
              <small class="p-error">
                <span class="col-12 p-0">Ngày trực không được để trống</span>
              </small>
            </div>
          </div>
        </div>
        <!-- <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Ca trực</label>
            <Dropdown
              :options="timelots"
              :filter="false"
              :showClear="false"
              :editable="false"
              v-model="props.model.is_timelot"
              optionLabel="title"
              optionValue="value"
              placeholder="Chọn ca trực"
              class="ip36"
              @change="changeTimelot(props.model.is_timelot)"
            >
              <template #option="slotProps">
                <div class="country-item flex align-items-center">
                  <div class="pt-1 pl-2">
                    {{ slotProps.option.title }}
                  </div>
                </div>
              </template>
            </Dropdown>
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Thời gian</label>
            <div>
              <Calendar
                inputId="time12"
                hourFormat="24"
                class="ip36"
                autocomplete="on"
                icon="pi pi-clock"
                :showIcon="true"
                :timeOnly="true"
                v-model="props.model.date_timelot_time"
                @date-select="changeTime()"
                @input="changeTime()"
              />
            </div>
          </div>
        </div> -->
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Nội dung</label>
            <Textarea
              v-model="props.model.contents"
              :autoResize="true"
              rows="5"
              cols="30"
            />
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Người trực ban <span class="redsao">(*)</span></label>
            <Dropdown
              :options="props.datatrucbans"
              :filter="true"
              :showClear="true"
              :editable="false"
              optionLabel="full_name"
              placeholder="Chọn người trực ban"
              v-model="props.model.trucban"
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
                          style="
                            background-color: #2196f3;
                            color: #ffffff;
                            width: 2rem;
                            height: 2rem;
                          "
                          :style="{
                            background: bgColor[slotProps.value.is_order % 7],
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
                      style="
                        background-color: #2196f3;
                        color: #ffffff;
                        width: 3rem;
                        height: 3rem;
                        font-size: 1.4rem !important;
                      "
                      :style="{
                        background: bgColor[slotProps.option.is_order % 7],
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
                <span v-else> Chưa có dữ liệu tuần </span>
              </template>
            </Dropdown>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Người chỉ huy</label>
            <Dropdown
              :options="props.datachihuys"
              :filter="true"
              :showClear="true"
              :editable="false"
              optionLabel="full_name"
              placeholder="Chọn người chỉ huy"
              v-model="props.model.chihuy"
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
                          style="
                            background-color: #2196f3;
                            color: #ffffff;
                            width: 2rem;
                            height: 2rem;
                          "
                          :style="{
                            background: bgColor[slotProps.value.is_order % 7],
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
                      style="
                        background-color: #2196f3;
                        color: #ffffff;
                        width: 3rem;
                        height: 3rem;
                        font-size: 1.4rem !important;
                      "
                      :style="{
                        background: bgColor[slotProps.option.is_order % 7],
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
                <span v-else> Chưa có dữ liệu tuần </span>
              </template>
            </Dropdown>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Địa điểm</label>
            <Dropdown
              :options="props.databoardrooms"
              :filter="true"
              :showClear="true"
              :editable="true"
              v-model="props.model.boardroom_id"
              optionLabel="boardroom_name"
              optionValue="boardroom_id"
              placeholder="Chọn địa điểm"
              class="ip36"
            >
              <template #option="slotProps">
                <div class="country-item flex align-items-center">
                  <div class="pt-1 pl-2">
                    {{ slotProps.option.boardroom_name }}
                  </div>
                </div>
              </template>
            </Dropdown>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Ghi chú</label>
            <Textarea
              v-model="props.model.note"
              :autoResize="true"
              rows="5"
              cols="30"
            />
          </div>
        </div>
        <div class="col-12 md-col-12">
          <div class="form-group">
            <label>Tệp đính kèm</label>
            <FileUpload
              :multiple="false"
              :show-upload-button="false"
              :show-cancel-button="true"
              :fileLimit="1"
              invalidFileLimitMessage="Giới hạn cho phép tối đa 1 tệp đính kèm"
              @remove="removeFile"
              @select="selectFile"
              name="demo[]"
              url="./upload.php"
              accept=""
              choose-label="Chọn tệp"
              cancel-label="Hủy"
            >
              <template #empty>
                <p>Kéo thả tệp đính kèm vào đây.</p>
              </template>
            </FileUpload>
            <div
              v-if="props.model.files != null && props.model.files.length > 0"
            >
              <DataView
                :lazy="true"
                :value="props.model.files"
                :rowHover="true"
                :scrollable="true"
                class="w-full h-full ptable p-datatable-sm flex flex-column"
                layout="list"
                responsiveLayout="scroll"
              >
                <template #list="slotProps">
                  <div class="w-full">
                    <Toolbar class="w-full">
                      <template #start>
                        <div
                          @click="goFile(slotProps.data)"
                          class="flex align-items-center"
                        >
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
                          <span style="line-height: 1.5">
                            {{ slotProps.data.file_name }}</span
                          >
                        </div>
                      </template>
                      <template #end>
                        <Button
                          icon="pi pi-times"
                          class="p-button-rounded p-button-danger"
                          @click="deleteFile(slotProps.data)"
                        />
                      </template>
                    </Toolbar>
                  </div>
                </template>
              </DataView>
            </div>
          </div>
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
      <Button
        v-if="props.temp"
        label="Lưu"
        icon="pi pi-check"
        @click="saveTemp()"
      ></Button>
      <Button
        v-if="!props.temp && props.isAdd"
        label="Lưu và tiếp tục"
        icon="pi pi-check"
        @click="saveModel(true)"
      />
      <Button
        v-if="!props.temp"
        label="Lưu"
        icon="pi pi-check"
        @click="saveModel()"
      />
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
