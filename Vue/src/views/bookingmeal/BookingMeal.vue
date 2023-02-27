<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { useRouter, useRoute } from "vue-router";
import ClassicEditor from "@ckeditor/ckeditor5-build-classic";
import moment from "moment";
import { encr } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
//Khai báo
const route = useRoute();
const timeInit = ref({});
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const selectedBookings = ref();
const checkDelList = ref(false);
const isFirstVideo = ref(false);
const users = ref([]);
const isAdd = ref(true);
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const rules = {
  title: {
    required,
  },
};
const first = ref(1);
const filterButs = ref();
const showFilter = ref(false);
const treedonvis = ref();
const selectCapcha = ref();
selectCapcha.value = {};
selectCapcha.value[store.getters.user.organization_id] = true;
//Lọc
const checkFilter = ref(false);
const toggleFilter = (event) => {
  filterButs.value.toggle(event);
};
const refilterBooking = () => {
  checkFilter.value = false;
  showFilter.value = false;
  selectCapcha.value[store.getters.user.organization_id] = true;
  options.value.department_id = null;
  loadData(true);
};
const filterBooking = () => {
  let keys = Object.keys(selectCapcha.value);
  options.value.department_id = parseInt(keys[0]);
  checkFilter.value = true;
  loadData(true);
};
//Refresh
const onRefersh = () => {
  checkFilter.value = false;
  selectedBookings.value = [];
  options.value = {
    IsNext: true,
    search: "",
    pageno: 1,
    pagesize: 20,
    user_id: store.getters.user.user_id,
    status: null,
    tenstatus: "",
  };
  first.value = 1;
  loadData(true);
};
//Phân trang dữ liệu
const isPaginator = ref(false);
const onPage = (event) => {
  if (event.rows != options.value.pagesize) {
    options.value.pagesize = event.rows;
  }

  options.value.pageno = event.page + 1;
  loadData();
};
const filterSQL = ref([]);
const isDynamicSQL = ref(false);
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  title: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  start_date: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_IS }],
  },
  is_hot: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.EQUALS }],
  },
  status: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.EQUALS }],
  },
});
//Sort
const loadDataSQL = () => {
  let data = {
    id: null,
    next: options.value.IsNext,
    sqlO: options.value.sort,
    Search: options.value.search,
    PageNo: options.value.pageno,
    PageSize: options.value.pagesize,
    fieldSQLS: filterSQL.value,
  };

  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filterbooking_main", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);

      let data = dt[0];

      if (data.length > 0) {
        data.forEach((element, i) => {
          element.is_sort = i + 1;
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
        controller: "BookingMeal.vue",
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
const onSort = (event) => {
  options.value.sort =
    event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
  if (event.sortField == "STT") {
    options.value.sort = "is_order" + (event.sortOrder == 1 ? " ASC" : " DESC");
  }
  options.value.pageno = 1;
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

  options.value.pageno = 1;
  first.value = 1;
  options.value.id = null;
  isDynamicSQL.value = true;
  loadData(true);
};

const toast = useToast();
const isFirst = ref(true);
const datalists = ref();
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
//ADD log
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
const submitted = ref(false);
const options = ref({
  IsNext: true,
  sort: "video_id DESC",
  search: "",
  pageno: 1,
  pagesize: 20,
  department_id: null,
  loading: true,
  totalRecords: null,
});
const video = ref({});
const booking = ref({
  listdates: [],
});
const user = ref({});
const v$ = useVuelidate(rules, video);
//METHOD
const handleSubmit = () => {
  submitted.value = true;
  if (booking.value.user == null) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng chọn người cần cắt cơm",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  } else booking.value.user_id = booking.value.user.user_id;
  if (booking.value.listdates.length == 0) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng chọn ngày cắt cơm",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  var formData = new FormData();
  formData.append("booking", JSON.stringify(booking.value));
  formData.append("listdates", JSON.stringify(booking.value.listdates));
  axios({
    method: isAdd.value == false ? "put" : "post",
    url:
      baseURL +
      `/api/BookingMeal/${
        isAdd.value == false ? "update_booking" : "add_booking"
      }`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Thêm phiếu báo thành công!");
        loadData(true);
        closeDialog();
      } else {
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const loadCount = () => {
  axios
    .post(
        baseURL + "/api/BookingMeal/GetDataProc",
        {
          str: encr(JSON.stringify({
            proc: "booking_meal_count",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "department_id", va: options.value.department_id },
          { par: "search", va: options.value.search },
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
      } else options.value.totalRecords = 0;
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
//Hiển thị dialog
const headerDialog = ref();
const invalidDates = ref([]);
const displayBasic = ref(false);
const onChangeUser = (user_id) => {
  axios
    .post(
        baseURL + "/api/BookingMeal/GetDataProc",
        {
          str: encr(JSON.stringify({
            proc: "booking_getdate_by_user",
        par: [{ par: "user_id", va: user_id }],
          }), SecretKey, cryoptojs
          ).toString()
        },
        config
      )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      invalidDates.value = [];
      if (data[0].length > 0) {
        data[0].forEach((item) => {
          let dt = new Date(item.booking_date);
          let datedis = new Date(dt.getFullYear(), dt.getMonth(), dt.getDate());
          if (
            booking.value.listdates.filter(
              (x) =>
                x.getFullYear() == datedis.getFullYear() &&
                x.getDate() == datedis.getDate() &&
                x.getMonth() == datedis.getMonth()
            ).length == 0
          ){
            invalidDates.value.push(datedis);
          }
          let current_time = new Date();
          //sau 3h chieu -> khong duoc huy dang ky cua ngay hom sau
          if(current_time.getHours() >= 15){
            if(current_time.getFullYear() == datedis.getFullYear() 
              && current_time.getMonth() == datedis.getMonth()
              && current_time.getDate()+1 == datedis.getDate()){
            invalidDates.value.push(datedis);
              }
          }

        });
      }
      invalidDates.value = invalidDates.value.concat(
        timeInit.value.working_days
      );
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
const openBasic = (str) => {
  // loadRelate();
  isAdd.value = true;
  invalidDates.value = [];
  booking.value = {
    is_order: options.value.totalRecords + 1,
    organization_id: store.getters.user.organization_id,
    user_id: store.getters.user.user_id,
    reason: null,
    full_name: store.getters.user.full_name,
    created_date: new Date(),
    listdates: [],
    user:{user_id: store.getters.user.user_id, full_name: store.getters.user.full_name, avatar: store.getters.user.avatar}
  };
  let dt = new Date();
  if (dt.getHours() < 15) {
    booking.value.unselect_date = new Date(
      dt.getFullYear(),
      dt.getMonth(),
      dt.getDate() + 1
    );
  } else
    booking.value.unselect_date = new Date(
      dt.getFullYear(),
      dt.getMonth(),
      dt.getDate() + 2
    );
    //onChangeUser(store.getters.user.user_id);
  submitted.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};
const closeDialog = () => {
  isFirstVideo.value = false;
  //loadRelate();
  displayBasic.value = false;
};
//sửa
const editBooking = (data) => {
   headerDialog.value = "Chỉnh sửa phiếu";
  invalidDates.value = [];
  displayBasic.value = true;
  submitted.value = false;
  isAdd.value = false;
  axios
    .post(
        baseURL + "/api/BookingMeal/GetDataProc",
        {
          str: encr(JSON.stringify({
            proc: "booking_meal_get",
        par: [{ par: "booking_id", va: data.booking_id }],
          }), SecretKey, cryoptojs
          ).toString()
        },
        config
      )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data[0].length > 0) {
        booking.value = data[0][0];
        booking.value.created_date = new Date(booking.value.created_date);
        booking.value.user = {
          user_id: booking.value.user_id,
          full_name: booking.value.full_name,
          avatar: booking.value.avatar,
        };
        booking.value.listdates = [];
      }
      if (data[1].length > 0) {
        data[1].forEach((item) => {
          let dt = new Date(item.booking_date);
          let datedis = new Date(dt.getFullYear(), dt.getMonth(), dt.getDate());
          booking.value.listdates.push(datedis);
        });
        // get list date selected by user
        if (booking.value.user_id) onChangeUser(booking.value.user_id);
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
//xóa
const delBooking = (bk) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá phiếu này không!",
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
          .delete(baseURL + "/api/BookingMeal/Del_Booking", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: [bk.booking_id],
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá phiếu thành công!");
              loadData(true);
            } else {
              swal.fire({
                title: "Thông báo!",
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
                title: "Thông báo!",
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
//Xóa nhiều
const deleteList = () => {
  let listId = new Array(selectedBookings.length);

  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xóa danh sách phiếu này không!",
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
        selectedBookings.value.forEach((item) => {
          listId.push(item.booking_id);
        });
        axios
          .delete(baseURL + "/api/BookingMeal/Del_Booking", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: listId != null ? listId : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá phiếu thành công!");
              checkDelList.value = false;

              loadData(true);
            } else {
              swal.fire({
                title: "Thông báo!",
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
                title: "Thông báo!",
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
//list
const loadData = (rf) => {
  if (rf) {
    options.value.loading = true;
    loadCount();
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
        baseURL + "/api/BookingMeal/GetDataProc",
        {
          str: encr(JSON.stringify({
            proc: "booking_meal_list",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "department_id", va: options.value.department_id },
          { par: "search", va: options.value.search },
          { par: "pageno", va: options.value.pageno || 1 },
          { par: "pagesize", va: options.value.pagesize || 20 },
        ],
          }), SecretKey, cryoptojs
          ).toString()
        },
        config
      )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      swal.close();
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.is_sort =
            (options.value.pageno - 1) * options.value.pagesize + i + 1;

          element.created_date = moment(new Date(element.created_date)).format(
            "DD-MM-YYYY"
          );
        });
        datalists.value = data;
        if(route.params.id !== null){
          //nhan thong bao tu notify
          openDetail_Noti(route.params.id);
        }
      } else {
        datalists.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      options.value.loading = false;
    })
    .catch((error) => {
      console.log(error);
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
//nhan notify
const openDetail_Noti = (id)=>{
  let arr = datalists.value.filter(x => x.booking_id == id);
  if (arr.length > 0) editBooking(arr[0]);
}
const loadUser = () => {
  axios
    .post(
        baseURL + "/api/BookingMeal/GetDataProc",
        {
          str: encr(JSON.stringify({
            proc: "booking_meal_user_list",
        par: [{ par: "user_id", va: store.getters.user.user_id }],
          }), SecretKey, cryoptojs
          ).toString()
        },
        config
      )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        users.value = data[0];
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
const initTudien = () => {
  axios
    .post(
        baseURL + "/api/BookingMeal/GetDataProc",
        {
          str: encr(JSON.stringify({
            proc: "booking_meal_dictionary",
        par: [{ par: "user_id", va: store.getters.user.user_id }],
          }), SecretKey, cryoptojs
          ).toString()
        },
        config
      )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data[0].length > 0) {
        let obj = renderTreeDV(
          data[0],
          "organization_id",
          "organization_name",
          "phòng ban"
        );
        treedonvis.value = obj.arrtreeChils;
      }
    })
    .catch((error) => {
      console.log(error);
    });
};
const initConfig = () => {
  axios
    .get(baseURL + "/api/BookingMeal/GetConfig", {
      headers: { Authorization: `Bearer ${store.getters.token}` },
    })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        timeInit.value = response.data.data;
        if (timeInit.value.working_days) {
          for (let i = 0; i < timeInit.value.working_days.length; i++) {
            timeInit.value.working_days[i] = new Date(
              timeInit.value.working_days[i]
            );
          }
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
//Khai báo function
const renderTreeDV = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      m.IsOrder = i + 1;
      let om = { key: m[id], data: m };
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em) => {
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

//Tìm kiếm
watch(selectedBookings, () => {
  if (selectedBookings.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});
onMounted(() => {
  loadData(true);
  loadUser();
  initConfig();
  initTudien();
  return {};
});
</script>

<template>
  <div class="d-container">
    <div class="d-lang-header">
      <h3 class="d-module-title">
        <i class="pi pi-id-card"></i> Danh sách phiếu ({{
          options.totalRecords
        }})
      </h3>
    </div>
    <Toolbar class="d-toolbar">
      <template #start>
        <span class="p-input-icon-left">
          <i class="pi pi-search" />
          <InputText
            v-model="options.search"
            @keyup.enter="loadData(true)"
            type="text"
            spellcheck="false"
            placeholder="Tìm kiếm"
          />
          <Button
            :class="
              checkFilter ? 'ml-2' : 'ml-2 p-button-secondary p-button-outlined'
            "
            icon="pi pi-filter"
            @click="toggleFilter"
            aria-haspopup="true"
            aria-controls="overlay_panelS"
          />
          <OverlayPanel
            ref="filterButs"
            appendTo="body"
            :showCloseIcon="false"
            id="overlay_panelS"
            style="width: 350px"
            :breakpoints="{ '960px': '20vw' }"
          >
            <div class="grid formgrid m-2">
              <div class="field col-12 md:col-12 flex align-items-center">
                <div class="col-4 p-0">Phòng ban:</div>
                <!-- <Dropdown
                  v-model="filterTrangthai"
                  :options="options_status"
                  optionLabel="name"
                  optionValue="code"
                  placeholder="Trạng thái"
                  class="col-8 p-0"
                /> -->
                <TreeSelect
                  class="col-8 p-0"
                  v-model="selectCapcha"
                  :options="treedonvis"
                  :showClear="true"
                  :max-height="200"
                  placeholder="Chọn phòng ban"
                  optionLabel="organization_name"
                  optionValue="organization_id"
                >
                </TreeSelect>
              </div>
              <div class="col-12 field p-0">
                <Toolbar class="toolbar-filter">
                  <template #start>
                    <Button
                      @click="refilterBooking"
                      class="p-button-outlined"
                      label="Xóa"
                    ></Button>
                  </template>
                  <template #end>
                    <Button @click="filterBooking" label="Lọc"></Button>
                  </template>
                </Toolbar>
              </div>
            </div>
          </OverlayPanel>
        </span>
        <!-- <TreeSelect
          style="margin-left: 24px; min-width: 200px"
          @change="selectTree()"
          v-model="menu_IDNode"
          :options="danhMuc"
          placeholder="Tất cả video"
        ></TreeSelect> -->
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
          @click="openBasic('Phiếu cắt cơm')"
          label="Đăng ký cắt cơm"
          icon="pi pi-plus"
          class="mr-2"
        />
        <Button
          class="mr-2 p-button-outlined p-button-secondary"
          icon="pi pi-refresh"
          @click="onRefersh"
        />
      </template>
    </Toolbar>
    <div class="d-lang-table">
      <DataTable
        class="w-full p-datatable-sm e-sm"
        @nodeSelect="onNodeSelect"
        @nodeUnselect="onNodeUnselect"
        @page="onPage($event)"
        @filter="onFilter($event)"
        @sort="onSort($event)"
        v-model:filters="filters"
        filterDisplay="menu"
        filterMode="lenient"
        dataKey="booking_id"
        responsiveLayout="scroll"
        :scrollable="true"
        scrollHeight="flex"
        :showGridlines="true"
        :rows="options.pagesize"
        :lazy="true"
        :value="datalists"
        :loading="options.loading"
        :paginator="true"
        :rowsPerPageOptions="[20, 30, 50, 100, 200]"
        :totalRecords="options.totalRecords"
        :row-hover="true"
        v-model:selection="selectedBookings"
        v-model:first="first"
      >
        <Column
          v-if="options.totalRecords > 0"
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:50px;height:50px"
          bodyStyle="text-align:center;max-width:50px"
          selectionMode="multiple"
        >
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:70px;height:50px"
          bodyStyle="text-align:center;max-width:70px"
          field="is_order"
          header="STT"
        >
          <template #body="data">
            <div
              :style="
                data.data.status == 2
                  ? 'color:#689F38'
                  : data.data.status == 3
                  ? 'color:#FBC02D'
                  : data.data.status == 4
                  ? 'color:#D32F2F'
                  : ''
              "
            >
              {{ data.data.is_sort }}
            </div>
          </template>
        </Column>
        <Column
          headerStyle="text-align:left;height:50px;max-width:160px;"
          bodyStyle="text-align:left;;max-width:160px;"
          field="users_name"
          header="Người cắt cơm"
        >
          <template #body="data">
            <div
              :style="
                data.data.status == 2
                  ? 'color:#689F38'
                  : data.data.status == 3
                  ? 'color:#FBC02D'
                  : data.data.status == 4
                  ? 'color:#D32F2F'
                  : ''
              "
            >
              {{ data.data.users_name }}
            </div>
          </template>
          <!-- <template #filter="{ filterModel }">
            <InputText
              type="text"
              v-model="filterModel.value"
              class="p-column-filter"
              placeholder="Tài khoản"
            />
          </template> -->
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;height:50px;max-width:200px"
          bodyStyle="text-align:center;;max-width:200px"
          field="department_name"
          header="Phòng ban"
        >
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;height:50px;max-width:200px"
          bodyStyle="text-align:center;max-width:200px"
          field="listdate"
          header="Ngày cắt cơm"
        >
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;height:50px"
          bodyStyle="text-align:center;"
          field="reason"
          header="Lý do"
        >
          <template #body="data">
            <div class="text-3line text-left">
              {{ data.data.reason }}
            </div>
          </template>
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;height:50px;max-width:150px"
          bodyStyle="text-align:center;max-width:150px"
          field="created_name"
          header="Người lập"
        >
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;height:50px;max-width:150px"
          bodyStyle="text-align:center;;max-width:150px"
          field="created_date"
          header="Ngày lập"
        >
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:100px;height:50px"
          bodyStyle="text-align:center;max-width:100px"
          header="Chức năng"
        >
          <template #body="data">
            <Button
              v-tooltip.top="'Sửa phiếu'"
              @click="editBooking(data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              :style="
                data.data.status == 2
                  ? 'color:#689F38'
                  : data.data.status == 3
                  ? 'color:#FBC02D'
                  : data.data.status == 4
                  ? 'color:#D32F2F'
                  : ''
              "
            ></Button>
            <Button
              v-tooltip.top="'Xóa phiếu'"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-trash"
              :style="
                data.data.status == 2
                  ? 'color:#689F38'
                  : data.data.status == 3
                  ? 'color:#FBC02D'
                  : data.data.status == 4
                  ? 'color:#D32F2F'
                  : ''
              "
              @click="delBooking(data.data)"
            ></Button>
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
      </DataTable>
    </div>
  </div>
  <Dialog
    @hide="closeDialog"
    :header="headerDialog"
    v-model:visible="displayBasic"
    :maximizable="true"
    :style="{ width: '52vw', zIndex: 2 }"
    :closable="true"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="text-left col-2"
            >Người cắt cơm <span class="redsao">(*)</span></label
          >
          <Dropdown
            @change="onChangeUser(booking.user.user_id)"
            v-model="booking.user"
            :options="users"
            optionLabel="full_name"
            placeholder="Chọn người"
            :showClear="true"
            :filter="true"
            class="col-10"
          >
            <template #value="slotProps">
              <div class="flex align-items-center" v-if="slotProps.value">
                <Avatar
                  v-bind:label="
                    slotProps.value.avatar
                      ? ''
                      :slotProps.value ? slotProps.value.full_name.split(' ').at(-1).substring(0, 1).toUpperCase()
                      :'A'                  
                      "
                  v-bind:image="basedomainURL + slotProps.value.avatar"
                  style="
                    background-color: #2196f3;
                    color: #ffffff;
                    vertical-align: middle;
                  "
                  :style="
                    slotProps.value.avatar
                      ? 'background-color: #2196f3'
                      : 'background:' +
                        bgColor[slotProps.value.full_name.length % 7]
                  "
                  class="mr-2"
                  size="small"
                  shape="circle"
                />
                <span>
                  {{ slotProps.value.full_name }}
                </span>
              </div>
              <span v-else>
                {{ slotProps.placeholder }}
              </span>
            </template>
            <template #option="slotProps">
              <div>
                <Avatar
                  v-bind:label="
                    slotProps.option.avatar
                      ? ''
                      :slotProps.option.full_name? slotProps.option.full_name.split(' ').at(-1).substring(0, 1).toUpperCase()
                      :'A'
                  "
                  v-bind:image="basedomainURL + slotProps.option.avatar"
                  style="
                    background-color: #2196f3;
                    color: #ffffff;
                    vertical-align: middle;
                  "
                  :style="
                    slotProps.option.avatar
                      ? 'background-color: #2196f3'
                      : 'background:' +
                        bgColor[slotProps.option.full_name.length % 7]
                  "
                  class="mr-2"
                  size="small"
                  shape="circle"
                />
                <span class="image-text">{{ slotProps.option.full_name }}</span>
              </div>
            </template>
          </Dropdown>
        </div>
        <div class="field col-12 md:col-12">
          <label class="text-left col-2" for="disableddays"
            >Chọn ngày <span class="redsao">(*)</span></label
          >
          <Calendar
            class="col-10"
            inputId="disableddays"
            selectionMode="multiple"
            :manualInput="false"
            :showIcon="true"
            autocomplete="on"
            :disabledDates="invalidDates"
            :minDate="
              booking.unselect_date ? booking.unselect_date : new Date()
            "
            v-model="booking.listdates"
            autoZIndex="true"
          >
          </Calendar>
        </div>
        <div class="field col-12 md:col-12 flex">
          <label class="col-2 text-left">Lý do</label>
          <Textarea
            style="border-radius: 5px;padding:0.5rem"
            class="col-10"
            spellcheck="false"
            :autoResize="true"
            rows="3"
            v-model="booking.reason"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Người lập</label>
          <InputText
            class="col-4 ip36"
            spellcheck="false"
            v-model="booking.full_name"
            disabled
          />
          <label class="col-2 text-right">Ngày lập</label>
          <Calendar
            inputId="basic"
            v-model="booking.created_date"
            autocomplete="off"
            Disabled="true"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">STT</label>
          <InputNumber class="col-2 ip36 p-0" v-model="booking.is_order" />
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog()"
        class="p-button-text"
      />

      <Button
        @click="handleSubmit()"
        label="Lưu"
        icon="pi pi-check"
        autofocus
      />
    </template>
  </Dialog>
</template>
<style scoped>
.special-day {
  text-decoration: line-through;
}

.text-3line {
  text-overflow: ellipsis;
  overflow: hidden;
  column-gap: initial;
  -webkit-line-clamp: 3;
  display: -webkit-box;
  -webkit-box-orient: vertical;
}
.p-calendar-w-btn {
  padding: 0px !important;
}
.icon-modules {
  width: 18px;
  height: 18px;
}
.d-container {
  background-color: #f5f5f5;
}

.d-lang-header {
  background-color: #ffff;
  padding: 12px 8px 0px 8px;
  margin: 8px 8px 0px 8px;
  height: 33px;
}
.d-lang-header h3,
i {
  font-weight: 600;
}
.d-module-title {
  margin: 0;
}
.d-lang-table {
  margin: 0px 8px 0px 8px;
  height: calc(100vh - 150px);
}

.d-toolbar {
  border: unset;
  outline: unset;
  background-color: #fff;
  margin: 0px 8px 0px 8px;
}

.d-btn-function {
  border-radius: 50%;
  margin-left: 6px;
}
.d-btn-infor {
  background-color: rgb(56, 180, 187);
  border: 1px solid rgb(106, 173, 139);
}
.d-btn-infor:hover {
  background-color: rgb(125, 221, 150);
  border: 1px solid rgb(214, 125, 125);
}
.d-btn-edit:hover {
  background-color: rgb(63, 46, 252);
}

.toolbar-filter {
  border: unset;
  outline: unset;
  background-color: #fff;
  padding-bottom: 0px;
}
@keyframes p-progress-spinner-color {
  100%,
  0% {
    stroke: #858585 !important;
  }
  40% {
    stroke: #858585 !important;
  }
  66% {
    stroke: #858585 !important;
  }
  80%,
  90% {
    stroke: #858585 !important;
  }
}
</style>
<style lang="scss" scoped>
::v-deep(.p-avatar) {
  .p-avatar-text {
    font-size: 1.125rem;
  }
}
</style>