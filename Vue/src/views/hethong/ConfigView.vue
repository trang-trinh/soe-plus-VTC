<script setup>
import { inject, onMounted, ref } from "vue";
import { useToast } from "vue-toastification";
import { decr, encr } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
const axios = inject("axios"); // inject axios
const config = ref({});
const store = inject("store");
const swal = inject("$swal");
const socket = inject("socket");
const toast = useToast();
const baseUrlCheck = baseURL;
const configHeader = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const initConfig = () => {
  axios
    .get(baseUrlCheck + "/api/Cache/GetConfig", {
      headers: { Authorization: `Bearer ${store.getters.token}` },
    })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        config.value = response.data.data;
        //console.log(response.data.data);
        if (
          config.value != null &&
          (config.value.fileNameSettingApp || "") != "" &&
          (config.value.filePathSettingApp || "") != ""
        ) {
          listFileUpdateApp.value.push({
            file_name: config.value.fileNameSettingApp,
            file_path: config.value.filePathSettingApp,
            file_type: config.value.fileNameSettingApp.substring(
              config.value.fileNameSettingApp.lastIndexOf(".") + 1,
            ),
          });
        } else {
          listFileUpdateApp.value = [];
        }
        filesUpdate = [];
        if (fileApkUp.value) {
          fileApkUp.value.clear();
        }
      } else {
        swal.fire({
          title: "Error!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
      if (config.value.psemail) {
        config.value.psemail = decr(
          config.value.psemail,
          SecretKey,
          cryoptojs,
        ).toString();
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const saveConfig = () => {
  var path_temp = config.value.path_xml;
  config.value.path_xml = encr(
    config.value.path_xml,
    SecretKey,
    cryoptojs,
  ).toString();
  if (config.value.email != null) {
    config.value.psemail = encr(
      config.value.psemail,
      SecretKey,
      cryoptojs,
    ).toString();
  }
  if (listFileUpdateApp.value.length == 0 && filesUpdate.length == 0) {
    config.value.fileNameSettingApp = null;
    config.value.filePathSettingApp = null;
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let formData = new FormData();
  formData.append("modelConfig", JSON.stringify(config.value));
  for (var i = 0; i < filesUpdate.length; i++) {
    let file = filesUpdate[i];
    formData.append("url_file_update", file);
  }
  axios
    // .post(baseURL + "/api/Cache/SetConfig", config.value, {
    //   headers: { Authorization: `Bearer ${store.getters.token}` },
    // })
    .post(baseUrlCheck + "/api/Cache/SetConfig", formData, configHeader)
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        toast.success("Cập nhật thiết lập thành công!");
        initConfig();
      } else {
        swal.fire({
          title: "Error!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
        config.value.path_xml = path_temp;
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
// const initPublicToken=()=>{
//   (async () => {
//     await axios
//     .get(baseURL + "/api/Cache/GetPublicToken?StrToken=vYIrl2C30cdvhjyroM0HYQr2fs7cNo9Qx01g8P1nPIS7joDA"
//     )
//     .then((response) => {
//       console.log(response);
//       swal.close();
//       if (response.data.err != "1") {
//         console.log(
//           response.data
//         );
//       } else {
//         swal.fire({
//           title: "Error!",
//           text: response.data.ms,
//           icon: "error",
//           confirmButtonText: "OK",
//         });
//       }
//     })
//     .catch((error) => {
//       console.log(error);
//       if (error.status === 401) {
//         swal.fire({
//           title: "Error!",
//           text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
//           icon: "error",
//           confirmButtonText: "OK",
//         });
//       }
//     });

//     await axios
//           .post(
//             baseURL + "/api/Proc/CallPublicProc",
//             {
//               proc: "news_list_bhbqp_public",
//               par: [

//                 { par: "search", va: null},
//                 { par: "pageno", va: 0 },
//                 { par: "pagesize", va: 10 }
//               ],
//               publictoken:'hnkweNW8RABLPcnisfHD4HGAaI13oGKqlSIX+kL68oQwMwKvtVvUAvGTlZC5flqA'
//             }
//           )
//           .then((response) => {
//             let data = JSON.parse(response.data.data)[0];
//             let data1 = JSON.parse(response.data.data)[1];
//             console.log("s",data,data1);
//           })
//           .catch((error) => {

//           }); })();
// }
const onChangeToken = () => {
  axios
    .get(baseURL + "/api/Cache/SwitchPublicToken", {
      headers: { Authorization: `Bearer ${store.getters.token}` },
    })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        config.value.publictoken = response.data.data;
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
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const showFormExportLog = ref(false);
const logError = ref({
  start_date: null,
  end_date: null,
});
const exportLogError = () => {
  showFormExportLog.value = true;
};
const closeDialogLog = () => {
  showFormExportLog.value = false;
  logError.value = {
    start_date: null,
    end_date: null,
  };
};
function formatPath(srcPath) {
  let pathReplace = srcPath
    .replace(/\\+/g, "/")
    .replace(/\/+/g, "/")
    .replace(/^\//g, "");
  var listPath = pathReplace.split("/");
  var pathFile = "";
  listPath.forEach((item) => {
    if (item.trim() != "") {
      pathFile += "/" + item;
    }
  });
  return pathFile;
}
const expotFileLog = () => {
  let data = {
    start_date: logError.value.start_date,
    end_date: logError.value.end_date,
  };
  swal.showLoading();
  axios
    .post(baseUrlCheck + "/api/Cache/ExportFileLogError", data, {
      headers: { Authorization: `Bearer ${store.getters.token}` },
    })
    .then((response) => {
      if (response.data.err == "0") {
        //window.open(baseUrlCheck + response.data.path);
        if (response.data.path != null) {
          let pathFile = formatPath(response.data.path);
          let nameFile = formatPath(response.data.filename).replace(/^\//g, "");
          const a = document.createElement("a");
          a.href =
            baseUrlCheck +
            "/Viewer/DownloadFile?url=" +
            encodeURIComponent(pathFile) +
            "&title=" +
            encodeURIComponent(nameFile);
          a.download = nameFile;
          //a.target = "_blank";
          a.click();
          a.remove();
        }
        swal.close();
      } else {
        //console.log(response.data.ms);
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
};
const listFileUpdateApp = ref([]);
var filesUpdate = [];
const onUploadFileApp = (event) => {
  filesUpdate = [];
  event.files.forEach((element) => {
    filesUpdate.push(element);
  });
  if (filesUpdate.length > 0) {
    listFileUpdateApp.value = [];
    let fileUse = filesUpdate[filesUpdate.length - 1];
    config.value.fileNameSettingApp = fileUse.name;
    //listFileUpdateApp.value.push({ file_name: fileUse.name, file_type: fileUse.name.substring(fileUse.name.lastIndexOf('.') + 1) });
  }
};
const loadUpdateApp = (data) => {
  var contentReload = { user_id: "chatbot" };
  socket.emit("sendData", { event: "updateAPK", contentReload });
  toast.success("Gửi yêu cầu update app tivi thành công.");
};
const deleteFileUpdate = (value) => {
  listFileUpdateApp.value = [];
};
const isTrueEmail = ref(true);
const ValidateEmail = () => {
  const textbox = document.getElementById("email");
  if (textbox.value != "" && textbox.value != null) {
    //var mailformat = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/;
    //if (textbox.value.match(mailformat)) {

    const regexExp = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/gi;
    
    if (regexExp.test(textbox.value)) {
      isTrueEmail.value = true;
      return;
    } else {
      isTrueEmail.value = false;
      return;
    }
  }
};
const EncryptUser = () => {
  var data = {};
  axios
    .post(
      baseURL + "/api/Users/EncryptAllUser",
      data,
      config
    )
    .then((response) => {
      if (response.data.err == "0") {
        toast.success("Cập nhật thành công!");
      }
      else if (response.data.err == "2") {
        toast.info(response.data.ms);
      }
      else {
        swal.fire({
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const fileApkUp = ref(null);
onMounted(() => {
  initConfig();

  return {
    saveConfig,
  };
});
</script>
<template>
  <div
    class="main-layout h-full p-4"
    v-if="store.getters.islogin"
  >
    <Card
      class="p-4"
      style="max-height: calc(100vh - 80px); overflow: scroll"
    >
      <template #header>
        <h3><i class="pi pi-cog"></i> Thiết lập tham số hệ thống</h3>
      </template>

      <template #content>
        <form @submit.prevent="saveConfig">
          <div class="grid formgrid m-2">
            <div class="field col-12 md:col-12 flex m-0">
              <label
                class="col-4 text-left"
                style="vertical-align: text-bottom"
                >Socket real time
              </label>
              <div
                class="field-checkbox flex col-8 p-0"
                :class="{
                  'switch-new-true': config.socket,
                  'switch-new-false': !config.socket,
                }"
              >
                <InputSwitch v-model="config.socket" />
                <label for="binary">{{ config.socket ? "Bật" : "Tắt" }}</label>
              </div>
            </div>
            <div class="field col-12 md:col-12">
              <label
                class="col-4 text-left"
                style="vertical-align: text-bottom"
                >Bật Debug
              </label>
              <InputSwitch
                v-model="config.debug"
                class="col-8"
              />
            </div>
            <div class="field col-12 md:col-12">
              <label
                class="col-4 text-left"
                style="vertical-align: text-bottom"
                >Bật Lưu Log
              </label>
              <InputSwitch
                v-model="config.wlog"
                class="col-8"
              />
            </div>
            <div class="field col-12 md:col-12">
              <label
                class="col-4 text-left"
                style="vertical-align: middle"
                >Lưu Token(phút)</label
              >
              <InputNumber
                class="col-8 ip36 p-0"
                v-model="config.timeout"
              />
            </div>
            <div class="field col-12 md:col-12">
              <label
                class="col-4 text-left"
                style="vertical-align: middle"
                >Lưu Log SQL (Milisec)
              </label>
              <InputNumber
                class="col-8 ip36 p-0"
                v-model="config.milisec"
              />
            </div>
            <div class="field col-12 md:col-12">
              <label class="col-4 text-left">Thông báo lỗi </label>
              <InputText
                spellcheck="false"
                class="col-8 ip36"
                v-model="config.logCongtent"
              />
            </div>
            <div class="field col-12 md:col-12">
              <label class="col-4 text-left">Version Cache </label>
              <InputNumber
                class="col-8 ip36 p-0"
                v-model="config.cache"
              />
            </div>
            <div class="field col-12 md:col-12 flex align-items-center">
              <label class="col-4 text-left">Public Token</label>
              <div class="col-8 md:col-8 p-0">
                <div class="p-inputgroup">
                  <InputText
                    placeholder="Public Token"
                    v-model="config.publictoken"
                  />
                  <span
                    class="p-inputgroup-addon cursor-pointer"
                    @click="onChangeToken()"
                    v-tooltip.bottom="'Tạo mã Token mới'"
                  >
                    <i class="pi pi-sync"></i>
                  </span>
                </div>
              </div>
            </div>
            <div class="field col-12 md:col-12">
              <label class="col-4 text-left">API BHBQP</label>
              <InputText
                class="col-8 ip36"
                v-model="config.apiBHBQP"
              />
            </div>
            <div class="field col-12 md:col-12">
              <label class="col-4 text-left">Token BHBQP</label>
              <InputText
                class="col-8 ip36"
                v-model="config.tokenBHBQP"
              />
            </div>
            <div class="field col-12 md:col-12">
              <label class="col-4 text-left">keycode BHBQP </label>
              <InputText
                class="col-8 ip36"
                v-model="config.keycodeBHBQP"
              />
            </div>
            <div class="field col-12 md:col-12">
              <label class="col-4 text-left">Đường dẫn lưu file XML </label>
              <InputText
                class="col-8 ip36"
                v-model="config.path_xml"
              />
            </div>
            <div class="field col-12 md:col-12">
              <label class="col-4 text-left">Đường dẫn Socket </label>
              <InputText
                class="col-8 ip36"
                v-model="config.socketUrl"
              />
            </div>
            <div
              class="field col-12 md:col-12 flex"
              style="align-items: center"
            >
              <label class="col-4 m-0 text-left">File cài đặt App Tivi </label>
              <!-- <InputText class="col-8 ip36" v-model="config.fileSettingApp" /> -->
              <div class="col-8 flex p-0">
                <FileUpload
                  mode="basic"
                  accept=".apk"
                  :fileLimit="1"
                  :maxFileSize="1000000000"
                  :chooseIcon="'pi pi-plus-circle'"
                  @select="onUploadFileApp"
                  chooseLabel="Chọn file .apk"
                  :showCancelButton="true"
                  ref="fileApkUp"
                />
                <Button
                  class="p-button-secondary ml-2"
                  label="Gửi yêu cầu cập nhật"
                  @click="loadUpdateApp"
                />
              </div>
            </div>
            <div
              class="field col-12 md:col-12 flex"
              v-if="listFileUpdateApp.length > 0"
            >
              <label class="col-4 m-0 text-left"></label>
              <div class="col-8 flex p-0">
                <DataView
                  class="w-full h-full ptable p-datatable-sm flex flex-column"
                  :lazy="true"
                  :value="listFileUpdateApp"
                  layout="list"
                  :rowHover="true"
                  responsiveLayout="scroll"
                  :scrollable="true"
                >
                  <template #list="slotProps">
                    <div class="w-full">
                      <Toolbar class="w-full">
                        <template #start>
                          <div class="flex align-items-center">
                            <img
                              class="mr-2"
                              :src="
                                baseUrlCheck +
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
                            @click="deleteFileUpdate(slotProps.data)"
                          />
                        </template>
                      </Toolbar>
                    </div>
                  </template>
                </DataView>
              </div>
            </div>

            <div class="field col-12 md:col-12 flex">
              <label class="col-4 m-0 text-left">Email</label>
              <InputText
                id="email"
                autocomplete="off"
                class="col-8 ip36"
                v-model="config.email"
                @input="ValidateEmail($event)"
              />
            </div>
            <div
              class="field col-12 md:col-12 flex p-0"
              v-if="isTrueEmail == false"
            >
              <label class="col-4 m-0 p-0 text-left"></label>
              <div class="col-8 ip36 p-0 m-0">
                <small class="p-error">Email chưa đúng định dạng.</small>
              </div>
            </div>

            <div
              class="field col-12 md:col-12 flex"
              v-if="
                config.email != null &&
                config.email != '' &&
                isTrueEmail == true
              "
            >
              <label class="col-4 m-0 text-left">Mật khẩu Email</label>
              <InputText
                autocomplete="off"
                class="col-8 ip36 p-0"
                v-model="config.psemail"
                :feedback="false"
                toggleMask
              />
            </div>
          </div>
        </form>
      </template>
      <template #footer>
        <div class="text-center">
          <Button
            class="mr-2"
            icon="pi pi-file"
            label="Xuất log lỗi"
            @click="exportLogError"
          />
          <Button
            icon="pi pi-save"
            label="Cập nhật"
            @click="saveConfig"
          />
        </div>
      </template>
    </Card>
  </div>
  <Dialog
    header="Xuất dữ liệu log"
    v-model:visible="showFormExportLog"
    :autoZIndex="true"
    style="width: 50vw"
  >
    <div class="grid formgrid m-2">
      <div class="col-12 md:col-12 flex">
        <div class="field col-6 md:col-6 p-0">
          <label class="col-3 text-left">Từ ngày</label>
          <Calendar
            class="col-8 p-0"
            id="date_from"
            v-model="logError.start_date"
            placeholder="dd/mm/yyyy"
            :manualInput="false"
            :showIcon="true"
          />
        </div>
        <div class="field col-6 md:col-6 p-0">
          <label class="col-3 text-left">Đến ngày</label>
          <Calendar
            class="col-8 p-0"
            id="date_to"
            v-model="logError.end_date"
            placeholder="dd/mm/yyyy"
            :manualInput="false"
            :showIcon="true"
          />
        </div>
      </div>
    </div>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialogLog()"
        class="p-button-text"
      />
      <Button
        label="Xuất file"
        icon="pi pi-check"
        @click="expotFileLog()"
      />      
      <Button
        label="Encrypt user"
        icon="pi pi-check"
        @click="EncryptUser()"
      />
    </template>
  </Dialog>
</template>
<style lang="scss" scoped>
::v-deep(.switch-new-false) {
  .p-inputswitch-slider {
    background: red !important;
  }
  .p-inputswitch.p-focus .p-inputswitch-slider {
    box-shadow: 0 0 0 0.2rem #f1948a;
  }
}
::v-deep(.switch-new-true) {
  .p-inputswitch-slider {
    background: #2196f3 !important;
  }
}
</style>
