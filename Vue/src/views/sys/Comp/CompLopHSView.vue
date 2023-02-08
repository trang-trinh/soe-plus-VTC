<script setup>
import { ref, defineProps, inject, onMounted, onUpdated } from "vue";
import { useToast } from "vue-toastification";
import moment from "moment";
const props = defineProps({
  lophoc: Object,
  tudiens: Array,
  reloadComponnent: Function,
});
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
//Valid Form
const monhoc = ref({
  LopUser_ID: -1,
  Lophoc_ID: props.lophoc.Lophoc_ID,
  Namhoc_ID: store.getters.namhoc.Namhoc_ID,
  STT: 1,
  IsType: 4,
  Trangthai: 1,
});
const monhoclops = ref([]);
const mlop = ref({
  LopUser_ID: -1,
  Lophoc_ID: props.lophoc.Lophoc_ID,
  Namhoc_ID: store.getters.namhoc.Namhoc_ID,
  STT: 1,
  IsType: 4,
  Trangthai: 1,
});
const opAddMon = ref();
const tdTrangthais = [
  { value: 0, text: "Đã khoá" },
  { value: 1, text: "Bình thường" },
  { value: 2, text: "Đang đợi xác nhận" },
  { value: 3, text: "Kết thúc năm học" },
];
const tdTypes = [
  { value: 1, text: "Giảng dạy" },
  { value: 2, text: "Trợ giảng" },
  { value: 3, text: "Người quản lý" },
];
const submitted = ref(false);
//Khai báo biến
const basedomainURL = fileURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const toast = useToast();
const isFirst = ref(true);
const loadLophocsinh = () => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "HocsinhListByClass",
        par: [
          { par: "user_id ", va: store.getters.user.user_id },
          { par: "Lophoc_ID ", va: props.lophoc.Lophoc_ID },
          { par: "Namhoc_ID ", va: store.getters.namhoc.Namhoc_ID },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        data[0].forEach((item, i) => {
          item.STT = i + 1;
          item.Ngay = moment(new Date(item.Ngaycapnhat)).format("DD/MM/YYYY HH:mm:ss");
        });
        monhoclops.value = data[0];
      } else {
        monhoclops.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      let sm = [...new Set(monhoclops.value.map((x) => x.user_id))].length;
      props.reloadComponnent({
        Lophoc_ID: props.lophoc.Lophoc_ID,
        Sohs: sm,
      });
    })
    .catch((error) => {});
};
const toggleAddmon = (event) => {
  opAddMon.value.toggle(event);
};
const addGVMon = (data) => {
  mlop.value = { Users: [{ user_id: data.user_id }] };
  saveMon();
};
const saveMon = () => {
  opAddMon.value.hide();
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let mons = [];
  if (mlop.value.Users) {
    mlop.value.Users.forEach((m) => {
      mons.push({
        LopUser_ID: -1,
        Namhoc_ID: store.getters.namhoc.Namhoc_ID,
        Lophoc_ID: props.lophoc.Lophoc_ID,
        user_id: m.user_id,
        Bosach_ID: mlop.value.Bosach_ID,
        Trangthai: mlop.value.Trangthai,
        IsType: mlop.value.IsType,
        STT: mons.length + 1,
        Monhoc_ID: mlop.value.Monhoc_ID,
      });
    });
  }
  axios
    .post(baseURL + "/api/Lophoc/Add_LophocUser", mons, config)
    .then((response) => {
      loadLophocsinh();
      mlop.value.Khoihocs = [];
      swal.close();
    })
    .catch((error) => {
      swal.close();
    });
};
const saveTableMon = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let mons = [];
  if (monhoclops.value.length > 0) {
    monhoclops.value.forEach((m) => {
      mons.push({
        LopUser_ID: m.LopUser_ID,
        Lophoc_ID: props.lophoc.Lophoc_ID,
        Monhoc_ID: m.Monhoc_ID,
        user_id: m.user_id,
        Bosach_ID: m.Bosach_ID,
        IsType: m.IsType,
        Trangthai: m.Trangthai,
      });
    });
  }
  axios
    .put(baseURL + "/api/Lophoc/Update_LophocUser", mons, config)
    .then((response) => {
      loadLophocsinh();
      mlop.value.Khoihocs = [];
      swal.close();
    })
    .catch((error) => {
      swal.close();
    });
};
const delMonhocLophoc = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá môn học này không!",
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
          .delete(baseURL + "/api/Lophoc/Del_LophocUser", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: [md.LopUser_ID],
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá Môn học thành công!");
              loadLophocsinh();
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
          });
      }
    });
};
//Export Excel
const menuButs = ref();
const itemButs = ref([
  {
    label: "Import Excel",
    icon: "pi pi-upload",
    command: () => {
      importHocSinh();
    },
  },
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: () => {
      exportLophoc("Sys_LopUser_ListExport");
    },
  },
  {
    label: "Xuất Mẫu",
    icon: "pi pi-file-excel",
    command: () => {
      exportLophoc("Sys_LopUser_ListExportMau");
    },
  },
]);
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
//Import
const filelist = ref([]);
const file = ref();
//Upload File
const onChange = () => {
  let len = file.value.files.length;
  let arr = [...filelist.value];
  for (let i = 0; i < len; i++) {
    arr.push(file.value.files[i]);
  }
  filelist.value = arr;
};
const removeFile = (i) => {
  filelist.value.splice(i, 1);
};
const drop = (event) => {
  event.preventDefault();
  file.value.files = event.dataTransfer.files;
  onChange(); // Trigger the onChange event manually
};
const showImportExcel = ref(false);
const importHocSinh = () => {
  showImportExcel.value = true;
  filelist.value = [];
  file.value = null;
};
const ImportExcel = () => {
  if (filelist.value.length == 0) {
    swal.fire({
      title: "Error!",
      text: "Vui lòng chọn file để import",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let formdata = new FormData();
  for (var i = 0; i < filelist.value.length; i++) {
    let file = filelist.value[i];
    formdata.append("execel", file);
  }
  formdata.append("Role_ID", "hocsinh");
  formdata.append("model", JSON.stringify(monhoc.value));
  axios
    .post(baseURL + "/api/Lophoc/ImportHocSinh", formdata, config)
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Import học sinh thành công!");
        showImportExcel.value = false;
        loadLophocsinh();
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
const exportLophoc = (proc) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let par = [
    { par: "user_id", va: store.getters.user.user_id },
    { par: "Lophoc_ID", va: props.lophoc.Lophoc_ID },
    { par: "Namhoc_ID", va: store.getters.namhoc.Namhoc_ID },
  ];
  axios
    .post(
      baseURL + "/api/Excel/ExportExcel",
      {
        excelname: "DANH SÁCH HỌC SINH",
        proc: proc,
        par: par,
      },
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Kết xuất Data thành công!");
        if (response.data.path != null) {
          let pathReplace = response.data.path.replace(/\\+/g, '/').replace(/\/+/g, '/').replace(/^\//g, '');
          var listPath = pathReplace.split('/');
          var pathFile = "";
          listPath.forEach(item => {
            if (item.trim() != "")
            {
                pathFile += "/" + item;
            }
          });
          //window.open(baseURL + response.data.path);
          window.open(baseURL + pathFile);
        }
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
onMounted(() => {
  loadLophocsinh();
  return {};
});
onUpdated(() => {
  //console.log(props.lophoc);
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2'" style="min-height: 450px !important">
    <DataTable
      class="w-full p-datatable-sm e-sm"
      :value="monhoclops"
      dataKey="LopUser_ID"
      :showGridlines="true"
      :rowHover="true"
      currentPageReportTemplate=""
      responsiveLayout="scroll"
      :scrollable="true"
      scrollHeight="flex"
    >
      <template #header>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <Button
              v-if="props.lophoc.IsPermision && monhoclops.length > 0"
              label="Cập nhật"
              icon="pi pi-save"
              @click="saveTableMon()"
            />
          </template>
          <template #end>
            <Button
              class="mr-2 p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              @click="loadLophocsinh()"
            />
            <Button
              label="Export"
              icon="pi pi-file-excel"
              class="mr-2 p-button-outlined p-button-secondary"
              @click="toggleExport"
              aria-haspopup="true"
              aria-controls="overlay_Export"
            />
            <Menu id="overlay_Export" ref="menuButs" :model="itemButs" :popup="true" />
            <Button
              v-if="props.lophoc.IsPermision"
              label="Thêm học sinh"
              icon="pi pi-plus"
              class="mr-2"
              @click="toggleAddmon"
              aria:haspopup="true"
              aria-controls="overlay_panel"
            />
            <OverlayPanel
              ref="opAddMon"
              appendTo="body"
              :showCloseIcon="true"
              id="overlay_panel"
              style="width: 450px"
              :breakpoints="{ '960px': '75vw' }"
            >
              <div class="field col-12 md:col-12">
                <label class="col-3 text-left">Học sinh</label>
                <MultiSelect
                  :virtualScrollerOptions="{ itemSize: 48 }"
                  :options="tudiens[3]"
                  v-model="mlop.Users"
                  optionLabel="full_name"
                  placeholder="Chọn học sinh"
                  class="col-9"
                  :popup="true"
                  :filter="true"
                  :showClear="true"
                >
                  <template #value="slotProps">
                    <div
                      class="user-item user-item-value"
                      v-if="slotProps.value && slotProps.value.length > 0"
                      v-for="u in slotProps.value"
                      :key="u.user_id"
                    >
                      <Avatar
                        v-bind:label="u.Avartar ? '' : u.full_name.substring(0, 1)"
                        v-bind:image="basedomainURL + u.Avartar"
                        style="background-color: #2196f3; color: #ffffff"
                        class="mr-2"
                        shape="circle"
                      />
                      <div>{{ u.full_name }}</div>
                    </div>
                    <span v-else>
                      {{ slotProps.placeholder }}
                    </span>
                  </template>
                  <template #option="slotProps">
                    <div class="user-item">
                      <Avatar
                        v-bind:label="
                          slotProps.option.Avartar
                            ? ''
                            : slotProps.option.full_name.substring(0, 1)
                        "
                        v-bind:image="basedomainURL + slotProps.option.Avartar"
                        style="background-color: #2196f3; color: #ffffff"
                        class="mr-2"
                        shape="circle"
                      />
                      <div>
                        {{ slotProps.option.full_name
                        }}<i class="gvpb">{{ slotProps.option.organization_name }}</i>
                      </div>
                    </div>
                  </template>
                </MultiSelect>
              </div>
              <div
                class="field col-12 md:col-12 align-items-center justify-content-center text-center"
              >
                <Button
                  label="Cập nhật"
                  icon="pi pi-plus"
                  class="m-auto"
                  @click="saveMon"
                />
              </div>
            </OverlayPanel>
          </template>
        </Toolbar>
      </template>
      <Column
        field="STT"
        header="STT"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:80px"
        bodyStyle="text-align:center;max-width:80px"
      ></Column>
      <Column field="full_name" header="Học sinh">
        <template #body="md">
          <Avatar
            v-bind:label="md.data.Avartar ? '' : md.data.full_name.substring(0, 1)"
            v-bind:image="basedomainURL + md.data.Avartar"
            style="background-color: #2196f3; color: #ffffff"
            class="mr-2"
            shape="circle"
          />
          <div>{{ md.data.full_name }}</div>
        </template>
      </Column>
      <Column
        field="Trangthai"
        header="Trạng thái"
        headerStyle="max-width:250px"
        bodyStyle="max-width:250px"
      >
        <template #body="md">
          <Dropdown
            v-model="md.data.Trangthai"
            :options="tdTrangthais"
            class="w-full"
            optionLabel="text"
            optionValue="value"
          />
        </template>
      </Column>
      <Column
        field="Ngay"
        header="Ngày thêm"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:180px"
        bodyStyle="text-align:center;max-width:180px"
      ></Column>
      <Column
        v-if="props.lophoc.IsPermision"
        headerClass="text-center"
        bodyClass="text-center"
        headerStyle="text-align:center;max-width:60px"
        bodyStyle="text-align:center;max-width:60px"
      >
        <template #header> </template>
        <template #body="md">
          <Button
            type="button"
            icon="pi pi-trash"
            class="p-button-rounded p-button-sm p-button-danger"
            @click="delMonhocLophoc(md.data)"
          ></Button>
        </template>
      </Column>
      <template #empty>
        <div
          class="m-auto align-items-center justify-content-center p-4 text-center"
          v-if="!isFirst"
        >
          <img src="../../../assets/background/nodata.png" height="144" />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </DataTable>
  </div>
  <Dialog
    header="Import học sinh"
    v-model:visible="showImportExcel"
    :style="{ width: '480px', zIndex: 1000 }"
    :autoZIndex="false"
    :modal="true"
  >
    <div class="flex items-center justify-center text-center">
      <div class="dragFile" @dragover="dragover" @dragleave="dragleave" @drop="drop">
        <input
          type="file"
          multiple
          name="fields[assetsFieldHandle][]"
          id="assetsFieldHandle"
          class="w-px h-px opacity-0 overflow-hidden absolute"
          @change="onChange"
          ref="file"
          accept=".xls,.xlsx,.jpg,.jpeg,.png"
        />
        <i
          class="pi pi-cloud-upload"
          style="font-size: 4rem; margin: 10px; color: #aaa"
        ></i>
        <label for="assetsFieldHandle" class="block cursor-pointer">
          <div>
            Kéo file hoặc
            <span class="underline" style="color: #0d89ec">click vào đây</span> để upload
            File
          </div>
        </label>
      </div>
    </div>
    <div>
      <div v-for="f in filelist" class="mt-3 mb-2 flex divfile">
        <Image
          v-bind:src="
            'src/assets/image/file/' +
            f.name.substring(f.name.lastIndexOf('.') + 1) +
            '.png'
          "
          height="16"
        />
        <span class="ml-1">{{ f.name }}</span>
        <Button
          icon="pi pi-times"
          @click="removeFile($index)"
          class="p-button-text p-button-secondary"
        />
      </div>
    </div>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="showImportExcel = false"
        class="p-button-raised p-button-secondary"
      />
      <Button label="Thực hiện" icon="pi pi-upload" @click="ImportExcel" />
    </template>
  </Dialog>
</template>
<style scoped>
.gvpb {
  display: block;
  color: #607d8b;
  font-size: 13px;
}
.divfile {
  background-color: #eee;
  padding: 5px;
}
</style>
