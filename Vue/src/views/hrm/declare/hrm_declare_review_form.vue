<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { encr, checkURL } from "../../../util/function.js";
import tree_users_hrm from "../component/tree_users_hrm.vue";
import { FilterMatchMode, FilterOperator } from "primevue/api";
//Khai báo
import moment from "moment";
const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const isDynamicSQL = ref(false);
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};

const rules = {
  review_form_name: {
    required,
    $errors: [
      {
        $property: "review_form_name",
        $validator: "required",
        $message: "Tên mẫu biểukhông được để trống!",
      },
    ],
  },
};

const datalistsDetails = ref();
const review_form = ref({
  review_form_name: "",
  emote_file: "",
  status: true,
  is_default: false,
  is_order: 1,
});

const submitted = ref(false);

const v$ = useVuelidate(rules, review_form);

const isSaveTem = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = baseURL;
const options = ref({
  IsNext: true,
  sort: "created_date",
  SearchText: "",
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
  loadingP: true,
  PageNoP: 0,
  PageSizeP: 20,
  searchP: "",
  sortP: "created_date",
  totalRecordsP: null,
});

//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);

const sttPaycheck = ref(1);
const isFirst = ref(true);

const treemodules = ref();

const checkShow = ref(false);
const listEvalCriterias = ref([]);
const listEvalChilds = ref([]);
const listTypeEvals = ref([
  {
    name: "Tiêu chí đánh giá",
    code: 1,
  },
  {
    name: "Công việc chuyên môn",
    code: 2,
  },
]);
//Lấy dữ liệu review_form
const loadData = (rf) => {
  if (rf) {
    axios
      .post(
        baseURL + "/api/hrm_ca_SQL/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "hrm_declare_review_form_list",
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
        if (isFirst.value) isFirst.value = false;
        data.forEach((element, i) => {
          element.STT = options.value.PageNo * options.value.PageSize + i + 1;
        });

        datalists.value = data;

        review_form.value = data[0];
        loadDataDetails(true);
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
      datalists.value[datalists.value.length - 1].review_form_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].review_form_id;
    options.value.IsNext = false;
  }
  options.value.PageNo = event.page;
  loadData(true);
};
const openBasic = (str) => {
  listEvalCriterias.value = [];
  submitted.value = false;
  let obj = {
    roman_order: numberToRoman(listEvalCriterias.value.length + 1),
    is_order: listEvalCriterias.value.length + 1,
    percen: null,
    eval_criteria_name: null,
    des: null,
    is_plan: true,
    is_results: true,
    status: true,
    type: 1,
  };
  listEvalCriterias.value.push(obj);

  isSaveTem.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};

const closeDialog = () => {
  review_form.value = {
    review_form_name: "",
    emote_file: "",
    status: true,
    is_default: false,
    is_order: 1,
  };

  displayBasic.value = false;
  loadData(true);
};

const saveData = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }

  if (review_form.value.review_form_name.length > 250) {
    swal.fire({
      title: "Error!",
      text: "Tên mẫu biểu không được vượt quá 250 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  let formData = new FormData();

  for (var i = 0; i < filesList.value.length; i++) {
    let file = filesList.value[i];
    formData.append("image", file);
  }
  formData.append("hrm_declare_review_form", JSON.stringify(review_form.value));
  formData.append(
    "list_eval_criterias",
    JSON.stringify(listEvalCriterias.value)
  );
  formData.append("list_eval_childs", JSON.stringify(listEvalChilds.value));
  formData.append("hrm_files", JSON.stringify(listFilesS.value));

  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (!isSaveTem.value) {
    axios
      .post(
        baseURL + "/api/hrm_declare_review_form/add_hrm_declare_review_form",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm mẫu biểuthành công!");

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
        baseURL + "/api/hrm_declare_review_form/update_hrm_declare_review_form",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa mẫu biểuthành công!");

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
const isView = ref(false);
//Sửa bản ghi
const editTem = (dataTem, header, view) => {
  submitted.value = false;
  isView.value = view;
  headerDialog.value = header;

  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_declare_review_form_get",
            par: [{ par: "review_form_id", va: dataTem.review_form_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (isFirst.value) isFirst.value = false;

      // datalists.value = data;
      review_form.value = data[0][0];
      listEvalCriterias.value = data[1];
      listEvalChilds.value = data[2];
      listFilesS.value = data[3];
      checkShow.value = true;
      options.value.loading = false;

      isSaveTem.value = true;
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
          .delete(
            baseURL +
              "/api/hrm_declare_review_form/delete_hrm_declare_review_form",
            {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: Tem != null ? [Tem.review_form_id] : 1,
            }
          )
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá mẫu biểuthành công!");
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

const changeViewDeclare = (value) => {
  review_form.value = value;
  loadDataDetails(true);
};
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
const listProfilesSave=ref([]);
const loadDataDetails = (rf) => {
  if (rf) {
    axios
      .post(
        baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "hrm_review_form_users_list",
              par: [
                { par: "review_form_id", va: review_form.value.review_form_id },
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
        data.forEach((element, i) => {
          element.STT = options.value.PageNo * options.value.PageSize + i + 1;
        });
         
        listProfiles.value = data;
        options.value.totalRecordsP = data1[0].totalRecordsP;
        listProfilesSave.value = data2;
        options.value.loadingP = false;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");

        options.value.loading = false;
      });
  }
};

const showHidePanel = (type) => {
  if (type == 1) {
    if (checkShow.value == true) {
      checkShow.value = false;
    } else {
      checkShow.value = true;
    }
  }
};
const addRow_Item = (item) => {
  checkShow.value = true;
  if (
    listEvalCriterias.value.find((x) => x.roman_order == item.roman_order) !=
    null
  ) {
    let obj = {
      is_order:
        listEvalChilds.value.filter((x) => x.roman_order == item.roman_order)
          .length + 1,
      eval_criteria_child_name: null,
      complete_results: null,
      complete_time: null,
      weight: null,
      parent_id: null,
      status: true,
      roman_order: item.roman_order,
    };
    listEvalChilds.value.push(obj);
  }
};

function numberToRoman(num) {
  var roman = "";
  var romanNumeralMap = {
    M: 1000,
    CM: 900,
    D: 500,
    CD: 400,
    C: 100,
    XC: 90,
    L: 50,
    XL: 40,
    X: 10,
    IX: 9,
    V: 5,
    IV: 4,
    I: 1,
  };
  for (var key in romanNumeralMap) {
    while (num >= romanNumeralMap[key]) {
      roman += key;
      num -= romanNumeralMap[key];
    }
  }
  return roman;
}
const onAddEvalCriterias = () => {
  checkShow.value = true;

  let obj = {
    roman_order: numberToRoman(listEvalCriterias.value.length + 1),
    is_order: listEvalCriterias.value.length + 1,
    percen: null,
    eval_criteria_name: null,
    des: null,
    is_plan: true,
    is_results: true,
    status: true,
    type: 1,
  };
  listEvalCriterias.value.push(obj);
};
const delRow_Item = (item, type) => {
  if (type == 1) {
    if (listEvalChilds.value.find((x) => x == item) != null) {
      listEvalChilds.value = listEvalChilds.value.filter((x) => x != item);
    }
  }
};
const listFilesS = ref([]);

const filesList = ref([]);
let fileSize = [];
const onUploadFile = (event) => {
  fileSize = [];
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
            filesList.value.push(fi);
            fileSize.push(fi.size);
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
const deleteFileH = (value) => {
  listFilesS.value = listFilesS.value.filter((x) => x.file_id != value.file_id);
};
const onMinusItem = (item) => {
  listEvalCriterias.value = listEvalCriterias.value.filter(
    (x) => x.roman_order != item.roman_order
  );
};

// CHọn nhân sự

const displayDialogUser = ref(false);

const selectedUser = ref();

const showTreeUser = () => {
  checkMultile.value = false;
  selectedUser.value = listProfilesSave.value;
  displayDialogUser.value = true;
};

const closeDialogUser = () => {
  displayDialogUser.value = false;
};

const checkMultile = ref(false);
const listProfiles = ref([]);
const choiceUser = () => {

  listProfiles.value = [];
  if (checkMultile.value == false) listProfiles.value = selectedUser.value;
  let formData = new FormData();
 

  formData.append("report", JSON.stringify(review_form.value));
  formData.append("hrm_review_form_users", JSON.stringify(listProfiles.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
 
    axios
      .post(baseURL + "/api/hrm_review_form_users/add_hrm_review_form_users", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm thông tin chiến dịch thành công!");

  
        } else {
          swal.fire({
            title: "Error!",
            text: response.data.ms,
            icon: "error",
            confirmButtonText: "OK",
          });
        }
        displayDialogUser.value = false;
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

const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  profile_user_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  } 
});


//Phân trang dữ liệu
const onPageP = (event) => {
  if (event.rows != options.value.PageSizeP) {
    options.value.PageSizeP = event.rows;
  }
  if (event.page == 0) {
    //Trang đầu
    options.value.id = null;
    options.value.IsNext = true;
  } else if (event.page > options.value.PageNoP + 1) {
    //Trang cuối
    options.value.id = -1;
    options.value.IsNext = false;
  } else if (event.page > options.value.PageNoP) {
    //Trang sau

    options.value.id = listProfiles.value[listProfiles.value.length - 1].profile_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNoP) {
    //Trang trước
    options.value.id = listProfiles.value[0].profile_id;
    options.value.IsNext = false;
  }
  options.value.PageNoP = event.page;
  loadDataSQL();
};

//Sort
const onSortP = (event) => {
  options.value.PageNoP = 0;

  if (event.sortField == null) {
    loadDataSQL();
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

const onFilterP = (event) => {
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
  options.value.PageNoP = 0;
  options.value.id = null;
 
  loadDataSQL();
};

const checkFilter = ref(false);
const filterSQL = ref([]);
 
const checkLoadCount = ref(true);
const loadDataSQL = () => {
  datalists.value = [];
  filterSQL.value.push({
    filterconstraints: [ { value: review_form.value.review_form_id, matchMode: "equals" }],
      filteroperator: "and",
      key: "review_form_id",
  });
  
  let data = {
    id: "profile_id",
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
    .post(baseURL + "/api/HRM_SQL/Filter_hrm_review_form_users", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
     
        listProfiles.value = data;
      } else {
        listProfiles.value = [];
      }
 
      options.value.loading = false;
      //Show Count nếu có
      if (dt.length >= 1 && checkLoadCount.value == true) {
    
        options.value.totalRecordsP = dt[1][0].totalRecords;
        
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

onMounted(() => {
  loadData(true);
  return {
    datalists,
    options,
    onPage,
    loadData,

    openBasic,
    closeDialog,
    basedomainURL,
    saveData,
    isFirst,
    searchStamp,
  };
});
</script>
<template>
  <div>
    <div class="p-3">
      <h2 class="module-title m-0">
        <i class="pi pi-file-o text-lg"></i> Danh sách mẫu biểu đánh giá
      </h2>
    </div>
    <div>
      <Splitter class="h-full w-full pb-0 pr-0">
        <SplitterPanel :size="35" class=" ">
          <div>
            <div>
              <Toolbar>
                <template #start>
                  <span class="p-input-icon-left">
                    <i class="pi pi-search" />
                    <InputText
                      v-model="options.search"
                      @keyup="searchStamp"
                      type="text"
                      spellcheck="false"
                      placeholder="Tìm kiếm biểu mẫu"
                    />
                  </span>
                </template>
                <template #end>
                  <Button
                    v-tooltip.top="'Thêm mới biểu mẫu'"
                    @click="openBasic('Thêm mới')"
                    label="Thêm mới"
                    icon="pi pi-plus"
                    class="mr-2"
                  />
                </template>
              </Toolbar>
            </div>

            <div style="border-top: 2px solid #dee2e6">
              <div class="w-full d-lang-table mx-2">
                <DataView
                  :value="datalists"
                  :loading="options.loading"
                  :paginator="true"
                  currentPageReportTemplate=""
                  :rows="options.PageSize"
                  :totalRecords="options.totalRecords"
                  :rowHover="true"
                  :showGridlines="true"
                  :pageLinks="5"
                  class="w-full h-full ptable p-datatable-sm flex flex-column"
                  :lazy="true"
                  layout="list"
                  responsiveLayout="scroll"
                  :scrollable="true"
                >
                  <template #list="slotProps">
                    <div
                      class="grid h-full w-full formgrid"
                      :class="
                        review_form.review_form_id ==
                        slotProps.data.review_form_id
                          ? 'bg-d-selected'
                          : ''
                      "
                      @click="changeViewDeclare(slotProps.data)"
                    >
                      <div class="col-12 mb-2 p-2 flex align-items-center">
                        <div class="px-2 col-9">
                          <div class="font-bold text-lg">
                            {{ slotProps.data.review_form_name }}
                          </div>

                          <div class="text-sm">
                            Người lập: {{ slotProps.data.full_name }} | Ngày lập
                            {{
                              moment(
                                new Date(slotProps.data.created_date)
                              ).format("DD/MM/YYYY")
                            }}
                          </div>
                        </div>

                        <div class="pr-2 col-3 flex">
                          <Toolbar class="w-full p-0 m-0 custoolbar">
                            <template #end>
                              <Button
                                @click="
                                  editTem(slotProps.data, 'Xem mẫu biểu', true)
                                "
                                class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                                type="button"
                                icon="pi pi-eye"
                                v-tooltip.top="'Xem'"
                              ></Button>

                              <div
                                v-if="
                                  store.state.user.is_super == true ||
                                  store.state.user.user_id ==
                                    slotProps.data.created_by ||
                                  (store.state.user.role_id == 'admin' &&
                                    store.state.user.organization_id ==
                                      slotProps.data.organization_id)
                                "
                              >
                                <Button
                                  @click="
                                    editTem(
                                      slotProps.data,
                                      'Sửa mẫu biểu',
                                      false
                                    )
                                  "
                                  class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                                  type="button"
                                  icon="pi pi-pencil"
                                  v-tooltip.top="'Sửa'"
                                ></Button>
                                <Button
                                  @click="delTem(slotProps.data)"
                                  class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                                  type="button"
                                  icon="pi pi-trash"
                                  v-tooltip.top="'Xóa'"
                                ></Button>
                              </div>
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
        <SplitterPanel :size="65">
          <div>
            <Toolbar>
              <template #start>
                <span class="p-input-icon-left">
                  <i class="pi pi-search" />
                  <InputText
                    v-model="options.search"
                    @keyup="searchStamp"
                    type="text"
                    spellcheck="false"
                    placeholder="Tìm kiếm nhân sự"
                  />
                </span>
              </template>
              <template #end>
                <Button
                  v-tooltip.top="'Thêm nhân sự'"
                  @click="showTreeUser()"
                  label="Thêm nhân sự"
                  icon="pi pi-plus"
                  class="mr-2"
                />
              </template>
            </Toolbar>
          </div>
          <div class="d-lang-table-d" v-if="listProfiles.length>0">
 
            <DataTable
              @page="onPageP($event)"
              @sort="onSortP($event)"
              @filter="onFilterP($event)"
              v-model:filters="filters"
              selectionMode="single"
              filterDisplay="menu"
              filterMode="lenient"
              :scrollable="true"
              scrollHeight="flex"
              :showGridlines="true"
              columnResizeMode="fit"
              :lazy="true"
              :loading="options.loading"
              :totalRecords="options.totalRecordsP"
              :reorderableColumns="true"
              :value="listProfiles"
              removableSort
              v-model:rows="options.PageSizeP"
              paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
              :rowsPerPageOptions="[20, 30, 50, 100, 200]"
              :paginator="true"
              dataKey="profile_id"
              responsiveLayout="scroll"
              :row-hover="true"
            >
              <Column
                field="STT"
                header="STT"
                class="align-items-center justify-content-center text-center overflow-hidden"
                headerStyle="text-align:center;max-width:55px;height:50px"
                bodyStyle="text-align:center;max-width:55px"
              ></Column>
              <Column
                field="candidate_code"
                header="Ảnh"
                headerStyle="text-align:center;max-width:70px;height:50px"
                bodyStyle="text-align:center;max-width:70px"
                class="align-items-center justify-content-center text-center overflow-hidden"
              >
                <template #body="slotProps">
                  <div>
                    <Avatar
                      style="color: #fff"
                      v-bind:label="
                        slotProps.data.avatar
                          ? ''
                          : slotProps.data.profile_user_name.substring(
                              slotProps.data.profile_user_name.lastIndexOf(
                                ' '
                              ) + 1,
                              slotProps.data.profile_user_name.lastIndexOf(
                                ' '
                              ) + 2
                            )
                      "
                      :image="basedomainURL + slotProps.data.avatar"
                      class="w-3rem"
                      size="large"
                      :style="
                        slotProps.data.avatar
                          ? 'background-color: #2196f3'
                          : 'background:' +
                            bgColor[slotProps.data.profile_user_name.length % 7]
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
                headerStyle=" height:50px"
                bodyStyle="text-align:left"
                class="overflow-hidden"
              >
              </Column>

              <Column
                field="position_name"
                header="Chức vụ"
                headerStyle="text-align:center;max-width:200px;height:50px"
                bodyStyle="text-align:center;max-width:200px"
                class="align-items-center justify-content-center text-center overflow-hidden"
              >
              </Column>

              <Column
                field="department_name"
                header="Phòng ban"
                headerStyle="text-align:center;max-width:200px;height:50px"
                bodyStyle="text-align:center;max-width:200px"
                class="align-items-center justify-content-center text-center overflow-hidden"
              >
              </Column>

              <Column
                header=""
                headerStyle="text-align:center;max-width:70px"
                bodyStyle="text-align:center;max-width:70px"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <Button
                    icon="pi pi-trash"
                    class="p-button-rounded p-button-danger p-button-outlined ml-2"
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
        </SplitterPanel>
      </Splitter>
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
          <div class="field col-12 md:col-12">
            <label class="w-10rem text-left p-0"
              >Tên mẫu biểu <span class="redsao">(*)</span></label
            >
            <InputText
              v-model="review_form.review_form_name"
              spellcheck="false"
              style="width: calc(100% - 10rem)"
              class="p36 px-2"
              :class="{
                'p-invalid': v$.review_form_name.$invalid && submitted,
              }"
              :disabled="isView"
              :style="isView ? 'opacity:1' : ''"
            />
          </div>
          <div
            class="field col-12 md:col-12 flex"
            v-if="
              (v$.review_form_name.$invalid && submitted) ||
              v$.review_form_name.$pending.$response
            "
          >
            <div class="w-10rem text-left"></div>
            <small style="width: calc(100% - 10rem)" class="p-error">
              <span class="col-12 p-0">{{
                v$.review_form_name.required.$message
                  .replace("Value", "Tên mẫu biểu")
                  .replace("is required", "không được để trống")
              }}</span>
            </small>
          </div>
          <!-- <div class="col-12 field md:col-12 flex">
            <div class="field col-6 md:col-6 p-0 align-items-center flex">
              <div class="w-10rem text-left p-0">STT</div>
              <InputNumber
                v-model="review_form.is_order"
                style="width: calc(100% - 10rem)"
                class="ip36 p-0"
              />
            </div>
            <div class="field col-6 md:col-6 p-0 align-items-center flex">
              <div class="col-6 text-center p-0">Trạng thái</div>
              <InputSwitch
                v-model="review_form.status"
                class="w-4rem lck-checked"
              />
            </div>
          </div> -->
          <div class="col-12 field md:col-12 p-0 pr-2 flex">
            <Toolbar class="custoolbar w-full">
              <template #start>
                <div class="font-bold text-lg">Danh sách chỉ tiêu</div>
              </template>
            </Toolbar>
          </div>
          <div
            class="col-12 field md:col-12 p-0 flex"
            v-for="(item, index) in listEvalCriterias"
            :key="index"
          >
            <Card class="w-full">
              <template #title>
                <Toolbar class="custoolbar p-0">
                  <template #start>
                    <div class="font-bold text-3xl">
                      Phần
                      <span
                        style="font-family: 'Times New Roman', Times, serif"
                        >{{ item.roman_order }}</span
                      >
                    </div>
                  </template>
                  <template #end>
                    <Button
                      v-if="!isView"
                      icon="pi pi-minus"
                      @click="onMinusItem(item)"
                      class="p-button-outlined p-button-danger"
                    >
                    </Button>
                  </template>
                </Toolbar>
              </template>
              <template #content>
                <div class="col-12 md:col-12 p-0 flex field">
                  <div class="col-12 p-0 flex align-items-center">
                    <div class="w-10rem p-0">Tên chỉ tiêu</div>
                    <div class="p-0" style="width: calc(100% - 10rem)">
                      <InputText
                        v-model="item.eval_criteria_name"
                        spellcheck="false"
                        class="px-2 w-full"
                        :disabled="isView"
                        :style="isView ? 'opacity:1' : ''"
                      />
                    </div>
                  </div>
                </div>
                <div class="col-12 md:col-12 p-0 flex field">
                  <div class="col-3 p-0 flex align-items-center">
                    <div class="w-10rem p-0 flex align-items-center">
                      Phần trăm đánh giá
                    </div>
                    <div
                      style="width: calc(100% - 10rem)"
                      class="p-0 flex align-items-center"
                    >
                      <InputNumber
                        class="w-full"
                        v-model="item.percen"
                        inputId="percent"
                        :max="100"
                        suffix=" %"
                        :disabled="isView"
                        :style="isView ? 'opacity:1' : ''"
                      />
                    </div>
                  </div>
                  <div class="col-4 p-0 flex align-items-center">
                    <div class="w-10rem p-0 pl-3 flex align-items-center">
                      Loại chỉ tiêu
                    </div>
                    <div
                      style="width: calc(100% - 10rem)"
                      class="p-0 flex align-items-center"
                    >
                      <Dropdown
                        :disabled="isView"
                        :style="isView ? 'opacity:1' : ''"
                        class="w-full"
                        v-model="item.type"
                        :options="listTypeEvals"
                        optionLabel="name"
                        optionValue="code"
                      />
                    </div>
                  </div>
                  <div class="col-5 p-0 flex align-items-center">
                    <div class="col-6 p-0 flex align-items-center">
                      <div class="col-6 p-0 pl-3 flex align-items-center">
                        Kế hoạch
                      </div>
                      <div class="col-6 p-0 flex align-items-center">
                        <InputSwitch
                          :disabled="isView"
                          :style="isView ? 'opacity:1' : ''"
                          v-model="item.is_plan"
                          class="w-4rem lck-checked"
                        />
                      </div>
                    </div>
                    <div class="col-6 p-0 flex align-items-center">
                      <div class="col-6 p-0 pl-3 flex align-items-center">
                        Kết quả
                      </div>
                      <div class="col-6 p-0 flex align-items-center">
                        <InputSwitch
                          :disabled="isView"
                          :style="isView ? 'opacity:1' : ''"
                          v-model="item.is_results"
                          class="w-4rem lck-checked"
                        />
                      </div>
                    </div>
                  </div>
                </div>
                <div
                  class="col-12 p-0 border-1 border-300 border-solid"
                  v-if="item.type == 1"
                >
                  <div
                    class="w-full surface-100 flex border-bottom-1 border-200"
                  >
                    <div
                      class="font-bold flex align-items-center w-full cursor-pointer p-3"
                      @click="showHidePanel(1)"
                    >
                      <i
                        class="pi pi-angle-right"
                        v-if="checkShow == false"
                        style="font-size: 1.25rem"
                      ></i>
                      <i
                        class="pi pi-angle-down"
                        v-if="checkShow == true"
                        style="font-size: 1.25rem"
                      ></i>
                      <div class="pl-2">
                        Danh sách tiêu chí đánh giá
                        <span
                          v-if="
                            listEvalChilds.filter(
                              (x) => x.roman_order == item.roman_order
                            ).length > 0
                          "
                        >
                          (
                          {{
                            listEvalChilds.filter(
                              (x) => x.roman_order == item.roman_order
                            ).length
                          }}
                          )</span
                        >
                      </div>
                    </div>
                    <div
                      class="w-1 text-right p-3 hover"
                      v-if="!isView"
                      @click="addRow_Item(item)"
                    >
                      <a class="hover" v-tooltip.top="'Thêm chỉ tiêu'">
                        <i
                          class="pi pi-plus-circle"
                          style="font-size: 18px"
                        ></i>
                      </a>
                    </div>
                  </div>

                  <div class="w-full p-0" v-if="checkShow == true">
                    <div>
                      <DataTable
                        :value="
                          listEvalChilds.filter(
                            (x) => x.roman_order == item.roman_order
                          )
                        "
                        :scrollable="true"
                        :lazy="true"
                        :rowHover="true"
                        :showGridlines="true"
                      >
                        <Column
                          v-if="!isView"
                          header=""
                          headerStyle="text-align:center;max-width:50px"
                          bodyStyle="text-align:center;max-width:50px"
                          class="align-items-center justify-content-center text-center"
                        >
                          <template #body="slotProps">
                            <a
                              @click="delRow_Item(slotProps.data, 1)"
                              class="hover cursor-pointer"
                              v-tooltip.top="'Xóa chỉ tiêu'"
                            >
                              <i
                                class="pi pi-times-circle"
                                style="font-size: 18px"
                              ></i>
                            </a>
                          </template>
                        </Column>
                        <Column
                          field="card_number"
                          header="STT"
                          headerStyle="text-align:center;max-width:70px;height:50px"
                          bodyStyle="text-align:center;max-width:70px;"
                          class="align-items-center justify-content-center text-center"
                        >
                          <template #body="slotProps">
                            {{ slotProps.data.is_order }}
                          </template>
                        </Column>
                        <Column
                          field="form"
                          header="Tiêu chí"
                          headerStyle="text-align:center; height:50px"
                          bodyStyle="text-align:center; "
                          class="align-items-center justify-content-center text-center"
                        >
                          <template #body="slotProps">
                            <Textarea
                              :autoResize="true"
                              rows="1"
                              cols="30"
                              v-model="slotProps.data.eval_criteria_child_name"
                              class="w-full"
                              spellcheck="false"
                              :disabled="isView"
                              :style="isView ? 'opacity:1' : ''"
                            />
                          </template>
                        </Column>
                        <Column
                          field="form"
                          header="Kết quả cần đạt"
                          headerStyle="text-align:center;max-width:250px;height:50px"
                          bodyStyle="text-align:center;max-width:250px;"
                          class="align-items-center justify-content-center text-center"
                        >
                          <template #body="slotProps">
                            <Textarea
                              :autoResize="true"
                              rows="1"
                              cols="30"
                              v-model="slotProps.data.complete_results"
                              class="w-full"
                              spellcheck="false"
                              :disabled="isView"
                              :style="isView ? 'opacity:1' : ''"
                            />
                          </template>
                        </Column>
                        <Column
                          field="weight"
                          header="Trọng số"
                          headerStyle="text-align:center;max-width:120px;height:50px"
                          bodyStyle="text-align:center;max-width:120px;"
                          class="align-items-center justify-content-center text-center"
                        >
                          <template #body="slotProps">
                            <InputNumber
                              spellcheck="false"
                              class="w-full d-design-it duy-inpput"
                              v-model="slotProps.data.weight"
                              :disabled="isView"
                              :style="isView ? 'opacity:1' : ''"
                            />
                          </template>
                        </Column>
                      </DataTable>
                    </div>
                  </div>
                </div>
              </template>
            </Card>
          </div>
          <div class="col-12 field md:col-12 p-0 pr-2 flex">
            <Toolbar class="custoolbar w-full">
              <template #end>
                <div v-if="!isView">
                  <Button
                    icon="pi pi-plus"
                    label="Thêm mới"
                    class="p-button-outlined"
                    @click="onAddEvalCriterias"
                  >
                  </Button>
                </div>
              </template>
            </Toolbar>
          </div>
          <div class="col-12 field text-lg font-bold" v-if="!isView">
            File đính kèm
          </div>
          <div class="w-full col-12 field p-0" v-if="!isView">
            <FileUpload
              chooseLabel="Chọn File"
              :showUploadButton="false"
              :showCancelButton="false"
              :multiple="false"
              :maxFileSize="524288000"
              @select="onUploadFile"
              @remove="removeFile"
              :invalidFileSizeMessage="'{0}: Dung lượng File không được lớn hơn {1}'"
            >
              <template #empty>
                <p class="p-0 m-0 text-500">Kéo thả hoặc chọn File.</p>
              </template>
            </FileUpload>

            <div class="col-12 p-0" v-if="listFilesS.length > 0">
              <DataTable
                :value="listFilesS"
                filterDisplay="menu"
                filterMode="lenient"
                scrollHeight="flex"
                :showGridlines="true"
                :paginator="false"
                :row-hover="true"
                columnResizeMode="fit"
              >
                <Column field="code" header="  File đính kèm">
                  <template #body="item">
                    <div
                      class="p-0 d-style-hover"
                      style="width: 100%; border-radius: 10px"
                    >
                      <div class="w-full flex align-items-center">
                        <div class="flex w-full text-900">
                          <div
                            v-if="item.data.is_image"
                            class="align-items-center flex"
                          >
                            <Image
                              :src="basedomainURL + item.data.file_path"
                              alt=""
                              width="70"
                              height="50"
                              style="
                                object-fit: contain;
                                border: 1px solid #ccc;
                                width: 70px;
                                height: 50px;
                              "
                              preview
                              class="pr-2"
                            />
                            <div class="ml-2" style="word-break: break-all">
                              {{ item.data.file_name }}
                            </div>
                          </div>
                          <div v-else>
                            <a
                              :href="basedomainURL + item.data.file_path"
                              download
                              class="w-full no-underline cursor-pointer text-900"
                            >
                              <div class="align-items-center flex">
                                <div>
                                  <img
                                    :src="
                                      basedomainURL +
                                      '/Portals/Image/file/' +
                                      item.data.file_path.substring(
                                        item.data.file_path.lastIndexOf('.') + 1
                                      ) +
                                      '.png'
                                    "
                                    style="
                                      width: 70px;
                                      height: 50px;
                                      object-fit: contain;
                                    "
                                    alt=""
                                  />
                                </div>
                                <div class="ml-2" style="word-break: break-all">
                                  <div
                                    class="ml-2"
                                    style="word-break: break-all"
                                  >
                                    <div style="word-break: break-all">
                                      {{ item.data.file_name }}
                                    </div>
                                    <div
                                      v-if="store.getters.user.is_super"
                                      style="
                                        word-break: break-all;
                                        font-size: 11px;
                                        font-style: italic;
                                      "
                                    >
                                      {{ item.data.organization_name }}
                                    </div>
                                  </div>
                                </div>
                              </div>
                            </a>
                          </div>
                        </div>
                        <div
                          class="w-3rem align-items-center d-style-hover-1"
                          v-if="
                            store.getters.user.organization_id ==
                            item.data.organization_id
                          "
                        >
                          <Button
                            icon="pi pi-times"
                            class="p-button-rounded bg-red-300 border-none"
                            @click="deleteFileH(item.data)"
                          />
                        </div>
                      </div>
                    </div>
                  </template>
                </Column>
              </DataTable>
            </div>
          </div>
        </div>
      </form>
      <template #footer>
        <div class="pt-3">
          <Button
            label="Hủy"
            icon="pi pi-times"
            @click="closeDialog"
            class="p-button-outlined"
          />

          <Button
            v-if="!isView"
            label="Lưu"
            icon="pi pi-check"
            @click="saveData(!v$.$invalid)"
            autofocus
          />
        </div>
      </template>
    </Dialog>
  </div>

  <tree_users_hrm
    v-if="displayDialogUser === true"
    :headerDialog="'Chọn nhân sự'"
    :displayDialog="displayDialogUser"
    :closeDialog="closeDialogUser"
    :one="checkMultile"
    :selected="selectedUser"
    :choiceUser="choiceUser"
  />
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
.d-lang-table-d {
  margin: 0px;
  height: calc(100vh - 160px);
}
.d-lang-table {
  margin: 0px;
  height: calc(100vh - 160px);
}
.d-lang-table-r {
  margin: 0px;
  height: calc(100vh - 160px);
}

.bg-d-selected {
  background-color: #e3f2fd !important;
}
.hover:hover {
  cursor: pointer;
  color: #2196f3 !important;
}
</style>
    