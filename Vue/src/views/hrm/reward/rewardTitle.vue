<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr } from "../../../util/function.js";
import moment from "moment";
import dialogReward from "./component/dialog_reward.vue";
import DropdownUser from "../component/DropdownUser.vue";
import router from "@/router";
//Khai báo
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
  reward_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  reward_code: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
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
            proc: "hrm_reward_count",
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
      let data1 = JSON.parse(response.data.data)[1];
      let data2 = JSON.parse(response.data.data)[2];

      if (data.length > 0) {
        options.value.totalRecords = data[0].totalRecords;
        options.value.totalRecords1 = data1[0].totalRecords;
        options.value.totalRecords2 = data2[0].totalRecords;

        sttStamp.value = data[0].totalRecords + 1;
      }
    })
    .catch((error) => {});
};

const reward = ref({
  reward_name: null,
  is_recruitment_proposal: null,
});
//Lấy dữ liệu reward
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
              proc: "hrm_reward_list",
              par: [
                { par: "pageno", va: options.value.PageNo },
                { par: "pagesize", va: options.value.PageSize },
                { par: "user_id", va: store.getters.user.user_id },
                { par: "reward_type", va: options.value.tab },
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

          if (element.listRewards) {
            element.listRewards = JSON.parse(element.listRewards);
            if (element.reward_type == 1 || element.reward_type == 3) {
              element.listRewards.forEach((item) => {
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
            }
          }
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
        console.log(error);
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;
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

    options.value.id = datalists.value[datalists.value.length - 1].reward_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].reward_id;
    options.value.IsNext = false;
  }
  options.value.PageNo = event.page;
  loadData(true);
};

const selectedStamps = ref();

const isSaveTem = ref(true);
const datalists = ref();
const toast = useToast();
const basedomainURL = baseURL;
const checkDelList = ref(false);

const options = ref({
  IsNext: true,
  sort: "reward_id desc ",
  SearchText: null,
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: 0,
  tab: 0,
  totalRecords1: 0,
  totalRecords2: 0,
  totalRecords3: 0,
  totalRecordsExport: 50,
  pagenoExport: 1,
  reward_name:[]
});

//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);
const openBasic = (str) => {
  reward.value = {
    reward_name: null,
    reward_type: 1,
    status: 1,
    reward_code: null,
    is_order: sttStamp.value,
    organization_id: store.getters.user.organization_id,

    reward_name_fake1: [],
    reward_name_fake2: {},
  };

  isSaveTem.value = true;
  headerDialog.value = str;

  displayBasic.value = true;
};

const closeDialog = () => {
  reward.value = {
    reward_name: "",
    emote_file: "",
    status: true,
    is_default: false,
    is_order: 1,
    reward_type: 1,
  };

  displayBasic.value = false;
  loadDataSQL();
};
const sttStamp = ref(1);
const listFilesS = ref([]);
//Sửa bản ghi
const editTem = (dataTem) => {
  reward.value = dataTem;

  headerDialog.value = "Sửa bản ghi";
  isSaveTem.value = false;
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
          .delete(baseURL + "/api/hrm_reward/delete_hrm_reward", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Tem != null ? [Tem.reward_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá bản ghi thành công!");
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
const checkLoadCount = ref(true);
const loadDataSQL = () => {
  datalists.value = [];


  if (options.value.tab==0) {
    let filterS1 = {
      filterconstraints: [{ value: 1, matchMode: "equals" },{ value: 2, matchMode: "equals" }],
      filteroperator: "or",
      key: "reward_type",
    };
      filterSQL.value.push(filterS1);
  }
  else{
    let filterS1 = {
      filterconstraints: [{ value: 3, matchMode: "equals" }],
      filteroperator: "and",
      key: "reward_type",
    };
      filterSQL.value.push(filterS1);

  }




  let data = {
    id: "reward_id",
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
    .post(baseURL + "/api/HRM_SQL/Filter_hrm_reward", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.STT = options.value.PageNo * options.value.PageSize + i + 1;

          if (element.listRewards) {
            element.listRewards = JSON.parse(element.listRewards);
            if (element.reward_type == 1 || element.reward_type == 3) {
              element.listRewards.forEach((item) => {
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
            }
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
          }
        });

        datalists.value = data;
      } else {
        datalists.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      options.value.loading = false;
      //Show Count nếu có
      if (dt.length >= 2 && checkLoadCount.value == true) {
        options.value.totalRecords = dt[1][0].totalRecords;
        options.value.totalRecords1 = dt[2][0].totalRecords;
        options.value.totalRecords2 = dt[3][0].totalRecords;
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

const refreshStamp = () => {
  options.value.SearchText = null;
  options.value.reward_title_id = null;
  options.value.reward_level_id = null;
  options.value.start_dateI = null;
  options.value.end_dateI = null;
  options.value.start_dateI = null;
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
const tabs = ref([
  { id: 0, title: "Khen thưởng", icon: "", total: 0 },
  { id: 1, title: "Kỷ luật", icon: "", total: 0 },
]);
//Checkbox
const onCheckBox = (value, check) => {
  if (check) {
    let data = {
      IntID: value.reward_id,
      TextID: value.reward_id + "",
      IntTrangthai: 1,
      BitTrangthai: value.status,
    };
    axios
      .put(baseURL + "/api/hrm_reward/update_s_hrm_reward", data, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa trạng thái bản ghi thành công!");
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
      IntID: value.reward_id,
      TextID: value.reward_id + "",
      BitMain: value.is_default,
    };
    axios
      .put(baseURL + "/api/hrm_reward/Update_DefaultStamp", data1, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa trạng thái bản ghi thành công!");
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

//Xuất excel

const exportExcelR = () => {
  showExport.value = false;

  exportData("ExportExcel");
};

const headerExport = ref("Cấu hình xuất Excel");
const menuButs = ref();
const showExport = ref(false);
const itemButs = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      showExport.value = true;
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
        baseURL + "/api/Excel/ExportExcelWithLogo",
        {
          excelname: options.value.tab==0?"DANH SÁCH KHEN THƯỞNG":"DANH SÁCH KỶ LUẬT",
          proc: "hrm_reward_export",
          par: [
            { par: "user_id", va: store.state.user.user_id },
            { par: "search", va: options.value.SearchText },
            {
              par: "reward_name",
              va: options.value.reward_name
                ? options.value.reward_name.toString()
                : null,
            },
            {
              par: "reward_level_id",
              va: options.value.reward_level_id
                ? options.value.reward_level_id.toString()
                : null,
            },
            {
              par: "reward_title_id",
              va: options.value.reward_title_id
                ? options.value.reward_title_id.toString()
                : null,
            },
            { par: "reward_type", va: options.value.tab  },
            { par: "start_date", va:   options.value.start_dateI},
            { par: "end_date", va:options.value.end_dateI },
        
            { par: "sort", va: options.value.sort },
            { par: "pageno", va: options.value.pagenoExport - 1 },
            { par: "pagesize", va: options.value.totalRecordsExport },
          ],
        },
        config
      )
      .then((response) => {
        swal.close();
        if (response.data.err != "1") {
          swal.close();

          toast.success("Kết xuất Data thành công!");

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

const activeTab = (tab) => {
  options.value.tab = tab.id;

  reFilter();
  if (tab.id == 0) {
    checkLoadCount.value = false;
    let filterS1 = {
      filterconstraints: [
        { value: 1, matchMode: "equals" },
        { value: 2, matchMode: "equals" },
      ],
      filteroperator: "or",
      key: "reward_type",
    };

    filterSQL.value.push(filterS1);
  } else if (tab.id == 1) {
    checkLoadCount.value = false;
    let filterS1 = {
      filterconstraints: [{ value: 3, matchMode: "equals" }],
      filteroperator: "and",
      key: "reward_type",
    };

    filterSQL.value.push(filterS1);
  }

  loadDataSQL();
};
const menuButMores = ref();
const itemButMores = ref([
  {
    label: "Hiệu chỉnh nội dung",
    icon: "pi pi-pencil",
    command: (event) => {
      editTem(reward.value, "Chỉnh sửa hợp đồng");
    },
  },
  {
    label: "Xoá",
    icon: "pi pi-trash",
    command: (event) => {
      delTem(reward.value);
    },
  },
]);
const toggleMores = (event, item) => {
  reward.value = item;
  selectedStamps.value = [];

  selectedStamps.value.push(item);
  menuButMores.value.toggle(event);
  //selectedNodes.value = item;
};
//Xóa nhiều
const deleteList = () => {
  let listId = new Array(selectedStamps.value.length);
  let checkD = false;

  if (!checkD) {
    swal
      .fire({
        title: "Thông báo",
        text: "Bạn có muốn xoá thông tin bản ghi này không!",
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
            listId.push(item.reward_id);
          });
          axios
            .delete(baseURL + "/api/hrm_reward/delete_hrm_reward", {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: listId != null ? listId : 1,
            })
            .then((response) => {
              swal.close();
              if (response.data.err != "1") {
                swal.close();
                toast.success("Xoá thông tin bản ghi thành công!");
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
const goProfile = (profile) => {
  router.push({
    name: "profileinfo",
    params: { id: profile.profile_code },
    query: { id: profile.profile_id },
  });
};
//Filter
const reFilter = () => {
  
  options.value.reward_level_id = null;
 
  options.value.start_dateI = null;
  options.value.end_dateI = null;
 
  options.value.reward_title_id = null;
 
  checkLoadCount.value = true;
  isDynamicSQL.value = false;
  checkFilter.value = false;
  filterSQL.value = [];
  options.value.SearchText = null;
};
const reFilterEmail = () => {
  reFilter();
  op.value.hide();
  loadData(true);
};
 
const filterFileds = () => {
  filterSQL.value = [];
  checkFilter.value = true;

  if (options.value.reward_name.length>0) {
    let filterS1 = {
      filterconstraints: [ { value: options.value.reward_name.toString(), matchMode: "arrIntersec" }],
      filteroperator: "or",
      key: "reward_name",
    };
     
      filterSQL.value.push(filterS1);
  
  }
  if (options.value.reward_level_id) {
    let filterS1 = {
      filterconstraints: [],
      filteroperator: "or",
      key: "reward_level_id",
    };
    if (options.value.reward_level_id.length > 0) {
      options.value.reward_level_id.forEach((element) => {
        var addr = { value: element, matchMode: "equals" };
        filterS1.filterconstraints.push(addr);
      });

      filterSQL.value.push(filterS1);
    }
  }

  if (options.value.reward_title_id) {
    let filterS2 = {
      filterconstraints: [],
      filteroperator: "or",
      key: "reward_title_id",
    };
    if (options.value.reward_title_id.length > 0) {
      options.value.reward_title_id.forEach((element) => {
        var addr = { value: element, matchMode: "equals" };
        filterS2.filterconstraints.push(addr);
      });

      filterSQL.value.push(filterS2);
    }
  }

  onDayClick();

  loadDataSQL();
  op.value.hide();
};

const onDayClick = () => {
  if (options.value.start_dateI != null) {
    if (!options.value.end_dateI)
      options.value.end_dateI = options.value.start_dateI;

    if (
      options.value.start_dateI &&
      options.value.start_dateI != options.value.end_dateI
    ) {
      let sDate = new Date(options.value.start_dateI);

      options.value.start_dateI = sDate;
      let filterS = {
        filterconstraints: [
          { value: options.value.start_dateI, matchMode: "dateAfter" },
        ],
        filteroperator: "and",
        key: "decision_date",
      };
      filterSQL.value.push(filterS);
    }
    if (
      options.value.end_dateI &&
      options.value.start_dateI != options.value.end_dateI
    ) {
      let eDate = new Date(options.value.end_dateI);

      options.value.end_dateI = eDate;
      let filterS = {
        filterconstraints: [
          { value: options.value.end_dateI, matchMode: "dateBefore" },
        ],
        filteroperator: "and",
        key: "decision_date",
      };
      filterSQL.value.push(filterS);
    }
    if (
      options.value.start_dateI &&
      options.value.start_dateI == options.value.end_dateI
    ) {
      let filterS1 = {
        filterconstraints: [
          { value: options.value.start_dateI, matchMode: "dateIs" },
        ],
        filteroperator: "and",
        key: "decision_date",
      };
      filterSQL.value.push(filterS1);
      let filterS2 = {
        filterconstraints: [
          { value: options.value.end_dateI, matchMode: "dateIs" },
        ],
        filteroperator: "and",
        key: "decision_date",
      };
      filterSQL.value.push(filterS2);
    }
  }

   
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

const listRewardLevels = ref([]);
const listDisciplineLevels = ref([]);
const listDisciplineTitles = ref([]);
const listRewardTitles = ref([]);
const initTudien = () => {
  listRewardLevels.value = [];
  listDisciplineTitles.value = [];
  listDisciplineLevels.value = [];
  listRewardTitles.value = [];
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_ca_reward_level_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 100000 },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "status", va: true },
              { par: "reward_type", va: null },
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

      data.forEach((element) => {
        if (element.reward_type == 1) {
          listRewardLevels.value.push({
            name: element.reward_level_name,
            code: element.reward_level_id,
          });
        } else if (element.reward_type == 2) {
          listDisciplineLevels.value.push({
            name: element.reward_level_name,
            code: element.reward_level_id,
          });
        }
      });
    })
    .catch((error) => {
      console.log(error);
    });

  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_ca_reward_title_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 100000 },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "status", va: true },
              { par: "reward_type", va: null },
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
        if (element.reward_type == 1) {
          listRewardTitles.value.push({
            name: element.reward_title_name,
            code: element.reward_title_id,
          });
        } else if (element.reward_type == 2) {
          listDisciplineTitles.value.push({
            name: element.reward_title_name,
            code: element.reward_title_id,
          });
        }
      });
    })
    .catch((error) => {
      console.log(error);
    });
};
const listDataUsers = ref([]);
const listDataUsersSave = ref([]);
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
          code: element.profile_id,
          avatar: element.avatar,
          department_name: element.department_name,
          department_id: element.department_id,
          work_position_name: element.work_position_name,
          position_name: element.position_name,

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
  emitter.on("emitData", (obj) => {
    switch (obj.type) {
      case "submitModel":
        if (obj.data) {
           
         options.value.reward_name=obj.data;
 
        }
        break;
     
      default: break;
    }
  });
onMounted(() => {
  loadUserProfiles();
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

    isFirst,
    searchStamp,
    onCheckBox,
    selectedStamps,
    deleteList,
  };
});
</script>
<template>
  <div class="p-3 surface-100">
    <div class="main-layout true flex-grow-1 pb-0 pr-0 surface-0">
      <div class="p-3 pb-0">
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-chart-line"></i> Danh sách khen thưởng/kỷ luật
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
                @click="toggle"
                type="button"
                class="ml-2 "
                aria:haspopup="true"
                aria-controls="overlay_panel"
                :class="
            checkFilter 
                    ? '': 'p-button-secondary p-button-outlined'
                "
              >
                <div>
                  <span class="mr-2"><i class="pi pi-filter"></i></span>
                  <span class="mr-2">Lọc dữ liệu</span>
                  <span><i class="pi pi-chevron-down"></i></span>
                </div>
              </Button>

              <OverlayPanel
                ref="op"
                appendTo="body"
                class="p-0 m-0"
                :showCloseIcon="false"
                id="overlay_panel"
                style="width: 600px"
              >
                <div class="grid formgrid m-0">
                  <div
                    class="col-12 md:col-12 p-0"
                    style="
                      min-height: unset;
                      max-height: calc(100vh - 200px);
                      overflow: auto;
                    "
                  >
                    <div class="flex">
                      <div class="col-12 md:col-12">
                        
                        <div class="row" >
                          <div class="col-12 md:col-12">
                            <div class="py-2"  >Đối tượng khen thưởng</div>
                            <DropdownUser  :model="options.reward_name"
                            
                            :display="'chip'"
                            :placeholder="'Chọn đối tượng khen thưởng'"/>
                          </div>
                          <div class="col-12 md:col-12">
                            <div class="py-2" v-if="options.tab == 0">Cấp khen thưởng</div>
                            <div class="py-2"   v-if="options.tab == 1">Cấp kỷ luật</div>

                          
                            <MultiSelect
                              :options="listRewardLevels"
                              :filter="true"
                              :showClear="true"
                              :editable="false"
                              v-model="options.reward_level_id"
                              optionLabel="name"
                              optionValue="code"
                              placeholder="Chọn cấp"
                              class="w-full limit-width"
                              style="min-height: 36px"
                              panelClass="d-design-dropdown"
                              display="chip"
                            >
                            </MultiSelect>
                          </div>
                          <div class="col-12 md:col-12">
                     
                            <div class="py-2" v-if="options.tab == 0">Hình thức khen thưởng</div>
                            <div class="py-2"   v-if="options.tab == 1">Hình thức kỷ luật</div>
                            <MultiSelect
                              :options="listRewardTitles"
                              :filter="false"
                              :showClear="true"
                              :editable="false"
                              v-model="options.reward_title_id"
                              optionLabel="name"
                              optionValue="code"
                              display="chip"
                              placeholder="Chọn hình thức"
                              class="w-full limit-width"
                              style="min-height: 36px"
                              panelClass="d-design-dropdown" 
                            >
                            </MultiSelect>
                          </div>
                        </div>
 
                        <div class="row">
                          <div class="col-12 md:col-12">
                            <div class="py-2">Ngày quyết định</div>
                            <div class="col-12 p-0 flex">
                              <div class="col-6 p-0 md:col-6">
                                <div class="form-group">
                                  <Calendar
                                    :showIcon="true"
                                    class="ip36"
                                    autocomplete="on"
                                    inputId="time24" :showOnFocus="false"
                                    v-model="options.start_dateI"
                                    placeholder="Từ ngày"
                                  />
                                </div>
                              </div>
                              <div class="col-6 p-0 pl-2 md:col-6">
                                <div class="form-group">
                                  <Calendar
                                    :showIcon="true"
                                    class="ip36"
                                    autocomplete="on"
                                    inputId="time24" :showOnFocus="false"
                                    v-model="options.end_dateI"
                                    placeholder="Đến ngày"
                                  />
                                </div>
                              </div>
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                  <div class="col-12 md:col-12  ">
                    <Toolbar
                      class="border-none surface-0 outline-none px-0 pb-0 w-full"
                    >
                      <template #start>
                        <Button
                          @click="reFilterEmail()"
                          class="p-button-outlined mx-2"
                          label="Bỏ chọn"
                        ></Button>
                      </template>
                      <template #end>
                        <Button
                          @click="filterFileds()"
                          class="mx-2"
                          label="Lọc"
                        ></Button>
                      </template>
                    </Toolbar>
                  </div>
                </div>
              </OverlayPanel>
            </span>
          </template>

          <template #end>
            <Button
              @click="openBasic('Thêm mới')"
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
            <Button
              v-if="checkDelList"
              @click="deleteList()"
              label="Xóa"
              icon="pi pi-trash"
              class="mr-2 p-button-danger"
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
      </div>
      <div class="tabview">
        <div class="tableview-nav-content">
          <ul class="tableview-nav">
            <li
              v-for="(tab, key) in tabs"
              :key="key"
              @click="activeTab(tab)"
              class="tableview-header"
              :class="{ highlight: options.tab === tab.id }"
            >
              <a>
                <i :class="tab.icon"></i>
                <span
                  >{{ tab.title }} ({{
                    tab.id == 0
                      ? options.totalRecords1
                      : tab.id == 1
                      ? options.totalRecords2
                      : options.totalRecords
                  }})</span
                >
              </a>
            </li>
          </ul>
        </div>
      </div>
      <div class="d-container">
        <div class="d-lang-table">
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
            dataKey="reward_id"
            responsiveLayout="scroll"
            v-model:selection="selectedStamps"
            :row-hover="true"
          >
            <Column
              class="align-items-center justify-content-center text-center overflow-hidden"
              headerStyle="text-align:center;max-width:50px;height:50px"
              bodyStyle="text-align:center;max-width:50px"
               selectionMode="multiple"  v-if="store.getters.user.is_super==true"
            >
            </Column>
            <Column
              field="STT"
              header="STT"
              class="align-items-center justify-content-center text-center overflow-hidden"
              headerStyle="text-align:center;max-width:55px;height:50px"
              bodyStyle="text-align:center;max-width:55px"
            >
            </Column>
            <Column
              field="reward_number"
              header="Số quyết định"
              headerStyle="text-align:center;max-width:150px;height:50px"
              bodyStyle="text-align:center;max-width:150px"
              class="align-items-center justify-content-center text-center overflow-hidden"
            >
            </Column>
            <Column
              
              header="Loại"
              headerStyle="text-align:center;max-width:150px;height:50px"
              bodyStyle="text-align:center;max-width:150px;overflow:hidden"
              class="align-items-center justify-content-center text-center overflow-hidden"
            >
              <template #body="data">
                <div v-if="data.data.reward_type == 1">Cá nhân</div>
                <div v-else-if="data.data.reward_type == 2">Phòng ban</div>
                <div v-else>Kỷ luật</div>
              </template>
            </Column>
            <Column
              field="vacancy_name"
              header="Đối tượng"
              headerStyle="text-align:center;max-width:250px;height:50px"
              bodyStyle="text-align:center;max-width:250px;overflow:hidden"
              class="align-items-center justify-content-center text-center overflow-hidden"
            >
              <template #body="data">
                <div
                  v-if="
                    data.data.reward_type == 1 || data.data.reward_type == 3
                  "
                
                >
                  <AvatarGroup>
                    <Avatar
                      v-for="(item, index) in data.data.listRewards.slice(0, 4)"
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
                          item.full_name +
                          item.position_name +
                          item.department_name,
                        escape: true,
                      }"
                      @click="goProfile(item)"
                    />
                    <Avatar
                      v-if="data.data.listRewards.length > 4"
                      :label="(data.data.listRewards.length - 4).toString()"
                      shape="circle"
                      style="
                        background-color: #2196f3;
                        color: #fff;
                        width: 2rem;
                        height: 2rem;
                        font-size: 1rem !important;
                      "
                    />
                  </AvatarGroup>
                </div>
                <div v-else>
                  <div
                    v-for="(item, index) in data.data.listRewards"
                    :key="index"
                  >
                    <!-- <Chip :label="item.department_name" /> -->
                    {{ item.department_name }}
                  </div>
                </div>
              </template>
            </Column>
            <Column
              field="reward_content"
              header="Nội dung"
              headerStyle="text-align:left;height:50px"
              headerClass="align-items-center justify-content-center text-center overflow-hidden"
              bodyStyle="text-align:left"
            >
            </Column>
            <Column
              field="reward_level_name"
              header="Cấp khen thưởng/kỷ luật"
              headerStyle="text-align:center;max-width:150px;height:50px"
              bodyStyle="text-align:center;max-width:150px"
              class="align-items-center justify-content-center text-center overflow-hidden"
            >
            </Column>
            <Column
              field="reward_title_name"
              header="Hình thức"
              headerStyle="text-align:center;max-width:200px;height:50px"
              bodyStyle="text-align:center;max-width:200px"
              class="align-items-center justify-content-center text-center overflow-hidden"
            >
            </Column>
            <Column
              field="start_date"
              header="Ngày quyết định"
              headerStyle="text-align:center;max-width:100px;height:50px"
              bodyStyle="text-align:center;max-width:100px"
              class="align-items-center justify-content-center text-center overflow-hidden"
            >
              <template #body="data">
                <div v-if="data.data.decision_date">
                  {{
                    moment(new Date(data.data.decision_date)).format(
                      "DD/MM/YYYY"
                    )
                  }}
                </div>
              </template>
            </Column>

            <Column
              field="created_date"
              header="Ngày/Người lập"
              headerStyle="text-align:center;max-width:120px;height:50px"
              bodyStyle="text-align:center;max-width:120px;"
              class="align-items-center justify-content-center text-center overflow-hidden"
            >
              <template #body="slotProps">
                <span class="mr-2">
                  {{
                    moment(new Date(slotProps.data.created_date)).format(
                      "DD/MM/YYYY"
                    )
                  }}</span
                >
                <div>
                  <Avatar
                    v-bind:label="
                      slotProps.data.avatar
                        ? ''
                        : slotProps.data.full_name.substring(0, 1)
                    "
                    v-bind:image="
                      slotProps.data.avatar
                        ? basedomainURL + slotProps.data.avatar
                        : basedomainURL + '/Portals/Image/noimg.jpg'
                    "
                    style="
                      background-color: #2196f3;
                      color: #fff;
                      width: 2rem;
                      height: 2rem;
                      font-size: 1rem !important;
                    "
                    :style="{
                      background: bgColor[slotProps.data.created_is_order % 7],
                    }"
                    class="text-avatar"
                    size="xlarge"
                    shape="circle"
                    v-tooltip.top="{
                      value:
                        slotProps.data.full_name +
                        slotProps.data.position_name +
                        slotProps.data.department_name,
                      escape: true,
                    }"
                  />
                </div>
              </template>
            </Column>
            <Column
              header=""
              headerStyle="text-align:center;max-width:50px"
              bodyStyle="text-align:center;max-width:50px"
              class="align-items-center justify-content-center text-center  "
            >
              <template #body="slotProps">
                <Button
                  icon="pi pi-ellipsis-h"
                  class="p-button-rounded p-button-text ml-2"
                  @click="toggleMores($event, slotProps.data)"
                  aria-haspopup="true"
                  aria-controls="overlay_More"
                  v-tooltip.top="'Tác vụ'"
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
        </div>
      </div>
    </div>
    <div v-if="displayBasic == true">
      <dialogReward
        :headerDialog="headerDialog"
        :displayBasic="displayBasic"
        :reward="reward"
        :files="listFilesS"
        :view="false"
        :checkadd="isSaveTem"
        :closeDialog="closeDialog"
      />
    </div>
  </div>

  <Menu
    id="overlay_More"
    ref="menuButMores"
    :model="itemButMores"
    :popup="true"
  />
  <Dialog
    :style="{ width: '20vw' }"
    :header="headerExport"
    v-model:visible="showExport"
    :modal="true"
  >
    <div class="grid">
      <div class="col-12 field flex">
        <div class="col-6 p-0">Số bản ghi:</div>
        <div class="col-6 p-0">
          <InputNumber class="w-full" v-model="options.totalRecordsExport" />
        </div>
      </div>
      <div class="col-12 field flex">
        <div class="col-6 p-0">Trang bắt đầu:</div>
        <div class="col-6 p-0">
          <InputNumber
            class="w-full"
            :min="1"
            :max="Math.ceil(options.totalRecords / options.totalRecordsExport)"
            v-model="options.pagenoExport"
          />
        </div>
      </div>
      <div class="col-12 p-0">
        <Toolbar class="surface-0 p-0 border-0">
          <template #end>
            <div>
              <Button label="Xuất" @click="exportExcelR"></Button>
            </div>
          </template>
        </Toolbar>
      </div>
    </div>
  </Dialog>
</template>
  
<style scoped>
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
  margin: 0px;
  height: calc(100vh - 200px);
}

.tableview-nav {
  background: #ffffff;
  border: 1px solid #dee2e6;
  border-width: 0 0 2px 0;
  display: flex;
  flex: 1 1 auto;
  list-style-type: none;
  margin: 0;
  padding: 0;
}

.tableview-header {
  display: inline-block;
}

.tableview-nav li {
  border: solid #dee2e6;
  border-width: 0 0 2px 0;
  padding: 1.25rem;
  font-weight: 700;
  margin: 0 0 -2px 0;
  transition: background-color 0.2s, border-color 0.2s, box-shadow 0.2s;
}

.tableview-nav li:hover {
  cursor: pointer;
}

.tableview-nav li.highlight {
  background: #ffffff;
  border-color: #3b82f6;
  color: #3b82f6;
}

.tableview-nav li:not(.highlight):hover {
  background: #ffffff;
  border-color: #adb5bd;
  color: #6c757d;
}

.tableview-nav li a:focus {
  outline: 0 none;
  outline-offset: 0;
  box-shadow: inset 0 0 0 0.2rem #bfdbfe;
}

.btn-hidden {
  filter: opacity(40%) !important;
  cursor: auto !important;
}

.hover:hover {
  cursor: pointer;
  color: #2196f3 !important;
}

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
    