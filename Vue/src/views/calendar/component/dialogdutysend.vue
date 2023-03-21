<script setup>
import { ref, inject, onMounted } from "vue";
import { encr } from "../../../util/function";
import { socketMethod } from "../../../util/methodSocket";
import moment from "moment";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { de } from "date-fns/locale";

const cryoptojs = inject("cryptojs");
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const emitter = inject("emitter");
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
const typeprocedures = ref([
  { value: 0, title: "Duyệt tuần tự" },
  { value: 1, title: "Duyệt một trong nhiều" },
  //{ value: 2, title: "Duyệt ngẫu nhiên" },
]);
const submitted = ref(false);

//Get arguments
const props = defineProps({
  headerDialog: String,
  displayDialog: Boolean,
  closeDialog: Function,
  modelsend: Object,
  files: Array,
  selectFile: Function,
  removeFile: Function,
  selectedNodes: Array,
  initData: Function,
});

//Function
const changeProcedure = (procedureform_id) => {
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_duty_get_signforms2",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "procedureform_id", va: procedureform_id },
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
            tbs[0].forEach((item, i) => {
              if (
                item["signuserforms"] != null &&
                item["signuserforms"] != ""
              ) {
                item["signuserforms"] = JSON.parse(item["signuserforms"]);
              }
              var idx = typeprocedures.value.findIndex(
                (x) => x["value"] === parseInt(item["is_type"])
              );
              if (idx != -1) {
                item["type_name"] = typeprocedures.value[idx]["title"];
              } else {
                item["type_name"] = "Chưa xác định";
              }
            });
          }
          props.modelsend.signforms = tbs[0];
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
const changeSign = (signform_id) => {
  if (signform_id != null) {
    axios
      .post(
        baseURL + "/api/calendar/get_datas",
        {
          str: encr(
            JSON.stringify({
              proc: "calendar_duty_signform_get2",
              par: [
                { par: "user_id", va: store.getters.user.user_id },
                { par: "signform_id", va: signform_id },
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
            props.modelsend.signform = tbs[0][0];
            var idx = typeprocedures.value.findIndex(
              (x) =>
                x["value"] === parseInt(props.modelsend.signform["is_type"])
            );
            if (idx != -1) {
              props.modelsend.signform["type_name"] =
                typeprocedures.value[idx]["title"];
            } else {
              props.modelsend.signform["type_name"] = "Chưa xác định";
            }
            props.modelsend.signform.signuserforms = tbs[1];
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
  } else {
    props.modelsend.signform = [];
  }
};
const changeUser = (user_id) => {
  props.modelsend.key_id = props.modelsend.user["user_id"];
};
const send = () => {
  submitted.value = true;
  if (!props.modelsend.key_id) {
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
  var obj = { ...props.modelsend };
  var calendars = props.selectedNodes.map((x) => x["calendar_duty_id"]);

  let formData = new FormData();
  formData.append("is_type_send", obj["is_type_send"]);
  formData.append("key_id", obj["key_id"]);
  formData.append("content", obj["content"]);
  formData.append("read_date", obj["read_date"]);
  for (var i = 0; i < props.files.length; i++) {
    let file = props.files[i];
    formData.append("files", file);
  }
  formData.append("calendars", JSON.stringify(calendars));
  axios
    .put(baseURL + "/api/calendar_duty/send_calendar", formData, config)
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
      props.initData(true);
      swal.close();
      toast.success("Gửi thành công!");
      props.closeDialog();
      // if (response.data.data != null) {
      //   var datas = JSON.parse(response.data.data);
      //   if (datas != null && datas.length > 0) {
      //     datas.forEach((item) => {
      //       socketMethod
      //         .post("sendnotification", {
      //           uids: item.uids,
      //           options: {
      //             title: item["title"],
      //             text: item["text"],
      //             image:
      //               baseURL +
      //               (store.getters.user.background_image ||
      //                 "../assets/background/bg.png"),
      //             tag: "project.soe.vn",
      //             url: "/calendarduty/detail/".concat(item["calendar_duty_id"]),
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
    });
  if (submitted.value) submitted.value = false;
};
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible="props.displayDialog"
    :style="{ width: '40vw' }"
    :maximizable="false"
    :closable="false"
    style="z-index: 1001"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 md-col-12">
          <div class="form-group">
            <label
              >{{
                props.modelsend.is_type_send === 0
                  ? "Quy trình"
                  : props.modelsend.is_type_send === 1
                  ? "Nhóm duyệt"
                  : props.modelsend.is_type_send === 2
                  ? "Người duyệt"
                  : ""
              }}
              <span class="redsao">(*)</span>
            </label>
            <div>
              <div v-if="props.modelsend.is_type_send === 0">
                <Dropdown
                  :options="props.modelsend.procedureforms"
                  :filter="true"
                  :showClear="true"
                  :class="{
                    'p-invalid': props.modelsend.key_id == null && submitted,
                  }"
                  v-model="props.modelsend.key_id"
                  optionLabel="procedureform_name"
                  optionValue="procedureform_id"
                  placeholder="Chọn quy trình"
                  class="ip36 mb-2"
                  @change="changeProcedure(props.modelsend.key_id)"
                >
                  <template #option="slotProps">
                    <div class="country-item flex align-items-center">
                      <div class="pt-1 pl-2">
                        {{ slotProps.option.procedureform_name }}
                      </div>
                    </div>
                  </template>
                </Dropdown>
                <div
                  v-if="
                    props.modelsend.key_id != null &&
                    props.modelsend.signforms != null &&
                    props.modelsend.signforms.length > 0
                  "
                  class="p-inputtext ip36 p-3"
                  style="min-height: 36px; height: auto"
                >
                  <Timeline
                    :value="props.modelsend.signforms"
                    align="alternate"
                    class="customized-timeline"
                  >
                    <template #content="slotProps">
                      <Card class="my-2">
                        <template #title>
                          {{ slotProps.item.signform_name }}
                        </template>
                        <template #subtitle>
                          {{ slotProps.item.type_name }}
                        </template>
                        <template #content>
                          <AvatarGroup class="format-flex-center">
                            <Avatar
                              v-for="(
                                item, index
                              ) in slotProps.item.signuserforms.slice(0, 3)"
                              v-bind:label="
                                item.avatar
                                  ? ''
                                  : item.last_name.substring(0, 1)
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
                              @error="
                                basedomainURL + '/Portals/Image/noimg.jpg'
                              "
                              size="large"
                              shape="circle"
                              class="cursor-pointer"
                              :style="{ backgroundColor: bgColor[index % 7] }"
                            />
                            <Avatar
                              v-if="
                                slotProps.item.signuserforms != null &&
                                slotProps.item.signuserforms.length > 3
                              "
                              v-bind:label="
                                '+' +
                                (
                                  slotProps.item.signuserforms.length - 3
                                ).toString()
                              "
                              shape="circle"
                              size="large"
                              style="background-color: #2196f3; color: #ffffff"
                              class="cursor-pointer"
                            />
                          </AvatarGroup>
                        </template>
                      </Card>
                    </template>
                  </Timeline>
                </div>
              </div>
              <div v-if="props.modelsend.is_type_send === 1">
                <Dropdown
                  :options="props.modelsend.signforms"
                  :filter="true"
                  :showClear="true"
                  :class="{
                    'p-invalid': props.modelsend.key_id == null && submitted,
                  }"
                  v-model="props.modelsend.key_id"
                  optionLabel="signform_name"
                  optionValue="signform_id"
                  placeholder="Chọn nhóm duyệt"
                  class="ip36 mb-2"
                  @change="changeSign(props.modelsend.key_id)"
                >
                  <template #option="slotProps">
                    <div class="country-item flex align-items-center">
                      <div class="pt-1 pl-2">
                        {{ slotProps.option.signform_name }}
                      </div>
                    </div>
                  </template>
                </Dropdown>
                <Card
                  v-if="
                    props.modelsend.key_id != null &&
                    props.modelsend.signform != null
                  "
                  class="my-2"
                >
                  <template #title>
                    {{ props.modelsend.signform.signform_name }}
                  </template>
                  <template #subtitle>
                    {{ props.modelsend.signform.type_name }}
                  </template>
                  <template #content>
                    <AvatarGroup class="format-flex-center">
                      <Avatar
                        v-for="(
                          item, index
                        ) in props.modelsend.signform.signuserforms.slice(0, 3)"
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
                          props.modelsend.signform.signuserforms != null &&
                          props.modelsend.signform.signuserforms.length > 3
                        "
                        v-bind:label="
                          '+' +
                          (
                            props.modelsend.signform.signuserforms.length - 3
                          ).toString()
                        "
                        shape="circle"
                        size="large"
                        style="background-color: #2196f3; color: #ffffff"
                        class="cursor-pointer"
                      />
                    </AvatarGroup>
                  </template>
                </Card>
              </div>
              <div v-if="props.modelsend.is_type_send === 2">
                <Dropdown
                  :options="props.modelsend.users"
                  :filter="true"
                  :showClear="true"
                  :class="{
                    'p-invalid': props.modelsend.user == null && submitted,
                  }"
                  v-model="props.modelsend.user"
                  optionLabel="full_name"
                  placeholder="Chọn người duyệt"
                  class="ip36 mb-2"
                  @change="changeUser(props.modelsend.user)"
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
                                basedomainURL + slotProps.value.avatar
                              "
                              style="
                                background-color: #2196f3;
                                color: #ffffff;
                                width: 2rem;
                                height: 2rem;
                              "
                              :style="{
                                background:
                                  bgColor[slotProps.value.is_order % 7],
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
                          v-bind:image="basedomainURL + slotProps.option.avatar"
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
                          <div>{{ slotProps.option.position_name || "" }}</div>
                          <div>
                            {{ slotProps.option.department_name || "" }}
                          </div>
                        </div>
                      </div>
                    </div>
                    <span v-else> Chưa có dữ liệu tuần </span>
                  </template>
                </Dropdown>
              </div>
            </div>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Nội dung</label>
            <Textarea
              v-model="props.modelsend.content"
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
              :fileLimit="1"
              :show-upload-button="false"
              :show-cancel-button="true"
              @remove="props.removeFile"
              @select="props.selectFile"
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
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog()"
        class="p-button-text"
      />
      <Button label="Gửi" icon="pi pi-send" @click="send()" />
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
