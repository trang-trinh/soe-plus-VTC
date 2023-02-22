<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import moment from "moment";
import { encr, checkURL } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const router = inject("router");
const isDynamicSQL = ref(false);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const itemExport = "/Portals/Mau Excel/Mẫu Excel người ký luật.xlsx"; 
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  signer_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
const rules = {
  signer_name: {
    required,
		maxLength: maxLength(250),
    $errors: [
      {
        $property: "signer_name",
        $validator: "required",
        $message: "Tên người ký không được để trống!",
      },
    ],
  },
};
const signer = ref({
  signer_name: "",
  status: true,
  is_order: 1,
});
const selectedDispatchs = ref();
const submitted = ref(false);
const v$ = useVuelidate(rules, signer);
const issaveDispatch = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
const checkDelList = ref(false);
const departmentOptions = ref([
  { name: "Chọn phòng ban...", code: null },
  { name: "Kế toán", code: 0 },
  { name: "Nhân sự", code: 1 },
  { name: "Giám đốc", code: -1 },
]);
const typeOptions = ref([
  { name: "Chọn loại văn bản...", code: null },
  { name: "Văn bản đến", code: 1 },
  { name: "Văn bản đi", code: 2 },
  { name: "Nội bộ", code: 3 },
]);
const options = ref({
  IsNext: true,
  sort: null,
  SearchText: "",
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
});
//Thêm log
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
//Lấy số bản ghi
const loadCount = () => {
  axios
    .post(
      baseUrlCheck + "api/law_doc_signers/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "law_doc_types_count",
						par: [
							{ par: "search", va: options.value.SearchText },
							{ par: "user_id", va: store.state.user.user_id },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        options.value.totalRecords = data[0].totalRecords;
        sttDispatch.value = options.value.totalRecords + 1;
      }
    })
    .catch((error) => {
      addLog({
        title: "Lỗi Console loadCount",
        controller: "SQLView.vue",
        logcontent: error.message,
        loai: 2,
      });
    });
};
const loadData = (rf) => {
  if (rf) {
    if (isDynamicSQL.value) {
      loadDataSQL();
      return false;
    }
    if (rf) {
      if (options.value.PageNo == 0) {
        loadCount();
      }
    }
    axios
      .post(
        baseUrlCheck + "api/law_doc_signers/GetDataProc",
        { 
          str: encr(JSON.stringify({
              proc: "law_doc_signers_list",
              par: [
                { par: "pageno", va: options.value.PageNo },
                { par: "pagesize", va: options.value.PageSize },
                { par: "search", va: options.value.SearchText },
                { par: "status", va: options.value.Status },
                { par: "user_id", va: store.state.user.user_id },
              ],
            }), SecretKey, cryoptojs
          ).toString()
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        if (isFirst.value) isFirst.value = false;
        data.forEach((element, i) => {
          element.is_order = options.value.PageNo * options.value.PageSize + i + 1;
        });
        datalists.value = data;
        options.value.loading = false;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;
        addLog({
          title: "Lỗi Console loadData",
          controller: "SQLView.vue",
          logcontent: error.message,
          loai: 2,
        });
        if (error && error.status === 401) {
          swal.fire({
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
  }
	if (store.state.user.is_super == 1) {
        loadDonvi();
    }
};
const treedonvis = ref();
const loadDonvi = () => {
  axios
    .post(
      baseUrlCheck + "api/law_doc_signers/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "sys_org_list",
					}), SecretKey, cryoptojs
				).toString()
			},
			config
    )
    .then((response) => {
      treedonvis.value = [];
      let data = JSON.parse(response.data.data)[0];
      let sys = { name: "Hệ thống", code: 0 };
      treedonvis.value.push(sys);
      //console.log(data);
      if (data.length > 0) {
        data.forEach((x) => {
          x = { name: x.organization_name, code: x.organization_id };
          treedonvis.value.push(x);
        });
      } else {
        treedonvis.value = [];
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

//Phân trang dữ liệu
const onPage = (event) => {
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

    options.value.id = datalists.value[datalists.value.length - 1].signer_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].signer_id;
    options.value.IsNext = false;
  }
  options.value.PageNo = event.page;
  loadData(true);
};
//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);
const openBasic = (str) => {
  submitted.value = false;
  signer.value = {
    signer_name: "",
    is_order: sttDispatch.value,
    status: true,
  };
  if (store.state.user.is_super) {
    signer.value.organization_id = 0;
  } else {
    signer.value.organization_id = store.getters.user.organization_id;
  }
  issaveDispatch.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};
const closeDialog = () => {
  signer.value = {
    signer_name: "",
    is_order: 1,
    status: true,
  };
  displayBasic.value = false;
  loadData(true);
};

//Thêm bản ghi
const saveSigners = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  signer.value.signer_name = signer.value.signer_name.trim();
	if(signer.value.signer_name.length >= 250){
		swal.fire({
			title: "Thông báo!",
			text: "Tên người ký không được vượt quá 250 kí tự!",
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
  axios({
    method: issaveDispatch.value ? "put" : "post",
    url:
      baseUrlCheck +
      `/api/law_doc_signers/${
        issaveDispatch.value ? "Update_Signer" : "Add_Signer"
      }`,
    data: signer.value,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật người ký thành công!");
        loadData(true);
        closeDialog();
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

const sttDispatch = ref();
//Thêm bản ghi con
const isChirlden = ref(false);

//Sửa bản ghi
const editDispatch = (dataPlace) => {
  submitted.value = false;
  signer.value = dataPlace;
  isChirlden.value = false;
  if (dataPlace.parent_id != null) {
    isChirlden.value = true;
  }
  headerDialog.value = "Sửa người ký";
  issaveDispatch.value = true;
  displayBasic.value = true;
  if (store.state.user.is_super) {
    signer.value.organization_id = 0;
  } else {
    signer.value.organization_id = store.getters.user.organization_id;
  }
};
//Xóa bản ghi
const delDispatch = (Dispatch) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá người ký này không!",
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
          .delete(baseUrlCheck +
            "/api/law_doc_signers/Delete_Signer", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Dispatch != null ? [Dispatch.signer_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá người ký thành công!");
              loadData(true);
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

//Xuất excel
const menuButs = ref();
const itemButs = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportData("ExportExcel");
    },
  },
  {
    label: "Nhập Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      ImportExcel(event);
    },
  },
]);
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const exportData = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/Excel/ExportExcel",
      {
        excelname: "DANH SÁCH NGƯỜI KÝ",
        proc: "law_doc_signers_listexport",
        par: [{ par: "search", va: options.value.SearchText }],
      },
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();

        toast.success("Kết xuất Data thành công!");
        window.open(baseUrlCheck + response.data.path);
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
        store.commit("gologout");
      }
    });
};
// Import excel
const Imp = ref(false);
const ImportExcel = () => {
  Imp.value = true;
};
let files = [];
const removeFile = (event) => {
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
	swal.fire({
		width: 110,
		didOpen: () => {
		swal.showLoading();
		},
	});
  axios
    .post(baseUrlCheck +
      "/api/law_doc_signers/ImportExcel", formData, config)
    .then((response) => {
      swal.close();
      if (response.data.err == "0") {
        toast.success("Nhập dữ liệu thành công");
        isDynamicSQL.value = false;
        loadData(true);
			}
			else {
				swal.fire({
					title: "Error!",
					text: "Xảy ra lỗi khi import người ký văn bản luật!",
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
//Sort
const onSort = (event) => {
  options.value.sort = event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
  if (event.sortField != "is_order") {
    options.value.sort += ",is_order " + (event.sortOrder == 1 ? " ASC" : " DESC");
  }
  if (event.sortField != "signer_name") {
    options.value.sort += ",signer_name " + (event.sortOrder == 1 ? " ASC" : " DESC");
  }
  options.value.sort += ", created_date DESC";
  isDynamicSQL.value = true;
  loadDataSQL();
};
const filterSQL = ref([]);
const isFirst = ref(true);
const loadDataSQL = () => {
  let data = {
    sqlS: filterTrangthai.value != null ? filterTrangthai.value.toString() : null,
    sqlF: filterPhanloai.value != null ? filterPhanloai.value.toString() : null,
    id: options.value.id,
    next: options.value.IsNext,
    sqlO: options.value.sort || "created_date DESC, is_order DESC",
    Search: options.value.SearchText,
    PageNo: options.value.PageNo,
    PageSize: options.value.PageSize,
    fieldSQLS: filterSQL.value,
  };

  options.value.loading = true;
  axios
    .post(baseUrlCheck +
      "/api/SQL/Filter_Law_Doc_Signers", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.is_order = options.value.PageNo * options.value.PageSize + i + 1;
        });

        datalists.value = data;
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
      toast.error("Tải dữ liệu không thành công!");
      addLog({
        title: "Lỗi Console loadData",
        controller: "SQLView.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
//Tìm kiếm
const searchSigners = (event) => {
  options.value.loading = true;
  loadData(true);
};
const onFilter = (event) => {
  filterSQL.value = [];

  for (const [key, value] of Object.entries(event.filters)) {
    if (key != "global") {
      let obj = {
        key: key != "signer_name" ? "signer_name" : key,
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
  options.value.PageNo = 1;
  options.value.id = null;
  isDynamicSQL.value = true;
  loadDataSQL();
};
//Checkbox
const onCheckBox = (value) => {
  let data = {
    IntID: value.signer_id,
    TextID: value.signer_id + "",
    IntTrangthai: 1,
    BitTrangthai: value.status,
  };
  axios
    .put(baseUrlCheck +
      "/api/law_doc_signers/Update_StatusSigners", data, config)
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái thành công!");
        loadData(true);
        closeDialog();
      } else {
        console.log("LỖI A:", response);
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
//Xóa nhiều
const deleteList = () => {
  let listId = new Array(selectedDispatchs.value.length);
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá người ký này không!",
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
        selectedDispatchs.value.forEach((item) => {
          listId.push(item.signer_id);
        });
        axios
          .delete(baseUrlCheck +
            "/api/law_doc_signers/Delete_Signer", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: listId != null ? listId : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá người ký thành công!");
              checkDelList.value = false;

              loadData(true);
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

//Filter
const showFilter = ref(false);
const filterButs = ref();
const toggleFilter = (event) => {
  filterButs.value.toggle(event);
  if (showFilter.value) {
    showFilter.value = false;
  } else {
    showFilter.value = true;
  }
};
const itemfilterButs = ref([
  {
    label: "Phân loại",
    check: true,
  },
  {
    label: "Trạng thái",
    check: false,
  },
]);

const trangThai = ref([
  { name: "Hiển thị", code: 1 },
  { name: "Không hiển thị", code: 0 },
]);
const phanLoai = ref([
  { name: "Hệ thống", code: 0 },
  { name: "Đơn vị", code: store.state.user.organization_id },
]);
const filterPhanloai = ref();
const filterTrangthai = ref();
const styleObj = ref();
const style = ref({
  "background-color": "#2196F3 !important",
  color: "#fff !important",
  " border": "1px solid #5ca7e3 !important",
});
const reFilter = () => {
  filterPhanloai.value = null;
  filterTrangthai.value = null;
  //filterSigners();
  loadData(true);
  showFilter.value = false;
  styleObj.value = "";
};
const filterSigners = () => {
  filterSQL.value = [];
  let arr = [];
  let obj = {};
  let obj1 = {};
  if (filterPhanloai.value != null) {
    obj.key = "organization_id";
    obj.filteroperator = "and";
    arr.push({
      matchMode: "equals",
      value: filterPhanloai.value,
    });
    obj.filterconstraints = arr;
    filterSQL.value.push(obj);
  }
  if (filterTrangthai.value != null) {
    obj1.key = "status";
    obj1.filteroperator = "and";
    arr = [];
    arr.push({
      matchMode: "equals",
      value: filterTrangthai.value,
    });
    obj1.filterconstraints = arr;
    filterSQL.value.push(obj1);
  }
  options.value.PageNo = 0;
  options.value.id = null;  
  isDynamicSQL.value = true;
  loadDataSQL();
  showFilter.value = false;
  styleObj.value = style.value;
};
const refreshData = () => {
  options.value = {
    IsNext: true,
    sort: null,
    SearchText: "",
    PageNo: 0,
    PageSize: 20,
    loading: true,
    totalRecords: 0,
  };
  filterPhanloai.value = null;
  filterTrangthai.value = null;
  styleObj.value = "";
  selectedDispatchs.value = [];
  isDynamicSQL.value = false;
  loadData(true);
};
watch(selectedDispatchs, () => {
  if (selectedDispatchs.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});

onMounted(() => {
	if (!checkURL(window.location.pathname, store.getters.listModule)) {
		//router.back();
	}
  loadData(true);
  return {
    datalists,
    options,
    onPage,
    loadData,
    loadCount,
    openBasic,
    closeDialog,
    basedomainURL,
    saveSigners,
    isFirst,
    searchSigners,
    onCheckBox,
    selectedDispatchs,
    deleteList,
  };
});
</script>
<template>
  <div class="surface-100">
    <div class="h-2rem p-3 pb-0 m-3 mb-0 surface-0">
      <h3 class="m-0">
        <i class="pi pi-pound"></i> Danh sách người ký ({{
          options.totalRecords
        }})
      </h3>
    </div>
    <Toolbar class="outline-none mx-3 surface-0 border-none">
      <template #start>
        <span class="p-input-icon-left">
          <i class="pi pi-search" />
          <InputText
            v-model="options.SearchText"
            v-on:keyup.enter="searchSigners"
            type="text"
            spellcheck="false"
            placeholder="Tìm kiếm"
          />
          <Button
            class="ml-2 p-button-outlined p-button-secondary"
            icon="pi pi-filter"
            @click="toggleFilter"
            aria-haspopup="true"
            aria-controls="overlay_panel"
            :style="[styleObj]"
          />          
          <OverlayPanel
            class="p-0 m-0"
            ref="filterButs"
            appendTo="body"
            :showCloseIcon="false"
            id="overlay_panel"
            :style="store.state.user.is_super == 1 ? 'width:40rem' : 'width:300px'"
					>
            <div class="grid formgrid m-0">
                <div class="field col-12 md:col-12 p-0 mb-1">
                  <div class="flex col-12 p-0 mb-3">
                    <div :class="store.state.user.is_super == 1
                          ? 'col-2 text-left pt-2 p-0'
                          : 'col-4 text-left pt-2 p-0'
                        "
                      style="text-align: left"
                    >
                      Phân loại
                    </div>
                    <div :class="store.state.user.is_super == 1 ? 'col-10 pr-0' : 'col-8 pr-0'">
                      <Dropdown
                        class="col-12 p-0 m-0 md:col-12"
                        v-model="filterPhanloai"
                        :options="treedonvis"
                        optionLabel="name"
                        optionValue="code"
                        placeholder="Đơn vị"
                        :virtualScrollerOptions="{ itemSize: 20 }"
                        v-if="store.state.user.is_super == 1"
                      />
                      <Dropdown
                        class="col-12 p-0 m-0"
                        v-model="filterPhanloai"
                        :options="phanLoai"
                        optionLabel="name"
                        optionValue="code"
                        placeholder="Phân loại"
                        v-else
                      />
                    </div>
                  </div>
                  <div >
                    <div class="flex">
                      <div :class="store.state.user.is_super == 1
                              ? 'col-2 text-left pt-2 p-0'
                              : 'col-4 text-left pt-2 p-0'
                            "
                        style="text-align: left"
                      >
                        Trạng thái
                      </div>
                      <div :class="store.state.user.is_super == 1 ? 'col-10 pr-0' : 'col-8 pr-0'">
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

                    <Toolbar class="border-none surface-0 outline-none pb-0 px-0">
                      <template #start>
                        <Button class="p-button-outlined" label="Xóa"
                          @click="reFilter"                          
                        ></Button>
                      </template>
                      <template #end>
                        <Button @click="filterSigners" label="Lọc"></Button>
                      </template>
                    </Toolbar>
                  </div>
                </div>
              </div>
          </OverlayPanel>
        </span>
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
          @click="openBasic('Thêm người ký')"
          label="Thêm mới"
          icon="pi pi-plus"
          class="mr-2"
        />
        <Button
          @click="refreshData"
          class="mr-2 p-button-outlined p-button-secondary"
          icon="pi pi-refresh"
        />

        <Button
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
        />
      </template>
    </Toolbar>
    <div class="d-lang-table ml-3">
      <DataTable
        :value="datalists"
        :paginator="true"
        :rows="options.PageSize"
        :rowsPerPageOptions="[20, 30, 50, 100, 200]"
        :scrollable="true"
        scrollHeight="flex"
        :loading="options.loading"
        v-model:selection="selectedDispatchs"
        :lazy="true"
        @page="onPage($event)"
        @filter="onFilter($event)"
        @sort="onSort($event)"
        :totalRecords="options.totalRecords"
        dataKey="signer_id"
        :rowHover="true"
        v-model:filters="filters"
        filterDisplay="menu"
        :showGridlines="true"
        filterMode="lenient"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks  NextPageLink LastPageLink RowsPerPageDropdown"
        responsiveLayout="scroll"
        :globalFilterFields="['signer_name']"
      >
        <Column
          selectionMode="multiple"
          headerStyle="text-align:center;max-width:75px;height:50px"
          bodyStyle="text-align:center;max-width:75px;max-height:60px"
          class="align-items-center justify-content-center text-center"
        ></Column>
        <Column
          field="is_order"
          header="STT"
          :sortable="true"
          headerStyle="text-align:center;max-width:75px;height:50px"
          bodyStyle="text-align:center;max-width:75px;max-height:60px"
          class="align-items-center justify-content-center text-center"
        >
        </Column>

        <Column
          field="signer_name"
          header="Tên người ký"
          :sortable="true"
          headerStyle="height:50px"
          bodyStyle="max-height:60px"
        >
        </Column>
        <!-- <Column
          field="nav_type"
          header="Loại văn bản"
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:150px;height:50px"
          bodyStyle="text-align:center;max-width:150px;max-height:60px"
        >
        </Column> -->
        <Column
          field="position"
          header="Chức vụ"
          class="align-items-center"
          headerStyle="text-align:center;justify-content:center;max-width:300px;height:50px"
          bodyStyle="text-align:left;max-width:300px;max-height:60px"
        >
        </Column>
        <Column
          field="status"
          header="Hiển thị"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;max-height:60px"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="data">
            <Checkbox
              :binary="data.data.status"
              v-model="data.data.status"
              @click="onCheckBox(data.data)"
            />
          </template>
        </Column>
        <Column
          field="organization_id"
          header="Hệ thống"
          headerStyle="text-align:center;max-width:125px;height:50px"
          bodyStyle="text-align:center;max-width:125px;max-height:60px"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="data">
            <div v-if="data.data.organization_id == 0">
              <i
                class="pi pi-check text-blue-400"
                style="font-size: 1.5rem"
              ></i>
            </div>
            <div v-else></div>
          </template>
        </Column>
        <Column
          header="Chức năng"
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:150px;height:50px"
          bodyStyle="text-align:center;max-width:150px;max-height:60px"
        >
          <template #body="Dispatch">
            <div v-if="
              store.state.user.is_super == true ||
              store.state.user.user_id == Dispatch.data.created_by ||
              (store.state.user.is_admin == true &&
                store.state.user.organization_id == Dispatch.data.organization_id)
              "
            >
              <Button
                @click="editDispatch(Dispatch.data)"
                class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                type="button"
                icon="pi pi-pencil"
                v-tooltip="'Sửa'"
              ></Button>
              <Button
                @click="delDispatch(Dispatch.data, true)"
                class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                type="button"
                icon="pi pi-trash"
                v-tooltip="'Xóa'"
              ></Button>
            </div>
          </template>
        </Column>
        <template #empty>
          <div
            class="align-items-center justify-content-center p-4 text-center m-auto"
            v-if="!isFirst"
          >
            <img src="../../assets/background/nodata.png" height="144" />
            <h3 class="m-1">Không có dữ liệu</h3>
          </div>
        </template>
      </DataTable>
    </div>
  </div>
  <Dialog
    header="Tải lên file Excel"
    v-model:visible="Imp"
    :style="{ width: '40vw' }"
    :closable="true"
  >
    <h3>
      <label>
        <a :href="basedomainURL + itemExport" download>Nhấn vào đây</a> để tải xuống
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
  <Dialog
    :header="headerDialog"
    v-model:visible="displayBasic"
    :style="{ width: '40vw' }"
    :closable="false"
  >
    <form @submit.prevent="saveSigners(!v$.$invalid)">
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 flex">
          <label class="col-3 text-left p-0 flex" style="align-items: center;"> 
            Tên người ký <span class="redsao pl-1"> (*)</span>
          </label>
          <InputText
            v-model="signer.signer_name"
            spellcheck="false"
            class="col-9 p-0 m-0 ip36 px-2"
            :class="{ 'p-invalid': v$.signer_name.$invalid && submitted }"
          />
        </div>
        <small class="col-12 p-error"
					v-if="(v$.signer_name.required.$invalid && submitted) || v$.signer_name.required.$pending.$response"
				>
					<div class="field col-12 md:col-12 flex">
						<label class="col-3 text-left"></label>
						<span class="col-9 p-0">
							{{
								v$.signer_name.required.$message
									.replace("Value", "Tên người ký")
									.replace("is required", "không được để trống")
							}}
						</span>
					</div>
				</small>
				<small class="col-12 p-error"
					v-if="(v$.signer_name.maxLength.$invalid && submitted) || v$.signer_name.maxLength.$pending.$response"
				>
					<div class="field col-12 md:col-12 flex">
						<label class="col-3 text-left"></label>
						<span class="col-9 p-0">
							{{
								v$.signer_name.maxLength.$message.replace(
									"The maximum length allowed is",
									"Tên người ký không được vượt quá"
								)
							}}
							ký tự
						</span>
					</div>
				</small>
        <div class="field col-12 md:col-12 flex">
          <label class="col-3 text-left p-0 flex" style="align-items: center;"> 
            Chức vụ
          </label>
          <InputText class="col-9 p-0 ip36 px-2" v-model="signer.position" />
        </div>
        <div class="field col-12 md:col-12 flex">
          <label class="col-3 text-left p-0 flex" style="align-items: center;"> 
            Phòng ban
          </label>
          <Dropdown
            class="col-9 p-0"
            v-model="signer.department_id"
            :options="departmentOptions"
            optionLabel="name"
            optionValue="code"
            placeholder="Chọn phòng ban"
            :editable="true"
            :filter="true"
          />
        </div>
        <div class="field col-12 md:col-12 flex">
          <label class="col-3 text-left p-0 flex" style="align-items: center;"> 
            Loại văn bản
          </label>
          <Dropdown
            class="col-9 p-0"
            v-model="signer.nav_type"
            :options="typeOptions"
            optionLabel="name"
            placeholder="Chọn loại văn bản"
            optionValue="code"
            :filter="true"
          />
        </div>
        <div class="col-12 md:col-12 flex">
          <div class="field col-6 md:col-6 p-0">
            <label class="col-6 p-0">Số thứ tự</label>
            <InputNumber v-model="signer.is_order" class="col-6 p-0 ip36" />
          </div>
          <div class="field col-6 md:col-6 p-0">
            <label class="col-6 text-center p-0"
                style="vertical-align: text-bottom">
                Trạng thái
              </label>
            <InputSwitch v-model="signer.status" />
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog"
        class="p-button-text"
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveSigners(!v$.$invalid)"
      />
    </template>
  </Dialog>
</template>

<style scoped>
.d-lang-table {
  height: calc(100vh - 155px);
}
.inputanh {
  border: 1px solid #ccc;
  width: 96px;
  height: 96px;
  cursor: pointer;
  padding: 1px;
}
.ipnone {
  display: none;
}
.inputanh img {
  object-fit: cover;
  width: 100%;
  height: 100%;
}
</style>
<style lang="scss" scoped>
  ::v-deep(.p-dropdown-panel){
    .p-dropdown-items {
        max-width: 500px;
    }
  }  
</style>
