<script setup>
import { onMounted, inject, ref } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../../util/function";
import moment from "moment";
import dialogassignment from "../../profile/component/dialogassignment.vue";

const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const cryoptojs = inject("cryptojs");
const toast = useToast();
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const basedomainURL = baseURL;

//Get arguments
const props = defineProps({
  profile_id: String,
  view: Number,
  functions: Object,
});

//Declare
const options = ref({});
const dictionarys = ref([]);
const task = ref({});
const tasks = ref([]);

//Dictionary
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const typestatus = ref([
  { value: 0, title: "Đang làm việc", bg_color: "#5fc57b", text_color: "#fff" },
  { value: 1, title: "Đã làm việc", bg_color: "red", text_color: "#fff" },
]);

//Function
const assignment = ref({});
const isAdd = ref(false);
const componentKey = ref({});
const forceRerender = (type) => {
  if (!componentKey.value[type]) {
    componentKey.value[type] = 0;
  }
  componentKey.value[type] += 1;
};
const headerDialogAssignment = ref();
const displayDialogAssignment = ref(false);
const openAddDialogAssignment = (str) => {
  isAdd.value = true;
  headerDialogAssignment.value = str;
  displayDialogAssignment.value = true;
  forceRerender(0);
};
const closeDialogAssignment = () => {
  displayDialogAssignment.value = false;
  forceRerender(0);
};
const openEditAssignment = (item, str) => {
  assignment.value = item;
  isAdd.value = false;
  headerDialogAssignment.value = str;
  displayDialogAssignment.value = true;
  forceRerender(0);
};
const deleteAssignment = (item) => {
  if (item != null) {
    swal
      .fire({
        title: "Thông báo",
        text: "Bạn có muốn xoá công việc phân công này không!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Có",
        cancelButtonText: "Không",
      })
      .then((result) => {
        if (result.isConfirmed) {
          options.value.loading = true;
          swal.fire({
            width: 110,
            didOpen: () => {
              swal.showLoading();
            },
          });
          var ids = [];
          if (item != null) {
            ids = [item["assignment_id"]];
          } else {
          }
          axios
            .delete(baseURL + "/api/hrm_assignment/delete_assignment", {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: ids,
            })
            .then((response) => {
              if (response.data.err === "1" || response.data.err === "2") {
                swal.close();
                if (options.value.loading) options.value.loading = false;
                swal.fire({
                  title: "Thông báo!",
                  text: response.data.ms,
                  icon: "error",
                  confirmButtonText: "OK",
                });
                return;
              }
              toast.success("Xoá thành công!");
              initView2(true);
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
        }
      });
  }
};

const menuButMores = ref();
const itemButMores = ref([
  {
    label: "Hiệu chỉnh",
    icon: "pi pi-file",
    command: (event) => {
      openEditAssignment(assignment.value, "Cập nhật công việc phân công");
    },
  },
  {
    label: "Xoá",
    icon: "pi pi-trash",
    command: (event) => {
      deleteAssignment(assignment.value);
    },
  },
]);
const toggleMores = (event, item) => {
  assignment.value = item;
  menuButMores.value.toggle(event);
};

//init
const initDictionary2 = () => {
  dictionarys.value = [];
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_contract_dictionary",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
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
          dictionarys.value = tbs;
        }
      }
    })
    .then(() => {
      initView2(true);
    });
};
const initView2 = (rf) => {
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
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_profile_assignment_gets",
            par: [{ par: "profile_id", va: props.profile_id }],
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
            if (item.managers != null) {
              item.managers = JSON.parse(item.managers);
            }

            if (item.is_active) {
              item["status_name"] = typestatus.value[0]["title"];
              item["bg_color"] = typestatus.value[0]["bg_color"];
              item["text_color"] = typestatus.value[0]["text_color"];
            } else {
              item["status_name"] = typestatus.value[1]["title"];
              item["bg_color"] = typestatus.value[1]["bg_color"];
              item["text_color"] = typestatus.value[1]["text_color"];
            }
            if (item["start_date"] != null) {
              item["start_date_string"] = moment(
                new Date(item["start_date"])
              ).format("DD/MM/YYYY");
            }
            if (item["end_date"] != null) {
              item["end_date_string"] = moment(
                new Date(item["end_date"])
              ).format("DD/MM/YYYY");
            }
            if (item["sign_date"] != null) {
              item["sign_date_string"] = moment(
                new Date(item["sign_date"])
              ).format("DD/MM/YYYY");
            }
          });
          tasks.value = tbs[0];
        } else {
          tasks.value = [];
        }
      }
      swal.close();
    })
    .catch((error) => {
      swal.close();
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
  if (props.view === 2) {
    initView2(true);
  }
});
</script>
<template>
  <Toolbar class="outline-none surface-0 border-none">
    <template #start> </template>
    <template #end>
      <Button
        v-if="props.functions.is_add"
        @click="openAddDialogAssignment('Thêm mới công việc')"
        label="Thêm mới"
        icon="pi pi-plus"
        class="mr-2"
      />
    </template>
  </Toolbar>
  <div
    class="d-lang-table my-3"
    :style="{
      height: 'calc(100vh - 237px) !important',
      overflowY: 'auto',
    }"
  >
    <div v-if="tasks && tasks.length > 0" class="w-full">
      <Timeline :value="tasks" align="alternate" class="customized-timeline">
        <template #marker="slotProps">
          <span
            class="flex w-2rem h-2rem align-items-center justify-content-center text-white border-circle z-1 shadow-1"
            :style="{
              backgroundColor: slotProps.item.bg_color,
            }"
          >
            <i class="pi pi-briefcase"></i>
          </span>
        </template>
        <template #content="slotProps">
          <Card>
            <template #title>
              <Toolbar class="outline-none surface-0 border-none p-0">
                <template #start>
                  <div class="w-full text-left">
                    <Button
                      :label="slotProps.item.status_name"
                      class="p-button-outlined"
                      :style="{
                        borderColor: slotProps.item.bg_color,
                        // backgroundColor: slotProps.data.bg_color,
                        color: slotProps.item.bg_color,
                        borderRadius: '15px',
                        padding: '0.3rem 0.75rem !important',
                      }"
                    />
                  </div>
                </template>
                <template #end>
                  <Button
                    v-if="props.functions.is_edit"
                    icon="pi pi-ellipsis-h"
                    class="p-button-rounded p-button-text"
                    @click="
                      toggleMores($event, slotProps.item);
                      $event.stopPropagation();
                    "
                    aria-haspopup="true"
                    aria-controls="overlay_More"
                    v-tooltip.top="'Tác vụ'"
                  />
                </template>
              </Toolbar>
            </template>
            <template #subtitle>
              <div class="w-full text-left">
                <div>
                  Từ: <b>{{ slotProps.item.start_date_string }}</b>
                  <span v-if="slotProps.item.end_date_string">
                    Đến: <b>{{ slotProps.item.end_date_string }}</b>
                  </span>
                </div>
              </div>
            </template>
            <template #content>
              <div class="w-full text-left">
                <div class="mb-2">
                  Chức vụ: <b>{{ slotProps.item.position_name }}</b>
                </div>
                <div class="mb-2">
                  Chức danh: <b>{{ slotProps.item.title_name }}</b>
                </div>
                <div class="mb-2">
                  Phòng ban: <b>{{ slotProps.item.department_name }}</b>
                </div>
                <div class="mb-2">
                  Công việc chuyên môn:
                  <b>{{ slotProps.item.professional_work_name }}</b>
                </div>
                <div class="mb-2">
                  Mô tả công việc: <b>{{ slotProps.item.description }}</b>
                </div>
                <div class="mb-2">
                  Loại nhân sự: <b>{{ slotProps.item.personel_groups_name }}</b>
                </div>
                <div
                  v-if="
                    slotProps.item.managers &&
                    slotProps.item.managers.length > 0
                  "
                  class="flex format-center justify-content-left"
                  :style="{ justifyContent: 'left' }"
                >
                  <span class="mr-2">Người quản lý: </span>
                  <AvatarGroup>
                    <Avatar
                      v-for="(item, index) in slotProps.item.managers.slice(
                        0,
                        3
                      )"
                      v-bind:label="
                        item.avatar
                          ? ''
                          : item.profile_user_name.substring(0, 1)
                      "
                      v-bind:image="
                        item.avatar
                          ? basedomainURL + item.avatar
                          : basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                      v-tooltip.top="item.profile_user_name"
                      :key="item.user_id"
                      style="border: 2px solid orange; color: white"
                      @click="onTaskUserFilter(item)"
                      @error="basedomainURL + '/Portals/Image/noimg.jpg'"
                      size="large"
                      shape="circle"
                      class="cursor-pointer"
                      :style="{
                        backgroundColor: bgColor[index % 7],
                        width: '2.5rem',
                        height: '2.5rem',
                      }"
                    />
                    <Avatar
                      v-if="
                        slotProps.item.managers &&
                        slotProps.item.managers.length > 3
                      "
                      v-bind:label="
                        '+' + (slotProps.item.managers - 3).toString()
                      "
                      shape="circle"
                      size="large"
                      :style="{
                        backgroundColor: '#2196f3',
                        color: '#ffffff',
                        width: '2.5rem',
                        height: '2.5rem',
                      }"
                      class="cursor-pointer"
                    />
                  </AvatarGroup>
                </div>
              </div>
            </template>
          </Card>
        </template>
      </Timeline>
    </div>
    <div v-else class="w-full h-full format-center">
      <div class="description">Hiện chưa có dữ liệu</div>
    </div>
  </div>

  <!--Dialog-->
  <dialogassignment
    :key="componentKey['0']"
    :headerDialog="headerDialogAssignment"
    :displayDialog="displayDialogAssignment"
    :closeDialog="closeDialogAssignment"
    :isAdd="isAdd"
    :profile_id="props.profile_id"
    :assignment="assignment"
    :initData="initView2"
  />

  <!--Menu-->
  <Menu
    id="overlay_More"
    ref="menuButMores"
    :model="itemButMores"
    :popup="true"
  />
</template>
<style scoped></style>
