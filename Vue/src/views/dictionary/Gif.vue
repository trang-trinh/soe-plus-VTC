<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../util/function.js";
import moment from "moment";
const cryoptojs = inject("cryptojs");
//khai báo
const axios = inject("axios");
const store = inject("store");
const toast = useToast();
const swal = inject("$swal");
const basedomainURL = baseURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const images = ref([]);
const first = ref();
const options = ref({
  IsNext: true,
  sort: "is_order desc",
  SearchText: "",
  PageNo: 0,
  PageSize: 18,
  loading: true,
  totalRecords: null,
});
const layout = ref("grid");
const isFirst = ref(false);
const datalists = ref();
const openDialog = ref(false);
const openDialog2 = ref(false);
const headerDialog = ref();
const headerDialog2 = ref();
const width = ref(window.screen.width);

let files = [];
//Thêm log
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
//Dữ liệu
const loadData = (rf) => {
  options.value.loading = true;
  if (rf) {
    axios
      .post(
        // eslint-disable-next-line no-undef
        baseURL + "/api/DictionaryProc/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "birthday_gif_list",
              par: [
                { par: "pageno", va: options.value.PageNo },
                { par: "pagesize", va: options.value.PageSize },
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
        datalists.value = data;
        let data1 = JSON.parse(response.data.data)[1];
        options.value.totalRecords = data1[0].totalRecords;
        options.value.loading = false;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        addLog({
          title: "Lỗi Console loadData",
          controller: "Gif.vue",
          logcontent: error.message,
          loai: 2,
        });
        if (error && error.status === 401) {
          swal.fire({
            title: "Thông báo",
            text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
            icon: "error",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
  }
};
const AddNewValue = (str) => {
  openDialog.value = true;
  headerDialog.value = str;
  files = [];
};
const removeFile = (event) => {
  files = files.filter((x) => x.name != event.file.name);
};
const selectFile = (event) => {
  event.files.forEach((element) => {
    files.push(element);
  });
};
const Upload = () => {
  let checkFile;
  openDialog.value = false;
  let formData = new FormData();
  if (files.length == 0) {
    checkFile = "Chưa có tệp tải lên!";
  }
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("url_file", file);

    if (file.name.includes(".gif") == true) {
      checkFile = null;
    } else {
      checkFile = "File không đúng định dạng! Vui lòng kiểm tra lại!";
    }
  }
  if (checkFile == null) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
    axios
      .post(baseURL + "/api/BirthDay_Gif/AddGif", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm mới Gif thành công!");
          loadData(true);
        } else {
          swal.close();
          swal.fire({
            title: "Thông báo",
            html: "Vui lòng kiểm tra lại: <br>" + response.data.ms,
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
const closeDialog = () => {
  openDialog.value = false;
};
const nextPage = (event) => {
  options.value.PageNo = event.page;
  options.value.PageSize = event.rows;
  if (event.rows != options.value.PageSize) {
    options.value.PageSize = event.rows;
  }
  loadData(true);
};
const refresh = () => {
  first.value = 0;
  options.value = {
    PageNo: 0,
    PageSize: 18,
  };
  loadData(true);
  files = [];
};
const DeleteItem = (id) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá ảnh động này không ?",
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
          .delete(baseURL + "/api/Birthday_Gif/Delete_Gif", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: id != null ? [id.gif_id] : 0,
          })
          .then((response) => {
            swal.close();
            if (response.data.err == "0") {
              swal.close();
              toast.success("Xoá ảnh động thành công!");
              if (
                (options.value.totalRecords - id.length) % 2 == 0 &&
                options.value.PageNo > 0
              ) {
                options.value.PageNo = options.value.PageNo - 1;
              }
              loadData(true);
            } else {
              swal.fire({
                title: "Lỗi",
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
                title: "Thông báo",
                text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
const Zoom = (data) => {
  images.value = [];
  headerDialog2.value = "Phóng to";
  openDialog2.value = true;
  images.value.push(data.data);
};
onMounted(() => {
  loadData(true);
});
</script>
<template>
  <!-- //View -->
  <div class="main-layout true flex-grow-1 p-2">
    <DataView
      :value="datalists"
      :layout="layout"
      :paginator="options.totalRecords > options.PageSize"
      :rows="options.PageSize"
      @page="nextPage($event)"
      :totalRecords="options.totalRecords"
      :lazy="true"
      :loading="options.loading"
      :pageLinkSize="options.PageSize"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink"
      responsiveLayout="scroll"
      v-model:first="first"
      :rowHover="true"
    >
      <template #header>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <h3 class="module-title mt-0 ml-1 mb-2">
              <i class="pi pi-book"> </i>
              Danh sách ảnh động ({{ options.totalRecords }})
            </h3>
          </template>

          <template #end>
            <Button
              @click="AddNewValue('Thêm ảnh động mới')"
              label="Thêm mới"
              icon="pi pi-plus"
              class="mr-2"
              v-if="store.state.user.is_super"
            />
            <Button
              @click="refresh()"
              class="mr-2 p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              v-tooltip="'Tải lại'"
            />
          </template>
        </Toolbar>
      </template>
      <template #empty>
        <div
          class="align-items-center justify-content-center p-4 text-center m-auto"
          v-if="!isFirst"
        >
          <img
            src="../../assets/background/nodata.png"
            height="144"
          />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
      <template #grid="data">
        <div class="col-12 md:col-2">
          <div class="product-grid-item">
            <div class="product-grid-item-top"></div>
            <div class="product-grid-item-content">
              <img
                style="object-fit: contain; border: unset; outline: unset"
                alt=" "
                :style="
                  width < 1900
                    ? 'width: 10rem; height: 10rem'
                    : 'width: 14rem; height: 14rem'
                "
                v-bind:src="
                  data.data.gif
                    ? basedomainURL + data.data.gif
                    : basedomainURL + '/Portals/Image/noimg.jpg'
                "
              />
              <button
                class="mybutton"
                v-if="store.state.user.is_super"
                @click="DeleteItem(data.data)"
              >
                <i class="fp pi pi-trash">11</i>
              </button>
              <button
                :class="store.state.user.is_super ? 'mybutton2' : 'myclass3'"
                @click="Zoom(data)"
              >
                <i class="fp pi pi-search-plus"></i>
              </button>
            </div>
            <div class="product-grid-item-bottom"></div>
          </div>
        </div>
      </template>
    </DataView>
  </div>
  <!-- //Dialog -->
  <!-- <Dialog
    :header="headerDialog2"
    v-model:visible="openDialog2"
    style="min-width: 50vw"
    :closable="true"
    :maximizable="true"
  > -->
  <Galleria
    :value="images"
    container-style="max-height: 70rem"
    :showThumbnails="false"
    :responsiveOptions="responsiveOptions"
    :fullScreen="true"
    v-model:visible="openDialog2"
  >
    <template #item="data">
      <div class="product-grid-item-content">
        <img
          style="
            object-fit: contain;
            border: unset;
            outline: unset;
            max-height: 99vh;
          "
          v-bind:src="
            data.item.gif
              ? basedomainURL + data.item.gif
              : basedomainURL + '/Portals/Image/noimg.jpg'
          "
        />
      </div>
    </template>
  </Galleria>
  <!-- </Dialog> -->
  <Dialog
    :header="headerDialog"
    v-model:visible="openDialog"
    style="width: 30vw"
    :closable="false"
  >
    <form>
      <FileUpload
        accept=".gif"
        @select="selectFile"
        @remove="removeFile"
        :show-upload-button="false"
        choose-label="Chọn tệp"
        cancel-label="Hủy"
        :fileLimit="1000"
        :multiple="true"
        :invalidFileTypeMessage="'Chỉ chấp nhận file dạng .gif'"
      >
        <template #empty>
          <p>Kéo và thả tệp vào đây để tải lên.</p>
        </template>
      </FileUpload>
    </form>
    <template #footer>
      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="Upload"
      />
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog"
      />
    </template>
  </Dialog>
</template>
<style scoped>
.p-galleria-content {
  max-height: 50rem;
}
.p-galleria-item-wrapper {
  max-height: 50rem;
}
.product-grid-item-content {
  position: relative;
  color: white;
}
.fp {
  font-size: 175%;
}
.mybutton {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-0%, -50%);
  background-color: rgba(255, 255, 255, 0.5);
  color: red;
  padding: 1rem;
  margin-left: 0.325rem;
  border-radius: 50%;
  border-color: #2196f3;
  cursor: pointer;
  display: none;
  font-size: xx-small;
}
.mybutton2 {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-100%, -50%);
  background-color: rgba(255, 255, 255, 0.5);
  color: #2196f3;
  padding: 1rem;
  border-radius: 50%;
  border-color: #2196f3;
  cursor: pointer;
  display: none;
  font-size: xx-small;
}
.myclass3 {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  background-color: rgba(255, 255, 255, 0.5);
  color: #2196f3;
  padding: 1rem;
  border-radius: 50%;
  border-color: #2196f3;
  cursor: pointer;
  display: none;
  font-size: xx-small;
}
.product-grid-item-content:hover .mybutton {
  display: block;
}
.product-grid-item-content:hover .mybutton2 {
  display: block;
}
.product-grid-item-content:hover .myclass3 {
  display: block;
}
@media only screen and (max-width: 1920px) {
  .p-dataview .p-dataview-content {
    min-height: 81vh !important;
  }
}
@media only screen and (max-width: 1420px) {
  .p-dataview .p-dataview-content {
    min-height: 45vh !important;
    max-width: 99vw;
  }
}
</style>
<style lang="scss" scoped>
.card {
  background: #ffffff;
  padding: 0.5rem;
  box-shadow: 0 2px 1px -1px rgba(0, 0, 0, 0.2), 0 1px 1px 0 rgba(0, 0, 0, 0.14),
    0 1px 3px 0 rgba(0, 0, 0, 0.12);
  border-radius: 4px;
  margin-bottom: 0rem;
}

::v-deep(.product-list-item) {
  display: flex;
  align-items: center;
  width: 100%;
  img {
    box-shadow: 0 3px 6px rgba(0, 0, 0, 0.16), 0 3px 6px rgba(0, 0, 0, 0.23);
    margin-right: 0rem;
  }
}

::v-deep(.product-grid-item) {
  margin: 0.5rem;
  border: 1px solid var(--surface-border);

  .product-grid-item-top,
  .product-grid-item-bottom {
    display: flex;
    align-items: center;
    justify-content: space-between;
  }

  img {
    box-shadow: 0 3px 6px rgba(0, 0, 0, 0.16), 0 3px 6px rgba(0, 0, 0, 0.23);
    margin: 1.5rem 0;
  }

  .product-grid-item-content {
    text-align: center;
  }
}
@media only screen and (max-width: 1420px) {
  .card {
    background: #ffffff;
    padding: 0.5rem;
    box-shadow: 0 2px 1px -1px rgba(0, 0, 0, 0.2),
      0 1px 1px 0 rgba(0, 0, 0, 0.14), 0 1px 3px 0 rgba(0, 0, 0, 0.12);
    border-radius: 4px;
    margin-bottom: 0rem;
  }

  ::v-deep(.product-list-item) {
    display: flex;
    align-items: center;
    width: 100%;
    img {
      box-shadow: 0 3px 6px rgba(0, 0, 0, 0.16), 0 3px 6px rgba(0, 0, 0, 0.23);
      margin-right: 0rem;
    }
  }

  ::v-deep(.product-grid-item) {
    margin: 0.5rem;
    border: 1px solid var(--surface-border);

    .product-grid-item-top,
    .product-grid-item-bottom {
      display: flex;
      align-items: center;
      justify-content: space-between;
    }

    img {
      box-shadow: 0 3px 6px rgba(0, 0, 0, 0.16), 0 3px 6px rgba(0, 0, 0, 0.23);
      margin: 0rem 0;
    }

    .product-grid-item-content {
      text-align: center;
    }
  }
}
</style>
