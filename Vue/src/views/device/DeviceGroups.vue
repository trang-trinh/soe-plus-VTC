<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../util/function.js";
//Khai báo
const router = inject("router");
const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const item = "/Portals/Mau Excel/Mẫu Excel Chức vụ.xlsx";
const emitter = inject("emitter");
const isDynamicSQL = ref(false);

const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const first = ref(0);

const rules = {
  groups_name: {
    required,
    $errors: [
      {
        $property: "groups_name",
        $validator: "required",
        $message: "Tên nhóm thiết bị không được để trống!",
      },
    ],
  },
  groups_code: {
    required,
    $errors: [
      {
        $property: "groups_code",
        $validator: "required",
        $message: "Mã nhóm thiết bị không được để trống!",
      },
    ],
  },
};
//Nơi nhận EMIT từ component

emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "hideSidebarGU":
      showSidebarUser.value = false;
      break;
  }
});
 
const device_groups = ref({
  groups_name: "",
  status: true,
  is_order: 1,
  type: true,
});
const selectedWarehouses = ref();
const submitted = ref(false);
const v$ = useVuelidate(rules, device_groups);
const issaveWarehouse = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = fileURL;
const checkDelList = ref(false);
const options = ref({
  IsNext: true,
  sort: "device_groups_id DESC",
  SearchText: null,
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
});
//Thêm log
 
//Lấy số bản ghi
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "device_groups_count",
        par: [
          { par: "user_id", va: store.state.user.user_id },
          { par: "status", va: null },
        ],
      }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      let data1 = JSON.parse(response.data.data)[1];

      if (data.length > 0) {
        options.value.totalRecords = data[0].totalRecords;
      }
      if (data1.length > 0) {
        options.value.totalRecords = data1[0].totalRecordsPage;
      }
    })
    .catch(( ) => {
      
    });
};
const showSidebarUser = ref(false);
 

const renderTree = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      m.STT = i + 1;
      let om = { key: m[id], data: m };
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em, j) => {
            em.STT = mm.data.STT + "." + (j + 1);
            let om1 = { key: em[id], data: em };
            rechildren(om1, em[id]);
            mm.children.push(om1);
          });
        }
      };
      rechildren(om, m[id]);
      arrChils.push(om);
      //
      om = { key: m[id], data: m[id], label: m[name] };
      const retreechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em) => {
            let om1 = { key: em[id], data: em[id], label: em[name] };
            retreechildren(om1, em[id]);
            mm.children.push(om1);
          });
        }
      };
      retreechildren(om, m[id]);
      arrtreeChils.push(om);
    });
  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};

const onNodeSelect = (node) => {
  selectedNodes.value.push(node.data.device_groups_id);
};
const onNodeUnselect = (node) => {
  selectedNodes.value.splice(
    selectedNodes.value.indexOf(node.data.device_groups_id),
    1
  );
};
const selectedNodes = ref([]);
const treemodules = ref();

//Lấy dữ liệu ngôn ngữ
const loadData = (rf) => {
  if (rf) {
    if (isDynamicSQL.value) {
      loadDataSQL();
      return;
    }
    if (rf) {
      loadCount();
    }
    axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "device_groups_list",
          par: [
            { par: "search", va: options.value.search },
            { par: "pageno", va: options.value.PageNo },
            { par: "pagesize", va: options.value.PageSize },
            { par: "user_id", va: store.state.user.user_id },
            { par: "status", va: null },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        data.forEach((element, i) => {
          element.STT = options.value.PageNo * options.value.PageSize + i + 1;
        });
        if (isFirst.value) isFirst.value = false;
        let obj = renderTree(
          data,
          "device_groups_id",
          "groups_name",
          "cấp cha"
        );
        treemodules.value = obj.arrtreeChils;

        datalists.value = obj.arrChils;
        options.value.loading = false;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        
        options.value.loading = false;

        if (error && error.status === 401) {
          swal.fire({
            title: "Thông báo",
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            icon: "error",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
  }
};
//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);
const openBasic = (str) => {
  submitted.value = false;
  selectCapcha.value = null;
  device_groups.value = {
    groups_name: null,
    price: null,
    is_order: datalists.value.filter((x) => x.parent_id == null).length + 1,
    status: true,
    organization_id: store.getters.user.organization_id,
    parent_id: -1,
  };
  issaveWarehouse.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};
const closeDialog = () => {
  device_groups.value = {
    groups_name: "",
    is_order: 1,
    status: true,
  };
  displayBasic.value = false;
  loadData(true);
};

//Thêm bản ghi
const saveWarehouse = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  if (device_groups.value.groups_code.length > 50) {
    swal.fire({
      title: "Thông báo!",
      text: "Mã nhóm thiết bị không quá 50 ký tự!",
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

  let arw = null;
  if (selectCapcha.value)
    Object.keys(selectCapcha.value).forEach((key) => {
      arw = key;
      return;
    });
  device_groups.value.parent_id = arw;
  if ((arw = "" || arw == -1)) device_groups.value.parent_id = null;
  if (!issaveWarehouse.value) {
    axios
      .post(
        baseURL + "/api/device_groups/add_device_groups",
        device_groups.value,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm nhóm thiết bị thành công!");
          loadData(true);
          closeDialog();
          Refresh();
        } else {
          let ms = response.data.ms;
          swal.fire({
            title: "Thông báo!",
            text:
              ms.includes("groups_name") == true
                ? "Tên nhóm thiết bị không quá 500 ký tự!"
                : ms,
            icon: "error",
            confirmButtonText: "OK",
          });
          loadData(true);
        }
      })
      .catch((error) => {
        swal.close();
        swal.fire({
          title: "Thông báo",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!" + error.message,
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  } else {
    axios
      .put(
        baseURL + "/api/device_groups/update_device_groups",
        device_groups.value,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa nhóm thiết bị thành công!");

          closeDialog();
        } else {
          let ms = response.data.ms;
          swal.fire({
            title: "Thông báo!",
            text:
              ms.includes("groups_name") == true
                ? "Tên nhóm thiết bị không quá 500 ký tự!"
                : ms,
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
  }
};

 
//Thêm bản ghi con
const isChirlden = ref(false);
const selectCapcha = ref();
const onChangeParent = () => {
  datalists.value;
  let arw = null;
  if (selectCapcha.value)
    Object.keys(selectCapcha.value).forEach((key) => {
      arw = key;
      return;
    });
  device_groups.value.is_order = arw
    ? datalists.value.filter((x) => x.parent_id == arw).length + 1
    : 1;
};
const addWarehouseChild = (data) => {
  submitted.value = false;
  selectCapcha.value = {};
  selectCapcha.value[data.device_groups_id] = true;
  device_groups.value = {
    groups_name: null,
    price: null,
    is_order: datalists.value.filter((x) => x.parent_id == null).length + 1,
    status: true,
    organization_id: store.getters.user.organization_id,
    parent_id: -1,
  };
  issaveWarehouse.value = false;
  headerDialog.value = "Thêm mới";
  displayBasic.value = true;
};
//Sửa bản ghi
const editWarehouse = (dataPlace) => {
  submitted.value = false;
  device_groups.value = dataPlace;

  selectCapcha.value = {};
  selectCapcha.value[dataPlace.parent_id] = true;
  device_groups.value.organization_id =
    store.state.user.is_super == true ? 0 : store.state.user.organization_id;
  isChirlden.value = false;
  if (dataPlace.parent_id != null) {
    isChirlden.value = true;
  }
  headerDialog.value = "Sửa nhóm thiết bị";
  issaveWarehouse.value = true;
  displayBasic.value = true;
};
//Xóa bản ghi
const delWarehouse = (Warehouse) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá nhóm thiết bị này không!",
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
          .delete(baseURL + "/api/device_groups/delete_device_groups", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Warehouse != null ? [Warehouse.device_groups_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err == "0") {
              swal.close();
              toast.success("Xoá nhóm thiết bị thành công!");
              if (
                (options.value.totalRecords - Warehouse.length) % 2 == 0 &&
                options.value.PageNo > 0
              ) {
                options.value.PageNo = options.value.PageNo - 1;
              }
              loadData(true);
            } else {
              swal.fire({
                title: "",
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
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};

//Xuất excel 
//Sort
const onSort = (event) => {
  options.value.sort =
    event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
  if (event.sortField == "STT") {
    options.value.sort = "is_order" + (event.sortOrder == 1 ? " ASC" : " DESC");
  }
  isDynamicSQL.value = true;
  loadDataSQL();
};

const onFilter = (event) => {
  filterSQL.value = [];
  for (const [key, value] of Object.entries(event.filters)) {
    if (key != "global") {
      let obj = {
        key: key,
        filteroperator: value.operator,
        filterconstraints: value.constraints,
      };
      if (value.value && value.value.length > 0) {
        obj.filteroperator = value.matchMode;
        obj.filterconstraints = [];
        value.value.forEach(function (vl) {
          obj.filterconstraints.push({ value: vl[obj.key] });
        });
      } else if (value.matchMode) {
        obj.filteroperator = "and";
        obj.filterconstraints = [value];
      }
      if (
        obj.filterconstraints &&
        obj.filterconstraints.filter((x) => x.value != null).length > 0
      )
        filterSQL.value.push(obj);
    }
  }
  options.value.PageNo = 0;
  first.value = 0;
  options.value.id = null;
  loadDataSQL();
};
const filterSQL = ref([]);
const isFirst = ref(true);
const loadDataSQL = () => {
  datalists.value = [];

  let data = {
    sqlS: filterTrangthai.value != null ? filterTrangthai.value : null,
    sqlO: options.value.sort,
    Search: options.value.SearchText,
    PageNo: options.value.PageNo,
    PageSize: options.value.PageSize,
    next: true,
    sqlF: null,
    fieldSQLS: filterSQL.value,
  };
  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filter_device_groups", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.STT = options.value.PageNo * options.value.PageSize + i + 1;
        });
        let obj = renderTree(
          data,
          "device_groups_id",
          "groups_name",
          "cấp cha"
        );
        treemodules.value = obj.arrtreeChils;

        datalists.value = obj.arrChils;
      } else {
        datalists.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      options.value.loading = false;
      //Show Count nếu có
      if (dt.length == 2) {
        options.value.totalRecords = dt[1][0].totalRecords;
      }
    })
    .catch((error) => {
      options.value.loading = false;

     
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
//Tìm kiếm
const searchWarehouses = () => {
  if (options.value.SearchText.includes("'")) {
    options.value.SearchText = options.value.SearchText.replace("'", "");
  }
  isDynamicSQL.value = true;
  loadData(true);
};

//Checkbox
const onCheckBox = (value) => {
  options.value.loading = true;
  let data = {
    IntID: value.device_groups_id,
    TextID: value.device_groups_id + "",
    IntTrangthai: 1,
    BitTrangthai: value.status,
  };

  axios
    .put(baseURL + "/api/device_groups/update_s_device_groups", data, config)
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Sửa nhóm thiết bị thành công!");
        loadData(true);
        closeDialog();
      } else {
        swal.fire({
          title: "Thông báo",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch(() => {
      swal.close();
      swal.fire({
        title: "Thông báo",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
//Xóa nhiều
const deleteList = () => {
  let listId = new Array(liKeyDel.value.length);
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá danh sách này không!",
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
        liKeyDel.value.forEach((item) => {
          listId.push(item);
        });
        axios
          .delete(baseURL + "/api/device_groups/delete_device_groups", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: listId != null ? listId : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá danh sách thành công!");
              checkDelList.value = false;

              loadData(true);
            } else {
              swal.fire({
                title: "",
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
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
const liKeyDel = ref([]);
watch(selectedWarehouses, () => {
  liKeyDel.value = [];
  Object.keys(selectedWarehouses.value).forEach((key) => {
    liKeyDel.value.push(Number(key));
  });
  if (Object.keys(selectedWarehouses.value).length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});
const onPage = (event) => {
  if (event.rows != options.value.PageSize) {
    options.value.PageSize = event.rows;
  }
  if (event.page == 0) {
    //Trang đầu
    options.value.id = null;
    options.value.IsNext = true;
  } else if (event.page > options.value.PageNo + 1) {
    //Trang cuối
    options.value.id = -1;
    options.value.IsNext = false;
  } else if (event.page > options.value.PageNo) {
    //Trang sau

    options.value.id = datalists.value[datalists.value.length - 1].place_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].place_id;
    options.value.IsNext = false;
  }
  options.value.PageNo = event.page;
  loadData(true);
};
const Refresh = () => {
  filterTrangthai.value = null;
  first.value = 0;
  selectedWarehouses.value = [];

  options.value = {
    IsNext: true,
    sort: "created_date DESC",
    SearchText: "",
    PageNo: 0,
    PageSize: 20,
    loading: true,
    totalRecords: null,
  };
  checkFilter.value = false;
  styleObj.value = "";
  datalists.value = [];
  filters.value = {
    groups_name: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
    },
  };
  isDynamicSQL.value = false;
  loadData(true);
  loadCount();
};
const op = ref();
const filterTrangthai = ref();
const checkFilter = ref(false);
const toggle = (event) => {
  op.value.toggle(event);
};
const trangThai = ref([
  { name: "Hiển thị", code: 1 },
  { name: "Không hiển thị", code: 0 },
]);
const reFilter = () => {
  isDynamicSQL.value = false;
  filterTrangthai.value = null;
  checkFilter.value = false;
  loadData(true);
};
const filter = (valueST) => {
  filterSQL.value = [];
  checkFilter.value = true;
  let filterS = {
    filterconstraints: [{ value: valueST, matchMode: "equals" }],
    filteroperator: "and",
    key: "status",
  };
  filterSQL.value.push(filterS);
  isDynamicSQL.value = true;
  loadData(true);
};
const filters = ref({
  groups_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});

const Imp = ref(false);
 
let files = [];
const removeFile = () => {
  files = [];
};
const selectFile = (event) => {
  event.files.forEach((element) => {
    files.push(element);
  });
};
const Upload = () => {
  Imp.value = false;
  let formData = new FormData();
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("url_file", file);
  }
  axios
    .post(baseURL + "/api/ImportExcel/Import_Warehouse", formData, config)
    .then(() => {
      toast.success("Nhập dữ liệu thành công");
      isDynamicSQL.value = false;
      loadData(true);
    })
    .catch(( ) => {
      swal.close();
      swal.fire({
        title: "Thông báo",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const styleObj = ref();
onMounted(() => {  if (!checkURL(window.location.pathname, store.getters.listModule)) {
     //router.back();
  }
  loadData(true);
  loadCount();
  return {
    datalists,
    options,
    loadData,
    loadCount,
    openBasic,
    closeDialog,
    basedomainURL,
    saveWarehouse,
    isFirst,
    searchWarehouses,
    onCheckBox,
    selectedWarehouses,
    deleteList,
  };
});
</script>
            <template>
  <div class="main-layout true flex-grow-1 p-2 pb-0 pr-0">
    <TreeTable
      :value="datalists"
      v-model:selectionKeys="selectedWarehouses"
      :loading="options.loading"
      @nodeSelect="onNodeSelect"
      @nodeUnselect="onNodeUnselect"
      :filters="filters"
      :showGridlines="true"
      selectionMode="checkbox"
      filterMode="strict"
      class="p-treetable-sm"
      :paginator="true"
      :rows="options.PageSize"
      :rowHover="true"
      responsiveLayout="scroll"
      :lazy="true"
      :scrollable="true"
      scrollHeight="flex"
      @page="onPage($event)"
      @sort="onSort($event)"
      @filter="onFilter($event)"
      :totalRecords="options.totalRecords"
      filterDisplay="menu"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-qrcode"></i> Danh sách nhóm thiết bị ({{
            options.totalRecords
          }})
        </h3>

        <Toolbar class="w-full custoolbar">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                v-model="options.SearchText"
                @keyup.enter="searchWarehouses"
                type="text"
                spellcheck="false"
                placeholder="Tìm kiếm"
              />
            </span>
            <Button
              :class="
                filterTrangthai != null && checkFilter
                  ? ''
                  : 'p-button-secondary p-button-outlined'
              "
              type="button"
              class="ml-2"
              icon="pi pi-filter"
              @click="toggle"
              aria:haspopup="true"
              aria-controls="overlay_panel"
              v-tooltip="'Bộ lọc'"
            />
            <OverlayPanel
              ref="op"
              appendTo="body"
              class="p-0 m-0"
              :showCloseIcon="false"
              id="overlay_panel"
              :style="
                store.state.user.is_super == 1 ? 'width:25rem' : 'width:300px'
              "
            >
              <div class="grid formgrid m-0">
                <div class="flex field col-12 p-0 align-items-center">
                  <div class="col-4">Trạng thái</div>
                  <div class="col-8">
                    <Dropdown
                      class="col-12 p-0 m-0"
                      v-model="filterTrangthai"
                      :options="trangThai"
                      optionLabel="name"
                      optionValue="code"
                      placeholder="Trạng thái"
                    />
                  </div>
                </div>
                <div class="flex col-12 p-0">
                  <Toolbar
                    class="border-none surface-0 outline-none pb-0 w-full"
                  >
                    <template #start>
                      <Button
                        @click="reFilter()"
                        class="p-button-outlined"
                        label="Xóa"
                      ></Button>
                    </template>
                    <template #end>
                      <Button
                        @click="filter(filterTrangthai)"
                        label="Lọc"
                      ></Button>
                    </template>
                  </Toolbar>
                </div>
              </div>
            </OverlayPanel>
          </template>

          <template #end>
            <Button
              v-if="checkDelList"
              @click="deleteList()"
              label="Xóa"
              icon="pi pi-trash"
              class="mr-2 p-button-danger"
            />
            <Button
              @click="openBasic('Thêm mới')"
              label="Thêm mới"
              icon="pi pi-plus"
              class="mr-2"
            />
            <Button
              @click="Refresh"
              class="mr-2 p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              v-tooltip="'Tải lại'"
            />

            <!-- <Button
              label="Tiện ích"
              icon="pi pi-file-excel"
              class="mr-2 p-button-outlined p-button-secondary"
              @click="toggleExport"
              aria-haspopup="true"
              aria-controls="overlay_Export"
            />
            <Menu
              id="overlay_Export"
              ref="menuButs"
              :model="itemButs"
              :popup="true"
            /> -->
          </template>
        </Toolbar></template
      >
      <Column
        field="is_order"
        :sortable="true"
        header="STT"
        class="align-items-center justify-content-center text-center font-bold"
        headerStyle="text-align:center;max-width:100px"
        bodyStyle="text-align:center;max-width:100px"
      >
        <template #body="md">
          <div v-bind:class="md.node.data.status ? '' : 'text-error'">
            {{ md.node.data.STT }}
          </div>
        </template>
      </Column>
      <Column
        field="groups_code"
        header="Mã nhóm"
        :sortable="true"
        headerStyle="text-align:center;max-width:180px"
        bodyStyle="text-align:center;max-width:180px"
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="groups_name"
        header="Tên nhóm"
        :sortable="true"
        :expander="true"
      >
      </Column>
      <Column
        field="status"
        header="Trạng thái"
        class="align-items-center justify-content-center"
        headerStyle="text-align:center;max-width:180px"
        bodyStyle="text-align:center;max-width:180px"
        filterMatchMode="contains"
      >
        <template #body="data">
          <Checkbox
            :disabled="
              !(
                store.state.user.is_super == true ||
                store.state.user.user_id == data.node.data.created_by ||
                (store.state.user.role_id == 'admin' &&
                  store.state.user.organization_id ==
                    data.node.data.organization_id)
              )
            "
            :binary="data.node.data.status"
            v-model="data.node.data.status"
            @click="onCheckBox(data.node.data)"
          />
        </template>
      </Column>
      <Column
        field="status"
        header="Chức năng"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px"
      >
        <template #body="data">
          <Button
            type="button"
            icon="pi pi-plus-circle"
            class="p-button-rounded p-button-secondary p-button-outlined"
            v-tooltip.top="'Thêm nhóm con'"
            @click="addWarehouseChild(data.node.data)"
          ></Button>
          <div
            v-if="
              store.state.user.is_super == true ||
              store.state.user.user_id == data.node.data.created_by ||
              (store.state.user.role_id == 'admin' &&
                store.state.user.organization_id ==
                  data.node.data.organization_id)
            "
          >
            <Button
              @click="editWarehouse(data.node.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              v-tooltip.top="'Sửa'"
            ></Button>
            <Button
              @click="delWarehouse(data.node.data, true)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-trash"
              v-tooltip.top="'Xóa'"
            ></Button>
          </div>
        </template>
      </Column>
      <template #empty>
        <div
          class="
            align-items-center
            justify-content-center
            p-4
            text-center
            m-auto
          "
          v-if="!isFirst"
        >
          <img src="../../assets/background/nodata.png" height="144" />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </TreeTable>
  </div>
  <Dialog
    :maximizable="true"
    :header="headerDialog"
    v-model:visible="displayBasic"
    :style="{ width: '40vw' }"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 md:col-12 p-0">
          <div class="field col-12 md:col-12 flex">
            <div class="col-12 p-0 flex align-items-center">
              <label class="col-4 text-left p-0"
                >Mã nhóm thiết bị <span class="redsao">(*)</span></label
              >
              <InputText
                v-model="device_groups.groups_code"
                spellcheck="false"
                class="col-8 ip36 px-2"
                :class="{ 'p-invalid': v$.groups_code.$invalid && submitted }"
              />
            </div>
          </div>
          <div
            v-if="
              (v$.groups_code.$invalid && submitted) ||
              v$.groups_code.$pending.$response
            "
            style="display: flex"
            class="field col-12 md:col-12"
          >
            <div class="col-2 text-left"></div>
            <small class="col-10 p-error p-0">
              <span class="col-12 p-0">{{
                v$.groups_code.required.$message
                  .replace("Value", "Mã nhóm thiết bị")
                  .replace("is required", "không được để trống")
              }}</span>
            </small>
          </div>
        </div>

        <div class="col-12 md:col-12 p-0">
          <div class="field col-12 md:col-12 flex">
            <div class="col-12 p-0 flex align-items-center">
              <label class="col-4 text-left p-0"
                >Tên nhóm thiết bị <span class="redsao">(*)</span></label
              >
              <InputText
                v-model="device_groups.groups_name"
                spellcheck="false"
                class="col-8 ip36 px-2"
                :class="{ 'p-invalid': v$.groups_name.$invalid && submitted }"
              />
            </div>
          </div>
          <div
            v-if="
              (v$.groups_name.$invalid && submitted) ||
              v$.groups_name.$pending.$response
            "
            style="display: flex"
            class="field col-12 md:col-12"
          >
            <div class="col-2 text-left"></div>
            <small class="col-10 p-error p-0">
              <span class="col-12 p-0">{{
                v$.groups_name.required.$message
                  .replace("Value", "Tên nhóm thiết bị")
                  .replace("is required", "không được để trống")
              }}</span>
            </small>
          </div>
        </div>
        <div class="col-12 md:col-12 p-0">
          <div class="field col-12 md:col-12 flex">
            <div class="col-12 p-0 flex align-items-center">
              <label class="col-4 text-left p-0">Cấp cha</label>
              <TreeSelect
                class="col-8 p-0"
                @change="onChangeParent"
                v-model="selectCapcha"
                :options="treemodules"
                :showClear="true"
                placeholder="-----Chọn cấp cha-----"
                optionLabel="data.groups_name"
                optionValue="data.groups_id"
                panelClass="d-design-dropdown"
              ></TreeSelect>
            </div>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex align-items-center">
          <label class="col-4 text-left p-0">STT</label>
          <InputNumber
            v-model="device_groups.is_order"
            spellcheck="false"
            class="col-8 p-0 ip36"
          />
        </div>
        <div class="field col-12 md:col-12 flex">
          <label style="vertical-align: text-bottom" class="col-4 text-left p-0"
            >Trạng thái
          </label>
          <InputSwitch
            class="w-4rem lck-checked"
            v-model="device_groups.status"
          />
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog"
        class="p-button-outlined"
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveWarehouse(!v$.$invalid)"
      />
    </template>
  </Dialog>
  <Dialog
    header="Tải lên file Excel"
    v-model:visible="Imp"
    :style="{ width: '40vw' }"
    :closable="true"
  >
    <h3>
      <label>
        <a :href="basedomainURL + item" download>Nhấn vào đây</a> để tải xuống
        tệp mẫu.
      </label>
    </h3>
    <form>
      <FileUpload
        accept=".xls,.xlsx"
        @remove="removeFile"
        @select="selectFile"
        :multiple="false"
        :show-upload-button="false"
        choose-label="Chọn tệp"
        cancel-label="Hủy"
      >
        <template #empty>
          <p>Kéo và thả tệp vào đây để tải lên.</p>
        </template>
      </FileUpload>
    </form>
    <template #footer
      ><Button label="Lưu" icon="pi pi-check" @click="Upload"
    /></template>
    <!-- Chức năng đang chỉnh sửa vui lòng liên hệ quản trị viên phần mềm -->
  </Dialog>
</template>
            
            <style scoped>
.sel-placeholder::placeholder {
  text-align: center;
  position: absolute;
  top: 0;
}
</style>