<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../../util/function.js";
import moment from "moment";
import dialogCampaign from "./component/dialog_campaign.vue";
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
  campaign_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  campaign_code: {
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

const listStatus = ref([
  { name: "Lên kế hoạch", code: 1 },
  { name: "Đang thực hiện", code: 2 },
  { name: "Đã hoàn thành", code: 3 },
  { name: "Tạm dừng", code: 4 },
  { name: "Đã hủy", code: 5 },
]);
const listFormTraining = ref([
  { name: "Bắt buộc", code: 1 },
  { name: "Đăng ký", code: 2 },
  { name: "Cả hai", code: 3 },
]);
//Lấy số bản ghi
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_campaign_count",
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
      let data1 = JSON.parse(response.data.data)[1];
      let data2 = JSON.parse(response.data.data)[2];
      let data3 = JSON.parse(response.data.data)[3];
      let data4 = JSON.parse(response.data.data)[4];
      let data5 = JSON.parse(response.data.data)[5];
      if (data.length > 0) {
        options.value.totalRecords = data[0].totalRecords;
        options.value.totalRecords1 = data1[0].totalRecords1;
        options.value.totalRecords2 = data2[0].totalRecords2;
        options.value.totalRecords3 = data3[0].totalRecords3;
        options.value.totalRecords4 = data4[0].totalRecord4;
        options.value.totalRecords5 = data5[0].totalRecords5;

        sttStamp.value = data[0].totalRecords + 1;
      }
    })
    .catch((error) => { });
};

const campaign = ref({
  campaign_name: null,
  is_recruitment_proposal: null,
  user_verify: null,
  user_follows: null,
  num_vacancies: null,
  expected_cost: null,
  start_date: null,
  end_date: null,
  rec_vacancies: null,
  rec_position_id: null,
  rec_formality_id: null,
  rec_salary_from: null,
  rec_workplace: null,
  rec_salary_to: null,
  rec_recruitment_deadline: null,
  rec_number_vacancies: null,
  rec_candidate_sheet_id: null,
  can_academic_level_id: null,
  can_specialization_id: null,
  can_experience_id: null,
  can_language_level_id: null,
  can_age_from: null,
  can_age_to: null,
  can_gender:null,
  can_height_from: null,
  can_height_to: null,
  can_weight_to: null,
  can_weight_from: null,
  job_description: null,
  


});
//Lấy dữ liệu campaign
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
              proc: "hrm_campaign_list",
              par: [
                { par: "pageno", va: options.value.PageNo },
                { par: "pagesize", va: options.value.PageSize },
                { par: "user_id", va: store.getters.user.user_id },
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
      datalists.value[datalists.value.length - 1].campaign_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].campaign_id;
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
  sort: "campaign_id desc ",
  SearchText: "",
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: 0,
  tab: -1,
  totalRecords1: 0,
  totalRecords2: 0,
  totalRecords3: 0,
  totalRecords4: 0,
  totalRecords5: 0,
});

//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);
const openBasic = (str) => {
  campaign.value = {
 
    campaign_name: null,
    form_training: 1,
    status: 1,
    training_place: null,
    is_order: sttStamp.value,
    organization_id: store.getters.user.organization_id,
    user_follows_fake: [],
    user_verify_fake: [],
 
  };

 
  isSaveTem.value = true;
  headerDialog.value = str;

  displayBasic.value = true;
};

const closeDialog = () => {
  campaign.value = {
    campaign_name: "",
    emote_file: "",
    status: true,
    is_default: false,
    is_order: 1,
  };

  displayBasic.value = false;
  loadData(true);
};
const sttStamp = ref(1);

//Sửa bản ghi
const editTem = (dataTem) => {
  campaign.value = dataTem;
  headerDialog.value = "Sửa chiến dịch";
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
          .delete(baseURL + "/api/hrm_campaign/delete_hrm_campaign", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Tem != null ? [Tem.campaign_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá thông tin chiến dịch thành công!");
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

  let data = {
    id: "campaign_id",
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
    .post(baseURL + "/api/HRM_SQL/Filter_hrm_campaign", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.STT = options.value.PageNo * options.value.PageSize + i + 1;
          if (element.li_user_verify) {
            element.li_user_verify = JSON.parse(element.li_user_verify);
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
        options.value.totalRecords1 = dt[2][0].totalRecords1;
        options.value.totalRecords2 = dt[3][0].totalRecords2;
        options.value.totalRecords3 = dt[4][0].totalRecords3;
        options.value.totalRecords4 = dt[5][0].totalRecords4;
        options.value.totalRecords5 = dt[6][0].totalRecords5;
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

const setStatus = (value) => {
  opstatus.value.hide();
  let data = {
    IntID: value.campaign_id,
    TextID: value.campaign_id + "",
    IntTrangthai: value.status,
    BitTrangthai: false,
  };
  axios
    .put(
      baseURL + "/api/hrm_campaign/update_s_hrm_campaign",
      data,
      config
    )
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái thành công!");
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
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};

const opstatus = ref();
const toggleStatus = (item, event) => {
  campaign.value = item;
  opstatus.value.toggle(event);
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
  options.value.status_filter = null;
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
  { id: 0, title: "Tất cả", icon: "", total: options.value.totalRecords },
  { id: 1, title: "Lên kế hoạch", icon: "", total: 0 },
  { id: 2, title: "Đang thực hiện", icon: "", total: 0 },
  { id: 3, title: "Đã hoàn thành", icon: "", total: 0 },
  { id: 4, title: "Tạm dừng", icon: "", total: 0 },
  { id: 5, title: "Đã hủy", icon: "", total: 0 },
]);
//Checkbox
const onCheckBox = (value, check) => {
  if (check) {
    let data = {
      IntID: value.campaign_id,
      TextID: value.campaign_id + "",
      IntTrangthai: 1,
      BitTrangthai: value.status,
    };
    axios
      .put(
        baseURL + "/api/hrm_campaign/update_s_hrm_campaign",
        data,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa trạng thái chiến dịch thành công!");
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
      IntID: value.campaign_id,
      TextID: value.campaign_id + "",
      BitMain: value.is_default,
    };
    axios
      .put(
        baseURL + "/api/hrm_campaign/Update_DefaultStamp",
        data1,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa trạng thái chiến dịch thành công!");
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
      baseURL + "/api/Excel/ExportExcelWithLogo",
      {
        excelname: "DANH SÁCH THÔNG TIN ĐÀO TẠO",
        proc: "hrm_campaign_export",
        par: [

          { par: "user_id", va: store.state.user.user_id },
          { par: "search", va: options.value.SearchText },
          { par: "training_groups", va: options.value.training_groups_id },
          { par: "user_verify", va: options.value.user_verify },
          { par: "user_follows", va: options.value.user_follows },
          { par: "form_training", va: options.value.type_formtraining },
          { par: "status ", va: options.value.status_filter },
          { par: "start_date", va: options.value.start_date },
          { par: "end_date", va: options.value.end_date },
          { par: "sort", va: options.value.sort },
          { par: "pageno", va: options.value.PageNo },
          { par: "pagesize", va: options.value.PageSize },
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

const activeTab = (tab) => {
  options.value.tab = tab.id;
  reFilter();
  if (tab.id) {
    checkLoadCount.value = false;
    let filterS1 = {
      filterconstraints: [{ value: tab.id, matchMode: "equals" }],
      filteroperator: "and",
      key: "status",
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
      editTem(campaign.value, "Chỉnh sửa hợp đồng");
    },
  },
  {
    label: "Xoá",
    icon: "pi pi-trash",
    command: (event) => {
      delTem(campaign.value);
    },
  },
]);
const toggleMores = (event, item) => {
  campaign.value = item;
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
        text: "Bạn có muốn xoá thông tin chiến dịch này không!",
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
            listId.push(item.campaign_id);
          });
          axios
            .delete(
              baseURL + "/api/hrm_campaign/delete_hrm_campaign",
              {
                headers: { Authorization: `Bearer ${store.getters.token}` },
                data: listId != null ? listId : 1,
              }
            )
            .then((response) => {
              swal.close();
              if (response.data.err != "1") {
                swal.close();
                toast.success("Xoá thông tin chiến dịch thành công!");
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
const reFilter = () => {
  options.value.user_follows = null;
  options.value.training_groups_id = null;
  options.value.user_verify = null;
  options.value.start_date = null;
  options.value.end_date = null
  options.value.type_formtraining = null;

  options.value.status_filter = null;
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
  if (options.value.status_filter) {
    let filterS1 = {
      filterconstraints: [],
      filteroperator: "or",
      key: "status",
    };
    if (options.value.status_filter.length > 0) {
      options.value.status_filter.forEach((element) => {
        var addr = { value: element, matchMode: "equals" };
        filterS1.filterconstraints.push(addr);
      });

      filterSQL.value.push(filterS1);
    }
  }
  if (options.value.type_formtraining) {
    let filterS2 = {
      filterconstraints: [],
      filteroperator: "or",
      key: "form_training",
    };
    if (options.value.type_formtraining.length > 0) {
      options.value.type_formtraining.forEach((element) => {
        var addr = { value: element, matchMode: "equals" };
        filterS2.filterconstraints.push(addr);
      });

      filterSQL.value.push(filterS2);
    }
  }
  if (options.value.user_follows) {
    let filterS3 = {
      filterconstraints: [],
      filteroperator: "or",
      key: "user_follows",
    };
    if (options.value.user_follows.length > 0) {
      options.value.user_follows.forEach((element) => {
        var addr = { value: element.code, matchMode: "contains" };
        filterS3.filterconstraints.push(addr);
      });

      filterSQL.value.push(filterS3);
    }
  }
  if (options.value.training_groups_id) {
    let filterS4 = {
      filterconstraints: [],
      filteroperator: "or",
      key: "training_groups_id",
    };
    if (options.value.training_groups_id.length > 0) {
      options.value.training_groups_id.forEach((element) => {
        var addr = { value: element, matchMode: "equals" };
        filterS4.filterconstraints.push(addr);
      });

      filterSQL.value.push(filterS4);
    }
  }
  if (options.value.user_verify) {
    let filterS5 = {
      filterconstraints: [],
      filteroperator: "or",
      key: "user_verify",
    };
    if (options.value.user_verify.length > 0) {
      options.value.user_verify.forEach((element) => {
        var addr = { value: element.code, matchMode: "contains" };
        filterS5.filterconstraints.push(addr);
      });

      filterSQL.value.push(filterS5);
    }
  }

  onDayClick();
  loadDataSQL();
  op.value.hide();
};


const onDayClick = () => {
  if (options.value.start_date != null) {

    if (!options.value.end_date)
      options.value.end_date = options.value.start_date;

    if (
      options.value.start_date &&
      options.value.start_date != options.value.end_date
    ) {
      let sDate = new Date(options.value.start_date);
      sDate.setDate(sDate.getDate() - 1);
      options.value.start_date = sDate;
      let filterS = {
        filterconstraints: [
          { value: options.value.start_date, matchMode: "dateAfter" },
        ],
        filteroperator: "and",
        key: "start_date",
      };
      filterSQL.value.push(filterS);
    }
    if (
      options.value.end_date &&
      options.value.start_date != options.value.end_date
    ) {
      let eDate = new Date(options.value.end_date);
      eDate.setDate(eDate.getDate() + 1);
      options.value.end_date = eDate;
      let filterS = {
        filterconstraints: [
          { value: options.value.end_date, matchMode: "dateBefore" },
        ],
        filteroperator: "and",
        key: "end_date",
      };
      filterSQL.value.push(filterS);
    }
    if (
      options.value.start_date &&
      options.value.start_date == options.value.end_date
    ) {
      let filterS1 = {
        filterconstraints: [
          { value: options.value.start_date, matchMode: "dateIs" },
        ],
        filteroperator: "and",
        key: "start_date",
      };
      filterSQL.value.push(filterS1);
      let filterS2 = {
        filterconstraints: [
          { value: options.value.end_date, matchMode: "dateIs" },
        ],
        filteroperator: "and",
        key: "end_date",
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

const listDropdownUserCheck = ref();
const listDropdownUser = ref();

const loadUser = () => {
  listDropdownUser.value = [];
  axios
    .post(
      baseURL + "/api/device_card/getData",
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
          role_name: element.role_name,
          position_name: element.position_name,
        });
      });

      listDropdownUserCheck.value = [...listDropdownUser.value];
    })
    .catch((error) => {
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

const listTrainingGroups = ref([]);
const listClasroom = ref([]);

const initTudien = () => {
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_ca_enecting_group_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 100000 },
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
      listTrainingGroups.value = [];
      data.forEach((element) => {
        listTrainingGroups.value.push({
          name: element.enecting_group_name,
          code: element.enecting_group_id,
        });
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
            proc: "hrm_ca_classroom_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 100000 },
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
      listClasroom.value = [];
      data.forEach((element) => {
        listClasroom.value.push({
          name: element.classroom_name,
          code: element.classroom_id,
        });
      });
    })
    .catch((error) => {
      console.log(error);
    });
  loadUser();
};

onMounted(() => {
  if (!checkURL(window.location.pathname, store.getters.listModule)) {
    //router.back();
  }
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
          <i class="pi pi-chart-line"></i> Danh sách chiến dịch
        </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText v-model="options.SearchText" @keyup="searchStamp" type="text" spellcheck="false"
                placeholder="Tìm kiếm" />

              <Button @click="toggle" type="button" class="ml-2 p-button-outlined p-button-secondary" aria:haspopup="true"
                aria-controls="overlay_panel" :class="
                  options.status_filter == null &&
                    options.form_training == null &&
                    !checkFilter
                    ? ''
                    : 'p-button-secondary p-button-outlined'
                ">
                <div>
                  <span class="mr-2"><i class="pi pi-filter"></i></span>
                  <span class="mr-2">Lọc dữ liệu</span>
                  <span><i class="pi pi-chevron-down"></i></span>
                </div>
              </Button>

              <OverlayPanel ref="op" appendTo="body" class="p-0 m-0" :showCloseIcon="false" id="overlay_panel"
                style="width: 700px">
                <div class="grid formgrid m-0">
                  <div class="col-12 md:col-12 p-0" style="
                                  min-height: unset;
                                  max-height: calc(100vh - 300px);
                                  overflow: auto;
                                ">
                    <div class="flex">
                      <div class="col-6 md:col-6">
                        <div class="row">
                          <div class="col-12 md:col-12 p-0">
                            <div class="form-group   ">
                              <div  class="py-2">Vị trí tuyển dụng</div>
                              <MultiSelect :options="listTrainingGroups" :filter="true" :showClear="true"
                                :editable="false" v-model="options.training_groups_id" optionLabel="name"
                                optionValue="code" placeholder="Chọn vị trí tuyển dụng" class="w-full limit-width"
                                style="min-height: 36px" panelClass="d-design-dropdown">
                              </MultiSelect>
                            </div>
                          </div>
                          <div class="col-12 md:col-12 p-0">
                            <div class="col-12 p-0 ">
                              <div  class="py-2">Người phụ trách</div>
                             
                            </div>
                            <MultiSelect :options="listDropdownUser" :filter="true" :showClear="true" :editable="false"
                              display="chip" v-model="options.user_verify" optionLabel="name"
                              placeholder="Chọn người phụ trách" panelClass="d-design-dropdown  d-tree-input"
                              class="col-12     " style="min-height: 36px">
                              <template #option="slotProps">
                                <div class="country-item flex align-items-center">
                                  <div class="grid w-full p-0">
                                    <div class="
                                                      field
                                                      p-0
                                                      py-1
                                                      col-12
                                                      flex
                                                      m-0
                                                      cursor-pointer
                                                      align-items-center
                                                    ">
                                      <div class="
                                                        col-1
                                                        mx-2
                                                        p-0
                                                        align-items-center
                                                      ">
                                        <Avatar v-bind:label="
                                          slotProps.option.avatar
                                            ? ''
                                            : slotProps.option.name.substring(
                                              slotProps.option.name.lastIndexOf(
                                                ' '
                                              ) + 1,
                                              slotProps.option.name.lastIndexOf(
                                                ' '
                                              ) + 2
                                            )
                                        " :image="
  basedomainURL +
  slotProps.option.avatar
" size="small" :style="
  slotProps.option.avatar
    ? 'background-color: #2196f3'
    : 'background:' +
    bgColor[
    slotProps.option.name
      .length % 7
    ]
" shape="circle" @error="
  $event.target.src =
  basedomainURL +
  '/Portals/Image/nouser1.png'
" />
                                      </div>
                                      <div class="
                                                        col-11
                                                        p-0
                                                        ml-3
                                                        align-items-center
                                                      ">
                                        <div class="pt-2">
                                          <div class="font-bold">
                                            {{ slotProps.option.name }}
                                          </div>
                                          <div class="
                                                            flex
                                                            w-full
                                                            text-sm
                                                            font-italic
                                                            text-500
                                                          ">
                                            <div>
                                              {{
                                                slotProps.option.position_name
                                              }}
                                            </div>
                                          </div>
                                        </div>
                                      </div>
                                    </div>
                                  </div>
                                </div>
                              </template>
                            </MultiSelect>

                          </div>
                          <div class="col-12 md:col-12 p-0">
                            <div class="form-group">
                              <div  class="py-2">Trình độ</div>
                             
                              <MultiSelect :options="listFormTraining" :filter="false" :showClear="true" :editable="false"
                                v-model="options.type_formtraining" optionLabel="name" optionValue="code" display="chip"
                                placeholder="Chọn trình độ" class="w-full limit-width"
                                style="min-height: 36px" panelClass="d-design-dropdown">
                              </MultiSelect>
                            </div>
                          </div>
                        </div>
                      </div>
                      <div class="col-6 md:col-6">
                        <div class="row">
                          <div class="col-12 md:col-12">
                            <div class="form-group m-0 py-2">
                              <div>Thời gian chiến dịch</div>
                            </div>
                          </div>
                          <div class="col-12 p-0 flex">
                            <div class="col-6 md:col-6">
                              <div class="form-group">
                                <Calendar :showIcon="true" class="ip36" autocomplete="on" inputId="time24"
                                  v-model="options.start_date" placeholder="Từ ngày" />
                              </div>
                            </div>
                            <div class="col-6 md:col-6">
                              <div class="form-group">
                                <Calendar :showIcon="true" class="ip36" autocomplete="on" inputId="time24"
                                  v-model="options.end_date" placeholder="Đến ngày" />
                              </div>
                            </div>
                          </div>
                          <div class="col-12 md:col-12">

                            <div class="col-12 p-0 pt-2">
                              <label>Người theo dõi</label>
                            </div>
                            <MultiSelect :options="listDropdownUser" :filter="true" :showClear="true" :editable="false"
                              display="chip" v-model="options.user_follows" optionLabel="name"
                              placeholder="Chọn người theo dõi" style="min-height: 36px"
                              panelClass="d-design-dropdown  d-tree-input" class="col-12 my-2   ">
                              <template #option="slotProps">
                                <div class="country-item flex align-items-center">
                                  <div class="grid w-full p-0">
                                    <div class="
                                                      field
                                                      p-0
                                                      py-1
                                                      col-12
                                                      flex
                                                      m-0
                                                      cursor-pointer
                                                      align-items-center
                                                    ">
                                      <div class="
                                                        col-1
                                                        mx-2
                                                        p-0
                                                        align-items-center
                                                      ">
                                        <Avatar v-bind:label="
                                          slotProps.option.avatar
                                            ? ''
                                            : slotProps.option.name.substring(
                                              slotProps.option.name.lastIndexOf(
                                                ' '
                                              ) + 1,
                                              slotProps.option.name.lastIndexOf(
                                                ' '
                                              ) + 2
                                            )
                                        " :image="
  basedomainURL +
  slotProps.option.avatar
" size="small" :style="
  slotProps.option.avatar
    ? 'background-color: #2196f3'
    : 'background:' +
    bgColor[
    slotProps.option.name
      .length % 7
    ]
" shape="circle" @error="
  $event.target.src =
  basedomainURL +
  '/Portals/Image/nouser1.png'
" />
                                      </div>
                                      <div class="
                                                        col-11
                                                        p-0
                                                        ml-3
                                                        align-items-center
                                                      ">
                                        <div class="pt-2">
                                          <div class="font-bold">
                                            {{ slotProps.option.name }}
                                          </div>
                                          <div class="
                                                            flex
                                                            w-full
                                                            text-sm
                                                            font-italic
                                                            text-500
                                                          ">
                                            <div>
                                              {{
                                                slotProps.option.position_name
                                              }}
                                            </div>
                                          </div>
                                        </div>
                                      </div>
                                    </div>
                                  </div>
                                </div>
                              </template>
                            </MultiSelect>

                          </div>

                          <div class="col-12 md:col-12">
                            <div class="form-group ">
                              <div class="pb-2">Chuyên ngành</div>
                              <MultiSelect :options="listStatus" v-model="options.status_filter" :filter="true"
                                :showClear="true" :editable="false" display="chip" optionLabel="name" optionValue="code"
                                placeholder="Chọn chuyên ngành" class="w-full limit-width" style="min-height: 36px"
                                panelClass="d-design-dropdown">
                              </MultiSelect>
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                  <div class="col-12 md:col-12 p-0">
                    <Toolbar class="
                                    border-none
                                    surface-0
                                    outline-none
                                    px-0
                                    pb-0
                                    w-full
                                  ">
                      <template #start>
                        <Button @click="reFilterEmail()" class="p-button-outlined mx-2" label="Bỏ chọn"></Button>
                      </template>
                      <template #end>
                        <Button @click="filterFileds()" class="mx-3" label="Lọc"></Button>
                      </template>
                    </Toolbar>
                  </div>
                </div>
              </OverlayPanel>
            </span>
          </template>

          <template #end>
            <Button @click="openBasic('Thêm mới chiến dịch')" label="Thêm mới" icon="pi pi-plus" class="mr-2" />
            <Button @click="refreshStamp" class="mr-2 p-button-outlined p-button-secondary" icon="pi pi-refresh"
              v-tooltip="'Tải lại'" />
            <Button v-if="checkDelList" @click="deleteList()" label="Xóa" icon="pi pi-trash"
              class="mr-2 p-button-danger" />
            <Button label="Tiện ích" icon="pi pi-file-excel" class="mr-2 p-button-outlined p-button-secondary"
              @click="toggleExport" aria-haspopup="true" aria-controls="overlay_Export" />
            <Menu id="overlay_Export" ref="menuButs" :model="itemButs" :popup="true" />
          </template>
        </Toolbar>
      </div>
      <div class="tabview">
        <div class="tableview-nav-content">
          <ul class="tableview-nav">
            <li v-for="(tab, key) in tabs" :key="key" @click="activeTab(tab)" class="tableview-header"
              :class="{ highlight: options.tab === tab.id }">
              <a>
                <i :class="tab.icon"></i>
                <span>{{ tab.title }} ({{
                  tab.id == 1
                  ? options.totalRecords1
                  : tab.id == 2
                    ? options.totalRecords2
                    : tab.id == 3
                      ? options.totalRecords3
                      : tab.id == 4
                        ? options.totalRecords4
                        : tab.id == 5
                          ? options.totalRecords5
                          : options.totalRecords
                }})</span>
              </a>
            </li>
          </ul>
        </div>
      </div>
      <div class="d-container">
        <div class="d-lang-table">
          <DataTable @page="onPage($event)" @sort="onSort($event)" @filter="onFilter($event)" v-model:filters="filters"
            filterDisplay="menu" filterMode="lenient" :filters="filters" :scrollable="true" scrollHeight="flex"
            :showGridlines="true" columnResizeMode="fit" :lazy="true" :totalRecords="options.totalRecords"
            :loading="options.loading" :reorderableColumns="true" :value="datalists" removableSort
            v-model:rows="options.PageSize"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
            :rowsPerPageOptions="[20, 30, 50, 100, 200]" :paginator="true" dataKey="campaign_id"
            responsiveLayout="scroll" v-model:selection="selectedStamps" :row-hover="true">
            <Column class="align-items-center justify-content-center text-center"
              headerStyle="text-align:center;max-width:70px;height:50px" bodyStyle="text-align:center;max-width:70px"
              selectionMode="multiple">
            </Column>

            <Column field="STT" header="STT" class="align-items-center justify-content-center text-center"
              headerStyle="text-align:center;max-width:70px;height:50px" bodyStyle="text-align:center;max-width:70px">
            </Column>
           
            <Column field="campaign_name" header="Tên chiến dịch" :sortable="true"
              headerStyle="text-align:left;height:50px" bodyStyle="text-align:left">
              <template #filter="{ filterModel }">
                <InputText type="text" v-model="filterModel.value" class="p-column-filter" placeholder="Từ khoá" />
              </template>
            </Column>
            <Column field="num_vacancies" header="Số lượng tuyển" headerStyle="text-align:center;max-width:100px;height:50px"
              bodyStyle="text-align:center;max-width:100px" class="align-items-center justify-content-center text-center">
             
            </Column>
            <Column field="expected_cost" header="Chi phí dự kiến" headerStyle="text-align:center;max-width:150px;height:50px"
              bodyStyle="text-align:center;max-width:150px" class="align-items-center justify-content-center text-center">
              <template #body="data">
                  <div>
                    {{ data.data.expected_cost?data.data.expected_cost.toLocaleString():'' }} VND
                  </div>
                </template>
            </Column>
            <Column field="start_date" header="Ngày bắt đầu" headerStyle="text-align:center;max-width:100px;height:50px"
              bodyStyle="text-align:center;max-width:100px" class="align-items-center justify-content-center text-center">
              <template #body="data">
                <div v-if="data.data.start_date">
                  {{
                    moment(new Date(data.data.start_date)).format("DD/MM/YYYY")
                  }}
                </div>
              </template>
            </Column>

            <Column field="end_date" header="Ngày kết thúc" headerStyle="text-align:center;max-width:100px;height:50px"
              bodyStyle="text-align:center;max-width:100px" class="align-items-center justify-content-center text-center">
              <template #body="data">
                <div v-if="data.data.end_date">
                  {{
                    moment(new Date(data.data.end_date)).format("DD/MM/YYYY")
                  }}
                </div>
              </template>
            </Column>
            <!-- <Column field="li_user_verify" header="Giảng viên" headerStyle="text-align:center;max-width:150px;height:50px"
              bodyStyle="text-align:center;max-width:150px" class="align-items-center justify-content-center text-center">
             
            </Column>
            <Column field="count_emps" header="Học viên" headerStyle="text-align:center;max-width:100px;height:50px"
              bodyStyle="text-align:center;max-width:100px" class="align-items-center justify-content-center text-center">
              <template #body="data">
                <div>
                  {{ data.data.count_emps ? data.data.count_emps : "0" }}
                </div>
              </template>
            </Column> -->
            <Column field="created_date" header="Ngày tạo" headerStyle="text-align:center;max-width:150px;height:50px"
              bodyStyle="text-align:center;max-width:150px" class="align-items-center justify-content-center text-center">
              <template #body="data">
                <div>
                  {{
                    moment(new Date(data.data.created_date)).format(
                      "DD/MM/YYYY"
                    )
                  }}
                </div>
              </template>
            </Column>
            <!-- <Column field="status" header="Trạng thái" headerStyle="text-align:center;max-width:11rem;height:50px"
              bodyStyle="text-align:center;max-width:11rem" class="align-items-center justify-content-center text-center">
              <template #body="slotProps">
                <div class="m-2" @click="
                  toggleStatus(slotProps.data, $event);
                $event.stopPropagation();
                                                                                                                              "
                  aria:haspopup="true" aria-controls="overlay_panel_status">
                  <Button :label="
                    slotProps.data.status == 1
                      ? 'Lên kế hoạch'
                      : slotProps.data.status == 2
                        ? 'Đang thực hiện'
                        : slotProps.data.status == 3
                          ? 'Đã hoàn thành'
                          : slotProps.data.status == 4
                            ? 'Tạm dừng'
                            : 'Đã hủy'
                  " :class="
  slotProps.data.status == 1
    ? 'bg-blue-500'
    : slotProps.data.status == 2
      ? 'bg-yellow-500'
      : slotProps.data.status == 3
        ? 'bg-green-500'
        : slotProps.data.status == 4
          ? 'bg-orange-500'
          : 'bg-pink-500'
" icon="pi pi-chevron-down" iconPos="right" class="px-2 w-10rem" />
                </div>
                <OverlayPanel :showCloseIcon="false" ref="opstatus" appendTo="body" class="p-0 m-0"
                  id="overlay_panel_status" style="width: 200px">
                  <div class="form-group">
                    <div class="col-12 p-0 field">Chọn trạng thái</div>
                    <div class="col-12 p-0">
                      <Dropdown :options="listStatus" :filter="false" :showClear="false" :editable="false"
                        v-model="campaign.status" optionLabel="name" optionValue="code" placeholder="Chọn trạng thái"
                        class="w-full" @change="setStatus(campaign)">
                      </Dropdown>
                    </div>
                  </div>
                </OverlayPanel>
              </template>
            </Column> -->
            <Column header="" headerStyle="text-align:center;max-width:50px" bodyStyle="text-align:center;max-width:50px"
              class="align-items-center justify-content-center text-center">
              <template #body="slotProps">
                <Button icon="pi pi-ellipsis-h" class="p-button-rounded p-button-text ml-2"
                  @click="toggleMores($event, slotProps.data)" aria-haspopup="true" aria-controls="overlay_More"
                  v-tooltip.top="'Tác vụ'" />
              </template>
            </Column>
            <!-- <Column
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
                                  store.state.user.organization_id ==
                                    Tem.data.organization_id)
                              "
                            >
                              <Button
                                @click="editTem(Tem.data)"
                                class="
                                  p-button-rounded p-button-secondary p-button-outlined
                                  mx-1
                                "
                                type="button"
                                icon="pi pi-pencil"
                                v-tooltip.top="'Sửa'"
                              ></Button>
                              <Button
                                class="
                                  p-button-rounded p-button-secondary p-button-outlined
                                  mx-1
                                "
                                type="button"
                                icon="pi pi-trash"
                                @click="delTem(Tem.data)"
                                v-tooltip.top="'Xóa'"
                              ></Button>
                            </div>
                          </template>
                        </Column> -->
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
          </DataTable>
        </div>
      </div>
    </div>
    <div v-if="displayBasic">
      <dialogCampaign :headerDialog="headerDialog" :displayBasic="displayBasic" :campaign="campaign" :checkadd="isSaveTem"
        :view="false" :closeDialog="closeDialog" />
    </div>
  </div>

  <Menu id="overlay_More" ref="menuButMores" :model="itemButMores" :popup="true" />
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
    