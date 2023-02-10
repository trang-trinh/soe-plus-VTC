<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../../util/function.js";

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
  training_emps_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
const rules = {
  training_emps_code: {
    required,
    $errors: [
      {
        $property: "training_emps_code",
        $validator: "required",
        $message: "Tên đào tạo không được để trống!",
      },
    ],
  },
  training_emps_name: {
    required,
    $errors: [
      {
        $property: "training_emps_code",
        $validator: "required",
        $message: "Tên đào tạo không được để trống!",
      },
    ],
  },
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
const listFormTraining = ref([
  { name: "Bắt buộc", code: 1 },
  { name: "Đăng ký", code: 2 },
  { name: "Cả hai", code: 3 },
]);

const listObjTraining = ref([
  { name: "Cấp lãnh đạo", code: 1 },
  { name: "Quản lý", code: 2 },
  { name: "Nhân viên", code: 3 },
]);

//Lấy số bản ghi
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_training_emps_count",
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
//Lấy dữ liệu training_emps
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
              proc: "hrm_training_emps_list",
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
      datalists.value[datalists.value.length - 1].training_emps_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].training_emps_id;
    options.value.IsNext = false;
  }
  options.value.PageNo = event.page;
  loadData(true);
};

const training_emps = ref({
  training_emps_name: "",
  emote_file: "",
  status: true,
  is_default: false,
  is_order: 1,
});

const selectedStamps = ref();
const submitted = ref(false);
const v$ = useVuelidate(rules, training_emps);
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
});

//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);
const openBasic = (str) => {
  submitted.value = false;
  training_emps.value = {
    training_emps_name: "",
    emote_file: "",
    status: true,
    is_default: false,
    is_order: sttStamp.value,
    organization_id: store.getters.user.organization_id,
    training_times: 0,
  };

  checkIsmain.value = false;
  isSaveTem.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};
const listDropdownUserGive = ref();
const list_users_training = ref([]);
const list_schedule = ref([]);

const listDropdownUserCheck = ref();
const listDropdownUser = ref();
const listUsers = ref([]);
const treedonvis = ref();
const initTudien = () => {
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_device_department",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data[1].length > 0) {
        let obj = renderTreeDV(
          data[1],
          "organization_id",
          "organization_name",
          "đơn vị"
        );

        treedonvis.value = obj.arrtreeChils;
      }
    })
    .catch((error) => {});
};
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
const loadUser = () => {
  listUsers.value = [];
  listDropdownUser.value = [];
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_users_list_dd",
            par: [
              { par: "search", va: null },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "role_id", va: null },
              {
                par: "organization_id",
                va: store.getters.user.organization_id,
              },
              { par: "department_id", va: null },
              { par: "position_id", va: null },
              { par: "pageno", va: 1 },
              { par: "pagesize", va: 10000 },
              { par: "isadmin", va: null },
              { par: "status", va: null },
              { par: "start_date", va: null },
              { par: "end_date", va: null },
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
        listDropdownUser.value.push({
          name: element.full_name,
          code: element.user_id,

          avatar: element.avatar,
          department_name: element.department_name,
          department_id: element.department_id,
          role_name: element.role_name,
          position_name: element.position_name,
          phone: element.phone,
          organization_id: element.organization_id,
        });
        listUsers.value.push({ data: element, active: false });
      });
      listUsers.value = data;
      listDropdownUserGive.value = listDropdownUser.value;
      listDropdownUserCheck.value = listDropdownUser.value.filter(
        (x) => x.code != store.getters.user.user_id
      );
    })
    .catch((error) => {
      console.log(error);

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
const closeDialog = () => {
  training_emps.value = {
    training_emps_name: "",
    emote_file: "",
    status: true,
    is_default: false,
    is_order: 1,
  };

  displayBasic.value = false;
  loadData(true);
};

const addRow_Item = (type) => {
  //relative
  if (type == 1) {
    let obj = {
      is_order: list_users_training.value.length + 1,
      data: null,
      full_name: null,
      role_id: null,
      position_name: null,
      position_id: null,
      role_name: null,
      note: null,
      department_name: null,
      profile_id: null,
    };
    list_users_training.value.push(obj);
  }
    if (type == 2) {
    let obj = {
      is_order: list_schedule.value.length + 1,
      class_schedule_name: null,
      limit: null,
      lecturers: null,
      phone_number: null,
      date_study: null,
      start_time: null,
      end_time: null,
      training_class_id: null,
    };
    list_schedule.value.push(obj);
  }
  
};

const delRow_Item = (item, type) => {
  if (type == 1) {
    list_users_training.value.splice(
      list_users_training.value.lastIndexOf(item),
      1
    );
  }
    if (type == 2) {
    list_schedule.value.splice(
      list_schedule.value.lastIndexOf(item),
      1
    );
  }
  
};
//Thêm bản ghi

const sttStamp = ref(1);
const saveData = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }

  if (training_emps.value.training_emps_name.length > 250) {
    swal.fire({
      title: "Error!",
      text: "Tên đào tạo không được vượt quá 250 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  let formData = new FormData();

  if (training_emps.value.countryside_fake)
    training_emps.value.countryside = training_emps.value.countryside_fake;
  formData.append("hrm_training_emps", JSON.stringify(training_emps.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (!isSaveTem.value) {
    axios
      .post(
        baseURL + "/api/hrm_training_emps/add_hrm_training_emps",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm đào tạo thành công!");
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
        baseURL + "/api/hrm_training_emps/update_hrm_training_emps",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa đào tạo thành công!");

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
  training_emps.value = dataTem;
  if (training_emps.value.countryside)
    training_emps.value.countryside_fake = training_emps.value.countryside;
  if (training_emps.value.is_default) {
    checkIsmain.value = false;
  } else {
    checkIsmain.value = true;
  }
  headerDialog.value = "Sửa đào tạo";
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
          .delete(baseURL + "/api/hrm_training_emps/delete_hrm_training_emps", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Tem != null ? [Tem.training_emps_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá đào tạo thành công!");
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
    id: "training_emps_id",
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
    .post(baseURL + "/api/hrm_ca_SQL/Filter_hrm_training_emps", data, config)
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
      IntID: value.training_emps_id,
      TextID: value.training_emps_id + "",
      IntTrangthai: 1,
      BitTrangthai: value.status,
    };
    axios
      .put(
        baseURL + "/api/hrm_training_emps/update_s_hrm_training_emps",
        data,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa trạng thái đào tạo thành công!");
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
      IntID: value.training_emps_id,
      TextID: value.training_emps_id + "",
      BitMain: value.is_default,
    };
    axios
      .put(
        baseURL + "/api/hrm_training_emps/Update_DefaultStamp",
        data1,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa trạng thái đào tạo thành công!");
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
      toast.error("Không được xóa đào tạo mặc định!");
      checkD = true;
      return;
    }
  });
  if (!checkD) {
    swal
      .fire({
        title: "Thông báo",
        text: "Bạn có muốn xoá đào tạo này không!",
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
            listId.push(item.training_emps_id);
          });
          axios
            .delete(
              baseURL + "/api/hrm_training_emps/delete_hrm_training_emps",
              {
                headers: { Authorization: `Bearer ${store.getters.token}` },
                data: listId != null ? listId : 1,
              }
            )
            .then((response) => {
              swal.close();
              if (response.data.err != "1") {
                swal.close();
                toast.success("Xoá đào tạo thành công!");
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

          organization_id: element.organization_id,
        });
      });
    })
    .catch((error) => {
      console.log(error);

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
const changeUserTrainding = (data, index) => {
  if (data && list_users_training.value[index]) {
    list_users_training.value[index].profile_id = data.profile_id;
    list_users_training.value[index].full_name = data.profile_user_name;
    list_users_training.value[index].role_id = data.work_position_id;
    list_users_training.value[index].work_position_name =
      data.work_position_name;
    list_users_training.value[index].position_name = data.position_name;
    list_users_training.value[index].position_id = data.position_id;
    list_users_training.value[index].department_id = data.department_id;
    list_users_training.value[index].department_name = data.department_name;
  }
  else{
        list_users_training.value[index].profile_id =null;
    list_users_training.value[index].full_name =null;
    list_users_training.value[index].role_id = null;
    list_users_training.value[index].work_position_name =
     null;
    list_users_training.value[index].position_name = null;
    list_users_training.value[index].position_id =null;
    list_users_training.value[index].department_id =null;
    list_users_training.value[index].department_name = null;
  }
};
onMounted(() => {
  if (!checkURL(window.location.pathname, store.getters.listModule)) {
    //router.back();
  }
  loadData(true);
  loadUser();
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
      dataKey="training_emps_id"
      responsiveLayout="scroll"
      v-model:selection="selectedStamps"
      :row-hover="true"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-book"></i> Danh sách đào tạo ({{
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
              @click="openBasic('Thêm mới thông tin đào tạo')"
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
          </template> </Toolbar
      ></template>

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

      <!-- <Column
        field="training_emps_name"
        header="Tên đào tạo"
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
        field="organization_id"
        header="Hệ thống"
        headerStyle="text-align:center;max-width:125px;height:50px"
        bodyStyle="text-align:center;max-width:125px;;max-height:60px"
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
        bodyStyle="text-align:center;max-width:150px"
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
      </Column> -->
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
  </div>

  <Dialog
    :header="headerDialog"
    v-model:visible="displayBasic"
    :style="{ width: '55vw' }"
    :closable="true"
    :modal="true"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 field p-0 text-lg font-bold">Thông tin thẻ</div>
        <div class="col-12 flex p-0">
          <div class="col-6 p-0">
            <div class="col-12 field flex p-0 text-left align-items-center">
              <div class="w-10rem">
                Mã số<span class="redsao pl-1"> (*)</span>
              </div>
              <div style="width: calc(100% - 10rem)">
                <InputText
                  v-model="training_emps.training_emps_code"
                  class="w-full"
                  :class="{
                    'p-invalid': v$.training_emps_code.$invalid && submitted,
                  }"
                />
              </div>
            </div>
            <div
              v-if="
                (v$.training_emps_code.$invalid && submitted) ||
                v$.training_emps_code.$pending.$response
              "
              class="col-12 field p-0 flex"
            >
              <div class="w-10rem"></div>
              <small style="width: calc(100% - 10rem)">
                <span style="color: red" class="w-full">{{
                  v$.training_emps_code.required.$message
                    .replace("Value", "Mã số")
                    .replace("is required", "không được để trống!")
                }}</span>
              </small>
            </div>
          </div>
          <div class="col-6 flex p-0 text-center align-items-center">
            <div class="col-12 field flex p-0 text-left align-items-center">
              <div class="w-10rem pl-3">
                Nhóm đào tạo<span class="redsao pl-1"> (*)</span>
              </div>
              <div style="width: calc(100% - 10rem)">
                <Dropdown
                  v-model="training_emps.training_groups_id"
                  :options="listSCard"
                  optionLabel="name"
                  optionValue="code"
                  placeholder="Chọn nhóm đào tạo"
                  class="w-full"
                />
              </div>
            </div>
          </div>
        </div>

        <div class="col-12 field flex p-0 align-items-center">
          <div class="w-10rem">
            Tên khóa đào tạo<span class="redsao pl-1"> (*)</span>
          </div>
          <div style="width: calc(100% - 10rem)">
            <div class="col-12 p-0">
              <div class="p-inputgroup">
                <Textarea
                  :autoResize="true"
                  rows="2"
                  cols="30"
                  v-model="training_emps.training_emps_name"
                  class="w-full"
                  :style="
                    training_emps.training_emps_name
                      ? 'background-color:white !important'
                      : ''
                  "
                />
              </div>
            </div>
          </div>
        </div>
        <!-- <div
              v-if="
                (v$.barcode_id.$invalid && submitted) ||
                v$.barcode_id.$pending.$response
              "
              class="col-12 field p-0 flex"
            >
              <div class="w-10rem"></div>
              <small style="width: calc(100% - 10rem)">
                <span style="color: red" class="w-full">{{
                  v$.barcode_id.required.$message
                    .replace("Value", "Mã Barcode")
                    .replace("is required", "không được để trống!")
                }}</span>
              </small>
            </div> -->
        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="col-6 field p-0 flex text-left align-items-center">
            <div class="w-10rem">Đối tượng đào tạo</div>
            <div style="width: calc(100% - 10rem)">
              <Dropdown
                v-model="training_emps.obj_training"
                :options="listObjTraining"
                optionLabel="name"
                optionValue="code"
                placeholder="----- Chọn đối tượng đào tạo -----"
                class="sel-placeholder w-full"
                panelClass="d-design-dropdown"
              />
            </div>
          </div>
          <div class="col-6 field p-0 flex text-left align-items-center">
            <div class="w-10rem pl-3">Hình thức đào tạo</div>
            <div style="width: calc(100% - 10rem)">
              <Dropdown
                v-model="training_emps.form_training"
                :options="listFormTraining"
                optionLabel="name"
                optionValue="code"
                placeholder="----- Chọn hình thức đào tạo -----"
                class="sel-placeholder w-full"
                panelClass="d-design-dropdown"
              />
            </div>
          </div>
        </div>
        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="col-6 field p-0 flex text-left align-items-center">
            <div class="w-10rem">Ngày bắt đầu</div>
            <div style="width: calc(100% - 10rem)">
              <Calendar
                class="w-full"
                id="basic_purchase_date"
                v-model="training_emps.start_date"
                autocomplete="on"
                :showIcon="true"
              />
            </div>
          </div>
          <div class="col-6 field p-0 flex text-left align-items-center">
            <div class="w-10rem pl-3">Ngày kết thúc</div>
            <div style="width: calc(100% - 10rem)">
              <Calendar
                class="w-full"
                id="basic_purchase_date"
                v-model="training_emps.end_date"
                autocomplete="on"
                :showIcon="true"
              />
            </div>
          </div>
        </div>
        <!-- <div
              v-if="!training_emps.status && submitted"
              class="col-12 field p-0 flex"
            >
              <div class="w-10rem"></div>
              <small style="width: calc(100% - 10rem)">
                <span style="color: red" class="w-full"
                  >Trạng thái không được để trống</span
                >
              </small>
            </div> -->
        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="col-6 field p-0 flex text-left align-items-center">
            <div class="w-10rem">Thời lượng</div>
            <div style="width: calc(100% - 10rem)">
              <Calendar
                inputId="time12"
                hourFormat="24"
                class="ip36 w-full"
                autocomplete="on"
                icon="pi pi-clock"
                :showIcon="true"
                :timeOnly="true"
                v-model="training_emps.training_times"
                suffix=" Tháng"
              />
            </div>
          </div>
          <div class="col-6 field p-0 flex text-left align-items-center">
            <div class="w-10rem pl-3">Hạn đăng ký</div>
            <div style="width: calc(100% - 10rem)">
              <Calendar
                class="w-full"
                id="basic_purchase_date"
                v-model="training_emps.registration_deadline"
                autocomplete="on"
                :showIcon="true"
              />
            </div>
          </div>
        </div>

        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="w-10rem">Đơn vị thực hiện</div>
          <div style="width: calc(100% - 10rem)">
            <TreeSelect
              class="w-full sel-placeholder"
              v-model="training_emps.corporation"
              :options="treedonvis"
              :showClear="true"
              :max-height="200"
              optionLabel="label"
              optionValue="data"
              panelClass="d-design-dropdown"
              placeholder="---------------  Chọn đơn vị thực hiện --------------- "
            >
            </TreeSelect>
            <!-- <Dropdown
              v-model="training_emps.producer"
              :options="listPCard"
              :filter="true"
              optionLabel="name"
              optionValue="code"
              placeholder="---------------  Chọn đơn vị thực hiện --------------- "
              class="sel-placeholder w-full"
              @change="onChangeCountry()"
            >
              <template #option="slotProps">
                <div class="country-item flex">
                  <div class="pt-1">{{ slotProps.option.name }}</div>
                </div>
              </template>
            </Dropdown> -->
          </div>
        </div>
        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="col-6 field p-0 flex text-left align-items-center">
            <div class="w-10rem">Người phụ trách</div>
            <div style="width: calc(100% - 10rem)">
              <MultiSelect
                v-model="training_emps.user_verify"
                :options="listDropdownUserGive"
                optionLabel="name"
                optionValue="code"
                placeholder="-------- Chọn người nhận --------"
                panelClass="d-design-dropdown"
                class="w-full p-0 d-tree-input"
                :class="{
                  'p-invalid': !training_emps.user_verify && submitted,
                }"
                display="chip"
              >
                <template #option="slotProps">
                  <div class="country-item flex align-items-center">
                    <div class="grid w-full p-0">
                      <div
                        class="
                          field
                          p-0
                          py-1
                          col-12
                          flex
                          m-0
                          cursor-pointer
                          align-items-center
                        "
                      >
                        <div class="col-1 mx-2 p-0 align-items-center">
                          <Avatar
                            v-bind:label="
                              slotProps.option.avatar
                                ? ''
                                : slotProps.option.name.substring(
                                    slotProps.option.name.lastIndexOf(' ') + 1,
                                    slotProps.option.name.lastIndexOf(' ') + 2
                                  )
                            "
                            :image="basedomainURL + slotProps.option.avatar"
                            size="small"
                            :style="
                              slotProps.option.avatar
                                ? 'background-color: #2196f3'
                                : 'background:' +
                                  bgColor[slotProps.option.name.length % 7]
                            "
                            shape="circle"
                            @error="
                              $event.target.src =
                                basedomainURL + '/Portals/Image/nouser1.png'
                            "
                          />
                        </div>
                        <div class="col-11 p-0 ml-3 align-items-center">
                          <div class="pt-2">
                            <div class="font-bold">
                              {{ slotProps.option.name }}
                            </div>
                            <div
                              class="flex w-full text-sm font-italic text-500"
                            >
                              <div>{{ slotProps.option.position_name }}</div>
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </template>
              </MultiSelect>
            </div>
          </div>
          <div class="col-6 field p-0 flex text-left align-items-center">
            <div class="w-10rem pl-3">Người theo dõi</div>
            <div style="width: calc(100% - 10rem)">
              <MultiSelect
                v-model="training_emps.user_follows"
                :options="listDropdownUserGive"
                optionLabel="name"
                optionValue="code"
                placeholder="-------- Chọn người theo dõi --------"
                panelClass="d-design-dropdown"
                class="w-full p-0 d-tree-input"
                :class="{
                  'p-invalid': !training_emps.user_follows && submitted,
                }"
                display="chip"
              >
                <template #option="slotProps">
                  <div class="country-item flex align-items-center">
                    <div class="grid w-full p-0">
                      <div
                        class="
                          field
                          p-0
                          py-1
                          col-12
                          flex
                          m-0
                          cursor-pointer
                          align-items-center
                        "
                      >
                        <div class="col-1 mx-2 p-0 align-items-center">
                          <Avatar
                            v-bind:label="
                              slotProps.option.avatar
                                ? ''
                                : slotProps.option.name.substring(
                                    slotProps.option.name.lastIndexOf(' ') + 1,
                                    slotProps.option.name.lastIndexOf(' ') + 2
                                  )
                            "
                            :image="basedomainURL + slotProps.option.avatar"
                            size="small"
                            :style="
                              slotProps.option.avatar
                                ? 'background-color: #2196f3'
                                : 'background:' +
                                  bgColor[slotProps.option.name.length % 7]
                            "
                            shape="circle"
                            @error="
                              $event.target.src =
                                basedomainURL + '/Portals/Image/nouser1.png'
                            "
                          />
                        </div>
                        <div class="col-11 p-0 ml-3 align-items-center">
                          <div class="pt-2">
                            <div class="font-bold">
                              {{ slotProps.option.name }}
                            </div>
                            <div
                              class="flex w-full text-sm font-italic text-500"
                            >
                              <div>{{ slotProps.option.position_name }}</div>
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </template>
              </MultiSelect>
            </div>
          </div>
        </div>
        <div class="col-12 field flex p-0 align-items-center">
          <div class="w-10rem">Địa điểm đào tạo</div>
          <div style="width: calc(100% - 10rem)">
            <div class="col-12 p-0">
              <div class="p-inputgroup">
                <Textarea
                  :autoResize="true"
                  rows="2"
                  cols="30"
                  v-model="training_emps.training_emps_name"
                  class="w-full"
                  :style="
                    training_emps.training_emps_name
                      ? 'background-color:white !important'
                      : ''
                  "
                />
              </div>
            </div>
          </div>
        </div>
        <div class="col-12 p-0 field">
          <Accordion class="accordion-custom w-full">
            <!-- 1. Thông tin chung -->
            <AccordionTab>
              <template #header>
                <span class="font-bold">Thông tin học viên</span>
              </template>
              <div style="text-align: end; padding-bottom: 0.5rem">
                <Button
                  label="Thêm"
                  icon="pi pi-plus"
                  style="padding: 0.3rem 0.6rem !important"
                  class="p-button-outlined p-button-secondary"
                  @click="addRow_Item(1)"
                />
              </div>

              <div style="overflow-x: scroll">
                <table
                v-if="list_users_training.length>0"
                  class="table table-condensed table-hover tbpad table-child"
                  style="table-layout: fixed"
                >
                  <thead class=" pb-3">
                    <tr class="w-full">
                      <th
                        class="text-center sticky"
                        style="width: 5%; left: 0px !important"
                      ></th>
                      <th class="text-center" style="width: 120px">STT</th>
                      <th class="text-center" style="width: 25%">Họ và tên</th>
                      <th class="text-center" style="width: 20%">Phòng ban</th>
                      <th class="text-center" style="width: 15%">Vị trí</th>
                      <th class="text-center" style="width: 15%">Chức vụ</th>
                      <th class="text-center" style="width: 30%">Ghi chú</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr
                      id="add_thanhpham"
                      v-for="(item, index) in list_users_training"
                      :key="index"
                    >
                      <td
                        class="row-content-pdx sticky"
                        align="center"
                        style="color: black; width: 100px; left: 0px !important"
                      >
                        <!-- <Button
                          icon="pi pi-times"
                          class="mr-2 p-button-danger"
                          @click="delRow_Item(item)"
                        /> -->
                        <i
                          class="pi pi-times text-xl cursor-pointer"
                          style="color: red"
                          v-tooltip.top="'Xóa'"
                          @click="delRow_Item(item, 1)"
                        ></i>
                      </td>

                      <td class="row-content-pdx" align="center">
                        {{ item.is_order }}
                      </td>
                      <td class="row-content-pdx mx-2" align="center">
                        <div class="mx-2">
                          <Dropdown
                            :options="listDataUsers"
                            :filter="true"
                            :showClear="true"
                            :editable="false"
                            optionLabel="profile_user_name"
                            optionValue="code"
                            placeholder="Chọn học viên"
                            v-model="item.data"
                            class="w-full"
                            style="height: auto; min-height: 36px"
                            @change="changeUserTrainding(item.data, index)"
                          >
                            <template #value="slotProps">
                              <div v-if="slotProps.value">
                                <div
                                  class="
                                    flex
                                    w-full
                                    align-items-center
                                    pr-2
                                    p-0
                                  "
                                >
                                  <Avatar
                                    v-bind:label="
                                      slotProps.value.avatar
                                        ? ''
                                        : slotProps.value.profile_user_name.substring(
                                            slotProps.value.profile_user_name.lastIndexOf(
                                              ' '
                                            ) + 1,
                                            slotProps.value.profile_user_name.lastIndexOf(
                                              ' '
                                            ) + 2
                                          )
                                    "
                                    :image="
                                      basedomainURL + slotProps.value.avatar
                                    "
                                    size="small"
                                    :style="
                                      slotProps.value.avatar
                                        ? 'background-color: #2196f3'
                                        : 'background:' +
                                          bgColor[
                                            slotProps.value.profile_user_name
                                              .length % 7
                                          ]
                                    "
                                    shape="circle"
                                    @error="
                                      $event.target.src =
                                        basedomainURL +
                                        '/Portals/Image/nouser1.png'
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
                                  <Avatar
                                    v-bind:label="
                                      slotProps.option.avatar
                                        ? ''
                                        : slotProps.option.profile_user_name.substring(
                                            0,
                                            1
                                          )
                                    "
                                    v-bind:image="
                                      slotProps.option.avatar
                                        ? basedomainURL +
                                          slotProps.option.avatar
                                        : basedomainURL +
                                          '/Portals/Image/noimg.jpg'
                                    "
                                    style="
                                      background-color: #2196f3;
                                      color: #ffffff;
                                      width: 3rem;
                                      height: 3rem;
                                      font-size: 1.4rem !important;
                                    "
                                    :style="{
                                      background:
                                        bgColor[slotProps.option.is_order % 7],
                                    }"
                                    class="text-avatar"
                                    size="xlarge"
                                    shape="circle"
                                  />
                                </div>
                                <div class="ml-3 format-center">
                                  <div class="mb-1">
                                    {{ slotProps.option.profile_user_name }}
                                  </div>
                                </div>
                              </div>
                              <span v-else> Chưa có dữ liệu </span>
                            </template>
                          </Dropdown>
                        </div>
                      </td>
                      <td class="row-content-pdx" align="center">
                        <InputText
                          spellcheck="false"
                          class="w-full"
                          style="width: 170px"
                          v-model="item.department_name"
                          disabled
                        />
                      </td>
                      <td class="row-content-pdx" align="center">
                        <InputText
                          spellcheck="false"
                          class="w-full"
                          style="width: 170px"
                          v-model="item.work_position_name"
                          disabled
                        />
                      </td>
                      <td class="row-content-pdx" align="center">
                        <InputText
                          spellcheck="false"
                          class="w-full"
                          style="width: 170px"
                          v-model="item.position_name"
                          disabled
                        />
                      </td>

                      <td class="row-content-pdx" align="center">
                        <InputText
                          spellcheck="false"
                          class="ip33"
                          style="width: 170px"
                          v-model="item.note"
                        />
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </AccordionTab>
            <AccordionTab>
              <template #header>
                <span>Lịch học</span>
              </template>
 
              <div style="text-align: end; padding-bottom: 0.5rem">
                <Button
                  label="Thêm"
                  icon="pi pi-plus"
                  style="padding: 0.3rem 0.6rem !important"
                  class="p-button-outlined p-button-secondary"
                  @click="addRow_Item(2)"
                />
              </div>

              <div style="overflow-x: scroll">
                <table
                v-if="list_schedule.length>0"
                  class="table table-condensed table-hover tbpad table-child"
                  style="table-layout: fixed"
                >
                  <thead class=" pb-3">
                    <tr class="w-full">
                      <th
                        class="text-center sticky"
                        style="width: 5%; left: 0px !important"
                      ></th>
                      <th class="text-center" style="width: 120px">STT</th>
                      <th class="text-center" style="width: 25%">Nội dung đào tạo</th>
                      <th class="text-center" style="width: 20%">Phạm vi</th>
                      <th class="text-center" style="width: 15%">Vị trí</th>
                      <th class="text-center" style="width: 15%">Chức vụ</th>
                      <th class="text-center" style="width: 30%">Ghi chú</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr
                      id="add_thanhpham"
                      v-for="(item, index) in list_schedule"
                      :key="index"
                    >
                      <td
                        class="row-content-pdx sticky"
                        align="center"
                        style="color: black; width: 100px; left: 0px !important"
                      >
                        <!-- <Button
                          icon="pi pi-times"
                          class="mr-2 p-button-danger"
                          @click="delRow_Item(item)"
                        /> -->
                        <i
                          class="pi pi-times text-xl cursor-pointer"
                          style="color: red"
                          v-tooltip.top="'Xóa'"
                          @click="delRow_Item(item, 1)"
                        ></i>
                      </td>

                      <td class="row-content-pdx" align="center">
                        {{ item.is_order }}
                      </td>
                      <td class="row-content-pdx mx-2" align="center">
                        <div class="mx-2">
                           <InputText
                          spellcheck="false"
                          class="w-full"
                          style="width: 170px"
                          v-model="item.department_name"
                          disabled
                        />
                        </div>
                      </td>
                      <td class="row-content-pdx" align="center">
                        <InputText
                          spellcheck="false"
                          class="w-full"
                          style="width: 170px"
                          v-model="item.department_name"
                          disabled
                        />
                      </td>
                      <td class="row-content-pdx" align="center">
                        <InputText
                          spellcheck="false"
                          class="w-full"
                          style="width: 170px"
                          v-model="item.work_position_name"
                          disabled
                        />
                      </td>
                      <td class="row-content-pdx" align="center">
                        <InputText
                          spellcheck="false"
                          class="w-full"
                          style="width: 170px"
                          v-model="item.position_name"
                          disabled
                        />
                      </td>

                      <td class="row-content-pdx" align="center">
                        <InputText
                          spellcheck="false"
                          class="ip33"
                          style="width: 170px"
                          v-model="item.note"
                        />
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </AccordionTab>
            <AccordionTab>
              <template #header>
                <!-- <i class="pi pi-chart-line mr-2"></i> -->
                <span>Đính kèm khác (file số hóa liên quan)</span>
              </template>
              <!-- <div class="field col-12 md:col-12 flex">
                <label class="text-left col-2">Tải file lên </label>
                <div class="col-10 p-0">
                  <FileUpload
                    chooseLabel="Chọn File"
                    :showUploadButton="false"
                    :showCancelButton="false"
                    :multiple="false"
                    accept=""
                    :maxFileSize="500000000"
                    @select="onUploadFile"
                    @remove="removeFile"
                  />
                </div>
              </div> -->
            </AccordionTab>
          </Accordion>
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
    