<script setup>
import { ref, inject, onMounted } from "vue";
import moment from "moment";
import { socketMethod } from "../../../util/methodSocket";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import treeuser from "../../../components/user/treeuser.vue";
import { de } from "date-fns/locale";
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
  temp: Boolean,
  headerDialog: String,
  displayDialog: Boolean,
  closeDialog: Function,
  isAdd: Boolean,
  submitted: Boolean,
  loading: Boolean,
  group: Number,
  model: Object,
  files: Array,
  selectFile: Function,
  removeFile: Function,
  boardrooms: Array,
  departments: Array,
  users: Array,
  cars: Array,
  initData: Function,
});

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
const types = ref([
  { is_type: 0, name: "Họp bình thường" },
  { is_type: 1, name: "Họp trực tuyến" },
]);
const iterations = ref([
  { is_iterations: 0, name: "Không lặp", short: "ngày" },
  { is_iterations: 1, name: "Lặp theo ngày", short: "ngày" },
  { is_iterations: 2, name: "Lặp theo tuần", short: "tuần" },
  { is_iterations: 3, name: "Lặp theo tháng", short: "tháng" },
  { is_iterations: 4, name: "Lặp theo năm", short: "năm" },
]);
const submitted = ref(props.submitted);

//Function
function isValidDate(d) {
  return d instanceof Date && !isNaN(d);
}
const changeStartDate = () => {
  var start_date = new Date(props.model["start_date"]);
  if (isValidDate(props.model["start_date_time"])) {
    props.model["start_date"] = new Date(
      start_date.getFullYear(),
      start_date.getMonth(),
      start_date.getDate(),
      props.model["start_date_time"].getHours(),
      props.model["start_date_time"].getMinutes(),
      0
    );
    props.model["end_date_time"] = props.model["start_date"];
  } else {
    props.model.start_date = new Date(
      start_date.getFullYear(),
      start_date.getMonth(),
      start_date.getDate(),
      7,
      0,
      0
    );
  }
  props.model.end_date = props.model.start_date;
};
const changeEndDate = () => {
  var end_date = new Date(props.model["end_date"]);
  if (isValidDate(props.model["end_date_time"])) {
    props.model["end_date"] = new Date(
      end_date.getFullYear(),
      end_date.getMonth(),
      end_date.getDate(),
      props.model["end_date_time"].getHours(),
      props.model["end_date_time"].getMinutes(),
      0
    );
  } else {
    props.model.end_date = new Date(
      end_date.getFullYear(),
      end_date.getMonth(),
      end_date.getDate(),
      7,
      0,
      0
    );
  }
};
const changeStartTime = () => {
  if (isValidDate(props.model["start_date_time"])) {
    props.model["start_date"] = new Date(
      props.model["start_date"].getFullYear(),
      props.model["start_date"].getMonth(),
      props.model["start_date"].getDate(),
      props.model["start_date_time"].getHours(),
      props.model["start_date_time"].getMinutes(),
      0
    );
  }
};
const changeEndTime = () => {
  if (isValidDate(props.model["end_date_time"])) {
    props.model["end_date"] = new Date(
      props.model["end_date"].getFullYear(),
      props.model["end_date"].getMonth(),
      props.model["end_date"].getDate(),
      props.model["end_date_time"].getHours(),
      props.model["end_date_time"].getMinutes(),
      0
    );
  }
};
const saveModel = (is_continue) => {
  submitted.value = true;
  let contents = document.getElementById("contents");
  if (contents) {
    props.model.contents = contents.innerHTML;
  }
  if (
    !props.model.contents ||
    (props.group === 0 && !props.model.boardroom_id) ||
    !props.model.start_date ||
    !props.model.end_date
  ) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng điền đầy đủ thông tin trường bôi đỏ!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (props.model.start_date > props.model.end_date) {
    swal.fire({
      title: "Thông báo!",
      text: "Ngày giờ bắt đầu phải nhỏ hơn ngày giờ kết thúc!",
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
  if (obj.start_date != null) {
    obj.start_date = moment(obj.start_date).format("YYYY-MM-DDTHH:mm:ssZZ");
  }
  if (obj.end_date != null) {
    obj.end_date = moment(obj.end_date).format("YYYY-MM-DDTHH:mm:ssZZ");
  }
  if (obj.create_date != null) {
    obj.create_date = moment(obj.create_date).format("YYYY-MM-DDTHH:mm:ssZZ");
  }
  var checkroom = props.boardrooms.findIndex(
    (x) => x["boardroom_id"] === (obj["boardroom_id"] || "")
  );
  if (checkroom === -1) {
    obj["place_name"] = obj["boardroom_id"] || "";
    obj["boardroom_id"] = null;
  }
  var chutris = obj.chutris
    .filter((x) => x["member_id"] == null)
    .map((x) => x["user_id"]);
  var thamgias = obj.thamgias
    .filter((x) => x["member_id"] == null)
    .map((x) => x["user_id"]);
  var phongbans = obj.departments.map((x) => x);
  let formData = new FormData();
  formData.append("isAdd", props.isAdd);
  formData.append("model", JSON.stringify(obj));
  formData.append("chutris", JSON.stringify(chutris));
  formData.append("thamgias", JSON.stringify(thamgias));
  formData.append("phongbans", JSON.stringify(phongbans));
  for (var i = 0; i < props.files.length; i++) {
    let file = props.files[i];
    formData.append("files", file);
  }
  axios
    .put(baseURL + "/api/calendar_week/update_calendar_week", formData, config)
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
      toast.success(
        props.isAdd
          ? "Thêm lịch họp thành công!"
          : "Cập nhật lịch họp thành công!"
      );
      if (!is_continue) {
        props.closeDialog();
      }
      props.initData(true);
      if (response.data.data != null) {
        var datas = JSON.parse(response.data.data);
        if (datas != null && datas.length > 0) {
          datas.forEach((item) => {
            socketMethod
              .post("sendnotification", {
                uids: item["uids"],
                options: {
                  title: item["title"],
                  text: item["text"],
                  image:
                    baseURL +
                    (store.getters.user.background_image ||
                      "../assets/background/bg.png"),
                  tag: "project.soe.vn",
                  url: "/calendar/detail/".concat(item["calendar_id"]),
                },
              })
              .then((res) => {});
          });
        }
      }
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
const saveTemp = () => {
  submitted.value = true;
  let contents = document.getElementById("contents");
  if (contents) {
    props.model.contents = contents.innerHTML;
  }
  if (
    !props.model.contents ||
    (props.group === 0 && !props.model.boardroom_id) ||
    !props.model.start_date ||
    !props.model.end_date
  ) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng điền đầy đủ thông tin trường bôi đỏ!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (props.model.start_date > props.model.end_date) {
    swal.fire({
      title: "Thông báo!",
      text: "Ngày giờ bắt đầu phải nhỏ hơn ngày giờ kết thúc!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (props.files != null && props.files.length > 0) {
    props.model.files = props.files;
  }
  props.initData(props.model);
  submitted.value = false;
};
const goFile = (file) => {
  window.open(basedomainURL + file.file_path, "_blank");
};
const deleteFile = (item, idx) => {
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
        if (item["file_id"] != null) {
          var ids = [item["file_id"]];
          axios
            .delete(baseURL + "/api/calendar_week/delete_file", {
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
                  text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                  icon: "error",
                  confirmButtonText: "OK",
                });
                return;
              }
            });
        } else {
          props.model.files.splice(idx, 1);
          swal.close();
        }
      }
    });
};

//Function choice user
// reload component
const componentKey = ref(0);
const forceRerender = () => {
  componentKey.value += 1;
};
const selectedUser = ref([]);
const is_one = ref(false);
const is_type = ref();
const headerDialogUser = ref();
const displayDialogUser = ref(false);
const showModalUser = (one, type) => {
  if (type != null) {
    switch (type) {
      case 0:
        selectedUser.value = [...props.model.chutris];
        headerDialogUser.value = "Chọn người chủ trì";
        break;
      case 1:
        selectedUser.value = [...props.model.thamgias];
        headerDialogUser.value = "Chọn người tham gia";
        break;
      default:
        break;
    }
  }

  is_one.value = one;
  is_type.value = type;
  displayDialogUser.value = true;
  forceRerender();
};
const closeDialogUser = () => {
  displayDialogUser.value = false;
};
const choiceUser = () => {
  if (is_type.value != null) {
    switch (is_type.value) {
      case 0:
        var notexist = selectedUser.value.filter(
          (a) =>
            props.model.chutris.findIndex(
              (b) => b["user_id"] === a["user_id"]
            ) === -1
        );
        if (notexist.length > 0) {
          props.model.chutris = notexist;
        }
        break;
      case 1:
        var notexist = selectedUser.value.filter(
          (a) =>
            props.model.thamgias.findIndex(
              (b) => b["user_id"] === a["user_id"]
            ) === -1
        );
        if (notexist.length > 0) {
          props.model.thamgias = props.model.thamgias.concat(notexist);
        }
        break;
      default:
        break;
    }
  }

  props.model.numeric_attendees =
    props.model.chutris.length + props.model.thamgias.length;
  closeDialogUser();
};
const changeAttendees = () => {
  props.model.numeric_attendees =
    props.model.chutris.length + props.model.thamgias.length;
};
const removeMember = (user, arr) => {
  if (user["member_id"] != null) {
    swal
      .fire({
        title: "Thông báo",
        text: "Bạn có muốn xoá thành viên này không!",
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
          var ids = [user["member_id"]];
          axios
            .delete(baseURL + "/api/calendar_week/delete_member", {
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
              ids.forEach((value, i) => {
                var idx = arr.findIndex((x) => x["member_id"] === value);
                if (idx != -1) {
                  arr.splice(idx, 1);
                  props.model.numeric_attendees -= 1;
                }
              });

              swal.close();
              toast.success("Xoá thành viên thành công!");
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
                  text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                  icon: "error",
                  confirmButtonText: "OK",
                });
                return;
              }
            });
        }
      });
  } else {
    var idx = arr.findIndex((x) => x["user_id"] === user["user_id"]);
    if (idx != -1) {
      arr.splice(idx, 1);
      props.model.numeric_attendees -= 1;
    }
  }
};

onMounted(() => {
  setTimeout(() => {
    if (document.getElementById("bold_button")) {
      document.getElementById("bold_button").onclick = () => {
        document.execCommand("bold");
      };
    }
    if (document.getElementById("italic_button")) {
      document.getElementById("italic_button").onclick = () => {
        document.execCommand("italic");
      };
    }
    if (document.getElementById("underline_button")) {
      document.getElementById("underline_button").onclick = () => {
        document.execCommand("underline");
      };
    }
  }, 500);
});
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible="props.displayDialog"
    :style="{ width: '50vw' }"
    :maximizable="true"
    :closable="false"
    style="z-index: 9000"
    class="test"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 md:col-12">
          <div class="form-group">
            <div class="format-center justify-content-between">
              <label>Nội dung <span class="redsao">(*)</span></label>
              <ul class="p-0 flex" :style="{ listStyle: 'none' }">
                <li class="mr-1">
                  <Button
                    id="bold_button"
                    class="p-button-outlined"
                    :style="{ width: '35px', textAlign: 'center' }"
                    ><b>B</b></Button
                  >
                </li>
                <li class="mr-1">
                  <Button
                    id="italic_button"
                    class="p-button-outlined"
                    :style="{ width: '35px', textAlign: 'center' }"
                    ><i>I</i></Button
                  >
                </li>
                <li class="mr-1">
                  <Button
                    id="underline_button"
                    class="p-button-outlined"
                    :style="{ width: '35px', textAlign: 'center' }"
                    >U</Button
                  >
                </li>
              </ul>
            </div>

            <div
              contentEditable="true"
              id="contents"
              class="box-contents w-full"
              v-html="props.model.contents"
              :class="{
                'p-invalid': !props.model.contents && submitted,
              }"
              :style="{
                minHeight: '100px',
                border: 'solid 1px #ced4da',
                borderRadius: '3px',
                padding: '0.5rem',
                backgroundColor: '#fff',
              }"
            ></div>
            <div v-if="!props.model.contents && submitted">
              <small class="p-error">
                <span class="col-12 p-0"
                  >Nội dung cuộc họp không được để trống</span
                >
              </small>
            </div>
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Ngày bắt đầu <span class="redsao">(*)</span></label>
            <div>
              <Calendar
                :showIcon="true"
                class="ip36"
                autocomplete="on"
                inputId="time24"
                :class="{
                  'p-invalid': !props.model.start_date && submitted,
                }"
                v-model="props.model.start_date"
                @date-select="changeStartDate()"
                @input="changeStartDate()"
              />
            </div>
            <div v-if="!props.model.start_date && submitted">
              <small class="p-error">
                <span class="col-12 p-0">Ngày bắt đầu không được để trống</span>
              </small>
            </div>
          </div>
        </div>
        <div v-if="props.group === 0" class="col-6 md:col-6">
          <div class="form-group">
            <label>Giờ bắt đầu <span class="redsao">(*)</span></label>
            <div>
              <Calendar
                inputId="time12"
                hourFormat="24"
                class="ip36"
                autocomplete="on"
                icon="pi pi-clock"
                :showIcon="true"
                :timeOnly="true"
                :class="{
                  'p-invalid': !props.model.start_date_time && submitted,
                }"
                v-model="props.model.start_date_time"
                @date-select="changeStartTime()"
                @input="changeStartTime()"
              />
            </div>
            <div v-if="!props.model.start_date_time && submitted">
              <small class="p-error">
                <span class="col-12 p-0">Giờ bắt đầu không được để trống</span>
              </small>
            </div>
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Ngày kết thúc <span class="redsao">(*)</span></label>
            <div>
              <Calendar
                :showIcon="true"
                class="ip36"
                autocomplete="on"
                inputId="time24"
                :class="{
                  'p-invalid': !props.model.end_date && submitted,
                }"
                v-model="props.model.end_date"
                @date-select="changeEndDate()"
              />
            </div>
            <div v-if="!props.model.end_date && submitted">
              <small class="p-error">
                <span class="col-12 p-0"
                  >Ngày kết thúc không được để trống</span
                >
              </small>
            </div>
          </div>
        </div>
        <div v-if="props.group === 0" class="col-6 md:col-6">
          <div class="form-group">
            <label>Giờ kết thúc <span class="redsao">(*)</span></label>
            <div>
              <Calendar
                inputId="time12"
                hourFormat="24"
                class="ip36"
                autocomplete="on"
                icon="pi pi-clock"
                :showIcon="true"
                :timeOnly="true"
                :class="{
                  'p-invalid': !props.model.end_date_time && submitted,
                }"
                v-model="props.model.end_date_time"
                @date-select="changeEndTime()"
                @input="changeEndTime()"
              />
            </div>
            <div v-if="!props.model.end_date_time && submitted">
              <small class="p-error">
                <span class="col-12 p-0">Giờ kết thúc không được để trống</span>
              </small>
            </div>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label
              >Người chủ trì
              <i
                class="pi pi-user-plus ml-2"
                v-tooltip.top="'Chọn người dùng'"
                @click="showModalUser(true, 0)"
                style="cursor: pointer; color: #2196f3"
              ></i
            ></label>
            <MultiSelect
              :options="props.users"
              :filter="true"
              :showClear="true"
              :editable="false"
              :selectionLimit="1"
              optionLabel="full_name"
              placeholder="Chọn người chủ trì"
              v-model="props.model.chutris"
              class="ip36"
              style="height: auto; min-height: 36px"
              @selectall-change="changeAttendees()"
              @change="changeAttendees()"
            >
              <template #value="slotProps">
                <ul
                  class="p-ulchip"
                  v-if="slotProps.value && slotProps.value.length > 0"
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
                                : (value.last_name ?? '').substring(0, 1)
                            "
                            v-bind:image="
                              value.avatar
                                ? basedomainURL + value.avatar
                                : basedomainURL + '/Portals/Image/noimg.jpg'
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
                        <span
                          tabindex="0"
                          class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                          @click="removeMember(value, props.model.chutris)"
                        ></span>
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
                    <div class="mb-1">{{ slotProps.option.full_name }}</div>
                    <div class="description">
                      <div>{{ slotProps.option.position_name }}</div>
                      <div>{{ slotProps.option.department_name }}</div>
                    </div>
                  </div>
                </div>
                <span v-else> Chưa có dữ liệu tuần </span>
              </template>
            </MultiSelect>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label
              >Người tham gia
              <i
                class="pi pi-user-plus ml-2"
                v-tooltip.top="'Chọn người dùng'"
                @click="showModalUser(false, 1)"
                style="cursor: pointer; color: #2196f3"
              ></i
            ></label>
            <MultiSelect
              :options="props.users"
              :filter="true"
              :showClear="true"
              :editable="false"
              optionLabel="full_name"
              placeholder="Chọn người tham gia"
              v-model="props.model.thamgias"
              class="ip36"
              style="height: auto; min-height: 36px"
              @selectall-change="changeAttendees()"
              @change="changeAttendees()"
            >
              <template #value="slotProps">
                <ul
                  class="p-ulchip"
                  v-if="slotProps.value && slotProps.value.length > 0"
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
                                : (value.last_name ?? '').substring(0, 1)
                            "
                            v-bind:image="
                              value.avatar
                                ? basedomainURL + value.avatar
                                : basedomainURL + '/Portals/Image/noimg.jpg'
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
                        <span
                          tabindex="0"
                          class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                          @click="removeMember(value, props.model.thamgias)"
                        ></span>
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
                    <div class="mb-1">{{ slotProps.option.full_name }}</div>
                    <div class="description">
                      <div>{{ slotProps.option.position_name }}</div>
                      <div>{{ slotProps.option.department_name }}</div>
                    </div>
                  </div>
                </div>
                <span v-else> Chưa có dữ liệu tuần </span>
              </template>
            </MultiSelect>
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Số người tham gia</label>
            <InputNumber
              inputId="minmax"
              v-model="props.model.numeric_attendees"
              mode="decimal"
              :min="0"
              class="ip36"
            />
          </div>
        </div>
        <div v-if="props.group === 0" class="col-6 md:col-6">
          <div class="form-group">
            <label>Hình thức họp</label>
            <div class="flex mt-2 justify-content-center">
              <div
                v-for="type of types"
                :key="type.key"
                class="field-radiobutton mr-5"
              >
                <RadioButton
                  :inputId="type.is_type"
                  :value="type.is_type"
                  v-model="props.model.is_type"
                  name="is_type"
                />
                <label :for="type.is_type">{{ type.name }}</label>
              </div>
            </div>
          </div>
        </div>
        <div class="col-6 md:col-6 format-center">
          <div class="form-group">
            <div
              class="field-checkbox flex justify-content-center"
              style="height: 100%"
            >
              <InputSwitch v-model="props.model.is_important" />
              <label for="binary">Lịch quan trọng</label>
            </div>
          </div>
        </div>
        <!-- <div class="col-6 md:col-6">
          <div class="form-group">
            <div
              class="field-checkbox flex justify-content-center"
              style="height: 100%"
            >
              <InputSwitch v-model="props.model.is_private" />
              <label for="binary">Lịch cá nhân</label>
            </div>
          </div>
        </div> -->
        <div v-if="props.group === 1" class="col-12 md:col-12">
          <div class="form-group">
            <label>Sử dụng xe</label>
            <Dropdown
              :options="props.cars"
              :filter="true"
              :showClear="true"
              :editable="false"
              v-model="props.model.car_id"
              optionLabel="car_name"
              optionValue="car_id"
              placeholder="Chọn xe"
              class="ip36"
            >
              <template #option="slotProps">
                <div
                  v-if="slotProps.option"
                  class="country-item flex align-items-center"
                >
                  <div class="pt-1 pl-2">
                    <div>
                      <b>{{ slotProps.option.car_code }}</b
                      ><span> / {{ slotProps.option.car_name }}</span>
                    </div>
                  </div>
                </div>
              </template>
            </Dropdown>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label
              >{{
                props.group === 0
                  ? "Địa điểm họp"
                  : props.group === 1
                  ? "Địa điểm công tác"
                  : ""
              }}
              <span v-if="props.group === 0" class="redsao">(*)</span></label
            >
            <div v-if="props.group === 0">
              <Dropdown
                :options="props.boardrooms"
                :filter="true"
                :showClear="true"
                :editable="true"
                :class="{
                  'p-invalid': !props.model.boardroom_id && submitted,
                }"
                v-model="props.model.boardroom_id"
                optionLabel="boardroom_name"
                optionValue="boardroom_id"
                :placeholder="
                  props.group === 0
                    ? 'Địa điểm họp'
                    : props.group === 1
                    ? 'Địa điểm công tác'
                    : ''
                "
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
            <div v-if="props.group === 1">
              <InputText
                v-model="props.model.boardroom_id"
                spellcheck="false"
                :placeholder="
                  props.group === 0
                    ? 'Địa điểm họp'
                    : props.group === 1
                    ? 'Địa điểm công tác'
                    : ''
                "
                class="ip36"
              />
            </div>
            <div
              v-if="props.group === 0 && !props.model.boardroom_id && submitted"
            >
              <small class="p-error">
                <span class="col-12 p-0"
                  >{{
                    props.group === 0
                      ? "Địa điểm họp"
                      : props.group === 1
                      ? "Địa điểm công tác"
                      : ""
                  }}
                  không được để trống</span
                >
              </small>
            </div>
          </div>
        </div>
        <div class="col-12 md-col-12">
          <div class="form-group">
            <label>Tệp đính kèm</label>
            <FileUpload
              :multiple="true"
              :show-upload-button="false"
              :show-cancel-button="true"
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
                          @click="
                            deleteFile(slotProps.data, slotProps.data.index)
                          "
                        />
                      </template>
                    </Toolbar>
                  </div>
                </template>
              </DataView>
            </div>
          </div>
        </div>
        <div v-if="props.group === 0" class="col-6 md-col-6">
          <div class="form-group">
            <label>Kiểu lặp</label>
            <Dropdown
              :options="iterations"
              :filter="false"
              :showClear="true"
              v-model="props.model.is_iterations"
              optionLabel="name"
              optionValue="is_iterations"
              placeholder="Chọn kiểu lặp"
              class="ip36"
            >
              <template #option="slotProps">
                <div class="country-item flex align-items-center">
                  <div class="pt-1 pl-2">{{ slotProps.option.name }}</div>
                </div>
              </template>
            </Dropdown>
          </div>
        </div>
        <div
          v-if="props.group === 0 && props.model.is_iterations !== 0"
          class="col-3 md-col-3"
        >
          <div class="form-group">
            <label
              >Số
              {{ iterations[props.model.is_iterations]?.short || "ngày" }}
              lặp</label
            >
            <InputNumber
              inputId="minmax"
              v-model="props.model.distance_iterations"
              mode="decimal"
              :min="0"
              class="ip36"
            />
          </div>
        </div>
        <div
          v-if="props.group === 0 && props.model.is_iterations !== 0"
          class="col-3 md-col-3"
        >
          <div class="form-group">
            <label>Số lần lặp</label>
            <InputNumber
              inputId="minmax"
              v-model="props.model.numeric_iterations"
              mode="decimal"
              :min="0"
              class="ip36"
            />
          </div>
        </div>
        <div v-if="props.group === 0" class="col-12 md:col-12">
          <div class="form-group">
            <label>Người được mời</label>
            <Textarea
              v-model="props.model.invitee"
              :autoResize="true"
              rows="3"
              cols="30"
            />
          </div>
        </div>
        <div v-if="props.group === 0" class="col-12 md:col-12">
          <div class="form-group">
            <label>Phòng ban tham gia</label>
            <MultiSelect
              :options="props.departments"
              v-model="props.model.departments"
              optionLabel="department_name"
              optionValue="department_id"
              placeholder="Chọn phòng ban"
              class="w-full ip36"
            >
              <template #value="slotProps">
                <div
                  class="p-dropdown-car-value flex text-justify"
                  v-if="slotProps.value"
                >
                  <div class="pr-2 text-justify">
                    <i class="pi pi-sliders-h"></i>
                  </div>
                  <div class="text-justify flex format-center">
                    <span v-for="(item, index) in slotProps.value" :key="index">
                      <span v-if="index > 0 && index < slotProps.value.length"
                        >,
                      </span>
                      {{
                        props.departments.find(
                          (x) => x["department_id"] === item
                        ).department_name
                      }}
                    </span>
                  </div>
                </div>
                <span v-else>
                  {{ slotProps.placeholder }}
                </span>
              </template>
            </MultiSelect>
          </div>
        </div>
        <div v-if="props.group === 0" class="col-12 md:col-12">
          <div class="form-group">
            <label>Chuẩn bị</label>
            <Textarea
              v-model="props.model.equip"
              :autoResize="true"
              rows="5"
              cols="30"
            />
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

  <!--treeuser-->
  <treeuser
    :key="componentKey"
    :headerDialog="headerDialogUser"
    :displayDialog="displayDialogUser"
    :closeDialog="closeDialogUser"
    :one="is_one"
    :selected="selectedUser"
    :choiceUser="choiceUser"
  />
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
