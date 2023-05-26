<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import moment from "moment";
import { encr } from "../../../util/function.js";
import FileInfoVue from "./FileInfo.vue";
const axios = inject("axios"); // inject axios
const store = inject("store");
const swal = inject("$swal");
const toast = useToast();
const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const basedomainURL = fileURL;
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
emitter.on("deleteFile", (obj) => {
  if (obj == true) {
    loadFile();
  }
});
const props = defineProps({
  id: Intl,
  psb: String,
  member: Array,
  isClose: Boolean,
});
const height1 = ref(window.screen.availHeight);
const options = ref({
  PageSize: 20,
  PageNo: 0,
  loading: true,
});
const listFile = ref();
const formatSize = (bytes) => {
  if (bytes === 0) {
    return "0 B";
  }

  let k = 1024,
    dm = 1,
    sizes = ["B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"],
    i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + " " + sizes[i];
};
const loadFile = () => {
  let type = selectedCity.value == -1 ? null : selectedCity.value;
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_file_get",
            par: [
              { par: "id", va: props.id },
              { par: "type", va: type },
              // { par: "pageno", va: options.value.PageNo },
              // { par: "pagesize", va: options.value.PageSize },
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
      let count = JSON.parse(response.data.data)[1];
      data.forEach((element) => {
        element.creator = JSON.parse(element.creator);
        element.file_size_display = formatSize(element.file_size);
        element.creator_tooltip =
          "Người tạo <br/>" +
          element.creator.full_name +
          "<br/>" +
          element.creator.positions +
          "<br/>" +
          (element.creator.department_name != null
            ? element.creator.department_name
            : element.creator.organiztion_name);
      });
      listFile.value = [];
      listFile.value = data;
      options.value.totalRecords = count[0].count;
    })
    .catch((error) => {
      // toast.error("Tải dữ liệu không thành công!");
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
let files = [];
const removeFile = (event) => {
  files = [];
};
const selectFile = (event) => {
  files = [];
  event.files.forEach((element) => {
    files.push(element);
  });
};
const openFileDialog = ref(false);
const OpenFileDialog = () => {
  openFileDialog.value = true;
  files = [];
};
const Upload = () => {
  let checkFile;
  openFileDialog.value = false;
  let formData = new FormData();
  if (files.length == 0) {
    checkFile = "Chưa có tệp tải lên!";
  }
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("url_file", file);
  }
  formData.append("task_id", JSON.stringify(props.id));

  if (checkFile == null) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
    axios
      .post(baseURL + "/api/task_Files/add_Task_File", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Tải tệp tài liệu lên thành công!");
          loadFile();
        } else {
          swal.close();
          swal.fire({
            title: "Thông báo",
            html: "Vui lòng kiểm tra lại: <br/>" + response.data.ms,
            icon: "error",
            confirmButtonText: "OK",
          });
        }
      })
      .catch((error) => {
        swal.close();
        swal.fire({
          title: "Thông báo",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  } else {
    swal.close();
    swal.fire({
      title: "Thông báo",
      text: checkFile,
      icon: "error",
      confirmButtonText: "OK",
    });
  }
};
const DelFile = (file) => {
  let id = [];
  id.push(file.file_id);
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá tệp tài liệu này không!",
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
          .delete(baseURL + "/api/task_Files/delete_Task_File", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: id,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá tệp tài liệu thành công!");
              loadFile();
              emitter.emit("deleteFile", true);
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
const selectedCity = ref();
const filterValue = ref([
  { name: "Tất cả tệp", code: -1 },
  // { name: "Tệp tài liệu dự án", code: 0 },
  { name: "Tệp tài liệu công việc", code: 1 },
  { name: "Tệp bình luận", code: 2 },
  { name: "Tệp báo cáo công việc", code: 3 },
  { name: "Tệp đánh giá công việc", code: 4 },
]);
const filterChange = (e) => {
  loadFile();
};
const close = () => {
  openFileDialog.value = false;
};
const fileInfo = ref();
const isViewFileInfo = ref(false);
const ViewFileInfo = (data) => {
  isViewFileInfo.value = true;
  fileInfo.value = data;
};
emitter.on("closeViewFile", (obj) => {
  isViewFileInfo.value = obj;
});
const Download = (file) => {
  var name = file.file_name || "file_download";
  const a = document.createElement("a");
  a.href =
    basedomainURL +
    "/Viewer/DownloadFile?url=" +
    file.file_path +
    "&title=" +
    name;
  a.download = name;
  a.click();
  a.remove();
};
onMounted(() => {
  selectedCity.value = -1;
  loadFile();
});
</script>
<template>
  <div>
    <div id="table1" class="col-12 p-0 m-0">
      <div
        class="row col-12 format-left font-bold p-scrollpanel-content p-0 m-0 w-full"
      >
        <div class="col-3 format-left">
          <Button
            v-if="props.isClose == false"
            icon="pi pi-upload"
            class=""
            label="Tải tệp lên"
            v-tooltip="{ value: 'Tải tệp lên' }"
            @click="OpenFileDialog()"
          />
        </div>
        <div class="col-4 font-light"></div>
        <div class="col-2 font-light format-right">Lọc theo loại:</div>
        <div class="col-3 format-right">
          <Dropdown
            v-model="selectedCity"
            :options="filterValue"
            optionLabel="name"
            optionValue="code"
            placeholder=""
            @change="filterChange($event)"
            class="w-full"
          />
        </div>
      </div>
      <div
        class="row col-12 format-center font-bold p-scrollpanel-content p-0 m-0 w-full"
      >
        <div class="col-4 my-border">Tên tệp tài liệu</div>
        <div class="col-1 my-border">Ảnh</div>
        <div class="col-1 my-border">Kích cỡ</div>
        <div class="col-2 my-border">Loại</div>
        <div class="col-2 my-border">Ngày tạo</div>
        <div class="col-2 my-border flex p-0">
          <div class="col-6 text-left p-0">Tạo bởi</div>
          <div class="col-6"></div>
        </div>
      </div>
    </div>
    <ScrollPanel
      :style="
        height1 < 1000
          ? 'height: calc(81vh) !important'
          : 'height: calc(83vh) !important'
      "
    >
      <div id="table2" v-if="options.totalRecords > 0" class="row col-12 p-0">
        <div
          v-for="(slotProps, index) in listFile"
          :key="index"
          class="py-0 flex"
          v-on:dblclick="ViewFileInfo(slotProps)"
          v-tooltip="{ value: 'Nháy chuột 2 lần để xem chi tiết' }"
        >
          <div class="col-4 format-left">
            <div class="" v-if="slotProps.is_image == 1">
              <Image
                :src="basedomainURL + slotProps.file_path"
                :alt="slotProps.file_name"
                width="24"
                preview
                style="
                  max-width: 24px;
                  white-space: nowrap;
                  overflow: hidden;
                  text-overflow: ellipsis;
                "
              />
            </div>
            <div class="" v-else>
              <img
                :src="
                  basedomainURL +
                  '/Portals/Image/file/' +
                  slotProps.file_name.substring(
                    slotProps.file_name.lastIndexOf('.') + 1
                  ) +
                  '.png'
                "
                style="width: 24px; object-fit: contain"
                :alt="' '"
                class="pt-1"
              />
            </div>
            <div
              class="format-left"
              style="
                max-width: 30rem;
                white-space: nowrap;
                overflow: hidden;
                text-overflow: ellipsis;
              "
            >
              <span
                class="pl-2 w-full"
                v-tooltip.top="{ value: slotProps.file_name }"
              >
                {{ " " + slotProps.file_name }}
              </span>
            </div>
          </div>
          <div class="col-1 format-center">
            <Checkbox
              :binary="slotProps.is_image"
              v-model="slotProps.is_image"
              class="format-center"
              :disabled="true"
            />
          </div>

          <div class="col-1 format-center">
            {{ slotProps.file_size_display }}
          </div>
          <div class="col-2 format-center">
            <span v-if="slotProps.is_type == 0">Tệp tài liệu dự án</span>
            <span v-if="slotProps.is_type == 1">Tệp tài liệu công việc</span>
            <span v-if="slotProps.is_type == 2">Tệp bình luận</span>
            <span v-if="slotProps.is_type == 3">Tệp báo cáo công việc</span>
            <span v-if="slotProps.is_type == 4">Tệp đánh giá công việc</span>
          </div>
          <div class="col-2 format-center">
            {{
              moment(new Date(slotProps.created_date)).format(
                "HH:mm DD/MM/YYYY"
              )
            }}
          </div>
          <div class="col-2 p-0 format-center">
            <div class="col-3">
              <Avatar
                @error="
                  $event.target.src =
                    basedomainURL + '/Portals/Image/nouser1.png'
                "
                v-tooltip.right="{
                  value: slotProps.creator_tooltip,
                  escape: true,
                }"
                v-bind:label="
                  slotProps.creator.avt
                    ? ''
                    : slotProps.creator.full_name
                        .split(' ')
                        .at(-1)
                        .substring(0, 1)
                "
                v-bind:image="basedomainURL + slotProps.creator.avt"
                style="color: #ffffff; cursor: pointer"
                :style="{
                  background: bgColor[Math.floor(Math.random() * 10) % 7],
                  border: '2px solid' + bgColor[(Math.random() * 11) % 7],
                }"
                class="p-0 m-0"
                size="small"
                shape="circle"
              />
            </div>
            <div class="col-9 flex">
              <a style="text-decoration: none" class="a-hover format-center">
                <Button
                  icon="pi pi-eye "
                  class="p-button-text p-button-secondary p-button-hover"
                  v-tooltip.bottom="{ value: 'Xem' }"
                  @click="ViewFileInfo(slotProps)"
                >
                </Button
              ></a>
              <a style="text-decoration: none" class="a-hover format-center">
                <Button
                  icon="pi pi-download "
                  class="p-button-text p-button-secondary p-button-hover"
                  v-tooltip.bottom="{ value: 'Tải tệp xuống' }"
                  @click="Download(slotProps)"
                >
                </Button
              ></a>
              <a
                v-if="props.isClose == false"
                style="text-decoration: none"
                class="a-hover format-center"
              >
                <Button
                  icon="pi pi-trash"
                  class="p-button-text p-button-secondary p-button-hover"
                  @click="DelFile(slotProps)"
                  v-tooltip.bottom="{ value: 'Xóa' }"
                  v-if="store.state.user.user_id == slotProps.created_by"
              /></a>
            </div>
          </div>
        </div>
      </div>
      <div
        class="row col-12 align-items-center justify-content-center p-4 text-center m-auto"
        v-else
      >
        <img src="../../../assets/background/nodata.png" height="144" />
        <h3 class="m-1">Không có tệp tài liệu</h3>
      </div>
    </ScrollPanel>
  </div>
  <Dialog
    header="Tải lên tệp tài liệu"
    v-model:visible="openFileDialog"
    :style="{ width: '40vw' }"
    :closable="true"
  >
    <form>
      <FileUpload
        @remove="removeFile"
        @select="selectFile"
        :show-upload-button="false"
        choose-label="Chọn tệp"
        cancel-label="Hủy"
        :multiple="true"
        :maxFileSize="104857600"
        :invalidFileSizeMessage="'Tệp tải lên không quá 100MB!'"
      >
        <template #empty>
          <p>Kéo và thả tệp vào đây để tải lên.</p>
        </template>
      </FileUpload>
    </form>
    <template #footer>
      <Button label="Huỷ" icon="pi pi-times" @click="close()" />
      <Button label="Lưu" icon="pi pi-check" @click="Upload" />
    </template>
    <!-- Chức năng đang chỉnh sửa vui lòng liên hệ quản trị viên phần mềm -->
  </Dialog>
  <FileInfoVue :data="fileInfo" v-if="isViewFileInfo"></FileInfoVue>
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

#table2 > div:hover {
  background-color: #d8edff;
}

.p-button-hover:hover {
  color: #0025f8 !important;
  background: #ffffff !important;
}
</style>
