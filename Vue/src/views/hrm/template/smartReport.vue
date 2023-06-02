<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL, decr } from "../../../util/function.js";
import DropdownUser from "../component/DropdownProfile.vue";
import DropdownUsers from "../component/DropdownUsers.vue";
import DocComponent from "./components/DocComponent.vue";
import { useRoute } from "vue-router";
//Khai báo
const emitter = inject("emitter");
const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const isDynamicSQL = ref(false);

const router = inject("router");
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  report_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  report_group: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.CONTAINS }],
  },
});
const visibleSidebarDoc = ref(false);
const rules = {
  report_name: {
    required,
    $errors: [
      {
        $property: "report_name",
        $validator: "required",
        $message: "Tên báo cáo không được để trống!",
      },
    ],
  },
};
const getProfileUser = (user, obj) => {
  if (obj) {
    smart_report.value[user] = obj.profile_id;
  } else {
    smart_report.value[user] = null;
  }
};

const execSQL = async (id) => {
  const axResponse = await axios.post(
    baseURL + "/api/HRM_SQL/getData",
    {
      str: encr(
        JSON.stringify({
          proc: "smart_report_get",
          par: [{ par: "report_id", va: id }],
        }),
        SecretKey,
        cryoptojs
      ).toString(),
    },
    config
  );

  return axResponse;
};
const configBaocao = async (row) => {
  axios;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  
  const axResponse = await execSQL(row.report_id);
  
  if (axResponse.status == 200) {
    if (axResponse.data.error) {
      toast.error("Không mở được bản ghi");
    } else {
      smart_report.value = JSON.parse(axResponse.data.data)[0][0];
 
      if (smart_report.value.proc_name)
        smart_report.value.proc_name1 =
          smart_report.value.proc_name.split(" ")[0];
        
      visibleSidebarDoc.value = true;
    }
  }
  swal.close();
};
//Lấy số bản ghi

const loadCount = () => {
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "smart_report_count",
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
//Lấy dữ liệu smart_report
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
            proc: "smart_report_list",
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

      expandedRowGroups.value = [];
      data.forEach((element, i) => {
        element.STT = options.value.PageNo * options.value.PageSize + i + 1;
        expandedRowGroups.value.push(element.report_group);

        if (element.report_group) {
          if (
            !liReportGroup.value.find(
              (x) => x.name == element.report_group.trim()
            )
          ) {
            liReportGroup.value.push({
              name: element.report_group,
            });
          }
        }
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
      }
    });
};
const onRowGroupExpand = (event) => {
  console.log(event, expandedRowGroups.value);
};

const initBaocao = async (id) => {
  let strSQL = {
    query: false,
    proc: "report_get_key",
    par: [
      {
        par: "report_key",
        va: id,
      },
    ],
  };
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  const axResponse = await axios.post(
    baseURL + "api/Proc/PostProc",
    encr(JSON.stringify(strSQL), SecretKey, cryoptojs).toString(),
    config
  );

  if (axResponse.status == 200) {
    if (axResponse.data.error) {
      toast.error("Không mở được báo cáo");
    } else {
      smart_report.value = JSON.parse(axResponse.data.data)[0][0];
    }
  }
  swal.close();
};
const onRowClickTable = async (data) => {
  const axResponse = await execSQL(data.data.report_id);
  if (axResponse.status == 200) {
    if (axResponse.data.error) {
      toast.error("Không mở được báo cáo");
    } else {
      var seg = JSON.parse(axResponse.data.data)[0][0];
      let srcMs = removeVietnameseTones(seg.report_name);
      store.commit("setnews", data);
      if (router)
        router.push({
          path:
            "/hrm/template/smart_report/" +
            srcMs.replace(/','|'.'/g, "").replace(/\s+/g, "-") +
            "-orient-" +
            seg.report_key,
        });
    }
  }

  // configBaocao(data.data);
};
function removeVietnameseTones(str) {
  str = str.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g, "a");
  str = str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, "e");
  str = str.replace(/ì|í|ị|ỉ|ĩ/g, "i");
  str = str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, "o");
  str = str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, "u");
  str = str.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, "y");
  str = str.replace(/đ/g, "d");
  str = str.replace(/À|Á|Ạ|Ả|Ã|Â|Ầ|Ấ|Ậ|Ẩ|Ẫ|Ă|Ằ|Ắ|Ặ|Ẳ|Ẵ/g, "A");
  str = str.replace(/È|É|Ẹ|Ẻ|Ẽ|Ê|Ề|Ế|Ệ|Ể|Ễ/g, "E");
  str = str.replace(/Ì|Í|Ị|Ỉ|Ĩ/g, "I");
  str = str.replace(/Ò|Ó|Ọ|Ỏ|Õ|Ô|Ồ|Ố|Ộ|Ổ|Ỗ|Ơ|Ờ|Ớ|Ợ|Ở|Ỡ/g, "O");
  str = str.replace(/Ù|Ú|Ụ|Ủ|Ũ|Ư|Ừ|Ứ|Ự|Ử|Ữ/g, "U");
  str = str.replace(/Ỳ|Ý|Ỵ|Ỷ|Ỹ/g, "Y");
  str = str.replace(/Đ/g, "D");
  // Some system encode vietnamese combining accent as individual utf-8 characters
  // Một vài bộ encode coi các dấu mũ, dấu chữ như một kí tự riêng biệt nên thêm hai dòng này
  str = str.replace(/\u0300|\u0301|\u0303|\u0309|\u0323/g, ""); // ̀ ́ ̃ ̉ ̣  huyền, sắc, ngã, hỏi, nặng
  str = str.replace(/\u02C6|\u0306|\u031B/g, ""); // ˆ ̆ ̛  Â, Ê, Ă, Ơ, Ư
  // Remove extra spaces
  // Bỏ các khoảng trắng liền nhau
  str = str.replace(/ + /g, " ");
  str = str.trim();
  // Remove punctuations
  // Bỏ dấu câu, kí tự đặc biệt
  str = str.replace(
    /!|@|%|\^|\*|\(|\)|\+|\=|\<|\>|\?|\/|,|\.|\:|\;|\'|\"|\&|\#|\[|\]|~|\$|_|`|-|{|}|\||\\/g,
    " "
  );
  return str;
}
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

    options.value.id = datalists.value[datalists.value.length - 1].report_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].report_id;
    options.value.IsNext = false;
  }
  options.value.PageNo = event.page;
  loadData(true);
};
const liReportGroup = ref([
  {
    name: "Bảng lương",
    code: 1,
  },
  {
    name: "Hợp đồng",
    code: 2,
  },
  {
    name: "Quyết định",
    code: 3,
  },
  {
    name: "Hồ sơ nhân sự",
    code: 4,
  },
]);
const smart_report = ref({
  report_name: "",
  emote_file: "",
  status: true,
  is_order: 1,
  user_access_fake: [],
  user_deny_fake: [],
});
const collapsed1 = ref(false);
const collapsed2 = ref(true);
const selectedStamps = ref();
const submitted = ref(false);
const v$ = useVuelidate(rules, smart_report);
const isSaveTem = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = baseURL;
const checkDelList = ref(false);

const options = ref({
  IsNext: true,
  sort: "report_group",
  SearchText: "",
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
  totalRecordView: 0,
  totalRecordProc: 0,
  report_group_filter: [],
});

//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);
const openBasic = (str) => {
  submitted.value = false;
  smart_report.value = {
    report_name: "",
    emote_file: "",
    status: true,
    is_order: sttStamp.value,
    organization_id: store.getters.user.organization_id,
    is_system: store.getters.user.is_super ? true : false,
    user_access_fake: [],
    user_deny_fake: [],
    report_template: null,
  };
  checkDisabled.value = false;
  checkUploadFile.value = false;
  checkIsmain.value = false;
  isSaveTem.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};
const openBasicWRP = (wrt) => {
  submitted.value = false;
  smart_report.value = {
    report_name: "",
    emote_file: "",
    status: true,
    is_order: sttStamp.value,
    organization_id: store.getters.user.organization_id,
    is_system: store.getters.user.is_super ? true : false,
    user_access_fake: [],
    user_deny_fake: [],
    report_template: null,

    report_group: wrt,
  };
  checkDisabled.value = false;
  checkUploadFile.value = false;
  checkIsmain.value = false;
  isSaveTem.value = false;
  headerDialog.value = "Thêm báo cáo";
  displayBasic.value = true;
};
const pars = ref([]);
const closeDialog = () => {
  smart_report.value = {
    report_name: "",
    emote_file: "",
    status: true,
    is_order: 1,
  };

  displayBasic.value = false;
  loadData(true);
};

//Thêm bản ghi
const checkDisabled = ref(false);
const sttStamp = ref(1);
const saveData = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  if (smart_report.value.proc_get==null ||
  smart_report.value.report_group==null ||
  smart_report.value. proc_name1==null ||
  smart_report.value. profile_id==null  
  ) {
    return;
  }

  if (smart_report.value.report_name.length > 250) {
    swal.fire({
      title: "Error!",
      text: "Tên báo cáo không được vượt quá 250 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  let formData = new FormData();
  if (smart_report.value.profile_id) {
    smart_report.value.proc_name =
      smart_report.value.proc_name1 +
      " '" +
      smart_report.value.profile_id +
      "'";
  }

  if (smart_report.value.user_access_fake.length > 0)
    smart_report.value.user_access =
      smart_report.value.user_access_fake.toString();
  else smart_report.value.user_access = null;
  if (smart_report.value.user_deny_fake.length > 0)
    smart_report.value.user_deny = smart_report.value.user_deny_fake.toString();
  else smart_report.value.user_deny = null;

  if (smart_report.value.report_group != null) {
    var ser = liReportGroup.value.find(
      (x) => smart_report.value.report_group.indexOf(x.name) != -1
    );
    if (ser != null) {
      smart_report.value.report_type = ser.code;
    }
    if (smart_report.value.report_type == 3) {
      if (smart_report.value.profile_id) {
        smart_report.value.proc_name =
          smart_report.value.proc_name1 +
          " '" +
          smart_report.value.decision_id +
          "','" +
          smart_report.value.profile_id +
          "' ";
      }
    }
  }
 
  formData.append("smart_report", JSON.stringify(smart_report.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });

  if (!isSaveTem.value) {
    axios
      .post(baseURL + "/api/smart_report/add_smart_report", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm báo cáo thành công!");
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
      .put(baseURL + "/api/smart_report/update_smart_report", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa báo cáo thành công!");

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

const copyTem = (dataTem) => {
  axios
    .post(
      baseURL + "/api/HRM_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "smart_report_get",
            par: [{ par: "report_id", va: dataTem.report_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0][0];
      if (isFirst.value) isFirst.value = false;
      collapsed1.value = false;
      collapsed2.value = false;
      smart_report.value = data;
      submitted.value = false;

      if (smart_report.value.report_template != null) {
        checkUploadFile.value = true;
        checkDisabled.value = false;
      }

      if (data.user_access)
        smart_report.value.user_access_fake = data.user_access.split(",");
      else smart_report.value.user_access_fake = [];
      if (data.user_deny)
        smart_report.value.user_deny_fake = data.user_deny.split(",");
      else smart_report.value.user_deny_fake = [];
      smart_report.value.proc_get = data.proc_get;
      smart_report.value.proc_name = data.proc_name;
      smart_report.value.report_name = null;
      headerDialog.value = "Thêm báo cáo";
      isSaveTem.value = false;

      displayBasic.value = true;
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
const editTem = (dataTem) => {
  axios
    .post(
      baseURL + "/api/HRM_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "smart_report_get",
            par: [{ par: "report_id", va: dataTem.report_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0][0];
      if (isFirst.value) isFirst.value = false;
      collapsed1.value = false;
      collapsed2.value = false;
      smart_report.value = data;
      submitted.value = false;

      if (smart_report.value.report_template != null) {
        checkUploadFile.value = true;
        checkDisabled.value = false;
      }
      if (data.proc_name)
        smart_report.value.proc_name1 = data.proc_name.split(" ")[0];
      if (data.user_access)
        smart_report.value.user_access_fake = data.user_access.split(",");
      else smart_report.value.user_access_fake = [];
      if (data.user_deny)
        smart_report.value.user_deny_fake = data.user_deny.split(",");
      else smart_report.value.user_deny_fake = [];
      smart_report.value.proc_get = data.proc_get;
      smart_report.value.proc_name = data.proc_name;
      headerDialog.value = "Sửa báo cáo";
      isSaveTem.value = true;

      displayBasic.value = true;
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
const callbackFun = (obj) => {
  Object.keys(obj).forEach((k) => {
    smart_report.value[k] = obj[k];
  });
 
  let formData = new FormData();
  formData.append("smart_report", JSON.stringify(smart_report.value));
   
  axios
    .put(baseURL + "/api/smart_report/update_smart_report", formData, config)
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
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
          .delete(baseURL + "/api/smart_report/delete_smart_report", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Tem != null ? [Tem.report_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá báo cáo thành công!");
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
    id: "report_id DESC",
    sqlS: null,
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
    .post(baseURL + "/api/HRM_SQL/Filter_smart_report", data, config)
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
      IntID: value.report_id,
      TextID: value.report_id + "",
      IntTrangthai: 1,
      BitTrangthai: value.status,
    };
    axios
      .put(baseURL + "/api/smart_report/update_s_smart_report", data, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa trạng thái báo cáo thành công!");
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
      IntID: value.report_id,
      TextID: value.report_id + "",
      BitMain: value.is_default,
    };
    axios
      .put(baseURL + "/api/smart_report/Update_DefaultStamp", data1, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa trạng thái báo cáo thành công!");
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

const expandedRowGroups = ref([1, 2]);
//Filter

const op = ref();
const toggle = (event) => {
  op.value.toggle(event);
};

const filesList = ref([]);

const onUpFile = (file) => {
  let formData = new FormData();
  formData.append("fileupload", file);
  axios({
    method: "post",
    url: baseURL + `/api/SRC/UpFile`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        smart_report.value.report_template = response.data.htmls[0];
        checkUploadFile.value = true;
        checkDisabled.value = false;
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
};
const checkUploadFile = ref(false);
const onUploadFile = (event) => {
  checkDisabled.value = true;
  checkUploadFile.value = false;

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
            onUpFile(fi);
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
const removeFile = (event) => {
  filesList.value = filesList.value.filter((a) => a != event.file);
};
const listProcDropdown = ref([]);
const listDecisions = ref([]);
const initTuDien = () => {
  listDecisions.value=[];
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "smart_proc_listdecision",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbs = JSON.parse(data);
    

          if (tbs[0] != null && tbs[0].length > 0) {
             
            tbs[0].forEach((item) => {
              listDecisions.value.push({
                name: item.type_decision_name,
                code: item.decision_id
                 
              });
            });
          }
        }
      }
    });
  listProcDropdown.value = [];
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "smart_proc_list",
            par: [
              { par: "pageno", va: options.value.PageNo },
              { par: "pagesize", va:  1000},
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
        listProcDropdown.value.push({
          name: element.proc_title,
          code: element.proc_name,
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

emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "submitDropdownUser":
      if (obj.data) {
        smart_report.value.profile_id = obj.data.profile_id;
      } else {
        smart_report.value.profile_id = null;
      }

      break;

    case "submitDropdownUsers":
      if (obj.data) {
        if (obj.data.type == 1) {
          smart_report.value.user_access_fake = [];

          obj.data.data.forEach((element) => {
            smart_report.value.user_access_fake.push(element.user_id);
          });
        } else {
          smart_report.value.user_deny_fake = [];

          obj.data.data.forEach((element) => {
            smart_report.value.user_deny_fake.push(element.user_id);
          });
        }
      } else {
        if (obj.data.type == 1) {
          smart_report.value.user_access_fake = null;
        } else {
          smart_report.value.user_deny_fake = null;
        }
      }

      break;
    case "delDropdownUsers":
      if (obj.data) {
        if (obj.data.type == 1) {
          smart_report.value.user_access_fake =
            smart_report.value.user_access_fake.filter(
              (x) => x != obj.data.data.user_id
            );
        } else {
          smart_report.value.user_deny_fake =
            smart_report.value.user_deny_fake.filter(
              (x) => x != obj.data.data.user_id
            );
        }
      }
      break;

    default:
      break;
  }
});
const onFilterReportGr = () => {
  if (options.value.report_group_filter.length > 0) {
    filterSQL.value = [];
    let filterS1 = {
      filterconstraints: [],
      filteroperator: "or",
      key: "report_group",
    };
    options.value.report_group_filter.forEach((element) => {
      var addr = { value: element, matchMode: "contains" };
      filterS1.filterconstraints.push(addr);
    });

    filterSQL.value.push(filterS1);
    loadDataSQL();
  } else {
    loadData();
  }
};
const  onChangRPGR=(item)=>{
  if(liReportGroup.value.find(x=>x.name==item) )
 {
    smart_report.value.report_type=liReportGroup.value.find(x=>x.name==item).code;
  }
}
onMounted(() => {
  loadData(true);
  initTuDien();
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
      dataKey="report_id"
      responsiveLayout="scroll"
      :row-hover="true"
      rowGroupMode="subheader"
      groupRowsBy="report_group"
      expandableRowGroups
      v-model:expandedRowGroups="expandedRowGroups"
      sortMode="single"
      sortField="report_group"
      :sortOrder="1"
      selectionMode="single"
      v-model:selection="selectedStamps"
      @rowgroup-expand="onRowGroupExpand($event)"
      @row-click="onRowClickTable($event)"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-bookmark-fill"></i> Danh sách báo cáo ({{
            options.totalRecords
          }})
        </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <div class="align-items-center flex">
              <span class="p-input-icon-left">
                <i class="pi pi-search" />
                <InputText
                  v-model="options.SearchText"
                  @keyup="searchStamp"
                  type="text"
                  spellcheck="false"
                  placeholder="Tìm kiếm"
                />
              </span>

              <div class="p-inputgroup flex-1 ml-2">
                <span class="p-inputgroup-addon">
                  <i class="pi pi-filter"></i>
                </span>
                <MultiSelect
                  v-model="options.report_group_filter"
                  :options="liReportGroup"
                  optionLabel="name"
                  optionValue="name"
                  spellcheck="false"
                  style="min-width: 13rem"
                  placeholder="Chọn nhóm báo báo"
                  @change="onFilterReportGr"
                />
              </div>
            </div>
          </template>

          <template #end>
            <Button
              @click="openBasic('Thêm báo cáo')"
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
      <template #groupheader="slotProps">
        <div class="flex align-items-center pl-3">
          <div class="font-bold text-blue-500">
            {{ slotProps.data.report_group }}
          </div>
          <Button
            style="padding: 5px"
            @click="openBasicWRP(slotProps.data.report_group)"
            icon="pi pi-plus-circle"
            class="ml-1 p-button-text p-button-rounded p-button-secondary"
          />
        </div>
      </template>
      <!-- <Column
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:50px;height:50px"
        bodyStyle="text-align:center;max-width:50px"
        selectionMode="multiple"
        v-if="store.getters.user.is_super == true"
      >
      </Column> -->

      <Column
        field="STT"
        header="STT"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:50px;height:50px"
        bodyStyle="text-align:center;max-width:50px"
      ></Column>

      <Column
        field="report_name"
        header="Tên báo cáo"
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
        header="Template"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:120px;height:50px"
        bodyStyle="text-align:center;max-width:120px"
        ><template #body="data">
          <div class="w-full flex" v-if="data.data.is_temp">
            <i
              class="pi pi-check-square text-blue-500 w-full format-center"
              style="font-size: 1.5rem"
            ></i></div></template
      ></Column>
      <Column
        header="Sử dụng"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:120px;height:50px"
        bodyStyle="text-align:center;max-width:120px"
        ><template #body="data">
          <div class="w-full flex" v-if="data.data.is_temp">
            <i
              class="pi pi-lock text-red-500 w-full format-center"
              style="font-size: 1.5rem"
            ></i>
          </div> </template
      ></Column>
      <Column
        header="Public"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:120px;height:50px"
        bodyStyle="text-align:center;max-width:120px"
        ><template #body="data">
          <div class="w-full flex" v-if="data.data.is_public">
            <i
              class="pi pi-check text-green-500 font-bold w-full format-center"
              style="font-size: 1.5rem"
            ></i></div></template
      ></Column>
      <Column
        field="status"
        header="Trạng thái"
        headerStyle="text-align:center;max-width:120px;height:50px"
        bodyStyle="text-align:center;max-width:120px"
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
        header=" "
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:250px;height:50px"
        bodyStyle="text-align:center;max-width:250px"
      >
        <template #body="Tem">
          <Button
            class="p-button-rounded p-button-secondary p-button-outlined mx-1"
            type="button"
            icon="pi pi-cog"
            v-tooltip.left="'Cấu hình báo cáo'"
            @click="configBaocao(Tem.data)"
          ></Button>
          <Button
            @click="copyTem(Tem.data)"
            class="p-button-rounded p-button-secondary p-button-outlined mx-1"
            type="button"
            icon="pi pi-copy"
            v-tooltip.left="'Copy báo cáo'"
          ></Button>
          <Button
            @click="editTem(Tem.data)"
            class="p-button-rounded p-button-secondary p-button-outlined mx-1"
            type="button"
            icon="pi pi-eye"
            v-tooltip.left="'Xem báo cáo'"
          ></Button>
          <div
       
          >
            <Button
              @click="editTem(Tem.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              v-tooltip.left="'Sửa'"
            ></Button>
            <Button
              class="p-button-rounded p-button-danger p-button-outlined mx-1"
              type="button"
              icon="pi pi-trash"
              @click="delTem(Tem.data)"
              v-tooltip.left="'Xóa'"
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
  </div>
  <Sidebar
    v-model:visible="visibleSidebarDoc"
    position="full"
    class="d-sidebar-full"
  >
    <template #header>
      <h2 class="p-0 m-0">
        <i class="pi pi-cog mr-2"></i>{{ smart_report.report_name }}
      </h2>
    </template>
    <div style="padding: 0 20px">
   <!-- {{ smart_report }} -->
      <DocComponent
        :isedit="true"
        :report="smart_report"
        :callbackFun="callbackFun"
      ></DocComponent>
    </div>
  </Sidebar>
  <Dialog
    :header="headerDialog"
    v-model:visible="displayBasic"
    :style="{ width: '40vw' }"
    :closable="true"
    :modal="true"
  >
    <form>
      <div class="grid formgrid">
        <div class="field col-12 md:col-12">
          <div class="col-12 text-left p-0 pb-2">
            Tên báo cáo <span class="redsao">(*)</span>
          </div>
          <InputText
            v-model="smart_report.report_name"
            spellcheck="false"
            class="col-12 ip36 px-2"
            :class="{
              'p-invalid': v$.report_name.$invalid && submitted,
            }"
          />
        </div>
        <!-- <div
          style="display: flex"
          class="field col-12 p-0 md:col-12"
          v-if="
            (v$.report_name.$invalid && submitted) ||
            v$.report_name.$pending.$response
          "
        >
          <small class="col-10 p-error">
            <span class="col-12 p-0">{{
              v$.report_name.required.$message
                .replace("Value", "Tên báo cáo")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div> -->

        <div class="col-12 field md:col-12 flex">
          <div class="col-6 md:col-6 p-0 align-items-center pr-1">
            <div class="col-12 text-left p-0 pb-2">Nhóm báo cáo  <span class="redsao">(*)</span></div>

            <Dropdown
              v-model="smart_report.report_group"
             
              :options="liReportGroup"
              optionLabel="name"
              optionValue="name"
              spellcheck="false"
              class="col-12 ip36"
              :class="{
              'p-invalid': smart_report.report_group==null && submitted,
            }"
              @change="onChangRPGR(smart_report.report_group)"
            />
          </div>
          <div class="col-6 md:col-6 p-0 align-items-center pl-1">
            <div class="col-12 text-left p-0 pb-2">STT</div>
            <InputNumber
              v-model="smart_report.is_order"
              class="col-12 ip36 p-0"
            />
          </div>
        </div>
        <div class="col-12 field md:col-12 flex align-items-center">
          <div class="col-6 field md:col-6 p-0 flex align-items-center">
            <div class="text-left p-0">Cho phép dùng chung</div>
            <InputSwitch
              v-model="smart_report.is_public"
              class="w-4rem lck-checked ml-3"
            />
          </div>
          <div class="col-6 field md:col-6 p-0 flex align-items-center pl-1">
            <div class="text-left p-0">Báo cáo dạng bảng (Excel)</div>
            <InputSwitch
              v-model="smart_report.is_table"
              class="w-4rem lck-checked ml-3"
            />
          </div>
        </div>
        <div class="col-12 field md:col-12 flex align-items-center">
          <div class="col-6 field md:col-6 p-0 flex align-items-center">
            <FileUpload
              mode="basic"
              name="demo[]"
              url="./upload.php"
              chooseLabel="Tải file mẫu"
              :multiple="false"
              accept=".doc,.docx,.xls,.xlsx"
              :maxFileSize="1000000000"
              :auto="true"
              @select="onUploadFile"
              @remove="removeFile"
            />
            <ProgressSpinner
              style="width: 30px; height: 30px"
              strokeWidth="8"
              fill="var(--surface-ground)"
              animationDuration=".5s"
              aria-label="Custom ProgressSpinner"
              v-if="checkDisabled && checkUploadFile == false"
            />
            <div
              class="w-full flex"
              v-if="checkDisabled == false && checkUploadFile"
            >
              <i
                class="pi pi-check text-green-500 font-bold w-full format-center"
                style="font-size: 1.5rem"
              ></i>
            </div>
          </div>
          <div class="col-3 field md:col-3 p-0 flex align-items-center pl-1">
            <div class="text-left p-0">Kích hoạt</div>
            <InputSwitch
              v-model="smart_report.status"
              class="w-4rem lck-checked ml-3"
            />
          </div>
          <div class="col-3 field md:col-3 p-0 flex align-items-center">
            <div class="text-left p-0">Template</div>
            <InputSwitch
              v-model="smart_report.is_temp"
              class="w-4rem lck-checked ml-3"
            />
          </div>
        </div>
        <div class="col-12 field md:col-12 flex align-items-center">
          <Panel toggleable class="w-full" :collapsed="collapsed1">
            <template #header>
              <div class="flex align-items-center p-0 m-0 font-bold text-lg">
                <button
                  class="p-panel-header-icon p-link p-0 m-0"
                  @click="toggle"
                >
                  <span class="pi pi-database font-bold text-lg"></span>
                </button>
                <div>Thông tin lấy dữ liệu</div>
              </div>
            </template>

            <div class="col-12 field md:col-12 flex">
              <div class="col-6 md:col-6 p-0 align-items-center pr-1">
                <div class="col-12 text-left p-0 pb-2">Thủ tục lấy dữ liệu  <span class="redsao">(*)</span></div>
                <div class="col-12 p-0 h-full">
                  <Dropdown
                    v-model="smart_report.proc_name1"
                    :options="listProcDropdown"
                    optionLabel="name"
                    optionValue="code"
                    class="w-full p-0"
                    style="height: auto; min-height: 36px"
                    :class="{
              'p-invalid': smart_report.proc_name1==null && submitted,
            }"
                  />
                </div>
              </div>
              <div class="col-6 md:col-6 p-0 align-items-center pl-1">
                <div class="col-12 text-left p-0 pb-2">Chọn nhân sự mẫu  <span class="redsao">(*)</span></div>
                <div class="col-12 p-0">
                  <DropdownUser
                    :model="smart_report.profile_id"
                    :placeholder="'Chọn nhân sự'"
                  
                    :editable="false"
                    optionLabel="profile_user_name"
                    optionValue="code"
                    :callbackFun="getProfileUser"
                    :key_user="'profile_id'"
                    :class="   (smart_report.proc_name1==null && submitted)?      'p-invalid w-full p-0':' w-full p-0' "
                  />
                </div>
              </div>
            </div>
       
            <div class="col-12 field md:col-12 flex" v-if="smart_report.report_type==3"  >
              <div class="col-6 p-0   align-items-center pr-1">
                <div class="col-12 text-left p-0 pb-2">
                  Thủ tục lấy danh sách hiển thị khi tra cứu  <span class="redsao">(*)</span>
                </div>
                <Dropdown
                  v-model="smart_report.proc_get"
                  :options="listProcDropdown"
                  optionLabel="name"
                  optionValue="code"
                  placeholder="Chọn thủ tục lấy dữ liệu"
                  class="col-12 p-0"
                  :class="   (smart_report.proc_get==null && submitted)?      'p-invalid w-full p-0':'' "
               
                />
              </div>
              <div class="col-6 p-0   align-items-center pl-1">
                <div class="col-12 text-left p-0 pb-2">
                  Loại quyết định
                </div>
                <Dropdown
                  v-model="smart_report.decision_id"
                  :options="listDecisions"
                  optionLabel="name"
                  optionValue="code"
                  placeholder="Chọn loại quyết định"
                  class="col-12 p-0"
                />
              </div>
            </div>
            <div class="col-12 field md:col-12  " v-else >
            
                <div class="col-12 text-left p-0 pb-2">
                  Thủ tục lấy danh sách hiển thị khi tra cứu
                </div>
                <Dropdown
                  v-model="smart_report.proc_get"
                  :options="listProcDropdown"
                  optionLabel="name"
                  optionValue="code"
                  placeholder="Chọn thủ tục lấy dữ liệu"
                  class="col-12 p-0"
                  :class="   (smart_report.proc_get==null && submitted)?      'p-invalid w-full p-0':'' "
               
                />
          
             
            </div>
          </Panel>
        </div>
        <div class="col-12 field md:col-12 flex align-items-center">
          <Panel toggleable class="w-full" :collapsed="collapsed2">
            <template #header>
              <div class="flex align-items-center p-0 m-0 font-bold text-lg">
                <button
                  class="p-panel-header-icon p-link p-0 m-0"
                  @click="toggle"
                >
                  <span class="pi pi-key font-bold text-lg"></span>
                </button>
                <div>Phân quyền truy cập báo cáo</div>
              </div>
            </template>
            <div class="col-12 field md:col-12">
              <div class="col-12 text-left p-0 pb-2 font-bold">
                <i class="pi pi-user-plus pr-2" style="font-size: 1.1rem"></i>
                Danh sách user được truy cập báo cáo
              </div>
              <DropdownUsers
                :model="smart_report.user_access_fake"
                :display="'chip'"
                :placeholder="'Chọn user được truy cập'"
                :type="1"
              />
            </div>
            <div class="col-12 field md:col-12">
              <div class="col-12 text-left p-0 pb-2 text-red-500 font-bold">
                <i class="pi pi-user-minus pr-2" style="font-size: 1.1rem"></i>
                Danh sách user không được truy cập báo cáo
              </div>
              <DropdownUsers
                :model="smart_report.user_deny_fake"
                :display="'chip'"
                :placeholder="'Chọn user không được truy cập'"
                :type="2"
              />
            </div>
          </Panel>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        v-if="isSaveTem == true"
        label="Xóa cấu hình"
        icon="pi pi-cog"
        @click="closeDialog"
        class="p-button-outlined p-button-danger"
      />

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
        :disabled="checkDisabled"
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
    