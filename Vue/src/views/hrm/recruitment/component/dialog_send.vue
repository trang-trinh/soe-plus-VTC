<script setup>
import { ref, inject, onMounted } from "vue";
import { encr } from "../../../../util/function";
import { socketMethod } from "../../../../util/methodSocket";
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

  dataSelected: Array,
});

//Function
const listTCard = ref([
  { name: "Duyệt một nhiều", code: 1 },
  { name: "Duyệt tuần tự", code: 2 },
  { name: "Duyệt ngẫu nhiên", code: 3 },
]);

const send = () => {
  submitted.value = true;
  if (!process.value.key_id) {
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
  let formData = new FormData();
  if (props.modelsend.type_send == 2) {
    formData.append("aprroved_type", process.value.aproved_type);
    let strv = "",
      strc = "";
    process.value.key_id.forEach((element) => {
      strv += strc + element.code;
      strc = ",";
    });
    obj.key_id = strv;
  } else if (props.modelsend.type_send == 1) {
    obj.key_id = process.value.key_id.code;
  } else {
    obj.key_id = process.value.key_id;
  }
  if( process.value.content)
  obj.content = process.value.content;
else
obj.content ="";
debugger
  formData.append("type_send", obj["type_send"]);
  formData.append("key_id", obj["key_id"]);
  formData.append("type_module", obj["type_module"]);
  formData.append("content", obj["content"]);
  for (var i = 0; i < filesList.value.length; i++) {
    let file = filesList.value[i];
    formData.append("files", file);
  }
  formData.append("hrm_obj", JSON.stringify(props.dataSelected));

  axios
    .post(
      baseURL + "/api/hrm_campage_process/add_hrm_campage_process",
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
        return;
      }

      swal.close();
      toast.success("Gửi thành công!");
      props.closeDialog();
    })
    .catch((error) => {
      console.log(error);
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
const displayDialog = ref(false);

const filesList = ref([]);

const onUploadFile = (event) => {
  filesList.value = [];

  var ms = false;

  event.files.forEach((fi) => {
    let formData = new FormData();
    formData.append("fileupload", fi);
    axios({
      method: "post",
      url: baseURL + `/api/chat/ScanFileUpload`,
      data: formData,
      headers: {
        Authorization: `Bearer ${store.getters.token}`,
      },
    })
      .then((response) => {
        if (response.data.err != "1") {
          if (fi.size > 100 * 1024 * 1024) {
            ms = true;
          } else {
            filesList.value.push(fi);
          }
        } else {
          filesList.value = filesList.value.filter((x) => x.name != fi.name);
          swal.fire({
            title: "Cảnh báo",
            text: "File bị xóa do tồn tại mối đe dọa với hệ thống!",
            icon: "warning",
            confirmButtonText: "OK",
          });
        }
        if (ms) {
          swal.fire({
            icon: "warning",
            type: "warning",
            title: "Thông báo",
            text: "Bạn chỉ được upload file có dung lượng tối đa 100MB!",
          });
        }
      })
      .catch(() => {
        swal.fire({
          title: "Thông báo",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  });
};
const removeFile = (event) => {
  filesList.value = filesList.value.filter((a) => a != event.file);
};
const process = ref({
  content: null,
  key_id: null,
  aproved_type: 3,
});
const listProcess = ref([]);
const listAproved = ref([]);
const listUsers = ref([]);
const initTudien = () => {
  if (props.modelsend.type_module == 0 && props.modelsend.type_send == 0) {
    listProcess.value = [];
    axios
      .post(
        baseURL + "/api/hrm_ca_SQL/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "sys_config_process_list_module",
              par: [
                { par: "search", va: null },
                { par: "user_id", va: store.getters.user.user_id },
                { par: "module_key", va: props.modelsend.module_key },
                { par: "pageno", va: 0 },
                { par: "pagesize", va: 100000 },
                { par: "status", va: null },
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
          let data = JSON.parse(response.data.data)[0];

          data.forEach((element, i) => {
            listProcess.value.push({
              name: element.config_process_name,
              code: element.config_process_id,
            });
          });
        } else {
          listProcess.value = [];
        }
        swal.close();
      })
      .catch((error) => {
        console.log(error);
        swal.close();
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      });
  } else if (
    props.modelsend.type_module == 0 &&
    props.modelsend.type_send == 1
  ) {
    listAproved.value = [];
    axios
      .post(
        baseURL + "/api/HRM_SQL/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "sys_config_approved_list_module",
              par: [
                { par: "pageno", va: 0 },
                { par: "pagesize", va: 100000 },
                { par: "user_id", va: store.getters.user.user_id },
                { par: "module_key", va: props.modelsend.module_key },
                { par: "status", va: null },
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
              if (tbs[0].length > 0) {
                tbs[0].forEach((element, i) => {
                  element["STT"] = i + 1;
                  if (element["signusers"] != null) {
                    element["signusers"] = JSON.parse(element["signusers"]);
                  }
                  if (element["signusers"] != null) {
                    element["signusers"].forEach((ilem) => {
                      if (ilem.is_order == "") ilem.is_order = null;
                      else ilem.is_order = Number(ilem.is_order);
                      if (ilem.approved_users_id == "")
                        ilem.approved_users_id = null;
                      else
                        ilem.approved_users_id = Number(ilem.approved_users_id);
                      if (ilem.department_id == "") ilem.department_id = null;
                      else ilem.department_id = Number(ilem.department_id);
                      if (ilem.avatar == "") ilem.avatar = null;
                    });

                    listAproved.value.push({
                      name: element.approved_group_name,
                      code: element.approved_groups_id,
                      data: element,
                      signusers: element.signusers,
                    });
                  }
                });
              }
            }
          }
        }
        swal.close();
      })
      .catch((error) => {
        swal.close();
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        console.log(error);
        return;
      });
  } else if (
    props.modelsend.type_module == 0 &&
    props.modelsend.type_send == 2
  ) {
    listUsers.value = [];
    axios
      .post(
        baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "sys_users_list_dd",
              par: [
                { par: "search", va: null },
                { par: "user_id", va: store.getters.user.user_id },
                { par: "role_id", va: null },
                {
                  par: "organization_id",
                  va: store.getters.user.organization_id,
                },
                { par: "department_id", va: null },
                { par: "position_id", va: null },
                { par: "pageno", va: 1 },
                { par: "pagesize", va: 100000 },
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
        data.forEach((element, i) => {
          listUsers.value.push({
            name: element.full_name,
            code: element.user_key,
            avatar: element.avatar,
            department_name: element.department_name,
            role_name: element.role_name,
            position_name: element.position_name,
          });
        });
      })
      .catch((error) => {
        if (error && error.status === 401) {
          swal.fire({
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
  }
};
const checkTimline = ref(false);
const signforms = ref();
const changeProcedure = (procedureform_id) => {
  axios
    .post(
      baseURL + "/api/HRM_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_link_approved_list",
            par: [
              { par: "search", va: null },
              { par: "config_process_id", va: procedureform_id },
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
            signforms.value = tbs[0];
            if (signforms.value.length > 0) {
              signforms.value.forEach((element, i) => {
                element["STT"] = i + 1;

                if (element["signusers"] != null) {
                  element["signusers"] = JSON.parse(element["signusers"]);
                }
              });
            }
            checkTimline.value = true;
          } else {
            signforms.value = [];
          }
        }
      }
      swal.close();
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
};
onMounted(() => {
  displayDialog.value = props.displayDialog;
  initTudien();
});
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible="displayDialog"
    :style="{ width: '40vw' }"
    :maximizable="false"
    :closable="true"
    style="z-index: 1001"
    @hide="props.closeDialog"
    :modal="true"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12  py-0 ">
          <div class="form-group pb-0 mb-0">
            <label
              >{{
                props.modelsend.type_send === 0
                  ? "Quy trình"
                  : props.modelsend.type_send === 1
                  ? "Nhóm duyệt"
                  : props.modelsend.type_send === 2
                  ? "Người duyệt"
                  : ""
              }}
              <span class="redsao">(*)</span>
            </label>
            <div>
              <div v-if="props.modelsend.type_send === 0">
                <Dropdown
                  :options="listProcess"
                  :filter="true"
                  :showClear="true"
                  :class="{
                    'p-invalid': process.key_id == null && submitted,
                  }"
                  v-model="process.key_id"
                  optionLabel="name"
                  optionValue="code"
                  placeholder="Chọn quy trình"
                  class="ip36 mb-2"
                  @change="changeProcedure(process.key_id)"
                >
                </Dropdown>
                <div
                  v-if="checkTimline"
                  class="p-inputtext ip36 p-3"
                  style="min-height: 36px; height: auto"
                >
                  <Timeline
                    :value="signforms"
                    align="alternate"
                    class="customized-timeline"
                  >
                    <template #content="slotProps">
                      <Card class="my-2">
                        <template #title>
                          {{ slotProps.item.approved_group_name }}
                        </template>
                        <template #subtitle>
                          {{
                            slotProps.item.approved_type == 1
                              ? "Duyệt một nhiều"
                              : slotProps.item.approved_type == 2
                              ? "Duyệt tuần tự"
                              : "Duyệt ngẫu nhiên"
                          }}
                        </template>
                        <template #content>
                          <AvatarGroup class="format-flex-center">
                            <Avatar
                              v-for="(
                                item, index
                              ) in slotProps.item.signusers.slice(0, 3)"
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
                                slotProps.item.signusers != null &&
                                slotProps.item.signusers.length > 3
                              "
                              v-bind:label="
                                '+' +
                                (slotProps.item.signusers.length - 3).toString()
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
              <div
                v-if="props.modelsend.type_send === 1"
                class="col-12 md:col-12 p-0 flex"
              >
                <Dropdown
                  v-model="process.key_id"
                  :options="listAproved"
                  optionLabel="name"
                  :filter="true"
                  panelClass="d-design-dropdown"
                  placeholder="Chọn nhóm duyệt"
                  class="p-0 p-design-dropdown w-full mb-2"
                  ref="isRefAprroved"
                >
                  <template #value="slotProps">
                    <div
                      class="p-dropdown-car-value format-center w-full h-full"
                      v-if="slotProps.value"
                    >
                      <div>
                        <div class="font-bold p-2 px-0">
                          {{ slotProps.value.name }}
                        </div>

                        <div class="flex px-2 format-center py-0">
                          <div v-if="slotProps.value.signusers">
                            <AvatarGroup
                              v-if="
                                slotProps.value.signusers &&
                                slotProps.value.signusers.length > 0
                              "
                            >
                              <Avatar
                                v-for="(
                                  elen, index1
                                ) in slotProps.value.signusers.slice(0, 3)"
                                v-bind:label="
                                  elen.avatar
                                    ? ''
                                    : elen.last_name.substring(0, 1)
                                "
                                v-bind:image="
                                  elen.avatar
                                    ? basedomainURL + elen.avatar
                                    : basedomainURL + '/Portals/Image/noimg.jpg'
                                "
                                v-tooltip.bottom="{
                                  value:
                                    elen.full_name +
                                    '<br/>' +
                                    elen.position_name +
                                    '<br/>' +
                                    elen.department_name,
                                  escape: true,
                                }"
                                :key="elen.user_id"
                                style="
                                  border: 2px solid white;
                                  color: white;
                                  width: 2.5rem;
                                  height: 2.5rem;
                                "
                                @error="
                                  basedomainURL + '/Portals/Image/noimg.jpg'
                                "
                                size="large"
                                shape="circle"
                                class="cursor-pointer"
                                :style="{
                                  backgroundColor: bgColor[index1 % 7],
                                }"
                              />
                              <Avatar
                                v-if="
                                  slotProps.value.signusers &&
                                  slotProps.value.signusers.length > 3
                                "
                                v-bind:label="
                                  '+' +
                                  (
                                    slotProps.value.signusers.length - 3
                                  ).toString()
                                "
                                shape="circle"
                                size="large"
                                style="
                                  background-color: #2196f3;
                                  color: #ffffff;
                                "
                                class="cursor-pointer"
                              />
                            </AvatarGroup>
                          </div>
                        </div>
                      </div>
                    </div>
                    <div v-else class="format-center h-full">
                      {{ slotProps.placeholder }}
                    </div>
                  </template>
                  <template #option="slotProps">
                    <div class="p-dropdown-car-option format-center">
                      <div>
                        <div class="font-bold p-2 px-0">
                          {{ slotProps.option.name }}
                        </div>

                        <div class="flex px-2 py-0 format-center">
                          <div v-if="slotProps.option.signusers">
                            <AvatarGroup
                              v-if="
                                slotProps.option.signusers &&
                                slotProps.option.signusers.length > 0
                              "
                            >
                              <Avatar
                                v-for="(
                                  elen, index1
                                ) in slotProps.option.signusers.slice(0, 3)"
                                v-bind:label="
                                  elen.avatar
                                    ? ''
                                    : elen.last_name.substring(0, 1)
                                "
                                v-bind:image="
                                  elen.avatar
                                    ? basedomainURL + elen.avatar
                                    : basedomainURL + '/Portals/Image/noimg.jpg'
                                "
                                v-tooltip.bottom="{
                                  value:
                                    elen.full_name +
                                    '<br/>' +
                                    elen.position_name +
                                    '<br/>' +
                                    elen.department_name,
                                  escape: true,
                                }"
                                :key="elen.user_id"
                                style="
                                  border: 2px solid white;
                                  color: white;
                                  width: 2.5rem;
                                  height: 2.5rem;
                                "
                                @error="
                                  basedomainURL + '/Portals/Image/noimg.jpg'
                                "
                                size="large"
                                shape="circle"
                                class="cursor-pointer"
                                :style="{
                                  backgroundColor: bgColor[index1 % 7],
                                }"
                              />
                              <Avatar
                                v-if="
                                  slotProps.option.signusers &&
                                  slotProps.option.signusers.length > 3
                                "
                                v-bind:label="
                                  '+' +
                                  (
                                    slotProps.option.signusers.length - 3
                                  ).toString()
                                "
                                shape="circle"
                                size="large"
                                style="
                                  background-color: #2196f3;
                                  color: #ffffff;
                                "
                                class="cursor-pointer"
                              />
                            </AvatarGroup>
                          </div>
                        </div>
                      </div>
                    </div>
                  </template>
                </Dropdown>
              </div>
              <div v-if="props.modelsend.type_send === 2">
                <div class="col-12 p-0 field">
                <MultiSelect
                  :options="listUsers"
                  :filter="true"
                  :showClear="true"
                  :class="{
                    'p-invalid': process.key_id == null && submitted,
                  }"
                  v-model="process.key_id"
                  optionLabel="name"
                  optioncode="code"
                  display="chip"
                  placeholder="Chọn người duyệt"
                  class="ip36 mb-2"
                  style="height: auto; min-height: 36px"
                >
                  <template #option="slotProps">
                    <div v-if="slotProps.option" class="flex">
                      <div class="format-center">
                        <Avatar
                          style="color: #fff"
                          v-bind:label="
                            slotProps.option.avatar
                              ? ''
                              : slotProps.option.name.substring(
                                  slotProps.option.name.lastIndexOf(' ') + 1,
                                  slotProps.option.name.lastIndexOf(' ') + 2
                                )
                          "
                          :image="basedomainURL + slotProps.option.avatar"
                          size="small"
                          :style="
                            slotProps.option.avatar
                              ? 'background-color: #2196f3'
                              : 'background:' +
                                bgColor[slotProps.option.name.length % 7]
                          "
                          shape="circle"
                          @error="
                            $event.target.src =
                              basedomainURL + '/Portals/Image/nouser1.png'
                          "
                        />
                      </div>
                      <div class="ml-3">
                        <div class="mb-1">{{ slotProps.option.name }}</div>
                        <div class="description">
                          <div>{{ slotProps.option.position_name || "" }}</div>
                          <div>
                            {{ slotProps.option.department_name || "" }}
                          </div>
                        </div>
                      </div>
                    </div>
                  </template>
                </MultiSelect>
              </div>
                
              </div>
            </div>
          </div>
        </div>

        <div class="col-12 md:col-12 py-0">
          <div class="form-group pb-0">
            <label>Loại duyệt</label>
            <Dropdown
              v-model="process.aproved_type"
              panelClass="d-design-dropdown"
              :options="listTCard"
              :filter="true"
              optionLabel="name"
              optionValue="code"
              class="w-full"
            >
            </Dropdown>
          </div>
        </div>
        <div class="col-12 p-0 field" v-if="process.aproved_type==2">
          <div class="col-12  field">
                    <OrderList
                      v-model="process.key_id"
                      listStyle="height:auto"
                      class="w-full"
                      dataKey="id"
                    >
                      <template #header>Thứ tự duyệt </template>
                      <template #item="slotProps">
                        <Toolbar class="surface-0 m-0 p-0 border-0 w-full">
                          <template #start>
                            <div class="flex align-items-center">
                              <div class="format-flex-center">
                                <b class="p-3">{{ slotProps.index + 1 }} </b>
                              </div>
                              <div class="flex">
                                <Avatar
                                  v-bind:label="
                                    slotProps.item.avatar
                                      ? ''
                                      : slotProps.item.name.substring(
                                          slotProps.item.name.lastIndexOf(
                                            ' '
                                          ) + 1,
                                          slotProps.item.name.lastIndexOf(
                                            ' '
                                          ) + 2
                                        )
                                  "
                                  :image="basedomainURL + slotProps.item.avatar"
                                  class="w-2rem h-2rem"
                                  size="large"
                                  :style="
                                    slotProps.item.avatar
                                      ? 'background-color: #2196f3'
                                      : 'background:' +
                                        bgColor[
                                          slotProps.item.name.length % 7
                                        ]
                                  "
                                  shape="circle"
                                  @error="
                                    $event.target.src =
                                      basedomainURL +
                                      '/Portals/Image/nouser1.png'
                                  "
                                />
                                <div class="pt-1 pl-2">
                                  {{ slotProps.item.name }}
                                </div>
                              </div>
                            </div>
                          </template>
                          
                        </Toolbar>
                      </template>
                    </OrderList>
                  </div>
             </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Nội dung</label>
            <Textarea
              v-model="process.content"
              :autoResize="true"
              rows="5"
              cols="30"
              class="w-full"
            />
          </div>
        </div>
        <div class="col-12 md-col-12">
          <div class="form-group">
            <label>Tệp đính kèm</label>
            <FileUpload
              chooseLabel="Chọn tệp"
              :showUploadButton="false"
              :showCancelButton="false"
              :multiple="false"
              :maxFileSize="524288000"
              @select="onUploadFile"
              @remove="removeFile"
              :invalidFileSizeMessage="'{0}: Dung lượng File không được lớn hơn {1}'"
            >
              <template #empty>
                <p class="p-0 m-0 text-500">Kéo thả hoặc chọn File.</p>
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
        class="p-button-outlined"
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
