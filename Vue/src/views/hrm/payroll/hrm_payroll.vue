<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import tree_users_hrm from "../component/tree_users_hrm.vue";
import DropdownProfile from "../component/DropdownProfile.vue";
import DropdownUser from "../component/DropdownProfiles.vue";
import { encr, checkURL } from "../../../util/function.js";
import DocComponent from "../template/components/DocComponent.vue";
import moment from "moment";

const getProfileUsers = (user, obj) => {
  if (user == "profile_id_fake") {
    payroll.value[user] = [];
    obj.forEach((element) => {
      payroll.value[user].push(element.profile_id);
    });
  } else {
    options.value.list_profile_id = [];
    obj.forEach((element) => {
      options.value.list_profile_id.push(element.profile_id);
    });
  }
};
//Khai báo
const router = inject("router");
const emitter = inject("emitter");
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
  payroll_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
const rules = {
  payroll_name: {
    required,
    $errors: [
      {
        $property: "payroll_name",
        $validator: "required",
        $message: "Tên bảng lương không được để trống!",
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
//Lấy số bản ghi
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_payroll_count",
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
//Lấy dữ liệu payroll
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
              proc: "hrm_payroll_list",
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

          if (element.report_key) {
            var arr = listTypeContractSave.value.find(
              (x) => x.report_key == element.report_key
            );
            if (arr) element.report_key_name = arr.report_name;
          }
          if (element.listUsers) {
            element.listUsers = JSON.parse(element.listUsers);

            element.listUsers.forEach((item) => {
              if (!item.position_name) {
                item.position_name = "";
              } else {
                item.position_name =
                  " </br> <span class='text-sm'>" +
                  item.position_name +
                  "</span>";
              }
              if (!item.department_name) {
                item.department_name = "";
              } else {
                item.department_name =
                  " </br> <span class='text-sm'>" +
                  item.department_name +
                  "</span>";
              }
            });
          } else element.listUsers = [];

          if (!element.position_name) {
            element.position_name = "";
          } else {
            element.position_name =
              " </br> <span class='text-sm'>" +
              element.position_name +
              "</span>";
          }
          if (!element.department_name) {
            element.department_name = "";
          } else {
            element.department_name =
              " </br> <span class='text-sm'>" +
              element.department_name +
              "</span>";
          }
        });
        datalists.value = data;

        options.value.loading = false;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;
        console.log("0e", error);
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

    options.value.id = datalists.value[datalists.value.length - 1].payroll_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].payroll_id;
    options.value.IsNext = false;
  }
  options.value.PageNo = event.page;
  loadData(true);
};

const payroll = ref({
  payroll_name: "",
  emote_file: "",
  status: true,
  is_order: 1,
  profile_id_fake: [],
});

const selectedStamps = ref();
const submitted = ref(false);
const v$ = useVuelidate(rules, payroll);
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
const listTypeContract = ref([]);
const openBasic = (str) => {
  submitted.value = false;
  payroll.value = {
    payroll_name: "",
    emote_file: "",
    status: true,
    is_order: sttStamp.value,
    organization_id: store.getters.user.organization_id,
    is_system: store.getters.user.is_super ? true : false,
    profile_id_fake: [],
  };
  listFilesS.value = [];
  checkIsmain.value = false;
  isSaveTem.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};

const openBasicWRP = (id) => {
  submitted.value = false;
  payroll.value = {
    payroll_name: "",
    emote_file: "",
    status: true,
    is_order: sttStamp.value,
    organization_id: store.getters.user.organization_id,
    is_system: store.getters.user.is_super ? true : false,
    declare_paycheck_id: id,
  };
  listFilesS.value = [];
  checkIsmain.value = false;
  isSaveTem.value = false;
  headerDialog.value = "Thêm mới bảng lương";
  displayBasic.value = true;
};

const closeDialog = () => {
  payroll.value = {
    payroll_name: "",
    emote_file: "",
    status: true,
    is_order: 1,
  };

  displayBasic.value = false;
  loadData(true);
};

//Thêm bản ghi

const sttStamp = ref(1);
const saveData = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  if (
    payroll.value.profile_id_fake == null ||
    payroll.value.payroll_name == null ||
    payroll.value.declare_paycheck_id == null
  ) {
    return;
  }

  if (payroll.value.profile_id_fake) {
    payroll.value.list_profile_id = payroll.value.profile_id_fake.toString();
  }
  if (payroll.value.payroll_month_fake) {
    payroll.value.payroll_month =
      payroll.value.payroll_month_fake.getMonth() + 1;
  }
  if (payroll.value.payroll_year_fake) {
    payroll.value.payroll_year = payroll.value.payroll_year_fake.getFullYear();
  }
  if (payroll.value.profile_id_fake) {
    payroll.value.list_profile_id = payroll.value.profile_id_fake.toString();
  }
  if (payroll.value.payroll_name.length > 250) {
    swal.fire({
      title: "Error!",
      text: "Tên bảng lương không được vượt quá 250 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  let formData = new FormData();

  formData.append("hrm_files", JSON.stringify(listFilesS.value));
  formData.append("hrm_payroll", JSON.stringify(payroll.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (!isSaveTem.value) {
    axios
      .post(baseURL + "/api/hrm_payroll/add_hrm_payroll", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm bảng lương thành công!");

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
      .put(baseURL + "/api/hrm_payroll/update_hrm_payroll", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa bảng lương thành công!");

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

const viewTem = (data) => {
  let o = {
    id: data.report_key,
    par: { profile_id: data.li_profile_id },
  };

  let url = encodeURIComponent(
    encr(JSON.stringify(o), SecretKey, cryoptojs).toString()
  );
  url =
    "/hrm/payroll/hrm_payroll/details/" +
    url.replaceAll("%", "==") +
    "?v=" +
    new Date().getTime().toString();

  if (router)
    router.push({
      path: url,
    });
};
const visibleSidebarDoc = ref(false);
const report = ref({ datadic: null });
const configPayroll = async (row) => {
  let strSQL = {
    query: false,
    proc: "payroll_config",
    par: [
      {
        par: "payroll_id",
        va: row.payroll_id,
      },
      {
        par: "report_key",
        va: row.report_key,
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
    baseURL + "/api/HRM_SQL/getData",
    {
      str: encr(JSON.stringify(strSQL), SecretKey, cryoptojs).toString(),
    },
    {
      headers: { Authorization: `Bearer ${store.getters.token}` },
    }
  );

  if (axResponse.status == 200) {
    if (axResponse.data.error) {
      toast.error("Không mở được bản ghi");
    } else {
      let dt = JSON.parse(axResponse.data.data);

      payroll.value = dt[2][0];
      report.value = dt[1][0];

      report.value.datadic = [{ title: "Bảng lương", data: dt[0][0] }];
      report.value.proc_name = `payroll_profile_list '${store.getters.user.user_id}', '${row.payroll_id}'`;
      report.value.proc_all = `payroll_profile_list_all '${store.getters.user.user_id}', '${row.payroll_id}'`;
      let cg = {};
      if (
        report.value.report_config &&
        report.value.report_config.trim() != ""
      ) {
        cg = JSON.parse(report.value.report_config);
      }
      cg.proc = {
        name: "payroll_user_get",
        parameters: [
          {
            Parameter_name: "@payroll_user_id",
            Type: "varchar",
            Length: 50,
            Param_order: 1,
          },
        ],
        sql: report.value.proc_name,
        data: JSON.stringify(cg.data),
        issql: true,
      };
      report.value.report_config = JSON.stringify(cg);
      if (payroll.value.payroll_config)
        report.value.is_config = JSON.parse(payroll.value.payroll_config);

      visibleSidebarDoc.value = true;
    }
  }
  swal.close();
};
const reloadDocComponent = () => {};
const callbackFun = (obj, check) => {
  if (check == false) {
    if (obj.is_config) {
      payroll.value.payroll_config = obj.is_config;

      saveDGLuong();

      return false;
    }
  } else if (check == true) {
    obj.forEach((r) => {
      var arrck = null;

      if (r.is_data) arrck = r.is_data[0][report.value.sum_key];
      if (!arrck) arrck = null;
      else arrck = Number(arrck);
      r.payroll_id = payroll.value.payroll_id;
      r.salary=arrck;
      r.is_data = JSON.stringify(r.is_data);
    })
      
    let formData = new FormData();
    formData.append("hrm_payroll_user", JSON.stringify(obj));

    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });

    axios
      .post(
        baseURL + "/api/hrm_payroll_user/add_li_hrm_payroll_user",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          console.log("okkke");
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

  // saveDGLuongUser(obj);
};
// const saveDGLuongUser = async (r) => {
//   let strSQL = {
//     query: false,
//     proc: "hrm_payroll_user_addd ",
//     par: [
//       { par: "payroll_user_id", va: r.payroll_user_id },
//       { par: "payroll_id", va: r.payroll_id },
//       { par: "profile_id", va: r.profile_id },
//       { par: "is_data", va: JSON.stringify(r.is_data) },
//       { par: "user_id", va: store.getters.user.user_id },
//       { par: "ip", va: store.getters.ip },
//       { par: "organization_id", va: store.getters.user.organization_id },
//       { par: "salary", va: arrck },
//     ],
//   };
//   // console.log("error",strSQL);
//   try {
//     const axResponse = await axios.post(
//       baseURL + "/api/HRM_SQL/getData",
//       {
//         str: encr(JSON.stringify(strSQL), SecretKey, cryoptojs).toString(),
//       },
//       {
//         headers: { Authorization: `Bearer ${store.getters.token}` },
//       }
//     );
//     // console.log("error",axResponse);
//   } catch (e) {
//     console.log(e);
//   }
// };
const goProfile = (profile) => {
  router.push({
    name: "profileinfo",
    params: { id: profile.profile_code },
    query: { id: profile.profile_id },
  });
};
const saveDGLuong = async () => {
  let ok = true;

  if (!payroll.value.report_key) {
    toast.warning("Vui lòng chọn mẫu bảng lương.");

    ok = false;
  }
  if (!payroll.value.payroll_name) {
    toast.warning("Vui lòng nhập tên bảng lương.");

    ok = false;
  }
  if (!payroll.value.sign_user) {
    toast.warning("Vui lòng nhập tên người ký bảng lương.");

    ok = false;
  }
  if (ok) {
    options.value.loading = true;
    let strSQL = {
      query: false,
      proc: "hrm_payroll_add",
      par: [
        { par: "payroll_id", va: payroll.value.payroll_id },
        { par: "payroll_month", va: payroll.value.payroll_month },
        { par: "payroll_year", va: payroll.value.payroll_year },
        { par: "payroll_name", va: payroll.value.payroll_name },
        { par: "payroll_config", va: payroll.value.payroll_config },
        { par: "list_profile_id", va: payroll.value.list_profile_id || null },
        { par: "sign_date", va: payroll.value.sign_date },
        { par: "sign_user", va: payroll.value.sign_user },
        { par: "profile_sign_id", va: payroll.value.profile_sign_id },
        { par: "declare_paycheck_id", va: payroll.value.declare_paycheck_id },

        { par: "report_key", va: payroll.value.report_key },
        { par: "status ", va: payroll.value.status },
        { par: "user_id", va: store.getters.user.user_id },
        { par: "ip", va: store.getters.ip },
        { par: "organization_id", va: store.getters.user.organization_id },
      ],
    };
    console.log(strSQL);
    try {
      const axResponse = await axios.post(
        baseURL + "/api/HRM_SQL/PostProc",
        {
          str: encr(JSON.stringify(strSQL), SecretKey, cryoptojs).toString(),
        },
        {
          headers: { Authorization: `Bearer ${store.getters.token}` },
        }
      );

      if (axResponse.status == 200) {
        displayBasic.value = false;
        // listsalary();
      } else {
        toast.error("Có lỗi xảy ra, vui lòng thử lại!");
      }
      options.value.loading = false;
    } catch (e) {
      console.log(e);
      options.value.loading = false;
      toast.error("Có lỗi xảy ra, vui lòng thử lại!");
    }
  }
};
//Sửa bản ghi
const editTem = (dataTem) => {
  submitted.value = false;
  payroll.value = dataTem;

  if (payroll.value.listUsers) {
    payroll.value.profile_id_fake = [];
    payroll.value.listUsers.forEach((element) => {
      payroll.value.profile_id_fake.push(element.profile_id);
    });
  }

  if (payroll.value.payroll_month)
    payroll.value.payroll_month_fake = new Date(
      payroll.value.payroll_month + "/01" + "/2023"
    );
  if (payroll.value.payroll_year)
    payroll.value.payroll_year_fake = new Date(
      "01/01/" + payroll.value.payroll_year
    );
  if (payroll.value.sign_date)
    payroll.value.sign_date = new Date(payroll.value.sign_date);
  headerDialog.value = "Sửa bảng lương";
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
          .delete(baseURL + "/api/hrm_payroll/delete_hrm_payroll", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Tem != null ? [Tem.payroll_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá bảng lương thành công!");
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
    id: "payroll_id",
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
    .post(baseURL + "/api/hrm_SQL/Filter_hrm_payroll", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.STT = options.value.PageNo * options.value.PageSize + i + 1;

          if (element.report_key) {
            element.report_key_name = listTypeContractSave.value.find(
              (x) => x.report_key == element.report_key
            ).report_name;
          }
          if (element.listUsers) {
            element.listUsers = JSON.parse(element.listUsers);

            element.listUsers.forEach((item) => {
              if (!item.position_name) {
                item.position_name = "";
              } else {
                item.position_name =
                  " </br> <span class='text-sm'>" +
                  item.position_name +
                  "</span>";
              }
              if (!item.department_name) {
                item.department_name = "";
              } else {
                item.department_name =
                  " </br> <span class='text-sm'>" +
                  item.department_name +
                  "</span>";
              }
            });
          } else element.listUsers = [];

          if (!element.position_name) {
            element.position_name = "";
          } else {
            element.position_name =
              " </br> <span class='text-sm'>" +
              element.position_name +
              "</span>";
          }
          if (!element.department_name) {
            element.department_name = "";
          } else {
            element.department_name =
              " </br> <span class='text-sm'>" +
              element.department_name +
              "</span>";
          }
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
const onCheckBox = (value, check) => {
  if (check) {
    let data = {
      IntID: value.payroll_id,
      TextID: value.payroll_id + "",
      IntTrangthai: 1,
      BitTrangthai: value.status,
    };
    axios
      .put(baseURL + "/api/hrm_payroll/update_s_hrm_payroll", data, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa trạng thái bảng lương thành công!");

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

  if (!checkD) {
    swal
      .fire({
        title: "Thông báo",
        text: "Bạn có muốn xoá bảng lương này không!",
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
            listId.push(item.payroll_id);
          });
          axios
            .delete(baseURL + "/api/hrm_payroll/delete_hrm_payroll", {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: listId != null ? listId : 1,
            })
            .then((response) => {
              swal.close();
              if (response.data.err != "1") {
                swal.close();
                toast.success("Xoá bảng lương thành công!");
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
  options.value.list_profile_id = [];
  options.value.SearchText = null;
  op.value.hide();
  loadData(true);
};
const filterFileds = () => {
  filterSQL.value = [];
  checkFilter.value = true;

  if (filterTrangthai.value) {
    let filterS = {
      filterconstraints: [
        { value: filterTrangthai.value, matchMode: "equals" },
      ],
      filteroperator: "and",
      key: "status",
    };
    filterSQL.value.push(filterS);
  }

  if (options.value.list_profile_id.length > 0) {
    let filterS1 = {
      filterconstraints: [
        {
          value: options.value.list_profile_id.toString(),
          matchMode: "arrIntersec",
        },
      ],
      filteroperator: "or",
      key: "list_profile_id",
    };

    filterSQL.value.push(filterS1);
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

const listTypeContractSave = ref([]);
const listDeclarePaycheck = ref([]);

const initTuDien = () => {
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_declare_paycheck_list",
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
        listDeclarePaycheck.value.push({
          name: element.declare_paycheck_name,
          code: element.declare_paycheck_id,
        });
      });

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

  listTypeContract.value = [];
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "smartreport_list ",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
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
      var arrGroups = [];
      data.forEach((element) => {
        var strchk = arrGroups.find((x) => x == element.report_group);
        if (strchk == null) {
          arrGroups.push(element.report_group);
        }
      });

      listTypeContractSave.value = [...data];
      arrGroups.forEach((item) => {
        var ardf = {
          label: item,
          items: [],
        };
        data
          .filter((x) => x.report_group == item)
          .forEach((z) => {
            ardf.items.push({ label: z.report_name, value: z.report_key });
          });
        listTypeContract.value.push(ardf);
      });
      loadData(true);
      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
    });
};
const listFilesS = ref([]);

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
  payroll.value.profile_id_fake = [];
  if (checkMultile.value == false)
    selectedUser.value.forEach((element) => {
      payroll.value.profile_id_fake.push(element.profile_id);
    });
  closeDialogUser();
};
const getProfileUser = (user, obj) => {
  if (obj) {
    payroll.value[user] = obj;
  } else {
    payroll.value.sign_user = null;
  }
};
emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "submitDropdownUser":
      if (obj.data) {
        payroll.value.sign_user = obj.data.profile_id;
      } else {
        payroll.value.sign_user = null;
      }
      break;
    case "submitModel":
      if (obj.data) {
        if (obj.data.type == 1) {
          payroll.value.profile_id_fake = [];
          obj.data.data.forEach((element) => {
            payroll.value.profile_id_fake.push(element.profile_id);
          });
        } else {
          options.value.list_profile_id = [];
          obj.data.data.forEach((element) => {
            options.value.list_profile_id.push(element.profile_id);
          });
        }
      }
      break;
    default:
      break;
  }
});
const onChangeUsersReceive = (declare_paycheck_id) => {
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_user_de_paycheck_get",
            par: [{ par: "declare_paycheck_id", va: declare_paycheck_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      payroll.value.profile_id_fake = [];
      data.forEach((element, i) => {
        payroll.value.profile_id_fake.push(element.profile_id);
      });
    })
    .catch((error) => {
      console.log(error);

      store.commit("gologout");
    });
};
const onRowClckTable = (data) => {
  selectedStamps.value = [];

  selectedStamps.value.push(data.data);
};

onMounted(() => {
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
      :filters="filters"
      :scrollable="true"
      filterDisplay="menu"
      filterMode="lenient"
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
      :row-hover="true"
      dataKey="payroll_id"
      responsiveLayout="scroll"
      v-model:selection="selectedStamps"
      rowGroupMode="subheader"
      groupRowsBy="declare_paycheck_name"
      @row-click="onRowClckTable"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-book"></i> Danh sách bảng lương ({{
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
                style="width: 400px"
              >
                <div class="grid formgrid m-0">
                  <div class="col-12 md:col-12">
                    <div class="py-2">Nhân sự nhận bảng lương</div>
                    <DropdownUser
                      :model="options.list_profile_id"
                      :display="'chip'"
                      :placeholder="'Chọn nhân sự'"
                      :type="2"
                      :callbackFun="getProfileUsers"
                      :key_user="'list_profile_id'"
                    />
                  </div>
                  <div class="field col-12 p-0">
                    <div class="col-12 text-left py-2" style="text-align: left">
                      Trạng thái
                    </div>
                    <div class="col-12">
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
              @click="openBasic('Thêm bảng lương')"
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
        <span class="ml-2 font-bold text-blue-500"
          >{{ slotProps.data.declare_paycheck_name }} ({{
            slotProps.data.totalRecordGroups
          }})</span
        >
        <Button
          style="padding: 5px"
          @click="openBasicWRP(slotProps.data.declare_paycheck_id)"
          icon="pi pi-plus-circle"
          class="ml-1 p-button-text p-button-rounded"
        />
      </template>

      <Column
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:70px;height:50px"
        bodyStyle="text-align:center;max-width:70px"
        selectionMode="multiple"
        v-if="store.getters.user.is_super == true"
      >
      </Column>

      <Column
        field="STT"
        header="STT"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:70px;height:50px"
        bodyStyle="text-align:center;max-width:70px"
      ></Column>

      <Column
        field="payroll_name"
        header="Tên bảng lương"
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
        header="Tháng"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px;overflow:hidden"
        class="align-items-center justify-content-center text-center overflow-hidden"
      >
        <template #body="slotProps">
          <div>
            {{
              moment(
                new Date(
                  slotProps.data.payroll_year,
                  slotProps.data.payroll_month - 1,
                  1
                )
              ).format("MM/YYYY")
            }}
          </div>
        </template>
      </Column>

      <Column
        header="Tổng lương (VNĐ)"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px;overflow:hidden"
        class="align-items-center justify-content-center text-center overflow-hidden"
      >
        <template #body="slotProps">
          <div v-if="slotProps.data.sum_salary">
            {{ slotProps.data.sum_salary.toLocaleString() }}
          </div>
        </template>
      </Column>
      <Column
        field="vacancy_name"
        header="Nhân sự nhận"
        headerStyle="text-align:center;max-width:250px;height:50px"
        bodyStyle="text-align:center;max-width:250px;overflow:hidden"
        class="align-items-center justify-content-center text-center overflow-hidden"
      >
        <template #body="data">
          <div>
            <AvatarGroup>
              <Avatar
                v-for="(item, index) in data.data.listUsers.slice(0, 4)"
                v-bind:label="
                  item.avatar
                    ? ''
                    : item.full_name.substring(
                        item.full_name.lastIndexOf(' ') + 1,
                        item.full_name.lastIndexOf(' ') + 2
                      )
                "
                style="
                  background-color: #2196f3;
                  color: #fff;
                  width: 3rem;
                  height: 3rem;
                  font-size: 1rem !important;
                "
                :key="index"
                :style="
                  item.avatar
                    ? 'background-color: #2196f3'
                    : 'background:' + bgColor[item.full_name.length % 7]
                "
                :image="basedomainURL + item.avatar"
                class="text-avatar cursor-pointer"
                size="xlarge"
                shape="circle"
                v-tooltip.top="{
                  value:
                    item.full_name + item.position_name + item.department_name,
                  escape: true,
                }"
                @click="goProfile(item)"
              />
              <Avatar
                v-if="data.data.listUsers.length > 4"
                :label="(data.data.listUsers.length - 4).toString()"
                shape="circle"
                style="
                  background-color: #2196f3;
                  color: #fff;
                  width: 3rem;
                  height: 3rem;
                  font-size: 1rem !important;
                "
              />
            </AvatarGroup>
          </div>
        </template>
      </Column>
      <Column
        field="sign_user"
        header="Người ký"
        headerStyle="text-align:center;max-width:250px;height:50px"
        bodyStyle="text-align:center;max-width:250px;overflow:hidden"
        class="align-items-center justify-content-center text-center overflow-hidden"
      >
      </Column>
      <Column
        header="Ngày ký"
        headerStyle="text-align:center;max-width:120px;height:50px"
        bodyStyle="text-align:center;max-width:120px;overflow:hidden"
        class="align-items-center justify-content-center text-center overflow-hidden"
      >
        <template #body="slotProps">
          <div v-if="slotProps.data.sign_date">
            {{ moment(slotProps.data.sign_date).format("DD/MM/YYYY") }}
          </div>
        </template>
      </Column>
      <!-- <Column
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
      ></Column> -->

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
              store.state.user.is_admin
            "
          >
            <!-- <Button
              @click="viewTem(Tem.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-eye"
              v-tooltip.top="'Xem'"
            ></Button> -->
            <Button
              @click="configPayroll(Tem.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-cog"
              v-tooltip.top="'Cấu hình bảng lương'"
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
  </div>
  <Sidebar
    v-model:visible="visibleSidebarDoc"
    position="full"
    class="d-sidebar-full"
    @hide="loadData(true)"
  >
    <template #header>
      <h2 class="p-0 m-0">
        <i class="pi pi-cog mr-2"></i>{{ payroll.payroll_name }}
      </h2>
    </template>
    <div style="padding: 0 20px">
      <DocComponent
        :isedit="true"
        :report="report"
        :callbackFun="callbackFun"
        :readonly="true"
        :reload="reloadDocComponent"
      ></DocComponent>
    </div>
  </Sidebar>
  <tree_users_hrm
    v-if="displayDialogUser === true"
    :headerDialog="'Chọn nhân sự'"
    :displayDialog="displayDialogUser"
    :closeDialog="closeDialogUser"
    :one="checkMultile"
    :selected="selectedUser"
    :choiceUser="choiceUser"
  />
  <Dialog
    :header="headerDialog"
    v-model:visible="displayBasic"
    :style="{ width: '40vw' }"
    :closable="true"
    :modal="true"
  >
    <form>
      <div class="grid formgrid m-2 my-0">
        <div class="field col-12 md:col-12">
          <div class="col-3 text-left p-0 pb-2">
            Mẫu phiếu lương <span class="redsao">(*)</span>
          </div>
          <Dropdown
            v-model="payroll.declare_paycheck_id"
            :options="listDeclarePaycheck"
            optionLabel="name"
            optionValue="code"
            class="col-12 ip36 px-2"
            placeholder="Chọn mẫu phiếu lương"
            panelClass="d-design-dropdown"
            :filter="true"
            :class="{
              'p-invalid': payroll.declare_paycheck_id == null && submitted,
            }"
            @change="onChangeUsersReceive(payroll.declare_paycheck_id)"
          />
        </div>
        <div
          v-if="
            (v$.payroll_name.$invalid && submitted) ||
            v$.payroll_name.$pending.$response
          "
          style="display: flex"
          class="field col-12 md:col-12 p-0"
        >
          <small class="col-12 p-error">
            Mẫu bảng lương không được để trống!
          </small>
        </div>
        <div class="col-12 field md:col-12 flex align-items-center">
          <div class="col-6 md:col-6 p-0 align-items-center">
            <div class="col-12 text-left p-0 pb-2">Tháng</div>

            <div class="col-12 p-0">
              <Calendar
                v-model="payroll.payroll_month_fake"
                view="month"
                dateFormat="mm"
                class="w-full"
                panelClass="d-calendar-design-m"
                :showIcon="true"
              >
              </Calendar>
            </div>
          </div>
          <div class="col-6 md:col-6 p-0 align-items-center pl-3">
            <div class="col-12 text-left p-0 pb-2">Năm</div>
            <div class="col-12 p-0">
              <Calendar
                v-model="payroll.payroll_year_fake"
                view="year"
                dateFormat="yy"
                class="w-full"
                :showIcon="true"
              >
              </Calendar>
            </div>
          </div>
        </div>
        <div class="field col-12 md:col-12">
          <div class="col-12 text-left p-0 pb-2">
            Tên bảng lương <span class="redsao">(*)</span>
          </div>
          <InputText
            v-model="payroll.payroll_name"
            spellcheck="false"
            class="col-12 ip36 px-2"
            :class="{
              'p-invalid': v$.payroll_name.$invalid && submitted,
            }"
          />
        </div>
        <div
          v-if="
            (v$.payroll_name.$invalid && submitted) ||
            v$.payroll_name.$pending.$response
          "
          style="display: flex"
          class="field col-12 md:col-12 p-0"
        >
          <small class="col-9 p-error">
            <span class="col-12 p-0">{{
              v$.payroll_name.required.$message
                .replace("Value", "Tên bảng lương")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
        <div class="col-12 field md:col-12 flex align-items-center">
          <div class="col-6 md:col-6 p-0 align-items-center">
            <div class="col-12 text-left p-0 pb-2">Ngày ký</div>

            <div class="col-12 p-0">
              <Calendar
                v-model="payroll.sign_date"
                class="w-full"
                :showIcon="true"
                :showOnFocus="false"
              >
              </Calendar>
            </div>
          </div>
          <div class="col-6 md:col-6 p-0 align-items-center pl-3">
            <div class="col-12 text-left p-0 pb-2">Người ký</div>
            <div class="col-12 p-0">
              <DropdownProfile
                :model="payroll.sign_user"
                :class="'w-full p-0'"
                :editable="true"
                optionLabel="profile_user_name"
                optionValue="profile_user_name"
                :callbackFun="getProfileUser"
                :key_user="'sign_user'"
              />
            </div>
          </div>
        </div>
        <div class="field flex align-items-center col-12 md:col-12">
          <div class="col-6 md:col-6 p-0 align-items-center flex">
            <div class="p-0 flex align-items-center">Duyệt</div>
            <InputSwitch
              v-model="payroll.is_approved"
              class="w-4rem lck-checked ml-3"
            />
          </div>
          <div class="col-6 md:col-6 p-0 align-items-center flex">
            <div class="p-0 flex align-items-center pl-3">Trạng thái</div>
            <InputSwitch
              v-model="payroll.status"
              class="w-4rem lck-checked ml-3"
            />
          </div>
        </div>

        <div class="field align-items-center col-12 md:col-12">
          <div class="col-12 p-0 flex align-items-center">
            <div class="text-left p-0">
              Danh sách nhân sự nhận bảng lương <span class="redsao">(*)</span>
            </div>
            <Button
              v-tooltip.top="'Chọn nhân sự'"
              @click="showTreeUser()"
              icon="pi pi-user-plus"
              class="p-button-text p-button-rounded"
            />
          </div>
          <div class="col-12 p-0">
            <DropdownUser
              :model="payroll.profile_id_fake"
              :display="'chip'"
              :placeholder="'Chọn nhân sự'"
              :class="{
                'p-invalid': payroll.profile_id_fake == null && submitted,
              }"
              :callbackFun="getProfileUsers"
              :key_user="'profile_id_fake'"
              :type="1"
            />
          </div>
        </div>
        <div
          v-if="payroll.profile_id_fake == null && submitted"
          style="display: flex"
          class="field col-12 md:col-12 p-0"
        >
          <small class="col-12 p-error">
            Danh sách nhân sự không được để trống!
          </small>
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
    