<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../../util/function.js";
import paycheck_form_dialog from "./component/paycheck_form_dialog.vue"
import moment from "moment";
//Khai báo

const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const isDynamicSQL = ref(false);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  faculty_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
 
//Lấy số bản ghi
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_paycheck_form_count",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
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
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        options.value.totalRecords = data[0].totalRecords;
        sttStamp.value = data[0].totalRecords + 1;
      }
    })
    .catch((error) => {});
};
const onFilterPaycheck = (event) => {
  listPaycheck.value = [];
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_declare_list",
            par: [
              { par: "search", va: event.value },
              { par: "type", va: null },
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 100 },
              { par: "user_id", va: store.getters.user.user_id },
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
      let data = JSON.parse(response.data.data)[0];
      if (isFirst.value) isFirst.value = false;
      data.forEach((element, i) => {
        element.STT = options.value.PageNo * options.value.PageSize + i + 1;

        listPaycheck.value.push({
          name: element.declare_name,
          code: element.declare_id,
        });
      });
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const listPaycheck = ref([]);
const initTudien = () => {
  listPaycheck.value = [];
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_declare_list",
            par: [
              { par: "search", va: options.value.SearchDeclare },
              { par: "type", va: null },
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 100 },
              { par: "user_id", va: store.getters.user.user_id },
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
      let data = JSON.parse(response.data.data)[0];
      if (isFirst.value) isFirst.value = false;
      data.forEach((element, i) => {
        element.STT = options.value.PageNo * options.value.PageSize + i + 1;

        listPaycheck.value.push({
          name: element.declare_name,
          code: element.declare_id,
        });
      });
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
//Lấy dữ liệu paycheck_form
const loadData = (rf) => {
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
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_paycheck_form_list",
            par: [
              { par: "pageno", va: options.value.PageNo },
              { par: "pagesize", va: options.value.PageSize },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "search", va: options.value.SearchText },
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
      let data = JSON.parse(response.data.data)[0];
      if (isFirst.value) isFirst.value = false;
      data.forEach((element, i) => {
        element.STT = options.value.PageNo * options.value.PageSize + i + 1;
      });
 
      datalists.value = data;

      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
      console.log(error);
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
//Phân trang dữ liệu
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

    options.value.id = datalists.value[datalists.value.length - 1].paycheck_form_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].paycheck_form_id;
    options.value.IsNext = false;
  }
  options.value.PageNo = event.page;
  loadData(true);
};

const paycheck_form = ref({
  faculty_name: "",
  emote_file: "",
  status: true,
  is_default: false,
  is_order: 1,
});

const selectedStamps = ref();
const submitted = ref(false);
 
const isSaveTem = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = baseURL;
const checkDelList = ref(false);

const options = ref({
  IsNext: true,
  sort: "created_date",
  SearchText: null,
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
});

//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);
const openBasic = (str) => {
  submitted.value = false;
  paycheck_form.value = {
    faculty_name: "",
    emote_file: "",
    status: true,
    is_default: false,
    is_order: sttStamp.value,
    organization_id: store.getters.user.organization_id,
  };

  checkIsmain.value = false;
  isSaveTem.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};
const closeDialogDialog= () => {
  
  paycheck_form.value = {
   
    status: true,
 
    is_order: 1,
  };

  displayPaycheck.value = false;
 
};
const closeDialog = () => {
  paycheck_form.value = {
    faculty_name: "",
    emote_file: "",
    status: true,
    is_default: false,
    is_order: 1,
  };

  displayBasic.value = false;
  loadData(true);
};

//Thêm bản ghi

const filesList = ref([]);
let fileSize = [];
const onUploadFile = (event) => {
  fileSize = [];
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
            fileSize.push(fi.size);
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
const listFilesS = ref([]);
const removeFile = (event) => {
  filesList.value = filesList.value.filter((a) => a != event.file);
};

const sttStamp = ref(1);
const saveData = ( ) => {
  submitted.value = true;
 

  let formData = new FormData();

  for (var i = 0; i < filesList.value.length; i++) {
    let file = filesList.value[i];
    formData.append("image", file);
  }
  paycheck_form.value.profile_id=paycheck_form.value.user_id_fake.profile_id

  formData.append("hrm_paycheck_form", JSON.stringify(paycheck_form.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
 
  if (!isSaveTem.value) {
    axios
      .post(
        baseURL + "/api/hrm_paycheck_form/add_hrm_paycheck_form",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm thông tin phiếu lương thành công!");

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
  } else {
    axios
      .put(
        baseURL + "/api/hrm_paycheck_form/update_hrm_paycheck_form",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa thông tin phiếu lương thành công!");

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
  }
};
const checkIsmain = ref(true);
//Sửa bản ghi
const editTem = (dataTem) => {
  submitted.value = false;
  paycheck_form.value = dataTem;
  if (paycheck_form.value.countryside)
    paycheck_form.value.countryside_fake = paycheck_form.value.countryside;
  if (paycheck_form.value.is_default) {
    checkIsmain.value = false;
  } else {
    checkIsmain.value = true;
  }
  headerDialog.value = "Sửa thông tin phiếu lương";
  isSaveTem.value = true;
  displayBasic.value = true;
};
//Xóa bản ghi
const delTem = (Tem) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá bản ghi này không!",
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
          .delete(baseURL + "/api/hrm_paycheck_form/delete_hrm_paycheck_form", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Tem != null ? [Tem.paycheck_form_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá thông tin phiếu lương thành công!");
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

//Sort
const onSort = (event) => {
  options.value.PageNo = 0;

  if (event.sortField == null) {
    isDynamicSQL.value = false;
    loadData(true);
  } else {
    options.value.sort =
      event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
    if (event.sortField == "STT") {
      options.value.sort =
        "is_order" + (event.sortOrder == 1 ? " ASC" : " DESC");
    }
    isDynamicSQL.value = true;
    loadDataSQL();
  }
};
const checkFilter = ref(false);
const filterSQL = ref([]);
const isFirst = ref(true);
const loadDataSQL = () => {
  datalists.value = [];

  let data = {
    id: "paycheck_form_id",
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
    .post(baseURL + "/api/hrm_ca_SQL/Filter_hrm_paycheck_form", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.STT = options.value.PageNo * options.value.PageSize + i + 1;
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
const searchStamp = (event) => {
  if (event.code == "Enter") {
    if (options.value.SearchText == "") {
      isDynamicSQL.value = false;
      options.value.loading = true;
      loadData(true);
    } else {
      isDynamicSQL.value = true;
      options.value.loading = true;
      loadData(true);
    }
  }
};
const refreshStamp = () => {
  options.value.SearchText = null;
  filterTrangthai.value = null;
  options.value.loading = true;
  selectedStamps.value = [];
  isDynamicSQL.value = false;
  filterSQL.value = [];
  loadData(true);
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
  options.value.id = null;
  isDynamicSQL.value = true;
  loadDataSQL();
};
//Checkbox
const onCheckBox = (value, check, checkIsmain) => {
  if (check) {
    let data = {
      IntID: value.paycheck_form_id,
      TextID: value.paycheck_form_id + "",
      IntTrangthai: 1,
      BitTrangthai: value.status,
    };
    axios
      .put(
        baseURL + "/api/hrm_paycheck_form/update_s_hrm_paycheck_form",
        data,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa trạng thái thông tin phiếu lương thành công!");
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
  } else {
    let data1 = {
      IntID: value.paycheck_form_id,
      TextID: value.paycheck_form_id + "",
      BitMain: value.is_default,
    };
    axios
      .put(
        baseURL + "/api/hrm_paycheck_form/Update_DefaultStamp",
        data1,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa trạng thái thông tin phiếu lương thành công!");
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
  }
};
//Xóa nhiều
const deleteList = () => {
  let listId = new Array(selectedStamps.value.length);
  let checkD = false;
  selectedStamps.value.forEach((item) => {
    if (item.is_default) {
      toast.error("Không được xóa thông tin phiếu lương mặc định!");
      checkD = true;
      return;
    }
  });
  if (!checkD) {
    swal
      .fire({
        title: "Thông báo",
        text: "Bạn có muốn xoá thông tin phiếu lương này không!",
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

          selectedStamps.value.forEach((item) => {
            listId.push(item.paycheck_form_id);
          });
          axios
            .delete(
              baseURL + "/api/hrm_paycheck_form/delete_hrm_paycheck_form",
              {
                headers: { Authorization: `Bearer ${store.getters.token}` },
                data: listId != null ? listId : 1,
              }
            )
            .then((response) => {
              swal.close();
              if (response.data.err != "1") {
                swal.close();
                toast.success("Xoá thông tin phiếu lương thành công!");
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
                  title: "Error!",
                  text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                  icon: "error",
                  confirmButtonText: "OK",
                });
              }
            });
        }
      });
  }
};

//Filter
const trangThai = ref([
  { name: "Hiển thị", code: 1 },
  { name: "Không hiển thị", code: 0 },
]);

const filterTrangthai = ref();

const reFilterEmail = () => {
  filterTrangthai.value = null;
  isDynamicSQL.value = false;
  checkFilter.value = false;
  filterSQL.value = [];
  options.value.SearchText = null;
  op.value.hide();
  loadData(true);
};
const filterFileds = () => {
  filterSQL.value = [];
  checkFilter.value = true;
  let filterS = {
    filterconstraints: [{ value: filterTrangthai.value, matchMode: "equals" }],
    filteroperator: "and",
    key: "status",
  };
  filterSQL.value.push(filterS);
  loadDataSQL();
};
watch(selectedStamps, () => {
  if (selectedStamps.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});
const op = ref();
const toggle = (event) => {
  op.value.toggle(event);
};
const listDataUsersSave = ref([]);
const listDataUsers = ref([]);
const loadUserProfiles = () => {
  listDataUsers.value = [];

  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_profile_list_filter",
            par: [
              { par: "search", va: null },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "work_position_id", va: null },
              { par: "position_id", va: null },
              { par: "department_id", va: null },
              { par: "status", va: 1 },
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
        listDataUsers.value.push({
          profile_user_name: element.profile_user_name,
          code: {
            profile_id: element.profile_id,
            avatar: element.avatar,
            profile_user_name: element.profile_user_name,
            department_name: element.department_name,
            department_id: element.department_id,
            work_position_name: element.work_position_name,
            position_name: element.position_name,
            position_id: element.position_id,
            work_position_id: element.work_position_id,
          },
          avatar: element.avatar,
          department_name: element.department_name,
          department_id: element.department_id,
          work_position_name: element.work_position_name,
          position_name: element.position_name,
          profile_id: element.profile_id,
          organization_id: element.organization_id,
        });
      });
      listDataUsersSave.value = [...listDataUsers.value];
    })
    .catch((error) => {
      console.log(error);

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};




const displayPaycheck=ref(false);
 
const openDetails=(data)=>{

  paycheck_form.value=data;
 
  displayPaycheck.value=true;
}
onMounted(() => {
  if (!checkURL(window.location.pathname, store.getters.listModule)) {
    //router.back();
  }
  loadData(true);
  initTudien();
  loadUserProfiles();
  return {
    datalists,
    options,
    onPage,
    loadData,
    loadCount,
    openBasic,
    closeDialog,
    basedomainURL,

    saveData,
    isFirst,
    searchStamp,
    onCheckBox,
    selectedStamps,
    deleteList,
  };
});
</script>
    <template>
  <div class="main-layout true flex-grow-1 p-2 pb-0 pr-0">
    <DataTable
      @page="onPage($event)"
      @sort="onSort($event)"
      @filter="onFilter($event)"
      v-model:filters="filters"
      filterDisplay="menu"
      filterMode="lenient"
      :filters="filters"
      :scrollable="true"
      scrollHeight="flex"
      :showGridlines="true"
      columnResizeMode="fit"
      :lazy="true"
      :totalRecords="options.totalRecords"
      :loading="options.loading"
      :reorderableColumns="true"
      :value="datalists"
      removableSort
      v-model:rows="options.PageSize"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      :paginator="true"
      dataKey="paycheck_form_id"
      responsiveLayout="scroll"
      v-model:selection="selectedStamps"
      :row-hover="true"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-file-pdf"></i> Danh sách thông tin phiếu lương ({{
            options.totalRecords
          }})
        </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                v-model="options.SearchText"
                @keyup="searchStamp"
                type="text"
                spellcheck="false"
                placeholder="Tìm kiếm"
              />
              <Button
                type="button"
                class="ml-2"
                icon="pi pi-filter"
                @click="toggle"
                aria:haspopup="true"
                aria-controls="overlay_panel"
                v-tooltip="'Bộ lọc'"
                :class="
                  filterTrangthai != null && checkFilter
                    ? ''
                    : 'p-button-secondary p-button-outlined'
                "
              />
              <OverlayPanel
                ref="op"
                appendTo="body"
                class="p-0 m-0"
                :showCloseIcon="false"
                id="overlay_panel"
                style="width: 300px"
              >
                <div class="grid formgrid m-0">
                  <div class="flex field col-12 p-0">
                    <div
                      class="col-4 text-left pt-2 p-0"
                      style="text-align: left"
                    >
                      Trạng thái
                    </div>
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
                          @click="reFilterEmail"
                          class="p-button-outlined"
                          label="Xóa"
                        ></Button>
                      </template>
                      <template #end>
                        <Button @click="filterFileds" label="Lọc"></Button>
                      </template>
                    </Toolbar>
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
              @click="openBasic('Thêm thông tin phiếu lương')"
              label="Thêm mới"
              icon="pi pi-plus"
              class="mr-2"
            />
            <Button
              @click="refreshStamp"
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
              class="align-items-center justify-content-center text-center"
              headerStyle="text-align:center;max-width:70px;height:50px"
              bodyStyle="text-align:center;max-width:70px"
              selectionMode="multiple"
            >
            </Column>

      <Column
        field="STT"
        header="STT"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:70px;height:50px"
        bodyStyle="text-align:center;max-width:70px"
        :sortable="true"
      ></Column>

      <Column
        field="profile_user_name"
        header="Họ và tên "
        :sortable="true"
        headerStyle="text-align:left;height:50px"
        bodyStyle="text-align:left"
      >
        <template #filter="{ filterModel }">
          <InputText
            type="text"
            v-model="filterModel.value"
            class="p-column-filter"
            placeholder="Từ khoá"
          />
        </template>

        <template #body="data">
          <div class="flex w-full align-items-center pr-2">
                  <Avatar
                    v-bind:label="
                      data.data.avatar
                        ? ''
                        : data.data.profile_user_name.substring(
                            data.data.profile_user_name.lastIndexOf(' ') +
                              1,
                            data.data.profile_user_name.lastIndexOf(' ') +
                              2
                          )
                    "
                    :image="basedomainURL + data.data.avatar"
                    size="small"
                    style="color:#fff"
                    :style="
                      data.data.avatar
                        ? 'background-color: #2196f3'
                        : 'background:' +
                          bgColor[data.data.profile_user_name.length % 7]
                    "
                    shape="circle"
                    @error="
                      $event.target.src =
                        basedomainURL + '/Portals/Image/nouser1.png'
                    "
                  />
                  <div class="px-2">
                    {{ data.data.profile_user_name }}
                  </div>
                </div>
        </template>
      </Column>
      <Column
        field="declare_name"
        header="Phiếu lương "
     
        headerStyle="text-align:center;max-width:450px;height:50px"
              bodyStyle="text-align:center;max-width:450px"
      >
      
      </Column>
      <Column
              field="form_training"
              header="Tháng"
              headerStyle="text-align:center;max-width:150px;height:50px"
              bodyStyle="text-align:center;max-width:150px"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="data">
                <div v-if="data.data.paycheck_form_date">
                  {{
                    moment(new Date(data.data.paycheck_form_date)).format(
                      "MM/YYYY"
                    )
                  }}
                </div>
              </template>
            </Column>
      <Column
        field="status"
        header="Trạng thái"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <Checkbox
            :disabled="
              !(
                store.state.user.is_super == true ||
                store.state.user.user_id == data.data.created_by ||
                (store.state.user.role_id == 'admin' &&
                  store.state.user.organization_id == data.data.organization_id)
              )
            "
            :binary="true"
            v-model="data.data.status"
            @click="onCheckBox(data.data, true, true)"
          /> </template
      ></Column>
      
      <Column
        header="Chức năng"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:200px;height:50px"
        bodyStyle="text-align:center;max-width:200px"
      >
        <template #body="Tem">
          <div
            v-if="
              store.state.user.is_super == true ||
              store.state.user.user_id == Tem.data.created_by ||
              (store.state.user.role_id == 'admin' &&
                store.state.user.organization_id == Tem.data.organization_id)
            "
          >
          <Button
              @click="openDetails(Tem.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-info"
              v-tooltip.top="'Chi tiết'"
            ></Button>
            <Button
              @click="editTem(Tem.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              v-tooltip.top="'Sửa'"
            ></Button>
            <Button
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-trash"
              @click="delTem(Tem.data)"
              v-tooltip.top="'Xóa'"
            ></Button>
          </div>
        </template>
      </Column>
      <template #empty>
        <div
          class="align-items-center justify-content-center p-4 text-center m-auto"
          v-if="!isFirst"
        >
          <img src="../../../assets/background/nodata.png" height="144" />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </DataTable>
    <div v-if="displayPaycheck">
    <paycheck_form_dialog     :paycheck_form="paycheck_form" :displayPaycheck="displayPaycheck" :closeDialogDialog="closeDialogDialog" />
  </div>
 
  </div>

  <Dialog
    :header="headerDialog"
    v-model:visible="displayBasic"
    :style="{ width: '35vw' }"
    :closable="true"
    :modal="true"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 flex md:col-12 align-items-center">
          <div class="col-2 text-left p-0">
            Nhân viên <span class="redsao">(*)</span>
          </div>
          <Dropdown
            :options="listDataUsers"
            :filter="true"
            :showClear="true"
            :editable="false"
            optionLabel="name"
            optionValue="code"
            placeholder="Chọn nhân viên nhận phiếu lương"
            v-model="paycheck_form.user_id_fake"
            class="col-10"
          >
            <template #value="slotProps">
              <div class=" " v-if="slotProps.value">
                <div class="flex w-full align-items-center pr-2">
                  <Avatar   style="color:#fff"
                    v-bind:label="
                      slotProps.value.avatar
                        ? ''
                        : slotProps.value.profile_user_name.substring(
                            slotProps.value.profile_user_name.lastIndexOf(' ') +
                              1,
                            slotProps.value.profile_user_name.lastIndexOf(' ') +
                              2
                          )
                    "
                    :image="basedomainURL + slotProps.value.avatar"
                    size="small"
                    :style="
                      slotProps.value.avatar
                        ? 'background-color: #2196f3'
                        : 'background:' +
                          bgColor[slotProps.value.profile_user_name.length % 7]
                    "
                    shape="circle"
                    @error="
                      $event.target.src =
                        basedomainURL + '/Portals/Image/nouser1.png'
                    "
                  />
                  <div class="px-2">
                    {{ slotProps.value.profile_user_name }}
                  </div>
                </div>
              </div>
              <span v-else> {{ slotProps.placeholder }} </span>
            </template>
            <template #option="slotProps">
              <div v-if="slotProps.option" class="flex">
                <div class="format-center">
                  <Avatar   style="color:#fff"
                    v-bind:label="
                      slotProps.option.avatar
                        ? ''
                        : slotProps.option.profile_user_name.substring(
                            slotProps.option.profile_user_name.lastIndexOf(
                              ' '
                            ) + 1,
                            slotProps.option.profile_user_name.lastIndexOf(
                              ' '
                            ) + 2
                          )
                    "
                    :image="basedomainURL + slotProps.option.avatar"
                    class="w-3rem"
                    size="large"
                    :style="
                      slotProps.option.avatar
                        ? 'background-color: #2196f3'
                        : 'background:' +
                          bgColor[slotProps.option.profile_user_name.length % 7]
                    "
                    shape="circle"
                    @error="
                      $event.target.src =
                        basedomainURL + '/Portals/Image/nouser1.png'
                    "
                  />
                </div>
                <div class="ml-3">
                  <div class="mb-1">
                    {{ slotProps.option.profile_user_name }}
                  </div>
                  <div class="description">
                    <div>{{ slotProps.option.position_name }}</div>
                    <!-- <div>{{ slotProps.option.department_name }}</div> -->
                  </div>
                </div>
              </div>
              <span v-else> Chưa có dữ liệu </span>
            </template>
          </Dropdown>
        </div>
        <div class="col-12 field md:col-12 flex">
          <div class="col-12 md:col-12 p-0 align-items-center flex">
            <div class="col-2 text-left p-0">
              Phiếu lương <span class="redsao">(*)</span>
            </div>

            <Dropdown
              :options="listPaycheck"
              :filter="true"
              :showClear="true"
              :editable="false"
              optionLabel="name"
              optionValue="code"
              placeholder="Chọn phiếu lương"
              v-model="paycheck_form.declare_id"
              @filter="onFilterPaycheck($event)"
              class="col-10"
            >
            </Dropdown>
          </div>
        </div>

        <div class="col-12 field md:col-12 flex">
          <div class="col-6 md:col-6 p-0 align-items-center flex">
            <div class="col-4 text-left p-0">Tháng</div>
            <Calendar
              :showIcon="true"
              class="col-8 p-0"
              autocomplete="off"
              view="month"
              dateFormat="mm/yy"
              v-model="paycheck_form.paycheck_form_date"
              placeholder="Chọn tháng"
            />
          </div>
          <div class="col-3 md:col-3 p-0 align-items-center flex">
            <div class="col-4 text-center p-0 pl-2">STT</div>
            <InputNumber
              v-model="paycheck_form.is_order"
              class="col-8 ip36 p-0"
            />
          </div>
          <div class="col-3 md:col-3 p-0 align-items-center flex">
            <div class="col-8 text-center p-0">Trạng thái</div>
            <InputSwitch
              class="w-4rem lck-checked"
              v-model="paycheck_form.status"
            />
          </div>
        </div>
        <div class="col-12 flex  ">
          <div class="col-2 text-left p-0">File đính kèm</div>
          <div class="col-10 p-0">
            <FileUpload
              chooseLabel="Chọn File"
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
        <div class="col-12 p-0">
          <div
            class="p-0 w-full flex"
            v-for="(item, index) in listFilesS"
            :key="index"
          >
            <div
              class="p-0 surface-50"
              style="width: 100%; border-radius: 10px"
            >
              <Toolbar class="w-full py-3">
                <template #start>
                  <div class="flex">
                    <div v-if="item.is_image" class="align-items-center flex">
                      <Image
                        :src="basedomainURL + item.file_path"
                        :alt="item.file_name"
                        width="70"
                        height="50"
                        style="
                          object-fit: contain;
                          border: 1px solid #ccc;
                          width: 70px;
                          height: 50px;
                        "
                        preview
                        class="pr-2"
                      />
                      <div class="ml-2">
                        {{ item.file_name }}
                      </div>
                    </div>
                    <div v-else>
                      <a
                        :href="basedomainURL + item.file_path"
                        download
                        class="w-full no-underline cursor-pointer"
                      >
                        <div class="align-items-center flex">
                          <div>
                            <img
                              :src="
                                basedomainURL +
                                '/Portals/Image/file/' +
                                item.file_path.substring(
                                  item.file_path.lastIndexOf('.') + 1
                                ) +
                                '.png'
                              "
                              style="
                                width: 70px;
                                height: 50px;
                                object-fit: contain;
                              "
                              :alt="item.file_name"
                            />
                          </div>
                          <div class="ml-2">
                            {{ item.file_name }}
                          </div>
                        </div>
                      </a>
                    </div>
                  </div>
                </template>
                <template #end>
                  <Button
                    icon="pi pi-times"
                    class="p-button-rounded p-button-danger"
                    @click="deleteFileH(item)"
                  />
                </template>
              </Toolbar>
            </div>
          </div>
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
        @click="saveData( )"
        autofocus
      />
    </template>
  </Dialog> 
  





</template>
    
    <style scoped>
.inputanh {
  border: 1px solid #ccc;
  width: 8rem;
  height: 8rem;
  cursor: pointer;
  padding: 0.063rem;
  display: block;
  margin-left: auto;
  margin-right: auto;
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
    