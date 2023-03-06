<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { required } from "@vuelidate/validators";
import { VuemojiPicker } from "vuemoji-picker";
import { encr } from "../../../util/function.js";
import moment from "moment";
const cryoptojs = inject("cryptojs");

const axios = inject("axios"); // inject axios
const store = inject("store");
const swal = inject("$swal");
const toast = useToast();
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const today = ref({});
today.value = new Date();
const basedomainURL = fileURL;

const height1 = ref(window.screen.height);
//Khai báo
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const props = defineProps({
  id: String,
  data: Object,
  member: Array,
  isClose: Boolean,
});

const panelEmoij1 = ref();

const showEmoji = (event, check) => {
  if (check == 1) panelEmoij1.value.toggle(event);
};

const handleEmojiClick = (event) => {
  if (openDialogTaskExtend.value == true) {
    TaskExtend.value.answer =
      (TaskExtend.value.answer != null ? TaskExtend.value.answer : "") +
      event.unicode;
  } else {
    TaskExtend.value.reasons =
      (TaskExtend.value.reasons != null ? TaskExtend.value.reasons : "") +
      event.unicode;
  }
};

// Gia hạn xử lý
const TaskExtend = ref({
  task_id: null,
  current_end_date: null,
  extend_old_date: null,
  extend_new_date: null,
  extend_new_dates: null,
  reason: null,
  reasons: null,
  is_agree: null,
  answer: null,
});
const rulesTaskExtend = {
  reasons: {
    required,
  },
  extend_new_dates: { required },
};
const submittedTaskExtend = ref(false);
const vTaskExtend = useVuelidate(rulesTaskExtend, TaskExtend);
const ListTaskExtend = ref();
const isData = ref(false);
const LoadTaskExtend = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_extend_list",
            par: [{ par: "id", va: props.id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data0 = JSON.parse(response.data.data)[0];
      ListTaskExtend.value = [];
      data0.forEach((x, i) => {
        x.creator_tooltip =
          "Người tạo gia hạn <br/>" +
          x.creator_full_name +
          "<br/>" +
          x.creator_positions +
          "<br/>" +
          (x.creator_department_name != null
            ? x.creator_department_name
            : x.creator_organiztion_name);
        x.acept_user_info = JSON.parse(x.acept_user_info);
        x.acept_user_info_tooltip =
          x.acept_user_info != null
            ? "Người duyệt gia hạn <br/>" +
              x.acept_user_info.full_name +
              "<br/>" +
              x.acept_user_info.positions +
              "<br/>" +
              (x.acept_user_info.department_name != null
                ? x.acept_user_info.department_name
                : x.acept_user_info.organiztion_name)
            : "";
      });
      ListTaskExtend.value = data0;
      if (data0.length > 0) {
        isData.value = true;
      }
    })
    .catch(() => {
      swal.close();
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const sendExtension = (isFormValid) => {
  submittedTaskExtend.value = true;
  if (!isFormValid) {
    return;
  }
  TaskExtend.value.extend_new_date = TaskExtend.value.extend_new_dates;
  if (
    memberType.value == 0 ||
    memberType1.value == 0 ||
    memberType2.value == 0 ||
    memberType3.value == 0
  ) {
    TaskExtend.value.is_agree = true;
    TaskExtend.value.acept_user = store.state.user.user_id;
    TaskExtend.value.accept_date = TaskExtend.value.extend_new_date;
  }
  TaskExtend.value.reason = TaskExtend.value.reasons;
  TaskExtend.value.reason = TaskExtend.value.reason.replace(/\n/g, "<br/>");
  TaskExtend.value.task_id = props.data.task_id;
  TaskExtend.value.current_end_date = props.data.end_date;
  TaskExtend.value.extend_old_date = props.data.end_date;
  axios({
    method: "post",
    url: baseURL + `/api/TaskExtend/${"Add_TaskExtend"}`,
    data: TaskExtend.value,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Thêm mới gia hạn xử lý công việc thành công!");
        TaskExtend.value = {
          extend_old_date: null,
          extend_new_date: null,
          extend_new_dates: null,
          reason: null,
          reasons: null,
          is_agree: null,
          answer: null,
        };
        TaskExtend.value.task_id = props.data.task_id;
        TaskExtend.value.current_end_date = props.data.current_end_date;
        submittedTaskExtend.value = false;
        LoadTaskExtend();
      } else {
        swal.fire({
          title: "Error!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch(() => {
      swal.close();
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const openDialogTaskExtend = ref(false);
const headerTaskExtend = ref();
const openTaskExtend = (stt, rp) => {
  if (stt == 0) {
    TaskExtend.value = rp;
    TaskExtend.value.is_agree = true;
    headerTaskExtend.value = "Duyệt gia hạn xử lý công việc";
  } else {
    TaskExtend.value = rp;
    TaskExtend.value.is_agree = false;
    headerTaskExtend.value = "Không duyệt gia hạn xử lý công việc";
  }
  openDialogTaskExtend.value = true;
};
const sendExtendReport = (e) => {
  if (e == "0") {
    openDialogTaskExtend.value = false;
    LoadTaskExtend();
  } else {
    TaskExtend.value.answer = TaskExtend.value.answer
      ? TaskExtend.value.answer.replace(/\n/g, "<br/>")
      : TaskExtend.value.answer;

    axios
      .put(
        baseURL + "/api/TaskExtend/Upgrade_Status_TaskExtend",
        TaskExtend.value,
        {
          headers: { Authorization: `Bearer ${store.getters.token}` },
        },
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Duyệt gia hạn công việc thành công!");
          openDialogTaskExtend.value = false;
          TaskExtend.value = {
            extend_old_date: null,
            extend_new_date: null,
            extend_new_dates: null,
            reason: "",
            reasons: "",
            is_agree: null,
            answer: "",
          };
          LoadTaskExtend();
        } else {
          let ms = response.data.ms;
          swal.fire({
            title: "Thông báo!",
            html: ms,
            icon: "error",
            confirmButtonText: "OK",
          });
        }
      })
      .catch((error) => {
        swal.close();
        swal.fire({
          title: "Thông báo",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!" + error,
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  }
};
const memberType = ref();
const memberType1 = ref();
const memberType2 = ref();
const memberType3 = ref();
const members = ref();
const loadMember = () => {
  members.value.forEach((element) => {
    if (store.state.user.user_id == element.user_id) {
      if (memberType.value == null) {
        memberType.value = element.is_type;
        return;
      }
      if (memberType1.value == null) {
        memberType1.value = element.is_type;
        return;
      }
      if (memberType2.value == null) {
        memberType2.value = element.is_type;
        return;
      }
      if (memberType3.value == null) {
        memberType3.value = element.is_type;
        return;
      }
    }
  });
  var byDate = members.value.slice(0);
  byDate.sort(function (a, b) {
    return a.is_type - b.is_type;
  });
  members.value = byDate;
};
const deleteTaskExtend = (ext_id) => {
  let id = [];
  id.push(ext_id);
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá gia hạn này không!",
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
        axios
          .delete(baseURL + "/api/TaskExtend/deleteTaskExtendTaskExtend", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: id,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá gia hạn thành công!");
              LoadTaskExtend();
            } else {
              swal.fire({
                title: "Error!",
                text: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          })
          .catch((error) => {
            swal.close();
            if (error.status === 401) {
              swal.fire({
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
onMounted(() => {
  members.value = props.member;
  LoadTaskExtend();
  loadMember();
  return {};
});
</script>
<template>
  <div class="h-custom">
    <div class="relative card-container blue-container w-full h-full">
      <div class="relative p-4 border-round w-full h-full">
        <ScrollPanel
          style="height: calc(100vh - 13rem)"
          class="relative"
        >
          <div class="col-12 p-0 m-0 font-bold text-xl">
            <i class="pi pi-check-square pr-2"></i>
            <span>
              {{ props.data.task_name }}
            </span>
          </div>
          <div v-if="isData == true">
            <div
              class="col-12 border-bottom-1 border-bluegray-100"
              v-for="(rp, index) in ListTaskExtend"
              :key="index"
            >
              <div
                class="col-12 p-0 m-0"
                id="extend"
              >
                <div class="col-12 p-0 format-left flex">
                  <div class="flex p-0 col-1 format-center">
                    <Avatar
                      v-tooltip="{
                        value: rp.creator_tooltip,
                        escape: true,
                      }"
                      v-bind:label="
                        rp.creator_avt
                          ? ''
                          : rp.creator_full_name
                              .split(' ')
                              .at(-1)
                              .substring(0, 1)
                      "
                      v-bind:image="basedomainURL + rp.creator_avt"
                      style="color: #ffffff; cursor: pointer"
                      :style="{
                        background: bgColor[index % 7],
                        border: '1px solid' + bgColor[index % 10],
                      }"
                      class="myTextAvatar p-0 m-0"
                      size="large"
                      shape="circle"
                    />
                  </div>
                  <div
                    class="flex col-10 format-left"
                    style="font-weight: 700; font-size: 16px; color: #385898"
                  >
                    <div class="col-12 p-0 m-0">
                      <div class="flex col-12 p-0 m-0 pl-2 format-left">
                        {{ rp.creator_full_name }}
                        <div
                          class="col-4 p-0 m-0 pl-2 format-left m-2"
                          :style="{}"
                        >
                          <Chip
                            :style="{
                              color: '#FFFFFF',
                              background:
                                rp.is_agree == null
                                  ? '#3355F3'
                                  : rp.is_agree == true
                                  ? '#59D05D'
                                  : '#FF0000',
                            }"
                            class="font-bold py-1 px-4"
                          >
                            <span v-if="rp.is_agree == null"
                              >Đang chờ duyệt</span
                            >
                            <span v-if="rp.is_agree == true">Đã duyệt</span>
                            <span v-if="rp.is_agree == false">Không duyệt</span>
                          </Chip>
                        </div>
                      </div>
                      <div
                        class="flex col-12 text-sm p-0 m-0 pl-2 text-dark font-light mt-1"
                      >
                        {{
                          moment(new Date(rp.created_date)).format(
                            "HH:mm DD/MM/YYYY",
                          )
                        }}
                      </div>
                    </div>
                  </div>
                  <div
                    class="col-1"
                    v-if="
                      rp.is_agree == null &&
                      rp.created_by === store.state.user.user_id
                    "
                  >
                    <Button
                      icon="pi pi-trash"
                      class="p-button-text"
                      @click="deleteTaskExtend(rp.extend_id)"
                    ></Button>
                  </div>
                </div>
                <div class="col-12 p-0">
                  <Chip
                    :label="
                      moment(new Date(rp.extend_old_date)).format('DD/MM/YYYY')
                    "
                    icon="pi pi-calendar"
                    class="mr-2 mb-2 old-date"
                    style="margin-left: 4rem"
                    v-if="rp.is_agree == true"
                    v-tooltip="{ value: 'Thời gian xử lý cũ' }"
                  />
                  <Chip
                    :label="
                      moment(new Date(rp.extend_new_date)).format('DD/MM/YYYY')
                    "
                    icon="pi pi-calendar"
                    class="mr-2 mb-2"
                    :class="rp.is_agree == true ? 'acept-new-date' : 'new-date'"
                    :style="
                      rp.is_agree != 1
                        ? 'margin-left: 4rem'
                        : 'margin-left: .5rem'
                    "
                    v-tooltip="{ value: 'Thời gian xử lý đề xuất' }"
                  />
                </div>
                <div class="col-12 contents">
                  <span v-html="rp.reason"></span>
                </div>
                <div
                  class="col-12"
                  style="padding-left: 4rem !important"
                  v-if="
                    props.isClose == false &&
                    rp.is_agree == null &&
                    (memberType == 0 ||
                      memberType1 == 0 ||
                      memberType2 == 0 ||
                      memberType3 == 0)
                  "
                >
                  <Button
                    class="p-button-text p-button-success p-0 m-0 mr-4"
                    @click="openTaskExtend(0, rp)"
                    >Đồng ý
                  </Button>
                  <Button
                    class="p-button-text p-button-danger p-0 m-0"
                    @click="openTaskExtend(1, rp)"
                    >Không đồng ý</Button
                  >
                </div>
                <div
                  class="col-12 flex"
                  v-if="rp.acept_user_info != null"
                >
                  <div class="col-1"></div>
                  <div class="col-11 contents3 mt-2">
                    <div class="flex col-12 p-0 m-0">
                      <div class="flex m-1 p-0 col-1 format-center">
                        <Avatar
                          v-tooltip="{
                            value: rp.acept_user_info_tooltip,
                            escape: true,
                          }"
                          v-bind:label="
                            rp.acept_user_info.avt
                              ? ''
                              : rp.acept_user_info.full_name
                                  .split(' ')
                                  .at(-1)
                                  .substring(0, 1)
                          "
                          v-bind:image="basedomainURL + rp.acept_user_info.avt"
                          style="color: #ffffff; cursor: pointer"
                          :style="{
                            background: bgColor[index % 7],
                            border: '1px solid' + bgColor[index % 10],
                          }"
                          class="myTextAvatar p-0 m-0"
                          size="large"
                          shape="circle"
                        />
                      </div>
                      <div
                        class="flex col-11 p-0 m-0 format-left pl-1"
                        style="
                          font-weight: 700;
                          font-size: 16px;
                          color: #385898;
                        "
                      >
                        <div class="col-12 p-0 m-0">
                          <div class="flex col-12 p-0 m-0 pl-2 format-left">
                            {{ rp.acept_user_info.full_name }}
                          </div>
                          <div
                            class="flex col-12 text-sm p-0 m-0 pl-2 text-dark font-light mt-1"
                          >
                            {{
                              moment(new Date(rp.accept_date)).format(
                                "HH:mm DD/MM/YYYY",
                              )
                            }}
                          </div>
                        </div>
                      </div>
                    </div>
                    <div
                      class="col-12 contents3 flex"
                      v-if="rp.answer != null && rp.answer != ''"
                    >
                      <div class="col-1"></div>
                      <div
                        class="col-11 border-1 border-blue-100 border-round-xl mt-1 bg-blue-100"
                        v-html="rp.answer"
                      ></div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div
            v-else
            class="col-12 align-items-center justify-content-center p-4 text-center m-auto"
            style="height: calc(100vh - 40rem)"
          >
            <img
              src="../../../assets/background/nodata.png"
              height="300"
            />
            <h3 class="m-1">Chưa có gia hạn</h3>
          </div>
        </ScrollPanel>

        <div
          v-if="props.isClose == false"
          class="absolute bottom-0 left-0 border-round w-full font-bold text-white"
        >
          <div
            class="col-12 border-1 border-round-xs border-600 flex"
            style="border-radius: 5px"
          >
            <div class="col-12 flex p-0 m-0">
              <div class="col-11 p-0 m-0">
                <div class="col-12 p-0 m-0 flex">
                  <div class="border-0 col-8 p-0 m-0 pr-2">
                    <small
                      v-if="
                        (vTaskExtend.reasons.$invalid && submittedTaskExtend) ||
                        vTaskExtend.reasons.$pending.$response
                      "
                      class="col-12 p-error"
                    >
                      <span class="col-12 p-0">{{
                        vTaskExtend.reasons.required.$message
                          .replace("Value", "Lý do gia hạn")
                          .replace("is required", "không được để trống")
                      }}</span>
                    </small>
                  </div>
                  <div class="border-0 col-4 p-0 m-0 pr-2">
                    <small
                      v-if="
                        (vTaskExtend.extend_new_dates.$invalid &&
                          submittedTaskExtend) ||
                        vTaskExtend.extend_new_dates.$pending.$response
                      "
                      class="col-12 p-error"
                    >
                      <span class="col-12 p-0">{{
                        vTaskExtend.extend_new_dates.required.$message
                          .replace("Value", "Ngày gia hạn")
                          .replace("is required", "không được để trống")
                      }}</span>
                    </small>
                  </div>
                </div>
                <div class="col-12 p-0 m-0 flex">
                  <div
                    class="border-1 col-8 p-0 m-0 pr-2 flex border-gray-400 border-round"
                  >
                    <Textarea
                      class="border-0 w-full h-full"
                      placeholder="Nhập lý do gia hạn xử lý..."
                      v-model="TaskExtend.reasons"
                    >
                    </Textarea
                    ><Button
                      class="p-button-text p-button-plain col-6 format-center w-3rem h-4rem"
                      @click="showEmoji($event, 1)"
                      v-tooltip="{ value: 'Biểu cảm' }"
                    >
                      <img
                        alt="logo"
                        src="/src/assets/image/smile.png"
                        width="20"
                        height="20"
                      />
                    </Button>
                  </div>
                  <div class="border-0 col-4 p-0 m-0 pr-3">
                    <Calendar
                      inputId="icon"
                      v-model="TaskExtend.extend_new_dates"
                      :showIcon="true"
                      class="w-full h-full"
                      placeholder="Thời gian xử lý mới"
                      :minDate="new Date()"
                    />
                  </div>
                </div>
              </div>

              <div class="col-1 p-0 m-0 format-center">
                <Button
                  icon="pi pi-send pt-1 font-bold"
                  class="p-button-text p-button-plain col-6 w-4rem h-4rem"
                  style="background-color: ; color: black"
                  v-tooltip="{ value: 'Gửi đề xuất gia hạn' }"
                  @click="sendExtension(!vTaskExtend.$invalid)"
                />
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
  <OverlayPanel
    class="p-0"
    ref="panelEmoij1"
    append-to="body"
    :show-close-icon="false"
    id="overlay_panelEmoij1"
  >
    <VuemojiPicker @emojiClick="handleEmojiClick($event)" />
  </OverlayPanel>
  <Dialog
    :header="headerTaskExtend"
    v-model:visible="openDialogTaskExtend"
    :style="{ width: '35vw', 'z-index': '10000' }"
    :closable="false"
  >
    <form>
      <div
        class="col-12 flex pb-0 mb-0"
        v-if="TaskExtend.is_agree == true"
      >
        <label class="col-4 p-0 m-0 format-left">Hạn xử lý hiện tại</label>
        <div :class="height1 < 1000 ? 'col-4 p-0 m-0' : 'col-4'">
          <div class="col-12">
            <Chip
              :label="
                moment(new Date(TaskExtend.extend_old_date)).format(
                  'DD/MM/YYYY',
                )
              "
              icon="pi pi-calendar"
              class="old-date col-12 format-center px-1"
              v-if="TaskExtend.is_agree == true"
              v-tooltip="{ value: 'Thời gian xử lý cũ' }"
            />
          </div>
        </div>
      </div>
      <div
        class="col-12 flex pb-0 mb-0"
        v-if="TaskExtend.is_agree == true"
      >
        <label class="col-4 format-left p-0 m-0">Hạn xử lý đề xuất</label>
        <div :class="height1 < 1000 ? 'col-4 p-0 m-0' : 'col-3'">
          <div class="col-12">
            <Chip
              :label="
                moment(new Date(TaskExtend.extend_new_date)).format(
                  'DD/MM/YYYY',
                )
              "
              icon="pi pi-calendar"
              class="old-date col-12 format-center px-1"
              :class="'acept-new-date'"
              v-tooltip="{ value: 'Thời gian xử lý đề xuất' }"
            />
          </div>
        </div>
      </div>
      <div class="col-12 flex pb-0 mb-0">
        <div
          class="col-4 format-left"
          v-if="TaskExtend.is_agree == true"
        >
          Nội dung duyệt:
        </div>
        <div
          class="col-4 format-left"
          v-else
        >
          Nội dung không duyệt:
        </div>
        <div class="col-8 p-0">
          <div class="col-12 flex">
            <div
              class="col-12 border-1 p-0 m-0 flex border-gray-400 border-round"
            >
              <Textarea
                class="border-0 w-full h-full"
                placeholder="Nội dung duyệt..."
                v-model="TaskExtend.answer"
              >
              </Textarea
              ><Button
                class="p-button-text p-button-plain format-right w-4rem h-4rem"
                @click="showEmoji($event, 1)"
                v-tooltip="{ value: 'Biểu cảm', escape: true }"
              >
                <img
                  alt="logo"
                  src="/src/assets/image/smile.png"
                  width="20"
                  height="20"
                />
              </Button>
            </div>
          </div>
        </div>
      </div>
    </form>

    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="sendExtendReport('0')"
        class="p-button-text"
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="sendExtendReport('1')"
      />
    </template>
  </Dialog>
</template>
<style scoped>
.contents {
  text-align: justify;
  color: #333;
  font-size: 14px;
  width: fit-content;
  padding: 1rem 1rem;
  background-color: #f5f5f5;
  border-radius: 20px;
  margin-left: 4rem;
}
.myTextAvatar .p-avatar-text {
  font-size: 2rem !important;
}
.format-center {
  display: flex;
  justify-content: center;
  align-items: center;
  vertical-align: middle;
  text-align: center;
}

.format-right {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  vertical-align: middle;
  text-align: center;
}

.format-left {
  display: flex;
  justify-content: flex-start;
  align-items: center;
  vertical-align: middle;
  text-align: center;
}
.format-default {
  justify-content: unset;
  align-items: unset;
  vertical-align: unset;
  text-align: unset;
}
.old-date {
  background-color: #ff8b4e;
  color: #fff;
}

.new-date {
  background-color: #ff8b4e;
  color: #fff;
}

.acept-new-date {
  color: #fff;
  background-color: hwb(242 0% 0%);
}
.contents3 {
  text-align: justify;
  color: #333;
  font-size: 14px;

  background-color: #f5f5f5;
  border-radius: 20px;
}
.h-custom {
  height: calc(100vh - 5rem);
}
</style>
