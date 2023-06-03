<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../util/function.js";
const toast = useToast();
const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");

const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
  "#FF88D3",
]);
const basedomainURL = fileURL;

const receiver = ref();
const options = ref({});
const listDropdownUser = ref();
const setting = ref();
const loadData = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "Task_Person_Config_get",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "is_BHBQP", va: 0 },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((x) => {
        x.receiver_info_display = JSON.parse(x.receiver_info);
      });
      setting.value = data;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const listUser = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_users_list_task_origin",
            par: [
              { par: "search", va: options.value.SearchTextUser },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "role_id", va: null },
              {
                par: "organization_id",
                va: store.getters.user.organization_id,
              },
              { par: "department_id", va: null },
              { par: "position_id", va: null },
              { par: "isadmin", va: null },
              { par: "status", va: null },
              { par: "start_date", va: null },
              { par: "end_date", va: null },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      listDropdownUser.value = data.map((x) => ({
        name: x.full_name,
        code: x.user_id,
        avatar: x.avatar,
        position_name: x.position_name,
        department_name: x.department_name,
      }));
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const Update = () => {
  let data = setting.value[0];
  data.receiver = receiver.value.code;
  axios({
    method: "put",
    url: baseURL + `/api/Task_Person_Config/${"Update"}`,
    data: data,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        toast.success("Cập nhật người duyệt thành công!");
        swal.close();
        loadData();
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
};
onMounted(() => {
  loadData();
  listUser();
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-4">
    <div class="bg-white p-4">
      <div class="format-center text-3xl font-bold text-blue-500">
        Cấu hình trình duyệt báo cáo công việc cá nhân
      </div>
      <div class="col-12 flex">
        <div class="col-3 text-xl format-center">Người nhận báo cáo:</div>
        <div class="col-9">
          <div
            class="col-12 card flex font-bold"
            v-for="(x, index) in setting"
            :key="index"
          >
            <div
              class="col-2 format-center"
              v-if="
                x.receiver_info_display != null && x.receiver_info_display != ''
              "
            >
              <Avatar
                @error="
                  $event.target.src =
                    basedomainURL + '/Portals/Image/nouser1.png'
                "
                v-bind:label="
                  x.receiver_info_display.avatar
                    ? ''
                    : x.receiver_info_display.full_name
                        .split(' ')
                        .at(-1)
                        .substring(0, 1)
                "
                v-bind:image="basedomainURL + x.receiver_info_display.avatar"
                style="color: #ffffff; cursor: pointer"
                :style="{
                  background: 'pink',
                  border: '2px solid #fffa8d',
                }"
                class="flex p-0 m-0"
                size="xlarge"
                shape="circle"
              />
            </div>
            <div class="col-10 format-left2">
              <div class="col-12 text-xl">
                {{ x.receiver_info_display.full_name }}
              </div>
              <div class="col-12 text-xl">
                {{ x.receiver_info_display.position_name }}
              </div>
              <div class="col-12 text-xl">
                {{
                  x.receiver_info_display.department_name ??
                  x.receiver_info_display.organization_name
                }}
              </div>
            </div>
          </div>
        </div>
      </div>
      <div class="format-center col-12">
        <div class="col-3 text-xl">Thay đổi người nhận:</div>
        <div class="col-9">
          <Dropdown
            :filter="true"
            v-model="receiver"
            :options="listDropdownUser"
            optionLabel="name"
            class="col-12 ip36 p-0"
            placeholder="Người đánh giá"
            display="chip"
            style="height: 3.125rem"
          >
            <template #value="slotProps">
              <div class="flex" v-if="slotProps.value">
                <div class="flex" style="margin-left: 10px">
                  <Avatar
                    @error="
                      $event.target.src =
                        basedomainURL + '/Portals/Image/nouser1.png'
                    "
                    v-bind:label="
                      slotProps.value.avatar
                        ? ''
                        : (slotProps.value.name ?? '').substring(0, 1)
                    "
                    v-bind:image="basedomainURL + slotProps.value.avatar"
                    style="
                      background-color: #2196f3;
                      color: #ffffff;
                      width: 32px;
                      height: 32px;
                      font-size: 15px !important;
                      margin-left: -10px;
                    "
                    :style="{
                      background: bgColor[10000 % 7] + '!important',
                    }"
                    class="cursor-pointer"
                    size="xlarge"
                    shape="circle"
                  />
                  <div class="pt-1" style="padding-left: 10px">
                    {{ slotProps.value.name }}
                  </div>
                </div>
              </div>
              <span v-else>
                {{ slotProps.placeholder }}
              </span>
            </template>
            <template #option="slotProps">
              <div
                class="country-item flex"
                style="align-items: center; margin-left: 10px"
              >
                <Avatar
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/nouser1.png'
                  "
                  v-bind:label="
                    slotProps.option.avatar
                      ? ''
                      : (slotProps.option.name ?? '').substring(0, 1)
                  "
                  v-bind:image="basedomainURL + slotProps.option.avatar"
                  style="
                    background-color: #2196f3;
                    color: #ffffff;
                    width: 32px;
                    height: 32px;
                    font-size: 15px !important;
                    margin-left: -10px;
                  "
                  :style="{
                    background: bgColor[slotProps.index % 7] + '!important',
                  }"
                  class="cursor-pointer"
                  size="xlarge"
                  shape="circle"
                />
                <div>
                  <div class="pt-1" style="padding-left: 10px">
                    {{ slotProps.option.name }}
                  </div>
                  <div class="pt-1" style="padding-left: 10px">
                    {{ slotProps.option.position_name }}
                  </div>
                  <div class="pt-1" style="padding-left: 10px">
                    {{ slotProps.option.department_name }}
                  </div>
                </div>
              </div>
            </template>
          </Dropdown>
        </div>
      </div>
      <div class="format-center col-12" v-if="receiver != null">
        <Button
          label="Cập nhật"
          class="p-button-raised"
          @click="Update()"
        ></Button>
      </div>
    </div>
  </div>
</template>
<style scoped>
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
.format-left2 {
  justify-content: flex-start;
  align-items: center;
  vertical-align: middle;
  text-align: start;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-avatar) {
  .p-avatar-text {
    font-size: 2rem;
  }
}
</style>
