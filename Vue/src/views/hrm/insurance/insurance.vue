<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import diloginsurance from "./component/diloginsurance.vue";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../../util/function.js";
import VueDatePicker from '@vuepic/vue-datepicker';

// import Datepicker from 'vuejs3-datepicker';

import moment from "moment";
const router = inject("router");
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
  insurance_id: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
//khai bao bien
const statuss = ref([
  { value: 1, text: "Trả" },
  { value: 2, text: "Sửa" },
  { value: 3, text: "Chốt" },
  { value: 4, text: "Xin cấp" },
  { value: 5, text: "Gộp" },
  { value: 6, text: "Người lao động giữ sổ" },
]);
const hinhthucs = ref([
  { value: 1, text: "Báo tăng" },
  { value: 2, text: "Báo giảm" },
]);
const groups = ref([
  { view: 1, icon: "pi pi-list", title: "list" },
  { view: 2, icon: "pi pi-align-right", title: "tree" },
]);
const first = ref(1);
const insurance_pays = ref([]);
const insurance_resolves = ref();
const dictionarys = ref();
const datatrees = ref()
//Function
const componentKey = ref(0);
const forceRerender = () => {
  componentKey.value += 1;
};
//Lấy số bản ghi
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_insurance_count_1",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "date", va: options.value.date },
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
const loadTudien = () => {
  axios
    .post(
      baseURL + "api/insurance/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_insurance_dictionary",
            par: [{ par: "user_id", va: store.state.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      dictionarys.value = data;
      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
//Lấy dữ liệu bank
const initData = (rf) => {
  if(isEmpty(options.value.date)) options.value.date = new Date(dt.getFullYear(), dt.getMonth()) ;
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
        baseURL + "/api/insurance/GetDataProc",
        {
          str: encr(
            JSON.stringify({
              proc: "hrm_insurance_list_1",
              par: [
                { par: "pageno", va: options.value.PageNo },
                { par: "pagesize", va: options.value.PageSize },
                { par: "user_id", va: store.getters.user.user_id },
                { par: "status", va: null },
                { par: "search", va: options.value.searchStamp },
                { par: "date", va: options.value.date },
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

        if (error && error.status === 401) {
          swal.fire({
            text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
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

    options.value.id = datalists.value[datalists.value.length - 1].bank_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].bank_id;
    options.value.IsNext = false;
  }
  options.value.PageNo = event.page;
  initData(true);
};

const insurance = ref({
  status: null,
  organization_payment: null,
  insurance_province_id: null,
  hospital_name: null,
  organization_id: store.getters.user.organization_id,
  is_order: 1,
  profile_id: 1,
});
const selectedStamps = ref();
const monthPickerFilter = ref();
const submitted = ref(false);
const isAdd = ref(false);
const isView = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = baseURL;
const checkDelList = ref(false);
var dt = new Date();
const options = ref({
  IsNext: true,
  sort: "created_date",
  SearchText: "",
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
  date : new Date(dt.getFullYear(), dt.getMonth()),
  view:1,
  start_date:  new Date(dt.getFullYear(), 0),
  end_date: new Date(dt.getFullYear(), dt.getMonth()),
});

//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);
const openBasic = () => {
  forceRerender();
  submitted.value = false;
  insurance.value = {
    status: null,
    organization_payment: null,
    insurance_province_id: null,
    hospital_name: null,
    organization_id: store.getters.user.organization_id,
    profile_id: null,
  };
  insurance_pays.value = [];
  // insurance_pays.value = [
  //   {
  //     start_date: null,
  //     payment_form: null,
  //     reason: null,
  //     end_date: null,
  //     organization_payment: null,
  //     total_payment: null,
  //     company_payment: null,
  //     member_payment: null,
  //   },
  // ];
  insurance_resolves.value = [
    {
      type_mode: null,
      payment_form: null,
      type_mode: null,
      completed_date: null,
      received_money_date: null,
      money: null,
    },
  ];
  checkIsmain.value = false;
  isAdd.value = true;
  headerDialog.value = 'Thêm mới bảo hiểm';
  displayBasic.value = true;
};

const closeDialog = () => {
  displayBasic.value = false;
  initData(true);
  forceRerender();
};

//Thêm bản ghi
const sttStamp = ref(1);
const saveData = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  insurance_pays.value.forEach((item) => {
    item.is_duplicate = false;
  });
  if (insurance_pays.value.length >= 2) {
    let count_duplicate = 0;
    for (let i = 0; i < insurance_pays.value.length - 1; i++) {
      for (let j = i + 1; j < insurance_pays.value.length; j++) {
        if (
          !isEmpty(insurance_pays.value[i].start_date) &&
          !isEmpty(insurance_pays.value[i].end_date) &&
          !isEmpty(insurance_pays.value[j].start_date) &&
          !isEmpty(insurance_pays.value[j].end_date) &&
          isMonth(
            insurance_pays.value[i],
            insurance_pays.value[j]
          )
        ) {
          insurance_pays.value[j].is_duplicate = true;
          insurance_pays.value[i].is_duplicate = true;
          count_duplicate++;
        }
      }
      if (count_duplicate > 0) {
        swal.fire({
          title: "Thông báo!",
          text: "Vui lòng nhập tháng đóng đóng bảo hiểm không được trùng nhau!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
    }
  }
  insurance_pays.value.forEach((item) => {
    if (!isEmpty(item.is_duplicate)) item.is_duplicate = null;
  });
  let formData = new FormData();

  formData.append("insurance", JSON.stringify(insurance.value));
  formData.append(
    "insurance_pay",
    JSON.stringify(
      insurance_pays.value.filter(
        (item) => !Object.values(item).every((o) => isEmpty(o))
      )
    )
  );
  formData.append(
    "insurance_resolve",
    JSON.stringify(
      insurance_resolves.value.filter(
        (item) => !Object.values(item).every((o) => isEmpty(o))
      )
    )
  );
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: isAdd.value == false ? "put" : "post",
    url:
      baseURL +
      `/api/insurance/${
        isAdd.value == false ? "update_insurance" : "add_insurance"
      }`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  }).then((response) => {
    if (response.data.err === "0") {
      swal.close();
      toast.success("Cập nhật thành công!");
      displayBasic.value = false;
      initData(true);
    } else {
      swal.fire({
        title: "Thông báo!",
        text: "Đã có mã sổ này trong hệ thống rồi!",
        icon: "error",
        confirmButtonText: "OK",
      });
    }
  });
};
const checkIsmain = ref(true);
//Sửa bản ghi
const editTem = (dataTem) => {
  submitted.value = false;
  axios
    .post(
      baseURL + "/api/Profile/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_insurance_get_1",
            par: [
              { par: "insurance_id", va: dataTem.insurance_id },
              { par: "date", va: options.value.date },
          ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        if (data[0][0].birthday) {
          data[0][0].birthday = new Date(data[0][0].birthday);
        }
        if (data[0][0].identity_date_issue) {
          data[0][0].identity_date_issue = new Date(data[0][0].identity_date_issue);
        }
        insurance.value = data[0][0];
        insurance.value.profile = {
          profile_id : insurance.value.profile_id,
          profile_code : insurance.value.profile_code,
          profile_user_name : insurance.value.profile_user_name,
          is_order : insurance.value.is_order,
        }
        insurance.value.birthplace_origin = insurance.value.birthplace_origin_name ? insurance.value.birthplace_origin_name:insurance.value.birthplace_origin_last;
        insurance.value.place_register_permanent = insurance.value.place_register_permanent_last ? insurance.value.place_register_permanent_last:((insurance.value.place_register_permanent_first||'') + ' '+ (insurance.value.place_register_permanent_name||''));
        //get child
        if (data[1].length > 0) {
          insurance_pays.value = data[1];
          insurance_pays.value.forEach((item) => {
            if (item.start_date != null) {
              item.start_date = new Date(item.start_date);
            }
            if (item.end_date != null) {
              item.end_date = new Date(item.end_date);
            }
          });
        } else insurance_pays.value = [];

        if (data[2].length > 0) {
          insurance_resolves.value = data[2];
          insurance_resolves.value.forEach((item) => {
            if (item.received_file_date != null) {
              item.received_file_date = new Date(item.received_file_date);
            }
            if (item.completed_date != null) {
              item.completed_date = new Date(item.completed_date);
            }
            if (item.received_money_date != null) {
              item.received_money_date = new Date(item.received_money_date);
            }
          });
        } else insurance_resolves.value = [];
      }
      headerDialog.value = "Sửa thông tin";
      isAdd.value = false;
      displayBasic.value = true;
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
          .delete(baseURL + "/api/insurance/del_insurance", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: [Tem.insurance_id],
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá thẻ bảo hiểm thành công!");
              initData(true);
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
//Xuất excel

//Sort
const onSort = (event) => {
  options.value.PageNo = 0;

  if (event.sortField == null) {
    isDynamicSQL.value = false;
    initData(true);
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
    id: "bank_id",
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
    .post(baseURL + "/api/hrm_ca_SQL/Filter_hrm_ca_bank", data, config)
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
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
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
      initData(true);
    } else {
      isDynamicSQL.value = true;
      options.value.loading = true;
      initData(true);
    }
  }
};
const onRefresh = () => {
   options.value = {
      IsNext: true,
      sort: "created_date",
      SearchText: "",
      PageNo: 0,
      PageSize: 20,
      loading: true,
      totalRecords: null,
      date : new Date(dt.getFullYear(), dt.getMonth()),
      view:1,
      start_date:  new Date(dt.getFullYear(), 0),
      end_date: new Date(dt.getFullYear(), dt.getMonth()),
    };
  // options.value.searchStamp = null;
  // options.value.date = null;
  monthPickerFilter.value = null;
  yearPickerFilter.value = null;
  filterTrangthai.value = null;
  options.value.loading = true;
  selectedStamps.value = [];
  isDynamicSQL.value = false;
  filterSQL.value = [];
  initData(true);
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
//Xóa nhiều
const deleteList = () => {
  let listId = new Array(selectedStamps.value.length);
  let checkD = false;
  selectedStamps.value.forEach((item) => {
    if (item.is_default) {
      toast.error("Không được xóa thẻ bảo hiểm mặc định!");
      checkD = true;
      return;
    }
  });
  if (!checkD) {
    swal
      .fire({
        title: "Thông báo",
        text: "Bạn có muốn xoá thẻ bảo hiểm này không!",
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
            listId.push(item.insurance_id);
          });
          axios
            .delete(baseURL + "/api/insurance/del_insurance", {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: listId != null ? listId : 1,
            })
            .then((response) => {
              swal.close();
              if (response.data.err != "1") {
                swal.close();
                toast.success("Xoá thẻ bảo hiểm thành công!");
                checkDelList.value = false;

                initData(true);
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
                  text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
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
const addRow = (type) => {
  //relative
  if (type == 1) {
    let obj = {
      start_date: null,
      payment_form: null,
      reason: null,
      end_date: null,
      organization_payment: null,
      total_payment: null,
      company_payment: null,
      member_payment: null,
    };
    insurance_pays.value.push(obj);
  }
  if (type == 2) {
    let obj = {
      type_mode: null,
      payment_form: null,
      type_mode: null,
      completed_date: null,
      received_money_date: null,
      money: null,
    };
    insurance_resolves.value.push(obj);
  }
};
const deleteRow = (idx, type) => {
  if (type == 1) {
    insurance_pays.value.splice(idx, 1);
  }
  if (type == 2) {
    insurance_resolves.value.splice(idx, 1);
  }
};
const goProfile = (item) => {
  router.push({
    name: "profileinfo",
    params: { id: generateUUID()},
    query: { id: item.profile_id },
  });
};
//filter date
const yearPickerFilter = ref();
const onFilterMonth = (type)=>{
  if(type == 1)
 {
  if (monthPickerFilter.value == null){
    options.value.date = null;
  } 
  else
    options.value.date = new Date(monthPickerFilter.value.month +1 +"/01" +"/" +monthPickerFilter.value.year);
  initData(true);
 } 
 else{
  options.value.start_date = new Date(yearPickerFilter.value, 0);
  options.value.end_date =  new Date(yearPickerFilter.value, 11);
  loadDataTree(true);
 }
}
const onCleanFilterMonth = (type) => {
  if( type == 1)
 {
  if (monthPickerFilter.value) monthPickerFilter.value = null;
  options.value.date = null;
  initData(true);
 } 
 else{
  if (yearPickerFilter.value) yearPickerFilter.value = null;
  options.value.start_date = new Date(dt.getFullYear(), 0);
  options.value.end_date = new Date(dt.getFullYear(), dt.getMonth());
  loadDataTree(true);
 }
};
const changeView = (view) => {
  options.value = {
      IsNext: true,
      sort: "created_date",
      SearchText: "",
      PageNo: 0,
      PageSize: 20,
      totalRecords: null,
      date : new Date(dt.getFullYear(), dt.getMonth()),
      view:view,
      start_date:  new Date(dt.getFullYear(), 0),
      end_date: new Date(dt.getFullYear(), dt.getMonth()),
    };
  monthPickerFilter.value = null;
  yearPickerFilter.value = null;
  if(view == 1) initData();
  else if(view == 2) loadDataTree();
};
const listDate= ref();
const isViewTree = ref(false);
const amount_paid_final = ref();
const payment_final = ref();
const loadDataTree = ()=>{
  if(isNaN(options.value.start_date)|| options.value.start_date.getFullYear()== 1900) options.value.start_date = new Date(dt.getFullYear(), 0)
  if(isNaN(options.value.end_date)|| options.value.end_date.getFullYear()== 1900) options.value.end_date = new Date(dt.getFullYear(), dt.getMonth())
  payment_final.value = 0;
  amount_paid_final.value = 0;
  listDate.value = dateRange(options.value.start_date, options.value.end_date)
  axios
        .post(
          baseURL + "/api/insurance/GetDataProc",
          {
            str: encr(
              JSON.stringify({
                proc: "hrm_insurance_list_tree",
                par: [
                  { par: "pageno", va: options.value.PageNo },
                  { par: "pagesize", va: options.value.PageSize },
                  { par: "user_id", va: store.getters.user.user_id },
                  { par: "status", va: null },
                  { par: "search", va: options.value.searchStamp },
                  { par: "start_date", va: options.value.start_date },
                  { par: "end_date", va: options.value.end_date },
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
            element.data = JSON.parse(element.data);
           // element.listDays = element.data;
            element.listDays = element.data? groupDataMonth(listDate.value,element.data ) : [];
            // element.listDays = element.data != null? element.data.map(x => ({start_date: x.start_date, end_date : x.end_date})): [];
            //element.listDays = element.data != null? geAllMonth(listDate.value,element.data): [];
            element.total_payment = element.listDays != null? element.listDays.map(x => parseInt(x.total_payment || 0)): [];
            element.amount_paid = element.listDays != null? element.listDays.map(x => parseInt(x.amount_paid || 0)): [];
            element.payment_all = element.total_payment.length== 0 ?0 : element.total_payment.reduce((a, b) => a + b, 0);
            element.amount_paid_all = element.amount_paid.length== 0 ?0 : element.amount_paid.reduce((a, b) => a + b, 0);
            payment_final.value += element.payment_all;
            amount_paid_final.value += element.amount_paid_all;
          });
          datatrees.value = data;
          // get du lieu tong
          listDate.value.forEach((item)=>{
            item.payment_all = 0;
            item.amount_paid_all = 0;
            data.forEach((user)=>{
              if(user.listDays && user.listDays.filter(x => x.pay_date == item.value).length>0){
                item.payment_all += parseInt(user.listDays.filter(x => x.pay_date == item.value)[0].total_payment || 0);
                item.amount_paid_all += parseInt(user.listDays.filter(x => x.pay_date == item.value)[0].amount_paid|| 0);
              }
            })
          })
          options.value.loading = false;
          isViewTree.value = true;
        })
        .catch((error) => {
          toast.error("Tải dữ liệu không thành công!");
          options.value.loading = false;

          if (error && error.status === 401) {
            swal.fire({
              text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
              confirmButtonText: "OK",
            });
            store.commit("gologout");
          }
        });
  
}
// lay tat ca thang hop le 
const groupDataMonth = (arr1, arr2)=>{
  var arr = [];
  arr1.forEach((date1)=>{
   if(arr2.filter(x => isDuringMonth(x.start_date, x.end_date,date1.value)).length>0){
    let obj = arr2.filter(x => isDuringMonth(x.start_date, x.end_date,date1.value)).sort((a, b) => b.start_date - a.start_date)[0]; // lay ngay muon nhat
    arr.push({pay_date: date1.value, amount_paid: obj.amount_paid, total_payment: obj.total_payment})
   }
  })
  return arr;
};
//item.listDays.filter(x => isDuringMonth(x.start_date, x.end_date,item_month.value))
const checkExistDate = (date,arrDates)=>{
  if(arrDates.filter(x => getValueDate(x.start_date)== getValueDate(date) || getValueDate(x.end_date)== getValueDate(date) ).length>0)
  return true;
  else return false;
};
const isDuringMonth = (start_date, end_date, date_check)=>{
  if(!end_date) return (getValueDate(date_check) > getValueDate(start_date));
  else 
  return (getValueDate(date_check) >= getValueDate(start_date) && getValueDate(date_check) < getValueDate(end_date));
}
const getValueDate = (date)=>{
  let dt = new Date(date);
  let date_format = new Date(dt.getFullYear(), dt.getMonth(),1);
  return date_format.getTime();
};
//excel
const exportExcel = () => {
  let text_string = "";
  text_string =
    "TỪ " +
    moment(new Date(options.value.start_date))
      .format("MM/YYYY")
      .toString() +
    " - " +
    moment(new Date(options.value.end_date)).format("MM/YYYY").toString();
  
  let name = "Danh sach dong bao hiem thang "+ moment(new Date()).format("MM-YYYY").toString();
  let id = "tablequizz";
  var htmltable1 = "";
  // htmltable1 = renderExcel_Ketqua();
  var tab_text = '<html xmlns:x="urn:schemas-microsoft-com:office:excel">';
  tab_text =
    tab_text +
    "<head><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet>";
  tab_text = tab_text + "<x:Name>Test Sheet</x:Name>";
  tab_text =
    tab_text +
    "<x:WorksheetOptions><x:Panes></x:Panes></x:WorksheetOptions></x:ExcelWorksheet>";
  tab_text =
    tab_text + "</x:ExcelWorksheets></x:ExcelWorkbook></xml></head><body>";
  tab_text =
    tab_text +
    "<style>.item-date{min-width:100px !important} th,td,table,tr{padding:5px;font-size:13pt} .text-right{text-align:right} .text-left{text-align:left}table{margin:20px auto;border-collapse: collapse;}</style>";
  tab_text =
    tab_text +
    '<style>.cstd{font-family: Times New Roman;border:none!important; font-size: 17px; font-weight: 700; text-align: center; vertical-align: center;color:#1769aa}</style><table><td colspan="' +
    '15' +
    '" class="cstd" > DANH SÁCH ĐÓNG BẢO HIỂM ' +
    text_string +
    "</td > ";
  tab_text = tab_text + "</table>";

  //var exportTable = $('#' + id).clone();
  //exportTable.find('input').each(function (index, elem) { $(elem).remove(); });\
  tab_text =
    tab_text +
    "<style>th,table,tr{font-family: Times New Roman; font-size: 12px; vertical-align: middle;}</style><table border='1'>";
  var exportTable = document
    .getElementById("table-bc")
    .cloneNode(true).innerHTML;
  tab_text = tab_text + exportTable.replaceAll('.', ',');
  tab_text = tab_text + htmltable1;
  tab_text = tab_text + "</table>";
  tab_text = tab_text + '<meta charset="utf-8"/></ta></body></html>';
  var data_type =
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
  var ua = window.navigator.userAgent;
  var msie = ua.indexOf("MSIE ");

  var fileName = name + " " + parseInt(Math.random() * 100) + ".xls";
  if (msie > 0 || !!navigator.userAgent.match(/Trident.*rv\:11\./)) {
    if (window.navigator.msSaveBlob) {
      var blob = new Blob([tab_text], {
        type: data_type, //"application/csv;charset=utf-8;"
      });
      navigator.msSaveBlob(blob, fileName);
    }
  } else {
    var blob2 = new Blob([tab_text], {
      type: data_type, //"application/csv;charset=utf-8;"
    });
    var filename = fileName;
    var elem = window.document.createElement("a");
    elem.href = window.URL.createObjectURL(blob2);
    elem.download = filename;
    document.body.appendChild(elem);
    elem.click();
    document.body.removeChild(elem);
  }
};

//export
const menuButs = ref();
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const itemButs = ref([
  {
    label: "Export dữ liệu ra Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportExcel();
    },
  },
  {
    label: "Import dữ liệu từ Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      //exportData("ExportExcel");
    },
  },
]);

//check empy object
//check month  date
function isMonth(data1, data2) {
  let start1 = new Date(data1.start_date).getTime();
  let end1 = new Date(data1.end_date).getTime();
  let start2 = new Date(data2.start_date).getTime();
  let end2 = new Date(data2.end_date).getTime();
  return start1 < end2
  || end1 > start2
  || (start1> start2 && end1> end2)
  || (start1< start2 && end1< end2)
    ? true
    : false;
}
function isEmpty(val) {
  return val === undefined || val == null || val.length <= 0 ? true : false;
}
function generateUUID() {
  // Public Domain/MIT
  var d = new Date().getTime(); //Timestamp
  var d2 =
    (typeof performance !== "undefined" &&
      performance.now &&
      performance.now() * 1000) ||
    0; //Time in microseconds since page-load or 0 if unsupported
  return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
    var r = Math.random() * 16; //random number between 0 and 16
    if (d > 0) {
      //Use timestamp until depleted
      r = (d + r) % 16 | 0;
      d = Math.floor(d / 16);
    } else {
      //Use microseconds since page-load if supported
      r = (d2 + r) % 16 | 0;
      d2 = Math.floor(d2 / 16);
    }
    return (c === "x" ? r : (r & 0x3) | 0x8).toString(16);
  });
}
function formatNumber(a, b, c, d) {
  var e = isNaN((b = Math.abs(b))) ? 2 : b;
  b = void 0 == c ? "," : c;
  d = void 0 == d ? "," : d;
  c = 0 > a ? "-" : "";
  var g = parseInt((a = Math.abs(+a || 0).toFixed(e))) + "",
    n = 3 < (n = g.length) ? n % 3 : 0;
  return (
    c +
    (n ? g.substr(0, n) + d : "") +
    g.substr(n).replace(/(\d{3})(?=\d)/g, "$1" + d) +
    (e
      ? b +
        Math.abs(a - g)
          .toFixed(e)
          .slice(2)
      : "")
  );
}

//get list month
function dateRange(startDate, endDate) {
  if(startDate == null) startDate = endDate;
  var start      = moment(startDate).format("yyyy/MM/DD").split('/');
  var end        =  moment(endDate).format("yyyy/MM/DD").split('/');
  var startYear  = parseInt(start[0]);
  var endYear    = parseInt(end[0]);
  var dates      = [];

  for(var i = startYear; i <= endYear; i++) {
    var endMonth = i != endYear ? 11 : parseInt(end[1]) - 1;
    var startMon = i === startYear ? parseInt(start[1])-1 : 0;
    for(var j = startMon; j <= endMonth; j = j > 12 ? j % 12 || 11 : j+1) {
      var month = j+1;
      var displayMonth = month < 10 ? '0'+month : month;
      var obj ={
        label: 'Tháng '+ month,
        year: i,
        month: month,
        value:[displayMonth, '01', i].join('/'),
        
      }
      dates.push(obj);
    }
  }
  return dates;
}
onMounted(() => {
  initData(true);
  loadTudien();

});
</script>
    <template>
  <div class="main-layout true flex-grow-1 p-2 pb-0 pr-0 insurance">
    <div style="background-color: #fff; padding: 1rem;">
      <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-building"></i> Danh sách đóng bảo hiểm  
          <span v-if="options.view == 1 && options.date">tháng {{moment(new Date(options.date)).format("MM/YYYY")}} </span>
          <span v-if="options.view == 2"> từ tháng {{moment(new Date(options.start_date)).format("MM/YYYY")}} đến tháng {{moment(new Date(options.end_date)).format("MM/YYYY")}}</span>
           ({{
            options.totalRecords
          }})
        </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                style="height:31px"
                v-model="options.searchStamp"
                v-on:keyup.enter="initData(true)"
                type="text"
                spellcheck="false"
                placeholder="Tìm kiếm"
              />            
            </span>
                        
           <VueDatePicker
           v-if="options.view == 2"
              @closed="onFilterMonth(2,true)"
              class="ml-2 datepicker"
              locale="vi"
              selectText="Lọc"
              cancelText="Hủy"
              placeholder="Lọc theo năm"
              v-model="yearPickerFilter"
              auto-apply
              year-picker
              ><template #clear-icon>
                <Button
                  @click="onCleanFilterMonth(2)"
                  icon="pi pi-times"
                  class="p-button-rounded p-button-text"
                />
              </template>
              <template #input-icon>
                <Button icon="pi pi-calendar" class="p-button-text" />
              </template>
            </VueDatePicker>
            <Datepicker
            v-if="options.view == 1"
              @closed="onFilterMonth(1)"
              class="ml-2 datepicker"
              locale="vi"
              selectText="Lọc"
              cancelText="Hủy"
              placeholder=" Lọc theo tháng"
              v-model="monthPickerFilter"
              auto-apply
              monthPicker
              ><template #clear-icon>
                <Button
                  @click="onCleanFilterMonth(1)"
                  icon="pi pi-times"
                  class="p-button-rounded p-button-text"
                />
              </template>
              <template #input-icon>
                <Button icon="pi pi-calendar" class="p-button-text" />
              </template>
            </Datepicker>
          </template>

          <template #end>
            <Button
               @click="openBasic()"
              label="Thêm mới"
              icon="pi pi-plus"
              class="mr-2"
            /> 
            <Button
              @click="onRefresh"
              class="mr-2 p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              v-tooltip="'Tải lại'"
            />

            <Button
              v-if="options.view == 2"
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
            <SelectButton
              v-model="options.view"
              :options="groups"
              @change="changeView(options.view)"
              optionValue="view"
              optionLabel="view"
              dataKey="view"
              aria-labelledby="custom"
            >
              <template #option="slotProps">
                <div v-tooptip.top="slotProps.option.title">
                  <i :class="slotProps.option.icon"></i>
                </div>
              </template>
            </SelectButton>
          </template>
        </Toolbar>
    </div>

    <!-- <DataTable
      v-if="options.view == 1"
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
      rowGroupMode="subheader"
      groupRowsBy="department_origin_id"
      removableSort
      v-model:rows="options.PageSize"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      :paginator="true"
      dataKey="insurance_id"
      responsiveLayout="scroll"
      v-model:selection="selectedStamps"
      :row-hover="true"
    > -->
    <DataTable
      v-if="options.view == 1"
        class="w-full p-datatable-sm e-sm"
        :value="datalists"
        dataKey="insurance_id"
        :showGridlines="true"
        :rowHover="true"
        currentPageReportTemplate=""
        responsiveLayout="scroll"
        :scrollable="true"
        scrollHeight="flex"
        rowGroupMode="subheader"
        groupRowsBy="department_origin_name"
        :rows="options.PageSize"
        :loading="options.loading"
        :paginator="true"
        :rowsPerPageOptions="[20, 30, 50, 100, 200]"
        :totalRecords="options.totalRecords"
        @nodeSelect="onNodeSelect"
        @nodeUnselect="onNodeUnselect"
        @page="onPage($event)"
        @filter="onFilter($event)"
        @sort="onSort($event)"
        style="max-height:calc(100vh - 150px);min-height: calc(100vh - 150px)"
      >
      <template #groupheader="slotProps">
        {{ slotProps.data.department_origin_name || "Không thuộc phòng ban" }}
      </template>
      <!-- <Column
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:70px;height:50px"
        bodyStyle="text-align:center;max-width:70px"
        selectionMode="multiple"
      >
      </Column> -->


      <!-- <Column
        field="profile_id"
        header="Mã nhân sự"
        headerStyle="text-align:center;max-width:100px;height:50px"
        bodyStyle="text-align:center;max-width:100px"
        class="align-items-center justify-content-center text-center"
      >
      </Column> -->
      <!-- <Column field="file_name" header="Tên file số hóa" headerStyle="text-align:left;height:50px"
            bodyStyle="text-align:left;word-break:break-word">
          </Column> -->
      <Column
        field="profile_user_name"
        header="Họ và tên"
        headerStyle="text-align:center;height:50px;justify-content:center"
        bodyStyle="text-align:left;justify-content:start"

      >
      <template #body="slotProps">
            <span>{{
              slotProps.data.profile_user_name
            }}
            </span>
          </template>
      </Column>
      <Column
        field="position_name"
        header="Chức vụ"
        headerStyle="text-align:center;max-width:200px;height:50px"
        bodyStyle="text-align:left;justify-content:start;max-width:200px;"
      >
      </Column>
      <Column
        field="recruitment_date"
        header="Ngày vào"
        headerStyle="text-align:center;max-width:100px;height:50px"
        bodyStyle="text-align:center;max-width:100px;"
        class="align-items-center justify-content-center text-center"
      >
      <template #body="{ data }">
          <span v-if="data.recruitment_date"> {{ moment(new Date(data.recruitment_date)).format("DD/MM/YYYY ") }}</span>
        </template>
      </Column>
      <Column
        field="insurance_id"
        header="Số sổ"
        headerStyle="text-align:center;max-width:100px;height:50px"
        bodyStyle="text-align:center;max-width:100px;"
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="insurance_code"
        header="Số thẻ"
        headerStyle="text-align:center;max-width:100px;height:50px"
        bodyStyle="text-align:center;max-width:100px;"
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="batdaudong"
        header="Bắt đầu đóng"
        headerStyle="text-align:center;max-width:120px;height:50px"
        bodyStyle="text-align:center;max-width:120px;"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="{ data }">
          <span v-if="data.batdaudong">{{ moment(new Date(data.batdaudong)).format("MM/YYYY ") }}</span>
        </template>
      </Column>
      <Column
        field="mucdong"
        header="Mức đóng"
        headerStyle="text-align:center;max-width:120px;height:50px;justify-content:center"
        bodyStyle="text-align:center;max-width:120px;justify-content:end"
      >
        <template #body="{ data }">
          {{ formatNumber(data.mucdong, 0, ".", ".") }}
        </template>
      </Column>
      <Column
        field="congtydong"
        header="Công ty đóng"
        headerStyle="text-align:center;max-width:120px;height:50px;justify-content:center"
        bodyStyle="text-align:center;max-width:120px; justify-content:end"
      >
        <template #body="{ data }">
          {{ formatNumber(data.congtydong, 0, ".", ".") }}
        </template>
      </Column>
      <Column
        field="nhanviendong"
        header="Người lao động đóng"
        headerStyle="text-align:center;max-width:120px;height:50px;justify-content:center"
        bodyStyle="text-align:right;max-width:120px;justify-content:end"
      >
        <template #body="{ data }">
          {{ formatNumber(data.nhanviendong, 0, ".", ".") }}
        </template>
      </Column>
      <Column
        header="Chức năng"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:120px;height:50px"
        bodyStyle="text-align:center;max-width:120px"
      >
        <template #body="Tem">
          <div>
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
          class="
            align-items-center
            justify-content-center
            p-4
            text-center
            m-auto
          "
          v-if="!isFirst"
        >
          <img src="../../../assets/background/nodata.png" height="144" />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </DataTable>
    <div style="width: 100%;height: calc(100vh - 160px);" class="overflow-scroll relative" v-if="isViewTree"> 
      <table class="w-full" style="overflow-y: scroll" id="table-bc">
      <thead>
      <tr style="background-color: #f8f9fa; z-index: 10 !important" class="top-0 sticky">
        <th rowspan="2" style="padding: 0.5rem;background-color: #f8f9fa;min-width: 50px;max-width: 50px;" class="m-checkbox-table top-0 sticky left-0">
          STT
        </th>
        <th rowspan="2" style="padding: 0.5rem;background-color: #f8f9fa;min-width: 100px;max-width: 100px;" class="m-checkbox-table top-0 sticky left-50">
          Mã NV
        </th>
        <th rowspan="2" style="padding: 0.5rem;background-color: #f8f9fa;min-width: 150px;max-width: 150px;" class="m-checkbox-table top-0 sticky left-150">
          Họ và tên
        </th>
        <th  v-for="(item,index) in listDate" :key="index" colspan="2" class="text-center py-2">{{ item.label }}</th>
        <th colspan="2" class="text-center py-2">
          Tổng
        </th>
      </tr>
      <tr class="sticky" style="z-index: 1 !important;top: 33px !important;background-color: #f8f9fa;">

        <template v-for="(item,index) in listDate" :key="index">
          <th class="item-date" style="padding: 0.5rem">Phải đóng</th>
          <th class="item-date" style="padding: 0.5rem">Đã đóng</th>
        </template>
        <th class="item-date" style="padding: 0.5rem">Phải đóng</th>
        <th class="item-date" style="padding: 0.5rem">Đã đóng</th>
      </tr>
      
    </thead>
    <tbody>
        <tr style="vertical-align: top" v-for="(item, index) in datatrees" :key="index">
          <td  class="sticky left-0 p-2 bg-white text-left"
            >
            <div class="format-center w-full h-full">
              {{ index + 1 }}
            </div>
          </td>
          <td class="sticky p-2 left-50 bg-white text-left" >
            <div class="format-center w-full h-full">
              {{ item.profile_id }}
            </div>
          </td>
          <td class="sticky p-2 left-150 bg-white text-left">
            <div class="format-center w-full h-full">
              {{ item.profile_user_name }}
            </div>
          </td>
          <template v-for="(item_month,index2) in listDate" :key="index2">
            <td class="text-right item-date bg-white" style="padding: 0.5rem">
              <span v-if="item.listDays.filter(x => x.pay_date == item_month.value).length>0">{{ item.listDays.filter(x => x.pay_date == item_month.value).length> 0 ?formatNumber(item.listDays.filter(x => x.pay_date == item_month.value)[0].total_payment, 0, ".", "."): ''}}</span>
              <!-- <span v-if="item.listDays && item.listDays.filter(x => isDuringMonth(x.start_date, x.end_date,item_month.value)).length > 0">
               {{  formatNumber(item.listDays.filter(x => isDuringMonth(x.start_date, x.end_date,item_month.value))[0].total_payment , 0, ".", ".")}} 
              </span> -->
            </td>
            <td class="text-right item-date bg-white" style="padding: 0.5rem">
              <span v-if="item.listDays.filter(x => x.pay_date == item_month.value).length>0">{{ item.listDays.filter(x => x.pay_date == item_month.value).length> 0 ?formatNumber(item.listDays.filter(x => x.pay_date == item_month.value)[0].amount_paid, 0, ".", "."): ''}}</span>
              <!-- <span v-if="item.listDays && item.listDays.filter(x => isDuringMonth(x.start_date, x.end_date,item_month.value)).length > 0">
               {{  formatNumber(item.listDays.filter(x => isDuringMonth(x.start_date, x.end_date,item_month.value))[0].total_payment , 0, ".", ".")}} 
              </span> -->
            </td>
          </template>
          <td class="text-right item-date bg-white" style="padding: 0.5rem">
            {{ item.payment_all != 0 ? formatNumber( item.payment_all, 0, ".", ".") : '' }}
          </td>
          <td class="text-right item-date bg-white" style="padding: 0.5rem">
            {{item.amount_paid_all!=0 ? formatNumber( item.amount_paid_all , 0, ".", "."):'' }}
          </td>
        </tr>
        <tr>
            <td class="sticky left-0 p-2 text-center bg-white"></td>
            <td
              class="sticky left-50 p-2  font-bold bg-white"
              colspan="2"
              style="text-align: center"
            >
              Tổng cộng
            </td>
            <template v-for="(item_month,index2) in listDate" :key="index2">
            <td class="text-right item-date bg-white font-bold" style="padding: 0.5rem">
              {{item_month.payment_all!=0 ? formatNumber( item_month.payment_all, 0, ".", "."):'' }}
            </td>
            <td class="text-right item-date bg-white font-bold" style="padding: 0.5rem" >
              {{item_month.amount_paid_all!=0 ? formatNumber( item_month.amount_paid_all, 0, ".", "."):'' }}
            </td>
          </template>
            <td class="text-right item-date bg-white font-bold" style="padding: 0.5rem">
              {{payment_final!=0 ? formatNumber( payment_final, 0, ".", "."):'' }}
            </td>
            <td class="text-right item-date bg-white font-bold" style="padding: 0.5rem" >
              {{amount_paid_final!=0 ? formatNumber(amount_paid_final, 0, ".", "."):'' }}
            </td>
          </tr>
    </tbody>
    </table>
    </div>

  </div>

  <diloginsurance
    :key="componentKey"
    :headerDialog="headerDialog"
    :displayDialog="displayBasic"
    :closeDialog="closeDialog"
    :isAdd="isAdd"
    :isView="isView"
    :model="insurance"
    :addRow="addRow"
    :deleteRow="deleteRow"
    :insurance_pays="insurance_pays"
    :insurance_resolves="insurance_resolves"
    :statuss="statuss"
    :hinhthucs="hinhthucs"
    :dictionarys="dictionarys"
    :initData="initData"
    :datefilter="options.date"
  />
</template>
    
<style scoped>
    .hover:hover {
  color: #0078d4;
}
.ip33 {
  height: 33px !important;
}
.scroll-outer {
  visibility: hidden;
  margin: 0 1rem;
}
.scroll-inner,
.scroll-outer:hover,
.scroll-outer:focus {
  visibility: visible;
}
.item-date {
  vertical-align: middle;
  min-width: 90px;
}
.left-50 {
  left: 50px !important;
}
.left-150 {
  left: 150px !important;
}
.left-320 {
  left: 320px !important;
}
.left-520 {
  left: 520px !important;
}
.left-600 {
  left: 600px !important;
}
#table-bc th, #table-bc td{
  height:44px ;
}
</style>
<style lang="scss" scoped>
::v-deep(.insurance) {
  .dp__action_row, .dp__action_buttons{
  display: none !important;
  }
}
::v-deep(.dp__input_wrap) {
  .dp__input_reg{
  border: 1px solid #607D8B;
  height: 31px;
  }
}
::v-deep(.p-datatable) {
  .p-datatable-tbody > tr > td{
  word-break: break-word;
  }
}
::v-deep(.dp__input_wrap ) {
  .dp__input_reg{
    border:1px solid #ced4da;
  }
}
</style>