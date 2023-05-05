<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../../util/function.js";
import tree_users_hrm from "../component/tree_users_hrm.vue";
import DropdownUser from "../component/DropdownProfiles.vue";
import moment from "moment";
import { forEach } from "jszip";
//Khai báo

const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const isDynamicSQL = ref(false);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  work_schedule_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
const rules = {
  work_schedule_name: {
    required,
    $errors: [
      {
        $property: "work_schedule_name",
        $validator: "required",
        $message: "Tên ca làm việc không được để trống!",
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
            proc: "hrm_work_schedule_count",
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
//Lấy dữ liệu work_schedule
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
              proc: "hrm_work_schedule_list",
              par: [
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
          if (element.datalists) {
            element.datalists = JSON.parse(element.datalists);
            if (element.datalists) {
              element.datalists.forEach((item, i) => {
                item.STT =
                  options.value.PageNo * options.value.PageSize + i + 1;
                if (item.status == "0") item.status = false;
                else item.status = true;
              });
            }
          }
        });

        expandedRows.value = [...data];
        datalists.value = data;

        options.value.loading = false;
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

    options.value.id =
      datalists.value[datalists.value.length - 1].work_schedule_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].work_schedule_id;
    options.value.IsNext = false;
  }
  options.value.PageNo = event.page;
  loadData(true);
};

const work_schedule = ref({
  work_schedule_name: "",
  emote_file: "",
  status: true,
  is_order: 1,
});

const selectedStamps = ref();
const submitted = ref(false);
const v$ = useVuelidate(rules, work_schedule);
const isSaveTem = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = baseURL;
const checkDelList = ref(false);

const options = ref({
  IsNext: true,
  sort: "profile_id",
  SearchText: "",
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
  work_schedule.value = {
    work_schedule_name: "",
    emote_file: "",
    status: true,
    is_order: sttStamp.value,
    organization_id: store.getters.user.organization_id,
    is_system: store.getters.user.is_super ? true : false,
    profile_id_fake: null,
  };
  options.value.profile_id = null;
  checkIsmain.value = false;
  isSaveTem.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};

const closeDialog = () => {
  work_schedule.value = {
    work_schedule_name: "",
    emote_file: "",
    status: true,
    is_order: 1,
  };

  displayBasic.value = false;
  loadData(true);
};
function onGetMonth(date) {
  var month = date.getMonth() + 1;
  var year = date.getFullYear();
  var newMonth = new Date(month + "/01" + "/" + year);
  return newMonth;
}
const onSelectedschedule = () => {
  var arr = [...work_schedule.value.work_schedule_daysfake];
  if (work_schedule.value.work_schedule_monthsfake)
    work_schedule.value.work_schedule_monthsfake.forEach((element) => {
      work_schedule.value.work_schedule_daysfake.forEach((item) => {
        if (onGetMonth(item).getTime() == element.getTime()) {
          arr = arr.filter((x) => x != item);
        }
      });
    });
  work_schedule.value.work_schedule_daysfake = arr;
};
const onSelectedscheduleYears = () => {
  var arr = [...work_schedule.value.work_schedule_daysfake];
  var arrm = [...work_schedule.value.work_schedule_monthsfake];
  if (work_schedule.value.work_schedule_yearsfake)
    work_schedule.value.work_schedule_yearsfake.forEach((element) => {
      work_schedule.value.work_schedule_daysfake.forEach((item) => {
        if (item.getFullYear() == element.getFullYear()) {
          arr = arr.filter((x) => x != item);
        }
      });
      work_schedule.value.work_schedule_monthsfake.forEach((item) => {
        if (item.getFullYear() == element.getFullYear()) {
          arrm = arrm.filter((x) => x != item);
        }
      });
    });
  work_schedule.value.work_schedule_monthsfake = arrm;
  work_schedule.value.work_schedule_daysfake = arr;
};

//Thêm bản ghi

const sttStamp = ref(1);
const saveData = (isFormValid) => {
  submitted.value = true;
  if (
    work_schedule.value.profile_id_fake == null ||
    work_schedule.value.config_work_location_id == null ||
    work_schedule.value.declare_shift_id == null
  ) {
    return;
  }
  let formData = new FormData();

  if (work_schedule.value.profile_id_fake)
    work_schedule.value.profile_id =
      work_schedule.value.profile_id_fake.toString();
  var trr = "";
  if (work_schedule.value.work_schedule_yearsfake) {
    work_schedule.value.work_schedule_years = "";

    work_schedule.value.work_schedule_yearsfake.forEach((element) => {
      work_schedule.value.work_schedule_years +=
        trr + moment(new Date(element)).format("MM/DD/YYYY").toString();
      trr = ",";
    });
  }
  else{
    work_schedule.value.work_schedule_years=null;
  }
   
  trr = "";
  if (work_schedule.value.work_schedule_monthsfake) {
    work_schedule.value.work_schedule_months = "";

    work_schedule.value.work_schedule_monthsfake.forEach((element) => {
      work_schedule.value.work_schedule_months +=
        trr + moment(new Date(element)).format("MM/DD/YYYY").toString();
      trr = ",";
    });
  }
  else{
    work_schedule.value.work_schedule_months=null;
  }
  trr = "";
  if (work_schedule.value.work_schedule_daysfake) {
    work_schedule.value.work_schedule_days = "";

    work_schedule.value.work_schedule_daysfake.forEach((element) => {
      work_schedule.value.work_schedule_days +=
        trr + moment(new Date(element)).format("MM/DD/YYYY").toString();
      trr = ",";
    });
  }  else{
    work_schedule.value.work_schedule_days=null;
  }

  formData.append("hrm_work_schedule", JSON.stringify(work_schedule.value));

  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });

  if (!isSaveTem.value) {
    axios
      .post(
        baseURL + "/api/hrm_work_schedule/add_hrm_work_schedule",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm ca làm việc thành công!");
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
      .put(
        baseURL + "/api/hrm_work_schedule/update_hrm_work_schedule",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa ca làm việc thành công!");

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
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_work_schedule_get",
            par: [{ par: "work_schedule_id", va: dataTem.work_schedule_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

      work_schedule.value = data[0];

      if (work_schedule.value.profile_id)
        work_schedule.value.profile_id_fake =
          work_schedule.value.profile_id.split(",");
      if (work_schedule.value.work_schedule_years) {
        work_schedule.value.work_schedule_yearsfake = [];
        work_schedule.value.work_schedule_years
          .split(",")
          .forEach((element) => {
            work_schedule.value.work_schedule_yearsfake.push(new Date(element));
          });
      }
      if (work_schedule.value.work_schedule_months) {
        work_schedule.value.work_schedule_monthsfake = [];
        work_schedule.value.work_schedule_months
          .split(",")
          .forEach((element) => {
            work_schedule.value.work_schedule_monthsfake.push(
              new Date(element)
            );
          });
      }

      if (work_schedule.value.work_schedule_days) {
        work_schedule.value.work_schedule_daysfake = [];
        work_schedule.value.work_schedule_days.split(",").forEach((element) => {
          work_schedule.value.work_schedule_daysfake.push(new Date(element));
        });
      }
      headerDialog.value = "Sửa ca làm việc";
      isSaveTem.value = true;
      displayBasic.value = true;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
    });
};

const menuButMores = ref();
const itemButMores = ref([
  {
    label: "Hiệu chỉnh nội dung",
    icon: "pi pi-pencil",
    command: (event) => {
      editTem(work_schedule.value);
    },
  },
  {
    label: "Xoá",
    icon: "pi pi-trash",
    command: (event) => {
      delTem(work_schedule.value);
    },
  },
]);
const toggleMores = (event, item) => {
  work_schedule.value = item;

  menuButMores.value.toggle(event);
  //selectedNodes.value = item;
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
          .delete(baseURL + "/api/hrm_work_schedule/delete_hrm_work_schedule", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Tem != null ? [Tem.work_schedule_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá ca làm việc thành công!");
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
    id: "work_schedule_id",
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
    .post(baseURL + "/api/HRM_SQL/Filter_hrm_work_schedule", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          if (element.datalists) {
            element.datalists = JSON.parse(element.datalists);
            if (element.datalists) {
              element.datalists.forEach((item, i) => {
                item.STT =
                  options.value.PageNo * options.value.PageSize + i + 1;
                if (item.status == "0") item.status = false;
                else item.status = true;
              });
            }
          }
        });
        expandedRows.value = [...data];

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
      console.log(error);
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
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const refreshStamp = () => {
  options.value.SearchText = null;
  filterTrangthai.value = null;
  options.value.config_work_location_id = null;
  options.value.profile_id = null;
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
      IntID: value.work_schedule_id,
      TextID: value.work_schedule_id + "",
      IntTrangthai: 1,
      BitTrangthai: value.status,
    };
    axios
      .put(
        baseURL + "/api/hrm_work_schedule/update_s_hrm_work_schedule",
        data,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa trạng thái ca làm việc thành công!");

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
      IntID: value.work_schedule_id,
      TextID: value.work_schedule_id + "",
      BitMain: value.is_default,
    };
    axios
      .put(
        baseURL + "/api/hrm_work_schedule/Update_DefaultStamp",
        data1,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa trạng thái ca làm việc thành công!");

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
      toast.error("Không được xóa ca làm việc mặc định!");
      checkD = true;
      return;
    }
  });
  if (!checkD) {
    swal
      .fire({
        title: "Thông báo",
        text: "Bạn có muốn xoá ca làm việc này không!",
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
            listId.push(item.work_schedule_id);
          });
          axios
            .delete(
              baseURL + "/api/hrm_work_schedule/delete_hrm_work_schedule",
              {
                headers: { Authorization: `Bearer ${store.getters.token}` },
                data: listId != null ? listId : 1,
              }
            )
            .then((response) => {
              swal.close();
              if (response.data.err != "1") {
                swal.close();
                toast.success("Xoá ca làm việc thành công!");
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
  options.value.config_work_location_id = null;
  options.value.profile_id = null;
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
  if (filterTrangthai.value != null) {
    let filterS = {
      filterconstraints: [
        { value: filterTrangthai.value, matchMode: "equals" },
      ],
      filteroperator: "and",
      key: "status",
    };
    filterSQL.value.push(filterS);
  }

  if (options.value.profile_id) {
    let filterS = {
      filterconstraints: [
        {
          value: options.value.profile_id.toString(),
          matchMode: "arrIntersec",
        },
      ],
      filteroperator: "and",
      key: "profile_id",
    };
    filterSQL.value.push(filterS);
  }

  if (options.value.declare_shift_id) {
    let filterS = {
      filterconstraints: [
        { value: options.value.declare_shift_id, matchMode: "equals" },
      ],
      filteroperator: "and",
      key: "declare_shift_id",
    };
    filterSQL.value.push(filterS);
  }

  if (options.value.config_work_location_id) {
    let filterS = {
      filterconstraints: [
        {
          value: options.value.config_work_location_id.toString(),
          matchMode: "equals",
        },
      ],
      filteroperator: "and",
      key: "config_work_location_id",
    };
    filterSQL.value.push(filterS);
  }
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
const listDeclareShift = ref([]);
const listWorkLocation = ref([]);
const datasShift = ref([]);
const initTudien = () => {
  listDeclareShift.value = [];
  listWorkLocation.value = [];
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_declare_shift_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 1000000 },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "status", va: true },
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
        listDeclareShift.value.push({
          name: element.declare_shift_name,
          code: element.declare_shift_id,
        });
      });
      datasShift.value = data;
      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
    });

  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_config_work_location_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 1000000 },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "status", va: true },
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
        listWorkLocation.value.push({
          name: element.config_work_location_name,
          code: element.config_work_location_id,
        });
      });

      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
    });
};

const displayDialogUser = ref(false);

const selectedUser = ref();

const showTreeUser = () => {
  checkMultile.value = false;
  selectedUser.value = [];
  displayDialogUser.value = true;
};

const closeDialogUser = () => {
  displayDialogUser.value = false;
};

const checkMultile = ref(false);

const choiceUser = () => {
  work_schedule.value.profile_id_fake = [];
  if (checkMultile.value == false)
    selectedUser.value.forEach((element) => {
      work_schedule.value.profile_id_fake.push(element.profile_id);
    });
  closeDialogUser();
};
const selectedDecS = ref();
emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "submitModel":
      if (obj.data) {
        work_schedule.value.profile_id_fake = obj.data;
        options.value.profile_id = obj.data;
      }
      break;

    default:
      break;
  }
});
const expandedRows = ref([]);
const onRowExpand = (event) => {
  toast.add({
    severity: "info",
    summary: "Product Expanded",
    detail: event.data.name,
    life: 3000,
  });
};
const onRowCollapse = (event) => {
  toast.add({
    severity: "success",
    summary: "Product Collapsed",
    detail: event.data.name,
    life: 3000,
  });
};
const expandAll = () => {
  expandedRows.value = [...datalists.value];
};
const collapseAll = () => {
  expandedRows.value = null;
};
const onRowSelect = (event) => {
  options.value.declare_shift_id = event.data.declare_shift_id;
  filterFileds();
};
const onRowUnselect = (event) => {
  options.value.declare_shift_id = null;
  filterFileds();
};

onMounted(() => {
  initTudien();
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
  <div class="main-layout true flex-grow-1 p-2 pb-0 pr-0">
    <div class="w-full p-0 surface-0 p-2">
      <h3 class="module-title mt-0 ml-1 mb-2">
        <i class="pi pi-map"></i> Danh sách ca làm việc ({{
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
                (filterTrangthai != null ||
                  options.config_work_location_id != null ||
                  options.profile_id != null) &&
                checkFilter
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
              style="width: 500px"
            >
              <div class="grid formgrid m-0">
                <div class="flex field col-12 p-0">
                  <div
                    class="col-3 text-left pt-2 p-0"
                    style="text-align: left"
                  >
                    Nhân sự
                  </div>
                  <div class="col-9">
                    <DropdownUser
                      :model="options.profile_id"
                      :display="'chip'"
                      :placeholder="'Chọn nhân sự'"
                    />
                  </div>
                </div>
                <div class="flex field col-12 p-0">
                  <div
                    class="col-3 text-left pt-2 p-0"
                    style="text-align: left"
                  >
                    Địa điểm
                  </div>
                  <div class="col-9">
                    <MultiSelect
                      :options="listWorkLocation"
                      :filter="true"
                      :showClear="true"
                      :editable="false"
                      v-model="options.config_work_location_id"
                      optionLabel="name"
                      optionValue="code"
                      placeholder="Chọn địa điểm làm việc"
                      class="w-full limit-width"
                      style="min-height: 36px"
                      panelClass="d-design-dropdown"
                      display="chip"
                    >
                    </MultiSelect>
                  </div>
                </div>
                <div class="flex field col-12 p-0">
                  <div
                    class="col-3 text-left pt-2 p-0"
                    style="text-align: left"
                  >
                    Trạng thái
                  </div>
                  <div class="col-9">
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
            text
            icon="pi pi-sort-amount-up
"
            label="Mở rộng"
            class="mr-2 p-button-secondary"
            @click="expandAll"
          />
          <Button
            text
            class="mr-2 p-button-secondary"
            icon="pi pi-sort-amount-down"
            label="Đóng lại"
            @click="collapseAll"
          />
          <Button
            @click="openBasic('Thêm ca làm việc')"
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
        </template>
      </Toolbar>
    </div>
    <Splitter class="w-full h-full">
      <SplitterPanel
        class="flex align-items-center justify-content-center"
        :size="15"
        :minSize="15"
      >
        <DataTable
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
          :value="datasShift"
          removableSort
          v-model:rows="options.PageSize"
          selectionMode="single"
          dataKey="declare_shift_id"
          class="w-full"
          responsiveLayout="scroll"
          v-model:selection="selectedDecS"
          :row-hover="true"
          @rowSelect="onRowSelect"
          @rowUnselect="onRowUnselect"
        >
          <Column
            field="declare_shift_name"
            header="Ca làm việc"
            headerClass="align-items-center justify-content-center text-center"
            headerStyle="text-align:left;height:50px"
            bodyStyle="text-align:left;cursor:pointer"
          >
            <template #body="data">
              <Chip
                :label="data.data.declare_shift_name"
                :style="{
                  backgroundColor: data.data.background_color,
                  color: data.data.text_color,
                }"
                class="w-full format-center"
              />
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
      </SplitterPanel>
      <SplitterPanel class="w-full" :size="85" :minSize="85">
        <DataTable
          @page="onPage($event)"
          @sort="onSort($event)"
          @filter="onFilter($event)"
          v-model:expandedRows="expandedRows"
          :value="datalists"
          dataKey="profile_id"
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
          :paginator="false"
          class="w-full"
          removableSort
          v-model:rows="options.PageSize"
          paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
          :rowsPerPageOptions="[20, 30, 50, 100, 200]"
          responsiveLayout="scroll"
          v-model:selection="selectedStamps"
          :row-hover="true"
        >
          <Column
            expander
            headerStyle="text-align:center;max-width:50px;height:50px"
            bodyStyle="text-align:center;max-width:50px;"
          />
          <Column
            field="candidate_code"
            header="Ảnh"
            headerStyle="text-align:center;max-width:70px;height:50px"
            bodyStyle="text-align:center;max-width:70px"
            class="align-items-center justify-content-center text-center"
          >
            <template #body="slotProps">
              <div>
                <Avatar
                  style="color: #fff"
                  v-bind:label="
                    slotProps.data.avatar
                      ? ''
                      : (slotProps.data.profile_user_name ?? '')
                          .substring(0, 1)
                          .toUpperCase()
                  "
                  v-bind:image="
                    slotProps.data.avatar
                      ? basedomainURL + slotProps.data.avatar
                      : basedomainURL + '/Portals/Image/noimg.jpg'
                  "
                  class="w-3rem"
                  size="large"
                  :style="
                    slotProps.data.avatar
                      ? 'background-color: #2196f3'
                      : 'background:' + bgColor[slotProps.index % 7]
                  "
                  shape="circle"
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/nouser1.png'
                  "
                />
              </div>
            </template>
          </Column>

          <Column
            field="profile_user_name"
            header="Họ và tên"
            headerClass="align-items-center justify-content-center text-center"
            headerStyle="text-align:center; height:50px"
            bodyStyle=" "
          >
            <template #body="data">
              <div class="font-bold">
                {{ data.data.profile_user_name }}
              </div>
            </template>
          </Column>

          <Column
            field="position_name"
            header="Chức vụ"
            class="align-items-center justify-content-center text-center"
            headerStyle="text-align:center;max-width:200px;height:50px"
            bodyStyle=" max-width:200px"
          >
          </Column>
          <Column
            field="work_position_name"
            header="Vị trí"
            class="align-items-center justify-content-center text-center"
            headerStyle="text-align:center;max-width:250px;height:50px"
            bodyStyle=" max-width:250px"
          >
          </Column>
          <Column
            field="department_name"
            header="Phòng ban"
            class="align-items-center justify-content-center text-center"
            headerStyle="text-align:center;max-width:250px;height:50px"
            bodyStyle=" max-width:250px"
          >
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

          <template #expansion="slotProps">
            <div class="p-0 w-full">
              <DataTable :value="slotProps.data.datalists">
                <Column
                  field="declare_shift_name"
                  header="Ca làm việc"
                  headerStyle="text-align:center;max-width:200px;height:50px"
                  bodyStyle="text-align:center;max-width:200px"
                  class="align-items-center justify-content-center text-center"
                  bodyClass="bg-indigo-50"
                >
                  <template #body="data">
                    <div
                      style="border-radius: 16px; padding: 0 0.5rem"
                      :style="{
                        backgroundColor: data.data.background_color,
                        color: data.data.text_color,
                      }"
                      class="w-full"
                    >
                      <div>{{ data.data.declare_shift_name }}</div>

                      <div class="text-sm pb-1">
                        {{ data.data.config_work_location_name }}
                      </div>
                    </div>
                  </template>
                </Column>
                <Column
                  field="status"
                  header="Thời gian"
                  headerClass="align-items-center justify-content-center text-center"
                  headerStyle="text-align:center; height:50px"
                  bodyClass="bg-indigo-50"
                >
                  <template #body="data">
                    <div class="w-full">
                      <div class="flex" v-if="data.data.is_full_time == '0'">
                        <div v-if="data.data.work_schedule_years">
                          <Chip
                            v-for="(
                              item, index
                            ) in data.data.work_schedule_years.split(',')"
                            :key="index"
                            class="m-1 bg-blue-500"
                            :label="
                              moment(new Date(item)).format('YYYY').toString()
                            "
                          ></Chip>
                        </div>
                        <div v-if="data.data.work_schedule_months">
                          <Chip
                            v-for="(
                              item, index
                            ) in data.data.work_schedule_months.split(',')"
                            :key="index"
                            class="m-1 bg-blue-300"
                            :label="
                              moment(new Date(item))
                                .format('MM/YYYY')
                                .toString()
                            "
                          ></Chip>
                        </div>
                        <div v-if="data.data.work_schedule_days">
                          <Chip
                            v-for="(
                              item, index
                            ) in data.data.work_schedule_days.split(',')"
                            :key="index"
                            class="m-1 bg-bluegray-300"
                            :label="
                              moment(new Date(item))
                                .format('DD/MM/YYYY')
                                .toString()
                            "
                          ></Chip>
                        </div>
                      </div>
                      <div class="flex" v-else>Toàn thời gian</div>
                    </div>
                  </template>
                </Column>

                <Column
                  field="status"
                  header="Trạng thái"
                  headerStyle="text-align:center;max-width:100px;height:50px"
                  bodyStyle="text-align:center;max-width:100px"
                  class="align-items-center justify-content-center text-center"
                  bodyClass="bg-indigo-50"
                >
                  <template #body="data">
                    <Checkbox
                      :disabled="
                        !(
                          store.state.user.is_super == true ||
                          store.state.user.user_id == data.data.created_by ||
                          (store.state.user.role_id == 'admin' &&
                            store.state.user.organization_id ==
                              data.data.organization_id)
                        )
                      "
                      :binary="true"
                      v-model="data.data.status"
                      @click="onCheckBox(data.data, true, true)"
                    /> </template
                ></Column>
                <Column
                  header=""
                  headerStyle="text-align:center;max-width:50px"
                  bodyStyle="text-align:center;max-width:50px"
                  class="align-items-center justify-content-center text-center"
                  bodyClass="bg-indigo-50"
                >
                  <template #body="slotProps">
                    <Button
                      icon="pi pi-ellipsis-h"
                      class="p-button-rounded p-button-text ml-2"
                      @click="toggleMores($event, slotProps.data)"
                      aria-haspopup="true"
                      aria-controls="overlay_More"
                    />
                  </template>
                </Column>

                <template #empty>
                  <div
                    class="align-items-center justify-content-center p-4 text-center m-auto"
                    v-if="!isFirst"
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
          </template>
        </DataTable>
      </SplitterPanel>
    </Splitter>
  </div>
  <Menu
    id="overlay_More"
    ref="menuButMores"
    :model="itemButMores"
    :popup="true"
  />
  <tree_users_hrm
    v-if="displayDialogUser === true"
    :headerDialog="'Chọn nhân sự ca làm việc'"
    :displayDialog="displayDialogUser"
    :closeDialog="closeDialogUser"
    :one="checkMultile"
    :selected="selectedUser"
    :choiceUser="choiceUser"
  />
  <Dialog
    :header="headerDialog"
    v-model:visible="displayBasic"
    :style="{ width: '35vw' }"
    :closable="true"
    :modal="true"
  >
    <form>
      <div class="grid formgrid px-2">
        <div class="field col-12 flex align-items-center md:col-12">
          <label class="col-3 text-left p-0"
            >Ca làm việc <span class="redsao">(*)</span></label
          >
          <Dropdown
            :filter="true"
            v-model="work_schedule.declare_shift_id"
            :options="listDeclareShift"
            optionLabel="name"
            optionValue="code"
            class="col-9 p-0"
            panelClass="d-design-dropdown"
            placeholder="Chọn ca làm việc"
            :class="{
              'p-invalid': work_schedule.declare_shift_id == null && submitted,
            }"
          />
        </div>
        <div class="field flex align-items-center col-12 md:col-12">
          <label class="col-3 text-left p-0"
            >Địa điểm làm việc <span class="redsao">(*)</span></label
          >
          <Dropdown
            :filter="true"
            v-model="work_schedule.config_work_location_id"
            :options="listWorkLocation"
            optionLabel="name"
            optionValue="code"
            class="col-9 p-0"
            panelClass="d-design-dropdown"
            placeholder="Chọn địa điểm làm việc"
            :class="{
              'p-invalid':
                work_schedule.config_work_location_id == null && submitted,
            }"
          />
        </div>
        <div class="field flex align-items-center col-12 md:col-12">
          <div class="col-3 p-0 flex align-items-center">
            <div class="text-left p-0">
              Nhân sự <span class="redsao">(*)</span>
            </div>
            <Button
              v-tooltip.top="'Chọn nhân sự'"
              @click="showTreeUser()"
              icon="pi pi-user-plus"
              class="p-button-text p-button-rounded"
              v-if="!isSaveTem"
            />
          </div>
          <div class="col-9 p-0">
            <DropdownUser
              :model="work_schedule.profile_id_fake"
              :display="'chip'"
              :placeholder="'Chọn nhân sự'"
              :disabled="isSaveTem"
              :class="{
                'p-invalid': work_schedule.profile_id_fake == null && submitted,
              }"
            />
          </div>
        </div>
        <div class="flex field col-12 md:col-12">
          <div class="col-6 p-0 align-items-center flex">
            <div class="col-6 p-0 flex align-items-center">Toàn thời gian</div>
            <div class="col-6 p-0">
              <InputSwitch
                class="w-4rem lck-checked"
                v-model="work_schedule.is_full_time"
              />
            </div>
          </div>
          <div class="col-6 p-0 align-items-center flex">
            <div class="col-6 p-0 flex align-items-center">Trạng thái</div>
            <div class="col-6 p-0">
              <InputSwitch
                class="w-4rem lck-checked"
                v-model="work_schedule.status"
              />
            </div>
          </div>
        </div>
        <div
          class="flex field align-items-center col-12 md:col-12"
          v-if="!work_schedule.is_full_time"
        >
          <div class="col-3 p-0 flex align-items-center">Đăng ký năm</div>
          <div class="col-9 p-0">
            <Calendar
              v-model="work_schedule.work_schedule_yearsfake"
              view="year"
              dateFormat="yy"
              class="w-full"
              :showIcon="true"
              selectionMode="multiple"
              @date-select="onSelectedscheduleYears($event)"
            >
              
            </Calendar>
          </div>
        </div>
        <div
          class="flex field align-items-center col-12 md:col-12"
          v-if="!work_schedule.is_full_time"
        >
          <div class="col-3 p-0 flex align-items-center">Đăng ký tháng</div>
          <div class="col-9 p-0">
            <Calendar
              v-model="work_schedule.work_schedule_monthsfake"
              view="month"
              dateFormat="mm/yy"
              class="w-full"
              :showOnFocus="false"
              :showIcon="true"
              selectionMode="multiple"
              @date-select="onSelectedschedule($event)"
            >
            </Calendar>
          </div>
        </div>

        <div
          class="flex align-items-center col-12 md:col-12"
          v-if="!work_schedule.is_full_time"
        >
          <div class="col-3 p-0 flex align-items-center">Đăng ký ngày</div>
          <div class="col-9 p-0">
            <Calendar
              v-model="work_schedule.work_schedule_daysfake"
              selectionMode="multiple"
              class="w-full"
              :showIcon="true"
              :showOnFocus="false"
              @date-select="onSelectedschedule($event)"
            >
              <!-- <template #date="slotProps">
                <strong
                  v-if="slotProps.date.day > 10 && slotProps.date.day < 15"
                  style="text-decoration: line-through"
                  >{{ slotProps.date.day }}</strong
                >
                <template v-else>{{ slotProps.date.day }}</template>
              </template> -->
            </Calendar>
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
        @click="saveData(!v$.$invalid)"
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
    