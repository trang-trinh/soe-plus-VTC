<template>
  <div>
    <Dialog
      :header="props.header"
      v-model:visible="openDocDialog"
      :style="{ width: '60vw' }"
      :closable="true"
      :maximizable="true"
      :autoZIndex="true"
      @maximize="checkMaximize = true"
      @unmaximize="checkMaximize = false"
    >
      <div class="col-12">
        <DataTable
          :value="LitsDocToLink"
          v-model:first="options.firstPage"
          :paginator="true"
          :rows="options.PageSize"
          :scrollable="true"
          :scrollHeight="checkMaximize == false ? '60vh' : '70vh'"
          :loading="options.loading"
          v-model:selection="selectedFields"
          :lazy="true"
          @page="onPage($event)"
          :totalRecords="options.totalRecord"
          dataKey="doc_master_id"
          :rowHover="true"
          filterDisplay="menu"
          :showGridlines="true"
          filterMode="lenient"
          paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
          :rowsPerPageOptions="[20, 30, 50, 100, 200]"
          responsiveLayout="scroll"
          @row-click="onRowSelect($event)"
        >
          <template #header>
            <div class="col-12 p-0 m-0">
              <InputText
                type="text"
                spellcheck="false"
                v-model="SearchDoc"
                class="col-12"
                placeholder="Nhập tên tệp..."
                @keyup.enter="SearchDocFunc()"
              ></InputText>
            </div>
          </template>
          <Column
            selectionMode="multiple"
            headerStyle="text-align:center;max-width:2rem;"
            bodyStyle="text-align:center;max-width:2.15rem"
            class="align-items-center justify-content-center text-center"
          >
          </Column>
          <Column
            field="file_name"
            header="Tên tệp văn bản"
          ></Column>
          <Column
            field="filesize_display"
            header="Kích thước tệp"
            headerStyle="max-width:10rem"
            bodyStyle="text-align:center;max-width:10rem"
            class="align-items-center justify-content-center text-center"
          ></Column>
          <Column
            header="Chức năng"
            headerStyle="max-width:8rem"
            bodyStyle="text-align:center;max-width:8rem"
            class="align-items-center justify-content-center text-center"
          >
            <template #body="f">
              <a
                download
                style="text-decoration: none"
                class="a-hover format-center"
              >
                <Button
                  icon="pi pi-download "
                  class="p-button-text p-button-secondary p-button-hover"
                  v-tooltip="{ value: 'Tải tệp xuống' }"
                  @click="download(f)"
                >
                </Button>
              </a>
            </template>
          </Column>
          <template #empty>
            <div
              class="align-items-center justify-content-center p-4 text-center m-auto"
              v-if="LitsDocToLink == null"
            >
              <img
                src="../../../assets/background/nodata.png"
                height="144"
              />
              <h3 class="m-1">Không có dữ liệu</h3>
            </div>
          </template>
        </DataTable>
      </div>
      <template #footer>
        <Button
          label="Hủy"
          icon="pi pi-times"
          @click="openDocDialog = false"
        />
        <Button
          label="Lưu"
          icon="pi pi-check"
          @click="LinkDoc()"
          v-if="checkSelect != false"
        />
      </template>
      <!-- Chức năng đang chỉnh sửa vui lòng liên hệ quản trị viên phần mềm -->
    </Dialog>
  </div>
  <FileInfoVue
    :data="fileInfo"
    v-if="isViewFileInfo"
  ></FileInfoVue>
</template>

<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import FileInfoVue from "./FileInfo.vue";
import { encr } from "../../../util/function.js";
const cryoptojs = inject("cryptojs");
const axios = inject("axios"); // inject axios
const store = inject("store");
const swal = inject("$swal");
const toast = useToast();
const emitter = inject("emitter");
emitter.on("closeViewFile", (obj) => {
  isViewFileInfo.value = obj;
});
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const today = ref({});
today.value = new Date();
const basedomainURL = fileURL;
//Lấy size màn hình
const props = defineProps({
  id: Intl,
  header: String,
  visible: Boolean,
  main: Boolean,
});
//Khai báo
const user = store.state.user;

const TaskLinkDOC = ref({
  organization_id: null,
  task_id: null,
  doc_master_id: null,
  is_main: null,
});
const LitsDocToLink = ref();
const SearchDoc = ref();
const options = ref({
  loading: true,
  firstPage: 0,
  PageNo: 0,
  PageSize: 20,
});
const LoadDocToLink = () => {
  LitsDocToLink.value = [];
  options.value.loading = true;
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "list_Doc_toLink_Task",
            par: [
              { par: "user_id", va: user.user_id },
              { par: "pageno", va: options.value.PageNo },
              { par: "pagesize", va: options.value.PageSize },
              { par: "search", va: SearchDoc.value },
              { par: "task_id", va: props.id },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      let count = JSON.parse(response.data.data)[1];
      options.value.totalRecord = count[0].totalRecord;
      data.forEach((x) => {
        x.filesize_display = x.file_size ? formatSize(x.file_size) : "";
      });
      LitsDocToLink.value = data;
      options.value.loading = false;
    })
    .catch((error) => {
      //// toast.error("Tải dữ liệu không thành công!");
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
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
const selectedFields = ref();
const openDocDialog = ref();
watch(openDocDialog, (vl) => {
  if (openDocDialog.value == false) {
    emitter.emit("closeDoc", false);
  }
});
const checkSelect = ref(false);
watch(selectedFields, (vl) => {
  if (
    selectedFields.value == null ||
    selectedFields.value == [] ||
    selectedFields.value.length == 0 ||
    selectedFields.value == {}
  ) {
    checkSelect.value = false;
  } else checkSelect.value = true;
});
const onPage = (event) => {
  if (event.rows != options.value.PageSize) {
    options.value.PageSize = event.rows;
  }
  options.value.PageNo = event.page;
  LoadDocToLink();
};
const checkMaximize = ref(false);
const SearchDocFunc = () => {
  LoadDocToLink();
};
const LinkDoc = () => {
  let list = [];
  selectedFields.value.forEach((x) => {
    TaskLinkDOC.value = {
      organization_id: user.organization_id,
      task_id: props.id,
      doc_master_id: null,
      is_main: props.main,
    };
    TaskLinkDOC.value.doc_master_id = x.doc_master_id;
    let task = TaskLinkDOC.value;
    list.push(task);
  });

  axios
    .post(baseURL + "/api/task_origin/Add_LinkTask_Doc", list, config)
    .then((response) => {
      selectedFields.value = [];
      openDocDialog.value = false;
      if (response.data.err != "1") {
        toast.success("Liên kết văn bản thành công");
      } else toast.success("Liên kết văn bản thất bại! Vui lòng kiểm tra lại");
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
const download = (file) => {
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
const fileInfo = ref();
const isViewFileInfo = ref(false);
const onRowSelect = (e) => {
  isViewFileInfo.value = true;
  fileInfo.value = e.data;
};

onMounted(() => {
  LoadDocToLink();
  if (props.visible == true) {
    openDocDialog.value = true;
  } else {
    return;
  }
  return;
});
</script>

<style scoped>
.p-button-hover:hover {
  color: #0025f8 !important;
  background: #ffffff !important;
}
</style>
