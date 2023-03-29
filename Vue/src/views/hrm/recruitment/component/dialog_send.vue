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
  modelsend:Object,
 
  dataSelected:Array,
 
});

//Function
 
const send = () => {
  submitted.value = true;
  if (!process.value.config_process_id) {
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
  obj.key_id=process.value.config_process_id;
  obj.content=process.value.content;
 

  let formData = new FormData();
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
    .post(baseURL + "/api/hrm_campage_process/add_hrm_campage_process", formData, config)
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
const displayDialog=ref(false);

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
const process=ref({
  content:null,
  config_process_id:null
})
const listProcess=ref([]);
const initTudien=()=>{
  if(props.modelsend.type_module==0&& props.modelsend.type_send==0){
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
              { par: "module_key", va:"M14"},
              { par: "pageno", va:0 },
              { par: "pagesize", va: 100000},
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
            name:element.config_process_name,
            code:element.config_process_id
          })


        });
 

      
      
      } else {
        listdatas.value = [];
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
  }
}
const checkTimline=ref(false);
const signforms=ref();
const changeProcedure = (procedureform_id) => {
  axios
    .post(
      baseURL + "/api/HRM_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_link_approved_list",
            par: [
              { par: "search", va: null},
              { par: "config_process_id", va:procedureform_id},
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
            checkTimline.value=true;
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
    displayDialog.value=props.displayDialog;
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
        <div class="col-12 md-col-12">
          <div class="form-group">
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
                    'p-invalid': process.config_process_id == null && submitted,
                  }"
                  v-model="process.config_process_id"
                  optionLabel="name"
                  optionValue="code"
                  placeholder="Chọn quy trình"
                  class="ip36 mb-2"
                  @change="changeProcedure(process.config_process_id)"
                >
                  
                </Dropdown>
                <div
                  v-if="
                  checkTimline
                  "
                  class="p-inputtext ip36 p-3"
                  style="min-height: 36px; height: auto"
                >
                  <Timeline
                    :value=" signforms"
                    align="alternate"
                    class="customized-timeline"
                  >
                    <template #content="slotProps">
                      <Card class="my-2">
                        <template #title>
                          {{ slotProps.item.approved_group_name }}
                        </template>
                        <template #subtitle>
                        
                          {{ slotProps.item.approved_type == 1
                      ? "Duyệt một nhiều"
                      : slotProps.item.approved_type == 2
                      ? "Duyệt tuần tự"
                      : "Duyệt ngẫu nhiên" }}
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
                                slotProps.item.signusers != null &&
                                slotProps.item.signusers.length > 3
                              "
                              v-bind:label="
                                '+' +
                                (
                                  slotProps.item.signusers.length - 3
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
              <!-- <div v-if="props.modelsend.is_type_send === 1">
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
              </div> -->
            </div>
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
