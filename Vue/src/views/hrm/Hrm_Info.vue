<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import moment from "moment";
import { encr, checkURL } from "../../util/function.js";
import { parse } from "date-fns/esm";
const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const router = inject("router");
const isDynamicSQL = ref(false);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const itemExport = "/Portals/Mau Excel/Mẫu Excel hồ sơ.xlsx";
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  language_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
const rules = {
  profile_id: {
    required,
    maxLength: maxLength(50),
    $errors: [
      {
        $property: "profile_id",
        $validator: "required",
        $message: "Mã nhân sự không được để trống!",
      },
    ],
  },
  profile_user_name: {
    required,
    maxLength: maxLength(250),
    $errors: [
      {
        $property: "profile_user_name",
        $validator: "required",
        $message: "Họ tên nhân sự không được để trống!",
      },
    ],
  },
};

const language = ref({
  language_name: "",
  current_num: 1,
  nav_type: 0,
  status: true,
  is_order: 1,
});
const profile = ref({
  status: 0,
  is_order: 1,
  profile_relative: {},
  profile_skill: {},
  profile_experience: {},
  profile_clan_history: {},
});
const genders = ref([
  { value: 1, text: "Nam" },
  { value: 2, text: "Nữ" },
]);
const hinhthucs = ref([
  { value: 0, text: "Dự bị" },
  { value: 1, text: "Chính thức" },
  { value: 1, text: "Điều chuyển" },
]);
//khai bao bien
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const isAdd = ref(false);
const active2 = ref(2);
const select_identity_place = ref(); // noi cap giay to
select_identity_place.value = {};
select_identity_place.value[-1] = true;

const select_birthplace = ref(); // noi sinh
select_birthplace.value = {};
select_birthplace.value[-1] = true;

const select_birthplace_origin = ref(); //que quan
select_birthplace_origin.value = {};
select_birthplace_origin.value[-1] = true;

const select_place_register_permanent = ref(); //Nơi đăng ký HKTT
select_place_register_permanent.value = {};
select_place_register_permanent.value[-1] = true;

const places = ref();
const headers = ref([]);
const isDisplayAvt = ref(false);
const selectedLanguages = ref();
const submitted = ref(false);
const v$ = useVuelidate(rules, profile);
const issaveProfile = ref(false);
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
const toast = useToast();
const checkDelList = ref(false);
const datalists = ref();
const datalist_places = ref();

const options = ref({
  IsNext: true,
  sort: null,
  SearchText: "",
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
});
// const Dictionarys = ref({
//   Identity_papers: [],
//   Nationalitys: [],
//   Ethnics: [],
//   Religions: [],
//   Banks: [],
//   Cultural_levels: [],
//   Academic_levels: [],
//   Specializations:[],
//   Political_theorys: [],
//   Language_levels: [],
//   Informatic_levels: [],
//   Relationships: [],
//   Form_tranings: [],
//   Certificates: [],
// });
const Dictionarys = ref([]);
//Thêm log
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
const loadCount = () => {
  axios
    .post(
      baseUrlCheck + "api/law_doc_language/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "law_doc_language_count",
            par: [
              { par: "search", va: options.value.SearchText },
              { par: "user_id", va: store.state.user.user_id },
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
        sttLanguage.value = options.value.totalRecords + 1;
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
    if (options.value.PageNo == 0) {
      loadCount();
    }
    axios
      .post(
        baseUrlCheck + "api/law_doc_language/GetDataProc",
        {
          str: encr(
            JSON.stringify({
              proc: "hrm_profile_list",
              par: [
                { par: "pageno", va: options.value.PageNo },
                { par: "pagesize", va: options.value.PageSize },
                { par: "search", va: options.value.SearchText },
                { par: "status", va: options.value.Status },
                { par: "user_id", va: store.state.user.user_id },
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
        // data.forEach((element, i) => {
        //   element.is_order =
        //     options.value.PageNo * options.value.PageSize + i + 1;
        //   element.open_date = moment(new Date(element.open_date)).format(
        //     "DD/MM/YYYY"
        //   );
        //   element.end_date = moment(new Date(element.end_date)).format(
        //     "DD/MM/YYYY"
        //   );
        // });
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
const loadTudien = () => {
  axios
    .post(
      baseUrlCheck + "api/FileMain/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_profile_dictionary",
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
      debugger;
      Dictionarys.value = data;
      if (Dictionarys.value[15].length > 0) {
        let obj = renderTree(Dictionarys.value[15], "place_id", "name", "");
        places.value = obj.arrtreeChils;
      }
      if (Dictionarys.value[16].length > 0) {
        headers.value = Dictionarys.value[16];
      }
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
};
const treedonvis = ref();
const loadDonvi = () => {
  axios
    .post(
      baseUrlCheck + "api/FileMain/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_org_list",
          }),
          SecretKey,
          cryoptojs
        ).toString(),
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

const checkSort = ref(false);
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

    options.value.id =
      datalists.value[datalists.value.length - 1].law_language_id;
    options.value.IsNext = true;
    if (checkSort.value) {
      options.value.id = null;
    }
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].law_language_id;
    options.value.IsNext = false;
  }
  options.value.PageNo = event.page;
  loadData(true);
};

//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);
// them
const openBasic = (str) => {
  isAdd.value = true;
  submitted.value = false;
  profile.value = {
    status: 0,
    is_order: 1,
    profile_relative: {},
    profile_skill: {},
    profile_experience: {},
    profile_clan_history: {},
  };
  if (store.state.user.is_super) {
    profile.value.organization_id = 0;
  } else {
    profile.value.organization_id = store.getters.user.organization_id;
  }
  issaveProfile.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};
// sua
const edit_Profile = (item) => {
  avts = [];
  submitted.value = false;
  isAdd.value = false;
  headerDialog.value = "Sửa hồ sơ";
  displayBasic.value = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/Users/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_profile_get",
            par: [{ par: "user_id", va: item.profile_id }],
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
        profile.value = data[0][0];
        //format date
        if (profile.value.birthday != null) {
          //profile.value.birthday = profile.value.birthday.split("T")[0];
          profile.value.birthday = new Date(profile.value.birthday);
        }
        if (profile.value.recruitment_date != null) {
          profile.value.recruitment_date = new Date(
            profile.value.recruitment_date
          );
        }
        if (profile.value.identity_date_issue != null) {
          profile.value.identity_date_issue = new Date(
            profile.value.identity_date_issue
          );
        }
        if (profile.value.military_start_date != null) {
          profile.value.military_start_date = new Date(
            profile.value.military_start_date
          );
        }
        if (profile.value.military_end_date != null) {
          profile.value.military_end_date = new Date(
            profile.value.military_end_date
          );
        }
        //get place
        select_birthplace.value = {};
        select_birthplace.value[profile.value.birthplace_id || -1] = true;
        select_birthplace_origin.value = {};
        select_birthplace_origin.value[
          profile.value.birthplace_origin_id || -1
        ] = true;
        select_place_register_permanent.value = {};
        select_place_register_permanent.value[
          profile.value.place_register_permanent || -1
        ] = true;
        select_identity_place.value = {};
        select_identity_place.value[
          profile.value.identity_place_id || -1
        ] = true;
        //get child
        if (data[1].length > 0) {
          profile.value.profile_clan_history = data[1][0];
          if (profile.value.profile_clan_history.start_date != null) {
            profile.value.profile_clan_history.start_date = new Date(
              profile.value.profile_clan_history.start_date
            );
          }
          if (profile.value.profile_clan_history.end_date != null) {
            profile.value.profile_clan_history.end_date = new Date(
              profile.value.profile_clan_history.end_date
            );
          }
        } else profile.value.profile_clan_history = {};

        if (data[2].length > 0) {
          profile.value.profile_experience = data[2][0];
          if (profile.value.profile_experience.start_date != null) {
            profile.value.profile_experience.start_date = new Date(
              profile.value.profile_experience.start_date
            );
          }
          if (profile.value.profile_experience.end_date != null) {
            profile.value.profile_experience.end_date = new Date(
              profile.value.profile_experience.end_date
            );
          }
        } else profile.value.profile_experience = {};

        if (data[3].length > 0) {
          profile.value.profile_skill = data[3][0];
          if (profile.value.profile_skill.start_date != null) {
            profile.value.profile_skill.start_date = new Date(
              profile.value.profile_skill.start_date
            );
          }
          if (profile.value.profile_skill.end_date != null) {
            profile.value.profile_skill.end_date = new Date(
              profile.value.profile_skill.end_date
            );
          }
          if (profile.value.profile_skill.certificate_start_date != null) {
            profile.value.profile_skill.certificate_start_date = new Date(
              profile.value.profile_skill.certificate_start_date
            );
          }
          if (profile.value.profile_skill.certificate_end_date != null) {
            profile.value.profile_skill.certificate_end_date = new Date(
              profile.value.profile_skill.certificate_end_date
            );
          }
        } else profile.value.profile_skill = {};

        if (data[4].length > 0) {
          profile.value.profile_relative = data[4][0];
          if (
            profile.value.profile_relative.identification_date_issue != null
          ) {
            profile.value.profile_relative.identification_date_issue = new Date(
              profile.value.profile_relative.identification_date_issue
            );
          }
          if (profile.value.profile_relative.start_date != null) {
            profile.value.profile_relative.start_date = new Date(
              profile.value.profile_relative.start_date
            );
          }
          if (profile.value.profile_relative.end_date != null) {
            profile.value.profile_relative.end_date = new Date(
              profile.value.profile_relative.end_date
            );
          }
        } else profile.value.profile_relative = {};
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
const saveProfile = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  debugger;
  // get place
  profile.value.identity_place_id =
    Object.keys(select_identity_place.value)[0] == -1
      ? null
      : Object.keys(select_identity_place.value)[0];
  profile.value.birthplace_id =
    Object.keys(select_birthplace.value)[0] == -1
      ? null
      : Object.keys(select_birthplace.value)[0];
  profile.value.birthplace_origin_id =
    Object.keys(select_birthplace_origin.value)[0] == -1
      ? null
      : Object.keys(select_birthplace_origin.value)[0];
  profile.value.place_register_permanent =
    Object.keys(select_place_register_permanent.value)[0] == -1
      ? null
      : Object.keys(select_place_register_permanent.value)[0];
  let formData = new FormData();
  formData.append("profile", JSON.stringify(profile.value));
  formData.append(
    "skill",
    JSON.stringify(
      !Object.keys(profile.value.profile_skill).length
        ? ""
        : [profile.value.profile_skill]
    )
  );
  formData.append(
    "relative",
    JSON.stringify(
      !Object.keys(profile.value.profile_relative).length
        ? ""
        : [profile.value.profile_relative]
    )
  );
  formData.append(
    "experience",
    JSON.stringify(
      !Object.keys(profile.value.profile_experience).length
        ? ""
        : [profile.value.profile_experience]
    )
  );
  formData.append(
    "clan_history",
    JSON.stringify(
      !Object.keys(profile.value.profile_clan_history).length
        ? ""
        : [profile.value.profile_clan_history]
    )
  );
  //save avt
  for (var k in avts) {
    let file = avts[k];
    formData.append("avatar", file);
  }
  axios({
    method: isAdd.value == false ? "put" : "post",
    url:
      baseURL +
      `/api/Profile/${isAdd.value == false ? "update_profile" : "add_profile"}`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  }).then((response) => {
    if (response.data.err === "0") {
      swal.close();
      toast.success("Cập nhật thành công!");
      displayBasic.value = false;
      loadData(true);
    } else {
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    }
  });
};
const closeDialog = () => {
  displayBasic.value = false;
};
//xoa
// xóa người dùng
const del_Profile = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá hồ sơ này không!",
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
          .delete(baseURL + "/api/Profile/del_profile", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: [md.profile_id],
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xóa hồ sơ thành công!");
              loadData(true);
            } else {
              swal.fire({
                title: "Thông báo!",
                text: "Xóa không thành công, vui lòng thử lại",
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
      baseUrlCheck + "/api/Excel/ExportExcel",
      {
        excelname: "DANH SÁCH NGÔN NGỮ",
        proc: "law_doc_language_listexport",
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

//Sort
const onSort = (event) => {
  options.value.sort =
    event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
  if (event.sortField != "is_order") {
    options.value.sort +=
      ",is_order " + (event.sortOrder == 1 ? " ASC" : " DESC");
  }
  if (event.sortField != "language_name") {
    options.value.sort +=
      ",language_name " + (event.sortOrder == 1 ? " ASC" : " DESC");
  }
  options.value.sort += ", created_date DESC";
  checkSort.value = true;
  isDynamicSQL.value = true;
  loadDataSQL();
};
const isFirst = ref(true);
const filterSQL = ref([]);
const loadDataSQL = () => {
  let data = {
    sqlS:
      filterTrangthai.value != null ? filterTrangthai.value.toString() : null,
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
    .post(baseUrlCheck + "/api/SQL/Filter_Law_Doc_Language", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.is_order =
            options.value.PageNo * options.value.PageSize + i + 1;
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
      toast.error("Tải dữ liệu không thành công!" + error.message);
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

const sttLanguage = ref();
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
// avatar
let avts = [];
const handleFileUpload = (event) => {
  avts = event.target.files;
  isDisplayAvt.value = true;
  var output = document.getElementById("logoLang");
  output.src = URL.createObjectURL(event.target.files[0]);
  output.onload = function () {
    URL.revokeObjectURL(output.src); // free memory
  };
};
const chonanh = (id) => {
  document.getElementById(id).click();
};
const initPlaces = () => {
  axios
    .post(
      baseURL + "/api/FileMain/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "ca_places_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 100 },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      RenderData(response);
    })
    .catch((error) => {
      console.log(error)
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
//delete img
const delAvatar = () => {
  avts = [];
  isDisplayAvt.value = false;
  var output = document.getElementById("logoLang");
  output.src = basedomainURL + "/Portals/Image/noimg.jpg";
  profile.value.avatar = null;
};
//define function
const RenderData = (response) => {
  options.value.allRecord = null;
  let list1 = [];
  let list2 = [];
  let list3 = [];
  let d1 = JSON.parse(response.data.data)[0];
  d1.forEach((element, i) => {
    let c = {
      key: element.place_id,
      data: element.place_id,
      label: element.name,
      children: null,
    };
    if (d1[i].children) {
      list2 = JSON.parse(d1[i].children);
      if (list2 != null) {
        list2.forEach((element, i) => {
          element.label = element.data.name;
          element.data = parseInt(element.data.place_id);
          element.key = element.data;
          //đổi is_order
          if (list2[i].children != null && list2[i].children.length >0) {
            // list3 = list2[i].children;
            // list2[i].children = list3;
              list2[i].children.forEach((element, i) => {
                element.label = element.data.name;
                element.data = parseInt(element.data.place_id);
                          element.key = element.data;
              });
            
          }
        });
      }
      c.children = list2;
    }
    list1.push(c);
  });
  datalist_places.value = list1;
  // if (JSON.parse(response.data.data)[1]) {
  //   let data2 = JSON.parse(response.data.data)[1];
  //   options.value.allRecord = data2[0].allRecord;
  // } else {
  //   options.value.allRecord = datalist_places.value.length;
  // }
};
const renderTree = (data, id, name, title) => {
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
  arrtreeChils.unshift({
    key: -1,
    data: -1,
    label: "-----Chọn " + title + "----",
  });
  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
onMounted(() => {
  loadData(true);
  loadTudien();
  initPlaces();
});
</script>

<template>
  <div class="surface-100">
    <div class="h-2rem p-3 pb-0 m-3 mb-0 surface-0">
      <h3 class="m-0">
        <i class="pi pi-globe"></i> Danh sách hồ sơ ({{ options.totalRecords }})
      </h3>
    </div>
    <Toolbar class="outline-none mx-3 surface-0 border-none">
      <template #start>
        <span class="p-input-icon-left">
          <i class="pi pi-search" />
          <InputText
            v-model="options.SearchText"
            v-on:keyup.enter="searchLanguages"
            type="text"
            spellcheck="false"
            placeholder="Tìm kiếm hồ sơ"
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
            :style="
              store.state.user.is_super == 1 ? 'width:40rem' : 'width:300px'
            "
          >
            <div class="grid formgrid m-0">
              <div class="field col-12 md:col-12 p-0 mb-1">
                <div class="flex col-12 p-0 mb-3">
                  <div
                    :class="
                      store.state.user.is_super == 1
                        ? 'col-2 text-left pt-2 p-0'
                        : 'col-4 text-left pt-2 p-0'
                    "
                    style="text-align: left"
                  >
                    Phân loại
                  </div>
                  <div
                    :class="
                      store.state.user.is_super == 1
                        ? 'col-10 pr-0'
                        : 'col-8 pr-0'
                    "
                  >
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
                <div>
                  <div class="flex">
                    <div
                      :class="
                        store.state.user.is_super == 1
                          ? 'col-2 text-left pt-2 p-0'
                          : 'col-4 text-left pt-2 p-0'
                      "
                      style="text-align: left"
                    >
                      Trạng thái
                    </div>
                    <div
                      :class="
                        store.state.user.is_super == 1
                          ? 'col-10 pr-0'
                          : 'col-8 pr-0'
                      "
                    >
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
                      <Button
                        class="p-button-outlined"
                        label="Xóa"
                        @click="reFilterLanguage"
                      ></Button>
                    </template>
                    <template #end>
                      <Button @click="filterLanguages" label="Lọc"></Button>
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
          @click="openBasic('Thêm hồ sơ')"
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
    <div class="d-lang-table mx-3">
      <TabView :v-model:activeIndex="2">
        <TabPanel v-for="(item, index) in headers" :key="index">
          <template #header>
            <span>{{ item.profile_header_name }}</span>
          </template>
        </TabPanel>
      </TabView>
      <table
        class="
          table table-condensed table-hover
          tbpad
          table-scroll
          bg-white
          w-full
        "
      >
        <tbody>
          <tr v-for="(item, index) in datalists" :key="index">
            <td width="100" align="center">
                            <div
                class="flex"
                style="justify-content: center;"
              >
                <img
                  v-if="item.avatar"
                  class="cp-0 avatar"
                  :src="basedomainURL + item.avatar"
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/nouser1.png'
                  "
                />
                <div
                  class="avatar format-center"
                  v-if="!item.avatar"
                  :style="{
                    background: bgColor[index % 7],
                  }"
                >
                  <span
                    style="color: #fff; font-size: 1.5rem; font-weight: 500"
                    >{{
                      item.profile_user_name.substring(0, 1).toUpperCase()
                    }}</span
                  >
                </div>
              </div>
            </td>
            <td>
              <div>
                <b>{{ item.profile_user_name }}</b>
              </div>
              <div>{{ item.profile_id }}</div>
              <div v-if="item.recruitment_date">
                Ngày vào:
                {{
                  moment(new Date(item.recruitment_date)).format("DD/MM/YYYY")
                }}
              </div>
            </td>
            <td width="300">
              <div v-if="item.gender">
                {{ item.gender == 1 ? "Nam" : "Nữ" }}
              </div>
              <div>
                {{ moment(new Date(item.birthday)).format("DD/MM/YYYY") }}
              </div>
              <div>{{ item.birthplace_name }}</div>
            </td>
            <td width="200">
              <div>{{ item.phone }}</div>
              <div>{{ item.email }}</div>
            </td>
            <td width="100">
              <div class="flex">
                <Button
                  v-tooltip.top="'Sửa hồ sơ'"
                  @click="edit_Profile(item)"
                  class="
                    p-button-rounded p-button-secondary p-button-outlined
                    mx-1
                  "
                  type="button"
                  icon="pi pi-pencil"
                ></Button>
                <Button
                  v-tooltip.top="'Xóa hồ sơ'"
                  class="
                    p-button-rounded p-button-secondary p-button-outlined
                    mx-1
                  "
                  type="button"
                  icon="pi pi-trash"
                  @click="del_Profile(item)"
                ></Button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
  <Dialog
    header="Thông tin hồ sơ"
    v-model:visible="displayBasic"
    :style="{ width: '72vw' }"
    :closable="true"
    :maximizable="true"
  >
    <form @submit.prevent="saveProfile(!v$.$invalid)">
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 flex">
          <Accordion class="accordion-custom w-full" :activeIndex="0">
            <!-- 1. Thông tin chung -->
            <AccordionTab>
              <template #header>
                <span>1. Thông tin chung</span>
              </template>
              <div class="field col-12 md:col-12 flex">
                <div class="field col-3">
                  <!-- <img
                    id="Anhdaidien"
                    style="width: 200px; height: 200px"
                    v-bind:src="basedomainURL + '/Portals/Image/noimg.jpg'"
                  /> -->
                  <div class="col-12 text-center field mt-2">Ảnh đại diện</div>
                  <div class="inputanh relative" style="margin: 0 auto">
                    <img
                      @click="chonanh('AnhLang')"
                      id="logoLang"
                      v-bind:src="
                        profile.avatar
                          ? basedomainURL + profile.avatar
                          : basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                    />
                    <Button
                      v-if="profile.avatar || isDisplayAvt"
                      style="width: 1.5rem; height: 1.5rem"
                      icon="pi pi-times"
                      @click="delAvatar"
                      class="
                        p-button-rounded
                        absolute
                        top-0
                        right-0
                        cursor-pointer
                      "
                    />
                    <input
                      class="ipnone"
                      id="AnhLang"
                      type="file"
                      accept=".png,.jpg,.jpeg,.gif,.raw"
                      @change="handleFileUpload"
                    />
                  </div>
                </div>
                <div class="field col-9">
                  <div class="field col-12 md:col-12">
                    <label class="col-2 text-left"
                      >Mã nhân sự <span class="redsao">(*)</span></label
                    >
                    <InputText
                      spellcheck="false"
                      class="col-4 ip33"
                      v-model="profile.profile_id"
                      v-bind:disabled="!isAdd"
                    />
                    <label class="col-2 text-left p-0 pl-2">Mã chấm công</label>
                    <InputText
                      spellcheck="false"
                      class="col-4 ip33"
                      v-model="profile.check_in_id"
                    />
                  </div>
                  <div class="field col-12 md:col-12">
                    <small
                      class="col-12 p-error block"
                      v-if="
                        (v$.profile_id.required.$invalid && submitted) ||
                        v$.profile_id.required.$pending.$response
                      "
                    >
                      <div class="field col-12 md:col-12 flex">
                        <label class="col-2 text-left"></label>
                        <span class="col-4 p-0">
                          {{
                            v$.profile_id.required.$message
                              .replace("Value", "Mã nhân sự ")
                              .replace("is required", "không được để trống")
                          }}
                        </span>
                      </div>
                    </small>
                    <small
                      class="col-12 p-error block"
                      v-if="
                        (v$.profile_id.maxLength.$invalid && submitted) ||
                        v$.profile_id.maxLength.$pending.$response
                      "
                    >
                      <div class="field col-12 md:col-12 flex">
                        <label class="col-2 text-left"></label>
                        <span class="col-4 p-0">
                          {{
                            v$.profile_id.maxLength.$message.replace(
                              "The maximum length allowed is",
                              "Mã nhân sự không được vượt quá"
                            )
                          }}
                          ký tự
                        </span>
                      </div>
                    </small>
                  </div>
                  <div class="field col-12 md:col-12">
                    <label class="col-2 text-left">Mã quản lý cấp trên</label>
                    <InputText
                      spellcheck="false"
                      class="col-4 ip33"
                      v-model="profile.superior_id"
                    />
                    <label class="col-2 text-left p-0 pl-2"
                      >Ngày tuyển dụng</label
                    >
                    <Calendar
                      class="col-4 ip33"
                      id="icon"
                      v-model="profile.recruitment_date"
                      :showIcon="true"
                    />
                    <!-- <InputText spellcheck="false" class="col-4 ip33" /> -->
                  </div>
                  <div class="field col-12 md:col-12">
                    <label class="col-2 text-left"
                      >Họ và tên <span class="redsao">(*)</span></label
                    >
                    <InputText
                      spellcheck="false"
                      class="col-4 ip33"
                      v-model="profile.profile_user_name"
                    />
                    <label class="col-2 text-left p-0 pl-2">Tên gọi khác</label>
                    <InputText
                      spellcheck="false"
                      class="col-4 ip33"
                      v-model="profile.profile_nick_name"
                    />
                  </div>
                  <div class="field col-12 md:col-12">
                    <small
                      class="col-12 p-error block"
                      v-if="
                        (v$.profile_user_name.required.$invalid && submitted) ||
                        v$.profile_user_name.required.$pending.$response
                      "
                    >
                      <div class="field col-12 md:col-12 flex">
                        <label class="col-2 text-left"></label>
                        <span class="col-4 p-0">
                          {{
                            v$.profile_user_name.required.$message
                              .replace("Value", "Họ tên ")
                              .replace("is required", "không được để trống")
                          }}
                        </span>
                      </div>
                    </small>
                    <small
                      class="col-12 p-error block"
                      v-if="
                        (v$.profile_user_name.maxLength.$invalid &&
                          submitted) ||
                        v$.profile_user_name.maxLength.$pending.$response
                      "
                    >
                      <div class="field col-12 md:col-12 flex">
                        <label class="col-2 text-left"></label>
                        <span class="col-4 p-0">
                          {{
                            v$.profile_user_name.maxLength.$message.replace(
                              "The maximum length allowed is",
                              "Họ tên không được vượt quá"
                            )
                          }}
                          ký tự
                        </span>
                      </div>
                    </small>
                  </div>
                  <div class="field col-12 md:col-12">
                    <label class="col-2 text-left"
                      >Ngày sinh <span class="redsao">(*)</span></label
                    >
                    <Calendar
                      class="col-4 ip33"
                      id="icon"
                      v-model="profile.birthday"
                      :showIcon="true"
                      :manualInput="false"
                    />
                    <label class="col-2 text-left p-0 pl-2">Giới tính</label>
                    <Dropdown
                      class="col-4 ip33"
                      v-model="profile.gender"
                      :options="genders"
                      optionLabel="text"
                      optionValue="value"
                      placeholder="Chọn giới tính"
                    />
                  </div>
                </div>
              </div>
              <div class="field col-12 md:col-12">
                <label class="col-2 text-left"
                  >Nơi sinh <span class="redsao">(*)</span></label
                >
                <TreeSelect
                  class="col-10 p-0 ip33"
                  v-model="select_birthplace"
                  :options="datalist_places"
                  :showClear="true"
                  :max-height="200"
                  placeholder="Chọn nơi sinh"
                  optionLabel="name"
                  optionValue="place_id"
                >
                </TreeSelect>
              </div>
              <div class="field col-12 md:col-12">
                <label class="col-2 text-left">Quê quán</label>
                <TreeSelect
                  class="col-10 p-0 ip33"
                  v-model="select_birthplace_origin"
                  :options="datalist_places"
                  :showClear="true"
                  :max-height="200"
                  placeholder="Chọn quê quán"
                  optionLabel="name"
                  optionValue="place_id"
                >
                </TreeSelect>
              </div>
              <div class="field col-12 md:col-12">
                <label class="col-2 text-left">Nơi đăng ký HKTT</label>
                <TreeSelect
                  class="col-10 p-0 ip33"
                  v-model="select_place_register_permanent"
                  :options="datalist_places"
                  :showClear="true"
                  :max-height="200"
                  placeholder="Chọn nơi đăng ký"
                  optionLabel="name"
                  optionValue="place_id"
                >
                </TreeSelect>
              </div>
              <div class="field col-12 md:col-12">
                <label class="col-2 text-left">Loại giấy tờ</label>
                <Dropdown
                  :showClear="true"
                  :options="Dictionarys[0]"
                  optionLabel="identity_papers_name"
                  optionValue="identity_papers_id"
                  placeholder="Chọn loại"
                  class="p-dropdown-sm col-2 p-0"
                  v-model="profile.identity_papers_id"
                />
                <label class="col-2 text-left p-0 pl-5">Số</label>
                <InputText
                  spellcheck="false"
                  class="col-2 ip33"
                  v-model="profile.identity_papers_code"
                />
                <label class="col-2 text-left p-0 pl-5">Ngày cấp</label>
                <Calendar
                  class="col-2 ip33"
                  id="icon"
                  v-model="profile.identity_date_issue"
                  :showIcon="true"
                />
              </div>
              <div class="field col-12 md:col-12">
                <label class="col-2 text-left">Nơi cấp </label>
                <TreeSelect
                  class="col-2 p-0"
                  v-model="select_identity_place"
                  :options="datalist_places"
                  :showClear="true"
                  :max-height="200"
                  placeholder="Chọn nơi cấp"
                  optionLabel="name"
                  optionValue="place_id"
                >
                </TreeSelect>
                <label class="col-2 text-left p-0 pl-5">Quốc tịch</label>
                <Dropdown
                  :showClear="true"
                  :options="Dictionarys[1]"
                  optionLabel="nationality_name"
                  optionValue="nationality_id"
                  placeholder="Chọn quốc tịch"
                  class="p-dropdown-sm col-2 p-0"
                  v-model="profile.nationality_id"
                />
                <label class="col-2 text-left p-0 pl-5"
                  >Trình trạng hôn nhân</label
                >
                <InputText spellcheck="false" class="col-2 ip33" />
              </div>
              <div class="field col-12 md:col-12">
                <label class="col-2 text-left">Dân tộc</label>
                <Dropdown
                  :showClear="true"
                  :options="Dictionarys[2]"
                  optionLabel="ethnic_name"
                  optionValue="ethnic_id"
                  placeholder="Chọn dân tộc"
                  class="p-dropdown-sm col-2 p-0"
                  v-model="profile.ethnic_id"
                />
                <label class="col-2 text-left p-0 pl-5">Tôn giáo</label>
                <Dropdown
                  :showClear="true"
                  :options="Dictionarys[3]"
                  optionLabel="religion_name"
                  optionValue="religion_id"
                  placeholder="Chọn tôn giáo"
                  class="p-dropdown-sm col-2 p-0"
                  v-model="profile.religion_id"
                />
                <label class="col-2 text-left p-0 pl-5">Mã số thuế</label>
                <InputText
                  spellcheck="false"
                  class="col-2 ip33"
                  v-model="profile.tax_code"
                />
              </div>
              <div class="field col-12 md:col-12">
                <label class="col-2 text-left">Ngân hàng</label>
                <Dropdown
                  :showClear="true"
                  :options="Dictionarys[4]"
                  optionLabel="bank_name"
                  optionValue="bank_id"
                  placeholder="Chọn ngân hàng"
                  class="p-dropdown-sm col-2 p-0"
                  v-model="profile.bank_id"
                />
                <label class="col-2 text-left p-0 pl-5">Số tài khoản</label>
                <InputText
                  spellcheck="false"
                  class="col-2 ip33"
                  v-model="profile.bank_number"
                />
                <label class="col-2 text-left p-0 pl-5">Tên tài khoản</label>
                <InputText
                  spellcheck="false"
                  class="col-2 ip33"
                  v-model="profile.bank_account"
                />
              </div>
            </AccordionTab>
            <AccordionTab>
              <template #header>
                <!-- <i class="pi pi-book mr-2"></i> -->
                <span>2. Trình độ học vấn</span>
              </template>
              <div class="field col-12 md:col-12">
                <label class="col-2 text-left">Trình độ phổ thông</label>
                <Dropdown
                  :showClear="true"
                  :options="Dictionarys[5]"
                  optionLabel="cultural_level_name"
                  optionValue="cultural_level_id"
                  placeholder="Chọn trình độ"
                  class="p-dropdown-sm col-4 p-0"
                  v-model="profile.cultural_level_id"
                />
                <label class="col-2 text-left p-0 pl-2"
                  >Trình độ học vấn cao nhất</label
                >
                <Dropdown
                  :showClear="true"
                  :options="Dictionarys[6]"
                  optionLabel="academic_level_name"
                  optionValue="academic_level_id"
                  placeholder="Chọn trình độ"
                  class="p-dropdown-sm col-4 p-0"
                  v-model="profile.academic_level_id"
                />
              </div>
              <div class="field col-12 md:col-12">
                <label class="col-2 text-left">Chuyên ngành học</label>
                <Dropdown
                  :showClear="true"
                  :options="Dictionarys[7]"
                  optionLabel="specialization_name"
                  optionValue="specialization_id"
                  placeholder="Chọn ngành"
                  class="p-dropdown-sm col-4 p-0"
                  v-model="profile.specialization_id"
                />
                <label class="col-2 text-left p-0 pl-2">Quản lý nhà nước</label>
                <Dropdown
                  :showClear="true"
                  :options="Dictionarys[14]"
                  optionLabel="management_state_name"
                  optionValue="management_state_id"
                  placeholder="Chọn cấp"
                  class="p-dropdown-sm col-4 p-0"
                  v-model="profile.management_state_id"
                />
              </div>
              <div class="field col-12 md:col-12">
                <label class="col-2 text-left">Lý luận chính trị</label>
                <Dropdown
                  :showClear="true"
                  :options="Dictionarys[8]"
                  optionLabel="political_theory_name"
                  optionValue="political_theory_id"
                  placeholder="Chọn cấp"
                  class="p-dropdown-sm col-2 p-0"
                  v-model="profile.political_theory_id"
                />
                <label class="col-2 text-center">Ngoại ngữ</label>
                <Dropdown
                  :showClear="true"
                  :options="Dictionarys[9]"
                  optionLabel="language_level_name"
                  optionValue="language_level_id"
                  placeholder="Chọn cấp"
                  class="p-dropdown-sm col-2 p-0"
                  v-model="profile.language_level_id"
                />
                <label class="col-2 text-center">Tin học</label>
                <Dropdown
                  :showClear="true"
                  :options="Dictionarys[10]"
                  optionLabel="informatic_level_name"
                  optionValue="informatic_level_id"
                  placeholder="Chọn cấp"
                  class="p-dropdown-sm col-2 p-0"
                  v-model="profile.informatic_level_id"
                />
              </div>
            </AccordionTab>
            <AccordionTab>
              <template #header>
                <!-- <i class="pi pi-info-circle mr-2"></i> -->
                <span>3. Thông tin liên hệ</span>
              </template>
              <div class="field col-12 md:col-12">
                <label class="col-2 text-left">Số điện thoại</label>
                <InputText
                  spellcheck="false"
                  class="col-4 ip33"
                  v-model="profile.phone"
                />
                <label class="col-2 text-left p-0 pl-5">Email</label>
                <InputText
                  spellcheck="false"
                  class="col-4 ip33"
                  v-model="profile.email"
                />
              </div>
              <div class="field col-12 md:col-12">
                <label class="col-2 text-left">Thường trú</label>
                <InputText
                  spellcheck="false"
                  class="col-4 ip33"
                  v-model="profile.place_permanent"
                />
                <label class="col-2 text-left p-0 pl-5">Chỗ ở hiện nay</label>
                <InputText
                  spellcheck="false"
                  class="col-4 ip33"
                  v-model="profile.place_residence"
                />
              </div>
              <h4>Khi cần báo tin cho</h4>
              <div class="field col-12 md:col-12">
                <label class="col-2 text-left">Cho</label>
                <InputText
                  spellcheck="false"
                  class="col-2 ip33"
                  v-model="profile.involved_name"
                />
                <label class="col-2 text-center">SĐT</label>
                <InputText
                  spellcheck="false"
                  class="col-2 ip33"
                  v-model="profile.involved_phone"
                />
                <label class="col-2 text-center">Địa chỉ</label>
                <InputText
                  spellcheck="false"
                  class="col-2 ip33"
                  v-model="profile.involved_place"
                />
              </div>
            </AccordionTab>
            <!-- 4. Thông tin gia đình, người phụ thuộc -->
            <AccordionTab>
              <template #header>
                <Toolbar class="w-full custoolbar p-0 font-bold">
                  <template #start>
                    <!-- <i class="pi pi-users mr-2"></i> -->
                    <span
                      >4. Thông tin gia đình, người phụ thuộc</span
                    ></template
                  >
                  <template #end>
                    <i
                      class="pi pi-plus-circle text-2xl"
                      v-tooltip.top="'Thêm thông tin'"
                    ></i>
                  </template>
                </Toolbar>
              </template>
              <Accordion class="accordion-custom w-full" activeIndex="0">
                <AccordionTab>
                  <template #header>
                    <span class="font-medium">Thông tin 1</span>
                  </template>
                  <div class="field col-12 md:col-12">
                    <label class="col-2 text-left">Họ tên</label>
                    <InputText
                      spellcheck="false"
                      class="col-4 ip33"
                      v-model="profile.profile_relative.relative_name"
                    />
                    <label class="col-2 text-center">Mối quan hệ</label>
                    <Dropdown
                      :showClear="true"
                      :options="Dictionarys[11]"
                      optionLabel="relationship_name"
                      optionValue="relationship_id"
                      placeholder="Chọn quan hệ"
                      class="p-dropdown-sm col-4 p-0"
                      v-model="profile.profile_relative.relationship_id"
                    />
                  </div>
                  <div class="field col-12 md:col-12">
                    <label class="col-2 text-left">Năm sinh</label>
                    <InputText
                      spellcheck="false"
                      class="col-2 ip33"
                      v-model="profile.profile_relative.birthday"
                    />
                    <label class="col-2 text-left p-0 pl-5">SĐT</label>
                    <InputText
                      spellcheck="false"
                      class="col-2 ip33"
                      v-model="profile.profile_relative.phone"
                    />
                    <label class="col-2 text-left p-0 pl-5">Mã số thuế</label>
                    <InputText
                      spellcheck="false"
                      class="col-2 ip33"
                      v-model="profile.profile_relative.tax_code"
                    />
                  </div>
                  <div class="field col-12 md:col-12">
                    <label class="col-2 text-left">CCCD/Hộ chiếu</label>
                    <InputText
                      spellcheck="false"
                      class="col-2 ip33"
                      v-model="profile.profile_relative.identification_citizen"
                    />
                    <label class="col-2 text-left p-0 pl-5">Ngày cấp</label>
                    <Calendar
                      class="col-2 ip33"
                      id="icon"
                      v-model="
                        profile.profile_relative.identification_date_issue
                      "
                      :showIcon="true"
                    />
                    <label class="col-2 text-left p-0 pl-5">Nơi cấp</label>
                    <InputText
                      spellcheck="false"
                      class="col-2 ip33"
                      v-model="
                        profile.profile_relative.identification_place_issue
                      "
                    />
                  </div>
                  <div class="field col-12 md:col-12">
                    <label class="col-2 text-left">Phụ thuộc</label>
                    <label class="col-2"
                      ><InputSwitch
                        class="col-2"
                        v-model="profile.profile_relative.is_dependent"
                    /></label>
                    <!-- <div class="col-1"></div> -->
                    <label class="col-2 text-left p-0 pl-5">Từ ngày</label>
                    <Calendar
                      class="col-2 ip33"
                      id="icon"
                      v-model="profile.profile_relative.start_date"
                      :showIcon="true"
                    />
                    <label class="col-2 text-left p-0 pl-5">Đến ngày</label>
                    <Calendar
                      class="col-2 ip33"
                      id="icon"
                      v-model="profile.profile_relative.end_date"
                      :showIcon="true"
                    />
                  </div>
                  <div class="field col-12 md:col-12 flex">
                    <label class="col-2 text-left">Thông tin cơ bản</label>
                    <Textarea
                      v-model="profile.profile_relative.info"
                      :autoResize="true"
                      rows="4"
                      class="col-10"
                      placeholder="Quê quán, nghề nghiệp, chức danh, chức vụ, đơn vị công tác,..."
                    />
                  </div>
                  <div class="field col-12 md:col-12 flex">
                    <label class="col-2 text-left">Ghi chú</label>
                    <Textarea
                      v-model="profile.profile_relative.note"
                      :autoResize="true"
                      rows="4"
                      class="col-10"
                    />
                  </div>
                </AccordionTab>
              </Accordion>
            </AccordionTab>
            <AccordionTab>
              <template #header>
                <Toolbar class="w-full custoolbar p-0 font-bold">
                  <template #start>
                    <!-- <i class="pi pi-replay mr-2"></i> -->
                    <span
                      >5. Quá trình đào tạo, bồi dưỡng về chuyên môn, nghiệp vụ,
                      lý luận chính trị, ngoại ngữ, tin học</span
                    ></template
                  >
                  <template #end>
                    <i
                      class="pi pi-plus-circle text-2xl"
                      v-tooltip.top="'Thêm thông tin'"
                    ></i>
                  </template>
                </Toolbar>
              </template>
              <Accordion class="accordion-custom w-full" activeIndex="0">
                <AccordionTab>
                  <template #header>
                    <span class="font-medium">Thông tin 1</span>
                  </template>
                  <div class="field col-12 md:col-12">
                    <label class="col-2 text-left">Tên trường</label>
                    <InputText
                      spellcheck="false"
                      class="col-4 ip33"
                      v-model="profile.profile_skill.university_name"
                    />
                    <label class="col-2 text-left p-0 pl-4"
                      >Chuyên ngành đào tạo</label
                    >
                    <InputText
                      spellcheck="false"
                      class="col-4 ip33"
                      v-model="profile.profile_skill.specialized"
                    />
                  </div>
                  <div class="field col-12 md:col-12">
                    <label class="col-2 text-left">Từ tháng, năm</label>
                    <Calendar
                      v-model="profile.profile_skill.start_date"
                      view="month"
                      dateFormat="mm/yy"
                      class="col-4 p-0 ip33"
                      placeholder="Bắt đầu"
                    />
                    <label class="col-2 text-left p-0 pl-4"
                      >Đến tháng, năm</label
                    >
                    <Calendar
                      v-model="profile.profile_skill.end_date"
                      view="month"
                      dateFormat="mm/yy"
                      class="col-4 p-0 ip33"
                      placeholder="Kết thúc"
                    />
                  </div>
                  <div class="field col-12 md:col-12">
                    <label class="col-2 text-left">Hình thức đào tạo</label>
                    <Dropdown
                      :showClear="true"
                      :options="Dictionarys[12]"
                      optionLabel="form_traning_name"
                      optionValue="form_traning_id"
                      placeholder="Chọn hình thức"
                      class="p-dropdown-sm col-4 p-0"
                      v-model="profile.profile_skill.form_traning_id"
                    />
                    <label class="col-2 text-left p-0 pl-4"
                      >Văn bằng, chứng chỉ</label
                    >
                    <Dropdown
                      :showClear="true"
                      :options="Dictionarys[13]"
                      optionLabel="certificate_name"
                      optionValue="certificate_id"
                      placeholder="Chọn văn bằng"
                      class="p-dropdown-sm col-4 p-0"
                      v-model="profile.profile_skill.certificate_id"
                    />
                  </div>
                  <div class="field col-12 md:col-12">
                    <label class="col-2 text-left">Ngày hiệu lực</label>
                    <Calendar
                      class="col-4 ip33"
                      id="icon"
                      v-model="profile.profile_skill.certificate_start_date"
                      :showIcon="true"
                    />
                    <label class="col-2 text-left p-0 pl-4"
                      >Ngày hết hiệu lực</label
                    >
                    <Calendar
                      class="col-4 ip33"
                      id="icon"
                      v-model="profile.profile_skill.certificate_end_date"
                      :showIcon="true"
                    />
                  </div>
                  <div class="field col-12 md:col-12">
                    <label class="col-2 text-left">Số hiệu</label>
                    <InputText
                      spellcheck="false"
                      class="col-2 ip33"
                      v-model="profile.profile_skill.certificate_key_code"
                    />
                    <label class="col-2 text-center">Phiên bản</label>
                    <InputText
                      spellcheck="false"
                      class="col-2 ip33"
                      v-model="profile.profile_skill.certificate_version"
                    />
                    <label class="col-2 text-center">Lần phát hành</label>
                    <InputText
                      spellcheck="false"
                      class="col-2 ip33"
                      v-model="profile.profile_skill.certificate_release_time"
                    />
                  </div>
                  <div class="field col-12 md:col-12 flex">
                    <label class="text-left col-2">Tệp đình kèm</label>
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
                  </div>
                </AccordionTab>
              </Accordion>
            </AccordionTab>
            <!-- 6. Lịch sử Đảng viên -->
            <AccordionTab>
              <template #header>
                <Toolbar class="w-full custoolbar p-0 font-bold">
                  <template #start>
                    <!-- <i class="pi pi-replay mr-2"></i> -->
                    <span>6. Lịch sử Đảng viên</span></template
                  >
                  <template #end>
                    <i
                      class="pi pi-plus-circle text-2xl"
                      v-tooltip.top="'Thêm thông tin'"
                    ></i>
                  </template>
                </Toolbar>
              </template>
              <Accordion class="accordion-custom w-full" activeIndex="0">
                <AccordionTab>
                  <template #header>
                    <span class="font-medium">Thông tin 1</span>
                  </template>
                  <div class="field col-12 md:col-12">
                    <label class="col-2 text-left">Số thẻ</label>
                    <InputText
                      spellcheck="false"
                      class="col-4 ip33"
                      v-model="profile.profile_clan_history.card_number"
                    />
                    <label class="col-2 text-left p-0 pl-5">Hình thức</label>
                    <Dropdown
                      :showClear="true"
                      :options="hinhthucs"
                      optionLabel="text"
                      optionValue="value"
                      placeholder="Chọn hình thức"
                      class="p-dropdown-sm col-4 p-0"
                      v-model="profile.profile_clan_history.form"
                    />
                  </div>
                  <div class="field col-12 md:col-12">
                    <label class="col-2 text-left">Từ ngày</label>
                    <Calendar
                      class="col-4 ip33"
                      id="icon"
                      v-model="profile.profile_clan_history.start_date"
                      :showIcon="true"
                    />
                    <label class="col-2 text-left p-0 pl-5">Đến ngày</label>
                    <Calendar
                      class="col-4 ip33"
                      id="icon"
                      v-model="profile.profile_clan_history.end_date"
                      :showIcon="true"
                    />
                  </div>
                  <div class="field col-12 md:col-12">
                    <label class="col-2 text-left">Nơi kết nạp</label>
                    <InputText
                      spellcheck="false"
                      class="col-4 ip33"
                      v-model="profile.profile_clan_history.admission_place"
                    />
                    <label class="col-2 text-left p-0 pl-5"
                      >Nơi điều chuyển</label
                    >
                    <InputText
                      spellcheck="false"
                      class="col-4 ip33"
                      v-model="profile.profile_clan_history.transfer_place"
                    />
                  </div>
                </AccordionTab>
              </Accordion>
            </AccordionTab>
            <!-- 7. Lịch sử tham gia quân đội -->
            <AccordionTab>
              <template #header>
                <!-- <i class="pi pi-chart-line mr-2"></i> -->
                <span>7. Lịch sử tham gia quân đội</span>
              </template>
              <div class="field col-12 md:col-12">
                <label class="col-2 text-left">Ngày nhập ngũ</label>
                <Calendar
                  class="col-4 ip33"
                  id="icon"
                  v-model="profile.military_start_date"
                  :showIcon="true"
                />
                <label class="col-2 text-left p-0 pl-5">Ngày xuất ngũ</label>
                <Calendar
                  class="col-4 ip33"
                  id="icon"
                  v-model="profile.military_end_date"
                  :showIcon="true"
                />
              </div>
              <div class="field col-12 md:col-12">
                <label class="col-2 text-left">Quân hàm cao nhất</label>
                <InputText
                  spellcheck="false"
                  class="col-4 ip33"
                  v-model="profile.military_rank"
                />
                <label class="col-2 text-left p-0 pl-5"
                  >Danh hiệu cao nhất</label
                >
                <InputText
                  spellcheck="false"
                  class="col-4 ip33"
                  v-model="profile.military_title"
                />
              </div>
              <div class="field col-12 md:col-12">
                <label class="col-2 text-left">Sở trường công tác</label>
                <InputText
                  spellcheck="false"
                  class="col-4 ip33"
                  v-model="profile.military_forte"
                />
                <label class="col-2 text-left p-0 pl-5">Sức khỏe</label>
                <InputText
                  spellcheck="false"
                  class="col-4 ip33"
                  v-model="profile.military_health"
                />
              </div>
              <div class="field col-12 md:col-12">
                <label class="col-2 text-left">Khen thưởng</label>
                <InputText
                  spellcheck="false"
                  class="col-4 ip33"
                  v-model="profile.military_reward"
                />
                <label class="col-2 text-left p-0 pl-5">Kỷ luật</label>
                <InputText
                  spellcheck="false"
                  class="col-4 ip33"
                  v-model="profile.military_discipline"
                />
              </div>
              <div class="field col-12 md:col-12">
                <label class="col-2 text-left">Thương binh hạng</label>
                <InputText
                  spellcheck="false"
                  class="col-4 ip33"
                  v-model="profile.military_veterans_rank"
                />
                <label class="col-2 text-left p-0 pl-5"
                  >Con gia đình chính sách</label
                >
                <InputText
                  spellcheck="false"
                  class="col-4 ip33"
                  v-model="profile.military_policy_family"
                />
              </div>
            </AccordionTab>
            <!-- 8. Kinh nghiệm làm việc -->
            <AccordionTab>
              <template #header>
                <Toolbar class="w-full custoolbar p-0 font-bold">
                  <template #start>
                    <span>8. Kinh nghiệm làm việc</span></template
                  >
                  <template #end>
                    <i
                      class="pi pi-plus-circle text-2xl"
                      v-tooltip.top="'Thêm thông tin'"
                    ></i>
                  </template>
                </Toolbar>
              </template>
              <Accordion class="accordion-custom w-full" activeIndex="0">
                <AccordionTab>
                  <template #header>
                    <span class="font-medium">Thông tin 1</span>
                  </template>
                  <div class="field col-12 md:col-12">
                    <label class="col-2 text-left">Từ tháng, năm</label>
                    <Calendar
                      v-model="profile.profile_experience.start_date"
                      view="month"
                      dateFormat="mm/yy"
                      class="col-4 p-0 ip33"
                      placeholder="Bắt đầu"
                    />
                    <label class="col-2 text-left p-0 pl-5"
                      >Đến tháng, năm</label
                    >
                    <Calendar
                      v-model="profile.profile_experience.end_date"
                      view="month"
                      dateFormat="mm/yy"
                      class="col-4 p-0 ip33"
                      placeholder="Kết thúc"
                    />
                  </div>
                  <div class="field col-12 md:col-12">
                    <label class="col-2 text-left">Công ty, đơn vị</label>
                    <InputText
                      spellcheck="false"
                      class="col-4 ip33"
                      v-model="profile.profile_experience.company"
                    />
                    <label class="col-2 text-left p-0 pl-5">Vị trí</label>
                    <InputText
                      spellcheck="false"
                      class="col-4 ip33"
                      v-model="profile.profile_experience.role"
                    />
                  </div>
                  <div class="field col-12 md:col-12">
                    <label class="col-2 text-left">Người tham chiếu</label>
                    <InputText
                      spellcheck="false"
                      class="col-4 ip33"
                      v-model="profile.profile_experience.reference_name"
                    />
                    <label class="col-2 text-left p-0 pl-5">Điện thoại</label>
                    <InputText
                      spellcheck="false"
                      class="col-4 ip33"
                      v-model="profile.profile_experience.reference_phone"
                    />
                  </div>
                  <div class="field col-12 md:col-12 flex">
                    <label class="col-2 text-left">Mô tả công việc</label>
                    <Textarea
                      :autoResize="true"
                      rows="4"
                      class="col-10"
                      v-model="profile.profile_experience.description"
                    />
                  </div>
                </AccordionTab>
              </Accordion>
            </AccordionTab>
            <!-- Đặc điểm lịch sử bản thân -->
            <AccordionTab>
              <template #header>
                <!-- <i class="pi pi-chart-line mr-2"></i> -->
                <span>9. Đặc điểm lịch sử bản thân</span>
              </template>
              <div class="field col-12 md:col-12 flex">
                <label class="col-2 text-left">Nhập thông tin</label>
                <Textarea
                  :autoResize="true"
                  rows="4"
                  class="col-10"
                  placeholder="Khai rõ: Bị bắt, bị tù, bản thân có làm việc trong chế độ cũ"
                  v-model="profile.biography_first"
                />
              </div>
              <div class="field col-12 md:col-12 flex">
                <label class="col-2 text-left">Nhập thông tin</label>
                <Textarea
                  :autoResize="true"
                  rows="4"
                  class="col-10"
                  placeholder="Tham gia hoặc có quan hệ với các tổ chức chính trị, kinh tế, xã hội nào ở nước ngoài"
                  v-model="profile.biography_second"
                />
              </div>
              <div class="field col-12 md:col-12 flex">
                <label class="col-2 text-left">Nhập thông tin</label>
                <Textarea
                  :autoResize="true"
                  rows="4"
                  class="col-10"
                  placeholder="Có thân nhân ở nước ngoài (làm gì, địa chỉ)"
                  v-model="profile.biography_third"
                />
              </div>
            </AccordionTab>
            <!-- 10.	Đính kèm khác (file số hóa liên quan) -->
            <AccordionTab>
              <template #header>
                <!-- <i class="pi pi-chart-line mr-2"></i> -->
                <span> 10. Đính kèm khác (file số hóa liên quan)</span>
              </template>
              <div class="field col-12 md:col-12 flex">
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
              </div>
            </AccordionTab>
          </Accordion>
        </div>
        <div class="field col-12 md:col-12 flex">
          <label class="col-1 text-left font-bold">Ghi chú</label>
          <Textarea
            :autoResize="true"
            rows="4"
            class="col-11"
            v-model="profile.note"
          />
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
        @click="saveProfile(!v$.$invalid)"
      />
    </template>
  </Dialog>
</template>
<style scoped>
.p-avatar {
  font-size: 1.5rem !important;
}
.p-calendar-w-btn {
  padding: 0px !important;
}
.ip33 {
  height: 33px;
}
.ipnone {
  display: none;
}
.avatar{
  width:56px;
  height:56px;
  border-radius:5px;
  object-fit:cover;
}
.inputanh {
  /* border: 1px solid #ccc; */
  width: 140px;
  height: 140px;
  cursor: pointer;
  padding: 1px;
}

.inputanh img {
  object-fit: cover;
  width: 100%;
  height: 100%;
}
table td{
  padding: 5px;
}
.d-lang-table {
  height: calc(100vh - 155px);
}
::-webkit-input-placeholder {
  font-style: italic;
}
:-moz-placeholder {
  font-style: italic;
}
::-moz-placeholder {
  font-style: italic;
}
:-ms-input-placeholder {
  font-style: italic;
}
</style>
<style lang="scss" scoped>
::v-deep(.col-12) {
  .p-inputswitch {
    top: 6px;
  }
}
::v-deep(.p-tabview-nav-container) {
  .p-tabview-nav {
    border-top: 1px solid #dee2e6;
  }
}
::v-deep(.p-tabview p-component) {
  .p-tabview-panels {
    padding: 0.5rem !important;
  }
}
</style>