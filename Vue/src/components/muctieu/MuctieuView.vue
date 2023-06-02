<script setup>
import { ref, defineProps, inject, onMounted } from "vue";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import moment from "moment";
import TrangthaiDuanView from "../duan/TrangthaiDuan.vue";
import TrangthaiCongviec from "../congviec/TrangthaiCongviec.vue";
import ModalCongviec from "../congviec/ModalCongviec.vue";
import MuctieuBox from "./MuctieuBox.vue";
import MuctieuItemListBox from "./MuctieuItemListBox.vue";
const emitter = inject("emitter");
emitter.on("duan", (obj) => {
  switch (obj.type) {
    case "closedisplayAddCongviec":
      displayAddCongViec.value = false;
      break;
    case "loadcongviec":
      loadMuctieu(true);
      break;
  }
});
const props = defineProps({
  Duan_ID: String,
});
const store = inject("store");
const swal = inject("$swal");
const router = inject("router");
const axios = inject("axios"); // inject axios
const basedomainURL = fileURL;
//Valid Form
const muctieu = ref({
  Duan_ID: props.Duan_ID,
  Muctieu_ID: "",
  Muctieu_Ten: "",
  STT: 1,
  Trangthai: 1,
});
const submitted = ref(false);
const rules = {
  Muctieu_Ten: {
    required,
  },
};
const v$ = useVuelidate(rules, muctieu);
//Khai báo biến
const Congviec_ID = ref();
const displayTrangthaiCV = ref(false);
const Congviec = ref({});
const isAdd = ref(true);
const selectedKey = ref();
const selectedNodes = ref([]);
const filters = ref({});
const opition = ref({
  search: "",
  PageNo: 1,
  PageSize: 100000,
  user_id: store.getters.user.user_id,
  Sort: "Ngaytao",
  Order: "asc",
  Type: 0,
});
const muctieus = ref([]);
const modeldate = ref({});
const displayAdd = ref(false);
const displayAddCongViec = ref(false);
const isFirst = ref(true);
let files = {};
const toast = useToast();
const layout = ref("grid");
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const tdTrangthaiCVs = [
  { value: 0, text: "Đang lập kế hoạch" },
  { value: 1, text: "Đang thực hiện" },
  { value: 2, text: "Đã hoàn thành" },
  { value: 3, text: "Chờ xác nhận" },
  { value: 4, text: "Hoàn thành sau hạn" },
  { value: 5, text: "Tạm dừng" },
  { value: 6, text: "Đóng" },
];
const tdTrangthais = ref([
  { value: 0, text: "Đang lập kế hoạch" },
  { value: 1, text: "Đang thực hiện" },
  { value: 2, text: "Đã hoàn thành" },
  { value: 3, text: "Tạm dừng" },
  { value: 4, text: "Đóng" },
]);
const tdUsers = ref([]);
const tdTukhoas = ref([]);
const menuButs = ref();
const menuButMores = ref();
const itemButs = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportMuctieu("ExportExcel");
    },
  },
  {
    label: "Xuất Mẫu",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportMuctieu("ExportExcelMau");
    },
  },
]);
const itemButMores = ref([
  {
    label: "Thêm công việc",
    icon: "pi pi-plus-circle",
    command: (event) => {
      addTask(mucieu.value);
    },
  },
  {
    label: "Sửa mục tiêu",
    icon: "pi pi-pencil",
    command: (event) => {
      editMuctieu(mucieu.value);
    },
  },
  {
    label: "Xoá mục tiêu",
    icon: "pi pi-trash",
    command: (event) => {
      delMuctieu(mucieu.value);
    },
  },
]);
//Khai báo function
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const toggleMores = (event, u) => {
  muctieu.value = u;
  menuButMores.value.toggle(event);
};
const onNodeSelect = (node) => {
  selectedNodes.value.push(node.data.Muctieu_ID);
};
const onNodeUnselect = (node) => {
  selectedNodes.value.splice(selectedNodes.value.indexOf(node.data.Muctieu_ID), 1);
};
const handleFileUpload = (event, ianh) => {
  files[ianh] = event.target.files[0];
  var output = document.getElementById(ianh);
  output.src = URL.createObjectURL(event.target.files[0]);
  output.onload = function () {
    URL.revokeObjectURL(output.src); // free memory
  };
};
//Show Modal
const showModalAdd = () => {
  submitted.value = false;
  muctieu.value = {
    Duan_ID: props.Duan_ID,
    Muctieu_Ten: "",
    STT: muctieus.value.length + 1,
    Trangthai: 1,
  };
  modeldate.value = {};
  isAdd.value = true;
  displayAdd.value = true;
};
const chonanh = (id) => {
  document.getElementById("ip" + id).click();
};
const closedisplayAdd = () => {
  displayAdd.value = false;
};
//Thêm sửa xoá
const onRefersh = () => {
  opition.value.search = "";
  opition.value.PageNo = 1;
  opition.value.PageSize = 20;
  loadMuctieu(true);
};
const onSearch = () => {
  loadMuctieu(true);
};
const initTudien = () => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "Plan_Duan_ListTudien",
        par: [{ par: "Duan_ID", va: opition.value.Duan_ID }],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        tdUsers.value = data[0];
        tdTukhoas.value = data[1];
      }
    })
    .catch((error) => {});
};
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "Plan_Muctieu_Count",
        par: [
          { par: "Duan_ID", va: props.Duan_ID },
          { par: "Search", va: opition.value.search },
          { par: "Tukhoa", va: opition.value.Tukhoa },
          { par: "user_id", va: opition.value.user_id },
          { par: "Trangthai", va: opition.value.Trangthai },
          { par: "StartDate", va: opition.value.StartDate },
          { par: "EndDate", va: opition.value.EndDate },
          { par: "Type ", va: opition.value.Type },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        opition.value.totalRecords = data[0].totalRecords;
        let countMuctieu = opition.value.totalRecords;
        //Cập nhật Count ở dự án
        emitter.emit("duan", {
          type: "refershCount",
          data: { Muctieu: countMuctieu },
        });
      }
    })
    .catch((error) => {});
};
const onPage = (event) => {
  opition.value.PageNo = event.page + 1;
  opition.value.PageSize = event.rows;
  loadMuctieu(true);
};
const formatDate = (d, fm) => {
  if (!(d instanceof Date)) {
    d = new Date(d.replace("AM", "").replace("PM", ""));
  }
  return moment(d).format(fm || "DD/MM/YYYY HH:mm:ss");
};
const loadMuctieu = (rf) => {
  if (rf) {
    opition.value.loading = true;
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
    if (opition.value.PageNo == 1) loadCount();
  }
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "Plan_Muctieu_List",
        par: [
          { par: "Duan_ID", va: props.Duan_ID },
          { par: "Search", va: opition.value.search },
          { par: "Tukhoa", va: opition.value.Tukhoa },
          { par: "user_id", va: opition.value.user_id },
          { par: "PageNo", va: opition.value.PageNo },
          { par: "PageSize", va: opition.value.PageSize },
          { par: "Trangthai", va: opition.value.Trangthai },
          { par: "StartDate", va: opition.value.StartDate },
          { par: "EndDate", va: opition.value.EndDate },
          { par: "Sort", va: opition.value.Sort },
          { par: "Order", va: opition.value.Order },
          { par: "Type ", va: opition.value.Type },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        data.forEach(function (r) {
          if (r.Ngaytao) r.NgaytaoS = formatDate(r.Ngaytao);
          if (r.NgayBD) r.NgayBDS = formatDate(r.NgayBD);
          if (r.NgayKT) r.NgayKTS = formatDate(r.NgayKT);
          if (r.NgayKT) r.NgayKTD = formatDate(r.NgayKT, "DD/MM/YYYY");
          if (r.NgayTH) r.NgayTHS = formatDate(r.NgayTH);
          if (r.NgayHT) r.NgayHTS = formatDate(r.NgayHT);
          if (r.Users) {
            r.Users = JSON.parse(r.Users);
          }
          if (r.Congviecs) {
            r.Congviecs = JSON.parse(r.Congviecs);
            r.Congviecs.forEach(function (r1) {
              if (r1.Ngaytao) r1.NgaytaoS = formatDate(r1.Ngaytao);
              if (r1.NgayBD) r1.NgayBDS = formatDate(r1.NgayBD);
              if (r1.NgayKT) r1.NgayKTS = formatDate(r1.NgayKT);
              if (r1.NgayKT) r1.NgayKTD = formatDate(r1.NgayKT, "DD/MM/YYYY");
              if (r1.NgayTH) r1.NgayTHS = formatDate(r1.NgayTH);
              if (r1.NgayHT) r1.NgayHTS = formatDate(r1.NgayHT);
            });
          }
        });
        muctieus.value = data;
      } else {
        muctieus.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      if (rf) {
        opition.value.loading = false;
        swal.close();
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const goMuctieu = (md) => {
  router.push({ name: "projectdetail", params: { duanid: md.Duan_ID } });
};
const editMuctieu = (md) => {
  submitted.value = false;
  isAdd.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  displayAdd.value = true;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "Plan_Muctieu_Get",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "Muctieu_ID", va: md.Muctieu_ID },
        ],
      },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        let obj = data[0][0];
        if (obj.Tukhoa) obj.Tukhoa = obj.Tukhoa.split(",");
        modeldate.value.thoigiankehoach = [];
        if (obj.NgayBD) {
          modeldate.value.thoigiankehoach.push(new Date(obj.NgayBD));
        }
        if (obj.NgayKT) {
          modeldate.value.thoigiankehoach.push(new Date(obj.NgayKT));
        }
        if (modeldate.value.thoigiankehoach.length == 0) {
          modeldate.value.thoigiankehoach = null;
        }
        muctieu.value = obj;
        //
        modeldate.value.thuchiens = data[1]
          .filter((x) => x.TypeUser == 1)
          .map((x) => ({
            user_id: x.user_id,
            full_name: x.full_name,
            Avartar: x.Avartar,
          }));
        modeldate.value.quanlys = data[1]
          .filter((x) => x.TypeUser == 2)
          .map((x) => ({
            user_id: x.user_id,
            full_name: x.full_name,
            Avartar: x.Avartar,
          }));
        modeldate.value.theodoi = data[1]
          .filter((x) => x.TypeUser == 0)
          .map((x) => ({
            user_id: x.user_id,
            full_name: x.full_name,
            Avartar: x.Avartar,
          }));
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const handleSubmit = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    //return;
  }
  addMuctieu();
};
const addMuctieu = () => {
  let data = {};
  let md = { ...muctieu.value };
  if (md.Tukhoa instanceof Array) {
    md.Tukhoa = md.Tukhoa.join(",");
  }
  if (modeldate.value.thoigiankehoach) {
    if (modeldate.value.thoigiankehoach.length > 0) {
      md.NgayBD = modeldate.value.thoigiankehoach[0];
    }
    if (modeldate.value.thoigiankehoach.length > 1) {
      md.NgayKT = modeldate.value.thoigiankehoach[1];
    }
  }
  let users = [];
  if (modeldate.value.thuchiens) {
    modeldate.value.thuchiens.forEach((u) => {
      users.push({ user_id: u.user_id, TypeUser: 1, Trangthai: true, IsCheck: false });
    });
  }
  if (modeldate.value.quanlys) {
    modeldate.value.quanlys.forEach((u) => {
      users.push({ user_id: u.user_id, TypeUser: 2, Trangthai: true, IsCheck: false });
    });
  }
  if (modeldate.value.theodoi) {
    modeldate.value.theodoi.forEach((u) => {
      users.push({ user_id: u.user_id, TypeUser: 0, Trangthai: true, IsCheck: false });
    });
  }
  if (users.length > 0) {
    //formData.append("users", JSON.stringify(users));
    data.users = users;
  }
  data.model = md;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: isAdd.value == false ? "put" : "post",
    url:
      baseURL + `/api/Muctieu/${isAdd.value == false ? "Update_Muctieu" : "Add_Muctieu"}`,
    data: data,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật mục tiêu thành công!");
        loadMuctieu();
        closedisplayAdd();
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
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};

const delMuctieu = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá mục tiêu này không!",
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
          .delete(baseURL + "/api/Muctieu/Del_Muctieu", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: md != null ? [md.Muctieu_ID] : selectedNodes.value,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá mục tiêu thành công!");
              loadMuctieu();
              if (!md) selectedNodes.value = [];
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
                text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};

const exportMuctieu = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/Excel/" + method,
      {
        excelname: "DANH SÁCH MỤC TIÊU",
        proc: "Plan_Muctieu_ListExport",
        par: [
          { par: "Search", va: opition.value.search },
          { par: "Duan_ID", va: muctieu.Duan_ID },
          { par: "Trangthai", va: opition.value.Trangthai },
          { par: "StartDate", va: opition.value.StartDate },
          { par: "EndDate", va: opition.value.EndDate },
        ],
      },
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Kết xuất Data thành công!");
        window.open(baseURL + response.data.path);
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
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
//Add Công việc
const addTask = (md) => {
  muctieu.value = md;
  Congviec_ID.value = null;
  displayAddCongViec.value = true;
};
const editCongviec = (md) => {
  Congviec_ID.value = md.Congviec_ID;
  displayAddCongViec.value = true;
};
const delCongviec = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá công việc này không!",
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
          .delete(baseURL + "/api/Congviec/Del_Congviec", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: md != null ? [md.Congviec_ID] : selectedNodes.value,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá công việc thành công!");
              loadCongviec(true);
              if (!md) selectedNodes.value = [];
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
                text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};

const checkQuyenCV = (md) => {
  return (
    store.getters.user.user_id == md.Nguoitao ||
    md.Users.findIndex(
      (x) => x.user_id == store.getters.user.user_id && x.TypeUser > 0
    ) != -1
  );
};

const changeTrangthaiCV = (md) => {
  if (checkQuyenCV(md)) {
    Congviec.value = md;
    displayTrangthaiCV.value = true;
  }
};
const closedisplayTrangthaiCV = () => {
  displayTrangthaiCV.value = false;
};
const updateTrangthaiCV = (tt) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn cập nhật trạng thái cho công việc này không!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then((result) => {
      if (result.isConfirmed) {
        displayTrangthaiCV.value = false;
        swal.fire({
          width: 110,
          didOpen: () => {
            swal.showLoading();
          },
        });
        axios({
          method: "put",
          url: baseURL + "/api/Congviec/Update_TrangthaiCongviec",
          data: { ids: [Congviec.value.Congviec_ID], tts: [tt.value] },
          headers: {
            Authorization: `Bearer ${store.getters.token}`,
          },
        })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Cập nhật công việc thành công!");
              loadMuctieu(true);
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
                text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
//End Công việc
onMounted(() => {
  //init
  loadMuctieu();
  loadCount();
  initTudien();
  return {
    displayAdd,
    isFirst,
    opition,
    showModalAdd,
    closedisplayAdd,
    addMuctieu,
    editMuctieu,
    onSearch,
    delMuctieu,
    handleFileUpload,
    chonanh,
    v$,
    handleSubmit,
    submitted,
    basedomainURL,
    filters,
    onRefersh,
    itemButs,
    menuButs,
    toggleExport,
    selectedKey,
    onNodeSelect,
    onNodeUnselect,
    selectedNodes,
    isAdd,
    layout,
    toggleMores,
    itemButMores,
    menuButMores,
    onPage,
    tdTukhoas,
  };
});
</script>
<template>
  <div
    style="background-color: #eee"
    class="flex-grow-1 h-full p-2 flex flex-column"
    v-if="store.getters.islogin"
  >
    <Toolbar class="w-full custoolbar">
      <template #start>
        <Dropdown
          :showClear="true"
          v-model="opition.Trangthai"
          :options="tdTrangthais"
          optionLabel="text"
          optionValue="value"
          placeholder="Lọc theo trạng thái"
          @change="loadMuctieu(true)"
        />
        <span class="p-input-icon-left ml-2">
          <i class="pi pi-search" />
          <InputText
            type="text"
            spellcheck="false"
            v-model="opition.search"
            placeholder="Tìm kiếm"
            v-on:keyup.enter="onSearch"
          />
        </span>
      </template>

      <template #end>
        <DataViewLayoutOptions v-model="layout" />
        <Button
          class="mr-2 ml-2 p-button-outlined p-button-secondary"
          icon="pi pi-refresh"
          @click="onRefersh"
        />
        <Button
          label="Xoá"
          icon="pi pi-trash"
          class="mr-2 p-button-danger"
          v-if="selectedNodes.length > 0"
          @click="delDuan"
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
          label="Thêm mục tiêu"
          icon="pi pi-plus"
          class="mr-2"
          @click="showModalAdd"
        />
      </template>
    </Toolbar>

    <div
      v-if="layout == 'grid' && muctieus.length > 0"
      class="w-full overflow-x-auto flex-grow-1"
    >
      <div class="ovegrid ptable p-datatable-sm e-sm flex">
        <MuctieuBox
          v-for="item in muctieus"
          v-bind:key="item.Muctieu_ID"
          :goMuctieu="goMuctieu"
          :editMuctieu="editMuctieu"
          :delMuctieu="delMuctieu"
          :editCongviec="editCongviec"
          :delCongviec="delCongviec"
          :changeTrangthaiCV="changeTrangthaiCV"
          :addTask="addTask"
          :Muctieu="item"
        ></MuctieuBox>
      </div>
    </div>
    <div
      v-if="layout != 'grid' && muctieus.length > 0"
      class="w-full flex-grow-1 overflow-y-auto ptable p-datatable-sm e-sm flex flex-column"
    >
      <MuctieuItemListBox
        v-for="item in muctieus"
        v-bind:key="item.Muctieu_ID"
        :goMuctieu="goMuctieu"
        :editMuctieu="editMuctieu"
        :delMuctieu="delMuctieu"
        :editCongviec="editCongviec"
        :delCongviec="delCongviec"
        :changeTrangthaiCV="changeTrangthaiCV"
        :addTask="addTask"
        :Muctieu="item"
      ></MuctieuItemListBox>
    </div>
    <div
      class="align-items-center justify-content-center p-4 text-center"
      v-if="!isFirst && muctieus.length == 0"
    >
      <img src="../../assets/background/nodata.png" height="144" />
      <h3 class="m-1">Không có dữ liệu</h3>
    </div>
  </div>
  <Dialog
    header="Cập nhật mục tiêu"
    v-model:visible="displayAdd"
    :style="{ width: '960px', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <form @submit.prevent="handleSubmit(!v$.$invalid)">
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Mã </label>
          <InputText
            spellcheck="false"
            placeholder="Có thể để trống"
            v-bind:disabled="!isAdd"
            class="col-4 ip36"
            v-model="muctieu.Muctieu_ID"
          />
          <label class="col-2 text-right">Thời gian từ </label>
          <Calendar
            class="col-4 ml-0 p-0"
            id="thoigiankehoach"
            v-model="modeldate.thoigiankehoach"
            selectionMode="range"
            :showIcon="true"
            :manualInput="true"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Tên <span class="redsao">(*)</span></label>
          <InputText
            spellcheck="false"
            class="col-10 ip36"
            v-model="muctieu.Muctieu_Ten"
            :class="{ 'p-invalid': v$.Muctieu_Ten.$invalid && submitted }"
          />
        </div>
        <small
          v-if="
            (v$.Muctieu_Ten.$invalid && submitted) || v$.Muctieu_Ten.$pending.$response
          "
          class="col-10 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3">{{
              v$.Muctieu_Ten.required.$message
                .replace("Value", "Tên mục tiêu")
                .replace("is required", "không được để trống")
            }}</span>
          </div></small
        >
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Từ khoá</label>
          <Chips
            spellcheck="false"
            class="col-10 p-0"
            v-model="muctieu.Tukhoa"
            :addOnBlur="true"
            separator=","
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">STT</label>
          <InputNumber class="col-4 ip36 p-0" v-model="muctieu.STT" />

          <label class="col-2 text-right">Trạng thái</label>
          <Dropdown
            class="col-4"
            v-model="muctieu.Trangthai"
            :options="tdTrangthais"
            optionLabel="text"
            optionValue="value"
            placeholder="Chọn trạng thái"
          />
        </div>
        <Accordion class="w-full m-1">
          <AccordionTab header="Thông tin thêm">
            <div class="field col-12 md:col-12">
              <label class="col-2 text-left">Người thực hiện</label>
              <MultiSelect
                v-model="modeldate.thuchiens"
                :virtualScrollerOptions="{ itemSize: 10 }"
                :options="tdUsers"
                optionLabel="full_name"
                placeholder="Chọn người thực hiện"
                :filter="true"
                class="col-10 multiselect-thuchien"
              >
                <template #value="slotProps">
                  <div
                    class="user-item user-item-value"
                    v-for="option of slotProps.value"
                    :key="option.user_id"
                  >
                    <Avatar
                      v-bind:label="option.Avartar ? '' : option.Duan_Ten.substring(0, 1)"
                      v-bind:image="basedomainURL + option.Avartar"
                      style="background-color: #2196f3; color: #ffffff"
                      class="mr-2"
                      shape="circle"
                    />
                    <div>{{ option.full_name }}</div>
                  </div>
                  <template v-if="!slotProps.value || slotProps.value.length === 0">
                    Chọn người thực hiện
                  </template>
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
                    <div>{{ slotProps.option.full_name }}</div>
                  </div>
                </template>
              </MultiSelect>
            </div>
            <div class="field col-12 md:col-12">
              <label class="col-2 text-left">Người quản lý</label>
              <MultiSelect
                v-model="modeldate.quanlys"
                :virtualScrollerOptions="{ itemSize: 10 }"
                :options="tdUsers"
                optionLabel="full_name"
                placeholder="Chọn người quản lý"
                :filter="true"
                class="col-10 multiselect-quanly"
              >
                <template #value="slotProps">
                  <div
                    class="user-item user-item-value"
                    v-for="option of slotProps.value"
                    :key="option.user_id"
                  >
                    <Avatar
                      v-bind:label="option.Avartar ? '' : option.Duan_Ten.substring(0, 1)"
                      v-bind:image="basedomainURL + option.Avartar"
                      style="background-color: #22c55e; color: #ffffff"
                      class="mr-2"
                      shape="circle"
                    />
                    <div>{{ option.full_name }}</div>
                  </div>
                  <template v-if="!slotProps.value || slotProps.value.length === 0">
                    Chọn người quản lý
                  </template>
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
                      style="background-color: #22c55e; color: #ffffff"
                      class="mr-2"
                      shape="circle"
                    />
                    <div>{{ slotProps.option.full_name }}</div>
                  </div>
                </template>
              </MultiSelect>
            </div>
            <div class="field col-12 md:col-12">
              <label class="col-2 text-left">Người Theo dõi</label>
              <MultiSelect
                v-model="modeldate.theodoi"
                :virtualScrollerOptions="{ itemSize: 10 }"
                :options="tdUsers"
                optionLabel="full_name"
                placeholder="Chọn người theo dõi"
                :filter="true"
                class="col-10 multiselect-theodoi"
              >
                <template #value="slotProps">
                  <div
                    class="user-item user-item-value"
                    v-for="option of slotProps.value"
                    :key="option.user_id"
                  >
                    <Avatar
                      v-bind:label="option.Avartar ? '' : option.Duan_Ten.substring(0, 1)"
                      v-bind:image="basedomainURL + option.Avartar"
                      style="background-color: #64748b; color: #ffffff"
                      class="mr-2"
                      shape="circle"
                    />
                    <div>{{ option.full_name }}</div>
                  </div>
                  <template v-if="!slotProps.value || slotProps.value.length === 0">
                    Chọn người theo dõi
                  </template>
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
                      style="background-color: #64748b; color: #ffffff"
                      class="mr-2"
                      shape="circle"
                    />
                    <div>{{ slotProps.option.full_name }}</div>
                  </div>
                </template>
              </MultiSelect>
            </div>
            <div class="col-12">
              <label class="col-12 text-rigth">Mô tả</label>
              <div class="p-2">
                <Editor
                  v-model="muctieu.Mota"
                  spellcheck="false"
                  editorStyle="height: 100px;font-size:14px"
                />
              </div>
            </div>
            <div class="col-12">
              <label class="col-12 text-rigth">Khó khăn</label>
              <div class="p-2">
                <Editor
                  v-model="muctieu.Khokhan"
                  spellcheck="false"
                  editorStyle="height: 100px;font-size:14px"
                />
              </div>
            </div>
          </AccordionTab>
        </Accordion>
      </div>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="closedisplayAdd"
        class="p-button-raised p-button-secondary"
      />
      <Button
        label="Cập nhật"
        icon="pi pi-save"
        @click="handleSubmit(!v$.$invalid)"
        
      />
    </template>
  </Dialog>
  <Dialog
    header="Trạng thái công việc"
    v-model:visible="displayTrangthaiCV"
    :style="{ width: '360px', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <div v-for="item in tdTrangthaiCVs" v-bind:key="item.value">
      <Button
        class="shadow-none p-button-text p-button-plain"
        @click="updateTrangthaiCV(item)"
      >
        <TrangthaiCongviec :Trangthai="item.value"></TrangthaiCongviec>
      </Button>
    </div>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="closedisplayTrangthaiCV"
        class="p-button-raised p-button-secondary"
      />
    </template>
  </Dialog>
  <ModalCongviec
    :Muctieu_ID="muctieu.Muctieu_ID"
    :Congviec_ID="Congviec_ID"
    :Duan_ID="props.Duan_ID"
    :displayAddCongViec="displayAddCongViec"
  ></ModalCongviec>
</template>
<style scoped>
.spduan-name {
  font-weight: 500;
  color: #6c757d;
  font-size: 0.87rem;
}
.spduan-ngay {
  color: #6c757d;
  font-size: 0.87rem;
}
.ipnone {
  display: none;
}
.inputanh {
  border: 1px solid #ccc;
  width: 96px;
  height: 96px;
  cursor: pointer;
  background-color: #eee;
  padding: 1px;
}
.inputanh img {
  object-fit: contain;
  width: 100%;
  height: 100%;
}
.spandate {
  background-color: #eee;
  padding: 5px 10px;
  width: max-content;
  border-radius: 5px;
  margin: 10px 0;
  height: fit-content;
  margin-bottom: 0;
  margin-right: 5px;
  font-weight: 500;
  color: #6c757d;
  font-size: 0.85rem;
}
</style>
