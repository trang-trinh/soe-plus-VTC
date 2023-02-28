<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../../util/function.js";
//Khai báo
import moment from "moment";
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
  declare_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
const rules = {
  declare_name: {
    required,
    $errors: [
      {
        $property: "declare_name",
        $validator: "required",
        $message: "Tên phiếu lương không được để trống!",
      },
    ],
  },
};
const rulesP = {
  paycheck_name: {
    required,
    $errors: [
      {
        $property: "paycheck_name",
        $validator: "required",
        $message: "Tên phiếu lương  không được để trống!",
      },
    ],
  }, 
};
//Lấy số bản ghi
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_declare_count",
            par: [
               { par: "search", va: options.value.search },
              { par: "type", va: options.value.typeDeclare },
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
    .catch((error) => { });
};
//Lấy dữ liệu declare
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
        baseURL + "/api/hrm_ca_SQL/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "hrm_declare_list",
              par: [
              { par: "search", va: options.value.search },
              { par: "type", va: options.value.typeDeclare },
                { par: "pageno", va: options.value.PageNo },
                { par: "pagesize", va: options.value.PageSize },
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
        });
        datalists.value = data;
        declare.value = data[0];
        options.value.loading = false;

        if (declare.value != null)
          loadDataDetails(true);
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
  }
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

    options.value.id = datalists.value[datalists.value.length - 1].declare_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].declare_id;
    options.value.IsNext = false;
  }
  options.value.PageNo = event.page;
  loadData(true);
};

const onPageP = (event) => {
  if (event.rows != options.value.pagesizeP) {
    options.value.pagesizeP = event.rows;
  }
  if (event.page == 0) {
    //Trang đầu
    options.value.id = null;
    options.value.IsNext = true;
  } else if (event.page > options.value.pagenoP + 1) {
    //Trang cuối
    options.value.id = -1;
    options.value.IsNext = false;
  } else if (event.page > options.value.pagenoP) {
    //Trang sau

    options.value.id = datalistsDetails.value[datalistsDetails.value.length - 1].paycheck_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.pagenoP) {
    //Trang trước
    options.value.id = datalists.value[0].paycheck_id;
    options.value.IsNext = false;
  }
  options.value.pagenoP = event.page;
  loadDataDetails(true);
};
const declare = ref({
  declare_name: "",
  emote_file: "",
  status: true,
  is_default: false,
  is_order: 1,
});
const paycheck = ref({
  paycheck_name: "",
  paycheck_code: "",
  status: true,
  is_default: false,
  is_order: 1,
});

const selectedStamps = ref();
const submitted = ref(false);
const submittedP = ref(false);
const v$ = useVuelidate(rules, declare);
const va$ = useVuelidate(rulesP, paycheck);
const isSaveTem = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = baseURL;
const checkDelList = ref(false);

const options = ref({
  IsNext: true,
  sort: "created_date",
  SearchText: "",
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
  loadingP: true,
  pagenoP: 0,
  pagesizeP: 20,
  searchP: "",
  sortP: "created_date"
});

//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);
const openBasic = (str) => {
  submitted.value = false;
  declare.value = {
  
    declare_name: "",
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
const openBasicP = (str) => {
  if(declare.value){
  selectCapcha.value = null;
  submittedP.value = false;
  paycheck.value = {
    paycheck_name: "",
    declare_id:declare.value.declare_id,
    status: true,
    paycheck_type:1,
    is_order: sttPaycheck.value,
    organization_id: store.getters.user.organization_id,
    parent_id:null
  };
  isSaveTem.value = false;
  headerDialogP.value = str;
  displayBasicP.value = true;
}
}
const headerDialogP =ref("");
  const closeDialogP = () => {
    paycheck.value = {
    paycheck_name: "",
  
    status: true,
  
 
    organization_id: store.getters.user.organization_id,
  }
  displayBasicP.value = false;
  loadDataDetails(true);
}

const closeDialog = () => {
  declare.value = {
    declare_name: "",
    emote_file: "",
    status: true,
    is_default: false,
    is_order: 1,
  };

  displayBasic.value = false;
  loadData(true);
};

//Thêm bản ghi

const onChangeParent = () => {
  datalistsDetails.value;
  let arw = null;
  if (selectCapcha.value)
    Object.keys(selectCapcha.value).forEach((key) => {
      arw = key;
      return;
    });
  paycheck.value.is_order = arw
    ? datalistsDetails.value.filter((x) => x.parent_id == arw).length + 1
    : 1;
};

const addWarehouseChild = (data) => {
  var stt=1;
  if(declare.value){
     
    if(datalistsDetails.value.length>0){
      var stv=datalistsDetails.value.find(x=>x.key==data.paycheck_id);
      if(stv!=null){
        if(stv.children){
          stt=stv.children.length+1;
        }
      }
    }
   
  submittedP.value = false;
  selectCapcha.value = {};
  selectCapcha.value[data.paycheck_id] = true;
  paycheck.value = {
    declare_id:declare.value.declare_id,
    paycheck_name: null,
    paycheck_type:2,
    is_order: stt,
    status: true,
    organization_id: store.getters.user.organization_id,
    parent_id: -1,
  };
  isSaveTem.value=false;
  headerDialogP.value = "Thêm mới";
  displayBasicP.value = true;
}
};

const editWarehouse = (dataPlace) => {
  submittedP.value = false;
  paycheck.value = dataPlace;

  selectCapcha.value = {};
  selectCapcha.value[dataPlace.parent_id] = true;
  paycheck.value.organization_id =
  store.state.user.organization_id;
  headerDialogP.value = "Sửa đầu mục";
  isSaveTem.value = true;
 
  displayBasicP.value = true;
};
//Xóa bản ghi
const delWarehouse = (Warehouse) => {
  var strW=Warehouse.paycheck_type==1?"Bạn có muốn xoá đầu mục này không!":"Bạn có muốn xoá chỉ tiêu này không!"
  swal
    .fire({
      title: "Thông báo",
      text: strW,
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
          .delete(baseURL + "/api/hrm_paycheck/delete_hrm_paycheck", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Warehouse != null ? [Warehouse.paycheck_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err == "0") {
              swal.close();
              toast.success("Xoá bản ghi thành công!");
             
              loadDataDetails(true);
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
             
          });
      }
    });
};

const isChirlden = ref(false);
const selectCapcha = ref();
const savePaycheck = (isFormValid) => {
  submittedP.value = true;
  if (!isFormValid) {
    return;
  }

  if (paycheck.value.paycheck_name.length > 250) {
    swal.fire({
      title: "Error!",
      text: "Tên đầu mục không được vượt quá 250 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  } 
   
  if ((paycheck.value.paycheck_code==null || selectCapcha.value==null) && paycheck.value.paycheck_type==2) {
   
    return;
  }

  let arw = null;
    
  if (selectCapcha.value)
    Object.keys(selectCapcha.value).forEach((key) => {
      arw = key;
      return;
    });
    if(arw!=null)
    paycheck.value.parent_id = Number(arw);
 else if ((arw = "" || arw == -1)) paycheck.value.parent_id = null;
  let formData = new FormData();

  
  formData.append("hrm_paycheck", JSON.stringify(paycheck.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
   
  if (!isSaveTem.value) {
    axios
      .post(baseURL + "/api/hrm_paycheck/add_hrm_paycheck", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm đầu mục thành công!");
    

          closeDialogP();
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
      .put(baseURL + "/api/hrm_paycheck/update_hrm_paycheck", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa đầu mục thành công!");

    

closeDialogP();
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
const sttStamp = ref(1);
const sttPaycheck = ref(1);
const saveData = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }

  if (declare.value.declare_name.length > 250) {
    swal.fire({
      title: "Error!",
      text: "Tên phiếu lương  không được vượt quá 250 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  let formData = new FormData();

  if (declare.value.countryside_fake)
    declare.value.countryside = declare.value.countryside_fake;
  formData.append("hrm_declare", JSON.stringify(declare.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (!isSaveTem.value) {
    axios
      .post(baseURL + "/api/hrm_declare/add_hrm_declare", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm phiếu lương thành công!");
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
    axios
      .put(baseURL + "/api/hrm_declare/update_hrm_declare", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa phiếu lương thành công!");

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
const menuButMores = ref();
const listDeclareType = ref([
  { name: "Người lao động", code: 1 },
  { name: "Người quản lý", code: 2 },
  { name: "Khác", code: 3 },
]);
const listPaycheckType = ref([
  { name: "Đầu mục", code: 1 },
  { name: "Chỉ tiêu", code: 2 },
  
]);

const listUnitType = ref([
  { name: "Cái", code: 1 },
  { name: "Chiếc", code: 2 },
  
]);

const toggleMores = (event, item) => {
  declare.value = item;
  menuButMores.value.toggle(event);
  //selectedNodes.value = item;
};
const itemButMores = ref([
  {
    label: "Hiệu chỉnh nội dung",
    icon: "pi pi-pencil",
    command: (event) => {
     ;
    },
  },
  {
    label: "Xoá",
    icon: "pi pi-trash",
    command: (event) => {
     
    },
  },
]);
const checkIsmain = ref(true);
//Sửa bản ghi
const editTem = (dataTem) => {
  submitted.value = false;
  declare.value = dataTem;
  if (declare.value.countryside)
    declare.value.countryside_fake = declare.value.countryside;
  if (declare.value.is_default) {
    checkIsmain.value = false;
  } else {
    checkIsmain.value = true;
  }
  headerDialog.value = "Sửa phiếu lương ";
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
          .delete(baseURL + "/api/hrm_declare/delete_hrm_declare", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Tem != null ? [Tem.declare_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá phiếu lương thành công!");
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
    id: "declare_id",
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
    .post(baseURL + "/api/hrm_ca_SQL/Filter_hrm_declare", data, config)
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
    if (options.value.search == "") {
     
      options.value.loading = true;
      loadData(true);
    } else {
    
      options.value.loading = true;
      loadData(true);
    }
  }
};

const searchPaycheck = (event) => {
  if (event.code == "Enter") {
    if (options.value.SearchText == "") {
     
      options.value.loading = true;
      loadData(true);
    } else {
    
      options.value.loading = true;
      loadData(true);
    }
  }
};

const refreshStamp = () => {
  options.value.SearchText = null;
  filterTrangthai.value = null;
  options.value.loadingP = true;
  selectedStamps.value = [];
  isDynamicSQL.value = false;
  filterSQL.value = [];
  options.value.typeDeclare =null;
  options.value.search =null;
  loadData(true);
   
};
const changeViewDeclare=(value)=>{
  declare.value=value;
  loadDataDetails(true);
}
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
const selectedDeclareId = ref({ icon: 'fa-solid fa-users', value: 'Tất cả', code: 0 });
const onFilterDeclacre = () => {
   
  if(selectedDeclareId.value.code==0)
  options.value.typeDeclare =null;
  else
  options.value.typeDeclare =selectedDeclareId.value.code;
  loadData(true);
}
const justifyOptions = ref([
  { icon: 'fa-solid fa-users', value: 'Tất cả', code: 0 },
  { icon: 'fa-solid fa-user-gear', value: 'Người lao động', code: 1 },
  { icon: 'fa-solid fa-user-tie', value: 'Người quản lý', code: 2 },
  { icon: 'fa-solid fa-user-large', value: 'Khác', code: 3 },

]);
//Checkbox
const onCheckBox = (value, check, checkIsmain) => {
 
    let data = {
      IntID: value.paycheck_id,
      TextID: value.paycheck_id + "",
      IntTrangthai: 1,
      BitTrangthai: value.status,
    };
    axios
      .put(baseURL + "/api/hrm_paycheck/update_s_hrm_paycheck", data, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa trạng thái thành công!");
        
          closeDialogP();
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
//Xóa nhiều
const deleteList = () => {
  let listId = new Array(selectedStamps.value.length);
  let checkD = false;
  selectedStamps.value.forEach((item) => {
    if (item.is_default) {
      toast.error("Không được xóa phiếu lương mặc định!");
      checkD = true;
      return;
    }
  });
  if (!checkD) {
    swal
      .fire({
        title: "Thông báo",
        text: "Bạn có muốn xoá phiếu lương này không!",
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
            listId.push(item.declare_id);
          });
          axios
            .delete(baseURL + "/api/hrm_declare/delete_hrm_declare", {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: listId != null ? listId : 1,
            })
            .then((response) => {
              swal.close();
              if (response.data.err != "1") {
                swal.close();
                toast.success("Xoá phiếu lương thành công!");
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
 






///////////////////////////////////////////////////////////

const op = ref();
const toggle = (event) => {
  op.value.toggle(event);
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
      baseURL + "/api/Excel/ExportTreeExcelWithLogo",
      {
        excelname: "THÔNG TIN PHIẾU LƯƠNG " +declare.value.declare_name,
        proc: "hrm_paycheck_list_export",
        par: [
          { par: "search", va: options.value.SearchText },
            { par: "declare_id", va: declare.value.declare_id },
            { par: "pageno", va: options.value.pagenoP },
            { par: "pagesize", va: options.value.pagesizeP },
            { par: "user_id", va: store.getters.user.user_id },
            { par: "status", va: null },
          ],
      },
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();

        toast.success("Kết xuất Data thành công!");
        debugger
        if (response.data.path != null) {
          let pathReplace = response.data.path
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
        store.commit("gologout");
      }
    });
};

const displayBasicP = ref(false);
const datalistsDetails = ref();
 
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
  selectedNodes.value.push(node.data.hrm_declare_id);
};
const onNodeUnselect = (node) => {
  selectedNodes.value.splice(
    selectedNodes.value.indexOf(node.data.hrm_declare_id),
    1
  );
};
const selectedNodes = ref([]);
const treemodules = ref();
const selectedWarehouses = ref();

const loadCountDetails = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_paycheck_count",
            par: [
            { par: "search", va: options.value.SearchText },
              { par: "declare_id", va: declare.value.declare_id },
              { par: "user_id", va: store.state.user.user_id },
              { par: "status", va: null },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      }, config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      let data1 = JSON.parse(response.data.data)[1];
 
      if (data.length > 0) {
        options.value.totalRecordsP = data[0].totalRecords;
      }
      if (data1.length > 0) {
        options.value.totalRecordsPage = data1[0].totalRecordsPage;
        sttPaycheck.value= options.value.totalRecordsPage+1;
      }
    })
    .catch(() => {

    });
};
const loadDataDetails = (rf) => {
  if (rf) {

    if (rf) {
      loadCountDetails();
    }
    axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "hrm_paycheck_list",
          par: [
          { par: "search", va: options.value.SearchText },
            { par: "declare_id", va: declare.value.declare_id },
            { par: "pageno", va: options.value.pagenoP },
            { par: "pagesize", va: options.value.pagesizeP },
            { par: "user_id", va: store.getters.user.user_id },
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
          "paycheck_id",
          "paycheck_name",
          "cấp cha"
        );
        treemodules.value = obj.arrtreeChils;

        datalistsDetails.value = obj.arrChils;
        options.value.loadingP = false;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");

        options.value.loading = false;

      });
  }
};













onMounted(() => {
  if (!checkURL(window.location.pathname, store.getters.listModule)) {
    //router.back();
  }
  // loadDataDetails(true);
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
  <div>
    <div class="p-3     ">
      <h3 class="module-title m-0 ">
        <i class="pi pi-book"></i> Danh sách phiếu lương 
      </h3>

    </div>
    <div>
      <Splitter class=" h-full w-full pb-0 pr-0">
        <SplitterPanel :size="35" class=" ">
          <div>
            <div>
              <Toolbar>
              <template #start>
                <span class="p-input-icon-left">
                 <i class="pi pi-search" />
                  <InputText v-model="options.search" @keyup="searchStamp" type="text" spellcheck="false"      placeholder="Tìm kiếm phiếu lương" />
                </span>
              </template> 
                <template #end>
                  <Button v-tooltip.top="'Thêm mới phiếu lương'" @click="openBasic('Thêm mới')" label="Thêm mới" icon="pi pi-plus" class="mr-2" />
              
                </template>
              </Toolbar>
            </div>
            <div class=" flex">
          



            </div>
            <div   style="border-top:2px solid #dee2e6">

              <div class="w-full d-lang-table  mx-2">
                <DataView :value="datalists" :loading="options.loading" :paginator="true" currentPageReportTemplate=""
                  :rows="options.PageSize" :totalRecords="totalRecords" :rowHover="true" :showGridlines="true"
                  :pageLinks="5" class="w-full h-full ptable p-datatable-sm flex flex-column" :lazy="true" layout="list"
                  responsiveLayout="scroll" :scrollable="true">
                  <template #list="slotProps">
                    <div class="grid h-full  w-full  formgrid"
   
   
                    :class="declare.declare_id==slotProps.data.declare_id?
                    'bg-d-selected':''"
                    @click="changeViewDeclare(slotProps.data)"
                    >
                      <div class="col-12  mb-2 p-2  flex align-items-center"
                      
                      >
                        
                        <div class="px-2 col-9 ">
                          <div class="  font-bold text-lg">
                            {{ slotProps.data.declare_name }}
                          </div>
                          <div class="pt-1 ">
                            Loại phiếu: {{ slotProps.data.declare_type == 1 ? 'Người lao động':slotProps.data.declare_type==2?'Người quản lý':'Khác' }}
                          </div>
                          <div class="text-sm ">
                            Người lập: {{ slotProps.data.full_name }} | Ngày lập {{ moment(new
                              Date(slotProps.data.created_date)).format("DD/MM/YYYY") }}
                          </div>
                        </div>
                        
                        <div class="pr-2 col-3 flex">
                          <Toolbar class="w-full p-0 m-0 custoolbar ">
            <template #end>
                          <Button @click=" editTem(slotProps.data, 'Chỉnh sửa hợp đồng')"
                    class="p-button-rounded p-button-secondary p-button-outlined mx-1" type="button" icon="pi pi-pencil"
                    v-tooltip.top="'Sửa'"></Button>
                  <Button @click=" delTem(slotProps.data)"
                    class="p-button-rounded p-button-secondary p-button-outlined mx-1" type="button" icon="pi pi-trash"
                    v-tooltip.top="'Xóa'"></Button>

</template>
</Toolbar>


                      
                        </div>
                      </div>

                    </div>
                  </template>


                </DataView>
              </div>
            </div>
          </div>
        </SplitterPanel>
        <SplitterPanel :size="65"  >
          <div class="d-lang-table-r">
          <Toolbar class="w-full  ">
            <template #start>
                  <span class="p-input-icon-left">
                    <i class="pi pi-search" />
                    <InputText v-model="options.SearchText" @keyup="searchPaycheck" type="text" spellcheck="false"
                      placeholder="Tìm kiếm đầu mục" />

                 

                    
                  </span>
                </template>

            <template #end>
            <Button v-tooltip.top="'Thêm mới đầu mục'"  @click="openBasicP('Thêm mới')" label="Thêm mới" icon="pi pi-plus" class="mx-2" />

              <Button @click="refreshStamp" class="mr-2 p-button-outlined p-button-secondary" icon="pi pi-refresh"
                v-tooltip="'Tải lại'" />
              <Button v-if="checkDelList" @click="deleteList()" label="Xóa" icon="pi pi-trash"
                class="mr-2 p-button-danger" />
              <Button label="Tiện ích" icon="pi pi-file-excel" class="mr-2 p-button-outlined p-button-secondary"
                @click="toggleExport" aria-haspopup="true" aria-controls="overlay_Export" />
              <Menu id="overlay_Export" ref="menuButs" :model="itemButs" :popup="true" />
            </template>
          </Toolbar>
          <TreeTable :value="datalistsDetails"
           v-model:selectionKeys="selectedWarehouses" 
           :loading="options.loadingP"
       
             :filters="filters" :showGridlines="true"
            class="p-treetable-sm"
            :paginator="true" :rows="options.pagesizeP"
            :rowHover="true" responsiveLayout="scroll"
             :lazy="true" :scrollable="true" scrollHeight="flex"
            @page="onPageP($event)"
           
              :totalRecords="options.totalRecordsPage"
            >

            <Column field="is_order" header="STT" class="align-items-center justify-content-center text-center font-bold"
              headerStyle="text-align:center;max-width:50px" bodyStyle="text-align:center;max-width:50px">
              <template #body="md">
                <div v-bind:class="md.node.data.status ? '' : 'text-error'">
                  {{ md.node.data.STT }}
                </div>
              </template>
            </Column> <Column field="type_order" header="Loại sắp xếp" class="align-items-center justify-content-center"
              headerStyle="text-align:center;max-width:70px" bodyStyle="text-align:center;max-width:70px"
              filterMatchMode="contains">
              <template #body="md">
                <div v-bind:class="md.node.data.parent_id==null ? 'font-bold ' : ' '">
                  {{ md.node.data.type_order }}
                </div>
              </template> 
            </Column>
            <Column field="paycheck_code" header="Mã số" headerStyle="text-align:center;max-width:120px"
              bodyStyle="text-align:center;max-width:120px" class="align-items-center justify-content-center text-center">
              <template #body="md">
                <div v-bind:class="md.node.data.parent_id==null ? 'font-bold ' : ' '">
                  {{ md.node.data.paycheck_code }}
                </div>
              </template>
            </Column>
            <Column field="paycheck_name"  header="Tên đầu mục" :expander="true">
              <template #body="md">
                <div v-bind:class="md.node.data.parent_id==null ? 'font-bold ' : ' '">
                  {{ md.node.data.paycheck_name }}
                </div>
              </template> </Column>
             
            <Column field="paycheck_unit" header="Đơn vị tính" class="align-items-center justify-content-center"
              headerStyle="text-align:center;max-width:100px" bodyStyle="text-align:center;max-width:100px"
              filterMatchMode="contains">
              <template #body="md">
                <div v-bind:class="md.node.data.parent_id==null ? 'font-bold ' : ' '">
                  {{ md.node.data.paycheck_unit }}
                </div>
              </template> 
            </Column>
           
            <Column field="status" header="Trạng thái" class="align-items-center justify-content-center"
              headerStyle="text-align:center;max-width:70px" bodyStyle="text-align:center;max-width:70px"
              filterMatchMode="contains">
              <template #body="data">
                <Checkbox :disabled="
                  !(
                    store.state.user.is_super == true ||
                    store.state.user.user_id == data.node.data.created_by ||
                    (store.state.user.role_id == 'admin' &&
                      store.state.user.organization_id ==
                      data.node.data.organization_id)
                  )
                " :binary="data.node.data.status" v-model="data.node.data.status"
                  @click="onCheckBox(data.node.data)" />
              </template>
            </Column>
            <Column field="status" header="Chức năng" class="align-items-center justify-content-center text-center"
              headerStyle="text-align:center;max-width:150px;height:50px" bodyStyle="text-align:center;max-width:150px">
              <template #body="data">
                <Button type="button" icon="pi pi-plus-circle"
                  class="p-button-rounded p-button-secondary p-button-outlined" v-tooltip.top="'Thêm chỉ tiêu'"
                  @click="addWarehouseChild(data.node.data)"></Button>
                <div v-if="
                  store.state.user.is_super == true ||
                  store.state.user.user_id == data.node.data.created_by ||
                  (store.state.user.role_id == 'admin' &&
                    store.state.user.organization_id ==
                    data.node.data.organization_id)
                ">
                  <Button @click="editWarehouse(data.node.data)"
                    class="p-button-rounded p-button-secondary p-button-outlined mx-1" type="button" icon="pi pi-pencil"
                    v-tooltip.top="'Sửa'"></Button>
                  <Button @click="delWarehouse(data.node.data, true)"
                    class="p-button-rounded p-button-secondary p-button-outlined mx-1" type="button" icon="pi pi-trash"
                    v-tooltip.top="'Xóa'"></Button>
                </div>
              </template>
            </Column>
            <template #empty>
              <div class="
                align-items-center
                justify-content-center
                p-4
                text-center
                m-auto
              " v-if="!isFirst">
                <img src="../../../assets/background/nodata.png" height="144" />
                <h3 class="m-1">Không có dữ liệu</h3>
              </div>
            </template>
          </TreeTable>
        </div>
        </SplitterPanel>
      </Splitter>
    </div>


    <Dialog :header="headerDialog" v-model:visible="displayBasic" :style="{ width: '35vw' }" :closable="true"
      :modal="true">
      <form>
        <div class="grid formgrid m-2">
          <div class="field col-12 md:col-12">
            <label class="col-3 text-left p-0">Tên phiếu lương  <span class="redsao">(*)</span></label>
            <InputText v-model="declare.declare_name" spellcheck="false" class="col-9 ip36 px-2" :class="{
              'p-invalid': v$.declare_name.$invalid && submitted,
            }" />
          </div>
          <div style="display: flex" class="field col-12 md:col-12">
            <div class="col-3 text-left"></div>
            <small v-if="
              (v$.declare_name.$invalid && submitted) ||
              v$.declare_name.$pending.$response
            " class="col-9 p-error">
              <span class="col-12 p-0">{{
                v$.declare_name.required.$message
                  .replace("Value", "Tên phiếu lương ")
                  .replace("is required", "không được để trống")
              }}</span>
            </small>
          </div>
          <div class="col-12 field md:col-12 flex align-items-center">
            <div class="col-3 text-left p-0">Loại phiếu </div>

            <Dropdown v-model="declare.declare_type" :options="listDeclareType" optionLabel="name" optionValue="code"
              placeholder="----- Chọn loại phiếu -----" class="sel-placeholder  col-9" panelClass="d-design-dropdown" />
          </div>
          <div class="col-12 field md:col-12 flex">
            <div class="field col-6 md:col-6 p-0 align-items-center flex">
              <div class="col-6 text-left p-0">STT</div>
              <InputNumber v-model="declare.is_order" class="col-6 ip36 p-0" />
            </div>
            <div class="field col-6 md:col-6 p-0 align-items-center flex">
              <div class="col-6 text-center p-0">Trạng thái</div>
              <InputSwitch v-model="declare.status" />
            </div>
          </div>
        </div>
      </form>
      <template #footer>
        <Button label="Hủy" icon="pi pi-times" @click="closeDialog" class="p-button-outlined" />

        <Button label="Lưu" icon="pi pi-check" @click="saveData(!v$.$invalid)" autofocus />
      </template>
    </Dialog>
    <Dialog :header="headerDialogP" v-model:visible="displayBasicP" :style="{ width: '35vw' }" 
    :closable="true"
      :modal="true">
      <form>
        <div class="grid formgrid m-2">
          <div class="field col-12 md:col-12">
            <label class="col-3 text-left p-0">Mã số <span v-if="paycheck.paycheck_type==2" class="redsao">(*)</span></label>
            <InputText v-model="paycheck.paycheck_code" spellcheck="false" class="col-9 ip36 px-2" 
            :class="{
              'p-invalid': paycheck.paycheck_code==null &&paycheck.paycheck_type==2  && submittedP,
            }"   />
          </div>
          <div style="display: flex" class="  col-12 md:col-12">
            <div class="col-3 text-left"></div>
            <small v-if="
           paycheck.paycheck_code==null &&paycheck.paycheck_type==2  && submittedP
            " class="col-9 p-0 field p-error">
              <span class="col-12 p-0">{{
                va$.paycheck_code.required.$message
                  .replace("Value", "Mã số ")
                  .replace("is required", "không được để trống")
              }}</span>
            </small>
          </div>
          <div class="field col-12 md:col-12">
            <label class="col-3 text-left p-0">Tên <span v-if="paycheck.paycheck_type==1" >đầu mục </span>
              <span v-else>chỉ tiêu </span>
              <span class="redsao">(*)</span></label>
            <InputText v-model="paycheck.paycheck_name" spellcheck="false" class="col-9 ip36 px-2"
            
            :class="{
              'p-invalid': va$.paycheck_name.$invalid && submittedP,
            }"/>
          </div>
          <div style="display: flex" class="  col-12 md:col-12">
            <div class="col-3 text-left"></div>
            <small v-if="
              (va$.paycheck_name.$invalid && submittedP) ||
              va$.paycheck_name.$pending.$response
            " class="col-9p-0 field p-error">
              <span class="col-12 p-0">{{
                va$.paycheck_name.required.$message
                  .replace("Value", "Tên đầu mục ")
                  .replace("is required", "không được để trống")
              }}</span>
            </small>
          </div>
          <div class="col-12 md:col-12 p-0" v-if="paycheck.paycheck_type==2">
          <div class="field col-12 md:col-12 flex">
            <div class="col-12 p-0 flex align-items-center">
              <label class="col-3 text-left p-0">Cấp cha</label>
              <TreeSelect
                class="col-9 p-0"
                @change="onChangeParent"
                v-model="selectCapcha"
                :options="treemodules"
                :showClear="true"
                placeholder="Chọn cấp cha "
                optionLabel="data.paycheck_name"
                optionValue="data.paycheck_id"
                panelClass="d-design-dropdown"
                :class="{
              'p-invalid': selectCapcha==null &&paycheck.paycheck_type==2  && submittedP,
            }"  
              ></TreeSelect>
            </div>
          </div>
        </div>
        <div style="display: flex" class="  col-12 md:col-12">
            <div class="col-3 text-left"></div>
            <small v-if="
          selectCapcha==null &&paycheck.paycheck_type==2  && submittedP
            " class="col-9 p-0 field p-error">
              <span class="col-12 p-0">Cấp cha không được để trống </span>
            </small>
          </div>
          <div class="col-12 field md:col-12 flex align-items-center">
            <div class="  col-6 md:col-6 p-0 align-items-center flex">
            <div class="col-6 text-left p-0">Loại phiếu </div>

            <Dropdown v-model="paycheck.paycheck_type" :options="listPaycheckType" optionLabel="name" optionValue="code"
              class="sel-placeholder  col-6" panelClass="d-design-dropdown" />
          </div>
          <div class="  col-6 md:col-6 p-0 align-items-center flex"  >
              <div class="col-6 text-left p-0 pl-3">Loại sắp xếp</div>
              <InputText v-model="paycheck.type_order" class="col-6 ip36 p-0 px-2" />
            </div>
      
            </div>
            <div class="col-12 field md:col-12 flex align-items-center" v-if="paycheck.paycheck_type==2">
              
            <div class="col-3 text-left p-0 ">Đơn vị tính </div>
            <InputText v-model="paycheck.paycheck_unit" class="col-9 ip36 p-0 px-2" />
 
       
              </div>
          <div class="col-12 field md:col-12 flex">
            <div class="field col-6 md:col-6 p-0 align-items-center flex">
              <div class="col-6 text-left p-0  ">STT</div>
              <InputNumber v-model="paycheck.is_order" class="col-6 ip36 p-0" />
            </div>
           
            <div class="field col-6 md:col-6 p-0 align-items-center flex">
              <div class="col-6 p-0 pl-3" >Trạng thái</div>
              <InputSwitch v-model="paycheck.status" />
            </div>
          </div>
        </div>
      </form>
      <template #footer>
        <Button label="Hủy" icon="pi pi-times" @click="closeDialogP" class="p-button-outlined" />

        <Button label="Lưu" icon="pi pi-check" @click="savePaycheck(!va$.$invalid)" autofocus />
      </template>
    </Dialog>
  </div>
  <Menu id="overlay_More" ref="menuButMores" :model="itemButMores" :popup="true" />
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

.d-lang-table {
  margin: 0px;
  height: calc(100vh - 160px);
}.d-lang-table-r{
  margin: 0px;
  height: calc(100vh - 160px);
}

.bg-d-selected{
  background-color:#E3F2FD !important;
   
}
</style>
    